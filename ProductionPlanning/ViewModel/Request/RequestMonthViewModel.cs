using ProductionPlanning.Models;

namespace ProductionPlanning.ViewModel.Request
{
    public class ProductsViewModel
    {
        public DateTime Date { get; set; }
        public List<DateTime> Days { get; set; } = new();
        public List<ProductGroupViewModel> ProductGroups { get; set; } = new List<ProductGroupViewModel>();
        public ProductsViewModel(DateTime _date, List<Equipment> _equipmentList, List<ProductRequest> _requestList)
        {
            Date = _date;
            Days = GetDays(_date);
            ProductGroups = GetProductGroups(_equipmentList, _requestList);
        }

        private List<ProductGroupViewModel> GetProductGroups(List<Equipment> _equipmentList, List<ProductRequest> _requestList)
        {
            var productGroups = new List<ProductGroupViewModel>();

            foreach (var equipment in _equipmentList)
            {
                // find requests for this equpment
                var requests = _requestList.FindAll(e => e.EquipmentId == equipment.Id);
                var model = new ProductGroupViewModel(equipment, requests, Days);

                productGroups.Add(model);
            }

            return productGroups;
        }
        private List<DateTime> GetDays(DateTime _date)
        {
            var days = Enumerable.Range(1, DateTime.DaysInMonth(_date.Year, _date.Month))
                .Select(day => new DateTime(_date.Year, _date.Month, day))
                .ToList();

            return days;
        }

        public int TotalOrderCount
        {
            get
            {
                int sum = 0;

                foreach(var prGroup in ProductGroups)
                {
                    foreach(var requestVM in prGroup.RequestVMList)
                    {
                        foreach(var order in requestVM.Request.Orders)
                        {
                            if(!order.IsDeleted)
                                sum += order.Count;
                        }
                    }
                }

                return sum;
            }
        }
    }


    public class ProductGroupViewModel
    {
        public Equipment Equipment { get; set; }
        public List<RequestViewModel> RequestVMList { get; set; } = new List<RequestViewModel>();

        /*  Прибор 
         *  заявки для прибора c Orders
         */         
        public ProductGroupViewModel(Equipment _equipment, List<ProductRequest> _requestList, List<DateTime> _days) 
        {
            Equipment = _equipment;

            foreach(var request in _requestList)
            {
                var rVM = new RequestViewModel(request, _days);
                RequestVMList.Add(rVM);
            }

        }
    }

    public class RequestViewModel
    {
        public ProductRequest Request { get; set; } // Orders Included in Request
        public List<DayCell> Calendar { get; set; }

        // Заявка, список дней с количеством изделий в определнный день
        public RequestViewModel(ProductRequest _request, List<DateTime> _days) 
        {
            Request = _request;
            Calendar = GetCalendar(_request, _days);
        }

        private List<DayCell> GetCalendar(ProductRequest _request, List<DateTime> _days) 
        {
            var calendar = new List<DayCell>();

            foreach(var day in _days)
            {
                int summa = _request.Orders
                    .Where(d => d.DateCreate.Year == day.Year
                            && d.DateCreate.Month == day.Month
                            && d.DateCreate.Day == day.Day
                            && !d.IsDeleted)
                    .Sum(c => c.Count);

                //bool _IsInRange = _request.DateCreate.Date <= day.Date && day.Date <= _request.DateMaxFinish.Date;
                bool _IsInRange = IsInRange(_request,day);
                
                var cell = new DayCell()
                {
                    Date = day.Date,
                    Count = summa,
                    IsToday = DateTime.Today.Day == day.Day,
                    IsInRange = _IsInRange

                };

                calendar.Add(cell);
            }

            return calendar;
        }

        private bool IsInRange(ProductRequest _request, DateTime _day)
        {
            var dateStart = _request.MonthDatePlanning;
            var dateFinish = _request.DateMaxFinish;

            if (_request.DatePlaningStart != null)
            {
                dateStart = _request.DatePlaningStart.Value;
            }
            if (_request.DateFinish != null)
            {
                dateFinish = _request.DateFinish.Value;
            }

            return dateStart.Date <= _day.Date && _day.Date <= dateFinish.Date;
        }
    }
    public class DayCell
    {
        public DateTime Date { get; set; }
        public int Count {  get; set; }
        public bool IsToday {  get; set; }
        public bool IsInRange{  get; set; }
    }
}
