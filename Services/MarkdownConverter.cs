using System.IO;
using Markdig;
namespace MarkToPdf.Services
{
    public class MarkdownConverter : IDocumentConverter
    {
        public string SupportedExtension  => ".md";
        
            public string ConvertToHtml(string filepath)
        {
            string markdown = File.ReadAllText(filepath);
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            return Markdown.ToHtml(markdown,pipeline);
        }
        

    }
}