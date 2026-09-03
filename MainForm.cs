using System;
using System.IO;
using System.Windows.Forms;
using Markdig;
using MarkToPdf.Services;

namespace MarkToPdf
{
    public class MainForm : Form
    {
        private TextBox txtFilePath;
        private Button btnBrowse;
        private Button btnConvert;
        private Label lblStatus;

        public MainForm()
        {
            this.Text = "MarkToPdf - Doküman Dönüştürücü";
            this.Width = 520;
            this.Height = 220;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            txtFilePath = new TextBox()
            {
                Left = 20,
                Top = 30,
                Width = 340,
                ReadOnly = true,
                PlaceholderText = "Lütfen dönüştürülecek .md dosyasını seçin..."
            };

            btnBrowse = new Button()
            {
                Text = "Gözat...",
                Left = 370,
                Top = 28,
                Width = 110,
                Height = 26
            };
            btnBrowse.Click += BtnBrowse_Click;

            btnConvert = new Button()
            {
                Text = "PDF'e Dönüştür",
                Left = 20,
                Top = 80,
                Width = 460,
                Height = 38
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
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Desteklenen Belgeler (*.md;*.txt)|*.md;*.txt|Markdown (*.md)|*.md|Düz Metin (*.txt)|*.txt|Tüm Dosyalar (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = ofd.FileName;
                    lblStatus.Text = "Dosya seçildi. Dönüştürmeye hazır.";
                }
            }
        }

        private async void BtnConvert_Click(object? sender, EventArgs e)
        {
            string inputPath = txtFilePath.Text;

            if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
            {
                MessageBox.Show("Lütfen geçerli bir dosya seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnConvert.Enabled = false;
            btnBrowse.Enabled = false;
            lblStatus.Text = "PDF oluşturuluyor, lütfen bekleyin...";

            try
            {
               string outputPath = Path.ChangeExtension(inputPath, ".pdf");

                
                var factory = new ConverterFactory();
                var converter = factory.GetDocumentConverter(inputPath);

                
                string htmlBody = converter.ConvertToHtml(inputPath);

                
                var pdfService = new PdfService();
                await pdfService.GeneratePdfAsync(htmlBody, outputPath);

                lblStatus.Text = $"Başarılı! Kaydedildi: {Path.GetFileName(outputPath)}";
                MessageBox.Show($"PDF başarıyla oluşturuldu:\n{outputPath}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Hata oluştu!";
                MessageBox.Show($"Dönüştürme sırasında hata:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnConvert.Enabled = true;
                btnBrowse.Enabled = true;
            }
        }
    }

    // MarkdownService'i doğrudan burada tanımlıyoruz:
    public class MarkdownService
    {
        public string ConvertMarkdownToHtml(string markdownContent)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            return Markdown.ToHtml(markdownContent, pipeline);
        }
    }
}

