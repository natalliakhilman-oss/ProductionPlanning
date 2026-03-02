using ProductionPlanning.Models;

namespace ProductionPlanning.ViewModel.Request
{
    public class RequestInfoViewModel
    {
        public ProductRequest Request { get; set; } = new();
        public int CompletedCount {  get; set; }
        public RequestInfoViewModel(ProductRequest _request, int _completedCount) 
        {
            Request = _request;
            CompletedCount = _completedCount;
        }
    }
}
