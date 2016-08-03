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

namespace WebLearningOffline
{
    public partial class Form1 : Form
    {
        public CookieCollection cookies;
        Form2 form2 = null;
        public bool[] coursechecked = null;
        public bool[] itemchecked = null;
        public bool desktop = true;
        public string savepath = null;

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.ClientSize = new Size(textBox1.Left + textBox1.Width + label1.Left, button1.Top + button1.Height + textBox1.Top);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                textBox2.Focus();
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                button1_Click(null, null);
            }
        }

        public void button1_Click(object sender, EventArgs e)
        {
            button1.Text = "正在登录...";
            button1.Enabled = false;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            DoLogin();
        }

        private void DoLogin()
        {
            var userid = textBox1.Text;
            var userpass = textBox2.Text;
            cookies = new CookieCollection();
            try
            {
                var ret= Http.Get("http://learn.tsinghua.edu.cn/MultiLanguage/lesson/teacher/loginteacher.jsp?"
                    + "userid=" + Uri.EscapeDataString(userid) + "&userpass=" + Uri.EscapeDataString(userpass),
                    out cookies, cookiesin: cookies);
                if (ret.Contains("用户名或密码错误")) throw new Exception("用户名或密码错误");
                if (!ret.Contains("loginteacher_action")) throw new Exception("跳转到了未知的网页");
            }
            catch (Exception e)
            {
                MessageBox.Show("登录失败！原因：\r\n"+e.Message);
                button1.Text = "登录";
                button1.Enabled = true;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                return;
            }
            button1.Text = "登录";
            button1.Enabled = true;
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            form2 = new Form2(this);
            form2.Show();
            this.Hide();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
