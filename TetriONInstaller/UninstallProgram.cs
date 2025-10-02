using System;
using System.Windows.Forms;

namespace TetriONInstaller
{
    internal static class UninstallProgram
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string installPath = args.Length > 0 ? args[0] : Environment.CurrentDirectory;
            Application.Run(new UninstallerForm(installPath));
        }
    }
}