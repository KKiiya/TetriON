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
            AutoSize = false,
        };

        // Buttons
        installButton = new Button
        {
            Text = "Install",
            Location = new Point(300, 320),
            Size = new Size(80, 30),
            Font = new Font(this.Font, FontStyle.Bold),
        };
        installButton.Click += InstallButton_Click;

        closeButton = new Button
        {
            Text = "Close",
            Location = new Point(390, 320),
            Size = new Size(80, 30),
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

        // Copy uninstaller files
        UpdateStatus("Installing uninstaller...");
        await CopyUninstaller(installPath);
        progressBar.Value = 70;

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

            // Handle Content/skins specially - extract to skins folder
            if (entry.FullName.StartsWith("Content/skins/", StringComparison.OrdinalIgnoreCase) ||
                entry.FullName.StartsWith("Content\\skins\\", StringComparison.OrdinalIgnoreCase))
            {

                // Remove "Content/" prefix and extract to installation directory
                var relativePath = entry.FullName.Substring(8); // Remove "Content/" or "Content\"
                var destinationPath = Path.Combine(installPath, relativePath);
                var destinationDir = Path.GetDirectoryName(destinationPath);

                if (!string.IsNullOrEmpty(destinationDir)) Directory.CreateDirectory(destinationDir);
                entry.ExtractToFile(destinationPath, true);
            }
            // Skip all other Content folder contents
            else if (entry.FullName.StartsWith("Content/", StringComparison.OrdinalIgnoreCase) ||
                     entry.FullName.StartsWith("Content\\", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            // Extract all other files normally
            else
            {
                var destinationPath = Path.Combine(installPath, entry.FullName);
                var destinationDir = Path.GetDirectoryName(destinationPath);

                if (!string.IsNullOrEmpty(destinationDir)) Directory.CreateDirectory(destinationDir);
                entry.ExtractToFile(destinationPath, true);
            }

            extractedFiles++;
            var fileProgress = (int)((double)extractedFiles / totalFiles * 40) + 10; // 10-50 range
            progressBar.Value = Math.Min(fileProgress, 50);

            UpdateStatus($"Extracting: {entry.Name}");
            await Task.Delay(10); // Allow UI to update
        }

        // Skip file organization - leave all files where .NET expects them
        UpdateStatus("Files extracted successfully");
        progressBar.Value = 60;
    }

    private async Task OrganizeInstallationFiles(string installPath)
    {
        try
        {
            var binFolder = Path.Combine(installPath, "bin");
            Directory.CreateDirectory(binFolder);

            // Get all DLL files in the installation folder
            var allDlls = Directory.GetFiles(installPath, "*.dll");

            // Move only non-essential libraries to /bin, keep .NET runtime core files in root
            var librariesToMove = new[] {
                // Third-party game libraries
                "MonoGame.Framework.dll",
                "NVorbis.dll",
                "SDL2.dll",
                "soft_oal.dll",
                // Non-essential .NET libraries that can be safely moved
                "Microsoft.CSharp.dll",
                "Microsoft.VisualBasic.Core.dll",
                "Microsoft.VisualBasic.dll",
                "Microsoft.Win32.Primitives.dll",
                "Microsoft.Win32.Registry.dll",
                "System.AppContext.dll",
                "System.Buffers.dll",
                "System.Collections.Concurrent.dll",
                "System.Collections.dll",
                "System.Collections.Immutable.dll",
                "System.Collections.NonGeneric.dll",
                "System.Collections.Specialized.dll",
                "System.ComponentModel.Annotations.dll",
                "System.ComponentModel.DataAnnotations.dll",
                "System.ComponentModel.dll",
                "System.ComponentModel.EventBasedAsync.dll",
                "System.ComponentModel.Primitives.dll",
                "System.ComponentModel.TypeConverter.dll",
                "System.Configuration.dll",
                "System.Console.dll",
                "System.Data.Common.dll",
                "System.Data.DataSetExtensions.dll",
                "System.Data.dll",
                "System.Diagnostics.Contracts.dll",
                "System.Diagnostics.Debug.dll",
                "System.Diagnostics.DiagnosticSource.dll",
                "System.Diagnostics.FileVersionInfo.dll",
                "System.Diagnostics.Process.dll",
                "System.Diagnostics.StackTrace.dll",
                "System.Diagnostics.TextWriterTraceListener.dll",
                "System.Diagnostics.Tools.dll",
                "System.Diagnostics.TraceSource.dll",
                "System.Diagnostics.Tracing.dll",
                "System.Drawing.dll",
                "System.Drawing.Primitives.dll",
                "System.Dynamic.Runtime.dll",
                "System.Formats.Asn1.dll",
                "System.Formats.Tar.dll",
                "System.Globalization.Calendars.dll",
                "System.Globalization.dll",
                "System.Globalization.Extensions.dll",
                "System.IO.Compression.Brotli.dll",
                "System.IO.Compression.dll",
                "System.IO.Compression.FileSystem.dll",
                "System.IO.Compression.ZipFile.dll",
                "System.IO.dll",
                "System.IO.FileSystem.AccessControl.dll",
                "System.IO.FileSystem.dll",
                "System.IO.FileSystem.DriveInfo.dll",
                "System.IO.FileSystem.Primitives.dll",
                "System.IO.FileSystem.Watcher.dll",
                "System.IO.IsolatedStorage.dll",
                "System.IO.MemoryMappedFiles.dll",
                "System.IO.Pipes.AccessControl.dll",
                "System.IO.Pipes.dll",
                "System.IO.UnmanagedMemoryStream.dll",
                "System.Linq.dll",
                "System.Linq.Expressions.dll",
                "System.Linq.Parallel.dll",
                "System.Linq.Queryable.dll",
                "System.Memory.dll",
                "System.Net.dll",
                "System.Net.Http.dll",
                "System.Net.Http.Json.dll",
                "System.Net.HttpListener.dll",
                "System.Net.Mail.dll",
                "System.Net.NameResolution.dll",
                "System.Net.NetworkInformation.dll",
                "System.Net.Ping.dll",
                "System.Net.Primitives.dll",
                "System.Net.Quic.dll",
                "System.Net.Requests.dll",
                "System.Net.Security.dll",
                "System.Net.ServicePoint.dll",
                "System.Net.Sockets.dll",
                "System.Net.WebClient.dll",
                "System.Net.WebHeaderCollection.dll",
                "System.Net.WebProxy.dll",
                "System.Net.WebSockets.Client.dll",
                "System.Net.WebSockets.dll",
                "System.Numerics.dll",
                "System.Numerics.Vectors.dll",
                "System.ObjectModel.dll",
                "System.Reflection.DispatchProxy.dll",
                "System.Reflection.dll",
                "System.Reflection.Emit.dll",
                "System.Reflection.Emit.ILGeneration.dll",
                "System.Reflection.Emit.Lightweight.dll",
                "System.Reflection.Extensions.dll",
                "System.Reflection.Metadata.dll",
                "System.Reflection.Primitives.dll",
                "System.Reflection.TypeExtensions.dll",
                "System.Resources.Reader.dll",
                "System.Resources.ResourceManager.dll",
                "System.Resources.Writer.dll",
                "System.Runtime.CompilerServices.Unsafe.dll",
                "System.Runtime.CompilerServices.VisualC.dll",
                "System.Runtime.Extensions.dll",
                "System.Runtime.Handles.dll",
                "System.Runtime.InteropServices.dll",
                "System.Runtime.InteropServices.JavaScript.dll",
                "System.Runtime.InteropServices.RuntimeInformation.dll",
                "System.Runtime.Intrinsics.dll",
                "System.Runtime.Loader.dll",
                "System.Runtime.Numerics.dll",
                "System.Runtime.Serialization.dll",
                "System.Runtime.Serialization.Formatters.dll",
                "System.Runtime.Serialization.Json.dll",
                "System.Runtime.Serialization.Primitives.dll",
                "System.Runtime.Serialization.Xml.dll",
                "System.Security.AccessControl.dll",
                "System.Security.Claims.dll",
                "System.Security.Cryptography.Algorithms.dll",
                "System.Security.Cryptography.Cng.dll",
                "System.Security.Cryptography.Csp.dll",
                "System.Security.Cryptography.dll",
                "System.Security.Cryptography.Encoding.dll",
                "System.Security.Cryptography.OpenSsl.dll",
                "System.Security.Cryptography.Primitives.dll",
                "System.Security.Cryptography.X509Certificates.dll",
                "System.Security.dll",
                "System.Security.Principal.dll",
                "System.Security.Principal.Windows.dll",
                "System.Security.SecureString.dll",
                "System.ServiceModel.Web.dll",
                "System.ServiceProcess.dll",
                "System.Text.Encoding.CodePages.dll",
                "System.Text.Encoding.dll",
                "System.Text.Encoding.Extensions.dll",
                "System.Text.Encodings.Web.dll",
                "System.Text.Json.dll",
                "System.Text.RegularExpressions.dll",
                "System.Threading.Channels.dll",
                "System.Threading.dll",
                "System.Threading.Overlapped.dll",
                "System.Threading.Tasks.Dataflow.dll",
                "System.Threading.Tasks.dll",
                "System.Threading.Tasks.Extensions.dll",
                "System.Threading.Tasks.Parallel.dll",
                "System.Threading.Thread.dll",
                "System.Threading.ThreadPool.dll",
                "System.Threading.Timer.dll",
                "System.Transactions.dll",
                "System.Transactions.Local.dll",
                "System.ValueTuple.dll",
                "System.Web.dll",
                "System.Web.HttpUtility.dll",
                "System.Windows.dll",
                "System.Xml.dll",
                "System.Xml.Linq.dll",
                "System.Xml.ReaderWriter.dll",
                "System.Xml.Serialization.dll",
                "System.Xml.XDocument.dll",
                "System.Xml.XmlDocument.dll",
                "System.Xml.XmlSerializer.dll",
                "System.Xml.XPath.dll",
                "System.Xml.XPath.XDocument.dll",
                "WindowsBase.dll"
            };

            var movedCount = 0;
            foreach (var dllPath in allDlls)
            {
                var fileName = Path.GetFileName(dllPath);

                // Move only explicitly listed libraries to /bin
                if (librariesToMove.Contains(fileName))
                {
                    var destPath = Path.Combine(binFolder, fileName);
                    File.Move(dllPath, destPath);
                    UpdateStatus($"Moved {fileName} to bin folder");
                    movedCount++;

                    await Task.Delay(25); // Allow UI to update
                }
            }

            UpdateStatus($"Moved {movedCount} library files to bin folder");

            // Move runtimes folder if it exists
            var runtimesSource = Path.Combine(installPath, "runtimes");
            var runtimesDest = Path.Combine(binFolder, "runtimes");

            if (Directory.Exists(runtimesSource))
            {
                Directory.Move(runtimesSource, runtimesDest);
                UpdateStatus("Moved runtimes to bin folder");
                await Task.Delay(50);
            }

            // For .NET Core/5+ apps, modify the runtimeconfig.json to include additional probe paths
            var runtimeConfigPath = Path.Combine(installPath, "TetriON.runtimeconfig.json");
            if (File.Exists(runtimeConfigPath))
            {
                try
                {
                    var runtimeConfigContent = @"{
  ""runtimeOptions"": {
    ""tfm"": ""net8.0"",
    ""framework"": {
      ""name"": ""Microsoft.NETCore.App"",
      ""version"": ""8.0.0""
    },
    ""additionalProbingPaths"": [
      ""bin""
    ],
    ""configProperties"": {
      ""System.Reflection.Metadata.MetadataUpdater.IsSupported"": false
    }
  }
}";
                    await File.WriteAllTextAsync(runtimeConfigPath, runtimeConfigContent);
                    UpdateStatus("Updated runtime configuration for /bin folder");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Runtime config modification failed: {ex.Message}");
                }
            }

            // Also create traditional .exe.config as fallback
            var configContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <runtime>
    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
      <probing privatePath=""bin"" />
    </assemblyBinding>
  </runtime>
</configuration>";

            var configPath = Path.Combine(installPath, "TetriON.exe.config");
            await File.WriteAllTextAsync(configPath, configContent);

            UpdateStatus("Created configuration files");
        }
        catch (Exception ex)
        {
            // If organization fails, continue anyway - the game should still work
            System.Diagnostics.Debug.WriteLine($"File organization failed: {ex.Message}");
        }
    }

    private async Task CopyUninstaller(string installPath)
    {
        try
        {
            // Get the directory where the installer is located (use AppContext.BaseDirectory for single-file apps)
            var installerDir = AppContext.BaseDirectory;
            var uninstallerPath = Path.Combine(installerDir, "uninstall.exe");

            // If uninstaller is not found in installer directory, try looking in common build locations
            if (!File.Exists(uninstallerPath))
            {
                // Try using Assembly location as fallback
                var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!string.IsNullOrEmpty(assemblyDir))
                {
                    uninstallerPath = Path.Combine(assemblyDir, "uninstall.exe");
                }

                // Try relative path from development build if still not found
                if (!File.Exists(uninstallerPath) && !string.IsNullOrEmpty(assemblyDir))
                {
                    uninstallerPath = Path.Combine(assemblyDir, "..", "TetriONUninstaller", "bin", "Release", "net8.0-windows", "TetriONUninstaller.exe");
                }
            }

            if (File.Exists(uninstallerPath))
            {
                var destPath = Path.Combine(installPath, "uninstall.exe");
                File.Copy(uninstallerPath, destPath, true);

                // Also copy dependencies if they exist
                var dependencies = new[] {
                    "TetriONUninstaller.dll",
                    "TetriONUninstaller.runtimeconfig.json",
                    "TetriONUninstaller.deps.json"
                };

                var uninstallerDir = Path.GetDirectoryName(uninstallerPath);
                foreach (var dep in dependencies)
                {
                    var srcDep = Path.Combine(uninstallerDir, dep);
                    var destDep = Path.Combine(installPath, dep.Replace("TetriONUninstaller", "uninstall"));

                    if (File.Exists(srcDep))
                    {
                        File.Copy(srcDep, destDep, true);
                    }
                }

                UpdateStatus("Uninstaller copied successfully");
            }
            else
            {
                UpdateStatus("Warning: Uninstaller not found, skipping...");
                System.Diagnostics.Debug.WriteLine($"Uninstaller not found at: {uninstallerPath}");
            }
        }
        catch (Exception ex)
        {
            // If uninstaller copy fails, continue anyway
            System.Diagnostics.Debug.WriteLine($"Uninstaller copy failed: {ex.Message}");
            UpdateStatus("Warning: Could not install uninstaller");
        }

        await Task.Delay(100); // Allow UI to update
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

    private void CheckDotNetRuntime()
    {
        if (!IsDotNet8Installed())
        {
            var result = MessageBox.Show(
                ".NET 8.0 Runtime is required to run TetriON but was not found on your system.\n\n" +
                "Would you like to download and install .NET 8.0 Runtime now?\n\n" +
                "Note: You can also download it manually from microsoft.com/dotnet",
                ".NET Runtime Required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://dotnet.microsoft.com/download/dotnet/8.0/runtime",
                        UseShellExecute = true
                    });
                    MessageBox.Show(
                        "Please install .NET 8.0 Runtime and then run the installer again.",
                        "Installation Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    Application.Exit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open download page: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    private bool IsDotNet8Installed()
    {
        try
        {
            // Check for .NET 8.0 runtime in registry
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedhost") ??
                             Registry.LocalMachine.OpenSubKey(@"SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost"))
            {
                if (key != null)
                {
                    var version = key.GetValue("Version")?.ToString();
                    if (!string.IsNullOrEmpty(version))
                    {
                        var versionParts = version.Split('.');
                        if (versionParts.Length >= 2 &&
                            int.TryParse(versionParts[0], out int major) &&
                            major >= 8)
                        {
                            return true;
                        }
                    }
                }
            }

            // Alternative check: try to run dotnet --version
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--list-runtimes",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processStartInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        var output = process.StandardOutput.ReadToEnd();
                        return output.Contains("Microsoft.NETCore.App 8.") ||
                               output.Contains("Microsoft.WindowsDesktop.App 8.");
                    }
                }
            }
            catch
            {
                // Ignore errors from dotnet command
            }

            return false;
        }
        catch
        {
            // If we can't check, assume it's installed to avoid blocking installation
            return true;
        }
    }
}
