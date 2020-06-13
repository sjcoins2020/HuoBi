using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Jli
{

    public partial class FMain : Form
    {
        System.Timers.Timer thq = new System.Timers.Timer(3000);

        private bool m_bSaveLayout = true;

        private DateTime dt;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;
        bool brun = false;
        FormHb frm2 = new FormHb();
        FormLeft frmL = new FormLeft();

        internal static bool bok = false;
        enum Data
        {
            C = 256, D = 288, E = 320, F = 341, G = 384, A = 426, B = 480, Bm = 453

        }
        public FMain()
        {
            InitializeComponent();
            SetSplashScreen();
            this.dt = DateTime.Now;
            SetState();

        }

        private void SetState()
        {
            //到达时间的时候执行注册事件
            thq.Elapsed += (obj, ee) =>
            {
                if (brun == false) return;
                RefreshState();

            };

            //设置是执行一次（false）还是一直执行(true)； 
            thq.AutoReset = true;

            //是否执行System.Timers.Timer.Elapsed注册事件；   
            thq.Enabled = true;
            ////Console.ReadKey();
            ///

        }

        private void RefreshState()
        {
            Task.Run(new Action(() =>
            {
                TimeSpan span = DateTime.Now - this.dt;

                if (span.TotalMinutes >= 3) //10分钟存一次
                {
                    this.BeginInvoke(
                        new Action(() =>
                        {
                            txtState.Text = $"服务器状态：异常";
                            txtJg.Text = $"异常时间:{span.TotalMinutes}分";
                            txtState.ForeColor = Color.Red;
                            Console.Beep((int)Data.C, 600);
                            Console.Beep((int)Data.D, 180);
                            Console.Beep((int)Data.E, 500);
                            Console.Beep((int)Data.C, 180);
                            Console.Beep((int)Data.E, 400);
                            Console.Beep((int)Data.C, 400);
                        })
                        );


                }
                else if (span.TotalSeconds >= 10)
                {
                    this.BeginInvoke(new Action(()=> {
                        bok = true;
                        txtState.Text = $"服务器状态：不佳";
                        txtJg.Text = $"已暂停:{span.TotalSeconds}秒";
                        txtState.ForeColor = Color.Green;
                        Console.Beep();
                    }));
                   
                }
                else
                {
                    this.BeginInvoke( new Action(()=> {
                        bok = true;
                        txtState.Text = $"服务器状态：正常";
                        txtState.ForeColor = Color.Blue;
                        txtJg.Text = $"刷新间隔:{span.TotalSeconds}秒";
                    }));
                   

                    //Console.Beep((int)Data.C, 600);
                    //Console.Beep((int)Data.D, 180);
                    //Console.Beep((int)Data.E, 500);
                    //Console.Beep((int)Data.C, 180);
                    //Console.Beep((int)Data.E, 400);
                    //Console.Beep((int)Data.C, 400);
                    //Console.Beep((int)Data.E, 800);
                    //Console.Beep((int)Data.D, 600);
                    //Console.Beep((int)Data.E, 180);
                    //Console.Beep((int)Data.F, 180);
                    //Console.Beep((int)Data.F, 180);
                    //Console.Beep((int)Data.E, 180);
                    //Console.Beep((int)Data.D, 180);
                    //Console.Beep((int)Data.F, 1600);
                    ////P1
                    //Console.Beep((int)Data.E, 600);
                    //Console.Beep((int)Data.F, 180);
                    //Console.Beep((int)Data.G, 580);
                    //Console.Beep((int)Data.E, 180);
                    //Console.Beep((int)Data.G, 400);
                    //Console.Beep((int)Data.E, 400);
                    //Console.Beep((int)Data.G, 800);
                    ////P2
                    //Console.Beep((int)Data.F, 600);
                    //Console.Beep((int)Data.G, 180);
                    //Console.Beep((int)Data.A, 180);
                    //Console.Beep((int)Data.A, 180);
                    //Console.Beep((int)Data.G, 180);
                    //Console.Beep((int)Data.F, 180);
                    //Console.Beep((int)Data.A, 1600);

                    ////
                    //Console.Beep((int)Data.G, 600);
                    //Console.Beep((int)Data.C, 180);
                    //Console.Beep((int)Data.D, 180);
                    //Console.Beep((int)Data.E, 180);
                    //Console.Beep((int)Data.F, 180);
                    //Console.Beep((int)Data.G, 180);
                    //Console.Beep((int)Data.A, 1600);
                    ////
                    //Console.Beep((int)Data.A, 600);
                    //Console.Beep((int)Data.D, 180);
                    //Console.Beep((int)Data.E, 180);
                    //Console.Beep((int)Data.F, 180);
                    //Console.Beep((int)Data.G, 180);
                    //Console.Beep((int)Data.A, 180);
                    //Console.Beep((int)Data.B, 1600);

                    ////
                    //Console.Beep((int)Data.B, 600);
                    //Console.Beep((int)Data.E, 180);
                    //Console.Beep((int)Data.F, 180);
                    //Console.Beep((int)Data.G, 180);
                    //Console.Beep((int)Data.A, 180);
                    //Console.Beep((int)Data.B, 180);
                    //Console.Beep((int)Data.C * 2, 1200);

                    //Console.Beep((int)Data.B, 180);
                    //Console.Beep((int)Data.Bm, 180);

                    //Console.Beep((int)Data.A, 350);
                    //Console.Beep((int)Data.F, 350);
                    //Console.Beep((int)Data.B, 350);
                    //Console.Beep((int)Data.G, 350);
                    //Console.Beep((int)Data.C * 2, 1000);

                }
            }));

        }

        private void FMain_Load(object sender, EventArgs e)
        {
            string configFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "DockPanel.config");

            if (File.Exists(configFile))
            {
                DeserializeDockContent ddContent = new DeserializeDockContent(GetContentFromPersistString);

                dockPanel1.LoadFromXml(configFile, ddContent);
            }
            else
            {
                Rectangle r = new Rectangle(100, 100, frmL.Width, frmL.Height);
                frmL.Show(dockPanel1, r);

                //frmL.Show(dockPanel1, DockState.DockLeft);
                frmL.DockState = DockState.DockLeft;
                frm2.Show(dockPanel1, DockState.Document);

                frm2.AMain = new Action<string>(SetMsg);
            }

        }
        public void ShowInDocker(object sender, EventArgs args)
        {
            (sender as FormMm).Show(dockPanel1, DockState.Document);
        }

        private IDockContent GetContentFromPersistString(string persistString)

        {

            try

            {

                if (persistString == typeof(FormHb).ToString())
                {
                    FormHb hb = new FormHb();
                    hb.AMain = new Action<string>(SetMsg);
                    hb.ShowInDock += ShowInDocker;

                    return hb;
                }

                if (persistString == typeof(FormLeft).ToString())
                {
                    FormLeft fl = new FormLeft();

                    return fl;

                }


                if (persistString == typeof(FormMm).ToString())
                {

                    return null;

                }
            }

            catch (Exception ex)

            {

                Console.WriteLine(persistString);

            }

            throw new Exception();

        }

        public void SetMsg(string s)
        {

            if (InvokeRequired)
            {
                MethodInvoker mi = new MethodInvoker(() =>
                {
                    brun = true;

                    this.txtMsg.Text = s;

                    this.dt = DateTime.Now;

                    RefreshState();
                });
                this.BeginInvoke(mi);


            }
            else
            {
                brun = true;

                this.txtMsg.Text = s;

                this.dt = DateTime.Now;

                RefreshState();
            }
        }

        private void FMain_MouseDown(object sender, MouseEventArgs e)
        {
            //ReleaseCapture();
            //SendMessage(this.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }

        private bool _showSplash;
        private SplashScreen _splashScreen;

        private void SetSplashScreen()
        {

            _showSplash = true;
            _splashScreen = new SplashScreen();

            ResizeSplash();
            _splashScreen.Visible = true;
            _splashScreen.TopMost = true;

            Timer _timer = new Timer();
            _timer.Tick += (sender, e) =>
            {
                _splashScreen.Visible = false;
                _timer.Enabled = false;
                _showSplash = false;
            };
            _timer.Interval = 2000;
            _timer.Enabled = true;
        }

        private void ResizeSplash()
        {
            if (_showSplash)
            {

                var centerXMain = (this.Location.X + this.Width) / 2.0;
                var LocationXSplash = Math.Max(0, centerXMain - (_splashScreen.Width / 2.0));

                var centerYMain = (this.Location.Y + this.Height) / 2.0;
                var LocationYSplash = Math.Max(0, centerYMain - (_splashScreen.Height / 2.0));

                _splashScreen.Location = new Point((int)Math.Round(LocationXSplash), (int)Math.Round(LocationYSplash));
            }
        }


        private void Bt_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Bt_min_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void FMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            string configFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "DockPanel.config");
            if (m_bSaveLayout)
                dockPanel1.SaveAsXml(configFile);
            else if (File.Exists(configFile))
                File.Delete(configFile);
        }

        private void 所有窗体ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (dockPanel1.Contents.Count == 0)
            {
                Rectangle r = new Rectangle(100, 100, frmL.Width, frmL.Height);
                frmL.Show(dockPanel1, r);

                //frmL.Show(dockPanel1, DockState.DockLeft);
                frmL.DockState = DockState.DockLeft;
                frm2.Show(dockPanel1, DockState.Document);

                frm2.AMain = new Action<string>(SetMsg);
            }
            //判断子窗体中是否已经存在在DockPanel中
            foreach (DockContent frm in dockPanel1.Contents)
            {
                if (frm is FormHb)
                {
                    frm.Activate();     //激活子窗体

                }
                else if (frm is FormLeft)
                {
                    frm.Activate();     //激活子窗体

                }
            }
        }
    }
}
