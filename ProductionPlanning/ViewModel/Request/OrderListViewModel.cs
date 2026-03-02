using ProductionPlanning.Extensions;
using ProductionPlanning.Models;

namespace ProductionPlanning.ViewModel.Request
{
    public class OrderListViewModel
    {
        public List<Order> Orders { get; set; } = new();
        public ProductRequest? Request { get; set; }

        public OrderListViewModel(List<Order> _orders, ProductRequest request)
        {
            Orders = _orders;
            Request = request;
        }
        public OrderListViewModel(List<Order> _orders)
        {
            Orders = _orders;
        }
        public OrderListViewModel() { }

        public string DateOrder()
        {
            if (Orders.Any())
            {
                return Orders[0].DateCreate.ToString("dd.MM.yyyy");
            }

            return "";
        }
        public string RequestNum()
        {
            if(Request != null)
            {
                return Request.GetNumString();
            }
            return "";
        }
        public int GetCount()
        {
            int count = 0;
            if (Orders.Any())
            {
                count = Orders.Sum(c => c.Count);
            }
            return count;
        }
    }
}
