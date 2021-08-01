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
                { ".bmp", "image/bmp" },
                { ".cs", "text/plain" },
                { ".csproj", "text/xml" },
                { ".css", "text/css" },
                { ".csv", "text/csv" },
                { ".eml", "message/rfc822" },
                { ".gif", "image/gif" },
                { ".htm", "text/html" },
                { ".html", "text/html" },
                { ".ico", "image/x-icon" },
                { ".jpeg", "image/jpeg" },
                { ".jpg", "image/jpeg" },
                { ".js", "text/javascript" },
                { ".json", "application/json" },
                { ".log", "text/plain" },
                { ".md", "text/plain" },
                { ".mp3", "audio/mpeg" },
                { ".pdf", "application/pdf" },
                { ".png", "image/png" },
                { ".ps1", "text/plain" },
                { ".rss", "application/rss+xml" },
                { ".rtf", "text/rtf" },
                { ".txt", "text/plain" },
                { ".wav", "audio/wav" },
                { ".webp", "image/webp" },
                { ".xml", "text/xml" },
                { ".xsd", "text/xml" },
                { ".xsl", "text/xml" },
                { ".xslt", "text/xml" },
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