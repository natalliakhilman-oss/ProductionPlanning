using ProductionPlanning.Models;
namespace ProductionPlanning.ViewModel.Request
{
    public class AddRequestViewModel
    {
        public ProductRequest App { get; set; } = new ProductRequest();

        public bool IsUseNote {  get; set; } = false;
        //public bool IsUseNote 
        //{ 
        //    get 
        //    { 
        //        if (App.NoteId != null) 
        //            return true; 

        //        return false; 
        //    } 
        //}
    }
}
