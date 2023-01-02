using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Net6.Models
{
    public class CoverType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Cover Type")]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
