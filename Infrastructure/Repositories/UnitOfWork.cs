using Application.Interfaces;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DocShareContext _context;

        public IUsers usersRepo { get; private set; }
        public IDocuments documentsRepo { get; private set; }
        public ITags tagsRepo { get; private set; }
        public IUserActivity userActivityRepo { get; private set; }

        public IUniversitites universititesRepo {  get; private set; }

        public UnitOfWork(DocShareContext context)
        {
            _context = context;
            usersRepo = new UsersRepo(context);
            documentsRepo = new DocumentsRepo(context);
            tagsRepo = new TagRepo(context);
            userActivityRepo = new UserActivity(context);
            universititesRepo =new UniversitiesRepo(context);
        }
        public async Task SaveAllAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
