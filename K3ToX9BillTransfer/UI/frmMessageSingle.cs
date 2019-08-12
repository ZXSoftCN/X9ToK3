using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace K3ToX9BillTransfer.UI
{
    public partial class frmMessageSingle : Form
    {
        private frmMessageSingle()
        {
            InitializeComponent();
        }

        static frmMessageSingle()
        {
            if (instance == null)
            {
                lock (typeof(frmMessageSingle))
                {
                    if (instance == null)
                    {
                        instance = new frmMessageSingle();
                    }
                }
            }
        }

        private static frmMessageSingle instance = null;
        public static frmMessageSingle Instance { 
            get 
            {
                return instance;
            } 
        }

        public static void Show(string msg, string detailMsg)
        {
            instance.txtAllInfo.Text = detailMsg;
            instance.lbErrorInfo.Text = msg;
            Process[] procs = Process.GetProcessesByName("kdmain");
            if (procs.Length != 0)
            {
                IntPtr hwnd = procs[0].MainWindowHandle;
                //WindowWrapper类见下面
                instance.ShowDialog(new WindowWrapper(hwnd)); //指定记事本为父窗体
            }

            //instance.ShowDialog();//MessageBoxOptions.ServiceNotifcation
        }

        private void frmMessageSingle_Load(object sender, EventArgs e)
        {
            //splitContainer1.Panel2Collapsed = true;
        }

        private void btnShowAllInfo_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();
            this.txtAllInfo.Visible = !this.txtAllInfo.Visible;
            if (this.txtAllInfo.Visible)
            {
                this.txtAllInfo.Height = 100;
            }
            if (this.txtAllInfo.Visible)
            {
                //this.txtAllInfo.Text = "<DocInfos>" + Environment.NewLine +
                //"<DocHead id=\"1\" DocNo=\"0000000001\">" + Environment.NewLine +
                //"</DocHead>";
                this.Height = this.panel1.Height + 38 + this.txtAllInfo.Height;
            }
            else
            {
                this.Height = this.Height - this.txtAllInfo.Height;
            }

            this.ResumeLayout(false);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
