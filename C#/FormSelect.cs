using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace dbComp
{
    public partial class FormSelect : Form
    {
        public FormSelect()
        {
            InitializeComponent();
        }
        public String path1, path2;

        private void btnOK_Click(object sender, EventArgs e)
        {
            path1 = txtDb1.Text;
            path2 = txtDb2.Text;
            if (!File.Exists(path1))
            {
                MessageBox.Show(this, "Please enter a valid file name!");
                txtDb1.Focus();
                DialogResult = DialogResult.None;
            }
            else if (!File.Exists(path2))
            {
                MessageBox.Show(this, "Please enter a valid file name!");
                txtDb2.Focus();
                DialogResult = DialogResult.None;
            }
        }

        private void btnBrowse1_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = txtDb1.Text;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                txtDb1.Text = openFileDialog1.FileName;
        }

        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = txtDb2.Text;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                txtDb2.Text = openFileDialog1.FileName;
        }

        private void FormSelect_Load(object sender, EventArgs e)
        {
            txtDb1.Text = path1;
            txtDb2.Text = path2;
        }
    }
}
