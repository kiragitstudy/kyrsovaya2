using ArtGalleryDLC.Models;
using ArtGalleryDLC.Repository;

namespace ArtGalleryDLC.Services
{
    public class ArtGalleryService
    {
        private readonly IRepository<Artwork> _artworkRepo;
        public readonly IRepository<Artist> _artistRepo;
        public readonly IRepository<Exhibition> _exhibitionRepo;
        public readonly IRepository<Visitor> _visitorRepo;
        public readonly IRepository<Ticket> _ticketRepo;
        public readonly IRepository<Sale> _saleRepo;
        private readonly IRepository<Rental> _rentalRepo;

        public ArtGalleryService()
        {
            _artworkRepo = new JsonRepository<Artwork>("artworks.json");
            _artistRepo = new JsonRepository<Artist>("artists.json");
            _exhibitionRepo = new JsonRepository<Exhibition>("exhibitions.json");
            _visitorRepo = new JsonRepository<Visitor>("visitors.json");
            _ticketRepo = new JsonRepository<Ticket>("tickets.json");
            _saleRepo = new JsonRepository<Sale>("sales.json");
            _rentalRepo = new JsonRepository<Rental>("rentals.json");
        }

        // Загрузка и установление связей между сущностями
        public void LoadReferences()
        {
            var artworks = _artworkRepo.GetAll();
            var artists = _artistRepo.GetAll();
            var exhibitions = _exhibitionRepo.GetAll();
            var visitors = _visitorRepo.GetAll();
            var tickets = _ticketRepo.GetAll();
            var sales = _saleRepo.GetAll();
            var rentals = _rentalRepo.GetAll();

            // Связываем художников и произведения искусства
            foreach (var artwork in artworks)
            {
                artwork.Artist = artists.FirstOrDefault(a => a.Id == artwork.ArtistId);
            }

            foreach (var artist in artists)
            {
                artist.Artworks = artworks.Where(a => a.ArtistId == artist.Id).ToList();
            }

            // Связываем выставки и произведения искусства
            foreach (var exhibition in exhibitions)
            {
                exhibition.Artworks = artworks
                    .Where(a => exhibition.ArtworkIds.Contains(a.Id))
                    .ToList();
            }

            // Связываем посетителей и покупки
            foreach (var visitor in visitors)
            {
                visitor.Purchases = sales
                    .Where(s => s.BuyerId == visitor.Id)
                    .ToList();
            }

            // Связываем билеты с выставками и посетителями
            foreach (var ticket in tickets)
            {
                ticket.Exhibition = exhibitions.FirstOrDefault(e => e.Id == ticket.ExhibitionId);
                ticket.Visitor = visitors.FirstOrDefault(v => v.Id == ticket.VisitorId);
            }

            // Связываем продажи с произведениями и покупателями
            foreach (var sale in sales)
            {
                sale.Artwork = artworks.FirstOrDefault(a => a.Id == sale.ArtworkId);
                sale.Buyer = visitors.FirstOrDefault(v => v.Id == sale.BuyerId);
            }

            // Связываем аренды с произведениями и арендаторами
            foreach (var rental in rentals)
            {
                rental.Artwork = artworks.FirstOrDefault(a => a.Id == rental.ArtworkId);
                rental.Renter = visitors.FirstOrDefault(v => v.Id == rental.RenterId);
            }
        }

        // === ЗАПРОСЫ ===

        // Список произведений искусства в наличии
        public List<Artwork> GetAvailableArtworks()
        {
            LoadReferences();
            return _artworkRepo.GetAll()
                .Where(a => a.Status == ArtworkStatus.InGallery)
                .ToList();
        }

        // Выставки, запланированные на ближайший месяц
        public List<Exhibition> GetUpcomingExhibitions()
        {
            LoadReferences();
            DateTime today = DateTime.Today;
            DateTime oneMonthAhead = today.AddMonths(1);
            
            return _exhibitionRepo.GetAll()
                .Where(e => e.StartDate <= oneMonthAhead && e.EndDate >= today)
                .ToList();
        }

        // Самые популярные художники по количеству проданных работ
        public List<(Artist Artist, int SoldCount)> GetPopularArtistsBySales()
        {
            LoadReferences();
            var artworks = _artworkRepo.GetAll();
            var sales = _saleRepo.GetAll();
            var artists = _artistRepo.GetAll();

            var soldArtworks = sales.Select(s => s.ArtworkId).ToList();
            
            return artists
                .Select(artist => (
                    Artist: artist,
                    SoldCount: soldArtworks.Count(id => 
                        artworks.FirstOrDefault(a => a.Id == id)?.ArtistId == artist.Id)
                ))
                .OrderByDescending(item => item.SoldCount)
                .ToList();
        }

