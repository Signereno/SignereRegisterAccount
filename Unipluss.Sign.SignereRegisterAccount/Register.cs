using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Unipluss.Sign.SignereRegisterAccount
{
    public partial class Register : Form
    {
        private  string _url;
        private readonly string _filepath;
        private readonly string Format;
        private readonly Guid Dealer;
        private WebBrowserEvents _events;

        public Register(string dealer, string url, string filepath, string format)
        {
            _url = url;
            _filepath = filepath;
            Format = format.ToLowerInvariant();
            InitializeComponent();
            Dealer = new Guid(dealer);
            _events = new WebBrowserEvents(webBrowser, Dealer, format, filepath); 

            webBrowser.CausesValidation = false;
            webBrowser.ScriptErrorsSuppressed = true;
            
            this.WindowState = FormWindowState.Maximized;

            
            if (string.IsNullOrEmpty(filepath) ||!Directory.Exists(Path.GetDirectoryName(filepath)))
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Filepath: {0} is not valid", filepath));
                Environment.Exit(34);
            }

            if (File.Exists(filepath))
            {
                System.Diagnostics.Debug.WriteLine(string.Format("File: {0} exists already", filepath));
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
            webBrowser.Navigate(_url,null,null,"X-Client:SignereRegisterAccount");
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
                else if (e.Url.ToString().ToLowerInvariant().Contains("useraborted") || e.Url.ToString().ToLowerInvariant().Contains("userabort") || e.Url.ToString().ToLowerInvariant().Contains("abort"))
                {
                    Environment.Exit(1);
                }
                else if (e.Url.ToString().ToLowerInvariant().Contains("error") &&
                         !e.Url.ToString().ToLowerInvariant().Contains("errorurl="))
                {
                    Environment.Exit(33);
                }
            }
            catch (Exception ex)
            {
                string msg= string.Format("Message: {0} Stack: {1} Source: {2} Timestamp: {3}", ex.Message, ex.StackTrace, ex.Source, DateTime.Now);
                System.Diagnostics.Debug.WriteLine(msg);
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
                    if (webResponse.StatusCode == HttpStatusCode.InternalServerError || webResponse.StatusCode==HttpStatusCode.NotFound)
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
                    System.Diagnostics.Debug.WriteLine(msg);
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
            StringBuilder sb=new StringBuilder();

            sb.AppendLine(
                "Dette programmet er laget for å hjelpe deg å registere en konto på Signere.no Norgest enkleste vei til elektronisk signering.");
            sb.AppendLine("Dersom du opplever problemer ta kontakt med program leverandøren din");

            MessageBox.Show(this, sb.ToString(), "Om programmet", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            //HelpBox box = new HelpBox();
            //box.Show(this);
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

        private void Register_FormClosed(object sender, FormClosedEventArgs e)
        {
           _events.Dispose();
        }

        private void Register_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.ExitCode=1;
        }


    }



    // this event sink declares the NewWindow3 event
    public class WebBrowserEvents : StandardOleMarshalObject, DWebBrowserEvents2, IDisposable
    {
        private readonly Guid _dealerId;
        private readonly string Format;
        private readonly string _filepath;
        private AxHost.ConnectionPointCookie _cookie;
        private readonly IList<string> tempfiles = new List<string>();

        public WebBrowserEvents(WebBrowser wb,Guid dealerId,string format,string filepath)
        {
            _dealerId = dealerId;
            Format = format;
            _filepath = filepath;
            _cookie = new AxHost.ConnectionPointCookie(wb.ActiveXInstance, this, typeof(DWebBrowserEvents2));
        }

      

        #region Un used events
        void DWebBrowserEvents2.StatusTextChange(string text)
        {
            
        }

        void DWebBrowserEvents2.ProgressChange(int progress, int progressMax)
        {
         
        }

        void DWebBrowserEvents2.CommandStateChange(int command, bool enable)
        {
            
        }

        void DWebBrowserEvents2.DownloadBegin()
        {
            
        }

        void DWebBrowserEvents2.DownloadComplete()
        {
    
        }

        void DWebBrowserEvents2.TitleChange(string text)
        {

        }

        void DWebBrowserEvents2.PropertyChange(string szProperty)
        {

        }

        void DWebBrowserEvents2.BeforeNavigate2(object pDisp, ref object URL, ref object flags, ref object targetFrameName, ref object postData, ref object headers, ref bool cancel)
        {

        }

        void DWebBrowserEvents2.NewWindow2(ref object pDisp, ref bool cancel)
        {

        }

        void DWebBrowserEvents2.NavigateComplete2(object pDisp, ref object URL)
        {
  
        }

        void DWebBrowserEvents2.DocumentComplete(object pDisp, ref object URL)
        {

        }

        void DWebBrowserEvents2.OnQuit()
        {

        }

        void DWebBrowserEvents2.OnVisible(bool visible)
        {

        }

        void DWebBrowserEvents2.OnToolBar(bool toolBar)
        {

        }

        void DWebBrowserEvents2.OnMenuBar(bool menuBar)
        {

        }

        void DWebBrowserEvents2.OnStatusBar(bool statusBar)
        {

        }

        void DWebBrowserEvents2.OnFullScreen(bool fullScreen)
        {

        }

        void DWebBrowserEvents2.OnTheaterMode(bool theaterMode)
        {

        }

        void DWebBrowserEvents2.WindowSetResizable(bool resizable)
        {

        }

        void DWebBrowserEvents2.WindowSetLeft(int left)
        {

        }

        void DWebBrowserEvents2.WindowSetTop(int top)
        {

        }

        void DWebBrowserEvents2.WindowSetWidth(int width)
        {
    
        }

        void DWebBrowserEvents2.WindowSetHeight(int height)
        {
            
        }

        void DWebBrowserEvents2.WindowClosing(bool isChildWindow, ref bool cancel)
        {
            
        }

        void DWebBrowserEvents2.ClientToHostWindow(ref int cx, ref int cy)
        {
            
        }

        void DWebBrowserEvents2.SetSecureLockIcon(int secureLockIcon)
        {
            
        }

        void DWebBrowserEvents2.FileDownload(ref bool cancel)
        {
            
        }

        void DWebBrowserEvents2.NavigateError(object pDisp, ref object URL, ref object frame, ref object statusCode, ref bool cancel)
        {
            
        }

        void DWebBrowserEvents2.PrintTemplateInstantiation(object pDisp)
        {
            
        }

        void DWebBrowserEvents2.PrintTemplateTeardown(object pDisp)
        {
            
        }

        void DWebBrowserEvents2.UpdatePageStatus(object pDisp, ref object nPage, ref object fDone)
        {
            
        }

        void DWebBrowserEvents2.PrivacyImpactedStateChange(bool bImpacted)
        {
            
        }
        #endregion

        void DWebBrowserEvents2.NewWindow3(ref object pDisp, ref bool cancel, int dwFlags, ref object bstrUrlContext, ref object bstrUrl)
        {
            cancel = true;

            using (var webClient = new WebClient())
            {
                webClient.Headers.Add(HttpRequestHeader.UserAgent, "Unipluss.Sign.SignereRegisterAccount");
                webClient.Headers.Add("DealerId", _dealerId.ToString());
                if (bstrUrl != null)
                {
                    var result = webClient.DownloadData(bstrUrl as string);

                    string filepath = string.Format("{0}{1}.pdf", System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("n"));
                    tempfiles.Add(filepath);
                    File.WriteAllBytes(filepath, result);
                    System.Diagnostics.Process.Start(filepath);
                }
            }
        }
        
        public void Dispose()
        {
            if (_cookie != null)
            {
                _cookie.Disconnect();
                _cookie = null;
            }

            foreach (var tempfile in tempfiles)
            {
                try
                {
                    if (File.Exists(tempfile))
                        File.Delete(tempfile);
                }
                catch (Exception) { }

            }
        }
    }
}
