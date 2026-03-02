using ProductionPlanning.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.Claims;

namespace ProductionPlanning.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static async Task<string> GetUserFullNameAsync(this ClaimsPrincipal principal, UserManager<User> userManager)
        {
            var user = await userManager.GetUserAsync(principal);
            return user?.UserFullName ?? principal.Identity?.Name ?? "?";
        }

        public static bool IsInRole(this ClaimsPrincipal principal, string role)
        {
            return principal.IsInRole(role);
        }

        public static int GetNextUserId(this List<User> users)
        {
            return users.Count > 0 ? users.Max(u => u.UserId) + 1 : 1;
        }
    }
}
