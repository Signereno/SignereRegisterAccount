using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

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

            #region Fix IE compability mode
            // http://msdn.microsoft.com/en-us/library/ee330720(v=vs.85).aspx

            // FeatureControl settings are per-process
            var fileName = System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

            // make the control is not running inside Visual Studio Designer
            if (String.Compare(fileName, "devenv.exe", true) == 0 || String.Compare(fileName, "XDesProc.exe", true) == 0)
                return;

            SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, GetBrowserEmulationMode()); // Webpages containing standards-based !DOCTYPE directives are displayed in IE10 Standards mode.
            SetBrowserFeatureControlKey("FEATURE_AJAX_CONNECTIONEVENTS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_MANAGE_SCRIPT_CIRCULAR_REFS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_DOMSTORAGE ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_GPU_RENDERING ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_IVIEWOBJECTDRAW_DMLT9_WITH_GDI  ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_LEGACY_COMPRESSION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_LOCALMACHINE_LOCKDOWN", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_OBJECT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_SCRIPT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_NAVIGATION_SOUNDS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SCRIPTURL_MITIGATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SPELLCHECKING", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_STATUS_BAR_THROTTLING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_TABBED_BROWSING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_VALIDATE_NAVIGATE_URL", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_DOCUMENT_ZOOM", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_POPUPMANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_MOVESIZECHILD", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ADDON_MANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBSOCKET", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WINDOW_RESTRICTIONS ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_XMLHTTP", fileName, 1);

            #endregion
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
                else if (e.Url.ToString().ToLowerInvariant().Contains("useraborted"))
                {
                    Environment.Exit(1);
                }else if (e.Url.ToString().ToLowerInvariant().Contains("error"))
                {
                    Environment.Exit(33);
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

        #region More IE fix
        private void SetBrowserFeatureControlKey(string feature, string appName, uint value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(
                String.Concat(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\", feature),
                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue(appName, (UInt32)value, RegistryValueKind.DWord);
            }
        }

        private UInt32 GetBrowserEmulationMode()
        {
            int browserVersion = 7;
            using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                RegistryKeyPermissionCheck.ReadSubTree,
                System.Security.AccessControl.RegistryRights.QueryValues))
            {
                var version = ieKey.GetValue("svcVersion");
                if (null == version)
                {
                    version = ieKey.GetValue("Version");
                    if (null == version)
                        throw new ApplicationException("Microsoft Internet Explorer is required!");
                }
                int.TryParse(version.ToString().Split('.')[0], out browserVersion);
            }

            UInt32 mode = 10000; // Internet Explorer 10. Webpages containing standards-based !DOCTYPE directives are displayed in IE10 Standards mode. Default value for Internet Explorer 10.
            switch (browserVersion)
            {
                case 7:
                    mode = 7000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode. Default value for applications hosting the WebBrowser Control.
                    break;
                case 8:
                    mode = 8000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode. Default value for Internet Explorer 8
                    break;
                case 9:
                    mode = 9000; // Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode. Default value for Internet Explorer 9.
                    break;
                default:
                    // use IE10 mode by default
                    break;
            }

            return mode;
        }
        #endregion


    }
}
