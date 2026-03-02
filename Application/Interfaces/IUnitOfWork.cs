


namespace Application.Interfaces
{
    public interface IUnitOfWork
    {
        public IUsers usersRepo { get; }
        public IDocuments documentsRepo { get; }
        public ITags tagsRepo { get; }
        public IUniversitites universititesRepo { get; }
        public Task SaveAllAsync();
    }
}
