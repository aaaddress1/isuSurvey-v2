using System;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Drawing;

using System.Threading;

namespace Survey
{

    /// <summary>
    /// 棍棍der C# 實做
    /// GitHub :https://gist.github.com/Inndy/8763264
    /// </summary>
    public class SpWebClient : WebClient
    {
        public const string ChromeUserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.76 Safari/537.36";
        public const string FireFoxUserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:29.0) Gecko/20100101 Firefox/29.0";
        public CookieContainer CookieContainer { get; private set; }
        public Uri ResponseUri { get; private set; }

        public SpWebClient()
            : base()
        {
            this.CookieContainer = new CookieContainer();
            this.ResponseUri = null;
            //this.Proxy = null;
        }

        public SpWebClient(CookieContainer CookieContainer)
            : base()
        {
            this.CookieContainer = CookieContainer;
            this.ResponseUri = null;
            this.Proxy = null;
        }

        public string DownloadString(string Uri, Encoding Encoding)
        {
                return Encoding.GetString(this.DownloadData(Uri));

        }

        public string DownloadString(Uri Uri, Encoding Encoding)
        {

                return Encoding.GetString(this.DownloadData(Uri));
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            HttpWebRequest webRequest = request as HttpWebRequest;
            if (webRequest != null) webRequest.CookieContainer = this.CookieContainer;
            //webRequest.Timeout = 5 * 1000;
            return request;
        }
        public void LoadCookieStrFromSource(string Source, string nDomain)
        {
            string CookieStr = Source.Replace(" ", "").Replace("\n", "");
            string[] items = CookieStr.Split(';');
            for (int i = 0; i < items.Length; i++)
            {
                Cookie nCookie = new Cookie(items[i].Split('=')[0], items[i].Split('=')[1]);
                nCookie.Domain = nDomain;
                this.CookieContainer.Add(nCookie);
            }
        }

        public string GetCookieStr(string URL)
        {
            return this.CookieContainer.GetCookieHeader(new Uri(URL));
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            try
            {
                WebResponse response = base.GetWebResponse(request);
                this.ResponseUri = response.ResponseUri;
                return response;
            }
            catch (Exception ex)
            {
            }
            return null;

        }

    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////


}
