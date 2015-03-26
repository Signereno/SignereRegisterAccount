using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Unipluss.Sign.SignereRegisterAccount
{
    public partial class Register : Form
    {
        private readonly string _url;
        private readonly string _filepath;
        private readonly string Format;
        private readonly Guid Dealer;

        public Register(string dealer, string url, string filepath, string format)
        {
            _url = url;
            _filepath = filepath;
            Format = format.ToLowerInvariant();
            InitializeComponent();
            webBrowser.CausesValidation = false;
            webBrowser.ScriptErrorsSuppressed = true;
            this.WindowState = FormWindowState.Maximized;
            Dealer = new Guid(dealer);

            if (File.Exists(filepath))
            {
                Console.WriteLine("File: {0} exists already",filepath);
                Environment.Exit(31);
            }
        }

        private void Register_Load(object sender, EventArgs e)
        {
            webBrowser.Navigate(_url);
        }

        private void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            try
            {
                if (e.Url.ToString().ToLowerInvariant().Contains("downloadcredentials"))
                {
                    e.Cancel = true;
                    byte[] data = null;
                    using (var webClient = new WebClient())
                    {
                        data = webClient.DownloadData(e.Url);
                        if (Format.Equals("lic"))
                            File.WriteAllBytes(_filepath, data);
                    }

                    switch (Format)
                    {
                        case "json":
                            DownloadCredentials(data, "application/json");
                            break;
                        case "xml":
                            DownloadCredentials(data, "application/xml");
                            break;
                    }

                    webBrowser.Stop();
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                string msg= string.Format("Message: {0} Stack: {1} Source: {2} Timestamp: {3}", ex.Message, ex.StackTrace, ex.Source, DateTime.Now);
                Console.WriteLine(msg);
                if (File.Exists("SignereLog.txt"))
                    File.Delete("SignereLog.txt");
                File.WriteAllText("SignereLog.txt",msg);
                Environment.Exit(3);
            }
        }

        private void DownloadCredentials(byte[] data, string format)
        {
            string url1 = string.Format("https://api2.signere.no/api/license/{0}", Dealer);
            string url2 = string.Format("https://testapi.signere.no/api/license/{0}", Dealer);
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("Accept", format);
                    string stringData = Encoding.UTF8.GetString(webClient.UploadData(url1, "POST", data));
                    File.WriteAllText(_filepath, stringData);
                }
            }
            catch (WebException wex)
            {
                try
                {
                    var webResponse = ((HttpWebResponse)wex.Response);
                    if (webResponse.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        using (var webClient = new WebClient())
                        {
                            webClient.Headers.Add("Accept", format);
                            string stringData = Encoding.UTF8.GetString(webClient.UploadData(url2, "POST", data));
                            File.WriteAllText(_filepath, stringData);
                        }
                    }

                }
                catch (Exception ex2)
                {
                    string msg = string.Format("Message: {0} Stack: {1} Source: {2} Timestamp: {3}", ex2.Message, ex2.StackTrace, ex2.Source, DateTime.Now);
                    Console.WriteLine(msg);
                    if (File.Exists("SignereLog.txt"))
                        File.Delete("SignereLog.txt");
                    File.WriteAllText("SignereLog.txt", msg);
                    Environment.Exit(32);
                }
            }
        }

        private void abortButton_Click(object sender, EventArgs e)
        {
            Environment.Exit(1);
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            HelpBox box = new HelpBox();
            box.Show(this);
        }




    }
}
