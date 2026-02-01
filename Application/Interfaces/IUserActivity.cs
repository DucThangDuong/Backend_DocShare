using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserActivity
    {
        public Task<bool> VoteDocumentAsync(int userId, int docId, bool? isLike);
        public Task<bool> ToggleSaveDocumentAsync(int userId, int docId);
        public Task<List<Document>> GetSavedDocumentsByUserAsync(int userId);

    }
}
