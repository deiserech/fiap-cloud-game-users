using System.ComponentModel.DataAnnotations;

namespace FiapCloudGames.Users.Application.DTOs
{
    public class GameDto
    {
        public required int Code { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}
