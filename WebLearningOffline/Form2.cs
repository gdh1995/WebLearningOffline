using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Web;

namespace WebLearningOffline
{
    public partial class Form2 : Form
    {
        public class Course
        {
            public bool selected;
            public bool isnew;
            public string id;
            public string name;
            public string term;
        }

        Form1 loginform;
        public CookieCollection cookies;

        public List<Course> courses;
        int nextjob = 0;
        int haserror = 0;
        int finished = 0;
        int totaltask = 0;
        object varlock = new object();

        public Form2(Form1 form1)
        {
            InitializeComponent();
            loginform = form1;
            cookies = form1.cookies;
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (button2.Text !="开始！")
            {
                e.NewValue = e.CurrentValue;
                Console.Beep();
                return;
            }
            CheckedListBox clb = (CheckedListBox)sender;
            // Switch off event handler
            clb.ItemCheck -= checkedListBox1_ItemCheck;
            clb.SetItemCheckState(e.Index, e.NewValue);

            // Now you can go further
            if (e.Index == 0)
            {
                for (int i = 1; i < clb.Items.Count; i++)
                    clb.SetItemCheckState(i, e.NewValue);
            }else
            {
                clb.SetItemChecked(0, true);
                for (int i = 1; i < clb.Items.Count; i++)
                    if (clb.GetItemChecked(i) == false)
                        clb.SetItemChecked(0, false);
            }
            // Switch on event handler
            clb.ItemCheck += checkedListBox1_ItemCheck;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                button1.Enabled = radioButton2.Checked = false;
                textBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                button1.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBox1.Text = folderBrowserDialog1.SelectedPath;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            textBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            checkedListBox2.DoubleBuffered(true);
            checkedListBox2.DoubleBuffering(true);
            new Thread(new ThreadStart(LoadCourses)).Start();
        }

        void LoadCourses()
        {
            var courseset = new HashSet<Course>();
            var typepageset = new HashSet<string>();
            try
            {
                var ret = Http.Get("http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/public_global.jsp",
                    out cookies, cookiesin: cookies);
                var matches = Regex.Matches(ret, @"MyCourse.jsp\?typepage=(\d+)");
                foreach (Match match in matches)
                {
                    typepageset.Add(match.Groups[1].Value);
                }
                foreach (var typepage in typepageset)
                {
                    ret=Http.Get("http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/MyCourse.jsp?typepage="+typepage,
                        out cookies, cookiesin: cookies);
                    matches = Regex.Matches(ret, @"<tr class=.info_tr\d?.>(.+?)<\/tr>", RegexOptions.Singleline);
                    foreach (Match match in matches)
                    {
                        var str = match.Groups[1].Value;
                        string id;
                        bool isnew;
                        if (str.Contains("course_id"))
                        {
                            id = Regex.Match(str, @"course_id=(\d+)").Groups[1].Value;
                            isnew = false;
                        }else
                        {
                            id = Regex.Match(str, @"coursehome\/([\w-]+)").Groups[1].Value;
                            isnew = true;
                        }
                        var name = Regex.Match(str, @"<a.+?>(.+?)<\/a>", RegexOptions.Singleline).Groups[1].Value;
                        name = name.Trim(new char[] { '\n', '\r', '\t', ' ' });
                        var term = name.Substring(name.LastIndexOf('(') + 1);
                        term = term.Substring(0, term.Length - 1);
                        name = name.Substring(0, name.LastIndexOf('('));
                        name = name.Substring(0, name.LastIndexOf('('));
                        var course = new Course() { isnew = isnew, id = id, name = name, term = term };
                        courseset.Add(course);
                    }
                }
            }catch(Exception e)
            {
                MessageBox.Show("获取课程列表失败！请重新登录。原因：\r\n" + e.Message);
                loginform.Show();
                this.Dispose();
                return;
            }
            courses = courseset.ToList();
            checkedListBox2.Items.Clear();
            checkedListBox2.Items.Add("全选");
            courses.ForEach(c => checkedListBox2.Items.Add(c.name + "(" + c.term + (c.isnew ? ")(新版)" : ")")));

            button2.Enabled = true;
            for (int i = 0; i < checkedListBox2.Items.Count; i++)
                checkedListBox2.SetItemChecked(i, true);
            checkedListBox2.Enabled = true;
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                checkedListBox1.SetItemChecked(i, true);
            groupBox1.Text += "(" + courses.Count + ")";
        }

