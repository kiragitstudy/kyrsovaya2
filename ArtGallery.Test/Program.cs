using NUnit.Framework;
using ArtGallery.Services;
using ArtGallery.Models;
using NUnit.Framework.Legacy;

namespace ArtGallery.Test
{
    [TestFixture]
    public class ArtGalleryServiceTests
    {
        private string _originalDir = null!;
        private string _tempDir = null!;

        private ArtGalleryService CreateService() => new ArtGalleryService();

        [SetUp]
        public void SetUp()
        {
            _originalDir = Environment.CurrentDirectory;
            _tempDir = Path.Combine(Path.GetTempPath(), "ArtGalleryTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            Environment.CurrentDirectory = _tempDir;

            TryDeleteIfExists(Path.Combine(_tempDir, "artists.json"));
            TryDeleteIfExists(Path.Combine(_tempDir, "artworks.json"));
            TryDeleteIfExists(Path.Combine(_tempDir, "exhibitions.json"));
            TryDeleteIfExists(Path.Combine(_tempDir, "visitors.json"));
            TryDeleteIfExists(Path.Combine(_tempDir, "sales.json"));
            TryDeleteIfExists(Path.Combine(_tempDir, "rentals.json"));
            TryDeleteIfExists(Path.Combine(_tempDir, "tickets.json"));

            var baseDir = AppContext.BaseDirectory;
            TryDeleteIfExists(Path.Combine(baseDir, "artists.json"));
            TryDeleteIfExists(Path.Combine(baseDir, "artworks.json"));
            TryDeleteIfExists(Path.Combine(baseDir, "exhibitions.json"));
            TryDeleteIfExists(Path.Combine(baseDir, "visitors.json"));
            TryDeleteIfExists(Path.Combine(baseDir, "sales.json"));
            TryDeleteIfExists(Path.Combine(baseDir, "rentals.json"));
            TryDeleteIfExists(Path.Combine(baseDir, "tickets.json"));
        }

        [TearDown]
        public void TearDown()
        {
            Environment.CurrentDirectory = _originalDir;
            try { Directory.Delete(_tempDir, true); } catch { /* ignore */ }
        }

        private static void TryDeleteIfExists(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { /* ignore */ }
        }

        private (Artist artist, Artwork artwork) SeedArtistAndArtwork(ArtGalleryService s)
        {
            var artist = s.AddArtist("Тест Автор", "RU", "1900-2000", "Реализм");
            var art = s.AddArtwork("Тест Картина", artist.Id, 1999, "Пейзаж", "Описание", 1000m);
            return (artist, art);
        }

        private Visitor SeedVisitor(ArtGalleryService s, string name = "Иван Тестов")
        {
            return s.AddVisitor(name, "test@example.com");
        }

        // 1) Простейшие Happy-path

        [Test]
        public void AddArtwork_Adds_Item_And_Relates_To_Artist()
        {
            // Arrange
            var s = CreateService();

            // Act
            var (artist, art) = SeedArtistAndArtwork(s);
            s.LoadReferences();

            // Assert
            var foundArt = s.GetAllArtworks().FirstOrDefault(a => a.Id == art.Id);
            var foundArtist = s.GetAllArtists().FirstOrDefault(a => a.Id == artist.Id);

            Assert.Multiple(() =>
            {
                Assert.That(foundArt, Is.Not.Null, "Произведение не найдено по Id");
                Assert.That(foundArt!.Status, Is.EqualTo(ArtworkStatus.InGallery));
                Assert.That(foundArtist, Is.Not.Null, "Художник не найден по Id");
                Assert.That(foundArtist!.ArtworkIds, Does.Contain(art.Id), "Связь художник->произведение не проставлена");
            });
        }

        [Test]
        public void SellArtwork_Increases_SalesRevenue_And_Changes_Status()
        {
            // Arrange
            var s = CreateService();
            var (_, art) = SeedArtistAndArtwork(s);
            var buyer = SeedVisitor(s);
            var (_, salesBefore, totalBefore) = s.GetTotalRevenue();

            // Act
            var amount = 12345m;
            var sale = s.SellArtwork(art.Id, buyer.Id, amount);
            s.LoadReferences();

            // Assert
            var soldArt = s.GetAllArtworks().First(a => a.Id == art.Id);
            var (_, salesAfter, totalAfter) = s.GetTotalRevenue();

            Assert.Multiple(() =>
            {
                Assert.That(sale.Amount, Is.EqualTo(amount));
                Assert.That(soldArt.Status, Is.EqualTo(ArtworkStatus.Sold));
                Assert.That(salesAfter - salesBefore, Is.EqualTo(amount));
                Assert.That(totalAfter - totalBefore, Is.EqualTo(amount));
            });
        }

        [Test]
        public void RentArtwork_Marks_As_Rented_And_Shows_In_RentedList()
        {
            // Arrange
            var s = CreateService();
            var (_, art) = SeedArtistAndArtwork(s);
            var renter = SeedVisitor(s);
            var from = DateTime.Today;
            var to = from.AddDays(3);

            // Act
            var rental = s.RentArtwork(art.Id, renter.Id, from, to, 777m);
            s.LoadReferences();

            // Assert
            var artReloaded = s.GetAllArtworks().First(a => a.Id == art.Id);
            var rented = s.GetRentedArtworks();
            var rentedThis = rented.FirstOrDefault(r => r.Rental.Id == rental.Id);

            Assert.Multiple(() =>
            {
                Assert.That(artReloaded.Status, Is.EqualTo(ArtworkStatus.Rented));
                Assert.That(rentedThis, Is.Not.Null, "Не нашли только что созданную аренду");
            });
        }

        [Test]
        public void BookTicket_Uses_ExhibitionPrice_And_Changes_TicketRevenue_By_Delta()
        {
            // Arrange
            var s = CreateService();
            var (_, art) = SeedArtistAndArtwork(s);
            var ex = s.AddExhibition("Тест Выставка", DateTime.Today, DateTime.Today.AddDays(10), "Зал 1",
                                     new() { art.Id }, 350m);
            var visitor = SeedVisitor(s);
            var (ticketBefore, salesBefore, totalBefore) = s.GetTotalRevenue();

            // Act
            var t1 = s.BookTicket(ex.Id, visitor.Id, DateTime.Today.AddDays(1));
            var t2 = s.BookTicket(ex.Id, visitor.Id, DateTime.Today.AddDays(2));
            var (ticketAfter, salesAfter, totalAfter) = s.GetTotalRevenue();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(t1.Price, Is.EqualTo(350m));
                Assert.That(t2.Price, Is.EqualTo(350m));
                Assert.That(ticketAfter - ticketBefore, Is.EqualTo(700m), "Δ ticket revenue");
                Assert.That(salesAfter - salesBefore, Is.EqualTo(0m), "Продаж не было");
                Assert.That(totalAfter - totalBefore, Is.EqualTo(700m), "Δ total revenue");
            });
        }

