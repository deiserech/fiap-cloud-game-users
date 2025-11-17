using System.Diagnostics;

namespace FiapCloudGames.Shared.Tracing
{
    public static class Tracing
    {
        public static readonly ActivitySource ActivitySource = new("FiapCloudGames.Application");
    }
}
