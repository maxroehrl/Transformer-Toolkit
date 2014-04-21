using System;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace Toolkit
{
    static class Program
    {

        [STAThread]
        private static void Main()
        {
            // Load AndroidLib.dll from embedded resources
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string dllName = args.Name.Contains(",")
                    ? args.Name.Substring(0, args.Name.IndexOf(','))
                    : args.Name.Replace(".dll", String.Empty);
                dllName = dllName.Replace(".", "_");

                if (dllName.EndsWith("_resources"))
                    return null;

                ResourceManager resourceManager = new ResourceManager("Toolkit.Properties.Resources",
                    Assembly.GetExecutingAssembly());
                return Assembly.Load((byte[]) resourceManager.GetObject(dllName));
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StartDialog());
        }
    }
}
