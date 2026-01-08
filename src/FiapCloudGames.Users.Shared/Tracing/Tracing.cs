using System.Diagnostics;

namespace FiapCloudGames.Users.Shared.Tracing
{
    public static class Tracing
    {
        public static readonly ActivitySource ActivitySource = new("FiapCloudGames.Users.Application");
    }
}
