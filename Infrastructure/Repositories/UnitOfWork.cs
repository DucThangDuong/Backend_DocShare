using Application.Interfaces;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DocShareContext _context;

        public IUsers UsersRepo { get; private set; }
        public IDocuments DocumentsRepo { get; private set; }
        public ITags TagsRepo { get; private set; }

        public IUniversities UniversitiesRepo {  get; private set; }

        public UnitOfWork(DocShareContext context)
        {
            _context = context;
            UsersRepo = new UsersRepo(context);
            DocumentsRepo = new DocumentsRepo(context);
            TagsRepo = new TagRepo(context);
            UniversitiesRepo = new UniversitiesRepo(context);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
