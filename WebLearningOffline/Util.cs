﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;

namespace WebLearningOffline
{
    public class Course
    {
        public bool selected;
        public bool isnew;
        public string id;
        public string name;
        public string term;
    }

    public static class Util
    {
        public static string safepath(string s)
        {
            s = HttpUtility.HtmlDecode(s);
            foreach (var c in Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()))
            {
                s = s.Replace(c, ' ');
            }
            return s;
        }
        public static void downfile(string url, string file, CookieCollection cookies)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    using (var wc = new CookieAwareWebClient(cookies))
                        wc.DownloadFile(url, file);
                    break;
                }
                catch (Exception e)
                {
                    if (i == 4) throw e;
                }
            }
        }
        public static Dictionary<string, object> initdict(Course c)
        {
            var dict = new Dictionary<string, object>();
            dict.Add("CourseId", c.id);
            dict.Add("IsNew", c.isnew ? "Yes" : "No");
            dict.Add("CourseName", c.name);
            dict.Add("CourseTerm", c.term);
            return dict;
        }

        public static void writehtml(string infile, string outfile, Dictionary<string, object> array)
        {
            using (var sr = new StreamReader(infile))
            using (var sw = new StreamWriter(outfile))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var match = Regex.Match(line, "<!-- replace (.+?) -->");
                    if (match.Success)
                    {
                        sw.WriteLine(replacevar(match.Groups[1].Value, array));
                    }
                    else
                    {
                        match = Regex.Match(line, "<!-- foreach (\\w+) (.+?) -->");
                        if (match.Success)
                        {
                            var aname = match.Groups[1].Value;
                            var templ = match.Groups[2].Value;
                            if (!array.ContainsKey(aname)) throw new Exception("模板错误 没有变量" + aname);
                            if (array[aname].GetType() != typeof(List<Dictionary<string, object>>)) throw new Exception("模板错误 不是数组" + aname);
                            var list = (List<Dictionary<string, object>>)array[aname];
                            list.ForEach(e => sw.WriteLine(replacevar(templ, e)));
                        }
                        else sw.WriteLine(line);
                    }
                }
            }
        }

        public static string replacevar(string str, Dictionary<string, object> array)
        {
            var ret = new StringBuilder();
            var varname = new StringBuilder();
            var building = false;
            var iifstage = 0;
            var iifresult = false;
            str += " ";
            foreach (var c in str)
            {
                if (building)
                {
                    if (char.IsLetterOrDigit(c)) varname.Append(c);
                    else
                    {
                        var aname = varname.ToString();
                        if (!array.ContainsKey(aname)) throw new Exception("模板错误 没有变量" + aname);
                        building = false;
                        varname = new StringBuilder();
                        if (c == '?')
                        {
                            iifresult = ((string)array[aname] == "Yes");
                            iifstage = 1;
                        }
                        else
                        {
                            ret.Append(array[aname]);
                            ret.Append(c);
                        }
                    }
                }
                else
                {
                    if (iifstage == 0)
                    {
                        if (c == '$') building = true;
                        else ret.Append(c);
                    }
                    else if (iifstage == 1)
                    {
                        if (c == ':') iifstage = 2;
                        else if (iifresult == true)
                        {
                            if (c == '$') building = true;
                            else ret.Append(c);
                        }
                    }
                    else if (iifstage == 2)
                    {
                        iifstage = 1;
                        if (c == ':') iifstage = 3;
                        else if (iifresult == true)
                        {
                            ret.Append(':');
                            if (c == '$') building = true;
                            else ret.Append(c);
                        }
                    }
                    else if (iifstage == 3)
                    {
                        if (c == ':') iifstage = 4;
                        else if (iifresult == false)
                        {
                            if (c == '$') building = true;
                            else ret.Append(c);
                        }
                    }
                    else if (iifstage == 4)
                    {
                        iifstage = 3;
                        if (c == ':') iifstage = 0;
                        else if (iifresult == false)
                        {
                            ret.Append(':');
                            if (c == '$') building = true;
                            else ret.Append(c);
                        }
                    }
                }
            }
            ret.Remove(ret.Length - 1, 1);
            return ret.ToString();
        }
    }

    public static class ControlExtensions
    {
        public static void DoubleBuffered(this Control control, bool enable)
        {
            var doubleBufferPropertyInfo = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            doubleBufferPropertyInfo.SetValue(control, enable, null);
        }
        public static void DoubleBuffering(this Control control, bool enable)
        {
            var method = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(control, new object[] { ControlStyles.OptimizedDoubleBuffer, enable });
        }
    }
    public class CookieAwareWebClient : WebClient
    {
        public CookieContainer m_container = new CookieContainer();

        public CookieAwareWebClient(CookieCollection cookies)
        {
            m_container.Add(cookies);
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            HttpWebRequest webRequest = request as HttpWebRequest;
            if (webRequest != null)
            {
                webRequest.CookieContainer = m_container;
            }
            return request;
        }
    }
}