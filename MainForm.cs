using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PuppeteerSharp;
using MarkToPdf.Services;
using DocumentFormat.OpenXml.Office.PowerPoint.Y2021.M06.Main;

namespace MarkToPdf
{
    public class MainForm : Form
    {
        private ProgressBar progressBar;
        private TextBox txtFilePath;
        private Button btnBrowse;
        private Button btnConvert;
        private Label lblStatus;
        private List<string> selectedFilePaths = new List<string>();
        private readonly ConverterFactory _converterFactory;

        public MainForm()
        {
            _converterFactory = new ConverterFactory();

            this.Text = "MarkToPdf - Doküman Dönüştürücü";
            this.Size = new Size(520, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            txtFilePath = new TextBox()
            {
                Left = 20,
                Top = 30,
                Width = 350,
                ReadOnly = true
            };

            btnBrowse = new Button()
            {
                Text = "Gözat...",
                Left = 380,
                Top = 28,
                Width = 100,
                Height = 26
            };
            btnBrowse.Click += BtnBrowse_Click;

            btnConvert = new Button()
            {
                Text = "PDF'e Dönüştür",
                Left = 20,
                Top = 75,
                Width = 460,
                Height = 38,
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };
            btnConvert.Click += BtnConvert_Click;
            progressBar = new ProgressBar()
            {
                Left = 20,
                Top = 125,
                Width = 460,
                Height = 16,
                Visible = false
            };

            lblStatus = new Label()
            {
                Text = "Hazır",
                Left = 20,
                Top = 155,
                Width = 460
            };

            this.Controls.Add(txtFilePath);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(btnConvert);
            this.Controls.Add(lblStatus);
            this.Controls.Add(progressBar);

            // Sürükle - Bırak (Drag & Drop)
            this.AllowDrop = true;
            this.DragEnter += MainForm_DragEnter;
            this.DragDrop += MainForm_DragDrop;
        }

        private void MainForm_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

     private void MainForm_DragDrop(object? sender, DragEventArgs e)
{
    if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
    {
        string[] supportedExtensions = { ".md", ".txt", ".png", ".jpg", ".jpeg", ".docx", ".html", ".htm", ".xlsx" };

        // Bırakılan dosyalardan sadece desteklenenleri filtrele
        var validFiles = files
            .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .ToList();

        if (validFiles.Count > 0)
        {
            selectedFilePaths = validFiles;
            UpdateSelectionDisplay();
        }
        else
        {
            MessageBox.Show("Bırakılan dosyalar arasında desteklenen bir format bulunamadı.",
                            "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}


         private void UpdateSelectionDisplay()
        {
            if(selectedFilePaths.Count == 1)
            {
                txtFilePath.Text = selectedFilePaths[0];
                lblStatus.Text = $"Seçildi: {Path.GetFileName(selectedFilePaths[0])}";
            }
            else
            {
                txtFilePath.Text = $"{selectedFilePaths.Count} dosya seçildi";
                lblStatus.Text = $"{selectedFilePaths.Count} adet dosya dönüştürülmeye hazır.";
            }
            progressBar.Visible = false;
            progressBar.Value = 0;



        }
        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = true;
                ofd.Filter = "Tüm Desteklenen Dosyalar (*.md;*.txt;*.png;*.jpg;*.jpeg;*.docx;*.html;*.htm;*.xlsx)|*.md;*.txt;*.png;*.jpg;*.jpeg;*.docx;*.html;*.htm;*.xlsx|Excel Belgeleri (*.xlsx)|*.xlsx|Word Belgeleri (*.docx)|*.docx|HTML Dosyaları (*.html;*.htm)|*.html;*.htm|Görseller (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Markdown (*.md)|*.md|Düz Metin (*.txt)|*.txt|Tüm Dosyalar (*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePaths = ofd.FileNames.ToList();
                    UpdateSelectionDisplay();
                }
            }
        }

        private async void BtnConvert_Click(object? sender, EventArgs e)
{
    if (selectedFilePaths.Count == 0)
    {
        MessageBox.Show("Lütfen dönüştürülecek en az bir dosya seçin veya sürükleyip bırakın!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    try
    {
        btnConvert.Enabled = false;
        btnBrowse.Enabled = false;

        // ProgressBar'ı adım adım moda alıyoruz
        progressBar.Visible = true;
        progressBar.Style = ProgressBarStyle.Blocks;
        progressBar.Minimum = 0;
        progressBar.Maximum = selectedFilePaths.Count;
        progressBar.Value = 0;

        lblStatus.Text = "Tarayıcı motoru hazırlanıyor...";

        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();

        // Chromium'u sadece 1 defa başlatıyoruz
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });

        int successCount = 0;
        int failCount = 0;

        for (int i = 0; i < selectedFilePaths.Count; i++)
        {
            string filePath = selectedFilePaths[i];
            string fileName = Path.GetFileName(filePath);

            lblStatus.Text = $"Dönüştürülüyor ({i + 1}/{selectedFilePaths.Count}): {fileName}";

            try
            {
                var converter = _converterFactory.GetDocumentConverter(filePath);
                string htmlContent = converter.ConvertToHtml(filePath);

                string outputPdfPath = Path.ChangeExtension(filePath, ".pdf");

                await using var page = await browser.NewPageAsync();
                await page.SetContentAsync(htmlContent);
                await page.PdfAsync(outputPdfPath);

                successCount++;
            }
            catch
            {
                failCount++;
            }

            // Her dosya bittiğinde çubuğu bir adım doldur
            progressBar.Value = i + 1;
        }

        lblStatus.Text = $"Tamamlandı! (Başarılı: {successCount}, Hata: {failCount})";
        MessageBox.Show($"Dönüştürme işlemi tamamlandı!\n\nBaşarılı: {successCount}\nHatalı: {failCount}", 
                        "Sonuç", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    catch (Exception ex)
    {
        lblStatus.Text = "Genel hata oluştu!";
        MessageBox.Show($"Beklenmedik bir hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    finally
    {
        btnConvert.Enabled = true;
        btnBrowse.Enabled = true;
    }
}

}

}
