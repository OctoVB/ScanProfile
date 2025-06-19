namespace ScanProfile
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MainTab = new System.Windows.Forms.TabControl();
            this.MainMenu = new System.Windows.Forms.TabPage();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.listCases = new System.Windows.Forms.ListView();
            this.listLetters = new System.Windows.Forms.ListView();
            this.treeYears = new System.Windows.Forms.TreeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnDelProfile = new System.Windows.Forms.Button();
            this.btnChange = new System.Windows.Forms.Button();
            this.btnNewListProfile = new System.Windows.Forms.Button();
            this.btnNewProfile = new System.Windows.Forms.Button();
            this.Settings = new System.Windows.Forms.TabPage();
            this.panel4 = new System.Windows.Forms.Panel();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.iconSet = new System.Windows.Forms.ImageList(this.components);
            this.cbDPI = new System.Windows.Forms.ComboBox();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.cbDriver = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnFindPath = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.cbPepperType = new System.Windows.Forms.ComboBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lableStatus = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.MainTab.SuspendLayout();
            this.MainMenu.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.Settings.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTab
            // 
            this.MainTab.Controls.Add(this.MainMenu);
            this.MainTab.Controls.Add(this.Settings);
            resources.ApplyResources(this.MainTab, "MainTab");
            this.MainTab.HotTrack = true;
            this.MainTab.Name = "MainTab";
            this.MainTab.SelectedIndex = 0;
            this.MainTab.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            // 
            // MainMenu
            // 
            this.MainMenu.Controls.Add(this.flowLayoutPanel1);
            this.MainMenu.Controls.Add(this.listCases);
            this.MainMenu.Controls.Add(this.listLetters);
            this.MainMenu.Controls.Add(this.treeYears);
            this.MainMenu.Controls.Add(this.panel1);
            resources.ApplyResources(this.MainMenu, "MainMenu");
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // listCases
            // 
            resources.ApplyResources(this.listCases, "listCases");
            this.listCases.HideSelection = false;
            this.listCases.Name = "listCases";
            this.listCases.UseCompatibleStateImageBehavior = false;
            // 
            // listLetters
            // 
            resources.ApplyResources(this.listLetters, "listLetters");
            this.listLetters.HideSelection = false;
            this.listLetters.Name = "listLetters";
            this.listLetters.UseCompatibleStateImageBehavior = false;
            // 
            // treeYears
            // 
            resources.ApplyResources(this.treeYears, "treeYears");
            this.treeYears.Name = "treeYears";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Controls.Add(this.btnDelProfile);
            this.panel2.Controls.Add(this.btnChange);
            this.panel2.Controls.Add(this.btnNewListProfile);
            this.panel2.Controls.Add(this.btnNewProfile);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.RoyalBlue;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnDelProfile
            // 
            this.btnDelProfile.BackColor = System.Drawing.Color.RoyalBlue;
            resources.ApplyResources(this.btnDelProfile, "btnDelProfile");
            this.btnDelProfile.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btnDelProfile.Name = "btnDelProfile";
            this.toolTip1.SetToolTip(this.btnDelProfile, resources.GetString("btnDelProfile.ToolTip"));
            this.btnDelProfile.UseVisualStyleBackColor = false;
            this.btnDelProfile.Click += new System.EventHandler(this.btnDelProfile_Click);
            // 
            // btnChange
            // 
            this.btnChange.BackColor = System.Drawing.Color.RoyalBlue;
            resources.ApplyResources(this.btnChange, "btnChange");
            this.btnChange.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btnChange.Name = "btnChange";
            this.toolTip1.SetToolTip(this.btnChange, resources.GetString("btnChange.ToolTip"));
            this.btnChange.UseVisualStyleBackColor = false;
            this.btnChange.Click += new System.EventHandler(this.btnChange_Click);
            // 
            // btnNewListProfile
            // 
            this.btnNewListProfile.BackColor = System.Drawing.Color.RoyalBlue;
            resources.ApplyResources(this.btnNewListProfile, "btnNewListProfile");
            this.btnNewListProfile.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btnNewListProfile.Name = "btnNewListProfile";
            this.toolTip1.SetToolTip(this.btnNewListProfile, resources.GetString("btnNewListProfile.ToolTip"));
            this.btnNewListProfile.UseVisualStyleBackColor = false;
            this.btnNewListProfile.Click += new System.EventHandler(this.btnNewListProfile_Click);
            // 
            // btnNewProfile
            // 
            this.btnNewProfile.BackColor = System.Drawing.Color.RoyalBlue;
            resources.ApplyResources(this.btnNewProfile, "btnNewProfile");
            this.btnNewProfile.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btnNewProfile.Name = "btnNewProfile";
            this.toolTip1.SetToolTip(this.btnNewProfile, resources.GetString("btnNewProfile.ToolTip"));
            this.btnNewProfile.UseVisualStyleBackColor = false;
            this.btnNewProfile.Click += new System.EventHandler(this.btnNewProfile_Click);
            // 
            // Settings
            // 
            this.Settings.Controls.Add(this.panel4);
            this.Settings.Controls.Add(this.panel3);
            resources.ApplyResources(this.Settings, "Settings");
            this.Settings.Name = "Settings";
            this.Settings.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.rtbLog);
            this.panel4.Controls.Add(this.textBox1);
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Name = "panel4";
            // 
            // rtbLog
            // 
            resources.ApplyResources(this.rtbLog, "rtbLog");
            this.rtbLog.Name = "rtbLog";
            // 
            // textBox1
            // 
            resources.ApplyResources(this.textBox1, "textBox1");
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.btnUpdate);
            this.panel3.Controls.Add(this.cbDPI);
            this.panel3.Controls.Add(this.txtPath);
            this.panel3.Controls.Add(this.btnCreate);
            this.panel3.Controls.Add(this.cbDriver);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Controls.Add(this.btnFindPath);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.cbPepperType);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // btnUpdate
            // 
            resources.ApplyResources(this.btnUpdate, "btnUpdate");
            this.btnUpdate.ImageList = this.iconSet;
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // iconSet
            // 
            this.iconSet.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("iconSet.ImageStream")));
            this.iconSet.TransparentColor = System.Drawing.Color.Transparent;
            this.iconSet.Images.SetKeyName(0, "Dot");
            this.iconSet.Images.SetKeyName(1, "Delete");
            this.iconSet.Images.SetKeyName(2, "Scaner");
            this.iconSet.Images.SetKeyName(3, "Update");
            // 
            // cbDPI
            // 
            resources.ApplyResources(this.cbDPI, "cbDPI");
            this.cbDPI.FormattingEnabled = true;
            this.cbDPI.Items.AddRange(new object[] {
            resources.GetString("cbDPI.Items"),
            resources.GetString("cbDPI.Items1"),
            resources.GetString("cbDPI.Items2"),
            resources.GetString("cbDPI.Items3"),
            resources.GetString("cbDPI.Items4"),
            resources.GetString("cbDPI.Items5"),
            resources.GetString("cbDPI.Items6"),
            resources.GetString("cbDPI.Items7"),
            resources.GetString("cbDPI.Items8"),
            resources.GetString("cbDPI.Items9"),
            resources.GetString("cbDPI.Items10"),
            resources.GetString("cbDPI.Items11"),
            resources.GetString("cbDPI.Items12")});
            this.cbDPI.Name = "cbDPI";
            this.cbDPI.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
            // 
            // txtPath
            // 
            this.txtPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.txtPath, "txtPath");
            this.txtPath.Name = "txtPath";
            this.txtPath.TextChanged += new System.EventHandler(this.txtPath_TextChanged);
            // 
            // btnCreate
            // 
            this.btnCreate.BackColor = System.Drawing.Color.RoyalBlue;
            resources.ApplyResources(this.btnCreate, "btnCreate");
            this.btnCreate.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.UseVisualStyleBackColor = false;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // cbDriver
            // 
            resources.ApplyResources(this.cbDriver, "cbDriver");
            this.cbDriver.FormattingEnabled = true;
            this.cbDriver.Name = "cbDriver";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // btnFindPath
            // 
            this.btnFindPath.BackColor = System.Drawing.Color.RoyalBlue;
            resources.ApplyResources(this.btnFindPath, "btnFindPath");
            this.btnFindPath.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btnFindPath.Name = "btnFindPath";
            this.btnFindPath.UseVisualStyleBackColor = false;
            this.btnFindPath.Click += new System.EventHandler(this.btnFindPath_Click);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // cbPepperType
            // 
            resources.ApplyResources(this.cbPepperType, "cbPepperType");
            this.cbPepperType.FormattingEnabled = true;
            this.cbPepperType.Items.AddRange(new object[] {
            resources.GetString("cbPepperType.Items")});
            this.cbPepperType.Name = "cbPepperType";
            this.cbPepperType.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Name = "progressBar1";
            // 
            // lableStatus
            // 
            resources.ApplyResources(this.lableStatus, "lableStatus");
            this.lableStatus.BackColor = System.Drawing.Color.Transparent;
            this.lableStatus.Name = "lableStatus";
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightGray;
            this.Controls.Add(this.MainTab);
            this.Controls.Add(this.lableStatus);
            this.Controls.Add(this.progressBar1);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.MainTab.ResumeLayout(false);
            this.MainMenu.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.Settings.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl MainTab;
        private System.Windows.Forms.TabPage MainMenu;
        private System.Windows.Forms.TabPage Settings;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbDPI;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbPepperType;
        private System.Windows.Forms.ComboBox cbDriver;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnFindPath;
        private System.Windows.Forms.ImageList iconSet;
        private System.Windows.Forms.Label lableStatus;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.ListView listCases;
        private System.Windows.Forms.ListView listLetters;
        private System.Windows.Forms.TreeView treeYears;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnDelProfile;
        private System.Windows.Forms.Button btnNewListProfile;
        private System.Windows.Forms.Button btnNewProfile;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnChange;
    }
}

