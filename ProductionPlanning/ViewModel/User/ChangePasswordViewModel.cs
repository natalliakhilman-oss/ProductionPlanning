using System.ComponentModel.DataAnnotations;

namespace ProductionPlanning.ViewModel.User
{
    public class ChangeUserPasswordViewModel
    {
        [Required(ErrorMessage = "ID пользователя обязателен")]
        public string UserId { get; set; }

        [Display(Name = "Новый пароль")]
        [Required(ErrorMessage = "Новый пароль обязателен")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Подтверждение пароля")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }

        // Для отображения информации о пользователе
        public string UserName { get; set; }
        public string UserFullName { get; set; }

        //public string ReturnUrl { get; set; }
    }
}
