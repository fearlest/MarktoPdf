using MarktoPdf.Services;
using System;


if (args.Length < 2 )
{
    Console.WriteLine("kullanım: dotnet run <girdi-dosyasi.md> <cıktı-dosyasi.pdf>"); 
    return;
}

if (!File.Exists(args[0]))
{
    Console.WriteLine($"Girdi dosyası bulunamadı: {args[0]}");
    return;
}

string inputFilePath = args[0];
string outputFilePath = args[1];

string markdownContent = await File.ReadAllTextAsync(inputFilePath);

var markdownService = new MarkdownService();
string htmlbody = markdownService.ConvertMarkdownToHtml(markdownContent);

var pdfService = new PdfService();
await pdfService.GeneratePdfAsync(htmlbody, outputFilePath);

Console.WriteLine($"PDF başarıyla oluşturuldu: {outputFilePath}");

try
{
    // PDF oluşturma işlemi burada yapılır
}
catch (Exception ex)
{
    Console.WriteLine($"PDF oluşturulurken bir hata oluştu: {ex.Message}");
}