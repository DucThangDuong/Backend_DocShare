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
        public string username { get; set; }
        public string fullname { get; set; }
        public string avatarurl { get; set; }
        public DateTime createdat { get; set; }
    }
    public class ResUserPrivate : ResUserPublicDto
    {
        public string email { get; set; }
        public long storagelimit { get; set; }
        public long usedstorage { get; set; }
    }
}
