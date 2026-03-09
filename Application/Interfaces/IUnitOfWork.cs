


namespace Application.Interfaces
{
    public interface IUnitOfWork
    {
        public IUsers UsersRepo { get; }
        public IDocuments DocumentsRepo { get; }
        public ITags TagsRepo { get; }
        public IUniversities UniversitiesRepo { get; }
        public Task SaveChangesAsync();
    }
}
