using ProductionPlanning.Models;

namespace ProductionPlanning.ViewModel.Request
{
    public class EditOrderViewModel
    {
        public Order Order { get; set; } = new Order();
        public ProductRequest App{ get; set; } = new();
        public int ComplitedCount {  get; set; }
        public int CurrentCount {  get; set; }  // Count before edit

        public int MaxCount
        {
            get
            {
                return App.Count - ComplitedCount;
            }
        }

        public EditOrderViewModel(Order _order, int _complitedCount, DateTime? _dateTime)
        {
            Order = _order;
            App = _order.ProductRequest;
            ComplitedCount = _complitedCount - _order.Count;
            CurrentCount = _order.Count;

            if(_dateTime != null)
            {
                Order.DateCreate = _dateTime.Value;
            }
        }
        public EditOrderViewModel() { }
    }
}
