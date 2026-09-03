using System;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace MarkToPdf.Services;
public class PdfService
{
            public async Task  GeneratePdfAsync(string htmlBody, string outputPath)
            {
                await new BrowserFetcher().DownloadAsync();
                var launchOptions = new LaunchOptions
                {
                    Headless = true
                };

                using (var browser = await Puppeteer.LaunchAsync(launchOptions))
        using (var page = await browser.NewPageAsync())
        {
            string fullHtml = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {{
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                        margin: 40px;
                        line-height: 1.6;
                        color: #1a202c;
                    }}
                    h1, h2, h3 {{
                        color: #0f172a;
                        border-bottom: 1px solid #e2e8f0;
                        padding-bottom: 8px;
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
                    table {{
                        width: 100%;
                        border-collapse: collapse;
                        margin: 20px 0;
                    }}
                    th, td {{
                        border: 1px solid #cbd5e1;
                        padding: 8px 12px;
                        text-align: left;
                    }}
                    th {{
                        background-color: #f8fafc;
                    }}
                </style>
            </head>
            <body>
                {htmlBody}
            </body>
            </html>";

            await page.SetContentAsync(fullHtml);

            await page.PdfAsync(outputPath, new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true
            });
        }
            }
}