        // 2) Короткие негатив-кейсы

        [Test]
        public void AddArtwork_With_Unknown_Artist_Throws()
        {
            // Arrange
            var s = CreateService();

            // Act + Assert
            var ex = Assert.Throws<Exception>(() =>
                s.AddArtwork("Без автора", "no-such-artist", 2000, "жанр", "desc", 10m));
            StringAssert.Contains("не найден", ex!.Message);
        }

        [Test]
        public void SellArtwork_When_Already_Sold_Throws()
        {
            // Arrange
            var s = CreateService();
            var (_, art) = SeedArtistAndArtwork(s);
            var buyer = SeedVisitor(s);
            s.SellArtwork(art.Id, buyer.Id, 100m);

            // Act + Assert
            var ex = Assert.Throws<Exception>(() => s.SellArtwork(art.Id, buyer.Id, 200m));
            StringAssert.Contains("недоступно для продажи", ex!.Message);
        }

        [Test]
        public void RentArtwork_With_Wrong_Dates_Throws()
        {
            // Arrange
            var s = CreateService();
            var (_, art) = SeedArtistAndArtwork(s);
            var renter = SeedVisitor(s);

            // Act + Assert (конец раньше начала)
            var ex = Assert.Throws<Exception>(() =>
                s.RentArtwork(art.Id, renter.Id, DateTime.Today, DateTime.Today.AddDays(-1), 10m));
            StringAssert.Contains("окончания", ex!.Message);
        }

        [Test]
        public void BookTicket_Outside_Exhibition_Period_Throws()
        {
            // Arrange
            var s = CreateService();
            var (_, art) = SeedArtistAndArtwork(s);
            var exb = s.AddExhibition("Выставка", DateTime.Today, DateTime.Today.AddDays(2), "Зал", new() { art.Id }, 300m);
            var v = SeedVisitor(s);

            // Act + Assert
            var ex = Assert.Throws<Exception>(() =>
                s.BookTicket(exb.Id, v.Id, DateTime.Today.AddDays(5)));
            StringAssert.Contains("период", ex!.Message);
        }
    }
}
