using System.IO;
using System.Net;
namespace MarkToPdf.Services
{
    public class TextConverter : IDocumentConverter
{
    public string SupportedExtension => ".txt";

    public string ConvertToHtml(string filePath)
    {
        string rawText = File.ReadAllText(filePath);

        string safeText = WebUtility.HtmlEncode(rawText);

        return $@"
            <pre style='
                font-family: Consolas, monospace;
                font-size: 11pt;
                white-space: pre-wrap;
                line-height: 1.5;
                color: #222;
            '>{safeText}</pre>";
    }
}
}

