using System.IO;

namespace MarkToPdf.Services
{
    public class HtmlConverter : IDocumentConverter
    {
        public string SupportedExtension => ".html";

        public string ConvertToHtml(string filepath)
        {
            return File.ReadAllText(filepath);
        }
    }
}