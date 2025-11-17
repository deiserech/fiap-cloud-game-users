using System.ComponentModel.DataAnnotations;

namespace FiapCloudGames.Application.DTOs
{
    public class LibraryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int GameId { get; set; }
    }
}
