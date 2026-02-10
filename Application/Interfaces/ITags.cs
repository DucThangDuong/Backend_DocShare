using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITags
    {
        public Task<Tag?> HasTag( string tagSlug, string tag);
        public Task CreateTag(Tag tag);
        public Task<List<string>?> GetTagOfDocument(int docid);
        public Task RemoveAllTagsByDocIdAsync(int docId);
    }
}
