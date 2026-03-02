using ProductionPlanning.Models;

namespace ProductionPlanning.ViewModel.Request
{
    public class AccounterOrderListViewModel
    {
        public List<Order> Orders { get; set; } = new();
        public PaginationInfo Pagination { get; set; }
        public string CurrentSortOrder { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
}
