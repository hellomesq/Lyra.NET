using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyra.Models
{
    [Table("USER_")]
    public class User
    {
        [Key]
        [Column("USER_ID")]
        public int User_Id { get; set; }

        [Column("NAME")]
        public required string Name { get; set; }

        [Column("EMAIL")]
        public required string Email { get; set; }

        [Column("PASSWORD")]
        public required string Password { get; set; }

        [Column("EXPERIENCE_LEVEL")]
        public string? Experience_Level { get; set; }

        [Column("CREATED_AT")]
        public DateTime? Created_At { get; set; }
    }
}
