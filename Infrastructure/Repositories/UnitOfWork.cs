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
        private readonly DocShareSimpleDbContext _context;

        public IUsers usersRepo { get; private set; }

        public UnitOfWork(DocShareSimpleDbContext context) { 
            _context=context;
            usersRepo = new Users_Repo(context);
        }
    }
}
