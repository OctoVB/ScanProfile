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
                progressBar1.Visible = true; //value > 0 && value < 100;
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
            //MainTab.ItemSize = new System.Drawing.Size(200, 200);
        }

        private void LoadComboBoxSettings(ComboBox comboBox, string savedValue)
        {
            int index = comboBox.Items.IndexOf(savedValue);
            if (index >= 0)
            {
                comboBox.SelectedIndex = index;
            }
            else if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;

            if (comboBox == cbPepperType)
            {
                Properties.Settings.Default.PepperType = comboBox.SelectedItem?.ToString();
            }
            else if (comboBox == cbDPI)
            {
                Properties.Settings.Default.DPI = comboBox.SelectedItem?.ToString();
            }
            Properties.Settings.Default.Save();
        }

        private void ConfigureComponents()
        {
            treeYears.ImageList = new ImageList();
            treeYears.ImageList.Images.Add("Year", iconSet.Images["Dot"]);
            treeYears.HideSelection = false;
            treeYears.AfterSelect += TreeYears_AfterSelect;

            listLetters.View = View.List;
            listLetters.SmallImageList = new ImageList();
            listLetters.SmallImageList.Images.Add("Letter", iconSet.Images["Dot"]);
            listLetters.MultiSelect = false;
            listLetters.SelectedIndexChanged += ListLetters_SelectedIndexChanged;

            listCases.View = View.Details;
            listCases.Columns.Add("Дело", 150);
            listCases.Columns.Add("Дата создания", 120);
            listCases.FullRowSelect = true;
            listCases.MultiSelect = false;
            listCases.SelectedIndexChanged += ListCases_SelectedIndexChanged;

            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            this.Resize += MainForm_Resize;

            progressBar1.Visible = false;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
        }

        private bool _isResizing = false;
        private Size _lastClientSize; // Изменили на Size вместо Rectangle
        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (_isResizing || this.ClientSize == _lastClientSize)
                return;

            _isResizing = true;
            _lastClientSize = this.ClientSize;

            flowLayoutPanel1.SuspendLayout();

            Task.Delay(100).ContinueWith(_ =>
            {
                if (flowLayoutPanel1.InvokeRequired)
                {
                    flowLayoutPanel1.Invoke(new Action(() =>
                    {
                        UpdatePanelSizes();
                        flowLayoutPanel1.ResumeLayout(true);
                        _isResizing = false;
                    }));
                }
                else
                {
                    UpdatePanelSizes();
                    flowLayoutPanel1.ResumeLayout(true);
                    _isResizing = false;
                }
            });
        }

        // Метод для обновления размеров панелей
        private void UpdatePanelSizes()
        {
            if (flowLayoutPanel1.Controls.Count == 0) return;

            // Рассчитываем размеры панели в зависимости от доступного пространства
            int availableWidth = flowLayoutPanel1.ClientSize.Width - flowLayoutPanel1.Padding.Horizontal - 20;
            int availableHeight = flowLayoutPanel1.ClientSize.Height - flowLayoutPanel1.Padding.Vertical - 20;

            // Базовые пропорции панели (ширина:высота = 2:3)
            int panelWidth = Math.Max(400, Math.Min(1600, availableWidth - 20));
            int panelHeight = Math.Min((int)(panelWidth * 1.5), availableHeight - 50); // Ограничиваем высоту

            foreach (Control control in flowLayoutPanel1.Controls.OfType<Panel>())
            {
                control.Width = panelWidth;
                control.Height = panelHeight;

                int padding = 10;
                int buttonsHeight = 100;
                int textHeight = 60;

                // Находим дочерние панели
                var imagePanel = control.Controls.OfType<Panel>().FirstOrDefault(p => p.Controls.OfType<PictureBox>().Any());
                var textPanel = control.Controls.OfType<Panel>().FirstOrDefault(p => p.Controls.OfType<Label>().Any());
                var buttonsPanel = control.Controls.OfType<Panel>().FirstOrDefault(p => p.Controls.OfType<Button>().Count() >= 2);

                // Обновляем панель изображения
                if (imagePanel != null)
                {
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

                // Обновляем текстовую панель
                if (textPanel != null)
                {
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

                // Обновляем панель кнопок
                if (buttonsPanel != null)
                {
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
            }
        }
        // Метод для центрирования панелей в flowLayoutPanel
        private void CenterPanelsInFlowLayout()
        {
            if (flowLayoutPanel1.Controls.Count == 0) return;

            // Рассчитываем общую ширину всех элементов
            int totalWidth = flowLayoutPanel1.Controls[0].Width + flowLayoutPanel1.Padding.Horizontal;

            // Если есть место для центрирования
            if (flowLayoutPanel1.ClientSize.Width > totalWidth)
            {
                flowLayoutPanel1.Padding = new Padding(
                    (flowLayoutPanel1.ClientSize.Width - totalWidth) / 2,
                    flowLayoutPanel1.Padding.Top,
                    0,
                    flowLayoutPanel1.Padding.Bottom);
            }
            else
            {
                flowLayoutPanel1.Padding = new Padding(10, flowLayoutPanel1.Padding.Top, 0, flowLayoutPanel1.Padding.Bottom);
            }
        }

        private bool _isLoadingYears = false;
        private async void LoadYears()
        {
            if (_isLoadingYears) return; // Если загрузка уже идет, выходим
            _isLoadingYears = true;

            try
            {
                treeYears.Nodes.Clear();
                listLetters.Items.Clear();
                listCases.Items.Clear();
                flowLayoutPanel1.Controls.Clear();

                if (Directory.Exists(txtPath.Text))
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    await Task.Run(() => LoadYearsAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Операция была отменена
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки годов: {ex.Message}");
            }
            finally
            {
                _cancellationTokenSource = null;
                _isLoadingYears = false; // Сбрасываем флаг после завершения загрузки
            }
        }

        private void LoadYearsAsync(CancellationToken cancellationToken)
        {
            var directories = Directory.GetDirectories(txtPath.Text);
            int total = directories.Length;
            int processed = 0;

            foreach (string dir in directories)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var yearName = Path.GetFileName(dir);
                Invoke(new Action(() =>
                {
                    var yearNode = new TreeNode(yearName)
                    {
                        ImageKey = "Year",
                        SelectedImageKey = "Year",
                        Tag = dir
                    };
                    treeYears.Nodes.Add(yearNode);
                }));

                processed++;
                int progress = (int)((double)processed / total * 100);
                ((IProgress<int>)_progress).Report(progress);
            }

            ((IProgress<int>)_progress).Report(0);
        }

        private async void TreeYears_AfterSelect(object sender, TreeViewEventArgs e)
        {
            listLetters.Items.Clear();
            listCases.Items.Clear();
            flowLayoutPanel1.Controls.Clear();

            if (e.Node == null) return;

            string yearPath = e.Node.Tag.ToString();

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                await Task.Run(() => LoadLettersAsync(yearPath, _cancellationTokenSource.Token), _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Операция была отменена
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки букв: {ex.Message}");
            }
            finally
            {
                _cancellationTokenSource = null;
            }
        }

        private void LoadLettersAsync(string yearPath, CancellationToken cancellationToken)
        {
            // Очищаем список перед загрузкой
            Invoke(new Action(() => listCases.Items.Clear()));

            var directories = Directory.GetDirectories(yearPath);
            int total = directories.Length;
            int processed = 0;

            foreach (string dir in directories)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var letterName = Path.GetFileName(dir);
                Invoke(new Action(() =>
                {
                    var item = new ListViewItem(letterName)
                    {
                        ImageKey = "Letter",
                        Tag = dir
                    };
                    listLetters.Items.Add(item);
                }));

                processed++;
                int progress = (int)((double)processed / total * 100);
                ((IProgress<int>)_progress).Report(progress);
            }

            ((IProgress<int>)_progress).Report(0);
        }

        private async void ListLetters_SelectedIndexChanged(object sender, EventArgs e)
        {
            listCases.Items.Clear();
            flowLayoutPanel1.Controls.Clear();

            if (listLetters.SelectedItems.Count == 0) return;

            string letterPath = listLetters.SelectedItems[0].Tag.ToString();

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                await Task.Run(() => LoadCasesAsync(letterPath, _cancellationTokenSource.Token), _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Операция была отменена
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки дел: {ex.Message}");
            }
            finally
            {
                _cancellationTokenSource = null;
            }
        }

        private void LoadCasesAsync(string letterPath, CancellationToken cancellationToken)
        {
            // Очищаем список перед загрузкой
            Invoke(new Action(() => listCases.Items.Clear()));

            var directories = Directory.GetDirectories(letterPath);
            int total = directories.Length;
            int processed = 0;

            foreach (string dir in directories)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var dirInfo = new DirectoryInfo(dir);
                Invoke(new Action(() =>
                {
                    var item = new ListViewItem(Path.GetFileName(dir))
                    {
                        Tag = dir,
                        SubItems = { dirInfo.CreationTime.ToString("dd.MM.yyyy") }
                    };
                    listCases.Items.Add(item);
                }));

                processed++;
                int progress = (int)((double)processed / total * 100);
                ((IProgress<int>)_progress).Report(progress);
            }

            Invoke(new Action(() =>
            {
                listCases.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listCases.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }));

            ((IProgress<int>)_progress).Report(0);
        }

        private async void ListCases_SelectedIndexChanged(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Clear();

            if (listCases.SelectedItems.Count == 0) return;

            string casePath = listCases.SelectedItems[0].Tag.ToString();
            await LoadFolderContentsAsync(casePath);
        }

        private async Task LoadFolderContentsAsync(string path)
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                await Task.Run(() => LoadImagesAsync(path, _cancellationTokenSource.Token), _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Операция была отменена
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображений: {ex.Message}");
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
                if (cancellationToken.IsCancellationRequested)
                    return;

                Invoke(new Action(() => AddImageToPanel(file)));

                processed++;
                int progress = (int)((double)processed / total * 100);
                ((IProgress<int>)_progress).Report(progress);
            }

            ((IProgress<int>)_progress).Report(0);
        }

        private void AddImageToPanel(string filePath)
        {
            try
            {
                // === БАЗОВЫЕ РАЗМЕРЫ И НАСТРОЙКИ ===
                Size mainPanelSize = new Size(400, 600); // Основной размер контейнера
                int buttonsHeight = 100; // Высота блока с кнопками
                int textHeight = 60;     // Высота блока с текстом
                int padding = 10;        // Общий отступ

                // Рассчитываем доступную высоту для изображения
                int imageAvailableHeight = mainPanelSize.Height - textHeight - buttonsHeight - padding * 3;

                // === СОЗДАНИЕ ГЛАВНОЙ ПАНЕЛИ ===
                var mainPanel = new Panel
                {
                    Width = mainPanelSize.Width,
                    Height = mainPanelSize.Height,
                    Margin = new Padding(padding),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // === ПАНЕЛЬ ДЛЯ ИЗОБРАЖЕНИЯ ===
                var imagePanel = new Panel
                {
                    Width = mainPanelSize.Width - padding * 2,
                    Height = imageAvailableHeight,
                    Top = padding,
                    Left = padding
                };

                using (var originalImage = new Bitmap(filePath)) // Лучше, чем Image.FromFile()
                {
                    double ratio = (double)originalImage.Width / originalImage.Height;
                    int imageWidth = Math.Min(originalImage.Width, imagePanel.Width); // Не увеличиваем сверх оригинала
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
                        Height = imageHeight,
                        Top = 0,
                        Left = 0
                    };
                    imagePanel.Controls.Add(pictureBox);
                }

                // === ПАНЕЛЬ С ТЕКСТОМ ===
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

                // === ПАНЕЛЬ С КНОПКАМИ ===
                var buttonsPanel = new Panel
                {
                    Width = mainPanelSize.Width - padding * 2,
                    Height = buttonsHeight,
                    Top = textPanel.Bottom + padding,
                    Left = padding
                };

                // Кнопка удаления
                var btnDelete = new Button
                {
                    Text = "Удалить",
                    Image = iconSet.Images["Delete"],
                    Width = (buttonsPanel.Width - padding * 3) / 2,
                    Height = 50,
                    Top = (buttonsHeight - 50) / 2,
                    Left = padding,
                    Tag = filePath,
                    ImageAlign = ContentAlignment.MiddleLeft,
                    TextAlign = ContentAlignment.MiddleRight,
                    TextImageRelation = TextImageRelation.ImageBeforeText
                };
                btnDelete.Click += (s, ev) => DeleteScanFile(filePath);
                buttonsPanel.Controls.Add(btnDelete);

                // Кнопка пересканирования
                var btnRescan = new Button
                {
                    Text = "Пересканировать",
                    Image = iconSet.Images["Scaner"],
                    Width = (buttonsPanel.Width - padding * 3) / 2,
                    Height = 50,
                    Top = (buttonsHeight - 50) / 2,
                    Left = btnDelete.Right + padding,
                    Tag = filePath,
                    ImageAlign = ContentAlignment.MiddleLeft,
                    TextAlign = ContentAlignment.MiddleRight,
                    TextImageRelation = TextImageRelation.ImageBeforeText
                };
                btnRescan.Click += async (s, ev) => await RescanFile(filePath);
                buttonsPanel.Controls.Add(btnRescan);

                // === ДОБАВЛЯЕМ ВСЕ ПАНЕЛИ В ГЛАВНУЮ ПАНЕЛЬ ===
                mainPanel.Controls.Add(imagePanel);
                mainPanel.Controls.Add(textPanel);
                mainPanel.Controls.Add(buttonsPanel);

                // Добавляем в flowLayoutPanel
                flowLayoutPanel1.Controls.Add(mainPanel);
                UpdatePanelSizes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки {Path.GetFileName(filePath)}: {ex.Message}");
            }
        }
        private void DeleteScanFile(string filePath)
        {
            if (MessageBox.Show($"Вы уверены, что хотите удалить файл {Path.GetFileName(filePath)}?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    File.Delete(filePath);

                    // Обновляем список файлов
                    string casePath = Path.GetDirectoryName(filePath);
                    flowLayoutPanel1.Controls.Clear();
                    LoadFolderContentsAsync(casePath);

                    UpdateStatus($"Файл {Path.GetFileName(filePath)} удален.");
                    LogAction($"Файл {Path.GetFileName(filePath)} удален.");
                    MessageBox.Show("Файл успешно удален.", "Удаление завершено",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    LogAction($"Ошибка при удалении файла: {ex.Message}");
                    MessageBox.Show($"Ошибка при удалении файла: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private async Task RescanFile(string filePath)
        {
            try
            {
                UpdateStatus($"Запущено пересканирование файла: {Path.GetFileName(filePath)}");
                LogAction($"Запущено пересканирование файла: {Path.GetFileName(filePath)}");
                _cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _cancellationTokenSource.Token;

                ((IProgress<int>)_progress).Report(10);

                // Загрузка сканера
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

                // Сканирование
                var imageFile = await Task.Run(() =>
                {
                    ((IProgress<int>)_progress).Report(50);
                    var file = (ImageFile)item.Transfer("{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}");
                    ((IProgress<int>)_progress).Report(80);
                    return file;
                }, cancellationToken);

                // Удаляем существующий файл перед сохранением
                if (File.Exists(filePath))
                {
                    this.Invoke(new Action(() =>
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
                    }));

                    File.Delete(filePath);
                    await Task.Delay(100);
                }

                // Сохранение с перезаписью существующего файла
                await Task.Run(() => imageFile.SaveFile(filePath), cancellationToken);
                ((IProgress<int>)_progress).Report(90);

                this.Invoke(new Action(() =>
                {
                    string casePath = Path.GetDirectoryName(filePath);
                    flowLayoutPanel1.Controls.Clear();
                    LoadFolderContentsAsync(casePath);
                    UpdateStatus($"Файл {Path.GetFileName(filePath)} успешно пересканирован.");
                    LogAction($"Файл {Path.GetFileName(filePath)} успешно пересканирован.");
                    MessageBox.Show("Файл успешно пересканирован и сохранен!", "Готово");
                }));

                ((IProgress<int>)_progress).Report(100);
                await Task.Delay(500);
                ((IProgress<int>)_progress).Report(0);
            }
            catch (OperationCanceledException)
            {
                ((IProgress<int>)_progress).Report(0);
                UpdateStatus("Пересканирование отменено.");
                LogAction("Пересканирование отменено.");
                MessageBox.Show("Сканирование отменено");
            }
            catch (Exception ex)
            {
                ((IProgress<int>)_progress).Report(0);
                UpdateStatus($"Ошибка при пересканировании файла: {Path.GetFileName(filePath)}");
                LogAction($"Ошибка при пересканировании файла: {ex.Message}");
                MessageBox.Show($"Ошибка при пересканировании: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private bool IsImageFile(string extension)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp", ".heic" };
            return imageExtensions.Contains(extension.ToLower());
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
                    string yearPath = Path.Combine(dataScanPath, year.ToString());
                    Directory.CreateDirectory(yearPath);
                }

                txtPath.Text = dataScanPath;
                Properties.Settings.Default.DataScanPath = dataScanPath;
                Properties.Settings.Default.Save();

                UpdateStatus($"Создана структура папок в каталоге: {dataScanPath}");
                LogAction($"Создана структура папок в каталоге: {dataScanPath}");
                LoadYears();
                MessageBox.Show($"Структура успешно создана в папке:\n{dataScanPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании структуры:\n{ex.Message}",
                               "Ошибка",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
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
                        cbDriver.Items.Add(device.Properties["Name"].get_Value());
                    }
                }

                if (cbDriver.Items.Count == 0)
                    cbDriver.Items.Add("Сканеры не найдены");

                cbDriver.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка WIA: {ex.Message}");
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            LoadScanners();
        }

        private async void btnNewProfile_Click(object sender, EventArgs e)
        {
            if (treeYears.SelectedNode == null)
            {
                MessageBox.Show("Сначала выберите год в дереве!");
                return;
            }

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

            var txtNumber = new TextBox
            {
                Location = new Point(150, 55),
                Size = new Size(200, 20)
            };

            var lblFIO = new Label
            {
                Text = "ФИО:",
                Location = new Point(20, 100),
                AutoSize = true
            };

            var txtFIO = new TextBox
            {
                Location = new Point(150, 95),
                Size = new Size(200, 20)
            };

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

            form.Controls.Add(lblYear);
            form.Controls.Add(lblNumber);
            form.Controls.Add(txtNumber);
            form.Controls.Add(lblFIO);
            form.Controls.Add(txtFIO);
            form.Controls.Add(btnOK);
            form.Controls.Add(btnCancel);

            if (form.ShowDialog() == DialogResult.OK)
            {
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

                    // Принудительно обновляем списки
                    await InvokeAsync(() =>
                    {
                        // Снимаем и возвращаем выделение буквы для перезагрузки дел
                        if (listLetters.SelectedItems.Count > 0)
                        {
                            var letterItem = listLetters.SelectedItems[0];
                            letterItem.Selected = false;
                            letterItem.Selected = true;
                        }
                    });

                    // Ждем завершения загрузки дел
                    await WaitForListCasesLoad();

                    // Ищем созданное дело с нормализацией путей
                    var caseItem = listCases.Items.Cast<ListViewItem>()
                        .FirstOrDefault(item =>
                            Path.GetFullPath(item.Tag.ToString()).Equals(
                                Path.GetFullPath(caseFolder),
                                StringComparison.OrdinalIgnoreCase
                            ));

                    if (caseItem != null)
                    {
                        await InvokeAsync(() =>
                        {
                            caseItem.Selected = true;
                            caseItem.Focused = true;
                            listCases.EnsureVisible(caseItem.Index);
                            listCases.Focus();
                            ListCases_SelectedIndexChanged(null, EventArgs.Empty);
                        });
                    }
                    UpdateStatus($"Создано новое дело: №{caseNumber:D3} {txtFIO.Text.Trim()} в папке {caseFolder}");
                    LogAction($"Создано новое дело: №{caseNumber:D3} {txtFIO.Text.Trim()} в папке {caseFolder}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }
        private async Task InvokeAsync(Action action)
        {
            try
            {
                if (IsDisposed || Disposing) return;

                if (InvokeRequired)
                    await Task.Run(() => Invoke(action));
                else
                    action();
            }
            catch (ObjectDisposedException)
            {
                // Игнорируем ошибки при закрытии формы
            }
        }

        private async Task WaitForListCasesLoad()
        {
            while (true)
            {
                await Task.Delay(50);
                if (!IsCasesLoading()) break;
            }
        }

        private bool IsCasesLoading()
        {
            // Проверяем, идет ли загрузка дел
            return listCases.Items.Cast<ListViewItem>()
                       .Any(item => item.Text == "Loading...") ||
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

                var savePath = listCases.SelectedItems[0].Tag.ToString();
                var caseFolder = Path.GetFileName(savePath);
                var fio = caseFolder.Split(new[] { ' ' }, 2).Last();

                UpdateStatus($"Запущено сканирование дела: {fio}");
                LogAction($"Запущено сканирование дела: {fio}");

                // Новая логика поиска пропущенных номеров
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

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                await Task.Run(() => imageFile.SaveFile(fullPath), cancellationToken);
                ((IProgress<int>)_progress).Report(90);

                this.Invoke(new Action(() =>
                {
                    flowLayoutPanel1.Controls.Clear();
                    LoadFolderContentsAsync(savePath);
                    UpdateStatus($"Сканирование дела {fio} завершено. Файл сохранен: {newFileName}");
                    LogAction($"Сканирование дела {fio} завершено. Файл сохранен: {newFileName}");

                }));

                ((IProgress<int>)_progress).Report(100);
                await Task.Delay(500);
                ((IProgress<int>)_progress).Report(0);
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

        private void txtPath_TextChanged(object sender, EventArgs e)
        {
            LoadYears();
        }
        private void UpdateStatus(string message)
        {
            if (lableStatus.InvokeRequired)
            {
                lableStatus.Invoke(new Action(() => lableStatus.Text = message));
                Console.WriteLine("lableA");
            }
            else
            {
                Console.WriteLine("lableB");
                lableStatus.Text = message;
            }
        }

        private void lableStatus_Click(object sender, EventArgs e)
        {

        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Пример горячих клавиш
            if (e.Control && e.KeyCode == Keys.A) // Ctrl + N
            {
                btnNewProfile_Click(this, EventArgs.Empty); // Создание дела
            }
            else if (e.Control && e.KeyCode == Keys.S) // Ctrl + O
            {
                btnNewListProfile_Click(this, EventArgs.Empty); // Сканирование страницы
            }
            else if (e.Control && e.KeyCode == Keys.D) // Ctrl + S
            {
                btnDelProfile_Click(this, EventArgs.Empty); // Удаление дела
            }
        }

        private void btnDelProfile_Click(object sender, EventArgs e)
        {
            if (listCases.SelectedItems.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите дело для удаления.", "Удаление дела", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedItem = listCases.SelectedItems[0];
            var casePath = selectedItem.Tag?.ToString();

            if (string.IsNullOrEmpty(casePath) || !Directory.Exists(casePath))
            {
                MessageBox.Show("Путь к выбранному делу недействителен или дело уже удалено.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirmation = MessageBox.Show($"Вы уверены, что хотите удалить дело \"{selectedItem.Text}\"?\nВсе файлы и папки внутри будут удалены безвозвратно.",
                                               "Подтверждение удаления",
                                               MessageBoxButtons.YesNo,
                                               MessageBoxIcon.Question);

            if (confirmation == DialogResult.Yes)
            {
                try
                {
                    // Удаляем папку дела
                    Directory.Delete(casePath, true);

                    // Удаляем элемент из списка
                    listCases.Items.Remove(selectedItem);

                    LogAction($"Дело \"{selectedItem.Text}\" удалено.");
                    MessageBox.Show("Дело успешно удалено.", "Удаление завершено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    LogAction($"Ошибка при удалении дела \"{selectedItem.Text}\": {ex.Message}");
                    MessageBox.Show($"Ошибка при удалении дела: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void LogAction(string message)
        {
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action(() => LogAction(message)));
            }
            else
            {
                string timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                rtbLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
                rtbLog.ScrollToCaret(); // Автопрокрутка к последней записи
            }
        }

        private void btSearch_Click(object sender, EventArgs e)
        {
            string searchText = tbSearch.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Введите текст для поиска.", "Поиск", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Сбрасываем выделение
            treeYears.SelectedNode = null;
            listLetters.SelectedItems.Clear();
            listCases.SelectedItems.Clear();

            // Проверяем, существует ли путь
            if (!Directory.Exists(txtPath.Text))
            {
                MessageBox.Show("Указанный путь не существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Ищем дело в файловой системе
            string foundCasePath = null;
            string foundLetterPath = null;
            string foundYearPath = null;

            foreach (var yearDir in Directory.GetDirectories(txtPath.Text))
            {
                foreach (var letterDir in Directory.GetDirectories(yearDir))
                {
                    foreach (var caseDir in Directory.GetDirectories(letterDir))
                    {
                        if (Path.GetFileName(caseDir).IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            foundCasePath = caseDir;
                            foundLetterPath = letterDir;
                            foundYearPath = yearDir;
                            break;
                        }
                    }

                    if (foundCasePath != null) break;
                }

                if (foundCasePath != null) break;
            }

            if (foundCasePath == null)
            {
                MessageBox.Show("Ничего не найдено.", "Поиск", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Отображаем найденные элементы
            var yearNode = treeYears.Nodes.Cast<TreeNode>().FirstOrDefault(node => node.Tag.ToString() == foundYearPath);
            if (yearNode != null)
            {
                treeYears.SelectedNode = yearNode;
                treeYears.Focus();
                TreeYears_AfterSelect(treeYears, new TreeViewEventArgs(yearNode));
            }

            var letterItem = listLetters.Items.Cast<ListViewItem>().FirstOrDefault(item => item.Tag.ToString() == foundLetterPath);
            if (letterItem != null)
            {
                letterItem.Selected = true;
                letterItem.Focused = true;
                ListLetters_SelectedIndexChanged(listLetters, EventArgs.Empty);
            }

            var caseItem = listCases.Items.Cast<ListViewItem>().FirstOrDefault(item => item.Tag.ToString() == foundCasePath);
            if (caseItem != null)
            {
                caseItem.Selected = true;
                caseItem.Focused = true;
                listCases.Focus();
            }
        }
    }
}