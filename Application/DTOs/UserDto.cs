using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ResUserPublicDto
    {
        public int id { get; set; }
        public string username { get; set; } = null!;
        public string fullname { get; set; } = null!;
        public string? avatarUrl{ get; set; }
        public int? UniversityId { get; set; }
        public string? UniversityName { get; set; }
    }
    public class ResUserPrivate : ResUserPublicDto
    {
        public string? email { get; set; }
        public long storagelimit { get; set; }
        public long usedstorage { get; set; }
        public bool hasPassword { get; set; }
    }
}
