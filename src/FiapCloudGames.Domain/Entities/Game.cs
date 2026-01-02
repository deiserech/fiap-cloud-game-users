using FiapCloudGames.Users.Domain.Enums;

namespace FiapCloudGames.Users.Domain.Entities
{
    public class Game
    {
        public Guid Id { get; set; }
        public int Code { get; set; }
        public string Title { get; set; } = string.Empty;
        public GameCategory Category { get; set; } = GameCategory.Unknown;
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? RemovedAt { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Library> LibraryEntries { get; set; } = new List<Library>();

        public Game() { }

        public Game(int code,
                    string title,
                    DateTimeOffset updatedAt,
                    DateTimeOffset? removedAt,
                    GameCategory category = GameCategory.Unknown)
        {
            Code = code;
            Title = title;
            Category = category;
            UpdatedAt = updatedAt;
            RemovedAt = removedAt;
            IsActive = removedAt == null;
        }
    }
}
