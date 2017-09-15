using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UploadFile
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        int id = 0;
        string verNumber;

        private void Form1_Load(object sender, EventArgs e)
        {
            var ver = OracleHelper.GetVersion();
            if (ver.Rows.Count == 0)
            {
                textBox1.Text = "1";
                textBox2.Text = "1";
                textBox3.Text = "1";
                textBox4.Text = "1";
                id = 1;
            }
            else
            {
                var versionTemp = ver.Rows[0]["VERSION"].ToString();
                var version = versionTemp.Split('.');
                textBox1.Text = version[0];
                textBox2.Text = version[1];
                textBox3.Text = version[2];
                textBox4.Text = (int.Parse(version[3]) + 1).ToString();

                if (int.Parse(version[3]) + 1 == 10)
                {
                    textBox4.Text = "1";
                    textBox3.Text = (int.Parse(version[2]) + 1).ToString();
                    if (int.Parse(version[2]) + 1 == 10)
                    {
                        textBox3.Text = "1";
                        textBox2.Text = (int.Parse(version[1]) + 1).ToString();
                        if (int.Parse(version[1]) + 1 == 10)
                        {
                            textBox2.Text = "1";
                            textBox1.Text = (int.Parse(version[0]) + 1).ToString();
                        }
                    }
                }
                id = int.Parse(ver.Rows[0]["id"].ToString()) + 1;            
            }

            verNumber = $"{textBox1.Text}.{textBox2.Text}.{textBox3.Text}.{textBox4.Text}";
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] files = e.Data.GetData(DataFormats.FileDrop, false) as string[];
                foreach (var item in files)
                {
                    FileInfo fi = new FileInfo(item);
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = fi.Name;
                    lvi.SubItems.Add(item);
                    listView1.Items.Add(lvi);
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, " Error ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            string description = string.Empty;
            if (textBox5.Text == null || textBox5.Text == "")
            {
                description = " ";
            }
            else
            {
                description = textBox5.Text;
            }
            if (OracleHelper.AddVersion(id, description, verNumber))
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    string path = item.SubItems[1].Text;

                    using (FileStream fs = new FileStream(@path, FileMode.Open, FileAccess.Read))
                    {
                        BinaryReader r = new BinaryReader(fs);
                        r.BaseStream.Seek(0, SeekOrigin.Begin);
                        var jg = r.ReadBytes((int)r.BaseStream.Length);
                        OracleHelper.AddUpdateFile(id, item.SubItems[0].Text, jg);
                    }
                }
                MessageBox.Show("OK");
                this.button2.Enabled = false;
            }
            else
            {
                MessageBox.Show("DB error");
            }
        }

        private void Form1_Leave(object sender, EventArgs e)
        {
            Console.WriteLine("Form1_Leave");
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            Console.WriteLine("Form1_Deactivate");
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            Console.WriteLine("Form1_Activated");
        }

        private void Form1_Enter(object sender, EventArgs e)
        {
            Console.WriteLine("Form1_Enter");
        }
    }
    
}
