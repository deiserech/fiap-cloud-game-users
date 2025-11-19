namespace FiapCloudGames.Users.Domain.Entities
{
    public class Game
    {

        public Guid Id { get; set; }
        public int Code { get; set; }
        public string Title { get; set; } = string.Empty;

        public DateTimeOffset UpdatedAt { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Library> LibraryEntries { get; set; } = [];
        public Game(Guid id, int code, string title, DateTimeOffset updatedAt, DateTimeOffset? removedAt)
        {
            Id = id;
            Code = code;
            Title = title;
            UpdatedAt = updatedAt;
            IsActive = removedAt == null;
        }
    }
}