        /// <summary>
        /// Общая выручка от билетов и продаж картин
        /// </summary>
        /// <returns>Выручка с билетов, Выручка с картин, Общая выручка</returns>
        public (decimal TicketRevenue, decimal SalesRevenue, decimal TotalRevenue) GetTotalRevenue()
        {
            LoadReferences();
            
            decimal ticketRevenue = _ticketRepo.GetAll()
                .Where(t => t.Status != TicketStatus.Cancelled)
                .Sum(t => t.Price);
                
            decimal salesRevenue = _saleRepo.GetAll()
                .Sum(s => s.Amount);
                
            return (ticketRevenue, salesRevenue, ticketRevenue + salesRevenue);
        }

        // Произведения, находящиеся в аренде
        public List<(Artwork Artwork, Rental Rental)> GetRentedArtworks()
        {
            LoadReferences();
            
            var rentals = _rentalRepo.GetAll();
            var artworks = _artworkRepo.GetAll();
            
            var currentRentals = rentals
                .Where(r => r.EndDate >= DateTime.Today)
                .ToList();
            
            return currentRentals
                .Select(rental => (
                    Artwork: artworks.FirstOrDefault(a => a.Id == rental.ArtworkId),
                    Rental: rental
                ))
                .Where(tuple => tuple.Artwork != null)
                .ToList()!;
        }

        // === ОПЕРАЦИИ ===

        // Добавление нового произведения искусства
        public Artwork AddArtwork(string title, string artistId, int year, string genre, 
            string description, decimal estimatedValue)
        {
            var artist = _artistRepo.GetById(artistId);
            if (artist == null)
            {
                throw new Exception("Художник не найден");
            }

            var artwork = new Artwork
            {
                Title = title,
                ArtistId = artistId,
                Year = year,
                Genre = genre,
                Description = description,
                EstimatedValue = estimatedValue,
                Status = ArtworkStatus.InGallery
            };

            _artworkRepo.Add(artwork);
            _artworkRepo.SaveChanges();

            // Обновляем список работ художника
            artist.ArtworkIds.Add(artwork.Id);
            _artistRepo.Update(artist);
            _artistRepo.SaveChanges();

            return artwork;
        }

        // Добавление нового художника
        public Artist AddArtist(string fullName, string country, string lifeYears, string style)
        {
            var artist = new Artist
            {
                FullName = fullName,
                Country = country,
                LifeYears = lifeYears,
                Style = style
            };
        
            _artistRepo.Add(artist);
            _artistRepo.SaveChanges();
        
            return artist;
        }

        // Добавление новой выставки
        public Exhibition AddExhibition(string title, DateTime startDate, DateTime endDate, 
            string location, List<string> artworkIds, decimal ticketPrice)
        {
            // Проверяем существование произведений
            foreach (var artworkId in artworkIds)
            {
                if (_artworkRepo.GetById(artworkId) == null)
                {
                    throw new Exception($"Произведение с ID {artworkId} не найдено");
                }
            }

            var exhibition = new Exhibition
            {
                Title = title,
                StartDate = startDate,
                EndDate = endDate,
                Location = location,
                ArtworkIds = artworkIds,
                TicketPrice = ticketPrice
            };

            _exhibitionRepo.Add(exhibition);
            _exhibitionRepo.SaveChanges();

            return exhibition;
        }

        // Регистрация нового посетителя
        public Visitor AddVisitor(string fullName, string contactInfo)
        {
            var visitor = new Visitor
            {
                FullName = fullName,
                ContactInfo = contactInfo
            };

            _visitorRepo.Add(visitor);
            _visitorRepo.SaveChanges();

            return visitor;
        }

        // Продажа произведения искусства
        public Sale SellArtwork(string artworkId, string buyerId, decimal amount)
        {
            var artwork = _artworkRepo.GetById(artworkId);
            if (artwork == null)
            {
                throw new Exception("Произведение не найдено");
            }

            if (artwork.Status != ArtworkStatus.InGallery)
            {
                throw new Exception("Это произведение недоступно для продажи");
            }

            var buyer = _visitorRepo.GetById(buyerId);
            if (buyer == null)
            {
                throw new Exception("Покупатель не найден");
            }

            // Создаем запись о продаже
            var sale = new Sale
            {
                ArtworkId = artworkId,
                BuyerId = buyerId,
                Date = DateTime.Now,
                Amount = amount
            };

            // Обновляем статус произведения
            artwork.Status = ArtworkStatus.Sold;
            _artworkRepo.Update(artwork);
            _artworkRepo.SaveChanges();

            // Добавляем продажу в историю покупок посетителя
            buyer.PurchaseIds.Add(sale.Id);
            _visitorRepo.Update(buyer);
            _visitorRepo.SaveChanges();

            // Сохраняем информацию о продаже
            _saleRepo.Add(sale);
            _saleRepo.SaveChanges();

            return sale;
        }

