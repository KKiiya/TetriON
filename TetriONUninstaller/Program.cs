using System.Security.Principal;

namespace TetriONUninstaller;

internal static class Program {
    [STAThread]
    static void Main(string[] args) {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Check if running as administrator for better uninstallation experience
        if (!CheckAdministratorPrivileges()) {
            return; // Exit if we're restarting with elevated privileges
        }

        string installPath = args.Length > 0 ? args[0] : Environment.CurrentDirectory;
        Application.Run(new UninstallerForm(installPath));
    }

    private static bool CheckAdministratorPrivileges() {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

        if (!isAdmin) {
            var result = MessageBox.Show(
                "TetriON Uninstaller works best when run as administrator.\n\n" +
                "Would you like to restart as administrator?",
                "Administrator Privileges",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes) {
                try {
                    var startInfo = new System.Diagnostics.ProcessStartInfo {
                        FileName = Application.ExecutablePath,
                        UseShellExecute = true,
                        Verb = "runas"
                    };

                    System.Diagnostics.Process.Start(startInfo);
                    return false; // Return false to indicate we should exit
                } catch (Exception ex) {
                    MessageBox.Show($"Failed to restart as administrator: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true; // Continue with current privileges
                }
            }
        }
        return true; // Continue normally (either already admin or user chose No)
    }
}
