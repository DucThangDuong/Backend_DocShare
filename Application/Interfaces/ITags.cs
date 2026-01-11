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
        public Task<Tag?> HasTag(string Slug,string tag);
        public Task Create(Tag tag);
    }
}
