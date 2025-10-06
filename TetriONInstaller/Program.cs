using System.Security.Principal;

namespace TetriONInstaller;

internal static class Program {
    [STAThread]
    static void Main() {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Check if running as administrator for better installation experience
        if (!CheckAdministratorPrivileges()) {
            return; // Exit if we're restarting with elevated privileges
        }

        Application.Run(new InstallerForm());
    }

    private static bool CheckAdministratorPrivileges() {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        if (!principal.IsInRole(WindowsBuiltInRole.Administrator)) {
            var result = MessageBox.Show(
                "For the best installation experience, it's recommended to run the installer as Administrator.\n\n"+
                "Would you like to restart the installer with elevated privileges?",
                "Administrator Privileges",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes) {
                try {
                    var startInfo = new System.Diagnostics.ProcessStartInfo {
                        FileName = Application.ExecutablePath,
                        Verb = "runas",
                        UseShellExecute = true,
                    };
                    System.Diagnostics.Process.Start(startInfo);
                    return false; // Return false to indicate we should exit
                } catch (Exception) {
                    MessageBox.Show(
                        "Failed to restart with administrator privileges. Continuing with current privileges.",
                        "Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return true; // Continue with current privileges
                }
            }
        }
        return true; // Continue normally (either already admin or user chose No)
    }
}