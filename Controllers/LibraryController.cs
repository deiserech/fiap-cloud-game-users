using Microsoft.AspNetCore.Mvc;
using FiapCloudGames.Users.Data;

namespace FiapCloudGames.Users.Controllers;

[ApiController]
[Route("api/users")]
public class LibraryController : ControllerBase
{
    private readonly UsersDbContext _db;

    public LibraryController(UsersDbContext db)
    {
        _db = db;
    }

    [HttpGet("{userId}/library")]
    public IActionResult GetLibrary(Guid userId)
    {
        var list = _db.Library.Where(l => l.UserId == userId)
            .Select(l => new { l.GameId, l.PurchaseId, l.AcquiredAt })
            .ToList();

        return Ok(list);
    }
}
