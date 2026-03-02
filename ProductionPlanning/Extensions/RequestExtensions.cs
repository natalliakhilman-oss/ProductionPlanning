using ProductionPlanning.Models;
using System.ComponentModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProductionPlanning.Extensions
{
    public static class ProductRequestExtensions
    {
        public static string GetNumString(this ProductRequest _request)
        {
            string result = $"{_request.DateCreate.ToString("yyyyMMdd")}_{_request.DayNumber.ToString("D4")}";

            if (_request.NoteId != null && _request.Note != null) 
            {
                result += _request.Note.GetNumString("_");
            }

            return result;
        }

        public static int GetOrderSum(this ProductRequest _request)
        {
            var totalSum = _request.Orders.Sum(o => o.Count);
            return totalSum;
        }

        public static string GetOrderProgress(this ProductRequest _request)
        {
            var totalSum = _request.Orders.Where(o => !o.IsDeleted).Sum(o => o.Count);
            return $"{totalSum}/{_request.Count}";
        }
        public static string GetDate(this DateTime? _date)
        {
            return _date != null ? _date.Value.ToString("dd.MM.yyyy") : "-";
        }
        public static string GetDate(this DateTime _date)
        {
            return _date != DateTime.MinValue ? _date.ToString("dd.MM.yyyy") : "-";
        }
        public static string GetRequestFinish(this ProductRequest _request)
        {
            var totalOrderSum = _request.GetOrderSum();
            if(totalOrderSum >= _request.Count)
            {
                var lastOrderDate = _request.Orders.Max(o => o.DateCreate);
                return lastOrderDate.GetDate();
            }

            return "-";
        }
        
    }
}
