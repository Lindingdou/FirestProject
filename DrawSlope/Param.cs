using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DrawSlope
{
    public partial class Param : Form
    {
        public bool isCon = true;
        public double slope = 0.08;
        public double width = 20;
        public bool isDir = true;

        public Param()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            isDir = rb1.Checked;
            if (double.TryParse(tb1.Text, out slope))
            {
                slope = slope / 100;
            }
            else
            {
                slope = 0.08;
            }
            if (!double.TryParse(tb2.Text, out width))
            {
                width = 20;
                
            }
            
            //return;
            this.Close();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //取消
            isCon = false;
            //return;
            this.Close();
        }

        private void rb1_CheckedChanged(object sender, EventArgs e)
        {
            rb2.Checked = !rb1.Checked;
        }






    }
}
