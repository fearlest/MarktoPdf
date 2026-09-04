using System;
using System.Collections.Generic;
using System.IO;

namespace MarkToPdf.Services
{
    public class ConverterFactory
    {
        private readonly List<IDocumentConverter> _converters;
        
        public ConverterFactory()
        {
            _converters = new List<IDocumentConverter>
            {
                new MarkdownConverter(),
                new TextConverter(),    
                new ImageConverter(),
                new DocxConverter(),
                new HtmlConverter(),
                new ExcelConverter()
            
            };
            
        }
         
            public IDocumentConverter GetDocumentConverter (string filepath)
        {
            string extension = Path.GetExtension (filepath).ToLower ();
            
            if (extension == ".xlsx")
            {
                return new ExcelConverter();
            }


             if (extension == ".html" || extension == ".htm")
            {
                return new HtmlConverter();
                }


            if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                {
                    return new ImageConverter();
                }

              foreach (var converter in _converters)
            {
                if (converter.SupportedExtension.Equals(extension, StringComparison.OrdinalIgnoreCase))
                return converter;
            }

            throw new NotSupportedException($"Desteklenmeyen dosya türü: {extension}");
        }
        
    }
}
