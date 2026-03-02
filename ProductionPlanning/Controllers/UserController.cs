using ProductionPlanning.Extensions;
using ProductionPlanning.Models;
using ProductionPlanning.ViewModel.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ProductionPlanning.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IServiceScopeFactory _scopeFactory;
        readonly ILogger _logger;

        public UserController(UserManager<User> userManager,
                                RoleManager<IdentityRole> roleManager,
                                ILogger<HomeController> logger,
                                IServiceScopeFactory scopeFactory)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        [Authorize(Roles = Role.Administrator)]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserId = user.UserId,
                    UserName = user.UserName,
                    UserFullName = user.UserFullName,
                    Roles = roles.ToList()
                });
            }

            userViewModels = userViewModels.OrderBy(x => x.UserId).ToList();
            return View(userViewModels);
        }

        // Add new user
        #region Add User
        [HttpGet]
        [Authorize(Roles = Role.Administrator)]
        public IActionResult AddUser()
        {
            var model = new AddUserViewModel()
            {
                AvailableRoles = Role.GetAllRoleConstants()
            };
            return View(model);
        }
        
        [HttpPost]
        [Authorize(Roles = Role.Administrator)]
        public async Task<IActionResult> AddUser(AddUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                List<User> allUsers = await _userManager.Users.ToListAsync();

                var user = new User
                {
                    UserId = allUsers.GetNextUserId(),
                    UserName = model.UserName,
                    UserFullName = model.FullName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Добавляем выбранные роли
                    if (model.SelectedRoles != null && model.SelectedRoles.Count > 0)
                    {
                        foreach (var role in model.SelectedRoles)
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }
                    else
                    {
                        // добавляем базовую роль User
                        await _userManager.AddToRoleAsync(user, Role.User);
                    }

                    return RedirectToAction("Users");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.AvailableRoles = Role.GetAllRoleConstants();
            return View(model);
        }
        #endregion

        //Edit User
        #region EditUser
        public async Task<IActionResult> EditUser(string userId, string? returnUrl = "Users/User")
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            // Проверка прав доступа
            if (!User.IsInRole(Role.Administrator) && User.FindFirstValue(ClaimTypes.NameIdentifier) != userId)
            {
                return Forbid(); // или RedirectToAction("AccessDenied")
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Получаем текущие роли пользователя
            var userRoles = await _userManager.GetRolesAsync(user);

            var viewModel = new EditUserViewModel
            {
                UserId = user.Id,
                Id = user.UserId,
                UserName = user.UserName,
                FullName = user.UserFullName, 
                SelectedRoles = userRoles.ToList(),
                AvailableRoles = Role.GetAllRoleConstants(),
                //AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList(),
                //CurrentRoles = userRoles.ToList(),
            };

            ViewData["ReturnUrlEditUser"] = returnUrl;
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(string userId, EditUserViewModel viewModel, string? returnUrl = "Users/User")
        {
            if (userId != viewModel.UserId)
            {
                return NotFound();
            }

            // Проверка прав доступа
            if (!User.IsInRole(Role.Administrator) && User.FindFirstValue(ClaimTypes.NameIdentifier) != userId)
            {
                return Forbid(); // или RedirectToAction("AccessDenied")
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                try
                {
                    // Обновляем основные данные
                    user.UserName = viewModel.UserName;
                    user.UserFullName = viewModel.FullName;

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        foreach (var error in updateResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(viewModel);
                    }

                    // Обновить роли может только администратор
                    if (User.IsInRole(Role.Administrator))
                    {
                        // Обновляем роли
                        var currentRoles = await _userManager.GetRolesAsync(user);
                        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        if (!removeResult.Succeeded)
                        {
                            foreach (var error in removeResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(viewModel);
                        }

                        if (viewModel.SelectedRoles != null && viewModel.SelectedRoles.Any())
                        {
                            var addResult = await _userManager.AddToRolesAsync(user, viewModel.SelectedRoles);
                            if (!addResult.Succeeded)
                            {
                                foreach (var error in addResult.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                                return View(viewModel);
                            }
                        }
                        else
                        {
                            // добавляем базовую роль User
                            await _userManager.AddToRoleAsync(user, Role.User);
                        }
                    }

                    TempData["SuccessMessage"] = "Данные пользователя успешно обновлены!";
                    var url = returnUrl.DecodeLocalUrl();

                    return RedirectToAction(url[0], url[1]);
                    //return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Ошибка при обновлении: {ex.Message}");
                }
            }

            // Если дошли сюда, что-то пошло не так
            viewModel.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(viewModel);
        }
        #endregion EditUser

        [HttpPost]
        [Authorize(Roles = Role.Administrator)]
        public async Task<JsonResult> DeleteUser(string userId)
        {
            try
            {
                // Проверяем, не пытается ли пользователь удалить сам себя
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == currentUserId)
                {
                    return Json(new { success = false, message = "Вы не можете удалить свой собственный аккаунт" });
                }

                User user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Пользователь не найден" });
                }

                // if default Admin
                if (user.UserId == AppInfo.MainAdminId)
                {
                    return Json(new { success = false, message = "Нельзя удалить этого пользователя" });
                }

                IdentityResult result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "Пользователь успешно удален" });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, message = $"Ошибка при удалении: {errors}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Произошла ошибка: {ex.Message}" });
            }
        }

        // Вспомогательный метод для получения partial view
        [HttpGet]
        [Authorize(Roles = Role.Administrator)]
        public async Task<PartialViewResult> GetUsersPartial()
        {
            var users = _userManager.Users.ToList();
            var userViewModels = new List<UserViewModel>();

            foreach (var _user in users)
            {
                var roles = await _userManager.GetRolesAsync(_user);
                userViewModels.Add(new UserViewModel
                {
                    Id = _user.Id,
                    UserId = _user.UserId,
                    UserName = _user.UserName,
                    UserFullName = _user.UserFullName,
                    Roles = roles.ToList()
                });
            }

            userViewModels = userViewModels.OrderBy(x => x.UserId).ToList();
            return PartialView("_UsersPartialView", userViewModels);
        }

        // Change Password
        #region ChangePassword
        [HttpGet]
        //[Authorize(Roles = Role.Administrator)]
        public async Task<IActionResult> ChangeUserPassword(string userId, string? returnUrl = "Users/User")
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            // Проверка прав доступа
            if (!User.IsInRole(Role.Administrator) && User.FindFirstValue(ClaimTypes.NameIdentifier) != userId)
            {
                return Forbid(); // или RedirectToAction("AccessDenied")
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ChangeUserPasswordViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserFullName = user.UserFullName,
            };

            ViewData["ReturnUrlEditPass"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        //[Authorize(Roles = Role.Administrator)]
        public async Task<IActionResult> ChangeUserPassword(ChangeUserPasswordViewModel model, string? returnUrl = "Users/User")
        {
            
            // Проверка прав доступа
            if (!User.IsInRole(Role.Administrator) && User.FindFirstValue(ClaimTypes.NameIdentifier) != model.UserId)
            {
                return Forbid(); // или RedirectToAction("AccessDenied")
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Пользователь не найден");
                return View(model);
            }

            try
            {
                // Генерируем токен сброса пароля
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Сбрасываем пароль
                var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                if (result.Succeeded)
                {
                    // Логируем действие
                    _logger.LogInformation("Пароль пользователя {UserName} изменен администратором {AdminUser}",
                        user.UserName, User.Identity.Name);

                    // Принудительно разлогиниваем пользователя (опционально)
                    await InvalidateUserSessions(user.Id);

                    TempData["SuccessMessage"] = $"Пароль для пользователя {user.UserName} успешно изменен!";
                    
                    var url = returnUrl.DecodeLocalUrl();

                    return RedirectToAction(url[0], url[1]);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при смене пароля для пользователя {UserId}", model.UserId);
                ModelState.AddModelError(string.Empty, "Произошла ошибка при смене пароля");
            }

            // Заполняем данные пользователя для повторного отображения формы
            model.UserName = user.UserName;
            model.UserFullName = user.UserFullName;
            return View(model);
        }

        // Метод для принудительного разлогинивания пользователя
        private async Task InvalidateUserSessions(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Обновляем security stamp - это инвалидирует все существующие сессии
                await _userManager.UpdateSecurityStampAsync(user);
            }
        }
        #endregion ChangePassword

    }


}

