using Microsoft.Win32;

namespace TetriONInstaller;

public partial class UninstallerForm : Form
{
    private ProgressBar progressBar;
    private Label statusLabel;
    private Button uninstallButton;
    private Button cancelButton;
    private CheckBox removeUserData;
    
    private string installPath;

    public UninstallerForm(string installPath)
    {
        this.installPath = installPath;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "TetriON Uninstaller";
        this.Size = new Size(400, 250);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var infoLabel = new Label
        {
            Text = "This will remove TetriON from your computer.",
            Location = new Point(20, 20),
            Size = new Size(350, 23),
            AutoSize = false
        };

        removeUserData = new CheckBox
        {
            Text = "Remove user data and settings",
            Location = new Point(20, 60),
            Size = new Size(250, 23)
        };

        progressBar = new ProgressBar
        {
            Location = new Point(20, 100),
            Size = new Size(340, 23),
            Style = ProgressBarStyle.Continuous
        };

        statusLabel = new Label
        {
            Text = "Ready to uninstall",
            Location = new Point(20, 130),
            Size = new Size(340, 23),
            AutoSize = false
        };

        uninstallButton = new Button
        {
            Text = "Uninstall",
            Location = new Point(220, 170),
            Size = new Size(80, 30)
        };
        uninstallButton.Click += UninstallButton_Click;

        cancelButton = new Button
        {
            Text = "Cancel",
            Location = new Point(310, 170),
            Size = new Size(80, 30)
        };
        cancelButton.Click += (s, e) => this.Close();

        this.Controls.AddRange(new Control[] {
            infoLabel, removeUserData, progressBar, statusLabel, uninstallButton, cancelButton
        });
    }

    private async void UninstallButton_Click(object sender, EventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to uninstall TetriON?", 
            "Confirm Uninstall", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (result != DialogResult.Yes) return;

        try
        {
            uninstallButton.Enabled = false;
            cancelButton.Text = "Close";

            await PerformUninstallation();

            MessageBox.Show("TetriON has been successfully uninstalled.", "Uninstall Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Uninstall failed: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task PerformUninstallation()
    {
        // Remove shortcuts
        UpdateStatus("Removing shortcuts...");
        RemoveShortcuts();
        progressBar.Value = 20;

        // Remove registry entries
        UpdateStatus("Removing registry entries...");
        RemoveRegistryEntries();
        progressBar.Value = 40;

        // Remove game files
        UpdateStatus("Removing game files...");
        await RemoveGameFiles();
        progressBar.Value = 80;

        // Remove user data if requested
        if (removeUserData.Checked)
        {
            UpdateStatus("Removing user data...");
            RemoveUserData();
        }
        progressBar.Value = 100;

        UpdateStatus("Uninstall completed successfully!");
    }

    private void RemoveShortcuts()
    {
        try
        {
            // Desktop shortcut
            var desktopShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "TetriON.lnk");
            if (File.Exists(desktopShortcut))
                File.Delete(desktopShortcut);

            // Start menu shortcut
            var startMenuFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "TetriON");
            if (Directory.Exists(startMenuFolder))
                Directory.Delete(startMenuFolder, true);
        }
        catch (Exception ex)
        {
            // Log but don't fail the uninstall
            Console.WriteLine($"Failed to remove shortcuts: {ex.Message}");
        }
    }

    private void RemoveRegistryEntries()
    {
        try
        {
            Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\TetriON", false);
        }
        catch
        {
            try
            {
                Registry.CurrentUser.DeleteSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\TetriON", false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to remove registry entries: {ex.Message}");
            }
        }
    }

    private async Task RemoveGameFiles()
    {
        if (Directory.Exists(installPath))
        {
            // Get all files first
            var files = Directory.GetFiles(installPath, "*", SearchOption.AllDirectories);
            var totalFiles = files.Length;
            var deletedFiles = 0;

            foreach (var file in files)
            {
                try
                {
                    // Don't delete the uninstaller itself until the end
                    if (Path.GetFileName(file).Equals("uninstall.exe", StringComparison.OrdinalIgnoreCase))
                        continue;

                    File.Delete(file);
                    deletedFiles++;
                    
                    var progress = (int)((double)deletedFiles / totalFiles * 40) + 40; // 40-80 range
                    progressBar.Value = Math.Min(progress, 80);
                    
                    await Task.Delay(5); // Allow UI to update
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete {file}: {ex.Message}");
                }
            }

            // Remove empty directories
            try
            {
                var directories = Directory.GetDirectories(installPath, "*", SearchOption.AllDirectories)
                    .OrderByDescending(d => d.Length); // Delete deepest first

                foreach (var dir in directories)
                {
                    if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any())
                        Directory.Delete(dir);
                }

                // Try to remove the main directory (will fail if uninstaller is still running)
                if (!Directory.EnumerateFiles(installPath).Any(f => 
                    Path.GetFileName(f).Equals("uninstall.exe", StringComparison.OrdinalIgnoreCase)))
                {
                    Directory.Delete(installPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to remove directories: {ex.Message}");
            }
        }
    }

    private void RemoveUserData()
    {
        try
        {
            var userDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TetriON");
            if (Directory.Exists(userDataPath))
                Directory.Delete(userDataPath, true);

            var localDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TetriON");
            if (Directory.Exists(localDataPath))
                Directory.Delete(localDataPath, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to remove user data: {ex.Message}");
        }
    }

    private void UpdateStatus(string status)
    {
        statusLabel.Text = status;
        statusLabel.Refresh();
    }
}