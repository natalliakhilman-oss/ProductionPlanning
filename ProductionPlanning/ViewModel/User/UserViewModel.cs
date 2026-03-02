namespace ProductionPlanning.ViewModel.User
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public int UserId {  get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public List<string> Roles { get; set; }
    }
}
