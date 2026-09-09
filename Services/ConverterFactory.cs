using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public IDocumentConverter GetDocumentConverter(string filepath)
        {
            string extension = Path.GetExtension(filepath).ToLowerInvariant();

            var converter = _converters.FirstOrDefault(c =>
                c.SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase));

            if (converter == null)
            {
                throw new NotSupportedException($"Desteklenmeyen dosya türü: {extension}");
            }

            return converter;
        }
    }
}
