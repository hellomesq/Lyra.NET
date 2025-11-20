namespace Lyra.DTOs
{
    public class HistoricoDto
    {
        public int PathId { get; set; }
        public required string Title { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
