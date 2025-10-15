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

        [Test]
        public void GetAvailableArtworks_Returns_Only_InGallery_Items_AreEquivalent()
        {
            var s = CreateService();

            var (artist, a1) = SeedArtistAndArtwork(s);
            var a2 = s.AddArtwork("Картина-2", artist.Id, 2001, "Жанр", "Desc", 100m);
            var a3 = s.AddArtwork("Картина-3", artist.Id, 2002, "Жанр", "Desc", 100m);
            var v  = SeedVisitor(s);

            // Делаем одну продажу и одну аренду, чтобы в наличии осталась только a3
            s.SellArtwork(a1.Id, v.Id, 500m);
            s.RentArtwork(a2.Id, v.Id, DateTime.Today, DateTime.Today.AddDays(3), 50m);

            var availableIds = s.GetAvailableArtworks().Select(x => x.Id).ToList();

            // a3 точно в наличии
            CollectionAssert.IsSubsetOf(new[] { a3.Id }, availableIds);
            // a1 (sold) и a2 (rented) точно НЕ в наличии
            CollectionAssert.DoesNotContain(availableIds, a1.Id);
            CollectionAssert.DoesNotContain(availableIds, a2.Id);

        }

        [Test]
        public void GetPopularArtistsBySales_Top2_AreEqual_With_Order()
        {
            var s = CreateService();

            // A: 2 продажи, B: 1 продажа
            var (aArtist, aArt1) = SeedArtistAndArtwork(s);
            var aArt2 = s.AddArtwork("A-2", aArtist.Id, 2000, "Жанр", "d", 1);
            var bArtist = s.AddArtist("Автор B", "RU", "1900-2000", "Стиль");
            var bArt1   = s.AddArtwork("B-1", bArtist.Id, 2001, "Жанр", "d", 1);

            var buyer = SeedVisitor(s);
            s.SellArtwork(aArt1.Id, buyer.Id, 100m);
            s.SellArtwork(aArt2.Id, buyer.Id, 200m);
            s.SellArtwork(bArt1.Id, buyer.Id, 300m);

            var onlyOurTwo = s.GetPopularArtistsBySales()
                .Where(x => x.Artist.Id == aArtist.Id || x.Artist.Id == bArtist.Id)
                .OrderByDescending(x => x.SoldCount)
                .Select(x => (x.Artist.FullName, x.SoldCount))
                .ToList();

            var expected = new List<(string, int)>
            {
                (aArtist.FullName, 2),
                (bArtist.FullName, 1)
            };

            CollectionAssert.AreEqual(expected, onlyOurTwo);
        }

        [Test]
        public void GetRentedArtworks_AllItemsHaveArtwork_AllItemsAreNotNull()
        {
            var s = CreateService();

            var (_, art) = SeedArtistAndArtwork(s);
            var renter   = SeedVisitor(s);
            s.RentArtwork(art.Id, renter.Id, DateTime.Today, DateTime.Today.AddDays(2), 42m);

            var rentedArtworks = s.GetRentedArtworks()
                                  .Select(t => t.Artwork)
                                  .ToList();

            // У всех записей аренды присутствует связанное произведение
            CollectionAssert.AllItemsAreNotNull(rentedArtworks);
        }

        [Test]
        public void AddExhibition_ArtworkIds_Are_Subset_Of_AllArtworks_IsSubsetOf()
        {
            var s = CreateService();

            var (artist, a1) = SeedArtistAndArtwork(s);
            var a2 = s.AddArtwork("Эксп-2", artist.Id, 2002, "Жанр", "d", 1m);
            var a3 = s.AddArtwork("Эксп-3", artist.Id, 2003, "Жанр", "d", 1m);

            var ex = s.AddExhibition(
                "Осенняя экспозиция",
                DateTime.Today,
                DateTime.Today.AddDays(5),
                "Зал 1",
                new List<string> { a1.Id, a3.Id },   // подмножество всех Id
                250m
            );

            var allArtworkIds = s.GetAllArtworks().Select(x => x.Id).ToList();

            // Id картин выставки — подмножество всех Id картин
            CollectionAssert.IsSubsetOf(ex.ArtworkIds, allArtworkIds);
        }
    }
}
