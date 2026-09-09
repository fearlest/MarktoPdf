using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using MarkToPdf.Services;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using Point = System.Drawing.Point;

namespace MarkToPdf
{
    // Titremeyi (Flicker) ve arka plan silme flaşını kökten engelleyen çift tamponlu panel
    public class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
            this.UpdateStyles();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Windows'un ara silme flaşını (WM_ERASEBKGND) iptal ediyoruz
        }
    }

    public class MainForm : Form
    {
        [DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse
        );

        // Canlı Animasyon Değişkenleri
        private System.Windows.Forms.Timer animTimer = null!;
        private float gradientAngle = 0f;

        // Arayüz Bileşenleri
        private Panel pnlMainCard = null!;
        private Panel pnlWhiteCard = null!;
        private Panel pnlDrop = null!;
        private ListView lvUploads = null!;
        private Panel pnlOptions = null!;
        private BufferedPanel btnConvert = null!;
        private BufferedPanel btnClear = null!;
        private BufferedPanel pnlCapsule = null!;
        private ComboBox cmbFormat = null!;
        private ProgressBar progressBar = null!;
        private Label lblStatus = null!;

        // Dile göre yeniden çizilmesi gereken kontroller (dil değişince metinleri güncellenir)
        private Label lblDropText = null!;
        private Label lblUploads = null!;
        private Label lblOptTitle = null!;
        private Label lblSelFormat = null!;
        private Label lblHelp = null!;
        private Label lblAboutUs = null!;
        private ColumnHeader colFileName = null!;
        private ColumnHeader colSize = null!;
        private ColumnHeader colStatus = null!;
        private ColumnHeader colActions = null!;
        private ToolStripMenuItem menuItemTurkish = null!;
        private ToolStripMenuItem menuItemEnglish = null!;

        // Hover Durumları
        private bool isConvertHovered = false;
        private bool isClearHovered = false;

        // Veri ve Dönüştürücü Motoru
        private List<string> selectedFilePaths = new List<string>();
        private readonly ConverterFactory _converterFactory;

        public MainForm()
        {
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true
            );
            this.DoubleBuffered = true;

            _converterFactory = new ConverterFactory();

            InitializeForm();
            SetupCustomUI();
            SetupAnimationTimer();
            ApplyLanguage();
        }

        private string? FindAsset(string fileName)
        {
            string[] paths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "Assets", fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Assets", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), fileName)
            };

            return paths.FirstOrDefault(File.Exists);
        }

        private void InitializeForm()
        {
            this.Text = "2 PDF CONVERTER";
            this.Size = new Size(1020, 720);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            string? iconPath = FindAsset("app.ico") ?? (File.Exists("app.ico") ? "app.ico" : null);
            if (iconPath != null)
            {
                this.Icon = new Icon(iconPath);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new LinearGradientBrush(this.ClientRectangle, Color.Black, Color.White, gradientAngle))
            {
                ColorBlend cb = new ColorBlend(4)
                {
                    Positions = new float[] { 0f, 0.35f, 0.70f, 1f },
                    Colors = new Color[] {
                        Color.FromArgb(255, 82, 85),   // Canlı Mercan Kırmızı
                        Color.FromArgb(255, 140, 110), // Sıcak Şeftali
                        Color.FromArgb(175, 168, 245), // Yumuşak Lavanta
                        Color.FromArgb(115, 178, 255)  // Ferah Gök Mavisi
                    }
                };
                brush.InterpolationColors = cb;
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
            base.OnPaint(e);
        }

        private void SetupAnimationTimer()
        {
            animTimer = new System.Windows.Forms.Timer() { Interval = 33 }; // 30 FPS akıcılık
            animTimer.Tick += (s, e) =>
            {
                gradientAngle = (gradientAngle + 0.8f) % 360f;

                // Sadece animasyon içeren bölgeleri donanım tamponunda tazele
                this.Invalidate(false);
                pnlCapsule.Invalidate();
            };
            animTimer.Start();

            this.FormClosed += (s, e) => animTimer.Stop();
        }

        // Dil değiştiğinde (veya form ilk açıldığında) tüm arayüz metinlerini
        // aktif dile göre günceller. Renkler/animasyon/layout'a dokunmaz.
        private void ApplyLanguage()
        {
            lblDropText.Text = Localization.T("DropText");
            lblUploads.Text = Localization.T("Uploads");
            lblOptTitle.Text = Localization.T("ConversionOptions");
            lblSelFormat.Text = Localization.T("SelectOutputFormat");
            lblHelp.Text = Localization.T("Help");
            lblAboutUs.Text = Localization.T("AboutUs");

            colFileName.Text = Localization.T("ColFileName");
            colSize.Text = Localization.T("ColSize");
            colStatus.Text = Localization.T("ColStatus");
            colActions.Text = Localization.T("ColActions");

            if (cmbFormat.Items.Count > 0)
            {
                cmbFormat.Items[0] = Localization.T("PdfDocument");
                cmbFormat.SelectedIndex = 0;
            }

            menuItemTurkish.Text = Localization.T("LangTurkish");
            menuItemEnglish.Text = Localization.T("LangEnglish");
            menuItemTurkish.Checked = Localization.Current == AppLanguage.Turkish;
            menuItemEnglish.Checked = Localization.Current == AppLanguage.English;

            // Henüz dosya seçilmemişse boş durum metnini de güncelle
            if (selectedFilePaths.Count == 0)
            {
                lblStatus.Text = Localization.T("StatusIdle");
            }

            // CLEAR / START CONVERSION metinleri kendi Paint olaylarında
            // Localization.T() ile canlı okunuyor, sadece yeniden çizilmeleri yeterli.
            btnClear.Invalidate();
            btnConvert.Invalidate();
        }

        private void SetLanguage(AppLanguage language)
        {
            Localization.Current = language;
            ApplyLanguage();
        }

        private void SetupCustomUI()
        {
            int mainX = (this.ClientSize.Width - 930) / 2;
            int mainY = (this.ClientSize.Height - 620) / 2;

            // 1. DIŞTAKİ BÜYÜK AÇIK GRİ KART
            pnlMainCard = new Panel()
            {
                Size = new Size(930, 620),
                Location = new Point(mainX, mainY),
                BackColor = Color.FromArgb(233, 236, 241)
            };
            pnlMainCard.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlMainCard.Width, pnlMainCard.Height, 32, 32));
            this.Controls.Add(pnlMainCard);

            // 2. BAŞLIK ("2 PDF CONVERTER") & SAĞ ÜST AYARLAR (ÇARK)
            Label lblTitle = new Label()
            {
                Text = "2 PDF CONVERTER",
                Font = new Font("Segoe UI", 21f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                BackColor = Color.Transparent,
                Location = new Point(35, 18),
                AutoSize = true
            };
            pnlMainCard.Controls.Add(lblTitle);

            // Ayarlar (dil seçimi) menüsü — her zaman erişilebilir olsun diye
            // cark.png bulunamasa bile bir "⚙" etiketiyle her zaman gösteriliyor.
            var langMenu = new ContextMenuStrip();
            menuItemTurkish = new ToolStripMenuItem("Türkçe");
            menuItemEnglish = new ToolStripMenuItem("English");
            menuItemTurkish.Click += (s, e) => SetLanguage(AppLanguage.Turkish);
            menuItemEnglish.Click += (s, e) => SetLanguage(AppLanguage.English);
            langMenu.Items.Add(menuItemTurkish);
            langMenu.Items.Add(menuItemEnglish);

            Control settingsControl;
            string? carkPath = FindAsset("cark.png");
            if (carkPath != null)
            {
                settingsControl = new PictureBox()
                {
                    Image = Image.FromFile(carkPath),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(28, 28),
                    Location = new Point(865, 24),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };
            }
            else
            {
                settingsControl = new Label()
                {
                    Text = "\u2699", // ⚙ gear glyph
                    Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(51, 65, 85),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(28, 28),
                    Location = new Point(865, 22),
                    Cursor = Cursors.Hand
                };
            }
            settingsControl.Click += (s, e) => langMenu.Show(settingsControl, new Point(0, settingsControl.Height));
            pnlMainCard.Controls.Add(settingsControl);

            // 3. SOL TARAF: TEK PARÇA BEYAZ KART (Dropzone + Uploads + Clear)
            pnlWhiteCard = new Panel()
            {
                Location = new Point(35, 75),
                Size = new Size(590, 485),
                BackColor = Color.White
            };
            pnlWhiteCard.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlWhiteCard.Width, pnlWhiteCard.Height, 26, 26));
            pnlMainCard.Controls.Add(pnlWhiteCard);

            // A) KESİK ÇİZGİLİ SÜRÜKLE-BIRAK ALANI (bulut.png)
            pnlDrop = new Panel()
            {
                Location = new Point(18, 18),
                Size = new Size(554, 160),
                BackColor = Color.White,
                AllowDrop = true
            };

            pnlDrop.Paint += (s, e) =>
            {
                using Pen pen = new Pen(Color.FromArgb(156, 163, 175), 1.5f);
                pen.DashStyle = DashStyle.Dash;
                using GraphicsPath path = GetRoundedRectanglePath(new Rectangle(2, 2, pnlDrop.Width - 5, pnlDrop.Height - 5), 16);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(pen, path);
            };

            string? bulutPath = FindAsset("bulut.png");
            if (bulutPath != null)
            {
                PictureBox picCloud = new PictureBox()
                {
                    Image = Image.FromFile(bulutPath),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(115, 70),
                    Location = new Point((pnlDrop.Width - 115) / 2, 18),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };
                picCloud.Click += (s, e) => SelectFiles();
                pnlDrop.Controls.Add(picCloud);
            }

            lblDropText = new Label()
            {
                Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(500, 25),
                Location = new Point((pnlDrop.Width - 500) / 2, 105),
                Cursor = Cursors.Hand
            };
            lblDropText.Click += (s, e) => SelectFiles();
            pnlDrop.Click += (s, e) => SelectFiles();

            pnlDrop.DragEnter += PnlDrop_DragEnter;
            pnlDrop.DragDrop += PnlDrop_DragDrop;

            pnlDrop.Controls.Add(lblDropText);
            pnlWhiteCard.Controls.Add(pnlDrop);

            // B) UPLOADS BAŞLIĞI
            lblUploads = new Label()
            {
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                BackColor = Color.Transparent,
                Location = new Point(22, 190),
                AutoSize = true
            };
            pnlWhiteCard.Controls.Add(lblUploads);

            // C) UPLOADS TABLOSU
            lvUploads = new ListView()
            {
                Location = new Point(18, 220),
                Size = new Size(554, 200),
                View = View.Details,
                FullRowSelect = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5f)
            };
            colFileName = lvUploads.Columns.Add("File Name", 250);
            colSize = lvUploads.Columns.Add("Size", 90);
            colStatus = lvUploads.Columns.Add("Status", 124);
            colActions = lvUploads.Columns.Add("Actions", 90);
            pnlWhiteCard.Controls.Add(lvUploads);

            // D) BEYAZ KARTIN SAĞ ALTINDAKİ "CLEAR" BUTONU (BufferedPanel)
            btnClear = new BufferedPanel()
            {
                Location = new Point(472, 432),
                Size = new Size(100, 36),
                Cursor = Cursors.Hand,
                BackColor = Color.White
            };
            btnClear.MouseEnter += (s, e) => { isClearHovered = true; btnClear.Invalidate(); };
            btnClear.MouseLeave += (s, e) => { isClearHovered = false; btnClear.Invalidate(); };
            btnClear.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.Clear(btnClear.BackColor);

                Color c1 = isClearHovered ? Color.FromArgb(255, 110, 115) : Color.FromArgb(255, 90, 95);
                Color c2 = isClearHovered ? Color.FromArgb(255, 195, 205) : Color.FromArgb(255, 175, 185);

                using LinearGradientBrush brush = new LinearGradientBrush(btnClear.ClientRectangle, c1, c2, LinearGradientMode.Horizontal);
                using GraphicsPath path = GetRoundedRectanglePath(btnClear.ClientRectangle, 16);
                e.Graphics.FillPath(brush, path);

                TextRenderer.DrawText(
                    e.Graphics,
                    Localization.T("Clear"),
                    new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    btnClear.ClientRectangle,
                    Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            };
            btnClear.Click += (s, e) =>
            {
                selectedFilePaths.Clear();
                lvUploads.Items.Clear();
                lblStatus.Text = Localization.T("StatusCleared");
            };
            pnlWhiteCard.Controls.Add(btnClear);

            // 4. SAĞ PANEL: CONVERSION OPTIONS
            pnlOptions = new Panel()
            {
                Location = new Point(645, 75),
                Size = new Size(250, 290),
                BackColor = Color.FromArgb(209, 213, 219)
            };
            pnlOptions.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlOptions.Width, pnlOptions.Height, 24, 24));

            lblOptTitle = new Label()
            {
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                BackColor = Color.Transparent,
                Location = new Point(18, 18),
                AutoSize = true
            };

            lblSelFormat = new Label()
            {
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(75, 85, 99),
                BackColor = Color.Transparent,
                Location = new Point(18, 48),
                AutoSize = true
            };

            Panel pnlComboHost = new Panel()
            {
                Location = new Point(18, 72),
                Size = new Size(214, 38),
                BackColor = Color.White
            };
            pnlComboHost.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlComboHost.Width, pnlComboHost.Height, 14, 14));

            cmbFormat = new ComboBox()
            {
                Location = new Point(8, 6),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 41, 59),
                Font = new Font("Segoe UI Semibold", 9.5f)
            };
            cmbFormat.Items.Add("PDF Document (*.pdf)");
            cmbFormat.SelectedIndex = 0;
            pnlComboHost.Controls.Add(cmbFormat);

            // START CONVERSION Butonu (BufferedPanel)
            btnConvert = new BufferedPanel()
            {
                Location = new Point(18, 220),
                Size = new Size(214, 48),
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(209, 213, 219)
            };
            btnConvert.MouseEnter += (s, e) => { isConvertHovered = true; btnConvert.Invalidate(); };
            btnConvert.MouseLeave += (s, e) => { isConvertHovered = false; btnConvert.Invalidate(); };
            btnConvert.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.Clear(btnConvert.BackColor);

                Color c1 = isConvertHovered ? Color.FromArgb(255, 100, 105) : Color.FromArgb(255, 80, 85);
                Color c2 = isConvertHovered ? Color.FromArgb(255, 195, 205) : Color.FromArgb(255, 175, 185);

                using LinearGradientBrush brush = new LinearGradientBrush(btnConvert.ClientRectangle, c1, c2, LinearGradientMode.Horizontal);
                using GraphicsPath path = GetRoundedRectanglePath(btnConvert.ClientRectangle, 22);
                e.Graphics.FillPath(brush, path);

                TextRenderer.DrawText(
                    e.Graphics,
                    Localization.T("StartConversion"),
                    new Font("Segoe UI", 10.5f, FontStyle.Bold),
                    btnConvert.ClientRectangle,
                    Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            };
            btnConvert.Click += BtnConvert_Click;

            pnlOptions.Controls.Add(lblOptTitle);
            pnlOptions.Controls.Add(lblSelFormat);
            pnlOptions.Controls.Add(pnlComboHost);
            pnlOptions.Controls.Add(btnConvert);
            pnlMainCard.Controls.Add(pnlOptions);

            // 5. SAĞ ALTTAKİ KAPSÜL (BufferedPanel + Anti-Aliased Çizim)
            pnlCapsule = new BufferedPanel()
            {
                Location = new Point(655, 395),
                Size = new Size(230, 130),
                BackColor = Color.FromArgb(233, 236, 241) // Dış kartla birebir aynı arka plan
            };
            pnlCapsule.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.Clear(pnlCapsule.BackColor);

                using LinearGradientBrush brush = new LinearGradientBrush(pnlCapsule.ClientRectangle, Color.Black, Color.White, gradientAngle);
                ColorBlend cb = new ColorBlend(4)
                {
                    Positions = new float[] { 0f, 0.35f, 0.70f, 1f },
                    Colors = new Color[] {
                        Color.FromArgb(255, 82, 85),
                        Color.FromArgb(255, 140, 110),
                        Color.FromArgb(175, 168, 245),
                        Color.FromArgb(115, 178, 255)
                    }
                };
                brush.InterpolationColors = cb;

                // Bölge kırpması yerine doğrudan pürüzsüz antialias çizim (Sıfır titreme)
                using GraphicsPath path = GetRoundedRectanglePath(new Rectangle(0, 0, pnlCapsule.Width - 1, pnlCapsule.Height - 1), 40);
                e.Graphics.FillPath(brush, path);
            };
            pnlMainCard.Controls.Add(pnlCapsule);

            // 6. ALT BİLGİ LİNKLERİ (Help / About Us, artık işlevsel) VE DURUM METNİ
            lblHelp = new Label()
            {
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                BackColor = Color.Transparent,
                Location = new Point(40, 575),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            lblHelp.Click += (s, e) =>
                MessageBox.Show(Localization.T("HelpBody"), Localization.T("HelpTitle"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            pnlMainCard.Controls.Add(lblHelp);

            lblAboutUs = new Label()
            {
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                BackColor = Color.Transparent,
                Location = new Point(120, 575),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            lblAboutUs.Click += (s, e) =>
                MessageBox.Show(Localization.T("AboutBody"), Localization.T("AboutTitle"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            pnlMainCard.Controls.Add(lblAboutUs);

            lblStatus = new Label()
            {
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(100, 116, 139),
                BackColor = Color.Transparent,
                Location = new Point(220, 577),
                Size = new Size(400, 20)
            };
            pnlMainCard.Controls.Add(lblStatus);

            progressBar = new ProgressBar()
            {
                Location = new Point(35, 563),
                Size = new Size(590, 5),
                Visible = false
            };
            pnlMainCard.Controls.Add(progressBar);
        }

        private GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void SelectFiles()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Multiselect = true;
                ofd.Filter =
                    $"{Localization.T("FileDialogSupported")} (*.md;*.txt;*.png;*.jpg;*.jpeg;*.docx;*.html;*.htm;*.xlsx)|*.md;*.txt;*.png;*.jpg;*.jpeg;*.docx;*.html;*.htm;*.xlsx|" +
                    $"{Localization.T("FileDialogAll")} (*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    AddFiles(ofd.FileNames);
                }
            }
        }

        private void AddFiles(IEnumerable<string> filePaths)
        {
            string[] supported = { ".md", ".txt", ".png", ".jpg", ".jpeg", ".docx", ".html", ".htm", ".xlsx" };

            foreach (var path in filePaths)
            {
                if (supported.Contains(Path.GetExtension(path).ToLowerInvariant()) && !selectedFilePaths.Contains(path))
                {
                    selectedFilePaths.Add(path);

                    FileInfo fi = new FileInfo(path);
                    string sizeText = fi.Length > 1024 * 1024
                        ? $"{fi.Length / (1024.0 * 1024.0):F1} MB"
                        : $"{fi.Length / 1024} KB";

                    ListViewItem row = new ListViewItem(fi.Name);
                    row.SubItems.Add(sizeText);
                    row.SubItems.Add(Localization.T("StatusReady"));
                    row.SubItems.Add(Localization.T("StatusWaiting"));
                    row.Tag = path;
                    lvUploads.Items.Add(row);
                }
            }

            lblStatus.Text = Localization.T("StatusFilesReady", selectedFilePaths.Count);
        }

        private void PnlDrop_DragEnter(object? sender, DragEventArgs e)
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

        private void PnlDrop_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                AddFiles(files);
            }
        }

        private async void BtnConvert_Click(object? sender, EventArgs e)
        {
            if (selectedFilePaths.Count == 0)
            {
                MessageBox.Show(Localization.T("WarnNoFileMsg"), Localization.T("WarnNoFileTitle"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnConvert.Enabled = false;

                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Minimum = 0;
                progressBar.Maximum = selectedFilePaths.Count;
                progressBar.Value = 0;

                lblStatus.Text = Localization.T("StatusPreparingEngine");

                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });

                int successCount = 0;
                int failCount = 0;

                for (int i = 0; i < selectedFilePaths.Count; i++)
                {
                    string filePath = selectedFilePaths[i];
                    string fileName = Path.GetFileName(filePath);

                    if (i < lvUploads.Items.Count)
                    {
                        lvUploads.Items[i].SubItems[2].Text = Localization.T("StatusConverting");
                    }

                    lblStatus.Text = Localization.T("StatusConvertingFile", i + 1, selectedFilePaths.Count, fileName);

                    try
                    {
                        var converter = _converterFactory.GetDocumentConverter(filePath);
                        string htmlContent = converter.ConvertToHtml(filePath);

                        string outputPdfPath = Path.ChangeExtension(filePath, ".pdf");

                        await using var page = await browser.NewPageAsync();
                        await page.SetContentAsync(htmlContent);

                        // PrintBackground olmadan Chromium arka plan renklerini/gölgeleri
                        // PDF'e basmıyor (örn. Excel tablo başlıkları, kod blokları kayboluyordu).
                        await page.PdfAsync(outputPdfPath, new PdfOptions
                        {
                            Format = PaperFormat.A4,
                            PrintBackground = true
                        });

                        if (i < lvUploads.Items.Count)
                        {
                            lvUploads.Items[i].SubItems[2].Text = Localization.T("StatusCompleted");
                        }
                        successCount++;
                    }
                    catch
                    {
                        if (i < lvUploads.Items.Count)
                        {
                            lvUploads.Items[i].SubItems[2].Text = Localization.T("StatusError");
                        }
                        failCount++;
                    }

                    progressBar.Value = i + 1;
                }

                lblStatus.Text = Localization.T("StatusDone", successCount, failCount);
                MessageBox.Show(Localization.T("ResultMsg", successCount, failCount),
                    Localization.T("ResultTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = Localization.T("StatusGeneralError");
                MessageBox.Show(Localization.T("ErrorMsg", ex.Message), Localization.T("ErrorTitle"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnConvert.Enabled = true;
            }
        }
    }
}