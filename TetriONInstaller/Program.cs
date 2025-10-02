using System.Security.Principal;

namespace TetriONInstaller;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Check if running as administrator for better installation experience
        CheckAdministratorPrivileges();

        Application.Run(new InstallerForm());
    }

    private static void CheckAdministratorPrivileges()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        
        if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
        {
            var result = MessageBox.Show(
                "For the best installation experience, it's recommended to run the installer as Administrator.\n\n" +
                "Would you like to restart the installer with elevated privileges?",
                "Administrator Privileges",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = Application.ExecutablePath,
                        Verb = "runas",
                        UseShellExecute = true
                    };
                    
                    System.Diagnostics.Process.Start(startInfo);
                    Application.Exit();
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to restart with administrator privileges. Continuing with current privileges.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}