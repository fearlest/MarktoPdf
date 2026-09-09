using System.IO;

namespace MarkToPdf.Services
{
    public class HtmlConverter : IDocumentConverter
    {
        public string[] SupportedExtensions => new[] { ".html", ".htm" };

        public string ConvertToHtml(string filepath)
        {
            return File.ReadAllText(filepath);
        }
    }
}
