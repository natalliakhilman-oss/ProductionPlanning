using Microsoft.AspNetCore.Identity;
using System.Reflection;

namespace ProductionPlanning.Models
{
    public static class Role
    {
        public const string Administrator = "Администратор системы";
        public const string User = "Пользователь системы";
        public const string Seller = "Сбыт";
        public const string Manufacture = "Производство";
        public const string Header = "Руководитель";
        public const string Accountant = "Учетчик";

        // Get All const 
        public static List<string> GetAllRoleConstants()
        {
            return typeof(Role)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                .Select(fi => (string)fi.GetValue(null))
                .ToList();
        }
    }
    public class DbInitializer
    {
        private readonly DBContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        private const int adminId = 1;
        private const int userId = 2;

        public DbInitializer(
            DBContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager; 
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            // Применить миграции (если используете Code First)
            //await _context.Database.MigrateAsync();

            // Создать роли
            await CreateRolesAsync();

            // Создать пользователей
            await CreateUsersAsync();
        }

        private async Task CreateRolesAsync()
        {
            string[] roleNames = { 
                Role.Administrator,
                Role.User,
                Role.Seller,
                Role.Manufacture,
                Role.Header,
                Role.Accountant
            };

            foreach (var roleName in roleNames)
            {
                var roleExist = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private async Task CreateUsersAsync()
        {
            // Создание администратора
            var adminUser = new User
            {
                UserId = adminId,
                UserName = "admin",
                //Email = "admin@example.com",
                UserFullName = "Administrator",
                EmailConfirmed = true
            };

            var adminPassword = _configuration["DefaultUsers:AdminPassword"] ?? "admin";

            var adminExists = await _userManager.FindByNameAsync(adminUser.UserName);
            if (adminExists == null)
            {
                var createAdmin = await _userManager.CreateAsync(adminUser, adminPassword);
                if (createAdmin.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, Role.Administrator);
                }
            }

            // Создание обычного пользователя
            var regularUser = new User
            {
                UserId = userId,
                UserName = "user",
                //Email = "user@example.com",
                UserFullName = "Пользователь",
                EmailConfirmed = true
            };

            var userPassword = _configuration["DefaultUsers:UserPassword"] ?? "user";

            var userExists = await _userManager.FindByNameAsync(regularUser.UserName);
            if (userExists == null)
            {
                var createUser = await _userManager.CreateAsync(regularUser, userPassword);
                if (createUser.Succeeded)
                {
                    await _userManager.AddToRoleAsync(regularUser, Role.User);
                }
            }
        }
    }
}

