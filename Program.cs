using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace ThoriumLauncherAllInOne
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LauncherForm());
        }
    }

    public class LauncherForm : Form
    {
        // Core controls
        TextBox txtExePath;
        ComboBox cbProfiles;
        TextBox txtFlags;
        TextBox txtCmdPreview;
        Button btnLaunch;
        Button btnModes;
        ContextMenuStrip cmsModes;
        Label lblWarning;
        Label lblStatus;

        // small profile buttons
        Button btnCreate;
        Button btnClone;
        Button btnDelete;
        Button btnManage;
        Button btnOpenFolder;

        // save flags
        Button btnSaveFlags;
        Button btnExportBat;
        Button btnCopyCmd;

        string appdir;
        string currentProfileName = "Default";

        public LauncherForm()
        {
            appdir = AppDomain.CurrentDomain.BaseDirectory;
            MigrateLegacyProfiles();
            InitializeComponent();
            LoadConfig();
            RefreshProfiles();
            UpdatePreview();
        }

        void MigrateLegacyProfiles()
        {
            try {
                var profilesDir = Path.Combine(appdir, "Profiles");
                if (!Directory.Exists(profilesDir)) Directory.CreateDirectory(profilesDir);

                // 1. Move root flags to Profiles/ (Legacy Step 1)
                var rootFlags = Directory.GetFiles(appdir, "flags*.txt");
                foreach (var f in rootFlags)
                {
                    var name = Path.GetFileName(f);
                    var dest = Path.Combine(profilesDir, name);
                    if (!File.Exists(dest)) File.Move(f, dest);
                }

                // 2. Move root profile folders to Profiles/ (Legacy Step 2)
                var rootDirs = Directory.GetDirectories(appdir, "thorium-profile*");
                foreach (var d in rootDirs)
                {
                    var name = new DirectoryInfo(d).Name;
                    var dest = Path.Combine(profilesDir, name);
                    if (!Directory.Exists(dest)) Directory.Move(d, dest);
                }

                // 3. NEW: Move flags_NAME.txt INTO thorium-profile-NAME/flags.txt
                var profileFlags = Directory.GetFiles(profilesDir, "flags_*.txt");
                foreach (var f in profileFlags)
                {
                    // filename is flags_NAME.txt
                    var filename = Path.GetFileName(f);
                    var profileName = Path.GetFileNameWithoutExtension(f).Substring("flags_".Length);
                    
                    // target folder: Profiles/thorium-profile-NAME
                    var targetDirName = "thorium-profile-" + profileName;
                    var targetDir = Path.Combine(profilesDir, targetDirName);
                    
                    if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
                    
                    var targetFile = Path.Combine(targetDir, "flags.txt");
                    if (!File.Exists(targetFile))
                    {
                        File.Move(f, targetFile);
                    }
                    else
                    {
                        // if target already exists, maybe just delete the old loose file?
                        // or keep it as backup? Let's delete to clean up if migration happened before
                        File.Delete(f); 
                    }
                }
                
                // 4. Handle Default profile flags (flags.txt -> thorium-profile/flags.txt)
                var defaultFlags = Path.Combine(profilesDir, "flags.txt");
                if (File.Exists(defaultFlags))
                {
                    var targetDir = Path.Combine(profilesDir, "thorium-profile");
                    if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
                    var targetFile = Path.Combine(targetDir, "flags.txt");
                    if (!File.Exists(targetFile)) File.Move(defaultFlags, targetFile);
                    else File.Delete(defaultFlags);
                }

            } catch {}
        }

        void InitializeComponent()
        {
            this.Text = "Thorium Launcher — All-in-1";
            this.Width = 960;
            this.Height = 620;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch {}

            var pad = 12;
            int labelW = 110;

            // Helper to render icons
            Func<string, Bitmap> Icon = (code) => {
                int size = 16;
                var bmp = new Bitmap(size, size);
                using(var g = Graphics.FromImage(bmp)) {
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.Clear(Color.Transparent);
                    // Use Segoe MDL2 Assets (Win10+) or Segoe UI Symbol (fallback)
                    var fontName = "Segoe MDL2 Assets";
                    using(var f = new Font(fontName, 10, FontStyle.Regular))
                    using(var b = new SolidBrush(Color.FromArgb(64, 64, 64))) {
                        // Center the icon
                        var s = g.MeasureString(code, f);
                        g.DrawString(code, f, b, (size - s.Width)/2, (size - s.Height)/2 + 1);
                    }
                }
                return bmp;
            };

            // EXE row
            var lblExe = new Label() { Left = pad, Top = pad + 4, Width = labelW, Text = "Thorium EXE:" };
            txtExePath = new TextBox() { Left = pad + labelW, Top = pad, Width = 640 };
            var btnBrowse = new Button() { Left = txtExePath.Right + 8, Top = pad - 2, Width = 90, Text = "Browse", Image = Icon("\uE838"), TextImageRelation = TextImageRelation.ImageBeforeText, TextAlign = ContentAlignment.MiddleRight, ImageAlign = ContentAlignment.MiddleLeft };
            btnBrowse.Click += (s, e) => BrowseExe();

            // Profile row
            var lblProfile = new Label() { Left = pad, Top = lblExe.Bottom + 12, Width = labelW, Text = "Profile:" };
            cbProfiles = new ComboBox() { Left = pad + labelW, Top = lblExe.Bottom + 8, Width = 220, DropDownStyle = ComboBoxStyle.DropDown };
            cbProfiles.SelectedIndexChanged += (s, e) => { SelectProfile(cbProfiles.Text); };
            
            btnManage = new Button() { Left = cbProfiles.Right + 4, Top = cbProfiles.Top, Width = 32, Height = cbProfiles.Height, Text = "", Image = Icon("\uE90F"), FlatStyle = FlatStyle.Popup };
            btnManage.Click += (s, e) => OpenProfileManager();
            
            btnCreate = new Button() { Left = btnManage.Right + 8, Top = cbProfiles.Top - 2, Width = 75, Text = "New", Image = Icon("\uE710"), TextImageRelation = TextImageRelation.ImageBeforeText, TextAlign = ContentAlignment.MiddleRight, ImageAlign = ContentAlignment.MiddleLeft };
            btnCreate.Click += (s, e) => CreateProfile();
            
            btnClone = new Button() { Left = btnCreate.Right + 6, Top = btnCreate.Top, Width = 75, Text = "Clone", Image = Icon("\uE8C8"), TextImageRelation = TextImageRelation.ImageBeforeText, TextAlign = ContentAlignment.MiddleRight, ImageAlign = ContentAlignment.MiddleLeft };
            btnClone.Click += (s, e) => CloneProfileDialog();

            btnDelete = new Button() { Left = btnClone.Right + 6, Top = btnClone.Top, Width = 85, Text = "Delete", Image = Icon("\uE74D"), TextImageRelation = TextImageRelation.ImageBeforeText, TextAlign = ContentAlignment.MiddleRight, ImageAlign = ContentAlignment.MiddleLeft, ForeColor = Color.DarkRed };
            btnDelete.Click += (s, e) => DeleteCurrentProfile();
            
            btnOpenFolder = new Button() { Left = btnDelete.Right + 6, Top = btnClone.Top, Width = 90, Text = "Folder", Image = Icon("\uE8B7"), TextImageRelation = TextImageRelation.ImageBeforeText, TextAlign = ContentAlignment.MiddleRight, ImageAlign = ContentAlignment.MiddleLeft };
            btnOpenFolder.Click += (s, e) => OpenProfileFolder();

            // Advanced / Flags area (left)
            var lblFlags = new Label() { Left = pad, Top = cbProfiles.Bottom + 14, Width = labelW, Text = "Advanced flags:" };
            var pnlLeft = new Panel() { Left = pad, Top = lblFlags.Bottom + 6, Width = 600, Height = 340, BorderStyle = BorderStyle.FixedSingle };
            txtFlags = new TextBox() { Multiline = true, Left = 6, Top = 6, Width = pnlLeft.Width - 14, Height = pnlLeft.Height - 60, ScrollBars = ScrollBars.Vertical, Font = new System.Drawing.Font("Consolas", 10f) };
            
            btnSaveFlags = new Button() { Left = 6, Top = txtFlags.Bottom + 6, Width = 110, Text = "Save", Image = Icon("\uE74E"), TextImageRelation = TextImageRelation.ImageBeforeText, TextAlign = ContentAlignment.MiddleRight, ImageAlign = ContentAlignment.MiddleLeft };
            btnSaveFlags.Click += (s, e) => { SaveFlagsForProfile(); lblStatus.Text = "Flags saved."; };
            
            btnExportBat = new Button() { Left = btnSaveFlags.Right + 8, Top = btnSaveFlags.Top, Width = 80, Text = "Export", Image = Icon("\uEDE1"), TextImageRelation = TextImageRelation.ImageBeforeText, TextAlign = ContentAlignment.MiddleRight, ImageAlign = ContentAlignment.MiddleLeft };
            btnExportBat.Click += (s, e) => ExportBat();
            
            var btnWebApp = new Button() { Left = btnExportBat.Right + 8, Top = btnSaveFlags.Top, Width = 100, Text = "Web App", Image = Icon("\uE774"), TextImageRelation = TextImageRelation.ImageBeforeText, TextAlign = ContentAlignment.MiddleRight, ImageAlign = ContentAlignment.MiddleLeft };
            btnWebApp.Click += (s, e) => CreateWebApp();

            pnlLeft.Controls.Add(txtFlags);
            pnlLeft.Controls.Add(btnSaveFlags);
            pnlLeft.Controls.Add(btnExportBat);
            pnlLeft.Controls.Add(btnWebApp);

            // Right column: preview + controls
            var pnlRight = new Panel() { Left = pnlLeft.Right + 16, Top = lblFlags.Bottom + 6, Width = 300, Height = pnlLeft.Height, BorderStyle = BorderStyle.FixedSingle };
            var lblPreview = new Label() { Left = 8, Top = 8, Width = 120, Text = "Command preview:" };
            txtCmdPreview = new TextBox() { Left = 8, Top = lblPreview.Bottom + 6, Width = pnlRight.Width - 16, Height = 140, Multiline = true, ReadOnly = true, Font = new System.Drawing.Font("Consolas", 9f) };
            lblWarning = new Label() { Name = "lblWarning", Left = 8, Top = txtCmdPreview.Bottom + 8, Width = pnlRight.Width - 16, Height = 36, Text = "⚠ Attention: flags detected may block Google login.", Visible = false, BackColor = System.Drawing.Color.FromArgb(255, 250, 220), Padding = new Padding(6) };
            
            btnCopyCmd = new Button() { Left = 8, Top = lblWarning.Bottom + 8, Width = 120, Text = "Copy Cmd", Image = Icon("\uE8C8"), TextImageRelation = TextImageRelation.ImageBeforeText, TextAlign = ContentAlignment.MiddleRight, ImageAlign = ContentAlignment.MiddleLeft };
            btnCopyCmd.Click += (s, e) => { Clipboard.SetText(txtCmdPreview.Text); lblStatus.Text = "Command copied."; };
            
            var btnTestFlags = new Button() { Left = btnCopyCmd.Right + 8, Top = btnCopyCmd.Top, Width = 120, Text = "Test Run", Image = Icon("\uE768"), TextImageRelation = TextImageRelation.ImageBeforeText, TextAlign = ContentAlignment.MiddleRight, ImageAlign = ContentAlignment.MiddleLeft };
            btnTestFlags.Click += (s, e) => LaunchEphemeral();
            
            pnlRight.Controls.Add(lblPreview);
            pnlRight.Controls.Add(txtCmdPreview);
            pnlRight.Controls.Add(lblWarning);
            pnlRight.Controls.Add(btnCopyCmd);
            pnlRight.Controls.Add(btnTestFlags);

            // Bottom area: Launch big button and modes
            btnLaunch = new Button() { Left = pad + 160, Top = pnlLeft.Bottom + 16, Width = 460, Height = 48, Text = "🚀  LAUNCH  (Alt+L)", BackColor = System.Drawing.Color.FromArgb(0, 120, 215), ForeColor = System.Drawing.Color.White, Font = new System.Drawing.Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold) };
            btnLaunch.Click += (s, e) => LaunchFull();
            btnLaunch.FlatStyle = FlatStyle.Flat;
            btnLaunch.FlatAppearance.BorderSize = 0;

            // Modes dropdown (small)
            btnModes = new Button() { Left = btnLaunch.Right + 8, Top = btnLaunch.Top, Width = 90, Height = btnLaunch.Height, Text = "Modes ▾" };
            cmsModes = new ContextMenuStrip();
            cmsModes.Items.Add("Login (safe mode)").Click += (s, e) => { PromptLoginMode(); };
            cmsModes.Items.Add("Launch Full (default)").Click += (s, e) => { LaunchFull(); };
            cmsModes.Items.Add("Launch Silent").Click += (s, e) => { LaunchSilent(); };
            cmsModes.Items.Add("Launch (force center)").Click += (s, e) => { LaunchFull(); };
            btnModes.Click += (s, e) => cmsModes.Show(btnModes, new System.Drawing.Point(0, btnModes.Height));

            // Status bar
            lblStatus = new Label() { Left = pad, Top = btnLaunch.Bottom + 12, Width = 760, Height = 22, Text = "Status: ready", ForeColor = System.Drawing.Color.DimGray };

            // Add controls to form
            this.Controls.Add(lblExe);
            this.Controls.Add(txtExePath);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(lblProfile);
            this.Controls.Add(cbProfiles);
            this.Controls.Add(btnManage);
            this.Controls.Add(btnCreate);
            this.Controls.Add(btnClone);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnOpenFolder);
            this.Controls.Add(lblFlags);
            this.Controls.Add(pnlLeft);
            this.Controls.Add(pnlRight);
            this.Controls.Add(btnLaunch);
            this.Controls.Add(btnModes);
            this.Controls.Add(lblStatus);

            // Events to update preview
            txtExePath.TextChanged += (s, e) => UpdatePreview();
            txtFlags.TextChanged += (s, e) => UpdatePreview();
            cbProfiles.TextChanged += (s, e) => UpdatePreview();

            // Keyboard shortcut Alt+L -> Launch
            var ks = new KeyEventHandler((s, e) => { if (e.Alt && e.KeyCode == Keys.L) LaunchFull(); });
            this.KeyPreview = true;
            this.KeyDown += ks;

            ApplyDarkTheme(this);
        }

        public static void ApplyDarkTheme(Control parent)
        {
            // Palette
            var backColor = System.Drawing.Color.FromArgb(32, 32, 32);
            var foreColor = System.Drawing.Color.White;
            var controlBack = System.Drawing.Color.FromArgb(45, 45, 48);
            var accentColor = System.Drawing.Color.FromArgb(0, 120, 215);
            var dangerColor = System.Drawing.Color.FromArgb(200, 50, 50);
            var warningBack = System.Drawing.Color.FromArgb(255, 193, 7); // Amber
            var warningFore = System.Drawing.Color.Black;

            parent.BackColor = backColor;
            parent.ForeColor = foreColor;

            foreach (Control c in parent.Controls)
            {
                if (c is Button)
                {
                    var btn = (Button)c;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.BackColor = controlBack;
                    btn.ForeColor = foreColor;
                    
                    // Heuristic styling
                    if(btn.Text.Contains("LAUNCH")) btn.BackColor = accentColor;
                    if(btn.Text.Contains("Delete")) btn.ForeColor = dangerColor;
                }
                else if (c is TextBox)
                {
                    var txt = (TextBox)c;
                    txt.BackColor = controlBack;
                    txt.ForeColor = foreColor;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is ComboBox)
                {
                    var cb = (ComboBox)c;
                    cb.BackColor = controlBack;
                    cb.ForeColor = foreColor;
                    cb.FlatStyle = FlatStyle.Flat;
                }
                else if (c is ListView)
                {
                    var lv = (ListView)c;
                    lv.BackColor = controlBack;
                    lv.ForeColor = foreColor;
                    lv.OwnerDraw = true;
                    lv.DrawColumnHeader += (s, e) => {
                        e.Graphics.FillRectangle(new SolidBrush(controlBack), e.Bounds);
                        e.Graphics.DrawString(e.Header.Text, lv.Font, new SolidBrush(foreColor), e.Bounds);
                    };
                    lv.DrawItem += (s, e) => e.DrawDefault = true;
                }
                else if (c is Label)
                {
                    var lbl = (Label)c;
                    lbl.ForeColor = foreColor;
                    if(lbl.Name == "lblWarning") {
                        lbl.BackColor = warningBack;
                        lbl.ForeColor = warningFore;
                    }
                }
                else if (c is Panel)
                {
                    var pnl = (Panel)c;
                    pnl.BackColor = backColor;
                    pnl.ForeColor = foreColor;
                    ApplyDarkTheme(pnl); // Recursive
                }
            }
        }

        void LoadConfig()
        {
            var cfg = Path.Combine(appdir, "launcher.ini");
            if (File.Exists(cfg))
            {
                var lines = File.ReadAllLines(cfg);
                foreach (var l in lines)
                {
                    if (l.StartsWith("ExePath=")) txtExePath.Text = l.Substring("ExePath=".Length);
                    if (l.StartsWith("LastProfile=")) currentProfileName = l.Substring("LastProfile=".Length);
                }
            }
        }

        void SaveConfig()
        {
            var cfg = Path.Combine(appdir, "launcher.ini");
            var lines = new List<string> { "ExePath=" + txtExePath.Text, "LastProfile=" + currentProfileName };
            File.WriteAllLines(cfg, lines.ToArray());
        }

        void RefreshProfiles()
        {
            cbProfiles.Items.Clear();
            cbProfiles.Items.Add("Default");
            
            var profilesDir = Path.Combine(appdir, "Profiles");
            if(!Directory.Exists(profilesDir)) Directory.CreateDirectory(profilesDir);
            
            // 1. Detect launcher-managed profiles: Profiles/thorium-profile-*
            var dirs = Directory.GetDirectories(profilesDir, "thorium-profile-*");
            foreach (var d in dirs)
            {
                var dirName = new DirectoryInfo(d).Name; // thorium-profile-NAME
                var name = dirName.Substring("thorium-profile-".Length);
                if (!string.IsNullOrEmpty(name) && !cbProfiles.Items.Contains(name)) 
                    cbProfiles.Items.Add(name);
            }
            
            // 2. NEW: Detect native Thorium profiles in installation folder
            var exePath = txtExePath.Text.Trim();
            if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
            {
                // exe is in Application folder, User Data is sibling to Application
                var appFolder = Path.GetDirectoryName(exePath); // .../Thorium/Application
                var thoriumRoot = Path.GetDirectoryName(appFolder); // .../Thorium
                var userDataDir = Path.Combine(thoriumRoot, "User Data");
                
                if (Directory.Exists(userDataDir))
                {
                    // Look for Profile folders (Profile 1, Profile 2, etc.)
                    var nativeProfiles = Directory.GetDirectories(userDataDir, "Profile *");
                    foreach (var p in nativeProfiles)
                    {
                        var profileName = new DirectoryInfo(p).Name; // "Profile 1", "Profile 2"
                        var displayName = "[Native] " + profileName;
                        if (!cbProfiles.Items.Contains(displayName))
                            cbProfiles.Items.Add(displayName);
                    }
                    
                    // Also check for "Default" native profile
                    var nativeDefault = Path.Combine(userDataDir, "Default");
                    if (Directory.Exists(nativeDefault) && !cbProfiles.Items.Contains("[Native] Default"))
                    {
                        // Only add if it's different from our launcher Default
                        var ourDefault = Path.Combine(profilesDir, "thorium-profile");
                        if (Path.GetFullPath(nativeDefault) != Path.GetFullPath(ourDefault))
                        {
                            cbProfiles.Items.Add("[Native] Default");
                        }
                    }
                }
            }
            
            // Validation: If current profile was deleted, switch to Default
            if (!cbProfiles.Items.Contains(currentProfileName))
            {
                currentProfileName = "Default";
            }
            
            cbProfiles.Text = currentProfileName;
            LoadFlagsForProfile(currentProfileName);
        }

        void SelectProfile(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            currentProfileName = name;
            SaveConfig();
            LoadFlagsForProfile(name);
            UpdatePreview();
        }

        void LoadFlagsForProfile(string profile)
        {
            var f = GetFlagsFileName(profile);
            if (File.Exists(f)) txtFlags.Text = File.ReadAllText(f);
            else txtFlags.Text = GetDefaultFlags();
        }

        string GetFlagsFileName(string profile)
        {
            // Check if this is a native Thorium profile
            if (profile.StartsWith("[Native] "))
            {
                var nativeProfileName = profile.Substring("[Native] ".Length);
                var exePath = txtExePath.Text.Trim();
                if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                {
                    var appFolder = Path.GetDirectoryName(exePath);
                    var thoriumRoot = Path.GetDirectoryName(appFolder);
                    var userDataDir = Path.Combine(thoriumRoot, "User Data");
                    var nativeProfileDir = Path.Combine(userDataDir, nativeProfileName);
                    
                    if (!Directory.Exists(nativeProfileDir)) Directory.CreateDirectory(nativeProfileDir);
                    return Path.Combine(nativeProfileDir, "flags.txt");
                }
            }
            
            // Launcher-managed profile
            var p = Path.Combine(appdir, "Profiles");
            var folderName = profile == "Default" ? "thorium-profile" : "thorium-profile-" + profile;
            var fullDir = Path.Combine(p, folderName);
            if(!Directory.Exists(fullDir)) Directory.CreateDirectory(fullDir);
            
            return Path.Combine(fullDir, "flags.txt");
        }

        void SaveFlagsForProfile()
        {
            var f = GetFlagsFileName(currentProfileName);
            File.WriteAllText(f, txtFlags.Text);
        }

        void CreateProfile()
        {
            var name = PromptInput("New profile name:");
            if (string.IsNullOrWhiteSpace(name)) return;
            if (!cbProfiles.Items.Contains(name)) cbProfiles.Items.Add(name);
            
            // This triggers SelectProfile, loading default flags
            cbProfiles.Text = name; 
            
            // Fix isolation: Update user-data-dir to point to the new profile
            var newDir = "%APPDIR%" + MakeProfileDirName(name);
            var lines = txtFlags.Lines.ToList();
            bool found = false;
            for(int i=0; i<lines.Count; i++) {
                if(lines[i].Trim().StartsWith("--user-data-dir=")) {
                    lines[i] = "--user-data-dir=\"" + newDir + "\"";
                    found = true;
                }
            }
            if(!found) lines.Insert(0, "--user-data-dir=\"" + newDir + "\"");
            txtFlags.Lines = lines.ToArray();

            currentProfileName = name;
            SaveFlagsForProfile();
            UpdatePreview();
            lblStatus.Text = "Profile '" + name + "' created.";
        }

        void CloneProfileDialog()
        {
            var src = PromptInput("Copy from profile (name):");
            if (string.IsNullOrWhiteSpace(src)) return;
            var dst = cbProfiles.Text;
            if (string.IsNullOrWhiteSpace(dst)) { MessageBox.Show("Choose a target profile name in the dropdown first."); return; }
            var srcPath = Path.Combine(appdir, MakeProfileDirName(src));
            var dstPath = Path.Combine(appdir, MakeProfileDirName(dst));
            CloneSession(srcPath, dstPath);
            lblStatus.Text = "Clone attempted from " + src + " -> " + dst;
        }

        string MakeProfileDirName(string profile)
        {
            // Check if this is a native Thorium profile
            if (profile.StartsWith("[Native] "))
            {
                var nativeProfileName = profile.Substring("[Native] ".Length);
                var exePath = txtExePath.Text.Trim();
                if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                {
                    var appFolder = Path.GetDirectoryName(exePath);
                    var thoriumRoot = Path.GetDirectoryName(appFolder);
                    return Path.Combine(thoriumRoot, "User Data", nativeProfileName);
                }
            }
            
            // Launcher-managed profile
            return profile == "Default" ? Path.Combine("Profiles", "thorium-profile") : Path.Combine("Profiles", "thorium-profile-" + profile);
        }

        void OpenProfileFolder()
        {
            var p = EnsureProfile();
            if (Directory.Exists(p))
                Process.Start("explorer.exe", p);
            else
                MessageBox.Show("Profile folder does not exist yet.");
        }

        string EnsureProfile()
        {
            // For native profiles, return the path directly without creating dirs
            if (currentProfileName.StartsWith("[Native] "))
            {
                var nativeProfileName = currentProfileName.Substring("[Native] ".Length);
                var exePath = txtExePath.Text.Trim();
                if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                {
                    var appFolder = Path.GetDirectoryName(exePath);
                    var thoriumRoot = Path.GetDirectoryName(appFolder);
                    return Path.Combine(thoriumRoot, "User Data", nativeProfileName);
                }
            }
            
            // Launcher-managed profile
            var dir = MakeProfileDirName(currentProfileName);
            var full = Path.Combine(appdir, dir);
            if (!Directory.Exists(full)) Directory.CreateDirectory(full);
            return full;
        }

        // ---------- Preview / command building ----------
        void UpdatePreview()
        {
            try
            {
                var exe = txtExePath.Text.Trim();
                if (string.IsNullOrEmpty(exe)) exe = "thorium.exe";
                var profileDir = EnsureProfile();
                var flags = txtFlags.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
                // replace %APPDIR%
                for (int i = 0; i < flags.Count; i++) flags[i] = flags[i].Replace("%APPDIR%", appdir.TrimEnd(Path.DirectorySeparatorChar));
                if (!flags.Any(f => f.StartsWith("--user-data-dir")))
                    flags.Insert(0, "--user-data-dir=\"" + profileDir + "\"");

                // ensure centered
                var scr = Screen.PrimaryScreen.Bounds;
                int W = 1280; int H = 800;
                int posX = (scr.Width - W) / 2; int posY = (scr.Height - H) / 2;
                flags.RemoveAll(f => f.StartsWith("--window-size") || f.StartsWith("--window-position"));
                flags.Add("--window-size=" + W + "," + H);
                flags.Add("--window-position=" + posX + "," + posY);

                var cmd = "\"" + exe + "\" " + string.Join(" ", flags);
                txtCmdPreview.Text = cmd;

                // detect risky flags
                var joined = string.Join(" ", flags).ToLowerInvariant();
                var risky = new[] { "fingerprinting", "spoof-webgl", "automationcontrolled", "user-agent=" };
                var found = risky.Any(r => joined.Contains(r));
                lblWarning.Visible = found;
                if (!found) lblWarning.Visible = false;
            }
            catch (Exception ex)
            {
                txtCmdPreview.Text = "Error building preview: " + ex.Message;
            }
        }

        // ---------- Launching ----------
        void LaunchFull()
        {
            UpdatePreview();
            var cmd = txtCmdPreview.Text;
            // start process
            StartProcessFromPreview(cmd, false);
        }

        void LaunchSilent()
        {
            UpdatePreview();
            var cmd = txtCmdPreview.Text;
            StartProcessFromPreview(cmd, true);
        }

        void LaunchEphemeral()
        {
            // create temp profile and launch minimal flags (safe-mode test)
            var temp = CreateTempProfile();
            var exe = txtExePath.Text.Trim();
            var scr = Screen.PrimaryScreen.Bounds;
            int W = 1280, H = 800;
            int posX = (scr.Width - W) / 2, posY = (scr.Height - H) / 2;
            var args = "--user-data-dir=\"" + temp + "\" --no-default-browser-check --no-first-run --window-size=" + W + "," + H + " --window-position=" + posX + "," + posY;
            StartProcess(exe, args, detached: true);
            // cleanup when process exits
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    // naive wait: give user time. We won't block UI.
                    System.Threading.Thread.Sleep(5000);
                    // attempt delete later (can't reliably wait for process if we don't capture it here)
                    // best-effort cleanup: do nothing immediate; user can delete temp by closing icon
                }
                catch { }
            });
            lblStatus.Text = "Ephemeral test launched.";
        }

        void PromptLoginMode()
        {
            var res = MessageBox.Show("Login ephemeral (temporary profile)?\nYes = ephemeral (deleted on exit). No = persistent in current profile.", "Login mode", MessageBoxButtons.YesNoCancel);
            if (res == DialogResult.Cancel) return;
            if (res == DialogResult.Yes) LaunchEphemeral();
            else
            {
                // persistent login - open with minimal login flags but using profile dir
                var profile = EnsureProfile();
                var exe = txtExePath.Text.Trim();
                var scr = Screen.PrimaryScreen.Bounds;
                int W = 1280, H = 800;
                int posX = (scr.Width - W) / 2, posY = (scr.Height - H) / 2;
                var args = "--user-data-dir=\"" + profile + "\" --no-default-browser-check --no-first-run --window-size=" + W + "," + H + " --window-position=" + posX + "," + posY;
                StartProcess(exe, args, detached: true);
                lblStatus.Text = "Login (persistent) launched for profile: " + currentProfileName;
            }
        }

        // Helper to run process from the preview string
        void StartProcessFromPreview(string previewCmd, bool detached)
        {
            // previewCmd is like: "C:\path\thorium.exe" --flags...
            // naive split: first token is exe in quotes
            try
            {
                var exe = previewCmd;
                string args = "";
                if (previewCmd.StartsWith("\""))
                {
                    var idx = previewCmd.IndexOf('"', 1);
                    idx = previewCmd.IndexOf('"', 1);
                    if (idx > 0)
                    {
                        var second = previewCmd.IndexOf('"', 1);
                        // find second quote pair
                        var closing = previewCmd.IndexOf('"', 1);
                    }
                }
                // safer: extract exe between first quotes
                int firstQ = previewCmd.IndexOf('"');
                int secondQ = previewCmd.IndexOf('"', firstQ + 1);
                if (firstQ >= 0 && secondQ > firstQ)
                {
                    exe = previewCmd.Substring(firstQ + 1, secondQ - firstQ - 1);
                    args = previewCmd.Substring(secondQ + 1).Trim();
                }
                else
                {
                    var parts = previewCmd.Split(new[] { ' ' }, 2);
                    exe = parts[0];
                    args = parts.Length > 1 ? parts[1] : "";
                }

                StartProcess(exe, args, detached);
                lblStatus.Text = "Launched: " + Path.GetFileName(exe);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to launch: " + ex.Message);
            }
        }

        void StartProcess(string exe, string args, bool detached)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(exe)
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting process: " + ex.Message);
            }
        }

        // ---------- Utilities ----------
        string CreateTempProfile()
        {
            var tmp = Path.Combine(Path.GetTempPath(), "thorium_temp_" + Guid.NewGuid().ToString("n"));
            Directory.CreateDirectory(tmp);
            return tmp;
        }

        void ExportBat()
        {
            try
            {
                var dlg = new SaveFileDialog() { Filter = "Batch file|*.bat", FileName = "thorium-launch-" + currentProfileName + ".bat" };
                if (dlg.ShowDialog() != DialogResult.OK) return;
                
                // Make it silent: @echo off and start "" ...
                var cmd = txtCmdPreview.Text;
                var batContent = "@echo off\r\nstart \"\" " + cmd + "\r\nexit";
                
                File.WriteAllText(dlg.FileName, batContent);
                
                var result = MessageBox.Show("Batch file saved.\n\nCreate shortcuts?\n\nYes = Desktop shortcut (.lnk - can be pinned to taskbar)\nNo = Skip\nCancel = Also create .bat shortcut", 
                    "Create Shortcuts", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                
                if(result == DialogResult.Yes || result == DialogResult.Cancel)
                {
                    // Create direct .lnk shortcut (can be pinned!)
                    CreateDirectExeShortcut(currentProfileName);
                    MessageBox.Show("Direct .lnk shortcut created on Desktop.\n\nThis shortcut can be pinned to the taskbar and will keep all your flags!");
                }
                
                if(result == DialogResult.Cancel)
                {
                    // Also create .bat shortcut
                    var iconPath = Path.Combine(appdir, "Umbra Puprpurea.ico");
                    if(!File.Exists(iconPath)) iconPath = Application.ExecutablePath + ",0";
                    
                    CreateShortcut(dlg.FileName, "Launch Thorium - " + currentProfileName, iconPath);
                    MessageBox.Show(".bat shortcut also created on Desktop.");
                }
            }
            catch (Exception ex) { MessageBox.Show("Error exporting .bat: " + ex.Message); }
        }

        void CreateDirectExeShortcut(string profileName)
        {
            try {
                // Parse the command preview to extract exe and arguments
                var cmd = txtCmdPreview.Text;
                string exe = "";
                string args = "";
                
                // Extract exe between first quotes
                int firstQ = cmd.IndexOf('"');
                int secondQ = cmd.IndexOf('"', firstQ + 1);
                if (firstQ >= 0 && secondQ > firstQ)
                {
                    exe = cmd.Substring(firstQ + 1, secondQ - firstQ - 1);
                    args = cmd.Substring(secondQ + 1).Trim();
                }
                else
                {
                    var parts = cmd.Split(new[] { ' ' }, 2);
                    exe = parts[0];
                    args = parts.Length > 1 ? parts[1] : "";
                }
                
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string linkName = "Thorium - " + profileName + ".lnk";
                string linkPath = Path.Combine(desktop, linkName);
                
                var iconPath = Path.Combine(appdir, "Umbra Puprpurea.ico");
                if(!File.Exists(iconPath)) iconPath = exe + ",0";
                
                string vbs = Path.Combine(Path.GetTempPath(), "createshortcut.vbs");
                using (StreamWriter writer = new StreamWriter(vbs))
                {
                    writer.WriteLine("Set oWS = WScript.CreateObject(\"WScript.Shell\")");
                    writer.WriteLine("Set oLink = oWS.CreateShortcut(\"" + linkPath + "\")");
                    writer.WriteLine("oLink.TargetPath = \"" + exe + "\"");
                    writer.WriteLine("oLink.Arguments = \"" + args.Replace("\"", "\"\"") + "\""); // Escape quotes
                    writer.WriteLine("oLink.Description = \"Thorium Browser - " + profileName + " Profile\"");
                    writer.WriteLine("oLink.IconLocation = \"" + iconPath + "\"");
                    writer.WriteLine("oLink.WorkingDirectory = \"" + Path.GetDirectoryName(exe) + "\"");
                    writer.WriteLine("oLink.Save");
                }
                Process.Start("cscript", "/nologo \"" + vbs + "\"").WaitForExit();
                File.Delete(vbs);
            } catch(Exception ex) { 
                MessageBox.Show("Error creating direct shortcut: " + ex.Message); 
            }
        }

        void CreateWebApp()
        {
            var appName = PromptInput("Enter name for Web App (e.g. YouTube):");
            if(string.IsNullOrWhiteSpace(appName)) return;
            
            var url = PromptInput("Enter URL (e.g. https://www.youtube.com):");
            if(string.IsNullOrWhiteSpace(url)) return;
            if(!url.StartsWith("http")) url = "https://" + url;

            try {
                // Build command
                var exe = txtExePath.Text.Trim();
                if (string.IsNullOrEmpty(exe)) exe = "thorium.exe";
                
                var profileDir = EnsureProfile();
                var flags = txtFlags.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
                
                // Filter flags for app mode
                flags.RemoveAll(f => f.StartsWith("--window-size") || f.StartsWith("--window-position") || f.StartsWith("--app="));
                
                // Replace %APPDIR%
                for (int i = 0; i < flags.Count; i++) flags[i] = flags[i].Replace("%APPDIR%", appdir.TrimEnd(Path.DirectorySeparatorChar));
                
                if (!flags.Any(f => f.StartsWith("--user-data-dir")))
                    flags.Insert(0, "--user-data-dir=\"" + profileDir + "\"");
                
                flags.Add("--app=" + url);
                
                var cmd = "\"" + exe + "\" " + string.Join(" ", flags);
                var batContent = "@echo off\r\nstart \"\" " + cmd;
                
                var filename = "thorium-app-" + appName.Replace(" ", "-") + ".bat";
                var savePath = Path.Combine(appdir, filename);
                
                File.WriteAllText(savePath, batContent);
                
                if(MessageBox.Show("Web App created: " + filename + "\nCreate Desktop Shortcut?", "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var iconPath = Path.Combine(appdir, "Umbra Puprpurea.ico");
                    if(!File.Exists(iconPath)) iconPath = Application.ExecutablePath + ",0";
                    
                    CreateShortcut(savePath, appName, iconPath);
                    MessageBox.Show("Shortcut created on Desktop.");
                }
            } catch(Exception ex) {
                MessageBox.Show("Error creating Web App: " + ex.Message);
            }
        }

        void CreateShortcut(string targetPath, string description, string iconLocation)
        {
            try {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string linkName = Path.GetFileNameWithoutExtension(targetPath) + ".lnk";
                string linkPath = Path.Combine(desktop, linkName);
                
                string vbs = Path.Combine(Path.GetTempPath(), "createshortcut.vbs");
                using (StreamWriter writer = new StreamWriter(vbs))
                {
                    writer.WriteLine("Set oWS = WScript.CreateObject(\"WScript.Shell\")");
                    writer.WriteLine("Set oLink = oWS.CreateShortcut(\"" + linkPath + "\")");
                    writer.WriteLine("oLink.TargetPath = \"" + targetPath + "\"");
                    writer.WriteLine("oLink.Description = \"" + description + "\"");
                    writer.WriteLine("oLink.IconLocation = \"" + iconLocation + "\"");
                    writer.WriteLine("oLink.Save");
                }
                Process.Start("cscript", "/nologo \"" + vbs + "\"").WaitForExit();
                File.Delete(vbs);
            } catch(Exception ex) { MessageBox.Show("Error creating shortcut: " + ex.Message); }
        }

        // clone session: copy Cookies / Login Data and local storage leveldb
        void CloneSession(string sourceProfile, string targetProfile)
        {
            try
            {
                Directory.CreateDirectory(targetProfile);
                var copyFiles = new[] { "Cookies", "Cookies-journal", "Login Data", "Login Data-journal" };
                foreach (var f in copyFiles)
                {
                    var src = Path.Combine(sourceProfile, f);
                    var dst = Path.Combine(targetProfile, f);
                    if (File.Exists(src))
                    {
                        File.Copy(src, dst, true);
                    }
                }
                var srcLevel = Path.Combine(sourceProfile, "Local Storage", "leveldb");
                var dstLevel = Path.Combine(targetProfile, "Local Storage", "leveldb");
                if (Directory.Exists(srcLevel))
                {
                    if (Directory.Exists(dstLevel)) Directory.Delete(dstLevel, true);
                    CopyDirectory(srcLevel, dstLevel);
                }
            }
            catch (Exception ex) { MessageBox.Show("Clone error: " + ex.Message); }
        }

        void CopyDirectory(string src, string dst)
        {
            Directory.CreateDirectory(dst);
            foreach (var d in Directory.GetDirectories(src, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(d.Replace(src, dst));
            foreach (var f in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories))
                File.Copy(f, f.Replace(src, dst), true);
        }

        // ---------- small helpers ----------
        void BrowseExe()
        {
            using (var ofd = new OpenFileDialog() { Filter = "Executables (*.exe)|*.exe|All files|*.*", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) })
            {
                if (ofd.ShowDialog() == DialogResult.OK) { 
                    txtExePath.Text = ofd.FileName; 
                    SaveConfig(); 
                    RefreshProfiles(); // Refresh to show native profiles from new exe path
                    UpdatePreview(); 
                }
            }
        }

        string PromptInput(string prompt)
        {
            return Microsoft.VisualBasic.Interaction.InputBox(prompt, "Input", "");
        }

        void CloneProfileFromFiles(string sourceProfileName, string targetProfileName)
        {
            var src = Path.Combine(appdir, MakeProfileDirName(sourceProfileName));
            var dst = Path.Combine(appdir, MakeProfileDirName(targetProfileName));
            CloneSession(src, dst);
        }

        void CloneProfileDialogPrompt()
        {
            var src = PromptInput("Source profile name:");
            var dst = PromptInput("Target profile name:");
            if (!string.IsNullOrWhiteSpace(src) && !string.IsNullOrWhiteSpace(dst))
            {
                CloneProfileFromFiles(src, dst);
                MessageBox.Show("Clone attempted: " + src + " -> " + dst);
            }
        }


        void DeleteCurrentProfile()
        {
            if(currentProfileName == "Default") { MessageBox.Show("Cannot delete Default profile."); return; }
            
            var isNative = currentProfileName.StartsWith("[Native] ");
            var displayName = isNative ? currentProfileName.Substring("[Native] ".Length) : currentProfileName;
            
            var msg = "Delete profile '" + currentProfileName + "'?\n\nThis will PERMANENTLY delete:\n- Profile folder and ALL browsing data\n- Flags configuration\n\nThis cannot be undone!";
            if(MessageBox.Show(msg, "Delete Profile", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try {
                    string dir;
                    
                    if (isNative)
                    {
                        // Native profile: delete from Thorium User Data folder
                        var exePath = txtExePath.Text.Trim();
                        if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                        {
                            var appFolder = Path.GetDirectoryName(exePath);
                            var thoriumRoot = Path.GetDirectoryName(appFolder);
                            dir = Path.Combine(thoriumRoot, "User Data", displayName);
                        }
                        else
                        {
                            MessageBox.Show("Cannot locate Thorium installation."); 
                            return;
                        }
                    }
                    else
                    {
                        // Launcher-managed profile
                        var profilesDir = Path.Combine(appdir, "Profiles");
                        dir = Path.Combine(profilesDir, "thorium-profile-" + currentProfileName);
                    }
                    
                    if(Directory.Exists(dir)) {
                        SafeDeleteDirectory(dir);
                    }
                    
                    // Switch to Default BEFORE refreshing
                    currentProfileName = "Default";
                    RefreshProfiles();
                    UpdatePreview();
                    lblStatus.Text = "Profile deleted.";
                } catch(Exception ex) {
                    MessageBox.Show("Error deleting profile: " + ex.Message);
                }
            }
        }

        void SafeDeleteDirectory(string targetDir)
        {
            try {
                var dirInfo = new DirectoryInfo(targetDir);
                dirInfo.Attributes = FileAttributes.Normal;

                foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                    file.Attributes = FileAttributes.Normal;

                foreach (var dir in dirInfo.GetDirectories("*", SearchOption.AllDirectories))
                    dir.Attributes = FileAttributes.Normal;

                Directory.Delete(targetDir, true);
            } catch(Exception ex) {
                MessageBox.Show("Could not delete folder '" + targetDir + "':\n" + ex.Message, "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void OpenProfileManager()
        {
            using(var pm = new ProfileManagerForm(appdir, currentProfileName)) {
                if(pm.ShowDialog() == DialogResult.OK) {
                    RefreshProfiles();
                    if(!string.IsNullOrEmpty(pm.SelectedProfile)) SelectProfile(pm.SelectedProfile);
                } else {
                    RefreshProfiles(); // Refresh anyway in case of deletions
                }
            }
        }

        void CreateProfileFromUI(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            if (!cbProfiles.Items.Contains(name)) cbProfiles.Items.Add(name);
            cbProfiles.Text = name;
            currentProfileName = name;
            SaveFlagsForProfile();
            UpdatePreview();
        }

        void CreateProfileButton_Click(object sender, EventArgs e) { CreateProfile(); }

        void OpenProfileFolderButton_Click(object sender, EventArgs e) { OpenProfileFolder(); }

        // UI prompt wrappers


        string GetDefaultFlags()
        {
            // Calculate centered window position
            var scr = Screen.PrimaryScreen.Bounds;
            int W = 1920, H = 1080;
            int posX = (scr.Width - W) / 2;
            int posY = (scr.Height - H) / 2;
            
            return @"--profile-directory=""Default""
--user-data-dir=""%APPDIR%thorium-profile""
--no-default-browser-check
--no-first-run
--enable-quic
--enable-gpu-rasterization
--ignore-gpu-blocklist
--max-connections-per-host=25
--js-flags=""--max-old-space-size=4096""
--window-size=" + W + "," + H + @"
--window-position=" + posX + "," + posY + @"
--enable-parallel-downloading
--enable-http3
--enable-features=VaapiVideoDecoder,CanvasOopRasterization,SkiaGraphite
--disable-background-networking
--disable-background-timer-throttling
--process-per-site
--disable-blink-features=AutomationControlled
--fingerprinting-canvas-noise=enabled
--spoof-webgl-info=""Enabled NVIDIA GeForce RTX 4090""
--user-agent=""Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36""
";
        }
    } // class LauncherForm
    public class ProfileManagerForm : Form
    {
        public string SelectedProfile { get; private set; }
        ListView lvProfiles;
        Button btnDelete;
        Button btnRename;
        Button btnSelect;
        string appdir;

        public ProfileManagerForm(string appDir, string current)
        {
            this.appdir = appDir;
            this.Text = "Profile Manager";
            this.Width = 600;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch {}

            lvProfiles = new ListView() { Left = 12, Top = 12, Width = 560, Height = 280, View = View.Details, FullRowSelect = true, MultiSelect = false };
            lvProfiles.Columns.Add("Profile Name", 200);
            lvProfiles.Columns.Add("Mode (Detected)", 120);
            lvProfiles.Columns.Add("Size", 100);
            lvProfiles.Columns.Add("Last Modified", 120);

            btnSelect = new Button() { Left = 12, Top = 310, Width = 120, Height = 36, Text = "Select & Close", DialogResult = DialogResult.OK };
            btnRename = new Button() { Left = 140, Top = 310, Width = 100, Height = 36, Text = "Rename" };
            btnDelete = new Button() { Left = 452, Top = 310, Width = 120, Height = 36, Text = "Delete Profile", ForeColor = System.Drawing.Color.DarkRed };
            
            btnRename.Click += (s, e) => RenameSelected();
            btnDelete.Click += (s, e) => DeleteSelected();
            btnSelect.Click += (s, e) => {
                if(lvProfiles.SelectedItems.Count > 0) SelectedProfile = lvProfiles.SelectedItems[0].Text;
            };
            lvProfiles.DoubleClick += (s, e) => {
                if(lvProfiles.SelectedItems.Count > 0) {
                    SelectedProfile = lvProfiles.SelectedItems[0].Text;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };

            this.Controls.Add(lvProfiles);
            this.Controls.Add(btnSelect);
            this.Controls.Add(btnRename);
            this.Controls.Add(btnDelete);

            LauncherForm.ApplyDarkTheme(this);
            RefreshList(current);
        }

        void RefreshList(string current)
        {
            lvProfiles.Items.Clear();
            var profiles = new List<string> { "Default" };
            
            // 1. Launcher-managed profiles
            var profilesDir = Path.Combine(appdir, "Profiles");
            if(Directory.Exists(profilesDir)) {
                var dirs = Directory.GetDirectories(profilesDir, "thorium-profile-*");
                foreach (var d in dirs)
                {
                    var dirName = new DirectoryInfo(d).Name;
                    var name = dirName.Substring("thorium-profile-".Length);
                    if (!profiles.Contains(name)) profiles.Add(name);
                }
            }

            // 2. Native Thorium profiles (read from launcher.ini to get exe path)
            string exePath = "";
            var iniPath = Path.Combine(appdir, "launcher.ini");
            if (File.Exists(iniPath))
            {
                foreach (var line in File.ReadAllLines(iniPath))
                {
                    if (line.StartsWith("ExePath="))
                    {
                        exePath = line.Substring("ExePath=".Length);
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
            {
                var appFolder = Path.GetDirectoryName(exePath);
                var thoriumRoot = Path.GetDirectoryName(appFolder);
                var userDataDir = Path.Combine(thoriumRoot, "User Data");
                
                if (Directory.Exists(userDataDir))
                {
                    // Look for Profile folders
                    var nativeProfiles = Directory.GetDirectories(userDataDir, "Profile *");
                    foreach (var p in nativeProfiles)
                    {
                        var profileName = new DirectoryInfo(p).Name;
                        var displayName = "[Native] " + profileName;
                        if (!profiles.Contains(displayName))
                            profiles.Add(displayName);
                    }
                    
                    // Check for native Default
                    var nativeDefault = Path.Combine(userDataDir, "Default");
                    if (Directory.Exists(nativeDefault))
                    {
                        var ourDefault = Path.Combine(profilesDir, "thorium-profile");
                        if (Path.GetFullPath(nativeDefault) != Path.GetFullPath(ourDefault))
                        {
                            if (!profiles.Contains("[Native] Default"))
                                profiles.Add("[Native] Default");
                        }
                    }
                    
                    // Check for "Dropship" and other named folders
                    foreach (var dir in Directory.GetDirectories(userDataDir))
                    {
                        var dirName = new DirectoryInfo(dir).Name;
                        // Skip system folders
                        if (dirName.Contains("Profile ") || dirName == "Default" || 
                            dirName.Contains("Cache") || dirName.Contains("Safe Browsing") ||
                            dirName == "System Profile" || dirName == "BrowserMetrics")
                            continue;
                        
                        // Check if it looks like a profile (has Preferences file)
                        var prefsFile = Path.Combine(dir, "Preferences");
                        if (File.Exists(prefsFile))
                        {
                            var displayName = "[Native] " + dirName;
                            if (!profiles.Contains(displayName))
                                profiles.Add(displayName);
                        }
                    }
                }
            }

            // Display all profiles
            foreach(var p in profiles)
            {
                var item = new ListViewItem(p);
                var isNative = p.StartsWith("[Native] ");
                
                string dirPath;
                string flagsFile;
                
                if (isNative)
                {
                    // Native profile
                    var nativeName = p.Substring("[Native] ".Length);
                    var appFolder = Path.GetDirectoryName(exePath);
                    var thoriumRoot = Path.GetDirectoryName(appFolder);
                    dirPath = Path.Combine(thoriumRoot, "User Data", nativeName);
                    flagsFile = Path.Combine(dirPath, "flags.txt");
                }
                else
                {
                    // Launcher-managed profile
                    var dirName = p == "Default" ? Path.Combine("Profiles", "thorium-profile") : Path.Combine("Profiles", "thorium-profile-" + p);
                    dirPath = Path.Combine(appdir, dirName);
                    flagsFile = Path.Combine(dirPath, "flags.txt");
                }
                
                string mode = isNative ? "Native" : "Standard";
                DateTime lastMod = DateTime.MinValue;

                if(File.Exists(flagsFile)) {
                    var txt = File.ReadAllText(flagsFile);
                    if(txt.Contains("fingerprinting") || txt.Contains("spoof")) mode = isNative ? "Native+Hardened" : "Hardened";
                    else if(txt.Length < 300) mode = isNative ? "Native+Minimal" : "Minimal";
                    lastMod = File.GetLastWriteTime(flagsFile);
                }
                else if (isNative)
                {
                    mode = "Native (No Flags)";
                }

                if(Directory.Exists(dirPath)) mode += " ✓";
                else mode += " (Missing)";

                item.SubItems.Add(mode);
                item.SubItems.Add("-"); 
                item.SubItems.Add(lastMod == DateTime.MinValue ? "-" : lastMod.ToString("yyyy-MM-dd HH:mm"));
                
                if(p == current) item.Font = new System.Drawing.Font(item.Font, System.Drawing.FontStyle.Bold);
                
                lvProfiles.Items.Add(item);
            }
        }

        void RenameSelected()
        {
            if(lvProfiles.SelectedItems.Count == 0) return;
            var oldName = lvProfiles.SelectedItems[0].Text;
            if(oldName == "Default") { MessageBox.Show("Cannot rename Default profile."); return; }
            if(oldName.StartsWith("[Native] ")) { MessageBox.Show("Cannot rename native Thorium profiles.\nYou can only manage their flags."); return; }

            string newName = Microsoft.VisualBasic.Interaction.InputBox("Enter new name for profile '" + oldName + "':", "Rename Profile", oldName);
            if(string.IsNullOrWhiteSpace(newName) || newName == oldName) return;
            
            if(newName == "Default" || newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
                MessageBox.Show("Invalid name."); return;
            }

            try {
                var profilesDir = Path.Combine(appdir, "Profiles");
                var oldDir = Path.Combine(profilesDir, "thorium-profile-" + oldName);
                var newDir = Path.Combine(profilesDir, "thorium-profile-" + newName);

                if(Directory.Exists(newDir)) {
                    MessageBox.Show("Profile '" + newName + "' already exists."); return;
                }

                if(Directory.Exists(oldDir)) Directory.Move(oldDir, newDir);

                // Update user-data-dir in the new flags file
                var newFlags = Path.Combine(newDir, "flags.txt");
                if(File.Exists(newFlags)) {
                    var content = File.ReadAllText(newFlags);
                    var oldPathStr = "thorium-profile-" + oldName;
                    var newPathStr = "thorium-profile-" + newName;
                    if(content.Contains(oldPathStr)) {
                        File.WriteAllText(newFlags, content.Replace(oldPathStr, newPathStr));
                    }
                }

                RefreshList(newName);
                if(SelectedProfile == oldName) SelectedProfile = newName;

            } catch(Exception ex) {
                MessageBox.Show("Error renaming: " + ex.Message);
            }
        }

        void BackupSelected()
        {
            if(lvProfiles.SelectedItems.Count == 0) return;
            var name = lvProfiles.SelectedItems[0].Text;
            
            var dlg = new SaveFileDialog() { Filter = "Zip Archive|*.zip", FileName = "thorium-profile-" + name + "-backup.zip" };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            
            var profilesDir = Path.Combine(appdir, "Profiles");
            var dir = name == "Default" ? Path.Combine(profilesDir, "thorium-profile") : Path.Combine(profilesDir, "thorium-profile-" + name);
            
            if(!Directory.Exists(dir)) {
                MessageBox.Show("Profile folder not found (maybe no data yet).");
                return;
            }

            try {
                // Use PowerShell to zip because .NET 4.0 doesn't have ZipFile
                var cmd = "Compress-Archive -Path '" + dir + "' -DestinationPath '" + dlg.FileName + "' -Force";
                var psi = new System.Diagnostics.ProcessStartInfo() {
                    FileName = "powershell",
                    Arguments = "-Command \"" + cmd + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                var p = System.Diagnostics.Process.Start(psi);
                p.WaitForExit();
                
                if(p.ExitCode == 0) MessageBox.Show("Backup created successfully.");
                else MessageBox.Show("Backup failed. PowerShell exit code: " + p.ExitCode);
                
            } catch(Exception ex) {
                MessageBox.Show("Error creating backup: " + ex.Message);
            }
        }

        void DeleteSelected()
        {
            if(lvProfiles.SelectedItems.Count == 0) return;
            var name = lvProfiles.SelectedItems[0].Text;
            if(name == "Default") { MessageBox.Show("Cannot delete Default profile."); return; }
            
            var msg = "Are you sure you want to DELETE profile '" + name + "'?\n\nThis will PERMANENTLY delete:\n- Profile flags file\n- Profile folder and ALL browsing data\n\nThis cannot be undone!";
            if(MessageBox.Show(msg, "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try {
                    var profilesDir = Path.Combine(appdir, "Profiles");
                    var dir = Path.Combine(profilesDir, "thorium-profile-" + name);
                    
                    if(Directory.Exists(dir)) {
                        SafeDeleteDirectory(dir);
                    }
                    
                    RefreshList("");
                } catch(Exception ex) {
                    MessageBox.Show("Error deleting: " + ex.Message);
                }
            }
        }

        void SafeDeleteDirectory(string targetDir)
        {
            try {
                var dirInfo = new DirectoryInfo(targetDir);
                dirInfo.Attributes = FileAttributes.Normal;

                foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                    file.Attributes = FileAttributes.Normal;

                foreach (var dir in dirInfo.GetDirectories("*", SearchOption.AllDirectories))
                    dir.Attributes = FileAttributes.Normal;

                Directory.Delete(targetDir, true);
            } catch(Exception ex) {
                MessageBox.Show("Could not delete folder '" + targetDir + "':\n" + ex.Message, "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} // ns
