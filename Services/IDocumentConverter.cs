namespace MarkToPdf.Services
{
    public interface IDocumentConverter
    {
        // Bir converter birden fazla uzantıyı destekleyebilir (örn. .html ve .htm)
        string[] SupportedExtensions { get; }

        string ConvertToHtml(string filePath);
    }
}
