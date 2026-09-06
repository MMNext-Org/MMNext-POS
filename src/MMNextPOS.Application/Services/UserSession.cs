using System.Collections.Generic;
using System.Linq;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Default implementation of IUserSession.
    /// </summary>
    public class UserSession : IUserSession
    {
        public User? CurrentUser { get; set; }

        public IReadOnlyList<Role> Roles { get; set; } = Array.Empty<Role>();

        public bool IsAuthenticated => CurrentUser != null;

        public bool HasRole(string roleCode)
        {
            if (string.IsNullOrWhiteSpace(roleCode) || Roles == null)
                return false;

            return Roles.Any(r => string.Equals(r.Code, roleCode, StringComparison.OrdinalIgnoreCase));
        }

        public void Clear()
        {
            CurrentUser = null;
            Roles = Array.Empty<Role>();
        }
    }
}
