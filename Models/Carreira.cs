using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyra.Models
{
    [Table("CAREER_PATH")] // maiúsculas igual USER_
    public class TrilhaConcluida
    {
        [Key]
        [Column("PATH_ID")]
        public int Id { get; set; }

        [Required]
        [Column("TITLE")]
        public string Trilha { get; set; } = null!;

        [Column("DESCRIPTION")]
        public string? Descricao { get; set; }

        [Required]
        [Column("USER_ID")]
        public int UserId { get; set; }

        [Column("CREATED_AT")]
        public DateTime DataConclusao { get; set; } = DateTime.UtcNow;
    }
}
