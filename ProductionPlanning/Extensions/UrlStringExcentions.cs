namespace ProductionPlanning.Extensions
{
    public static class UrlStringExcentions
    {
        public static string[] DecodeLocalUrl(this string url)
        {
            // return Method/Controller
            if (!string.IsNullOrEmpty(url))
            {
                // Заменяем разделитель
                var actionAndController = url.Replace("_", "/");
                var parts = actionAndController.Split('/');

                return parts.ToArray();
            }

            return new string[0];
        }
    }
}