        // Передача произведения в аренду
        public Rental RentArtwork(string artworkId, string renterId, DateTime startDate, 
            DateTime endDate, decimal cost)
        {
            var artwork = _artworkRepo.GetById(artworkId);
            if (artwork == null)
            {
                throw new Exception("Произведение не найдено");
            }

            if (artwork.Status != ArtworkStatus.InGallery)
            {
                throw new Exception("Это произведение недоступно для аренды");
            }

            var renter = _visitorRepo.GetById(renterId);
            if (renter == null)
            {
                throw new Exception("Арендатор не найден");
            }

            // Создаем запись об аренде
            var rental = new Rental
            {
                ArtworkId = artworkId,
                RenterId = renterId,
                StartDate = startDate,
                EndDate = endDate,
                Cost = cost
            };

            // Обновляем статус произведения
            artwork.Status = ArtworkStatus.Rented;
            _artworkRepo.Update(artwork);
            _artworkRepo.SaveChanges();

            // Сохраняем информацию об аренде
            _rentalRepo.Add(rental);
            _rentalRepo.SaveChanges();

            return rental;
        }

        // Возврат произведения из аренды
        public void ReturnRentedArtwork(string rentalId)
        {
            var rental = _rentalRepo.GetById(rentalId);
            if (rental == null)
            {
                throw new Exception("Аренда не найдена");
            }

            var artwork = _artworkRepo.GetById(rental.ArtworkId);
            if (artwork == null)
            {
                throw new Exception("Произведение не найдено");
            }

            // Обновляем статус произведения
            artwork.Status = ArtworkStatus.InGallery;
            _artworkRepo.Update(artwork);
            _artworkRepo.SaveChanges();

            // Обновляем запись об аренде (помечаем, что аренда завершена досрочно)
            rental.EndDate = DateTime.Today;
            _rentalRepo.Update(rental);
            _rentalRepo.SaveChanges();
        }

        // Бронирование билетов на выставку
        public Ticket BookTicket(string exhibitionId, string visitorId, DateTime visitDate)
        {
            var exhibition = _exhibitionRepo.GetById(exhibitionId);
            if (exhibition == null)
            {
                throw new Exception("Выставка не найдена");
            }

            if (visitDate < exhibition.StartDate || visitDate > exhibition.EndDate)
            {
                throw new Exception("Дата посещения должна быть в период проведения выставки");
            }

            var visitor = _visitorRepo.GetById(visitorId);
            if (visitor == null)
            {
                throw new Exception("Посетитель не найден");
            }

            // Создаем билет
            var ticket = new Ticket
            {
                ExhibitionId = exhibitionId,
                VisitorId = visitorId,
                VisitDate = visitDate,
                Price = exhibition.TicketPrice,
                Status = TicketStatus.Reserved
            };

            // Добавляем запись о посещении
            visitor.VisitHistory.Add($"{exhibition.Title} ({visitDate.ToShortDateString()})");
            _visitorRepo.Update(visitor);
            _visitorRepo.SaveChanges();

            // Сохраняем билет
            _ticketRepo.Add(ticket);
            _ticketRepo.SaveChanges();

            return ticket;
        }

        // Изменение статуса билета (использован)
        public void UseTicket(string ticketId)
        {
            var ticket = _ticketRepo.GetById(ticketId);
            if (ticket == null)
            {
                throw new Exception("Билет не найден");
            }

            ticket.Status = TicketStatus.Used;
            _ticketRepo.Update(ticket);
            _ticketRepo.SaveChanges();
        }

        // Отмена бронирования билета
        public void CancelTicket(string ticketId)
        {
            var ticket = _ticketRepo.GetById(ticketId);
            if (ticket == null)
            {
                throw new Exception("Билет не найден");
            }

            ticket.Status = TicketStatus.Cancelled;
            _ticketRepo.Update(ticket);
            _ticketRepo.SaveChanges();
        }
    }
}