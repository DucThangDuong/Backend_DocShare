using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ReqUniversitySectionDTO
    {
        [Required]
        public string Name { get; set; }
    }
}
