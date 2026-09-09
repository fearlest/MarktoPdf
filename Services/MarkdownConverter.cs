using System.IO;
using Markdig;

namespace MarkToPdf.Services
{
    public class MarkdownConverter : IDocumentConverter
    {
        public string[] SupportedExtensions => new[] { ".md" };

        public string ConvertToHtml(string filepath)
        {
            string markdown = File.ReadAllText(filepath);
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            string bodyHtml = Markdown.ToHtml(markdown, pipeline);

            // Diğer converter'larla (Docx, Excel) tutarlı görünüm için
            // aynı stil şablonuna sarılıyor.
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8' />
                    <style>
                        body {{
                            font-family: 'Segoe UI', Calibri, Arial, sans-serif;
                            font-size: 11pt;
                            line-height: 1.6;
                            color: #333333;
                            padding: 40px;
                        }}
                        h1, h2, h3, h4 {{
                            color: #111111;
                        }}
                        table {{
                            border-collapse: collapse;
                            width: 100%;
                            margin-bottom: 1em;
                        }}
                        table, th, td {{
                            border: 1px solid #cccccc;
                            padding: 8px;
                        }}
                        th {{
                            background-color: #f2f4f7;
                        }}
                        code {{
                            background: #f1f5f9;
                            padding: 2px 6px;
                            border-radius: 4px;
                            font-family: Consolas, monospace;
                        }}
                        pre {{
                            background: #1e293b;
                            color: #f8fafc;
                            padding: 14px;
                            border-radius: 6px;
                            overflow-x: auto;
                        }}
                        pre code {{
                            background: transparent;
                            padding: 0;
                        }}
                        img {{
                            max-width: 100%;
                            height: auto;
                        }}
                    </style>
                </head>
                <body>
                    {bodyHtml}
                </body>
                </html>";
        }
    }
}
