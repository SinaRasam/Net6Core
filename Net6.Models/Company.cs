using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net6.Models
{
    [Table("Company")]

    public class Company
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("Name")]
        public string Name { get; set; }

        [Column("StreetAddress")]
        public string? StreetAddress { get; set; }
        [Column("City")]
        public string? City { get; set; }
        [Column("State")]
        public string? State { get; set; }
        [Column("PostalCode")]
        public string? PostalCode { get; set; }
        [Column("PhoneNumber")]
        public string? PhoneNumber { get; set; }
    }
}
