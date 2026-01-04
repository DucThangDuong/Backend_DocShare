using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories
{
    public class Users_Repo : IUsers
    {
        DocShareSimpleDbContext _context;
        public Users_Repo(DocShareSimpleDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public async Task<bool> ExistEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(e => e.Email == email);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<User?> GetUserAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiryDate)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id ==userId );

            if (user != null)
            {
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = expiryDate;
                await _context.SaveChangesAsync();
            }
        }
    }
}
