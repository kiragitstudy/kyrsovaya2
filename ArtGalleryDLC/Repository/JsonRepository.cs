using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace ArtGalleryDLC.Repository
{
    public class JsonRepository<T> : IRepository<T> where T : ArtGalleryDLC.Models.BaseEntity
    {
        private readonly string _filePath;
        private List<T> _entities;

        public JsonRepository(string fileName)
        {
            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _filePath = Path.Combine(directory, fileName);
            _entities = LoadFromFile();
        }

        private List<T> LoadFromFile()
        {
            if (!File.Exists(_filePath))
            {
                return new List<T>();
            }

            string json = File.ReadAllText(_filePath);
            
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<T>();
            }
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            
            return JsonSerializer.Deserialize<List<T>>(json, options) ?? new List<T>();
        }

        public List<T> GetAll()
        {
            return _entities;
        }

        public T? GetById(string id)
        {
            return _entities.FirstOrDefault(e => e.Id == id);
        }

        public void Add(T entity)
        {
            _entities.Add(entity);
        }

        public void Update(T entity)
        {
            int index = _entities.FindIndex(e => e.Id == entity.Id);
            if (index != -1)
            {
                _entities[index] = entity;
            }
        }

        public void Delete(string id)
        {
            var entity = GetById(id);
            if (entity != null)
            {
                _entities.Remove(entity);
            }
        }

        public void SaveChanges()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            
            string json = JsonSerializer.Serialize(_entities, options);
            File.WriteAllText(_filePath, json, Encoding.UTF8);
        }
    }
}