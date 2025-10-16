using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZYCLEANERV1
{
    // Rename to Form1 if your designer uses Form1
    public partial class MainForm : Form
    {
        private enum TargetType
        {
            Directory,
            FilePatternInDirectory,   // e.g., thumbcache_*.db in a folder
            SpecialRecycleBin
        }

        private class CacheTarget
        {
            public string Category { get; set; }
            public string Path { get; set; }
            public TargetType Type { get; set; }
            public bool RequiresAdmin { get; set; }
            public long SizeBytes { get; set; }
            public string Status { get; set; }
            public string FilePattern { get; set; } // only for FilePatternInDirectory

            public CacheTarget()
            {
                Category = "";
                Path = "";
                Type = TargetType.Directory;
                RequiresAdmin = false;
                SizeBytes = 0;
                Status = "Pending";
                FilePattern = "";
            }
        }

        private readonly List<CacheTarget> _targets = new List<CacheTarget>();
        private CancellationTokenSource _cts;
        private long _lastFreedBytes = 0; // NEW: track freed total

        [Flags]
        private enum RecycleFlags : int
        {
            SHERB_NOCONFIRMATION = 0x00000001,
            SHERB_NOPROGRESSUI = 0x00000002,
            SHERB_NOSOUND = 0x00000004
        }

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlags dwFlags);

        public MainForm()
        {
            // Optional auto-elevate; comment out if you don’t want UAC prompt
            /*
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator))
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = Application.ExecutablePath,
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    Process.Start(psi);
                    Application.Exit();
                    return;
                }
                catch { }
            }
            */
            InitializeComponent();
            PrepareListView();
            SetupContextMenu();   // NEW: right-click menu
            PopulateTargets();
            WireEvents();
        }

        private void WireEvents()
        {
            btnScan.Click += async (s, e) => await ScanAsync();
            btnClean.Click += async (s, e) => await CleanSelectedAsync();
            this.FormClosing += (s, e) => { if (_cts != null) _cts.Cancel(); };
        }

        private void PrepareListView()
        {
            lvResults.CheckBoxes = true;
            lvResults.FullRowSelect = true;
            lvResults.GridLines = true;
            lvResults.View = View.Details;

            if (lvResults.Columns.Count == 0)
            {
                lvResults.Columns.Add("Category", 220);
                lvResults.Columns.Add("Path", 520);
                lvResults.Columns.Add("Size", 120);
                lvResults.Columns.Add("Status", 140);
            }
        }

        // NEW: context menu for opening folder
        private void SetupContextMenu()
        {
            var cms = new ContextMenuStrip();
            var openItem = new ToolStripMenuItem("Open Location");
            openItem.Click += (s, e) =>
            {
                if (lvResults.SelectedItems.Count == 0) return;
                var idx = lvResults.SelectedItems[0].Index;
                if (idx < 0 || idx >= _targets.Count) return;
                var t = _targets[idx];
                if (t.Type == TargetType.SpecialRecycleBin) return;
                if (!string.IsNullOrWhiteSpace(t.Path) && Directory.Exists(t.Path))
                    Process.Start("explorer.exe", t.Path);
            };
            cms.Items.Add(openItem);
            lvResults.ContextMenuStrip = cms;
        }

        private static bool IsAdministrator()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch { return false; }
        }

        private void PopulateTargets()
        {
            _targets.Clear();

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            // Temp (user + system)
            _targets.Add(new CacheTarget { Category = "Temp (User)", Path = System.IO.Path.GetTempPath(), Type = TargetType.Directory });
            _targets.Add(new CacheTarget { Category = "Temp (System)", Path = System.IO.Path.Combine(windowsDir, "Temp"), Type = TargetType.Directory, RequiresAdmin = true });

            // Prefetch
            _targets.Add(new CacheTarget { Category = "Prefetch", Path = System.IO.Path.Combine(windowsDir, "Prefetch"), Type = TargetType.Directory, RequiresAdmin = true });

            // Delivery Optimization
            _targets.Add(new CacheTarget { Category = "Delivery Optimization", Path = @"C:\ProgramData\Microsoft\Windows\DeliveryOptimization", Type = TargetType.Directory, RequiresAdmin = true });

            // Thumbnail cache (thumbcache_*.db)
            _targets.Add(new CacheTarget
            {
                Category = "Thumbnail Cache",
                Path = System.IO.Path.Combine(localAppData, @"Microsoft\Windows\Explorer"),
                Type = TargetType.FilePatternInDirectory,
                FilePattern = @"^thumbcache_.*\.db$"
            });

            // Windows Update download cache
            _targets.Add(new CacheTarget { Category = "Windows Update Cache", Path = System.IO.Path.Combine(windowsDir, @"SoftwareDistribution\Download"), Type = TargetType.Directory, RequiresAdmin = true });

            // Chrome profiles
            string chromeBase = System.IO.Path.Combine(localAppData, @"Google\Chrome\User Data");
            if (Directory.Exists(chromeBase))
            {
                var profiles = new List<string> { "Default" };
                try
                {
                    foreach (var d in Directory.GetDirectories(chromeBase, "Profile *"))
                    {
                        var name = System.IO.Path.GetFileName(d);
                        profiles.Add(name ?? d);
                    }
                }
                catch { }

                foreach (var prof in profiles.Distinct())
                {
                    _targets.Add(new CacheTarget { Category = "Chrome (" + prof + ") Cache", Path = System.IO.Path.Combine(chromeBase, prof, "Cache"), Type = TargetType.Directory });
                    _targets.Add(new CacheTarget { Category = "Chrome (" + prof + ") Code Cache", Path = System.IO.Path.Combine(chromeBase, prof, @"Code Cache"), Type = TargetType.Directory });
                }
            }

            // Edge profiles
            string edgeBase = System.IO.Path.Combine(localAppData, @"Microsoft\Edge\User Data");
            if (Directory.Exists(edgeBase))
            {
                var profiles = new List<string> { "Default" };
                try
                {
                    foreach (var d in Directory.GetDirectories(edgeBase, "Profile *"))
                    {
                        var name = System.IO.Path.GetFileName(d);
                        profiles.Add(name ?? d);
                    }
                }
                catch { }

                foreach (var prof in profiles.Distinct())
                {
                    _targets.Add(new CacheTarget { Category = "Edge (" + prof + ") Cache", Path = System.IO.Path.Combine(edgeBase, prof, "Cache"), Type = TargetType.Directory });
                    _targets.Add(new CacheTarget { Category = "Edge (" + prof + ") Code Cache", Path = System.IO.Path.Combine(edgeBase, prof, @"Code Cache"), Type = TargetType.Directory });
                }
            }

            // Firefox profiles
            string ffProfiles = System.IO.Path.Combine(appDataRoaming, @"Mozilla\Firefox\Profiles");
            if (Directory.Exists(ffProfiles))
            {
                foreach (var profileDir in Directory.GetDirectories(ffProfiles))
                {
                    string name = System.IO.Path.GetFileName(profileDir);
                    if (string.IsNullOrEmpty(name)) name = profileDir;
                    _targets.Add(new CacheTarget { Category = "Firefox (" + name + ") cache2", Path = System.IO.Path.Combine(profileDir, "cache2"), Type = TargetType.Directory });
                }
            }

            // Recycle Bin
            _targets.Add(new CacheTarget { Category = "Recycle Bin", Path = "Shell:RecycleBin", Type = TargetType.SpecialRecycleBin });

            RefreshListView();
        }

        private void RefreshListView()
        {
            lvResults.BeginUpdate();
            try
            {
                lvResults.Items.Clear();
                foreach (var t in _targets)
                {
                    var item = new ListViewItem(t.Category);
                    item.SubItems.Add(t.Path);
                    item.SubItems.Add(t.SizeBytes > 0 ? FormatSize(t.SizeBytes) : "-");
                    item.SubItems.Add(t.Status);
                    item.Checked = true;
                    lvResults.Items.Add(item);
                }
            }
            finally
            {
                lvResults.EndUpdate();
            }
        }

        // ---------- Scan -----------------------------------------------------

        private async Task ScanAsync()
        {
            if (_cts != null) { _cts.Cancel(); _cts = null; }
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            LogClear();
            SetStatus("Scanning caches...");
            progressBar.Value = 0;

            int total = _targets.Count;
            int done = 0;

            for (int i = 0; i < _targets.Count; i++) _targets[i].SizeBytes = 0;

            try
            {
                foreach (var target in _targets)
                {
                    if (token.IsCancellationRequested) break;

                    UpdateTargetStatus(target, "Scanning...");
                    long size = 0;

                    try
                    {
                        if (target.Type == TargetType.Directory)
                            size = await Task.Run(() => DirSizeSafe(target.Path), token);
                        else if (target.Type == TargetType.FilePatternInDirectory)
                            size = await Task.Run(() => PatternSizeSafe(target.Path, target.FilePattern), token);
                        else
                            size = await Task.Run(() => EstimateRecycleBinSize(), token);

                        target.SizeBytes = size;
                        UpdateTargetStatus(target, size > 0 ? "Ready" : "Empty");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        UpdateTargetStatus(target, "Access denied");
                    }
                    catch (Exception ex)
                    {
                        UpdateTargetStatus(target, "Error: " + ex.Message);
                    }

                    UpdateListViewRow(target);
                    done++;
                    SetProgress(done, total);
                }

                SetStatus("Scan complete.");
                Log("Scan complete.");
            }
            catch (Exception ex)
            {
                Log("Scan failed: " + ex.Message);
                SetStatus("Scan failed.");
            }
        }

        // ---------- Clean ----------------------------------------------------

        private async Task CleanSelectedAsync()
        {
            if (_cts != null) { _cts.Cancel(); _cts = null; }
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            var selectedTargets = GetCheckedTargets();
            if (selectedTargets.Count == 0)
            {
                MessageBox.Show("Select at least one category to clean.", "Nothing selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (selectedTargets.Any(t => t.RequiresAdmin) && !IsAdministrator())
            {
                var dr = MessageBox.Show(
                    "Some selected items require Administrator privileges.\nRun the app as Administrator to fully clean them.\n\nContinue anyway?",
                    "Admin Recommended",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.No) return;
            }

            // NEW: record “before” to compute freed later
            _lastFreedBytes = selectedTargets.Sum(t => t.SizeBytes);

            LogClear();
            SetStatus("Cleaning...");
            progressBar.Value = 0;

            int total = selectedTargets.Count;
            int done = 0;

            foreach (var target in selectedTargets)
            {
                if (token.IsCancellationRequested) break;

                UpdateTargetStatus(target, "Cleaning...");
                UpdateListViewRow(target);

                try
                {
                    if (target.Type == TargetType.Directory)
                    {
                        // NEW: wrap Windows Update cache with service stop
                        if (target.Path.IndexOf(@"\SoftwareDistribution\Download", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            await Task.Run(() =>
                            {
                                WithServicesStopped(new[] { "wuauserv", "bits" }, () =>
                                {
                                    DeleteDirectoryContents(target.Path);
                                });
                            }, token);
                        }
                        else
                        {
                            await Task.Run(() => DeleteDirectoryContents(target.Path), token);
                        }
                    }
                    else if (target.Type == TargetType.FilePatternInDirectory)
                    {
                        await Task.Run(() => DeletePattern(target.Path, target.FilePattern), token);
                    }
                    else
                    {
                        await Task.Run(() => EmptyRecycleBin(), token);
                    }

                    long size = 0;
                    if (target.Type == TargetType.Directory)
                        size = await Task.Run(() => DirSizeSafe(target.Path), token);
                    else if (target.Type == TargetType.FilePatternInDirectory)
                        size = await Task.Run(() => PatternSizeSafe(target.Path, target.FilePattern), token);
                    else
                        size = await Task.Run(() => EstimateRecycleBinSize(), token);

                    target.SizeBytes = size;
                    UpdateTargetStatus(target, size == 0 ? "Cleaned" : "Partially cleaned");
                }
                catch (Exception ex)
                {
                    UpdateTargetStatus(target, "Error: " + ex.Message);
                }

                UpdateListViewRow(target);
                done++;
                SetProgress(done, total);
            }

            // NEW: compute & show freed total
            long after = selectedTargets.Sum(t => t.SizeBytes);
            long freed = _lastFreedBytes - after;
            if (freed < 0) freed = 0;

            Log("------------------------------------------------------------");
            Log("Freed: " + FormatSize(freed));
            SetStatus("Cleaning complete. Freed " + FormatSize(freed));
            Log("Cleaning complete.");
        }

        private List<CacheTarget> GetCheckedTargets()
        {
            var res = new List<CacheTarget>();
            for (int i = 0; i < lvResults.Items.Count; i++)
            {
                if (lvResults.Items[i].Checked)
                    res.Add(_targets[i]);
            }
            return res;
        }

        // ---------- Helpers: size + deletion --------------------------------

        private static long DirSizeSafe(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return 0;
            long size = 0;
            try
            {
                var dir = new DirectoryInfo(path);
                foreach (var file in dir.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    try { size += file.Length; } catch { }
                }
            }
            catch { }
            return size;
        }

        private static long PatternSizeSafe(string directory, string regexPattern)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory)) return 0;
            long size = 0;
            Regex rx = new Regex(regexPattern, RegexOptions.IgnoreCase);
            try
            {
                foreach (var f in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        string name = System.IO.Path.GetFileName(f);
                        if (rx.IsMatch(name))
                            size += new FileInfo(f).Length;
                    }
                    catch { }
                }
            }
            catch { }
            return size;
        }

        // NEW: safer delete; skips layout.ini if Prefetch
        private static void DeleteDirectoryContents(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;

            bool isPrefetch = path.EndsWith(@"\Prefetch", StringComparison.OrdinalIgnoreCase);

            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                try
                {
                    if (isPrefetch && string.Equals(System.IO.Path.GetFileName(file), "layout.ini", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var attr = File.GetAttributes(file);
                    if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        File.SetAttributes(file, attr & ~FileAttributes.ReadOnly);
                    File.Delete(file);
                }
                catch { }
            }

            foreach (var dir in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories)
                                         .OrderByDescending(d => d.Length))
            {
                try
                {
                    if (!Directory.EnumerateFileSystemEntries(dir).Any())
                        Directory.Delete(dir, false);
                }
                catch { }
            }
        }

        private static void DeletePattern(string directory, string regexPattern)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory)) return;
            Regex rx = new Regex(regexPattern, RegexOptions.IgnoreCase);

            foreach (var f in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    string name = System.IO.Path.GetFileName(f);
                    if (rx.IsMatch(name))
                    {
                        var attr = File.GetAttributes(f);
                        if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                            File.SetAttributes(f, attr & ~FileAttributes.ReadOnly);
                        File.Delete(f);
                    }
                }
                catch { }
            }
        }

        private static long EstimateRecycleBinSize()
        {
            return 0; // not calculated; still emptied via API
        }

        private static void EmptyRecycleBin()
        {
            SHEmptyRecycleBin(IntPtr.Zero, null,
                RecycleFlags.SHERB_NOCONFIRMATION |
                RecycleFlags.SHERB_NOPROGRESSUI |
                RecycleFlags.SHERB_NOSOUND);
        }

        // NEW: stop services while running an action (Windows Update cache)
        private static void WithServicesStopped(string[] serviceNames, Action action)
        {
            var controllers = new List<ServiceController>();
            try
            {
                foreach (var name in serviceNames)
                {
                    try
                    {
                        var sc = new ServiceController(name);
                        controllers.Add(sc);
                        if (sc.Status == ServiceControllerStatus.Running ||
                            sc.Status == ServiceControllerStatus.StartPending)
                        {
                            sc.Stop();
                            sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20));
                        }
                    }
                    catch { }
                }

                try { action(); } catch { }
            }
            finally
            {
                foreach (var sc in controllers)
                {
                    try
                    {
                        if (sc.Status == ServiceControllerStatus.Stopped)
                        {
                            sc.Start();
                            sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(20));
                        }
                    }
                    catch { }
                }
            }
        }

        // ---------- UI plumbing ---------------------------------------------

        private void UpdateTargetStatus(CacheTarget t, string status)
        {
            t.Status = status;
        }

        private void UpdateListViewRow(CacheTarget t)
        {
            int index = _targets.IndexOf(t);
            if (index < 0 || index >= lvResults.Items.Count) return;

            var item = lvResults.Items[index];
            item.SubItems[2].Text = t.SizeBytes > 0 ? FormatSize(t.SizeBytes) : "-";
            item.SubItems[3].Text = t.Status;
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            double kb = bytes / 1024d;
            if (kb < 1024) return kb.ToString("0.##") + " KB";
            double mb = kb / 1024d;
            if (mb < 1024) return mb.ToString("0.##") + " MB";
            double gb = mb / 1024d;
            return gb.ToString("0.##") + " GB";
        }

        private void SetProgress(int done, int total)
        {
            if (total <= 0) { progressBar.Value = 0; return; }
            int val = (int)(done * 100.0 / total);
            if (val < 0) val = 0;
            if (val > 100) val = 100;
            progressBar.Value = val;
        }

        private void SetStatus(string text)
        {
            lblStatus.Text = "Status: " + text;
        }

        private void Log(string line)
        {
            txtLog.AppendText(line + Environment.NewLine);
        }

        private void LogClear()
        {
            txtLog.Clear();
        }
    }
}
