using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace WebLearningOffline
{
    public static class Http
    {
        public static string GetResponse(ref HttpWebRequest req, out CookieCollection cookies, bool NeedResponse = true)
        {
            HttpWebResponse res = null;
            cookies = null;
            try
            {
                res = (HttpWebResponse)req.GetResponse();
                cookies = res.Cookies;
            }
            catch (Exception)
            {
                try
                {
                    res.Close();
                }
                catch (Exception) { }
            }
            if (NeedResponse)
            {
                StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
                string respHTML = reader.ReadToEnd();
                reader.Close();
                res.Close();
                //Console.WriteLine(respHTML);
                return respHTML;
            }
            else
            {
                res.Close();
                return "";
            }
        }
        public static string GetResponse(ref HttpWebRequest req, bool NeedResponse = true)
        {
            CookieCollection c;
            return GetResponse(ref req, out c, NeedResponse);
        }
        public static HttpWebRequest GenerateRequest(string URL, string Method, bool KeepAlive = false, string ContentType = null, byte[] data = null, int offset = 0, int length = 0, int Timeout = 20 * 1000, string host = null, string Referer = null, string Accept = null, CookieCollection cookies = null)
        {
            Uri httpUrl = new Uri(URL);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(httpUrl);
            req.ProtocolVersion = new System.Version("1.0");
            req.Timeout = Timeout;
            req.ReadWriteTimeout = Timeout;
            req.Method = Method;
            req.KeepAlive = KeepAlive;
            if (ContentType != null) req.ContentType = ContentType;
            if (Referer != null) req.Referer = Referer;
            if (Accept != null) req.Accept = Accept;
            if (cookies != null)
            {
                req.CookieContainer = new CookieContainer();
                req.CookieContainer.Add(cookies);
            }
            if (data != null)
            {
                req.ContentLength = length;
                Stream stream = req.GetRequestStream();
                stream.Write(data, offset, length);
                stream.Close();
            }
            return req;
        }
        public static string Get(string URL, bool AllowAutoRedirect = true, bool NeedResponse = true, int Timeout = 5 * 1000, string host = null, CookieCollection cookiesin = null)
        {
            HttpWebRequest req = GenerateRequest(URL, "GET", false, null, null, 0, 0, Timeout, host, cookies: cookiesin);
            if (AllowAutoRedirect == false) req.AllowAutoRedirect = false;
            return GetResponse(ref req, NeedResponse);
        }
        public static string Get(string URL, out CookieCollection cookies, bool GetLocation = false, bool AllowAutoRedirect = true, bool NeedResponse = true, int Timeout = 5 * 1000, string host = null, CookieCollection cookiesin = null)
        {
            HttpWebRequest req = GenerateRequest(URL, "GET", false, null, null, 0, 0, Timeout, host, cookies: cookiesin);
            if (AllowAutoRedirect == false) req.AllowAutoRedirect = false;
            var newcookies = new CookieCollection();
            var ret= GetResponse(ref req, out newcookies, NeedResponse);
            if (cookiesin == null) cookies = newcookies;
            else
            {
                cookiesin.Add(newcookies);
                cookies = cookiesin;
            }
            return ret;
        }
        public static string Post(string URL, byte[] data, int offset = 0, int length = -1, bool NeedResponse = true, int Timeout = 20 * 1000, string host = null, string ContentType = null, string Referer = null, string Accept = null, CookieCollection cookiesin = null)
        {
            if (length == -1) length = data.Length;
            HttpWebRequest req = GenerateRequest(URL, "POST", false, ContentType, data, 0, data.Length, Timeout, host, Referer, Accept, cookiesin);
            return GetResponse(ref req, NeedResponse);
        }
        public static string Post(string URL, byte[] data, out CookieCollection cookies, int offset = 0, int length = -1, bool NeedResponse = true, int Timeout = 20 * 1000, string host = null, string ContentType = null, string Referer = null, string Accept = null, CookieCollection cookiesin = null)
        {
            if (length == -1) length = data.Length;
            HttpWebRequest req = GenerateRequest(URL, "POST", false, ContentType, data, 0, data.Length, Timeout, host, Referer, Accept, cookiesin);
            var newcookies = new CookieCollection();
            var ret = GetResponse(ref req, out newcookies, NeedResponse);
            if (cookiesin == null) cookies = newcookies;
            else
            {
                cookiesin.Add(newcookies);
                cookies = cookiesin;
            }
            return ret;
        }
    }
}
