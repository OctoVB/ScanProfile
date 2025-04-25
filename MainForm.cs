using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using WIA;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace ScanProfile
{
    public partial class MainForm : Form
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Progress<int> _progress;
        private bool _isResizing = false;
        private Size _lastClientSize;
        private bool _isLoadingYears = false;

        public MainForm()
        {
            InitializeComponent();
            ConfigureComponents();
            InitializeProgress();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            Properties.Settings.Default.Save();
            base.OnFormClosing(e);
        }

        private void InitializeProgress()
        {
            _progress = new Progress<int>(value =>
            {
                progressBar1.Value = value;
                progressBar1.Visible = true;
            });
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.DataScanPath))
            {
                txtPath.Text = Properties.Settings.Default.DataScanPath;
                LoadYears();
            }

            LoadComboBoxSettings(cbPepperType, Properties.Settings.Default.PepperType);
            LoadComboBoxSettings(cbDPI, Properties.Settings.Default.DPI);
            LoadScanners();

            var toolTip = new ToolTip();
            toolTip.SetToolTip(btnUpdate, "Обновить список всех доступных сканеров (USB и сетевых)");
        }

        private void LoadComboBoxSettings(ComboBox comboBox, string savedValue)
        {
            int index = comboBox.Items.IndexOf(savedValue);
            comboBox.SelectedIndex = index >= 0 ? index : 0;
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;

            if (comboBox == cbPepperType)
                Properties.Settings.Default.PepperType = comboBox.SelectedItem?.ToString();
            else if (comboBox == cbDPI)
                Properties.Settings.Default.DPI = comboBox.SelectedItem?.ToString();

            Properties.Settings.Default.Save();
        }

        private void ConfigureComponents()
        {
            // TreeYears configuration
            treeYears.ImageList = new ImageList();
            treeYears.ImageList.Images.Add("Year", iconSet.Images["Dot"]);
            treeYears.HideSelection = false;
            treeYears.AfterSelect += TreeYears_AfterSelect;

            // ListLetters configuration
            listLetters.View = View.List;
            listLetters.SmallImageList = new ImageList();
            listLetters.SmallImageList.Images.Add("Letter", iconSet.Images["Dot"]);
            listLetters.MultiSelect = false;
            listLetters.SelectedIndexChanged += ListLetters_SelectedIndexChanged;

            // ListCases configuration
            listCases.View = View.Details;
            listCases.Columns.Add("Дело", 150);
            listCases.Columns.Add("Дата создания", 120);
            listCases.FullRowSelect = true;
            listCases.MultiSelect = false;
            listCases.SelectedIndexChanged += ListCases_SelectedIndexChanged;

            // FlowLayoutPanel configuration
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            this.Resize += MainForm_Resize;

            // ProgressBar configuration
            progressBar1.Visible = false;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (_isResizing || this.ClientSize == _lastClientSize) return;

            _isResizing = true;
            _lastClientSize = this.ClientSize;

            flowLayoutPanel1.SuspendLayout();

            Task.Delay(100).ContinueWith(_ =>
            {
                this.InvokeIfRequired(() =>
                {
                    UpdatePanelSizes();
                    flowLayoutPanel1.ResumeLayout(true);
                    _isResizing = false;
                });
            });
        }

        private void UpdatePanelSizes()
        {
            if (flowLayoutPanel1.Controls.Count == 0) return;

            int availableWidth = flowLayoutPanel1.ClientSize.Width - flowLayoutPanel1.Padding.Horizontal - 20;
            int availableHeight = flowLayoutPanel1.ClientSize.Height - flowLayoutPanel1.Padding.Vertical - 20;

            int panelWidth = Math.Max(400, Math.Min(1600, availableWidth - 20));
            int panelHeight = Math.Min((int)(panelWidth * 1.5), availableHeight - 50);

            foreach (Control control in flowLayoutPanel1.Controls.OfType<Panel>())
            {
                control.Width = panelWidth;
                control.Height = panelHeight;

                int padding = 10;
                int buttonsHeight = 100;
                int textHeight = 60;

                var imagePanel = control.Controls.OfType<Panel>().FirstOrDefault(p => p.Controls.OfType<PictureBox>().Any());
                var textPanel = control.Controls.OfType<Panel>().FirstOrDefault(p => p.Controls.OfType<Label>().Any());
                var buttonsPanel = control.Controls.OfType<Panel>().FirstOrDefault(p => p.Controls.OfType<Button>().Count() >= 2);

                UpdateImagePanel(imagePanel, panelWidth, panelHeight, padding, textHeight, buttonsHeight);
                UpdateTextPanel(textPanel, panelWidth, padding, textHeight, imagePanel);
                UpdateButtonsPanel(buttonsPanel, panelWidth, padding, buttonsHeight, textPanel, imagePanel);
            }

            CenterPanelsInFlowLayout();
        }

        private void UpdateImagePanel(Panel imagePanel, int panelWidth, int panelHeight, int padding, int textHeight, int buttonsHeight)
        {
            if (imagePanel == null) return;

            imagePanel.Width = panelWidth - padding * 2;
            imagePanel.Height = panelHeight - textHeight - buttonsHeight - padding * 3;
            imagePanel.Top = padding;
            imagePanel.Left = padding;

            var pictureBox = imagePanel.Controls.OfType<PictureBox>().FirstOrDefault();
            if (pictureBox != null)
            {
                pictureBox.Width = imagePanel.Width;
                pictureBox.Height = imagePanel.Height;
            }
        }

        private void UpdateTextPanel(Panel textPanel, int panelWidth, int padding, int textHeight, Panel imagePanel)
        {
            if (textPanel == null) return;

            textPanel.Width = panelWidth - padding * 2;
            textPanel.Top = (imagePanel?.Bottom ?? padding) + padding;
            textPanel.Left = padding;
            textPanel.Height = textHeight;

            var label = textPanel.Controls.OfType<Label>().FirstOrDefault();
            if (label != null)
            {
                label.Width = textPanel.Width;
                label.Height = textHeight;
            }
        }

        private void UpdateButtonsPanel(Panel buttonsPanel, int panelWidth, int padding, int buttonsHeight, Panel textPanel, Panel imagePanel)
        {
            if (buttonsPanel == null) return;

            buttonsPanel.Width = panelWidth - padding * 2;
            buttonsPanel.Top = (textPanel?.Bottom ?? padding * 2 + (imagePanel?.Height ?? 0)) + padding;
            buttonsPanel.Left = padding;
            buttonsPanel.Height = buttonsHeight;

            var buttons = buttonsPanel.Controls.OfType<Button>().ToArray();
            if (buttons.Length >= 2)
            {
                int buttonWidth = (buttonsPanel.Width - padding * 3) / 2;
                int buttonHeight = 50;

                buttons[0].Width = buttonWidth;
                buttons[0].Height = buttonHeight;
                buttons[0].Top = (buttonsPanel.Height - buttonHeight) / 2;

                buttons[1].Width = buttonWidth;
                buttons[1].Height = buttonHeight;
                buttons[1].Top = (buttonsPanel.Height - buttonHeight) / 2;
                buttons[1].Left = buttons[0].Right + padding;
            }
        }

        private void CenterPanelsInFlowLayout()
        {
            if (flowLayoutPanel1.Controls.Count == 0) return;

            int totalWidth = flowLayoutPanel1.Controls[0].Width + flowLayoutPanel1.Padding.Horizontal;
            flowLayoutPanel1.Padding = new Padding(
                flowLayoutPanel1.ClientSize.Width > totalWidth
                    ? (flowLayoutPanel1.ClientSize.Width - totalWidth) / 2
                    : 10,
                flowLayoutPanel1.Padding.Top,
                0,
                flowLayoutPanel1.Padding.Bottom);
        }

        private async void LoadYears()
        {
            if (_isLoadingYears) return;
            _isLoadingYears = true;

            try
            {
                ClearAllControls();
                if (Directory.Exists(txtPath.Text))
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    await Task.Run(() => LoadYearsAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException) { /* Operation was canceled */ }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки годов: {ex.Message}");
            }
            finally
            {
                _cancellationTokenSource = null;
                _isLoadingYears = false;
            }
        }

        private void ClearAllControls()
        {
            treeYears.Nodes.Clear();
            listLetters.Items.Clear();
            listCases.Items.Clear();
            flowLayoutPanel1.Controls.Clear();
        }

        private void LoadYearsAsync(CancellationToken cancellationToken)
        {
            var directories = Directory.GetDirectories(txtPath.Text);
            int total = directories.Length;
            int processed = 0;

            foreach (string dir in directories)
            {
                if (cancellationToken.IsCancellationRequested) return;

                var yearName = Path.GetFileName(dir);
                this.InvokeIfRequired(() =>
                {
                    treeYears.Nodes.Add(new TreeNode(yearName)
                    {
                        ImageKey = "Year",
                        SelectedImageKey = "Year",
                        Tag = dir
                    });
                });

                processed++;
                ((IProgress<int>)_progress).Report((int)((double)processed / total * 100));
            }

            ((IProgress<int>)_progress).Report(0);
        }

        private async void TreeYears_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ClearSelectionControls();
            if (e.Node == null) return;

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                await Task.Run(() => LoadLettersAsync(e.Node.Tag.ToString(), _cancellationTokenSource.Token),
                    _cancellationTokenSource.Token);

                // Обновляем дерево годов, если нужно
                this.InvokeIfRequired(() =>
                {
                    if (!treeYears.Nodes.Cast<TreeNode>().Any(node => node.Tag.ToString() == e.Node.Tag.ToString()))
                    {
                        treeYears.Nodes.Add(new TreeNode(e.Node.Text)
                        {
                            Tag = e.Node.Tag
                        });
                    }
                });
            }
            catch (OperationCanceledException) { /* Operation was canceled */ }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки букв: {ex.Message}");
            }
            finally
            {
                _cancellationTokenSource = null;
            }
        }

        private void ClearSelectionControls()
        {
            listLetters.Items.Clear();
            listCases.Items.Clear();
            flowLayoutPanel1.Controls.Clear();
        }

        private async void LoadLettersAsync(string yearPath, CancellationToken cancellationToken)
        {
            this.InvokeIfRequired(() => listCases.Items.Clear());

            var directories = Directory.GetDirectories(yearPath);
            int total = directories.Length;
            int processed = 0;

            foreach (string dir in directories)
            {
                if (cancellationToken.IsCancellationRequested) return;

                var letterName = Path.GetFileName(dir);
                this.InvokeIfRequired(() =>
                {
                    // Проверяем, существует ли буква в списке
                    if (!listLetters.Items.Cast<ListViewItem>().Any(item => item.Text == letterName))
                    {
                        listLetters.Items.Add(new ListViewItem(letterName)
                        {
                            ImageKey = "Letter",
                            Tag = dir
                        });
                    }
                });

                processed++;
                ((IProgress<int>)_progress).Report((int)((double)processed / total * 100));
            }

            ((IProgress<int>)_progress).Report(0);
        }

        private async void ListLetters_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearCaseControls();
            if (listLetters.SelectedItems.Count == 0) return;

            string tag = listLetters.SelectedItems[0].Tag.ToString();

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                await Task.Run(() => LoadCasesAsync(tag, _cancellationTokenSource.Token), _cancellationTokenSource.Token);

                // После загрузки дел, если есть выбранное дело, убедимся, что оно видимо
                if (listCases.SelectedItems.Count > 0)
                {
                    listCases.EnsureVisible(listCases.SelectedItems[0].Index);
                }
            }
            catch (OperationCanceledException) { /* Operation was canceled */ }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки дел: {ex.Message}");
            }
            finally
            {
                _cancellationTokenSource = null;
            }
        }

        private void ClearCaseControls()
        {
            listCases.Items.Clear();
            flowLayoutPanel1.Controls.Clear();
        }

        private void LoadCasesAsync(string letterPath, CancellationToken cancellationToken)
        {
            this.InvokeIfRequired(() => listCases.Items.Clear());

            var directories = Directory.GetDirectories(letterPath);
            int total = directories.Length;
            int processed = 0;

            foreach (string dir in directories)
            {
                if (cancellationToken.IsCancellationRequested) return;

                var dirInfo = new DirectoryInfo(dir);
                this.InvokeIfRequired(() =>
                {
                    // Проверяем, существует ли дело в списке
                    if (!listCases.Items.Cast<ListViewItem>().Any(item => item.Tag.ToString() == dir))
                    {
                        listCases.Items.Add(new ListViewItem(Path.GetFileName(dir))
                        {
                            Tag = dir,
                            SubItems = { dirInfo.CreationTime.ToString("dd.MM.yyyy") }
                        });
                    }
                });

                processed++;
                ((IProgress<int>)_progress).Report((int)((double)processed / total * 100));
            }

            this.InvokeIfRequired(() =>
            {
                listCases.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listCases.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            });

            ((IProgress<int>)_progress).Report(0);
        }

        private async void ListCases_SelectedIndexChanged(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Clear();
            if (listCases.SelectedItems.Count == 0) return;

            await LoadFolderContentsAsync(listCases.SelectedItems[0].Tag.ToString());
        }

        private async Task LoadFolderContentsAsync(string path)
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                await Task.Run(() => LoadImagesAsync(path, _cancellationTokenSource.Token), _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException) { /* Operation was canceled */ }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки изображений: {ex.Message}");
            }
            finally
            {
                _cancellationTokenSource = null;
            }
        }

        private void LoadImagesAsync(string path, CancellationToken cancellationToken)
        {
            var files = Directory.GetFiles(path).Where(f => IsImageFile(Path.GetExtension(f))).ToArray();
            int total = files.Length;
            int processed = 0;

            foreach (var file in files)
            {
                if (cancellationToken.IsCancellationRequested) return;
                this.InvokeIfRequired(() => AddImageToPanel(file));
                processed++;
                ((IProgress<int>)_progress).Report((int)((double)processed / total * 100));
            }

            ((IProgress<int>)_progress).Report(0);
        }

        private bool IsImageFile(string extension)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp", ".heic" };
            return imageExtensions.Contains(extension.ToLower());
        }

        private void AddImageToPanel(string filePath)
        {
            try
            {
                const int buttonsHeight = 100;
                const int textHeight = 60;
                const int padding = 10;
                Size mainPanelSize = new Size(400, 600);

                var mainPanel = CreateMainPanel(mainPanelSize, padding);
                var imagePanel = CreateImagePanel(filePath, mainPanelSize, padding, textHeight, buttonsHeight);
                var textPanel = CreateTextPanel(filePath, mainPanelSize, padding, textHeight, imagePanel);
                var buttonsPanel = CreateButtonsPanel(filePath, mainPanelSize, padding, buttonsHeight, textPanel, imagePanel);

                mainPanel.Controls.Add(imagePanel);
                mainPanel.Controls.Add(textPanel);
                mainPanel.Controls.Add(buttonsPanel);

                flowLayoutPanel1.Controls.Add(mainPanel);
                UpdatePanelSizes();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки {Path.GetFileName(filePath)}: {ex.Message}");
            }
        }

        private Panel CreateMainPanel(Size size, int padding)
        {
            return new Panel
            {
                Width = size.Width,
                Height = size.Height,
                Margin = new Padding(padding),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private Panel CreateImagePanel(string filePath, Size mainPanelSize, int padding, int textHeight, int buttonsHeight)
        {
            int imageAvailableHeight = mainPanelSize.Height - textHeight - buttonsHeight - padding * 3;
            var imagePanel = new Panel
            {
                Width = mainPanelSize.Width - padding * 2,
                Height = imageAvailableHeight,
                Top = padding,
                Left = padding
            };

            using (var originalImage = new Bitmap(filePath))
            {
                double ratio = (double)originalImage.Width / originalImage.Height;
                int imageWidth = Math.Min(originalImage.Width, imagePanel.Width);
                int imageHeight = (int)(imageWidth / ratio);

                if (imageHeight > imageAvailableHeight)
                {
                    imageHeight = imageAvailableHeight;
                    imageWidth = (int)(imageHeight * ratio);
                }

                var previewImage = new Bitmap(imageWidth, imageHeight);
                using (var g = Graphics.FromImage(previewImage))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.DrawImage(originalImage, 0, 0, imageWidth, imageHeight);
                }

                var pictureBox = new PictureBox
                {
                    Image = previewImage,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Width = imagePanel.Width,
                    Height = imageHeight
                };
                imagePanel.Controls.Add(pictureBox);
            }

            return imagePanel;
        }

        private Panel CreateTextPanel(string filePath, Size mainPanelSize, int padding, int textHeight, Panel imagePanel)
        {
            var textPanel = new Panel
            {
                Width = mainPanelSize.Width - padding * 2,
                Height = textHeight,
                Top = imagePanel.Bottom + padding,
                Left = padding
            };

            var fi = new FileInfo(filePath);
            var label = new Label
            {
                Text = $"{fi.Name}\nСоздан: {fi.CreationTime:dd.MM.yyyy HH:mm}",
                Width = textPanel.Width,
                Height = textHeight,
                TextAlign = ContentAlignment.MiddleLeft
            };
            textPanel.Controls.Add(label);

            return textPanel;
        }

        private Panel CreateButtonsPanel(string filePath, Size mainPanelSize, int padding, int buttonsHeight,
                                        Panel textPanel, Panel imagePanel)
        {
            var buttonsPanel = new Panel
            {
                Width = mainPanelSize.Width - padding * 2,
                Height = buttonsHeight,
                Top = textPanel.Bottom + padding,
                Left = padding
            };

            var btnDelete = CreateButton("Удалить", iconSet.Images["Delete"],
                (buttonsPanel.Width - padding * 3) / 2, 50, padding,
                (buttonsHeight - 50) / 2, filePath);
            btnDelete.Click += (s, ev) => DeleteScanFile(filePath);

            var btnRescan = CreateButton("Пересканировать", iconSet.Images["Scaner"],
                (buttonsPanel.Width - padding * 3) / 2, 50, btnDelete.Right + padding,
                (buttonsHeight - 50) / 2, filePath);
            btnRescan.Click += async (s, ev) => await RescanFile(filePath);

            buttonsPanel.Controls.Add(btnDelete);
            buttonsPanel.Controls.Add(btnRescan);

            return buttonsPanel;
        }

        private Button CreateButton(string text, Image image, int width, int height,
                                   int left, int top, string tag)
        {
            return new Button
            {
                Text = text,
                Image = image,
                Width = width,
                Height = height,
                Top = top,
                Left = left,
                Tag = tag,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleRight,
                TextImageRelation = TextImageRelation.ImageBeforeText
            };
        }

        private void DeleteScanFile(string filePath)
        {
            if (ShowConfirmation($"Вы уверены, что хотите удалить файл {Path.GetFileName(filePath)}?",
                "Подтверждение удаления") != DialogResult.Yes) return;

            try
            {
                File.Delete(filePath);
                string casePath = Path.GetDirectoryName(filePath);
                flowLayoutPanel1.Controls.Clear();
                LoadFolderContentsAsync(casePath);

                UpdateStatus($"Файл {Path.GetFileName(filePath)} удален.");
                LogAction($"Файл {Path.GetFileName(filePath)} удален.");
                ShowInfo("Файл успешно удален.", "Удаление завершено");
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка при удалении файла: {ex.Message}");
                ShowError($"Ошибка при удалении файла: {ex.Message}", "Ошибка");
            }
        }

        private async Task RescanFile(string filePath)
        {
            try
            {
                UpdateStatus($"Запущено пересканирование файла: {Path.GetFileName(filePath)}");
                LogAction($"Запущено пересканирование файла: {Path.GetFileName(filePath)}");

                btnCancel.Enabled = true;

                _cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _cancellationTokenSource.Token;

                ((IProgress<int>)_progress).Report(10);

                var deviceManager = await Task.Run(() => new DeviceManager());
                ((IProgress<int>)_progress).Report(20);

                var scannerInfo = deviceManager.DeviceInfos.Cast<DeviceInfo>()
                    .First(d => d.Properties["Name"].get_Value().ToString() == cbDriver.Text);

                var scanner = await Task.Run(() => scannerInfo.Connect());
                ((IProgress<int>)_progress).Report(30);

                var item = scanner.Items[1];

                if (int.TryParse(cbDPI.Text, out int dpi))
                {
                    item.Properties["6147"].set_Value(dpi);
                    item.Properties["6148"].set_Value(dpi);
                }

                ((IProgress<int>)_progress).Report(40);

                var imageFile = await Task.Run(() =>
                {
                    ((IProgress<int>)_progress).Report(50);
                    var file = (ImageFile)item.Transfer("{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}");
                    ((IProgress<int>)_progress).Report(80);
                    return file;
                }, cancellationToken);

                CleanUpExistingFile(filePath);
                await Task.Run(() => imageFile.SaveFile(filePath), cancellationToken);
                ((IProgress<int>)_progress).Report(90);

                this.InvokeIfRequired(() =>
                {
                    string casePath = Path.GetDirectoryName(filePath);
                    flowLayoutPanel1.Controls.Clear();
                    LoadFolderContentsAsync(casePath);
                    UpdateStatus($"Файл {Path.GetFileName(filePath)} успешно пересканирован.");
                    LogAction($"Файл {Path.GetFileName(filePath)} успешно пересканирован.");
                    ShowInfo("Файл успешно пересканирован и сохранен!", "Готово");
                    btnCancel.Enabled = false;
                });

                ((IProgress<int>)_progress).Report(100);
                await Task.Delay(500);
                ((IProgress<int>)_progress).Report(0);
            }
            catch (OperationCanceledException)
            {
                ((IProgress<int>)_progress).Report(0);
                UpdateStatus("Пересканирование отменено.");
                LogAction("Пересканирование отменено.");
                ShowInfo("Сканирование отменено");
                btnCancel.Enabled = false;
            }
            catch (Exception ex)
            {
                ((IProgress<int>)_progress).Report(0);
                UpdateStatus($"Ошибка при пересканировании файла: {Path.GetFileName(filePath)}");
                LogAction($"Ошибка при пересканировании файла: {ex.Message}");
                ShowError($"Ошибка при пересканировании: {ex.Message}", "Ошибка");
                btnCancel.Enabled = false;
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void CleanUpExistingFile(string filePath)
        {
            if (!File.Exists(filePath)) return;

            this.InvokeIfRequired(() =>
            {
                foreach (Control control in flowLayoutPanel1.Controls)
                {
                    if (control is Panel panel)
                    {
                        var pictureBox = panel.Controls.OfType<PictureBox>()
                            .FirstOrDefault(pb => pb.Tag?.ToString() == filePath);

                        if (pictureBox != null)
                        {
                            pictureBox.Image?.Dispose();
                            pictureBox.Image = null;
                        }
                    }
                }
            });

            File.Delete(filePath);
        }

        private void btnFindPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = dialog.SelectedPath;
                    Properties.Settings.Default.DataScanPath = dialog.SelectedPath;
                    Properties.Settings.Default.Save();
                    LogAction($"Путь к каталогу папок изменен на: {dialog.SelectedPath}");
                    LoadYears();
                }
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                string appPath = Application.StartupPath;
                string dataScanPath = Path.Combine(appPath, "dataScan");

                Directory.CreateDirectory(dataScanPath);

                int currentYear = DateTime.Now.Year;
                for (int year = 2000; year <= currentYear; year++)
                {
                    Directory.CreateDirectory(Path.Combine(dataScanPath, year.ToString()));
                }

                txtPath.Text = dataScanPath;
                Properties.Settings.Default.DataScanPath = dataScanPath;
                Properties.Settings.Default.Save();

                UpdateStatus($"Создана структура папок в каталоге: {dataScanPath}");
                LogAction($"Создана структура папок в каталоге: {dataScanPath}");
                LoadYears();
                ShowInfo($"Структура успешно создана в папке:\n{dataScanPath}");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при создании структуры:\n{ex.Message}", "Ошибка");
            }
        }

        private void LoadScanners()
        {
            cbDriver.Items.Clear();
            try
            {
                var deviceManager = new DeviceManager();

                foreach (DeviceInfo device in deviceManager.DeviceInfos)
                {
                    if (device.Type == WiaDeviceType.ScannerDeviceType)
                    {
                        string deviceName = device.Properties["Name"].get_Value().ToString();
                        cbDriver.Items.Add(deviceName);
                    }
                }

                if (cbDriver.Items.Count == 0)
                {
                    cbDriver.Items.Add("Сканеры не найдены");
                    ShowInfo("Не найдено ни одного сканера (USB или сетевого). Проверьте подключение.");
                }

                cbDriver.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка WIA: {ex.Message}\n\nПопробуйте следующее:\n1. Проверьте, что сканер включен и подключен\n2. Для сетевых сканеров проверьте сетевое подключение\n3. Установите драйверы для сканера",
                    "Ошибка подключения");
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateStatus("Поиск сканеров...");
                LoadScanners();

                if (cbDriver.Items.Count > 0 && !cbDriver.Items[0].ToString().Contains("не найдены"))
                {
                    UpdateStatus($"Найдено {cbDriver.Items.Count} сканеров (USB и сетевых)");
                    LogAction($"Обновлен список сканеров. Найдено устройств: {cbDriver.Items.Count}");
                }
                else
                {
                    UpdateStatus("Сканеры не найдены");
                    LogAction("Сканеры не найдены при обновлении списка");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка при обновлении списка сканеров: {ex.Message}");
                LogAction($"Ошибка при обновлении списка сканеров: {ex.Message}");
            }
        }

        private async void btnNewProfile_Click(object sender, EventArgs e)
        {
            if (treeYears.SelectedNode == null)
            {
                ShowInfo("Сначала выберите год в дереве!");
                return;
            }

            using (var form = CreateNewProfileForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    await ProcessNewProfileCreation(form);
                }
            }
        }

        private Form CreateNewProfileForm()
        {
            var form = new Form
            {
                Text = "Добавить новое дело",
                Size = new Size(400, 250),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblYear = new Label
            {
                Text = $"Выбранный год: {treeYears.SelectedNode.Text}",
                Location = new Point(20, 20),
                AutoSize = true
            };

            var lblNumber = new Label
            {
                Text = "Номер дела:",
                Location = new Point(20, 60),
                AutoSize = true
            };

            var txtNumber = new TextBox { Name = "txtNumber", Location = new Point(150, 55), Size = new Size(200, 20) };

            var lblFIO = new Label
            {
                Text = "ФИО:",
                Location = new Point(20, 100),
                AutoSize = true
            };

            var txtFIO = new TextBox { Name = "txtFIO", Location = new Point(150, 95), Size = new Size(200, 20) };

            var btnOK = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(150, 150),
                Size = new Size(80, 30)
            };

            var btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new Point(250, 150),
                Size = new Size(80, 30)
            };

            form.Controls.AddRange(new Control[] { lblYear, lblNumber, txtNumber, lblFIO, txtFIO, btnOK, btnCancel });
            return form;
        }

        private async Task ProcessNewProfileCreation(Form form)
        {
            var txtNumber = form.Controls.OfType<TextBox>().First(t => t.Name.Contains("Number"));
            var txtFIO = form.Controls.OfType<TextBox>().First(t => t.Name.Contains("FIO"));

            try
            {
                if (!int.TryParse(txtNumber.Text, out int caseNumber))
                    throw new ArgumentException("Номер дела должен быть числом!");

                if (string.IsNullOrWhiteSpace(txtFIO.Text))
                    throw new ArgumentException("Введите ФИО!");

                var firstName = txtFIO.Text.Trim().Split().FirstOrDefault();
                if (string.IsNullOrEmpty(firstName))
                    throw new ArgumentException("Некорректное ФИО!");

                var letter = firstName[0].ToString().ToUpper();
                var yearPath = treeYears.SelectedNode.Tag.ToString();
                var letterPath = Path.Combine(yearPath, letter);
                var caseFolder = Path.Combine(letterPath, $"№{caseNumber:D3} {txtFIO.Text.Trim()}");

                Directory.CreateDirectory(letterPath);
                Directory.CreateDirectory(caseFolder);

                // Обновляем UI
                await this.InvokeIfRequiredAsync(async () =>
                {
                    // 1. Обновляем список букв
                    var existingLetter = listLetters.Items.Cast<ListViewItem>().FirstOrDefault(item => item.Text == letter);
                    if (existingLetter == null)
                    {
                        listLetters.Items.Add(new ListViewItem(letter)
                        {
                            ImageKey = "Letter",
                            Tag = letterPath
                        });
                    }

                    // 2. Выбираем нужную букву
                    foreach (ListViewItem letterItem in listLetters.Items)
                    {
                        if (letterItem.Text == letter)
                        {
                            listLetters.SelectedItems.Clear();
                            letterItem.Selected = true;
                            letterItem.Focused = true;
                            listLetters.EnsureVisible(letterItem.Index);
                            break;
                        }
                    }

                    // 3. Ждем обновления списка дел
                    await Task.Delay(100); // Даем время на обработку события SelectedIndexChanged

                    // 4. Находим и выбираем созданное дело
                    var newCaseItem = listCases.Items.Cast<ListViewItem>()
                        .FirstOrDefault(item => item.Tag.ToString().Equals(caseFolder, StringComparison.OrdinalIgnoreCase));

                    if (newCaseItem != null)
                    {
                        listCases.SelectedItems.Clear();
                        newCaseItem.Selected = true;
                        newCaseItem.Focused = true;
                        listCases.EnsureVisible(newCaseItem.Index);
                        listCases.Focus();
                    }
                    else
                    {
                        ShowError("Новое дело не найдено в списке дел.");
                    }
                });
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
        }

        private async Task RefreshUIAfterProfileCreation(string letter, string caseFolder)
        {
            await this.InvokeIfRequiredAsync(() =>
            {
                if (listLetters.SelectedItems.Count > 0)
                {
                    var letterItem = listLetters.SelectedItems[0];
                    letterItem.Selected = false;
                    letterItem.Selected = true;
                }
            });

            await WaitForListCasesLoad();

            var caseItem = listCases.Items.Cast<ListViewItem>()
                .FirstOrDefault(item => Path.GetFullPath(item.Tag.ToString()).Equals(
                    Path.GetFullPath(caseFolder), StringComparison.OrdinalIgnoreCase));

            if (caseItem != null)
            {
                await this.InvokeIfRequiredAsync(() =>
                {
                    caseItem.Selected = true;
                    caseItem.Focused = true;
                    listCases.EnsureVisible(caseItem.Index);
                    listCases.Focus();
                    ListCases_SelectedIndexChanged(null, EventArgs.Empty);
                });
            }
        }

        private async Task WaitForListCasesLoad()
        {
            while (IsCasesLoading())
            {
                await Task.Delay(50);
            }
        }

        private bool IsCasesLoading()
        {
            return listCases.Items.Cast<ListViewItem>().Any(item => item.Text == "Loading...") ||
                   flowLayoutPanel1.Controls.Count == 0;
        }

        private async void btnNewListProfile_Click(object sender, EventArgs e)
        {
            try
            {
                if (treeYears.SelectedNode == null ||
                    listLetters.SelectedItems.Count == 0 ||
                    listCases.SelectedItems.Count == 0)
                    throw new Exception("Выберите элементы во всех трех уровнях!");

                await ProcessNewScan();
            }
            catch (OperationCanceledException)
            {
                ((IProgress<int>)_progress).Report(0);
                UpdateStatus("Сканирование отменено.");
                LogAction("Сканирование отменено.");
            }
            catch (Exception ex)
            {
                ((IProgress<int>)_progress).Report(0);
                UpdateStatus($"Ошибка сканирования дела: {ex.Message}");
                LogAction($"Ошибка сканирования дела: {ex.Message}");
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async Task ProcessNewScan()
        {
            var savePath = listCases.SelectedItems[0].Tag.ToString();
            var caseFolder = Path.GetFileName(savePath);
            var fio = caseFolder.Split(new[] { ' ' }, 2).Last();

            btnCancel.Enabled = true;

            UpdateStatus($"Запущено сканирование дела: {fio}");
            LogAction($"Запущено сканирование дела: {fio}");

            int nextScanNumber = CalculateNextScanNumber(savePath, fio);

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            ((IProgress<int>)_progress).Report(10);

            var deviceManager = await Task.Run(() => new DeviceManager());
            ((IProgress<int>)_progress).Report(20);

            var scannerInfo = deviceManager.DeviceInfos.Cast<DeviceInfo>()
                .First(d => d.Properties["Name"].get_Value().ToString() == cbDriver.Text);

            var scanner = await Task.Run(() => scannerInfo.Connect());
            ((IProgress<int>)_progress).Report(30);

            var item = scanner.Items[1];

            if (int.TryParse(cbDPI.Text, out int dpi))
            {
                item.Properties["6147"].set_Value(dpi);
                item.Properties["6148"].set_Value(dpi);
            }

            ((IProgress<int>)_progress).Report(40);

            var imageFile = await Task.Run(() =>
            {
                ((IProgress<int>)_progress).Report(50);
                var file = (ImageFile)item.Transfer("{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}");
                ((IProgress<int>)_progress).Report(80);
                return file;
            }, cancellationToken);

            var newFileName = $"{fio} {nextScanNumber:D3}.jpg";
            var fullPath = Path.Combine(savePath, newFileName);

            Directory.CreateDirectory(savePath);
            await Task.Run(() => imageFile.SaveFile(fullPath), cancellationToken);
            ((IProgress<int>)_progress).Report(90);

            this.InvokeIfRequired(() =>
            {
                flowLayoutPanel1.Controls.Clear();
                btnCancel.Enabled = false;
                LoadFolderContentsAsync(savePath);
                UpdateStatus($"Сканирование дела {fio} завершено. Файл сохранен: {newFileName}");
                LogAction($"Сканирование дела {fio} завершено. Файл сохранен: {newFileName}");
            });

            ((IProgress<int>)_progress).Report(100);
            await Task.Delay(500);
            ((IProgress<int>)_progress).Report(0);
        }

        private int CalculateNextScanNumber(string savePath, string fio)
        {
            var files = Directory.GetFiles(savePath, $"{fio} *.jpg");
            var numbers = new List<int>();

            foreach (var file in files)
            {
                var currentFileName = Path.GetFileNameWithoutExtension(file);
                var parts = currentFileName.Split(' ');
                if (parts.Length < 2) continue;

                if (int.TryParse(parts.Last(), out int num))
                    numbers.Add(num);
            }

            numbers.Sort();

            int nextScanNumber = 1;
            foreach (var num in numbers)
            {
                if (num > nextScanNumber) break;
                nextScanNumber = num + 1;
            }

            return nextScanNumber;
        }

        private void txtPath_TextChanged(object sender, EventArgs e)
        {
            LoadYears();
        }

        private void UpdateStatus(string message)
        {
            this.InvokeIfRequired(() => lableStatus.Text = message);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Control && e.KeyCode == Keys.A)
                btnNewProfile_Click(this, EventArgs.Empty);
            else if (e.Control && e.KeyCode == Keys.S)
                btnNewListProfile_Click(this, EventArgs.Empty);
            else if (e.Control && e.KeyCode == Keys.D)
                btnDelProfile_Click(this, EventArgs.Empty);
        }

        private void btnDelProfile_Click(object sender, EventArgs e)
        {
            if (listCases.SelectedItems.Count == 0)
            {
                ShowInfo("Пожалуйста, выберите дело для удаления.", "Удаление дела");
                return;
            }

            var selectedItem = listCases.SelectedItems[0];
            var casePath = selectedItem.Tag?.ToString();

            if (string.IsNullOrEmpty(casePath) || !Directory.Exists(casePath))
            {
                ShowError("Путь к выбранному делу недействителен или дело уже удалено.", "Ошибка");
                return;
            }

            if (ShowConfirmation($"Вы уверены, что хотите удалить дело \"{selectedItem.Text}\"?\nВсе файлы и папки внутри будут удалены безвозвратно.",
                               "Подтверждение удаления") != DialogResult.Yes) return;

            try
            {
                Directory.Delete(casePath, true);
                listCases.Items.Remove(selectedItem);

                LogAction($"Дело \"{selectedItem.Text}\" удалено.");
                ShowInfo("Дело успешно удалено.", "Удаление завершено");
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка при удалении дела \"{selectedItem.Text}\": {ex.Message}");
                ShowError($"Ошибка при удалении дела: {ex.Message}", "Ошибка");
            }
        }

        private void LogAction(string message)
        {
            this.InvokeIfRequired(() =>
            {
                string timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                rtbLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
                rtbLog.ScrollToCaret();
            });
        }

        private void btSearch_Click(object sender, EventArgs e)
        {
            string searchText = tbSearch.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                ShowInfo("Введите текст для поиска.", "Поиск");
                return;
            }

            ClearSearchSelection();

            if (!Directory.Exists(txtPath.Text))
            {
                ShowError("Указанный путь не существует.", "Ошибка");
                return;
            }

            var searchResult = SearchCaseInFileSystem(searchText);
            if (searchResult.casePath == null && searchResult.letterPath == null && searchResult.yearPath == null)
            {
                ShowInfo("Ничего не найдено.", "Поиск");
                return;
            }

            DisplaySearchResult(searchResult);
        }

        private void ClearSearchSelection()
        {
            treeYears.SelectedNode = null;
            listLetters.SelectedItems.Clear();
            listCases.SelectedItems.Clear();
        }

        private (string casePath, string letterPath, string yearPath) SearchCaseInFileSystem(string searchText)
        {
            foreach (var yearDir in Directory.GetDirectories(txtPath.Text))
            {
                foreach (var letterDir in Directory.GetDirectories(yearDir))
                {
                    foreach (var caseDir in Directory.GetDirectories(letterDir))
                    {
                        if (Path.GetFileName(caseDir).IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                            return (caseDir, letterDir, yearDir);
                    }
                }
            }
            return (null, null, null);
        }

        private void DisplaySearchResult((string casePath, string letterPath, string yearPath) result)
        {
            var yearNode = treeYears.Nodes.Cast<TreeNode>().FirstOrDefault(node => node.Tag.ToString() == result.yearPath);
            if (yearNode != null)
            {
                treeYears.SelectedNode = yearNode;
                treeYears.Focus();
                TreeYears_AfterSelect(treeYears, new TreeViewEventArgs(yearNode));
            }

            var letterItem = listLetters.Items.Cast<ListViewItem>().FirstOrDefault(item => item.Tag.ToString() == result.letterPath);
            if (letterItem != null)
            {
                letterItem.Selected = true;
                letterItem.Focused = true;
                ListLetters_SelectedIndexChanged(listLetters, EventArgs.Empty);
            }

            var caseItem = listCases.Items.Cast<ListViewItem>().FirstOrDefault(item => item.Tag.ToString() == result.casePath);
            if (caseItem != null)
            {
                caseItem.Selected = true;
                caseItem.Focused = true;
                listCases.Focus();
            }
        }

        private void ShowError(string message, string title = "Ошибка")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowInfo(string message, string title = "Информация")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private DialogResult ShowConfirmation(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                UpdateStatus("Операция сканирования отменена.");
                LogAction("Операция сканирования была отменена пользователем.");
                ShowInfo("Сканирование успешно отменено.", "Отмена");
                btnCancel.Enabled = false;
            }
            else
            {
                ShowInfo("Нет активной операции для отмены.", "Отмена");
            }
        }
    }

    public static class ControlExtensions
    {
        public static void InvokeIfRequired(this Control control, Action action)
        {
            if (control.InvokeRequired)
                control.Invoke(action);
            else
                action();
        }

        public static async Task InvokeIfRequiredAsync(this Control control, Action action)
        {
            if (control.InvokeRequired)
                await Task.Run(() => control.Invoke(action));
            else
                action();
        }
    }
}