using System.ComponentModel.DataAnnotations;

namespace BLOOM_FILTER.Entities
{
    public class User
    {
        [Required]
        public Guid Id { get; set; }
        public string? Email { get; set; }
    }
}
