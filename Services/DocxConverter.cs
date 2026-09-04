using System.IO;
using  Mammoth;

namespace MarkToPdf.Services
{
    public class DocxConverter : IDocumentConverter
    {
        public string SupportedExtension => ".docx";
        
        public string ConvertToHtml(string filepath)
        {
            var converter = new DocumentConverter();
            var result = converter.ConvertToHtml(filepath);
            string htmlContent = result.Value;

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
                        img {{
                            max-width: 100%;
                            height: auto;
                        }}
                    </style>
                </head>
                <body>
                    {htmlContent}
                </body>
                </html>";
        }
    }
}