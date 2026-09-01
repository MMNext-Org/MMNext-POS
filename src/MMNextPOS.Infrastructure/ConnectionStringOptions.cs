using Microsoft.Extensions.Configuration;

namespace MMNextPOS.Infrastructure
{
    /// <summary>
    /// Configuration options for database connection strings.
    /// </summary>
    public sealed class ConnectionStringOptions
    {
        public const string SectionName = "ConnectionStrings";

        public string Default { get; set; } = string.Empty;
    }
}