namespace Inmobiliaria.Net8.Helpers
{
    public static class GoogleDriveHelper
    {
        public static string? ConvertirUrlThumbnail(string? urlCompartir)
        {
            if (string.IsNullOrEmpty(urlCompartir))
                return null;

            if (urlCompartir.Contains("drive.google.com/thumbnail"))
                return urlCompartir;

            string? fileId = null;

            if (urlCompartir.Contains("/file/d/"))
            {
                var startIndex = urlCompartir.IndexOf("/file/d/") + 8;
                var endIndex = urlCompartir.IndexOf("/", startIndex);
                if (endIndex == -1)
                    endIndex = urlCompartir.IndexOf("?", startIndex);
                if (endIndex == -1)
                    endIndex = urlCompartir.Length;

                fileId = urlCompartir.Substring(startIndex, endIndex - startIndex);
            }
            else if (urlCompartir.Contains("id="))
            {
                var startIndex = urlCompartir.IndexOf("id=") + 3;
                var endIndex = urlCompartir.IndexOf("&", startIndex);
                if (endIndex == -1)
                    endIndex = urlCompartir.Length;

                fileId = urlCompartir.Substring(startIndex, endIndex - startIndex);
            }
            else if (!urlCompartir.Contains("drive.google.com"))
            {
                fileId = urlCompartir;
            }

            if (!string.IsNullOrEmpty(fileId))
                return $"https://drive.google.com/thumbnail?id={fileId}&sz=w800";

            return urlCompartir;
        }
    }
}
