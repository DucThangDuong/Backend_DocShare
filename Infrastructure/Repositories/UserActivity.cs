using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
namespace Infrastructure.Repositories
{
    public class UserActivity : IUserActivity
    {
        private readonly DocShareContext _context;
        public UserActivity(DocShareContext context)
        {
            _context = context;
        }
        public async Task AddVoteDocumentAsync(int userId, int docId, bool? isLike)
        {
            var existingVote = await _context.DocumentVotes
                                             .FirstOrDefaultAsync(v => v.UserId == userId && v.DocumentId == docId);

            if (existingVote != null)
            {
                if (isLike == null)
                {
                    _context.DocumentVotes.Remove(existingVote);
                }
                else
                {
                    existingVote.IsLike = isLike.Value;
                    existingVote.VotedAt = DateTime.UtcNow;
                }
            }
            else if (isLike.HasValue)
            {
                var newVote = new DocumentVote
                {
                    UserId = userId,
                    DocumentId = docId,
                    IsLike = isLike.Value,
                    VotedAt = DateTime.UtcNow
                };
                _context.DocumentVotes.Add(newVote);
            }
        }

        public async Task AddUserSaveDocumentAsync(int userId, int docId)
        {
            var savedDoc = await _context.SavedDocuments
                                         .FirstOrDefaultAsync(s => s.UserId == userId && s.DocumentId == docId);

            if (savedDoc != null)
            {
                _context.SavedDocuments.Remove(savedDoc);
            }
            else
            {
                var newSave = new SavedDocument
                {
                    UserId = userId,
                    DocumentId = docId,
                    SavedAt = DateTime.UtcNow,
                };
                _context.SavedDocuments.Add(newSave);
            }
        }

        public async Task<List<Document>> GetSavedDocumentsByUserAsync(int userId)
        {
            return await _context.SavedDocuments.AsNoTracking().Where(s => s.UserId == userId)
                           .Include(s => s.Document).Select(s => s.Document).ToListAsync();
        }

        public void AddFollowing(int followerId, int followedId)
        {
            UserFollow result = new UserFollow
            {
                FollowerId = followerId,
                FollowedId = followedId,
                CreatedAt = DateTime.UtcNow
            };
            _context.UserFollows.Add(result);
        }
        public async Task<bool> HasFollowedAsync(int followerId, int followedId)
        {
            return await _context.UserFollows.AsNoTracking().AnyAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);
        }

        public async Task RemoveFollowingAsync(int followerId, int followedId)
        {
            var follow = _context.UserFollows.FirstOrDefault(f => f.FollowerId == followerId && f.FollowedId == followedId);
            if (follow != null)
            {
                _context.UserFollows.Remove(follow);
            }
        }
        public async Task SaveChangeAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

