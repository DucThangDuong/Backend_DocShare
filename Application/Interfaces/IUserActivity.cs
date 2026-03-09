

namespace Application.Interfaces
{
    public interface IUserActivity
    {
        public Task ToggleDocumentVoteAsync(int userId, int docId, bool? isLike);
        public Task ToggleSaveDocumentAsync(int userId, int docId);


        public void FollowUser(int followerId, int followedId);
        public Task<bool> HasFollowedAsync(int followerId, int followedId);
        public Task UnfollowUserAsync(int followerId, int followedId);
        public Task SaveChangesAsync();
    }
}
