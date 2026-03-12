using Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Category> Categories { get; set; }

        DbSet<Document> Documents { get; set; }

        DbSet<DocumentVote> DocumentVotes { get; set; }

        DbSet<SavedDocument> SavedDocuments { get; set; }

        DbSet<Tag> Tags { get; set; }

        DbSet<University> Universities { get; set; }

        DbSet<UniversitySection> UniversitySections { get; set; }

        DbSet<User> Users { get; set; }

        DbSet<UserFollow> UserFollows { get; set; }
    }
}
