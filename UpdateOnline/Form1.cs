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

namespace UpdateOnline
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            VerItems = GetDBVersion();
            if (VerItems.Count > 0)
                GetUpdateFile();
            else
            {
                label5.Text = "无可用更新";
                button1.Visible = true;
                button2.Visible = true;
            }
        }
        /// <summary>
        /// 目标启动程序名,无exe后缀
        /// </summary>
        string TargetExe = "TestDemo";

        List<DBVer> VerItems = new List<DBVer>();
        /// <summary>
        /// update路径
        /// </summary>
        private string D_Dir = Application.StartupPath + @"\update";
        /// <summary>
        /// 主程序路径
        /// </summary>
        private string L_Dir = Application.StartupPath;

        /// <summary>
        /// 获取本地版本号
        /// </summary>
        string GetLocalVersion()
        {
            string path = Application.StartupPath + "\\UpdateVer.ini";
            var version = OperateIniFile.ReadIniData("info", "Version", "", path);
            if (version == null || version == "")
                return "1.1.1.1";
            return version;
        }
        /// <summary>
        /// 获取服务器高版本号
        /// </summary>
        List<DBVer> GetDBVersion()
        {
            var HighVers = OracleHelper.GetVersion(GetLocalVersion());
            List<DBVer> verItems = new List<DBVer>();
            foreach (DataRow item in HighVers.Rows)
            {
                DBVer db = new DBVer();
                db.ID = int.Parse(item[0].ToString());
                db.Verison = item[1].ToString();
                //verItems.Add(int.Parse(item[0].ToString()));
                verItems.Add(db);
            }
            return verItems;
        }

        /// <summary>
        /// 判断是否需要升级
        /// </summary>
        /// <returns></returns>
        public  bool WhetherUpdate()
        {
            return GetDBVersion().Count > 0 ? true : false;
        }

        /// <summary>
        /// 逐条获取升级文件
        /// </summary>
        void GetUpdateFile()
        {
            label5.Text = "更新中。。。";
            progressBar1.Maximum = VerItems.Count;
            progressBar1.Value = 0;
            foreach (var item in VerItems)
            {
                progressBar1.Value++;
                label3.Text = $"({progressBar1.Value}/{ progressBar1.Maximum})";
                var files = OracleHelper.GetUpdateFile(item.ID);
                progressBar2.Maximum = files.Rows.Count;
                progressBar2.Value = 0;
                foreach (DataRow file in files.Rows)
                {
                    progressBar2.Value++;
                    label4.Text = $"({progressBar2.Value}/{ progressBar2.Maximum})";
                    string fileName = file["FILENAME"].ToString();
                    byte[] fileByte = (byte[])file["DOCUMENT"];
                    if (!Directory.Exists(D_Dir))
                    {
                        Directory.CreateDirectory(D_Dir);
                    }
                    using (FileStream fs = new FileStream($@"{D_Dir}\{fileName}", FileMode.OpenOrCreate))
                    {
                        fs.Write(fileByte, 0, fileByte.Length);
                    }

                }
            }
            ReplaceFile();
        }

        /// <summary>
        /// 替换文件
        /// </summary>
        private void ReplaceFile()
        {
            //替换前kill掉主程序
            Process[] processes = Process.GetProcessesByName(TargetExe);
            if (processes.Length > 1)
            {
                foreach (var item in processes)
                {
                    item.Kill();
                }
            }

            //替换主程序
            var filePaths = Directory.GetFiles(D_Dir);
            foreach (var filePath in filePaths)
            {
                FileInfo fi = new FileInfo(filePath);
                if (File.Exists($@"{L_Dir}\{fi.Name}"))
                {
                    File.Delete($@"{L_Dir}\{fi.Name}");
                }
                fi.MoveTo($@"{L_Dir}\{fi.Name}");
            }
            label5.Text = "更新完成";
            var ver = VerItems.OrderByDescending(x => x.ID).First().Verison;

            string path = Application.StartupPath + "\\UpdateVer.ini";
            if (!File.Exists(path))
            {
                File.Create(path);
            }
            OperateIniFile.WriteIniData("info", "Version", ver, Application.StartupPath + "\\UpdateVer.ini");
            button1.Visible = true;
            button2.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //重新打开主程序
                System.Diagnostics.Process ps = new System.Diagnostics.Process();
                var path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
                             + Path.DirectorySeparatorChar.ToString();
                ps.StartInfo.FileName = $@"{path}\{TargetExe}.exe";
                ps.Start();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
           
        }
    }

    public class DBVer
    {
        public int ID { get; set; }
        public string Verison { get; set; }
    }
}
