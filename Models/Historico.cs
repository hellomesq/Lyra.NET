using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyra.Models
{
    [Table("CAREER_PATH")]
    public class CareerPath
    {
        [Key]
        [Column("PATH_ID")]
        public int Path_Id { get; set; }

        [Column("TITLE")]
        public required string Title { get; set; }

        [Column("CREATED_AT")]
        public DateTime? Created_At { get; set; }

        [Column("USER_ID")]
        public int User_Id { get; set; }
    }
}
