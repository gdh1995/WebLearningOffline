using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WebLearningOffline
{
    public partial class Form3 : Form
    {
        Form2 instance;

        public Form3(Form2 caller)
        {
            instance = caller;
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://student.tsinghua.edu.cn/");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://student.tsinghua.edu.cn/");
            linkLabel1.LinkVisited = true;
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("若在第一阶段取消，下次需要重新下载所有课程，确认吗？","取消下载",MessageBoxButtons.YesNo)==DialogResult.Yes)
                instance.canceltask();
        }
    }
}