        private void checkedListBox2_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (button2.Text != "开始！")
            {
                e.NewValue = e.CurrentValue;
                var freq = (int)(440 * Math.Pow(2.0, (checkedListBox2.Items.Count / 2 - e.Index) / 12.0));
                if (freq < 37) freq = 37;
                if (freq > 32767) freq = 32767;
                Console.Beep(freq,400);
                return;
            }
            CheckedListBox clb = (CheckedListBox)sender;
            // Switch off event handler
            clb.ItemCheck -= checkedListBox2_ItemCheck;
            clb.SetItemCheckState(e.Index, e.NewValue);

            // Now you can go further
            if (e.Index == 0)
            {
                for (int i = 1; i < clb.Items.Count; i++)
                    clb.SetItemCheckState(i, e.NewValue);
            }
            else
            {
                clb.SetItemChecked(0, true);
                for (int i = 1; i < clb.Items.Count; i++)
                    if (clb.GetItemChecked(i) == false)
                        clb.SetItemChecked(0, false);
            }
            // Switch on event handler
            clb.ItemCheck += checkedListBox2_ItemCheck;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "重新登录")
            {
                loginform.Show();
                this.Dispose();
                return;
            }
            button2.Text = "正在运行";
            checkedListBox2.BeginUpdate();
            for (int i = 1; i < checkedListBox2.Items.Count; i++)
            {
                courses[i - 1].selected = checkedListBox2.GetItemChecked(i);
                if (courses[i - 1].selected) totaltask++;
            }
            checkedListBox2.EndUpdate();
            button1.Enabled = button2.Enabled = false;
            radioButton1.Enabled = radioButton2.Enabled = false;
            new Thread(new ThreadStart(run)).Start();
        }

        void run()
        {
            SystemSleepManagement.PreventSleep(false);
            var threads = new Thread[5];
            lock (varlock)
            {
                for (int i = 0; i < threads.Length; i++)
                {
                    var tcookies = new CookieCollection();
                    tcookies.Add(cookies);
                    threads[i] = new Thread(new ParameterizedThreadStart(work));
                    threads[i].Start(tcookies);
                }
            }
            for(int i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }
            SystemSleepManagement.ResotreSleep();
            if (haserror > 0) MessageBox.Show("成功" + finished + "个，失败" + haserror + "个。详见课程列表。\r\n重新登录，可以再次下载出错的课程。");
            else MessageBox.Show(finished + "个全部成功！");
            button2.Text = "重新登录";
            button2.Enabled = true;
        }

        void work(object incookies)
        {
            lock (varlock) { }
            int i;
            var bp = textBox1.Text;
            var mycookies = (CookieCollection)incookies;
            if (!bp.EndsWith(Path.DirectorySeparatorChar + "")) bp += Path.DirectorySeparatorChar;
            while (true)
            {
                lock (varlock)
                {
                    i = nextjob;
                    if (i == courses.Count) return;
                    nextjob++;
                }
                if (!courses[i].selected) continue;
                try
                {
                    var course = courses[i];
                    var home = bp + safepath(course.term);
                    Directory.CreateDirectory(home);
                    home += Path.DirectorySeparatorChar + safepath(course.name);
                    Directory.CreateDirectory(home);
                    home += Path.DirectorySeparatorChar;

                    if (!course.isnew)
                    {
                        listitem(i, "正在下载");
                        var course_locate = Http.Get("http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/course_locate.jsp?course_id=" + course.id, out mycookies, cookiesin: mycookies);
                        if (course_locate.Contains("getnoteid_student")&&checkedListBox1.GetItemChecked(1)&&!File.Exists(home+"课程公告.html"))
                        {
                            var array = initdict(course);
                            var note = Http.Get("http://learn.tsinghua.edu.cn/MultiLanguage/public/bbs/getnoteid_student.jsp?course_id=" + course.id, out mycookies, cookiesin: mycookies);
                            var trs = Regex.Matches(note, @"<tr class=.tr\d?.+?>(.+?)<\/tr>", RegexOptions.Singleline);
                            var list = new List<Dictionary<string, object>>();
                            for (int j = 0; j < trs.Count; j++)
                            {
                                var tr = trs[j];
                                var tnote = new Dictionary<string, object>();
                                var tds = Regex.Matches(tr.Groups[1].Value, @"<td.*?>(.*?)<\/td>", RegexOptions.Singleline);
                                tnote.Add("NoteNumber", tds[0].Groups[1].Value);
                                tnote.Add("NoteCaption", Regex.Match(tds[1].Groups[1].Value, @"<a.*?>(.*?)<\/a>", RegexOptions.Singleline).Groups[1].Value);
                                tnote.Add("NoteAuthor", tds[2].Groups[1].Value);
                                tnote.Add("NoteDate", tds[3].Groups[1].Value);
                                var noteid = Regex.Match(tr.Groups[1].Value, @"href='(.+?)'").Groups[1].Value;
                                var notecontent = Http.Get("http://learn.tsinghua.edu.cn/MultiLanguage/public/bbs/" + noteid, out mycookies, cookiesin: mycookies);
                                var body = Regex.Matches(notecontent, @"<td.+?tr_l2.+?>(.*?)<\/td>", RegexOptions.Singleline)[1].Groups[1].Value;
                                tnote.Add("NoteBody", body);
                                list.Add(tnote);
                            }
                            array.Add("Notes", list);
                            writehtml("课程公告.html", home + "课程公告.html", array);
                        }
                        if (course_locate.Contains("course_info") && checkedListBox1.GetItemChecked(2) && !File.Exists(home + "课程信息.html"))
                        {
                            var array = initdict(course);
                            var info = Http.Get("http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/course_info.jsp?course_id=" + course.id, out mycookies, cookiesin: mycookies);
                            info = Regex.Match(info, @"(<table id.+?\/table>)", RegexOptions.Singleline).Groups[1].Value;
                            info = Regex.Replace(info, @"<img.+?>", "");
                            array.Add("InfoBody", info);
                            writehtml("课程信息.html", home + "课程信息.html", array);
                        }
                        if (course_locate.Contains("download") && checkedListBox1.GetItemChecked(3) && !File.Exists(home + "课程文件.html"))
                        {
                            var array = initdict(course);
                            var page= Http.Get("http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/download.jsp?course_id=" + course.id, out mycookies, cookiesin: mycookies);
                            var classes = Regex.Matches(page, @"<td class=.textTD.+?>(.*?)<\/td>",RegexOptions.Singleline);
                            var layers = Regex.Matches(page, @"<div class=.layerbox.+?>(.*?)<\/div>",RegexOptions.Singleline);
                            var list = new List<Dictionary<string, object>>();
                            Directory.CreateDirectory(home + "课程文件");
                            for (int j= 0;j < layers.Count;j++)
                            {
                                var classname = classes[j].Groups[1].Value;
                                var trs = Regex.Matches(layers[j].Groups[1].Value, @"<tr class=.tr\d?.>(.*?)<\/tr>", RegexOptions.Singleline);
                                foreach (Match tr in trs)
                                {
                                    var tfile = new Dictionary<string, object>();
                                    var tds = Regex.Matches(tr.Groups[1].Value, @"<td.*?>(.*?)<\/td>", RegexOptions.Singleline);
                                    tfile.Add("FileClass", classname);
                                    tfile.Add("FileNumber", tds[0].Groups[1].Value);
                                    var filetitle = Regex.Match(tds[2].Groups[1].Value, @"<a.+?>(.*?)<\/a>", RegexOptions.Singleline).Groups[1].Value;
                                    tfile.Add("FileTitle", filetitle);
                                    var filename = Regex.Match(tds[1].Groups[1].Value, @"getfilelink=(.+?)&").Groups[1].Value;
                                    tfile.Add("FileName", filename);
                                    tfile.Add("FileComment", tds[3].Groups[1].Value);
                                    tfile.Add("FileSize", tds[4].Groups[1].Value);
                                    tfile.Add("FileDate", tds[5].Groups[1].Value);
                                    var url = "http://learn.tsinghua.edu.cn" + Regex.Match(tds[2].Groups[1].Value, "href=\"(.*?)\"").Groups[1].Value;
                                    tfile.Add("FileUrl", url);
                                    var local = "课程文件" + Path.DirectorySeparatorChar + safepath(filename);
                                    tfile.Add("FileLocal", local);
                                    downfile(url, home+local, mycookies);
                                    list.Add(tfile);
                                }
                            }
                            array.Add("Files", list);
                            writehtml("课程文件.html", home + "课程文件.html", array);
                        }
                        if (course_locate.Contains("hom_wk_brw") && checkedListBox1.GetItemChecked(4) && !File.Exists(home + "课程作业.html"))
                        {
                            var array = initdict(course);
                            var page = Http.Get("http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/hom_wk_brw.jsp?course_id=" + course.id, out mycookies, cookiesin: mycookies);
                            var items = Regex.Matches(page, @"<tr class=.tr\d?.>(.+?)<\/tr>", RegexOptions.Singleline);
                            var list = new List<Dictionary<string, object>>();
                            Directory.CreateDirectory(home + "课程作业");
                            for(int j = 0; j < items.Count; j++)
                            {
                                var thwk = new Dictionary<string, object>();
                                var tds = Regex.Matches(items[j].Groups[1].Value, @"<td.*?>(.*?)<\/td>",RegexOptions.Singleline);
                                var name = Regex.Match(tds[0].Groups[1].Value, @"<a.*?>(.*?)<\/a>",RegexOptions.Singleline).Groups[1].Value;
                                thwk.Add("HomeworkName", name);
                                Directory.CreateDirectory(home + "课程作业" + Path.DirectorySeparatorChar + safepath(name));
                                thwk.Add("HomeworkStart", tds[1].Groups[1].Value);
                                thwk.Add("HomeworkEnd", tds[2].Groups[1].Value);
                                thwk.Add("HomeworkSubmitted", tds[3].Groups[1].Value.Contains("已经") ? "Yes": "No");
                                var scored = Regex.Match(tds[5].Groups[1].Value, "查看批阅\" (.)").Groups[1].Value != "d";
                                thwk.Add("HomeworkScored", scored ? "Yes" : "No");
                                var detailurl= "http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/" + Regex.Match(tds[0].Groups[1].Value, "href=\"(.*?)\"").Groups[1].Value;
                                var detailpage = Http.Get(detailurl, out mycookies, cookiesin: mycookies);
                                var content = Regex.Matches(detailpage, @"<textarea.*?>(.*?)<\/textarea>",RegexOptions.Singleline)[0].Groups[1].Value;
                                thwk.Add("HomeworkHandout", content);
                                thwk.Add("HomeworkHasHandout", content.Trim() != ""?"Yes":"No");
                                var handin = Regex.Matches(detailpage, @"<textarea.*?>(.*?)<\/textarea>", RegexOptions.Singleline)[1].Groups[1].Value;
                                thwk.Add("HomeworkHandin", handin);
                                thwk.Add("HomeworkHasHandin", handin.Trim() != "" ? "Yes" : "No");
                                var trs = Regex.Matches(detailpage, @"<tr>(.*?)<\/tr>",RegexOptions.Singleline);
                                var attn = "";
                                var attnname = "";
                                var upattn = "";
                                var upattnname = "";
                                foreach (Match tr in trs)
                                {
                                    var inner = tr.Groups[1].Value;
                                    if(inner.Contains("> 作业附件<"))
                                    {
                                        var tmatch = Regex.Match(inner, "href=\"(.*?)\"");
                                        if (tmatch.Success) attn = "http://learn.tsinghua.edu.cn" + tmatch.Groups[1].Value;
                                        tmatch = Regex.Match(inner, @"<a.*?>(.*?)<\/a>",RegexOptions.Singleline);
                                        if (tmatch.Success) attnname = tmatch.Groups[1].Value;
                                    }
                                    if (inner.Contains(">上交作业附件<"))
                                    {
                                        var tmatch = Regex.Match(inner, "href=\"(.*?)\"");
                                        if (tmatch.Success) upattn = "http://learn.tsinghua.edu.cn" + tmatch.Groups[1].Value;
                                        tmatch = Regex.Match(inner, @"<a.*?>(.*?)<\/a>",RegexOptions.Singleline);
                                        if (tmatch.Success) upattnname = tmatch.Groups[1].Value;
                                    }
                                }
                                thwk.Add("HomeworkHasAttnOut", attn != "" ? "Yes" : "No");
                                thwk.Add("HomeworkAttnOut", attn);
                                thwk.Add("HomeworkAttnOutName", attnname);
                                if (attn != "")
                                {
                                    var aolocal = home + "课程作业" + Path.DirectorySeparatorChar + safepath(name) + Path.DirectorySeparatorChar + safepath(attnname);
                                    thwk.Add("HomeworkAttnOutLocal", aolocal);
                                    downfile(attn, aolocal, mycookies);
                                }
                                else thwk.Add("HomeworkAttnOutLocal", "");
                                thwk.Add("HomeworkHasAttnIn", upattn != "" ? "Yes" : "No");
                                thwk.Add("HomeworkAttnIn", upattn);
                                thwk.Add("HomeworkAttnInName", upattnname);
                                if (upattn != "")
                                {
                                    var ailocal = home + "课程作业" + Path.DirectorySeparatorChar + safepath(name) + Path.DirectorySeparatorChar + safepath(upattnname);
                                    thwk.Add("HomeworkAttnInLocal", ailocal);
                                    downfile(upattn, ailocal, mycookies);
                                }
                                else thwk.Add("HomeworkAttnInLocal", "");
                                if (scored)
                                {
                                    var url = "http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/hom_wk_view.jsp?" + Regex.Match(items[j].Groups[1].Value, @"hom_wk_view.jsp\?(.+?)'").Groups[1].Value;
                                    var scorepage = Http.Get(url, out mycookies, cookiesin: mycookies);
                                    trs = Regex.Matches(scorepage, @"<tr.*?>(.*?)<\/tr>",RegexOptions.Singleline);
                                    string teacher = "", date = "", score = "", comment = "", file = "", filename="";
                                    foreach (Match tr in trs)
                                    {
                                        var inner = tr.Groups[1].Value;
                                        if (inner.Contains("批阅老师"))
                                        {
                                            var tmatch = Regex.Matches(inner, @"<td.*?>(.*?)<\/td>",RegexOptions.Singleline);
                                            teacher = tmatch[1].Groups[1].Value;
                                            date = tmatch[3].Groups[1].Value;
                                        }
                                        else if (inner.Contains("得分"))
                                        {
                                            var tmatch = Regex.Matches(inner, @"<td.*?>(.*?)<\/td>", RegexOptions.Singleline);
                                            score = tmatch[1].Groups[1].Value;
                                        }
                                        else if (inner.Contains(">评语<"))
                                        {
                                            var tmatch = Regex.Match(inner, @"<textarea.*?>(.*?)<\/textarea>", RegexOptions.Singleline);
                                            comment = tmatch.Groups[1].Value;
                                        }
                                        else if (inner.Contains("评语附件"))
                                        {
                                            var tmatch = Regex.Match(inner, "href=\"(.*?)\"");
                                            if (tmatch.Success) file = "http://learn.tsinghua.edu.cn" + tmatch.Groups[1].Value;
                                            tmatch = Regex.Match(inner, @"<a.*?>(.*?)<\/a>", RegexOptions.Singleline);
                                            if (tmatch.Success) filename = tmatch.Groups[1].Value;
                                        }
                                    }
                                    thwk.Add("HomeworkScorer", teacher);
                                    thwk.Add("HomeworkScoreDate", date);
                                    thwk.Add("HomeworkScore", score);
                                    thwk.Add("HomeworkScoreComment", comment);
                                    thwk.Add("HomeworkScoreHasAttn", file != "" ? "Yes": "No");
                                    thwk.Add("HomeworkScoreAttn", file);
                                    thwk.Add("HomeworkScoreAttnName", filename);
                                    if (file != "")
                                    {
                                        var aslocal = home + "课程作业" + Path.DirectorySeparatorChar + safepath(name) + Path.DirectorySeparatorChar + safepath(filename);
                                        thwk.Add("HomeworkScoreAttnLocal", aslocal);
                                        downfile(file, aslocal, mycookies);
                                    }
                                    else thwk.Add("HomeworkScoreAttnLocal", "");
                                }
                                list.Add(thwk);
                            }
                            array.Add("Homeworks", list);
                            writehtml("课程作业.html", home + "课程作业.html", array);
                        }
                    }
                    else
                    {

                    }
                    listitem(i, "成功");
                }
                catch (FormatException ex)
                {
                    listitem(i, "失败：" + ex.Message);
                    lock (varlock)
                    {
                        haserror++;
                    }
                    continue;
                }
                lock (varlock)
                {
                    finished++;
                    progressBar1.Value = (int)(100.0 / totaltask * finished);
                }
            }
        }

        static string safepath(string s)
        {
            s = HttpUtility.HtmlDecode(s);
            foreach (var c in Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()))
            {
                s = s.Replace(c, ' ');
            }
            return s;
        }

        void listitem(int i, string s)
        {
            var c = courses[i];
            lock (varlock)
            {
                checkedListBox2.Items[i + 1] = c.name + "(" + c.term + (c.isnew ? ")(新版)" : ")") + "  " + s;
            }
        }

        Dictionary<string,object> initdict(Course c)
        {
            var dict = new Dictionary<string, object>();
            dict.Add("CourseId", c.id);
            dict.Add("IsNew", c.isnew);
            dict.Add("CourseName", c.name);
            dict.Add("CourseTerm", c.term);
            return dict;
        }

        void writehtml(string infile, string outfile, Dictionary<string,object> array)
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
                    }else
                    {
                        match = Regex.Match(line, "<!-- foreach (\\w+) (.+?) -->");
                        if (match.Success)
                        {
                            var aname = match.Groups[1].Value;
                            var templ = match.Groups[2].Value;
                            if (!array.ContainsKey(aname)) throw new Exception("模板错误 没有变量" + aname);
                            if (array[aname].GetType()!=typeof(List<Dictionary<string,object>>)) throw new Exception("模板错误 不是数组" + aname);
                            var list = (List<Dictionary<string, object>>)array[aname];
                            list.ForEach(e => sw.WriteLine(replacevar(templ, e)));
                        }
                        else sw.WriteLine(line);
                    }
                }
            }
        }

        string replacevar(string str, Dictionary<string,object> array)
        {
            var ret = new StringBuilder();
            var varname = new StringBuilder();
            var building = false;
            var iifstage = 0;
            var iifresult = false;
            str += " ";
            foreach(var c in str)
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
                }else
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
                    else if(iifstage==2)
                    {
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

        void downfile(string url,string file,CookieCollection cookies)
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

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
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
