using System;
using System.IO;

namespace MarkToPdf.Services
{
    public class ImageConverter : IDocumentConverter
    {
public string SupportedExtension => ".png";

        public string ConvertToHtml(string filepath)
        {
             byte[] imageBytes = File.ReadAllBytes(filepath);
            string base64String = Convert.ToBase64String(imageBytes);
             
             string extension = Path.GetExtension(filepath).ToLower().TrimStart('.');
             string mimeType = extension  == "jpg" ? "jpeg" : extension;

             return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        @page {{
                            margin: 0;
                            size: auto;
                        }}
                        body {{
                            margin: 0;
                            padding: 20px;
                            display: flex;
                            justify-content: center;
                            align-items: center;
                            min-height: 100vh;
                            box-sizing: border-box;
                            background-color: #ffffff;
                        }}
                        img {{
                            max-width: 100%;
                            max-height: 100%;
                            object-fit: contain;
                        }}
                    </style>
                </head>
                <body>
                    <img src='data:image/{mimeType};base64,{base64String}' />
                </body>
                </html>";
        }
    }
}