/*
 * Program.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.Windows.Forms;

namespace Toolkit
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Adb.KillAllAdbProcesses();
            
            ResourceManager.ExtractAdbBinaries();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StartDialog());
        }
    }
}