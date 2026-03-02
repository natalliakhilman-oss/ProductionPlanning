using ProductionPlanning.Models;
namespace ProductionPlanning.ViewModel.Request
{
    public class EditRequestViewModel
    {
        public ProductRequest App { get; set; } = new ProductRequest();
        public EditRequestViewModel() { }
        public EditRequestViewModel(ProductRequest _app)
        {
            App = _app;
        }

        public bool IsUseNoteChB { get; set; } = false;
        public bool IsUseNote
        {
            get
            {
                if (App.NoteId != null)
                    return true;

                return false;
            }
        }
    }
}
