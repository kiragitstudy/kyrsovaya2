namespace ArtGallery.Repository
{
    public interface IRepository<T> where T : ArtGallery.Models.BaseEntity
    {
        List<T> GetAll();
        T? GetById(string id);
        void Add(T entity);
        void Update(T entity);
        void Delete(string id);
        void SaveChanges();
    }
}