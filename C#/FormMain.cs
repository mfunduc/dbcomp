using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dbComp
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }
        String path1, path2;
        DbInfo db1, db2;
        const int MATCHED_INDEX = 0;
        const int UNMATCHED_INDEX = 1;
        const int FILE1_INDEX = 2;
        const int FILE2_INDEX = 3;

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSelect frmSelect = new FormSelect();
            frmSelect.path1 = path1;
            frmSelect.path2 = path2;
            if (frmSelect.ShowDialog(this) == DialogResult.OK)
            {
                path1 = frmSelect.path1;
                path2 = frmSelect.path2;
                Rebuild();
            }
        }

        void Rebuild()
        {
            try
            {
                var watch = Stopwatch.StartNew();
                db1 = new DbInfo(path1);
                db2 = new DbInfo(path2);
                UpdateView();
                watch.Stop();
                toolStripStatusLabel1.Text = String.Format("Processing time: {0} seconds", watch.Elapsed.TotalSeconds);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                db1 = null;
                db2 = null;
            }
        }

        void UpdateView()
        {
            if ((db1 == null) || (db2 == null))
                return;
            List<TableInfo> tablesIn1 = new List<TableInfo>(), tablesIn2 = new List<TableInfo>();
            List<Tuple<TableInfo, TableInfo>> tablesMatched = new List<Tuple<TableInfo, TableInfo>>();
            List<Tuple<TableInfo, TableInfo, int>> tablesUnmatched = new List<Tuple<TableInfo, TableInfo, int>>();
            bool matched = db1.Compare(db2, ref tablesIn1, ref tablesIn2, ref tablesMatched, ref tablesUnmatched);

            listDB.BeginUpdate();
            listDB.Columns[0].Text = Path.GetFileNameWithoutExtension(path1) + " Tables";
            listDB.Columns[1].Text = Path.GetFileNameWithoutExtension(path2) + " Tables";
            listDB.Items.Clear();
            foreach (var table in tablesMatched)
            {
                ListViewItem lvi = listDB.Items.Add(table.Item1.Name);
                lvi.SubItems.Add(table.Item2.Name);
                lvi.ImageIndex = MATCHED_INDEX;
                lvi.SubItems.Add("Tables match: " + table.Item1.GetDescription());
                lvi.Tag = table;
            }
            foreach (var table in tablesUnmatched)
            {
                ListViewItem lvi = listDB.Items.Add(table.Item1.Name);
                lvi.SubItems.Add(table.Item2.Name);
                lvi.ImageIndex = UNMATCHED_INDEX;
                if (table.Item3 >= 0)
                    lvi.SubItems.Add(String.Format("Tables' data differs in row {0}", table.Item3 + 1));
                else
                    lvi.SubItems.Add(String.Format("Table 1 {0} and table 2 {1}", table.Item1.GetDescription(),
                        table.Item2.GetDescription()));
                lvi.Tag = table;
            }
            foreach (var table in tablesIn1)
            {
                ListViewItem lvi = listDB.Items.Add(table.Name);
                lvi.SubItems.Add("");
                lvi.ImageIndex = FILE1_INDEX;
                lvi.SubItems.Add("Table is only in the first database");
                lvi.Tag = table;
            }
            foreach (var table in tablesIn2)
            {
                ListViewItem lvi = listDB.Items.Add("");
                lvi.SubItems.Add(table.Name);
                lvi.ImageIndex = FILE2_INDEX;
                lvi.SubItems.Add("Table is only in the second database");
                lvi.Tag = table;
            }
            listDB.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listDB.EndUpdate();
            listTable.Items.Clear();
        }

        private void listDB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listDB.SelectedItems.Count > 0)
            {
                TableInfo table = null;
                switch (listDB.SelectedItems[0].ImageIndex)
                {
                    case MATCHED_INDEX:
                        table = (listDB.SelectedItems[0].Tag as Tuple<TableInfo, TableInfo>).Item1;
                    break;
                    case UNMATCHED_INDEX:
                        table = (listDB.SelectedItems[0].Tag as Tuple<TableInfo, TableInfo, int>).Item1;
                    break;
                    case FILE1_INDEX:
                    case FILE2_INDEX:
                        table = listDB.SelectedItems[0].Tag as TableInfo;
                    break;

                    default:
                    return;
                }

                UseWaitCursor = true;
                listTable.BeginUpdate();
                listTable.Items.Clear();

                foreach (var col in table.Columns)
                {
                    ListViewItem lvi = listTable.Items.Add(col.Item1);
                    lvi.SubItems.Add(col.Item2);
                }

                listTable.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listTable.EndUpdate();
                UseWaitCursor = false;
            }
        }

        private void copyMenuItem_Click(object sender, EventArgs e)
        {
            CopyListViewToClipboard(listDB);
        }

        public void CopyListViewToClipboard(ListView lv)
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("Result\t");
            foreach (ColumnHeader col in lv.Columns)
            {
                buffer.Append(col.Text);
                buffer.Append("\t");
            }
            buffer.Append("\r\n");

            foreach (ListViewItem lvi in lv.Items)
            {
                switch (lvi.ImageIndex)
                {
                    case MATCHED_INDEX:
                        buffer.Append("Matched\t");
                        break;
                    case UNMATCHED_INDEX:
                        buffer.Append("Not Matched\t");
                        break;
                    case FILE1_INDEX:
                        buffer.Append("First\t");
                        break;
                    case FILE2_INDEX:
                        buffer.Append("Second\t");
                        break;
                    default:
                        buffer.Append("\t");
                        break;
                }
                foreach (ListViewItem.ListViewSubItem lviSubitem in lvi.SubItems)
                {
                    buffer.Append(lviSubitem.Text);
                    buffer.Append("\t");
                }
                buffer.Append("\r\n");
            }

            Clipboard.SetText(buffer.ToString());
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);
        }
    }
}
