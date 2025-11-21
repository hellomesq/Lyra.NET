using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyra.Models
{
    [Table("career_path")] // tabela de trilhas
    public class TrilhaConcluida
    {
        [Key]
        [Column("path_id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        public string Trilha { get; set; } = null!;

        [Column("description")]
        public string? Descricao { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("created_at")]
        public DateTime DataConclusao { get; set; } = DateTime.UtcNow;
    }
}
