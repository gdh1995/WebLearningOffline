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

        public Form2(Form1 form1)
        {
            InitializeComponent();
            textBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            loginform = form1;
            cookies = form1.cookies;
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
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
                    matches = Regex.Matches(ret, @"<tr class=.info_tr2?.>(.+?)<\/tr>", RegexOptions.Singleline);
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
            for (int i = 0; i < checkedListBox2.Items.Count; i++)
                checkedListBox2.SetItemChecked(i, true);
            checkedListBox2.Enabled = true;
            button2.Enabled = true;
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                checkedListBox1.SetItemChecked(i, true);
            groupBox1.Text += "(" + courses.Count + ")";
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void checkedListBox2_ItemCheck(object sender, ItemCheckEventArgs e)
        {
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
            for (int i = 1; i < checkedListBox2.Items.Count; i++)
                courses[i - 1].selected = checkedListBox2.GetItemChecked(i);

        }
    }
}
