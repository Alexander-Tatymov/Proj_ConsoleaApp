using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTracker.App.Security
{
    public static class AccessControl
    {
        public static bool IsAdmin(string? role)
        {
            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }
        public static void RequireAdmin(string? role)
        {
            if (!IsAdmin(role))
                throw new ArgumentException("Недостаточно прав. Нужно: Admin");
        }
    }
}
