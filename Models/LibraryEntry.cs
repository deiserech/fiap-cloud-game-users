using System.ComponentModel.DataAnnotations;

namespace FiapCloudGames.Users.Models;

public class LibraryEntry
{
    [Key]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public Guid PurchaseId { get; set; }
    public DateTimeOffset AcquiredAt { get; set; }
}
