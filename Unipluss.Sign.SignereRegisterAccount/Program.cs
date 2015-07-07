using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Unipluss.Sign.SignereRegisterAccount
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string argsText = "";
            foreach (var s in args)
            {
                argsText += s + Environment.NewLine;
            }
            MessageBox.Show(argsText);

            if (args == null || args.Length == 0)
            {

                MessageBox.Show("Missing arguments first argument is the dealerid (GUID) second: register url," +
                                " third: is filepath for licensefile/credentilas and forth (optional) format (json,xml or licensefile)",
                    "En feil oppstod", MessageBoxButtons.OK, MessageBoxIcon.Error);

                System.Diagnostics.Debug.WriteLine("Missing arguments first argument is the dealerid (GUID) second: register url," +
                                  " third: is filepath for licensefile/credentilas and forth (optional) format (json,xml or licensefile)");
                Environment.ExitCode=2;
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                string format = "lic";
                if (args.Length > 3)
                {
                    format = args[3];
                }

                Application.Run(new Register(args[0], args[1], args[2], format));
            }




        }
    }
}
