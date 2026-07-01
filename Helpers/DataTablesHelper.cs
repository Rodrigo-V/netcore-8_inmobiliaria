using Microsoft.AspNetCore.Http;

namespace Inmobiliaria.Net8.Helpers
{
    public class DataTablesRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string OrderColumnIndex { get; set; } = "0";
        public string OrderDirection { get; set; } = "desc";
        public string? SearchValue { get; set; }

        public int PageNumber => Length > 0 ? (Start / Length) + 1 : 1;
    }

    public static class DataTablesHelper
    {
        public static DataTablesRequest Parse(IFormCollection form)
        {
            return new DataTablesRequest
            {
                Draw = Convert.ToInt32(form["draw"].FirstOrDefault() ?? "1"),
                Start = Convert.ToInt32(form["start"].FirstOrDefault() ?? "0"),
                Length = Convert.ToInt32(form["length"].FirstOrDefault() ?? "10"),
                OrderColumnIndex = form["order[0][column]"].FirstOrDefault() ?? "0",
                OrderDirection = form["order[0][dir]"].FirstOrDefault() ?? "desc",
                SearchValue = form["search[value]"].FirstOrDefault()
            };
        }

        public static string? GetFormValue(IFormCollection form, string key)
        {
            var value = form[key].FirstOrDefault();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        public static object Success(int draw, int total, IEnumerable<object> data) => new
        {
            draw,
            recordsTotal = total,
            recordsFiltered = total,
            data
        };

        public static object Error(int draw, string message) => new
        {
            draw,
            recordsTotal = 0,
            recordsFiltered = 0,
            data = Array.Empty<object>(),
            error = message
        };
    }
}
