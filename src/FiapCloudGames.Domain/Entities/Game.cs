namespace FiapCloudGames.Domain.Entities
{
    public class Game
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public ICollection<Library> LibraryEntries { get; set; } = [];
    }
}
