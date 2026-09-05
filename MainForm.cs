using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PuppeteerSharp;
using MarkToPdf.Services;

namespace MarkToPdf
{
    public class MainForm : Form
    {
        private TextBox txtFilePath;
        private Button btnBrowse;
        private Button btnConvert;
        private Label lblStatus;
        private string selectedFilePath = string.Empty;

        private readonly ConverterFactory _converterFactory;

        public MainForm()
        {
            _converterFactory = new ConverterFactory();

            this.Text = "MarkToPdf - Doküman Dönüştürücü";
            this.Size = new Size(520, 230);
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

            lblStatus = new Label()
            {
                Text = "Hazır",
                Left = 20,
                Top = 135,
                Width = 460
            };

            this.Controls.Add(txtFilePath);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(btnConvert);
            this.Controls.Add(lblStatus);

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
        string droppedFile = files[0];
        string extension = Path.GetExtension(droppedFile).ToLowerInvariant();

        string[] supportedExtensions = { ".md", ".txt", ".png", ".jpg", ".jpeg", ".docx", ".html", ".htm", ".xlsx" };

        if (supportedExtensions.Contains(extension))
        {
            selectedFilePath = droppedFile;
            txtFilePath.Text = droppedFile;
            lblStatus.Text = $"Dosya seçildi: {Path.GetFileName(droppedFile)}";
        }
        else
        {
            MessageBox.Show($"Desteklenmeyen dosya türü: {extension}\nLütfen desteklenen bir döküman veya görsel bırakın.",
                            "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Tüm Desteklenen Dosyalar (*.md;*.txt;*.png;*.jpg;*.jpeg;*.docx;*.html;*.htm;*.xlsx)|*.md;*.txt;*.png;*.jpg;*.jpeg;*.docx;*.html;*.htm;*.xlsx|Excel Belgeleri (*.xlsx)|*.xlsx|Word Belgeleri (*.docx)|*.docx|HTML Dosyaları (*.html;*.htm)|*.html;*.htm|Görseller (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Markdown (*.md)|*.md|Düz Metin (*.txt)|*.txt|Tüm Dosyalar (*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = ofd.FileName;
                    txtFilePath.Text = ofd.FileName;
                    lblStatus.Text = $"Dosya seçildi: {Path.GetFileName(ofd.FileName)}";
                }
            }
        }

        private async void BtnConvert_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                MessageBox.Show("Lütfen geçerli bir dosya seçin veya sürükleyip bırakın!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnConvert.Enabled = false;
                btnBrowse.Enabled = false;
                lblStatus.Text = "PDF oluşturuluyor, lütfen bekleyin...";

                var converter = _converterFactory.GetDocumentConverter(selectedFilePath);
                string htmlContent = converter.ConvertToHtml(selectedFilePath);

                string outputPdfPath = Path.ChangeExtension(selectedFilePath, ".pdf");

                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                await using var page = await browser.NewPageAsync();
                await page.SetContentAsync(htmlContent);
                await page.PdfAsync(outputPdfPath);

                lblStatus.Text = "Dönüştürme tamamlandı!";
                MessageBox.Show($"PDF başarıyla oluşturuldu:\n{outputPdfPath}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Hata oluştu!";
                MessageBox.Show($"Dönüştürme sırasında hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnConvert.Enabled = true;
                btnBrowse.Enabled = true;
            }
        }
    }
}