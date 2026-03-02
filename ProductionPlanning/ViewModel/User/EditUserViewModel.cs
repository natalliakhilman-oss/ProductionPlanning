using System.ComponentModel.DataAnnotations;

namespace ProductionPlanning.ViewModel.User
{
    public class EditUserViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        [Required(ErrorMessage = "Логин обязательно")]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [Display(Name = "Имя пользователя")]
        public string FullName { get; set; }

        public List<string> SelectedRoles { get; set; } = new();
        public List<string> AvailableRoles { get; set; } = new();

        // Для отображения текущих ролей
        //public List<string> CurrentRoles { get; set; } = new();
    }
}
