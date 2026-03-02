using ProductionPlanning.Models;

namespace ProductionPlanning.ViewModel.Request
{
    public class AddOrderViewModel
    {
        public Order Order { get; set; } = new Order();
        public ProductRequest App{ get; set; } = new();
        public int ComplitedCount {  get; set; }
        public int MaxCount
        {
            get
            {
                return App.Count - ComplitedCount;
            }
        }
        public AddOrderViewModel(ProductRequest _request, int _complitedCount, DateTime? _dateTime)
        {
            App = _request;
            ComplitedCount = _complitedCount;

            if(_dateTime != null)
            {
                Order.DateCreate = _dateTime.Value;
            }
        }
        public AddOrderViewModel() { }
    }
}
