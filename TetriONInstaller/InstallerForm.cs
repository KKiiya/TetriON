using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using Microsoft.Win32;

namespace TetriONInstaller;

public partial class InstallerForm : Form
{
    private ProgressBar progressBar;
    private Label statusLabel;
    private Button installButton;
    private Button closeButton;
    private TextBox installPathTextBox;
    private Button browseButton;
    private CheckBox createDesktopShortcut;
    private CheckBox createStartMenuShortcut;
    
    private string defaultInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "TetriON");
    private bool isInstalling = false;

    public InstallerForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Form setup
        this.Text = "TetriON Installer";
        this.Size = new Size(500, 400);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // Install path selection
        var pathLabel = new Label
        {
            Text = "Installation Directory:",
            Location = new Point(20, 20),
            Size = new Size(150, 23),
            AutoSize = true
        };

        installPathTextBox = new TextBox
        {
            Text = defaultInstallPath,
            Location = new Point(20, 50),
            Size = new Size(350, 23)
        };

        browseButton = new Button
        {
            Text = "Browse...",
            Location = new Point(380, 49),
            Size = new Size(80, 25)
        };
        browseButton.Click += BrowseButton_Click;

        // Shortcuts options
        createDesktopShortcut = new CheckBox
        {
            Text = "Create desktop shortcut",
            Location = new Point(20, 90),
            Size = new Size(200, 23),
            Checked = true
        };

        createStartMenuShortcut = new CheckBox
        {
            Text = "Create Start Menu shortcut",
            Location = new Point(20, 120),
            Size = new Size(200, 23),
            Checked = true
        };

        // Progress bar
        progressBar = new ProgressBar
        {
            Location = new Point(20, 180),
            Size = new Size(440, 23),
            Style = ProgressBarStyle.Continuous
        };

        // Status label
        statusLabel = new Label
        {
            Text = "Ready to install TetriON",
            Location = new Point(20, 210),
            Size = new Size(440, 23),
            AutoSize = false
        };

        // Buttons
        installButton = new Button
        {
            Text = "Install",
            Location = new Point(300, 320),
            Size = new Size(80, 30),
            Font = new Font(this.Font, FontStyle.Bold)
        };
        installButton.Click += InstallButton_Click;

        closeButton = new Button
        {
            Text = "Close",
            Location = new Point(390, 320),
            Size = new Size(80, 30)
        };
        closeButton.Click += (s, e) => this.Close();

        // Add controls to form
        this.Controls.AddRange(new Control[] {
            pathLabel, installPathTextBox, browseButton,
            createDesktopShortcut, createStartMenuShortcut,
            progressBar, statusLabel, installButton, closeButton
        });
    }

    private void BrowseButton_Click(object sender, EventArgs e)
    {
        using var folderDialog = new FolderBrowserDialog();
        folderDialog.Description = "Select installation directory";
        folderDialog.SelectedPath = installPathTextBox.Text;

        if (folderDialog.ShowDialog() == DialogResult.OK)
        {
            installPathTextBox.Text = folderDialog.SelectedPath;
        }
    }

    private async void InstallButton_Click(object sender, EventArgs e)
    {
        if (isInstalling) return;

        try
        {
            isInstalling = true;
            installButton.Enabled = false;
            browseButton.Enabled = false;
            installPathTextBox.Enabled = false;

            await PerformInstallation();

            MessageBox.Show("TetriON has been installed successfully!", "Installation Complete", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // Ask if user wants to launch the game
            var result = MessageBox.Show("Would you like to launch TetriON now?", "Launch Game", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                LaunchGame();
            }

            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Installation failed: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            isInstalling = false;
            installButton.Enabled = true;
            browseButton.Enabled = true;
            installPathTextBox.Enabled = true;
        }
    }

    private async Task PerformInstallation()
    {
        var installPath = installPathTextBox.Text;
        
        // Create installation directory
        UpdateStatus("Creating installation directory...");
        Directory.CreateDirectory(installPath);
        progressBar.Value = 10;

        // Extract game files
        UpdateStatus("Extracting game files...");
        await ExtractGameFiles(installPath);
        progressBar.Value = 60;

        // Create shortcuts
        if (createDesktopShortcut.Checked)
        {
            UpdateStatus("Creating desktop shortcut...");
            CreateDesktopShortcut(installPath);
        }
        progressBar.Value = 80;

        if (createStartMenuShortcut.Checked)
        {
            UpdateStatus("Creating Start Menu shortcut...");
            CreateStartMenuShortcut(installPath);
        }
        progressBar.Value = 90;

        // Register uninstaller
        UpdateStatus("Registering uninstaller...");
        RegisterUninstaller(installPath);
        progressBar.Value = 100;

        UpdateStatus("Installation completed successfully!");
    }

    private async Task ExtractGameFiles(string installPath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "TetriONInstaller.game_files.zip";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new InvalidOperationException("Game files not found in installer");

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        
        var totalFiles = archive.Entries.Count;
        var extractedFiles = 0;

        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name)) continue;

            var destinationPath = Path.Combine(installPath, entry.FullName);
            var destinationDir = Path.GetDirectoryName(destinationPath);
            
            if (!string.IsNullOrEmpty(destinationDir))
                Directory.CreateDirectory(destinationDir);

            entry.ExtractToFile(destinationPath, true);
            
            extractedFiles++;
            var fileProgress = (int)((double)extractedFiles / totalFiles * 50) + 10; // 10-60 range
            progressBar.Value = Math.Min(fileProgress, 60);
            
            UpdateStatus($"Extracting: {entry.Name}");
            await Task.Delay(10); // Allow UI to update
        }
    }

    private void CreateDesktopShortcut(string installPath)
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var shortcutPath = Path.Combine(desktopPath, "TetriON.lnk");
        var targetPath = Path.Combine(installPath, "TetriON.exe");
        
        CreateShortcut(shortcutPath, targetPath, installPath);
    }

    private void CreateStartMenuShortcut(string installPath)
    {
        var startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "TetriON");
        Directory.CreateDirectory(startMenuPath);
        
        var shortcutPath = Path.Combine(startMenuPath, "TetriON.lnk");
        var targetPath = Path.Combine(installPath, "TetriON.exe");
        
        CreateShortcut(shortcutPath, targetPath, installPath);
    }

    private void CreateShortcut(string shortcutPath, string targetPath, string workingDirectory)
    {
        // Using WScript.Shell COM object to create shortcuts
        var shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
        var shortcut = shell.GetType().InvokeMember("CreateShortcut", 
            System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
        
        shortcut.GetType().InvokeMember("TargetPath", 
            System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { targetPath });
        shortcut.GetType().InvokeMember("WorkingDirectory", 
            System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { workingDirectory });
        shortcut.GetType().InvokeMember("Description", 
            System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { "TetriON Game" });
        shortcut.GetType().InvokeMember("Save", 
            System.Reflection.BindingFlags.InvokeMethod, null, shortcut, null);
    }

    private void RegisterUninstaller(string installPath)
    {
        try
        {
            using var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\TetriON");
            
            key.SetValue("DisplayName", "TetriON");
            key.SetValue("DisplayVersion", "1.0.0");
            key.SetValue("Publisher", "TetriON Team");
            key.SetValue("InstallLocation", installPath);
            key.SetValue("UninstallString", $"\"{Path.Combine(installPath, "uninstall.exe")}\"");
            key.SetValue("DisplayIcon", Path.Combine(installPath, "TetriON.exe"));
            key.SetValue("NoModify", 1);
            key.SetValue("NoRepair", 1);
        }
        catch (UnauthorizedAccessException)
        {
            // If we can't write to HKLM, try HKCU
            using var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\TetriON");
            
            key.SetValue("DisplayName", "TetriON");
            key.SetValue("DisplayVersion", "1.0.0");
            key.SetValue("Publisher", "TetriON Team");
            key.SetValue("InstallLocation", installPath);
            key.SetValue("UninstallString", $"\"{Path.Combine(installPath, "uninstall.exe")}\"");
            key.SetValue("DisplayIcon", Path.Combine(installPath, "TetriON.exe"));
            key.SetValue("NoModify", 1);
            key.SetValue("NoRepair", 1);
        }
    }

    private void LaunchGame()
    {
        try
        {
            var gamePath = Path.Combine(installPathTextBox.Text, "TetriON.exe");
            Process.Start(new ProcessStartInfo
            {
                FileName = gamePath,
                UseShellExecute = true,
                WorkingDirectory = installPathTextBox.Text
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to launch game: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void UpdateStatus(string status)
    {
        statusLabel.Text = status;
        statusLabel.Refresh();
    }
}