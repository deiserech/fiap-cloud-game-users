using FiapCloudGames.Users.Domain.Entities;

namespace FiapCloudGames.Users.Application.DTOs
{
    public class LibraryDto
    {
        public int UserCode { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public int GameCode { get; set; }
        public string? GameTitle { get; set; }

        public static IEnumerable<LibraryDto> FromEntity(IEnumerable<Library> library)
        {
            return library.Select(l =>
                new LibraryDto
                {
                    UserCode = l.User.Code,
                    UserName = l.User.Name,
                    UserEmail = l.User.Email,
                    GameCode = l.Game.Code,
                    GameTitle = l.Game.Title
                });
        }
    }
}
