namespace Lyra.DTOs
{
    public class CarreiraDto
    {
        public int UserId { get; set; }
        public string Trilha { get; set; } = null!;
        public string? Descricao { get; set; }
    }
}
