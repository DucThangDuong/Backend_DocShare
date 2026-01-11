using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DocShareContext _context;

        public IUsers usersRepo { get; private set; }
        public IDocuments documentsRepo { get; private set; }
        public ITags tagsRepo { get; private set; }

        public UnitOfWork(DocShareContext context)
        {
            _context = context;
            usersRepo = new UsersRepo(context);
            documentsRepo = new DocumentsRepo(context);
            tagsRepo = new TagRepo(context);
        }
    }
}
