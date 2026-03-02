using ProductionPlanning.Extensions;
using ProductionPlanning.Models;

namespace ProductionPlanning.ViewModel.Request
{
    public class RequestTabaleViewModel
    {
        public List<ProductRequest> Requests { get; set; } = new();
        //public List<Note> Notes { get; set; } = new();

        public RequestTabaleViewModel(List<ProductRequest> _requests) 
        {
            Requests = _requests;
        }

        public string GetNumber(ProductRequest _request)
        {
            string result = _request.GetNumString();

            return result;
        }
    }
}
