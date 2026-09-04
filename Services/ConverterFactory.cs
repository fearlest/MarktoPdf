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
                new ImageConverter()
            
            };
            
        }
         
            public IDocumentConverter GetDocumentConverter (string filepath)
        {
            string extension = Path.GetExtension (filepath).ToLower ();
            foreach (var converter in _converters)
            {
                if (converter.SupportedExtension.Equals(extension, StringComparison.OrdinalIgnoreCase))
                return converter;
            }
            throw new NotSupportedException($"Desteklenmeyen dosya türü: {extension}");
        }
        
    }
}
