

namespace Application.Interfaces
{
    public interface IUserActivity
    {
        public Task AddVoteDocumentAsync(int userId, int docId, bool? isLike);
        public Task AddUserSaveDocumentAsync(int userId, int docId);


        public void AddFollowing(int followerId, int followedId);
        public Task<bool> HasFollowedAsync(int followerId, int followedId);
        public Task RemoveFollowingAsync(int followerId, int followedId);
        public Task SaveChangeAsync();
    }
}
