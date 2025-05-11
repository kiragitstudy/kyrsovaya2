// Проект: Система управления художественной галереей
// Структура проекта:
// 1. Модели данных
// 2. Хранилище (репозитории для работы с JSON файлами)
// 3. Сервисы (бизнес-логика)
// 4. Консольный интерфейс

// ======== 4. ИНТЕРФЕЙС КОНСОЛЬНОГО ПРИЛОЖЕНИЯ ========

namespace ArtGalleryDLC
{
    using ArtGalleryDLC.Models;
    using ArtGalleryDLC.Services;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class Program
    {
        private static ArtGalleryService _service;

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");

            _service = new ArtGalleryService();

            // Инициализация примерных данных для демонстрации
            InitializeSampleData();

            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("=== СИСТЕМА УПРАВЛЕНИЯ ХУДОЖЕСТВЕННОЙ ГАЛЕРЕЕЙ ===");
                Console.WriteLine("1. Просмотр произведений искусства");
                Console.WriteLine("2. Просмотр художников");
                Console.WriteLine("3. Просмотр выставок");
                Console.WriteLine("4. Просмотр посетителей");
                Console.WriteLine("5. Управление билетами");
                Console.WriteLine("6. Продажи и аренда");
                Console.WriteLine("7. Отчеты");
                Console.WriteLine("0. Выход");
                Console.Write("\nВыберите действие: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.Clear();

                    switch (choice)
                    {
                        case 1:
                            ArtworksMenu();
                            break;
                        case 2:
                            ArtistsMenu();
                            break;
                        case 3:
                            ExhibitionsMenu();
                            break;
                        case 4:
                            VisitorsMenu();
                            break;
                        case 5:
                            TicketsMenu();
                            break;
                        case 6:
                            SalesAndRentalsMenu();
                            break;
                        case 7:
                            ReportsMenu();
                            break;
                        case 0:
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Неверный выбор. Нажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Неверный ввод. Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        // === МЕНЕДЖМЕНТ ПРОИЗВЕДЕНИЙ ИСКУССТВА ===
        private static void ArtworksMenu()
        {
            bool back = false;

            while (!back)
            {
                Console.Clear();
                Console.WriteLine("=== ПРОИЗВЕДЕНИЯ ИСКУССТВА ===");
                Console.WriteLine("1. Просмотр всех произведений");
                Console.WriteLine("2. Просмотр доступных произведений");
                Console.WriteLine("3. Добавить новое произведение");
                Console.WriteLine("0. Назад");
                Console.Write("\nВыберите действие: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.Clear();

                    switch (choice)
                    {
                        case 1:
                            ShowAllArtworks();
                            break;
                        case 2:
                            ShowAvailableArtworks();
                            break;
                        case 3:
                            AddNewArtwork();
                            break;
                        case 0:
                            back = true;
                            break;
                        default:
                            Console.WriteLine("Неверный выбор. Нажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Неверный ввод. Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        private static void ShowAllArtworks()
        {
            _service.LoadReferences();
            var artworks = _service.GetAvailableArtworks();

            Console.WriteLine("=== ВСЕ ПРОИЗВЕДЕНИЯ ИСКУССТВА ===");

            if (artworks.Count == 0)
            {
                Console.WriteLine("Произведения отсутствуют.");
            }
            else
            {
                foreach (var artwork in artworks)
                {
                    Console.WriteLine($"ID: {artwork.Id}");
                    Console.WriteLine($"Название: {artwork.Title}");
                    Console.WriteLine($"Художник: {artwork.Artist?.FullName ?? "Неизвестно"}");
                    Console.WriteLine($"Год создания: {artwork.Year}");
                    Console.WriteLine($"Жанр: {artwork.Genre}");
                    Console.WriteLine($"Статус: {GetArtworkStatusName(artwork.Status)}");
                    Console.WriteLine($"Оценочная стоимость: {artwork.EstimatedValue:C}");
                    Console.WriteLine("-----------------------------------");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void ShowAvailableArtworks()
        {
            var artworks = _service.GetAvailableArtworks();

            Console.WriteLine("=== ДОСТУПНЫЕ ПРОИЗВЕДЕНИЯ ИСКУССТВА ===");

            if (artworks.Count == 0)
            {
                Console.WriteLine("Доступные произведения отсутствуют.");
            }
            else
            {
                foreach (var artwork in artworks)
                {
                    Console.WriteLine($"ID: {artwork.Id}");
                    Console.WriteLine($"Название: {artwork.Title}");
                    Console.WriteLine($"Художник: {artwork.Artist?.FullName ?? "Неизвестно"}");
                    Console.WriteLine($"Год создания: {artwork.Year}");
                    Console.WriteLine($"Жанр: {artwork.Genre}");
                    Console.WriteLine($"Оценочная стоимость: {artwork.EstimatedValue:C}");
                    Console.WriteLine("-----------------------------------");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void AddNewArtwork()
        {
            Console.WriteLine("=== ДОБАВЛЕНИЕ НОВОГО ПРОИЗВЕДЕНИЯ ===");

            // Показываем список художников для выбора
            var artists = _service._artistRepo.GetAll();
            Console.WriteLine("Доступные художники:");
            foreach (var artist in artists)
            {
                Console.WriteLine($"{artist.Id}: {artist.FullName} ({artist.Country})");
            }

            Console.Write("\nВведите название произведения: ");
            string title = Console.ReadLine();

            Console.Write("Введите ID художника: ");
            string artistId = Console.ReadLine();

            Console.Write("Введите год создания: ");
            if (!int.TryParse(Console.ReadLine(), out int year))
            {
                Console.WriteLine("Неверный формат года.");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите жанр: ");
            string genre = Console.ReadLine();

            Console.Write("Введите описание: ");
            string description = Console.ReadLine();

            Console.Write("Введите оценочную стоимость: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal value))
            {
                Console.WriteLine("Неверный формат стоимости.");
                Console.ReadKey();
                return;
            }

            try
            {
                var artwork = _service.AddArtwork(title, artistId, year, genre, description, value);
                Console.WriteLine($"Произведение \"{artwork.Title}\" успешно добавлено!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        // === МЕНЕДЖМЕНТ ХУДОЖНИКОВ ===
        private static void ArtistsMenu()
        {
            bool back = false;

            while (!back)
            {
                Console.Clear();
                Console.WriteLine("=== ХУДОЖНИКИ ===");
                Console.WriteLine("1. Просмотр всех художников");
                Console.WriteLine("2. Популярные художники по продажам");
                Console.WriteLine("3. Добавить нового художника");
                Console.WriteLine("0. Назад");
                Console.Write("\nВыберите действие: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.Clear();

                    switch (choice)
                    {
                        case 1:
                            ShowAllArtists();
                            break;
                        case 2:
                            ShowPopularArtists();
                            break;
                        case 3:
                            AddNewArtist();
                            break;
                        case 0:
                            back = true;
                            break;
                        default:
                            Console.WriteLine("Неверный выбор. Нажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Неверный ввод. Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        private static void ShowAllArtists()
        {
            _service.LoadReferences();
            var artists = _service._artistRepo.GetAll();

            Console.WriteLine("=== ВСЕ ХУДОЖНИКИ ===");

            if (artists.Count == 0)
            {
                Console.WriteLine("Художники отсутствуют.");
            }
            else
            {
                foreach (var artist in artists)
                {
                    Console.WriteLine($"ID: {artist.Id}");
                    Console.WriteLine($"ФИО: {artist.FullName}");
                    Console.WriteLine($"Страна: {artist.Country}");
                    Console.WriteLine($"Годы жизни: {artist.LifeYears}");
                    Console.WriteLine($"Стиль: {artist.Style}");
                    Console.WriteLine($"Количество работ: {artist.ArtworkIds.Count}");
                    Console.WriteLine("-----------------------------------");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void ShowPopularArtists()
        {
            var popularArtists = _service.GetPopularArtistsBySales();

            Console.WriteLine("=== ПОПУЛЯРНЫЕ ХУДОЖНИКИ ПО ПРОДАЖАМ ===");

            if (popularArtists.Count == 0)
            {
                Console.WriteLine("Данные о продажах отсутствуют.");
            }
            else
            {
                int rank = 1;
                foreach (var item in popularArtists)
                {
                    Console.WriteLine($"{rank}. {item.Artist.FullName} - {item.SoldCount} проданных работ");
                    rank++;
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void AddNewArtist()
        {
            Console.WriteLine("=== ДОБАВЛЕНИЕ НОВОГО ХУДОЖНИКА ===");

            Console.Write("Введите ФИО художника: ");
            string fullName = Console.ReadLine();

            Console.Write("Введите страну: ");
            string country = Console.ReadLine();

            Console.Write("Введите годы жизни (например, 1856-1925): ");
            string lifeYears = Console.ReadLine();

            Console.Write("Введите стиль: ");
            string style = Console.ReadLine();

            try
            {
                var artist = _service.AddArtist(fullName, country, lifeYears, style);
                Console.WriteLine($"Художник \"{artist.FullName}\" успешно добавлен!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        // === МЕНЕДЖМЕНТ ВЫСТАВОК ===
        private static void ExhibitionsMenu()
        {
            bool back = false;

            while (!back)
            {
                Console.Clear();
                Console.WriteLine("=== ВЫСТАВКИ ===");
                Console.WriteLine("1. Просмотр всех выставок");
                Console.WriteLine("2. Предстоящие выставки");
                Console.WriteLine("3. Добавить новую выставку");
                Console.WriteLine("0. Назад");
                Console.Write("\nВыберите действие: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.Clear();

                    switch (choice)
                    {
                        case 1:
                            ShowAllExhibitions();
                            break;
                        case 2:
                            ShowUpcomingExhibitions();
                            break;
                        case 3:
                            AddNewExhibition();
                            break;
                        case 0:
                            back = true;
                            break;
                        default:
                            Console.WriteLine("Неверный выбор. Нажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Неверный ввод. Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        private static void ShowAllExhibitions()
        {
            _service.LoadReferences();
            var exhibitions = _service._exhibitionRepo.GetAll();

            Console.WriteLine("=== ВСЕ ВЫСТАВКИ ===");

            if (exhibitions.Count == 0)
            {
                Console.WriteLine("Выставки отсутствуют.");
            }
            else
            {
                foreach (var exhibition in exhibitions)
                {
                    Console.WriteLine($"ID: {exhibition.Id}");
                    Console.WriteLine($"Название: {exhibition.Title}");
                    Console.WriteLine(
                        $"Период: {exhibition.StartDate.ToShortDateString()} - {exhibition.EndDate.ToShortDateString()}");
                    Console.WriteLine($"Место: {exhibition.Location}");
                    Console.WriteLine($"Количество экспонатов: {exhibition.ArtworkIds.Count}");
                    Console.WriteLine($"Цена билета: {exhibition.TicketPrice:C}");
                    Console.WriteLine("-----------------------------------");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void ShowUpcomingExhibitions()
        {
            var exhibitions = _service.GetUpcomingExhibitions();

            Console.WriteLine("=== ПРЕДСТОЯЩИЕ ВЫСТАВКИ ===");

            if (exhibitions.Count == 0)
            {
                Console.WriteLine("Предстоящие выставки отсутствуют.");
            }
            else
            {
                foreach (var exhibition in exhibitions)
                {
                    string status = DateTime.Today < exhibition.StartDate ? "Скоро" : "Действует";

                    Console.WriteLine($"ID: {exhibition.Id}");
                    Console.WriteLine($"Название: {exhibition.Title} ({status})");
                    Console.WriteLine(
                        $"Период: {exhibition.StartDate.ToShortDateString()} - {exhibition.EndDate.ToShortDateString()}");
                    Console.WriteLine($"Место: {exhibition.Location}");
                    Console.WriteLine($"Цена билета: {exhibition.TicketPrice:C}");
                    Console.WriteLine("-----------------------------------");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void AddNewExhibition()
        {
            Console.WriteLine("=== ДОБАВЛЕНИЕ НОВОЙ ВЫСТАВКИ ===");

            Console.Write("Введите название выставки: ");
            string title = Console.ReadLine();

            Console.Write("Введите дату начала (дд.мм.гггг): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime startDate))
            {
                Console.WriteLine("Неверный формат даты.");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите дату окончания (дд.мм.гггг): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime endDate))
            {
                Console.WriteLine("Неверный формат даты.");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите место проведения: ");
            string location = Console.ReadLine();

            Console.Write("Введите цену билета: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal ticketPrice))
            {
                Console.WriteLine("Неверный формат цены.");
                Console.ReadKey();
                return;
            }

            // Выбор произведений для выставки
            var availableArtworks = _service.GetAvailableArtworks();
            List<string> selectedArtworkIds = new List<string>();

            Console.WriteLine("\nДоступные произведения для выставки:");
            foreach (var artwork in availableArtworks)
            {
                Console.WriteLine($"{artwork.Id}: {artwork.Title} ({artwork.Artist?.FullName ?? "Неизвестный"})");
            }

            Console.WriteLine("\nВведите ID произведений через запятую или оставьте пустым для завершения:");
            string artworkIdsInput = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(artworkIdsInput))
            {
                selectedArtworkIds = artworkIdsInput.Split(',')
                    .Select(id => id.Trim())
                    .ToList();
            }

            try
            {
                var exhibition = _service.AddExhibition(title, startDate, endDate, location, selectedArtworkIds,
                    ticketPrice);
                Console.WriteLine($"Выставка \"{exhibition.Title}\" успешно добавлена!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        // === МЕНЕДЖМЕНТ ПОСЕТИТЕЛЕЙ ===
        private static void VisitorsMenu()
        {
            bool back = false;

            while (!back)
            {
                Console.Clear();
                Console.WriteLine("=== ПОСЕТИТЕЛИ ===");
                Console.WriteLine("1. Просмотр всех посетителей");
                Console.WriteLine("2. Добавить нового посетителя");
                Console.WriteLine("0. Назад");
                Console.Write("\nВыберите действие: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.Clear();

                    switch (choice)
                    {
                        case 1:
                            ShowAllVisitors();
                            break;
                        case 2:
                            AddNewVisitor();
                            break;
                        case 0:
                            back = true;
                            break;
                        default:
                            Console.WriteLine("Неверный выбор. Нажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Неверный ввод. Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        private static void ShowAllVisitors()
        {
            _service.LoadReferences();
            var visitors = _service._visitorRepo.GetAll();

            Console.WriteLine("=== ВСЕ ПОСЕТИТЕЛИ ===");

            if (visitors.Count == 0)
            {
                Console.WriteLine("Посетители отсутствуют.");
            }
            else
            {
                foreach (var visitor in visitors)
                {
                    Console.WriteLine($"ID: {visitor.Id}");
                    Console.WriteLine($"ФИО: {visitor.FullName}");
                    Console.WriteLine($"Контактная информация: {visitor.ContactInfo}");
                    Console.WriteLine($"Количество посещений: {visitor.VisitHistory.Count}");
                    Console.WriteLine($"Количество покупок: {visitor.PurchaseIds.Count}");
                    Console.WriteLine("-----------------------------------");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void AddNewVisitor()
        {
            Console.WriteLine("=== ДОБАВЛЕНИЕ НОВОГО ПОСЕТИТЕЛЯ ===");

            Console.Write("Введите ФИО посетителя: ");
            string fullName = Console.ReadLine();

            Console.Write("Введите контактную информацию (телефон/email): ");
            string contactInfo = Console.ReadLine();

            try
            {
                var visitor = _service.AddVisitor(fullName, contactInfo);
                Console.WriteLine($"Посетитель \"{visitor.FullName}\" успешно добавлен!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        // === МЕНЕДЖМЕНТ БИЛЕТОВ ===
        private static void TicketsMenu()
        {
            bool back = false;

            while (!back)
            {
                Console.Clear();
                Console.WriteLine("=== БИЛЕТЫ ===");
                Console.WriteLine("1. Просмотр всех билетов");
                Console.WriteLine("2. Забронировать билет");
                Console.WriteLine("3. Отметить билет использованным");
                Console.WriteLine("4. Отменить бронирование");
                Console.WriteLine("0. Назад");
                Console.Write("\nВыберите действие: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.Clear();

                    switch (choice)
                    {
                        case 1:
                            ShowAllTickets();
                            break;
                        case 2:
                            BookNewTicket();
                            break;
                        case 3:
                            UseExistingTicket();
                            break;
                        case 4:
                            CancelTicketBooking();
                            break;
                        case 0:
                            back = true;
                            break;
                        default:
                            Console.WriteLine("Неверный выбор. Нажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Неверный ввод. Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        private static void ShowAllTickets()
        {
            _service.LoadReferences();
            var tickets = _service._ticketRepo.GetAll();

            Console.WriteLine("=== ВСЕ БИЛЕТЫ ===");

            if (tickets.Count == 0)
            {
                Console.WriteLine("Билеты отсутствуют.");
            }
            else
            {
                foreach (var ticket in tickets)
                {
                    Console.WriteLine($"ID: {ticket.Id}");
                    Console.WriteLine($"Выставка: {ticket.Exhibition?.Title ?? "Неизвестно"}");
                    Console.WriteLine($"Посетитель: {ticket.Visitor?.FullName ?? "Неизвестно"}");
                    Console.WriteLine($"Дата посещения: {ticket.VisitDate.ToShortDateString()}");
                    Console.WriteLine($"Цена: {ticket.Price:C}");
                    Console.WriteLine($"Статус: {GetTicketStatusName(ticket.Status)}");
                    Console.WriteLine("-----------------------------------");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void BookNewTicket()
        {
            Console.WriteLine("=== БРОНИРОВАНИЕ БИЛЕТА ===");

            // Показываем список выставок
            var exhibitions = _service.GetUpcomingExhibitions();
            Console.WriteLine("Доступные выставки:");
            foreach (var exhibition in exhibitions)
            {
                Console.WriteLine($"{exhibition.Id}: {exhibition.Title} " +
                                  $"({exhibition.StartDate.ToShortDateString()} - {exhibition.EndDate.ToShortDateString()})");
            }

            Console.Write("\nВведите ID выставки: ");
            string exhibitionId = Console.ReadLine();

            // Показываем список посетителей
            var visitors = _service._visitorRepo.GetAll();
            Console.WriteLine("\nЗарегистрированные посетители:");
            foreach (var visitor in visitors)
            {
                Console.WriteLine($"{visitor.Id}: {visitor.FullName}");
            }

            Console.Write("\nВведите ID посетителя: ");
            string visitorId = Console.ReadLine();

            Console.Write("Введите дату посещения (дд.мм.гггг): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime visitDate))
            {
                Console.WriteLine("Неверный формат даты.");
                Console.ReadKey();
                return;
            }

            try
            {
                var ticket = _service.BookTicket(exhibitionId, visitorId, visitDate);
                Console.WriteLine($"Билет успешно забронирован! ID билета: {ticket.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void UseExistingTicket()
        {
            Console.WriteLine("=== ИСПОЛЬЗОВАНИЕ БИЛЕТА ===");

            // Показываем список забронированных билетов
            _service.LoadReferences();
            var tickets = _service._ticketRepo.GetAll()
                .Where(t => t.Status == TicketStatus.Reserved)
                .ToList();

            Console.WriteLine("Забронированные билеты:");
            foreach (var ticket in tickets)
            {
                Console.WriteLine($"{ticket.Id}: {ticket.Exhibition?.Title ?? "Неизвестно"} - " +
                                  $"{ticket.Visitor?.FullName ?? "Неизвестно"} ({ticket.VisitDate.ToShortDateString()})");
            }

            Console.Write("\nВведите ID билета: ");
            string ticketId = Console.ReadLine();

            try
            {
                _service.UseTicket(ticketId);
                Console.WriteLine("Билет отмечен как использованный!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void CancelTicketBooking()
        {
            Console.WriteLine("=== ОТМЕНА БРОНИРОВАНИЯ БИЛЕТА ===");

            // Показываем список забронированных билетов
            _service.LoadReferences();
            var tickets = _service._ticketRepo.GetAll()
                .Where(t => t.Status == TicketStatus.Reserved)
                .ToList();

            Console.WriteLine("Забронированные билеты:");
            foreach (var ticket in tickets)
            {
                Console.WriteLine($"{ticket.Id}: {ticket.Exhibition?.Title ?? "Неизвестно"} - " +
                                  $"{ticket.Visitor?.FullName ?? "Неизвестно"} ({ticket.VisitDate.ToShortDateString()})");
            }

            Console.Write("\nВведите ID билета: ");
            string ticketId = Console.ReadLine();

            try
            {
                _service.CancelTicket(ticketId);
                Console.WriteLine("Бронирование билета отменено!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        // === ПРОДАЖИ И АРЕНДА ===
        private static void SalesAndRentalsMenu()
        {
            bool back = false;

            while (!back)
            {
                Console.Clear();
                Console.WriteLine("=== ПРОДАЖИ И АРЕНДА ===");
                Console.WriteLine("1. Просмотр всех продаж");
                Console.WriteLine("2. Просмотр всех аренд");
                Console.WriteLine("3. Продать произведение");
                Console.WriteLine("4. Сдать произведение в аренду");
                Console.WriteLine("5. Вернуть произведение из аренды");
                Console.WriteLine("0. Назад");
                Console.Write("\nВыберите действие: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.Clear();

                    switch (choice)
                    {
                        case 1:
                            ShowAllSales();
                            break;
                        case 2:
                            ShowAllRentals();
                            break;
                        case 3:
                            SellArtwork();
                            break;
                        case 4:
                            RentArtwork();
                            break;
                        case 5:
                            ReturnRentedArtwork();
                            break;
                        case 0:
                            back = true;
                            break;
                        default:
                            Console.WriteLine("Неверный выбор. Нажмите любую клавишу для продолжения...");
                            Console.ReadKey();
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Неверный ввод. Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        private static void ShowAllSales()
        {
            _service.LoadReferences();
            var sales = _service._saleRepo.GetAll();

            Console.WriteLine("=== ВСЕ ПРОДАЖИ ===");

            if (sales.Count == 0)
            {
                Console.WriteLine("Продажи отсутствуют.");
            }
            else
            {
                foreach (var sale in sales)
                {
                    Console.WriteLine($"ID: {sale.Id}");
                    Console.WriteLine($"Произведение: {sale.Artwork?.Title ?? "Неизвестно"}");
                    Console.WriteLine($"Художник: {sale.Artwork?.Artist?.FullName ?? "Неизвестно"}");
                    Console.WriteLine($"Покупатель: {sale.Buyer?.FullName ?? "Неизвестно"}");
                    Console.WriteLine($"Дата: {sale.Date.ToShortDateString()}");
                    Console.WriteLine($"Сумма: {sale.Amount:C}");
                    Console.WriteLine("-----------------------------------");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void ShowAllRentals()
        {
            var rentedArtworks = _service.GetRentedArtworks();

            Console.WriteLine("=== АРЕНДЫ ПРОИЗВЕДЕНИЙ ===");

            if (rentedArtworks.Count == 0)
            {
                Console.WriteLine("Активные аренды отсутствуют.");
            }
            else
            {
                foreach (var item in rentedArtworks)
                {
                    Console.WriteLine($"ID аренды: {item.Rental.Id}");
                    Console.WriteLine($"Произведение: {item.Artwork?.Title ?? "Неизвестно"}");
                    Console.WriteLine($"Художник: {item.Artwork?.Artist?.FullName ?? "Неизвестно"}");
                    Console.WriteLine($"Арендатор: {item.Rental.RenterId}");
                    Console.WriteLine(
                        $"Период: {item.Rental.StartDate.ToShortDateString()} - {item.Rental.EndDate.ToShortDateString()}");
                    Console.WriteLine($"Стоимость: {item.Rental.Cost:C}");
                    Console.WriteLine("-----------------------------------");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void SellArtwork()
        {
            Console.WriteLine("=== ПРОДАЖА ПРОИЗВЕДЕНИЯ ===");

            // Показываем список доступных произведений
            var availableArtworks = _service.GetAvailableArtworks();

            Console.WriteLine("Доступные произведения:");
            foreach (var artwork in availableArtworks)
            {
                Console.WriteLine($"{artwork.Id}: {artwork.Title} - {artwork.Artist?.FullName ?? "Неизвестно"} " +
                                  $"(Оценочная стоимость: {artwork.EstimatedValue:C})");
            }

            Console.Write("\nВведите ID произведения: ");
            string artworkId = Console.ReadLine();

            // Показываем список посетителей
            var visitors = _service._visitorRepo.GetAll();
            Console.WriteLine("\nПосетители (потенциальные покупатели):");
            foreach (var visitor in visitors)
            {
                Console.WriteLine($"{visitor.Id}: {visitor.FullName}");
            }

            Console.Write("\nВведите ID покупателя: ");
            string buyerId = Console.ReadLine();

            Console.Write("Введите сумму продажи: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                Console.WriteLine("Неверный формат суммы.");
                Console.ReadKey();
                return;
            }

            try
            {
                var sale = _service.SellArtwork(artworkId, buyerId, amount);
                Console.WriteLine($"Произведение успешно продано за {sale.Amount:C}!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void RentArtwork()
        {
            Console.WriteLine("=== АРЕНДА ПРОИЗВЕДЕНИЯ ===");

            // Показываем список доступных произведений
            var availableArtworks = _service.GetAvailableArtworks();

            Console.WriteLine("Доступные произведения:");
            foreach (var artwork in availableArtworks)
            {
                Console.WriteLine($"{artwork.Id}: {artwork.Title} - {artwork.Artist?.FullName ?? "Неизвестно"}");
            }

            Console.Write("\nВведите ID произведения: ");
            string artworkId = Console.ReadLine();

            // Показываем список посетителей
            var visitors = _service._visitorRepo.GetAll();
            Console.WriteLine("\nПосетители (потенциальные арендаторы):");
            foreach (var visitor in visitors)
            {
                Console.WriteLine($"{visitor.Id}: {visitor.FullName}");
            }

            Console.Write("\nВведите ID арендатора: ");
            string renterId = Console.ReadLine();

            Console.Write("Введите дату начала аренды (дд.мм.гггг): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime startDate))
            {
                Console.WriteLine("Неверный формат даты.");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите дату окончания аренды (дд.мм.гггг): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime endDate))
            {
                Console.WriteLine("Неверный формат даты.");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите стоимость аренды: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal cost))
            {
                Console.WriteLine("Неверный формат стоимости.");
                Console.ReadKey();
                return;
            }

            try
            {
                var rental = _service.RentArtwork(artworkId, renterId, startDate, endDate, cost);
                Console.WriteLine($"Произведение успешно сдано в аренду до {rental.EndDate.ToShortDateString()}!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        // Возврат произведения из аренды (меню "Продажи и аренда" -> пункт 5)
        private static void ReturnRentedArtwork()
        {
            Console.WriteLine("=== ВОЗВРАТ ПРОИЗВЕДЕНИЯ ИЗ АРЕНДЫ ===");
            var rented = _service.GetRentedArtworks();

            if (rented.Count == 0)
            {
                Console.WriteLine("Активных аренд нет.");
            }
            else
            {
                Console.WriteLine("Список активных аренд:");
                foreach (var item in rented)
                {
                    Console.WriteLine($"ID аренды: {item.Rental.Id} | Произведение: {item.Artwork.Title} | Арендатор: {item.Rental.Renter.FullName} | До: {item.Rental.EndDate.ToShortDateString()}");
                }
            }

            Console.Write("\nВведите ID аренды для возврата: ");
            string rentalId = Console.ReadLine();
            try
            {
                _service.ReturnRentedArtwork(rentalId);
                Console.WriteLine("Произведение успешно возвращено в галерею!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        // Меню отчетов
        private static void ReportsMenu()
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("=== ОТЧЕТЫ ===");
                Console.WriteLine("1. Список доступных произведений");
                Console.WriteLine("2. Выставки ближайшего месяца");
                Console.WriteLine("3. Популярные художники по продажам");
                Console.WriteLine("4. Общая выручка");
                Console.WriteLine("5. Арендованные произведения");
                Console.WriteLine("0. Назад");
                Console.Write("\nВыберите действие: ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Неверный ввод.");
                    Console.ReadKey();
                    continue;
                }

                Console.Clear();
                switch (choice)
                {
                    case 1:
                        var available = _service.GetAvailableArtworks();
                        Console.WriteLine("=== ДОСТУПНЫЕ ПРОИЗВЕДЕНИЯ ===");
                        if (available.Count == 0)
                            Console.WriteLine("Нет доступных произведений.");
                        else
                            foreach (var art in available)
                                Console.WriteLine($"{art.Title} ({art.Artist.FullName}) — {art.Genre}, {art.Year}");
                        break;

                    case 2:
                        var upcoming = _service.GetUpcomingExhibitions();
                        Console.WriteLine("=== ПРЕДСТОЯЩИЕ ВЫСТАВКИ ===");
                        if (upcoming.Count == 0)
                            Console.WriteLine("Нет выставок на ближайший месяц.");
                        else
                            foreach (var ex in upcoming)
                                Console.WriteLine($"{ex.Title}: {ex.StartDate.ToShortDateString()} - {ex.EndDate.ToShortDateString()} ({ex.Location})");
                        break;

                    case 3:
                        var popular = _service.GetPopularArtistsBySales();
                        Console.WriteLine("=== ПОПУЛЯРНЫЕ ХУДОЖНИКИ ===");
                        if (popular.Count == 0)
                            Console.WriteLine("Данные о продажах отсутствуют.");
                        else
                        {
                            int rank = 1;
                            foreach (var item in popular)
                                Console.WriteLine($"{rank++}. {item.Artist.FullName} — {item.SoldCount} проданных");
                        }
                        break;

                    case 4:
                        var (ticketRev, salesRev, total) = _service.GetTotalRevenue();
                        Console.WriteLine("=== ОБЩАЯ ВЫРУЧКА ===");
                        Console.WriteLine($"Выручка от билетов: {ticketRev:C}");
                        Console.WriteLine($"Выручка от продаж: {salesRev:C}");
                        Console.WriteLine($"Итого: {total:C}");
                        break;

                    case 5:
                        var rentedArts = _service.GetRentedArtworks();
                        Console.WriteLine("=== АРЕНДОВАННЫЕ ПРОИЗВЕДЕНИЯ ===");
                        if (rentedArts.Count == 0)
                            Console.WriteLine("Нет активных аренд.");
                        else
                            foreach (var item in rentedArts)
                                Console.WriteLine($"{item.Artwork.Title} (аренда до {item.Rental.EndDate.ToShortDateString()})");
                        break;

                    case 0:
                        back = true;
                        continue;

                    default:
                        Console.WriteLine("Неверный выбор.");
                        break;
                }

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }
        // Инициализация демонстрационных данных
        private static void InitializeSampleData()
        {
            // Если данные уже загружены, пропускаем повторное добавление
            if (_service._artistRepo.GetAll().Any()) return;

            // Добавляем художников
            var artist1 = _service.AddArtist("Иван Иванов", "Россия", "1950-2000", "Реализм");
            var artist2 = _service.AddArtist("Анна Смирнова", "Франция", "1965-наст", "Импрессионизм");

            // Добавляем произведения искусства
            var art1 = _service.AddArtwork(
                "Утро в лесу",
                artist1.Id,
                1995,
                "Пейзаж",
                "Теплый весенний пейзаж",
                150000m
            );
            var art2 = _service.AddArtwork(
                "Вечерний город",
                artist2.Id,
                2005,
                "Городской пейзаж",
                "Огни ночного мегаполиса",
                200000m
            );

            // Добавляем выставку
            var exhibition = _service.AddExhibition(
                "Весенняя коллекция",
                DateTime.Today.AddDays(1),
                DateTime.Today.AddDays(30),
                "Главный зал",
                new List<string> { art1.Id, art2.Id },
                500m
            );

            // Добавляем посетителя
            var visitor = _service.AddVisitor("Ольга Петрова", "olga.pet@example.com");

            // Бронирование билета
            _service.BookTicket(exhibition.Id, visitor.Id, DateTime.Today.AddDays(2));

            // Продажа и аренда произведений
            _service.SellArtwork(art1.Id, visitor.Id, art1.EstimatedValue);
            _service.RentArtwork(art2.Id, visitor.Id, DateTime.Today, DateTime.Today.AddDays(7), 10000m);
        }

        

        // Помощники для отображения статусов
        private static string GetArtworkStatusName(ArtworkStatus status)
        {
            return status switch
            {
                ArtworkStatus.InGallery => "В галерее",
                ArtworkStatus.Sold => "Продано",
                ArtworkStatus.Rented => "В аренде",
                _ => status.ToString()
            };
        }

        private static string GetTicketStatusName(TicketStatus status)
        {
            return status switch
            {
                TicketStatus.Reserved => "Забронирован",
                TicketStatus.Used => "Использован",
                TicketStatus.Cancelled => "Отменен",
                _ => status.ToString()
            };
        }
    }
}
