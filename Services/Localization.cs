using System.Collections.Generic;

namespace MarkToPdf.Services
{
    public enum AppLanguage
    {
        Turkish,
        English
    }

    // Basit bir çeviri sözlüğü. Uygulama genelinde tek bir "aktif dil" tutulur;
    // dil değiştiğinde MainForm.ApplyLanguage() tüm arayüz metinlerini yeniden çeker.
    public static class Localization
    {
        public static AppLanguage Current { get; set; } = AppLanguage.Turkish;

        private static readonly Dictionary<string, (string Tr, string En)> _strings = new()
        {
            ["DropText"] = ("Dosyaları buraya sürükleyin veya [Dosya Seç]'e tıklayın", "Drag and drop files here or click [Choose File]"),
            ["Uploads"] = ("Yüklenenler", "Uploads"),
            ["ColFileName"] = ("Dosya Adı", "File Name"),
            ["ColSize"] = ("Boyut", "Size"),
            ["ColStatus"] = ("Durum", "Status"),
            ["ColActions"] = ("İşlemler", "Actions"),
            ["Clear"] = ("TEMİZLE", "CLEAR"),
            ["ConversionOptions"] = ("DÖNÜŞTÜRME SEÇENEKLERİ", "CONVERSION OPTIONS"),
            ["SelectOutputFormat"] = ("Çıktı Formatı Seçin", "Select Output Format"),
            ["PdfDocument"] = ("PDF Belgesi (*.pdf)", "PDF Document (*.pdf)"),
            ["StartConversion"] = ("DÖNÜŞTÜRMEYİ BAŞLAT", "START CONVERSION"),
            ["Help"] = ("Yardım", "Help"),
            ["AboutUs"] = ("Hakkımızda", "About Us"),
            ["LangTurkish"] = ("Türkçe", "Turkish"),
            ["LangEnglish"] = ("İngilizce", "English"),

            ["StatusIdle"] = ("Dönüştürülecek dosyaları sürükleyin veya seçin...", "Drag or select files to convert..."),
            ["StatusReady"] = ("Hazır", "Ready"),
            ["StatusWaiting"] = ("Bekliyor", "Waiting"),
            ["StatusConverting"] = ("Dönüştürülüyor...", "Converting..."),
            ["StatusCompleted"] = ("Tamamlandı ✔", "Completed ✔"),
            ["StatusError"] = ("Hata ✖", "Error ✖"),
            ["StatusFilesReady"] = ("{0} dosya dönüştürülmeye hazır.", "{0} file(s) ready to convert."),
            ["StatusCleared"] = ("Tüm seçimler temizlendi.", "All selections cleared."),
            ["StatusPreparingEngine"] = ("Tarayıcı motoru hazırlanıyor...", "Preparing browser engine..."),
            ["StatusConvertingFile"] = ("Dönüştürülüyor ({0}/{1}): {2}", "Converting ({0}/{1}): {2}"),
            ["StatusDone"] = ("Bitti! (Başarılı: {0}, Hata: {1})", "Done! (Success: {0}, Failed: {1})"),
            ["StatusGeneralError"] = ("Genel hata oluştu!", "An unexpected error occurred!"),

            ["WarnNoFileTitle"] = ("Uyarı", "Warning"),
            ["WarnNoFileMsg"] = ("Lütfen dönüştürülecek en az bir dosya seçin veya sürükleyin!", "Please select or drag at least one file to convert!"),
            ["ResultTitle"] = ("Sonuç", "Result"),
            ["ResultMsg"] = ("Dönüştürme işlemi tamamlandı!\n\nBaşarılı: {0}\nHatalı: {1}", "Conversion completed!\n\nSuccess: {0}\nFailed: {1}"),
            ["ErrorTitle"] = ("Hata", "Error"),
            ["ErrorMsg"] = ("Beklenmedik bir hata oluştu:\n{0}", "An unexpected error occurred:\n{0}"),

            ["FileDialogSupported"] = ("Tüm Desteklenen Dosyalar", "All Supported Files"),
            ["FileDialogAll"] = ("Tüm Dosyalar", "All Files"),

            ["HelpTitle"] = ("Yardım", "Help"),
            ["HelpBody"] = (
                "MarkToPdf Nasıl Kullanılır?\n\n" +
                "1) Dönüştürmek istediğiniz dosyaları sürükleyip bırakın ya da 'Dosya Seç' ile yükleyin.\n" +
                "   Desteklenen formatlar: .md, .txt, .docx, .xlsx, .html/.htm, .png, .jpg/.jpeg\n\n" +
                "2) Sağ panelden çıktı formatını kontrol edin (şu an sadece PDF).\n\n" +
                "3) 'DÖNÜŞTÜRMEYİ BAŞLAT' butonuna basın. Her dosya, bulunduğu klasöre aynı isimle .pdf olarak kaydedilir.\n\n" +
                "4) Listeyi temizlemek için 'TEMİZLE' butonunu kullanabilirsiniz.\n\n" +
                "Dil değiştirmek için sağ üstteki ayarlar simgesine tıklayın.",
                "How to use MarkToPdf?\n\n" +
                "1) Drag and drop the files you want to convert, or use 'Choose File' to add them.\n" +
                "   Supported formats: .md, .txt, .docx, .xlsx, .html/.htm, .png, .jpg/.jpeg\n\n" +
                "2) Check the output format on the right panel (currently PDF only).\n\n" +
                "3) Click 'START CONVERSION'. Each file is saved as a .pdf with the same name, in the same folder.\n\n" +
                "4) Use 'CLEAR' to reset the list.\n\n" +
                "To change the language, click the settings icon in the top right."
            ),
            ["AboutTitle"] = ("Hakkımızda", "About Us"),
            ["AboutBody"] = (
                "2 PDF Converter (MarkToPdf)\n\n" +
                "Markdown, metin, Word, Excel, HTML ve görsel dosyalarını hızlıca PDF'e dönüştürmek için " +
                "geliştirilmiş bir masaüstü uygulamasıdır.\n\n" +
                "Kullanılan teknolojiler: .NET 8 / Windows Forms, PuppeteerSharp (Chromium tabanlı PDF motoru), " +
                "Markdig, Mammoth, ClosedXML.",
                "2 PDF Converter (MarkToPdf)\n\n" +
                "A desktop application built to quickly convert Markdown, text, Word, Excel, HTML and image files " +
                "into PDF.\n\n" +
                "Technologies used: .NET 8 / Windows Forms, PuppeteerSharp (Chromium-based PDF engine), " +
                "Markdig, Mammoth, ClosedXML."
            ),
        };

        public static string T(string key)
        {
            return _strings.TryGetValue(key, out var pair)
                ? (Current == AppLanguage.Turkish ? pair.Tr : pair.En)
                : key;
        }

        public static string T(string key, params object[] args)
        {
            return string.Format(T(key), args);
        }
    }
}