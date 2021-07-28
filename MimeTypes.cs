namespace HttpListening
{
    using System;
    using System.Collections.Generic;

    internal static class MimeTypes
    {
        private static readonly Dictionary<string, string> Map;

        static MimeTypes()
        {
            Map = new Dictionary<string, string>
            {
                { ".txt", "text/plain" },
                { ".md", "text/plain" },
                { ".cs", "text/plain" },
                { ".csproj", "text/xml" },
                { ".html", "text/html" },
                { ".htm", "text/html" },
                { ".css", "text/css" },
                { ".js", "text/javascript" },
                { ".json", "application/json" },
                { ".xml", "text/xml" },
                { ".bmp", "image/bmp" },
                { ".png", "image/png" },
                { ".gif", "image/gif" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".webp", "image/webp" },
                { ".pdf", "application/pdf" },
                { ".ps1", "text/plain" },
            };
        }

        public static string GetMimeType(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            var extention = System.IO.Path.GetExtension(fileName);
            string mimeType;

            return Map.TryGetValue(extention, out mimeType) ?
                mimeType : "application/octet-stream";
        }
    }
}