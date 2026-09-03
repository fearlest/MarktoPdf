namespace MarkToPdf.Services
{
    public interface IDocumentConverter
    {
        string SupportedExtension { get; }

        string ConvertToHtml(string filePath);
    }
}