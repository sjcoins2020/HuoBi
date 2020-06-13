using Huobi.Rest.CSharp.Demo;
using Huobi.Rest.CSharp.Demo.Model;
using Huobi.Rest.CSharp.Demo.Model.Extend;
using Jli.Common;
using Jli.Core;
using Jli.Model;
//using LevelDB;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using NPlotDemo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tools.Data;
using Websocket.Client;
using WebSocketClient;
using WeifenLuo.WinFormsUI.Docking;

namespace Jli
{
    //public delegate void InformHandle(object sender);
    public partial class FormHb : DockContent
    {
        public event EventHandler ShowInDock;

        public Action<string> AMain;

        static List<string> lexistcoin = new List<string>();//排除不需要的币种

        static List<string> lcoin = new List<string>();//需要的币种

        static NLog.Logger logger;

        internal static HuobiApi api = new HuobiApi("023de28f-f786fdad-d398e417-47055", "c1f69c7a-3423b2a1-4186f002-a015b");


        internal static string suserid = "";

        WSocketClient client = new WSocketClient("ws://127.0.0.1:7181/");


        static List<string> Lcbbz = new List<string>(); //持币币种

        public static CommonSymbols hcs = new CommonSymbols();

        public event EventHandler SendMsgEvent; //关联买卖界面事件处理

        private Hashtable hstable = new Hashtable();
        //创建一个全局集合，用来放置已经show出的窗体对象
        List<Form> lform = new List<Form>();

        private static readonly object SequenceLock = new object();

        string spath = System.Windows.Forms.Application.StartupPath;

        static string otcusdt = "";

        static object o = new object();

        static ObjectCache<string, DetailOtc> BufferOtc = new ObjectCache<string, DetailOtc>(); //缓存场外交易价格

        public static ObjectCache<string, long> BufferSid = new ObjectCache<string, long>(); //缓存用户id

        public static ObjectCache<string, double> BufferCjj = new ObjectCache<string, double>(); //缓存币种成交价;

        private static int origionWidth;

        static DataTable dtjy;

        #region "声明变量"

        /// <summary>
        /// 写入INI文件
        /// </summary>
        /// <param name="section">节点名称[如[TypeName]]</param>
        /// <param name="key">键</param>
        /// <param name="val">值</param>
        /// <param name="filepath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <param name="key">键</param>
        /// <param name="def">值</param>
        /// <param name="retval">stringbulider对象</param>
        /// <param name="size">字节大小</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);

        private string strFilePath = Application.StartupPath + "\\FileConfig.ini";//获取INI文件路径
        private string strSec = ""; //INI文件名


        /// <summary>
        /// 自定义读取INI文件中的内容方法
        /// </summary>
        /// <param name="Section">键</param>
        /// <param name="key">值</param>
        /// <returns></returns>
        private string ContentValue(string Section, string key)
        {

            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(Section, key, "", temp, 1024, strFilePath);
            return temp.ToString();
        }

        #endregion
        public FormHb()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BufferSid.LoseTime = 60 * 360; //360分钟
            BufferCjj.LoseTime = 60 * 120; //120分钟
            BufferCjj.ObjectCount = 1000;
            Init();
            ControlInit();//初始化日期控件为空值;
        }

        private void ControlInit()
        {
            InitDateTimePicker(this.dt_rq1);
            InitDateTimePicker(this.dt_rq2);
        }
        private void Init()
        {
            //SetStyle(ControlStyles.UserPaint, true);
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.  
            //SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲  


            ArrayList lists = new ArrayList();

            lists.Add(new
            {
                name = "火币正式api",
                value = "api.huobi.com"
            });
            lists.Add(new
            {
                name = "火币测试api",
                value = "api.huobi.br.com"
            });

            lists.Add(new
            {
                name = "火币测试1api",
                value = "api.hbdm.com"
            });


            this.cb_apihost.DisplayMember = "name";

            this.cb_apihost.ValueMember = "value";

            this.cb_apihost.DataSource = lists;

            LogInit();//初始化配置nlog
            setview();//初始化设置
            lexistcoin.Add("bt1");
            lexistcoin.Add("bt2");
            lexistcoin.Add("cdc");
            lexistcoin.Add("ven");
            //this.dgv_jy.DoubleBuffered(true);

            this.cb_apihost.SelectedIndexChanged += new System.EventHandler(this.Cb_apihost_SelectedIndexChanged);
        }

        #region  日期控件初始为空值处理

        /// <summary>
        /// 初始化日期时间控件
        /// </summary>
        /// <param name="dtp"></param>
        public static void InitDateTimePicker(DateTimePicker dtp)
        {
            dtp.Format = DateTimePickerFormat.Custom;
            dtp.CustomFormat = " ";  //必须设置成" "
            dtp.ValueChanged -= DateTimePicker_ValueChanged;
            dtp.ValueChanged += DateTimePicker_ValueChanged;
            dtp.KeyPress -= DateTimePicker_KeyPress;
            dtp.KeyPress += DateTimePicker_KeyPress;
        }

        public static void DateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            DateTimePicker dtp = (DateTimePicker)sender;
            dtp.Format = DateTimePickerFormat.Long;
            dtp.CustomFormat = null; //null;
            dtp.Checked = false;// 解决BUG ：防止日期控件不能选择相同日期的 --- 要放置在设置格式之后
        }

        public static void DateTimePicker_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)8)  // backspace左删除键
            {
                DateTimePicker dtp = (DateTimePicker)sender;
                dtp.Format = DateTimePickerFormat.Custom;
                dtp.CustomFormat = " ";
            }
        }
        #endregion


        private async void Button1_Click(object sender, EventArgs e)
        {

            if (SetView.blog)
            {
                MyConsole mc = new MyConsole();
                Console.Title = "[开发者模式]";//设置窗口标题
            }
            //Console.Title = "Jli行情";//设置窗口标题
            //Console.WindowWidth = 120;
            //Console.BufferHeight = 1000;


            api = new HuobiApi(SetView.accesskey, SetView.secretkey, SetView.hostapi);
            long sid; //用户帐号id;
            string sbz = "";



            if (await api.GetCurrencys() == "") //获取上线币种总量
            {
                MessageBox.Show($"{SetView.hostapi}获取数据失败!", "提醒");
                this.bt_login.Enabled = true;

                return;
            }
            else
            {
                this.bt_login.Enabled = false;

                this.bt_buybtc.Enabled = true;
                this.bt_sellbtc.Enabled = true;
                this.bt_buybtc.Visible = true;
                this.bt_sellbtc.Visible = true;
            }

            hcs = await api.GetCommonSymbols(); //获取交易精度
            if (hcs == null)
            {
                Logs("[error]查询Pro站支持的所有交易对及精度出错");
                return;
            }



            var tmpb = hcs.Data.Where(a => a.QuoteCurrency == "btc").ToList();
            Logs($"BTC可交易币种总数为：{tmpb.Count}个");


            string sBalance = "";
            double dBalance = 0d;

            #region 关注
            Thread tgz = new Thread(new ThreadStart(GuanZhu));
            tgz.IsBackground = true;
            tgz.Start();
            #endregion
            #region 行情
            //实例化Timer类，设置间隔时间为10000毫秒； 
            Thread thq = new Thread(new ThreadStart(HangQin));
            thq.IsBackground = true;
            thq.Start();
            #endregion

            #region 我的委托    未成交或未完全成交【委托买卖单】
            Thread twt = new Thread(new ThreadStart(WeiTuo));
            twt.IsBackground = true;
            twt.Start();
            #endregion

            #region 主程序
            //Thread tmain = new Thread(new ThreadStart(
            //   new Action(async () =>
            //   {
            //       List<Jy> ljyc = new List<Jy>();

            //       while (true)
            //       {

            //           MarketHistoryKline mk = new MarketHistoryKline();
            //           Application.DoEvents();
            //           Delay(300);
            //           sBalance = "";
            //           dBalance = 0d;
            //           System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            //           stopwatch.Start(); //  开始监视代码运行时间

            //           DetailOtc jg = null;
            //           if (BufferOtc["otc"] != null)
            //           {
            //               jg = BufferOtc["otc"] as DetailOtc;
            //               Logs($"[info]读缓存场外价格");
            //           }
            //           else
            //           {
            //               jg = await api.GetOtcRmb();
            //               BufferOtc["otc"] = jg;
            //               Logs($"[info]写缓存场外价格");
            //           }






            //           var result = await api.GetTradeDetail("btcusdt", jg);// 当前比特币成交价

            //           //mhk = result;
            //           //return result.DataMd[0].Open;

            //           //var tmprs = await api.GetMarketHistoryKline($"symbol=btcusdt&period=1day&size=1");

            //           var tmprs = 0d;
            //           string sparam = $"symbol=btcusdt&period=1day&size=1";
            //           tmprs = await KLine(sbz, jg.Buy, sparam);

            //           var rs = 0d;

            //           try
            //           {
            //               rs = tmprs * jg.Buy; //开盘价
            //           }
            //           catch
            //           {

            //           }




            //           if (rs == 0)
            //           {
            //               Logs($"[info]比特币开盘价获取超时");
            //               continue;
            //           }
            //           if (result == 0)
            //           {
            //               Logs($"[info]重新获取比特币成交价");
            //               continue;
            //           }

            //           #region

            //           var rsbz = 0d;
            //           try
            //           {
            //               rsbz = tmprs;
            //           }
            //           catch
            //           {

            //           }
            //           //#region 开盘价数据持久化
            //           //if (rsbz != 0)
            //           //{
            //           //    lock (SequenceLock)
            //           //    {
            //           //        string sql = $"select* from  KLine where symbol='btcusdt' and  rq='{DateTime.Now.ToShortDateString()}' order by ts desc limit 0,1";

            //           //        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

            //           //        SQLiteDataReader reader = SQLiteHelper.ExecuteReader(sconn, command) as SQLiteDataReader;

            //           //        var dt2 = DateTime.Now; ;
            //           //        if (reader.HasRows)
            //           //        {

            //           //            while (reader.Read())
            //           //            {
            //           //                dt2 = DateTime.Parse(reader["ts"].ToString());
            //           //            }

            //           //            var dt1 = DateTime.Now;

            //           //            TimeSpan span = dt1 - dt2;

            //           //            if (span.TotalMinutes > 20)
            //           //            {
            //           //                Logs(string.Format("[xxxxxxxxxxxxxxx]表kline,{0}分钟没有写进数据了，请检查！", (int)Math.Floor(span.TotalMinutes)));
            //           //            }

            //           //            if (span.TotalMinutes >= 10) //10分钟存一次
            //           //            {
            //           //                SaveKline("btcusdt", jg.Buy, mk, "", command);
            //           //            }
            //           //            else if (span.TotalSeconds >= 1)
            //           //            {

            //           //            }

            //           //            else
            //           //            {

            //           //            }

            //           //        }
            //           //        else
            //           //        {
            //           //            SaveKline("btcusdt", jg.Buy, mk, "", command);
            //           //        }
            //           //    }
            //           //}
            //           //#endregion
            //           #endregion

            //           otcusdt = jg.Buy.ToString();
            //           double dzdf = (result - rs) / rs * 100;
            //           Logs($"比特币涨跌幅：{dzdf.ToString("f2")}");

            //           if(this.InvokeRequired)
            //           {
            //               this.Invoke(new Action(()=> { this.lblBtmJg.Text = $"比特币价格：{result.ToString("f2")}元,{dzdf.ToString("f2")}%"; } ));
            //           }
            //           else
            //           {
            //               this.lblBtmJg.Text = $"比特币价格：{result.ToString("f2")}元,{dzdf.ToString("f2")}%";
            //           }

            //           sid = api.GetSpotAccountId();

            //           suserid = sid.ToString();//用户id;

            //           //#region 当前帐号未成交订单

            //           //string sparams = $"account-id={sid}&symbol=sspbtc&side=buy&size=500";
            //           //await api.GetOpenOrders(sparams);
            //           //#endregion
            //           IList<Huobi.Rest.CSharp.Demo.Model.List> lbalance = await api.GetAccountBalance(sid.ToString());

            //           while (lbalance == null)
            //           {
            //               Delay(1000);
            //               Logs("[info]重新用户获取持仓数据");
            //               spinLock.Enter();
            //               try
            //               {
            //                   lbalance = await api.GetAccountBalance(sid.ToString());
            //               }
            //               finally
            //               {
            //                   spinLock.Exit();
            //               }

            //           }
            //           var lbtc = lbalance.Where(a => a.Currency == "btc").ToList();

            //           if (lbtc.Count == 2)
            //           {
            //               foreach (var vbtc in lbtc)
            //               {
            //                   if (vbtc.Type == "trade")
            //                   {
            //                       sBalance = vbtc.Balance <= 0.000000001 ? "【资产】" : sBalance + $"【资产】比特币可用：{vbtc.Balance.ToString("f10")},对应人民币为：{(vbtc.Balance * result).ToString("f2")}元";
            //                       Logs(sBalance);
            //                       dBalance = dBalance + vbtc.Balance * result;
            //                   }
            //                   else if (vbtc.Type == "frozen")
            //                   {
            //                       Logs($"比特币冻结：{vbtc.Balance.ToString("f10")},对应人民币为：{(vbtc.Balance * result).ToString("f2")}元");
            //                       sBalance = vbtc.Balance <= 0.000000001 ? sBalance : sBalance + $" 比特币冻结：{vbtc.Balance.ToString("f10")},对应人民币为：{(vbtc.Balance * result).ToString("f2")}元";
            //                       dBalance = dBalance + vbtc.Balance * result;
            //                   }
            //                   else
            //                   {
            //                       throw new Exception("");
            //                   }


            //               }
            //           }

            //           var lusdt = lbalance.Where(a => a.Currency == "usdt").ToList();
            //           if (lusdt.Count == 2)
            //           {
            //               foreach (var vusdt in lusdt)
            //               {
            //                   if (vusdt.Type == "trade")
            //                   {

            //                       sBalance = vusdt.Balance <= 0.000000001 ? sBalance : sBalance + $" usdt可用：{vusdt.Balance.ToString("f10")},对应人民币为：{(vusdt.Balance * double.Parse(otcusdt)).ToString("f2")}元";
            //                       Logs(sBalance);
            //                       dBalance = dBalance + vusdt.Balance * double.Parse(otcusdt);
            //                   }
            //                   else if (vusdt.Type == "frozen")
            //                   {
            //                       Logs($"，usdt冻结：{vusdt.Balance.ToString("f10")},对应人民币为：{(vusdt.Balance * double.Parse(otcusdt)).ToString("f2")}元");
            //                       sBalance = vusdt.Balance <= 0.000000001 ? sBalance : sBalance + $" usdt冻结：{vusdt.Balance.ToString("f10")},对应人民币为：{(vusdt.Balance * double.Parse(otcusdt)).ToString("f2")}元";
            //                       dBalance = dBalance + vusdt.Balance * double.Parse(otcusdt);
            //                   }
            //                   else
            //                   {
            //                       throw new Exception("");
            //                   }


            //               }
            //           }

            //           int ic = tmpb.Count;
            //           int ii = 0;
            //           List<Jy> ljy = new List<Jy>();


            //           ljy.Clear();
            //           #region
            //           foreach (var tmpbz in tmpb)
            //           {

            //               sbz = tmpbz.BaseCurrency;

            //               if (lexistcoin.Contains(sbz)) continue;
            //               //await NewMethod(sbz, result, lbalance, ic, ii);
            //               //Logs($"[{ii+1}.{DateTime.Now}]准备获取{tmpbz.BaseCurrency}的数据");

            //               JgReturn jr;
            //               double d1 = 0d;
            //               if (SetView.vw_debugger)
            //               {
            //                   if (ii >= 20)
            //                   {
            //                       d1 = 0; //debugger 模式只返回三条数据
            //                   }
            //                   else
            //                   {
            //                       jr = await NewMethod(sbz, result, lbalance, ic, ii, ljy);
            //                       while (jr.Cjj == 0d)
            //                       {
            //                           Delay(500);
            //                           Logs("[error]重新获取成交价");
            //                           jr = await NewMethod(sbz, result, lbalance, ic, ii, ljy);
            //                       }

            //                       d1 = jr.Dtotal;
            //                   }
            //               }
            //               else
            //               {
            //                   jr = await NewMethod(sbz, result, lbalance, ic, ii, ljy);
            //                   while (jr.Cjj == 0d)
            //                   {
            //                       Delay(500);
            //                       Logs("[error]重新获取成交价");
            //                       jr = await NewMethod(sbz, result, lbalance, ic, ii, ljy);
            //                   }
            //                   d1 = jr.Dtotal;
            //               }
            //               //Logs($"[{ii + 1}.{DateTime.Now}]{tmpbz.BaseCurrency}的数据为{d1}");
            //               dBalance = dBalance + d1;

            //               ii++;

            //               Application.DoEvents();
            //               await Task.Run(() =>
            //               {
            //                   if (this.tt_msg.IsHandleCreated)
            //                   {
            //                       this.Invoke(new Action(() =>
            //                       {
            //                           //this.tt_msg.Text = $"更新：{ii}/{tmpb.Count},{(((double)ii / tmpb.Count) * 100).ToString("f2")}%";
            //                           AMain($"更新：{ii}/{tmpb.Count},{(((double)ii / tmpb.Count) * 100).ToString("f2")}%");
            //                       }));
            //                   }

            //               }
            //               );

            //           }

            //           ljyc.Clear();
            //           ljyc = ljy.OrderByDescending(i => i.Zdf).ToList<Jy>(); //.ThenByDescending(i => i.Zdf).ToList<Jy>();
            //                                                                  //foreach(var jy in ljy)
            //                                                                  //{
            //                                                                  //    ljyc.Add(jy);
            //                                                                  //}
            //           if (this.IsHandleCreated)
            //           {
            //               this.BeginInvoke(new Action(() =>
            //               {
            //                   try
            //                   {
            //                       this.dgv_jy.AutoGenerateColumns = false;
            //                       dtjy = ToDataTable<Jy>(ljyc);
            //                       if (SetView.autofy)
            //                       {
            //                           this.pagerControl1.PageSize = SetView.pagesize;
            //                           LoadData();
            //                       }
            //                       else
            //                       {
            //                           this.dgv_jy.DataSource = dtjy;// new BindingList<Jy>(ljyc);
            //                                                         //this.dgv_jy.Refresh();
            //                           this.dgv_jy.Restore();
            //                       }

            //                   }
            //                   catch
            //                   {

            //                   }
            //               }));
            //           }




            //           double d = dBalance;

            //           //new Thread(( d, s) =>
            //           //{

            //           //    Action action = (d,s) =>
            //           //    {
            //           //        this.txtBalance.Clear();

            //           //        txtBalance.Text = $"资产合计：{dBalance.ToString("f2")}元," + sBalance;
            //           //    };
            //           //    BeginInvoke(action);

            //           //}).Start();
            //           if (this.txtBalance.IsHandleCreated)
            //           {
            //               this.Invoke(new Action(() =>
            //               {

            //                   txtBalance.Text = $"资产合计：{dBalance.ToString("f2")}元," + sBalance;

            //               }));
            //           }

            //           #endregion
            //           stopwatch.Stop(); //  停止监视
            //           TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            //           double hours = timespan.TotalHours; // 总小时
            //           double minutes = timespan.TotalMinutes;  // 总分钟
            //           double seconds = timespan.TotalSeconds;  //  总秒数
            //           double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数

            //           Logs(result + "   " + $"{seconds}秒{milliseconds}毫秒\r\n");
            //           //Delay(1000);

            //       }
            //   }
            //   )));
            //tmain.IsBackground = true;
            //tmain.Start();
            #endregion
            await Task.Factory.StartNew(async () =>
            {
                List<Jy> ljyc = new List<Jy>();

                while (true)
                {

                    MarketHistoryKline mk = new MarketHistoryKline();
                    Application.DoEvents();
                    Delay(300);
                    sBalance = "";
                    dBalance = 0d;
                    System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start(); //  开始监视代码运行时间

                    DetailOtc jg = null;
                    if (BufferOtc["otc"] != null)
                    {
                        jg = BufferOtc["otc"] as DetailOtc;
                        Logs($"[info]读缓存场外价格");
                    }
                    else
                    {
                        jg = await api.GetOtcRmb();
                        BufferOtc["otc"] = jg;
                        Logs($"[info]写缓存场外价格");
                    }






                    var result = await api.GetTradeDetail("btcusdt", jg);// 当前比特币成交价

                    //mhk = result;
                    //return result.DataMd[0].Open;

                    //var tmprs = await api.GetMarketHistoryKline($"symbol=btcusdt&period=1day&size=1");

                    var tmprs = 0d;
                    string sparam = $"symbol=btcusdt&period=1day&size=1";
                    tmprs = await KLine(sbz, jg.Buy, sparam);

                    var rs = 0d;

                    try
                    {
                        rs = tmprs * jg.Buy; //开盘价
                    }
                    catch
                    {

                    }




                    if (rs == 0)
                    {
                        Logs($"[info]比特币开盘价获取超时");
                        continue;
                    }
                    if (result == 0)
                    {
                        Logs($"[info]重新获取比特币成交价");
                        continue;
                    }

                    #region

                    var rsbz = 0d;
                    try
                    {
                        rsbz = tmprs;
                    }
                    catch
                    {

                    }
                    //#region 开盘价数据持久化
                    //if (rsbz != 0)
                    //{
                    //    lock (SequenceLock)
                    //    {
                    //        string sql = $"select* from  KLine where symbol='btcusdt' and  rq='{DateTime.Now.ToShortDateString()}' order by ts desc limit 0,1";

                    //        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

                    //        SQLiteDataReader reader = SQLiteHelper.ExecuteReader(sconn, command) as SQLiteDataReader;

                    //        var dt2 = DateTime.Now; ;
                    //        if (reader.HasRows)
                    //        {

                    //            while (reader.Read())
                    //            {
                    //                dt2 = DateTime.Parse(reader["ts"].ToString());
                    //            }

                    //            var dt1 = DateTime.Now;

                    //            TimeSpan span = dt1 - dt2;

                    //            if (span.TotalMinutes > 20)
                    //            {
                    //                Logs(string.Format("[xxxxxxxxxxxxxxx]表kline,{0}分钟没有写进数据了，请检查！", (int)Math.Floor(span.TotalMinutes)));
                    //            }

                    //            if (span.TotalMinutes >= 10) //10分钟存一次
                    //            {
                    //                SaveKline("btcusdt", jg.Buy, mk, "", command);
                    //            }
                    //            else if (span.TotalSeconds >= 1)
                    //            {

                    //            }

                    //            else
                    //            {

                    //            }

                    //        }
                    //        else
                    //        {
                    //            SaveKline("btcusdt", jg.Buy, mk, "", command);
                    //        }
                    //    }
                    //}
                    //#endregion
                    #endregion

                    otcusdt = jg.Buy.ToString();
                    double dzdf = (result - rs) / rs * 100;
                    Logs($"比特币涨跌幅：{dzdf.ToString("f2")}");

                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() => { this.lblBtmJg.Text = $"比特币价格：{result.ToString("f2")}元,{dzdf.ToString("f2")}%"; }));
                    }
                    else
                    {
                        this.lblBtmJg.Text = $"比特币价格：{result.ToString("f2")}元,{dzdf.ToString("f2")}%";
                    }

                    sid = api.GetSpotAccountId();

                    suserid = sid.ToString();//用户id;

                    //#region 当前帐号未成交订单

                    //string sparams = $"account-id={sid}&symbol=sspbtc&side=buy&size=500";
                    //await api.GetOpenOrders(sparams);
                    //#endregion
                    IList<Huobi.Rest.CSharp.Demo.Model.List> lbalance = await api.GetAccountBalance(sid.ToString());

                    while (lbalance == null)
                    {
                        Delay(1000);
                        Logs("[info]重新用户获取持仓数据");
                        spinLock.Enter();
                        try
                        {
                            lbalance = await api.GetAccountBalance(sid.ToString());
                        }
                        finally
                        {
                            spinLock.Exit();
                        }

                    }
                    var lbtc = lbalance.Where(a => a.Currency == "btc").ToList();

                    if (lbtc.Count == 2)
                    {
                        foreach (var vbtc in lbtc)
                        {
                            if (vbtc.Type == "trade")
                            {
                                sBalance = vbtc.Balance <= 0.000000001 ? "【资产】" : sBalance + $"【资产】比特币可用：{vbtc.Balance.ToString("f10")},对应人民币为：{(vbtc.Balance * result).ToString("f2")}元";
                                Logs(sBalance);
                                dBalance = dBalance + vbtc.Balance * result;
                            }
                            else if (vbtc.Type == "frozen")
                            {
                                Logs($"比特币冻结：{vbtc.Balance.ToString("f10")},对应人民币为：{(vbtc.Balance * result).ToString("f2")}元");
                                sBalance = vbtc.Balance <= 0.000000001 ? sBalance : sBalance + $" 比特币冻结：{vbtc.Balance.ToString("f10")},对应人民币为：{(vbtc.Balance * result).ToString("f2")}元";
                                dBalance = dBalance + vbtc.Balance * result;
                            }
                            else
                            {
                                throw new Exception("");
                            }


                        }
                    }

                    var lusdt = lbalance.Where(a => a.Currency == "usdt").ToList();
                    if (lusdt.Count == 2)
                    {
                        foreach (var vusdt in lusdt)
                        {
                            if (vusdt.Type == "trade")
                            {

                                sBalance = vusdt.Balance <= 0.000000001 ? sBalance : sBalance + $" usdt可用：{vusdt.Balance.ToString("f10")},对应人民币为：{(vusdt.Balance * double.Parse(otcusdt)).ToString("f2")}元";
                                Logs(sBalance);
                                dBalance = dBalance + vusdt.Balance * double.Parse(otcusdt);
                            }
                            else if (vusdt.Type == "frozen")
                            {
                                Logs($"，usdt冻结：{vusdt.Balance.ToString("f10")},对应人民币为：{(vusdt.Balance * double.Parse(otcusdt)).ToString("f2")}元");
                                sBalance = vusdt.Balance <= 0.000000001 ? sBalance : sBalance + $" usdt冻结：{vusdt.Balance.ToString("f10")},对应人民币为：{(vusdt.Balance * double.Parse(otcusdt)).ToString("f2")}元";
                                dBalance = dBalance + vusdt.Balance * double.Parse(otcusdt);
                            }
                            else
                            {
                                throw new Exception("");
                            }


                        }
                    }

                    int ic = tmpb.Count;
                    int ii = 0;
                    List<Jy> ljy = new List<Jy>();


                    ljy.Clear();
                    #region
                    foreach (var tmpbz in tmpb)
                    {

                        sbz = tmpbz.BaseCurrency;

                        if (lexistcoin.Contains(sbz)) continue;
                        if (!lcoin.Contains(sbz)) lcoin.Add(sbz);
                        //await NewMethod(sbz, result, lbalance, ic, ii);
                        //Logs($"[{ii+1}.{DateTime.Now}]准备获取{tmpbz.BaseCurrency}的数据");

                        JgReturn jr;
                        double d1 = 0d;
                        if (SetView.vw_debugger)
                        {
                            if (ii >= 20)
                            {
                                d1 = 0; //debugger 模式只返回三条数据
                            }
                            else
                            {
                                jr = await NewMethod(sbz, result, lbalance, ic, ii, ljy);
                                while (jr.Cjj == 0d)
                                {
                                    Delay(500);
                                    Logs("[error]重新获取成交价");
                                    jr = await NewMethod(sbz, result, lbalance, ic, ii, ljy);
                                }

                                d1 = jr.Dtotal;
                            }
                        }
                        else
                        {
                            jr = await NewMethod(sbz, result, lbalance, ic, ii, ljy);
                            while (jr.Cjj == 0d)
                            {
                                Delay(500);
                                Logs("[error]重新获取成交价");
                                jr = await NewMethod(sbz, result, lbalance, ic, ii, ljy);
                            }
                            d1 = jr.Dtotal;
                        }
                        //Logs($"[{ii + 1}.{DateTime.Now}]{tmpbz.BaseCurrency}的数据为{d1}");
                        dBalance = dBalance + d1;

                        ii++;

                        Application.DoEvents();
                        await Task.Run(() =>
                        {
                            if (this.tt_msg.IsHandleCreated)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    //this.tt_msg.Text = $"更新：{ii}/{tmpb.Count},{(((double)ii / tmpb.Count) * 100).ToString("f2")}%";
                                    AMain($"更新：{ii}/{tmpb.Count},{(((double)ii / tmpb.Count) * 100).ToString("f2")}%");
                                }));
                            }

                        }
                        );

                    }

                    ljyc.Clear();
                    ljyc = ljy.OrderByDescending(i => i.Zdf).ToList<Jy>(); //.ThenByDescending(i => i.Zdf).ToList<Jy>();
                                                                           //foreach(var jy in ljy)
                                                                           //{
                                                                           //    ljyc.Add(jy);
                                                                           //}
                    if (this.IsHandleCreated)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                this.dgv_jy.AutoGenerateColumns = false;
                                dtjy = ToDataTable<Jy>(ljyc);
                                if (SetView.autofy)
                                {
                                    this.pagerControl1.PageSize = SetView.pagesize;

                                    #region 缓存数据，加快显示速度
                                    Task t= Task.Run(new Action(()=> {
                                        foreach (string tmpbz in lcoin)
                                        {
                                            double drmb = 0d;


                                            string skey = tmpbz + "昨天";
                                            if (BufferCjj[skey] == null)
                                            {
                                                drmb = TodayJg(tmpbz);

                                                BufferCjj[skey] = drmb;

                                            }
                                            drmb = 0d;
                                            skey = tmpbz + "前天";
                                            if (BufferCjj[skey] == null)
                                            {
                                                drmb = QtJg(tmpbz);

                                                BufferCjj[skey] = drmb;
                                            }
                                        }
                                    }));                                    
                                    Task.WaitAll(t);
                                    #endregion
                                    LoadData();
                                }
                                else
                                {
                                    this.dgv_jy.DataSource = dtjy;// new BindingList<Jy>(ljyc);
                                                                  //this.dgv_jy.Refresh();
                                    this.dgv_jy.Restore();
                                }

                            }
                            catch
                            {

                            }
                        }));
                    }




                    double d = dBalance;

                    //new Thread(( d, s) =>
                    //{

                    //    Action action = (d,s) =>
                    //    {
                    //        this.txtBalance.Clear();

                    //        txtBalance.Text = $"资产合计：{dBalance.ToString("f2")}元," + sBalance;
                    //    };
                    //    BeginInvoke(action);

                    //}).Start();
                    if (this.txtBalance.IsHandleCreated)
                    {
                        this.Invoke(new Action(() =>
                        {

                            txtBalance.Text = $"资产合计：{dBalance.ToString("f2")}元," + sBalance;

                        }));
                    }

                    #endregion
                    stopwatch.Stop(); //  停止监视
                    TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
                    double hours = timespan.TotalHours; // 总小时
                    double minutes = timespan.TotalMinutes;  // 总分钟
                    double seconds = timespan.TotalSeconds;  //  总秒数
                    double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数

                    Logs(result + "   " + $"{seconds}秒{milliseconds}毫秒\r\n");
                    //Delay(1000);

                }

            }, TaskCreationOptions.LongRunning);

        }

        private void WeiTuo()
        {
            //实例化Timer类，设置间隔时间为10000毫秒； 
            System.Timers.Timer t = new System.Timers.Timer(10000);

            //到达时间的时候执行注册事件
            t.Elapsed += async (obj, ee) =>
            {


                Application.DoEvents();
                new Thread(() =>
                {
                    if (this.lblOpenOrders.IsHandleCreated)
                    {
                        Action action = () =>
                        {
                            this.lblOpenOrders.Text = "";
                        };
                        BeginInvoke(action);
                    }

                }).Start();


                string sparams = $"&account-id=&symbol=&side=sell&size=500";
                OpenOrders s = await OpenOrders(sparams, 46471);
                if (s.Status == "ok")
                {
                    if (s.Data.Count == 0)
                    {

                        Logs("无未成交的委托卖单！");

                        new Thread(() =>
                        {
                            if (this.lblOpenOrders.IsHandleCreated)
                            {
                                Action action = () =>
                                {
                                    this.lblOpenOrders.Text = this.lblOpenOrders.Text + "无未成交的委托卖单！";

                                    dgv_sell.DataSource = null;
                                    try
                                    {
                                        if (dgv_sell.Columns.Count != 0)
                                        {
                                            DataGridViewColumn column = dgv_sell.Columns[0];

                                            if (column is DataGridViewLinkColumn)
                                            {
                                                dgv_sell.Columns.Remove(column);
                                            }
                                        }

                                    }
                                    catch
                                    {

                                    }


                                };
                                BeginInvoke(action);
                            }

                        }).Start();


                        //var accounts = api.GetAllAccount();
                        //var spotAccountId = accounts.FirstOrDefault(a => a.Type == "spot" && a.State == "working")?.Id;
                        //if (spotAccountId <= 0)
                        //    throw new ArgumentException("spot account unavailable");
                        //OrderPlaceRequest req = new OrderPlaceRequest();
                        //req.account_id = spotAccountId.ToString();
                        //req.amount = "1";
                        //req.price = "1";
                        //req.source = "api";
                        //req.symbol = "sspbtc";
                        //req.type = "sell-limit";
                        //var result = api.OrderPlace(req);

                        //Action asell = () =>
                        //{
                        //    dgv_sell.DataSource = null;
                        //};
                        //if (this.dgv_sell.IsHandleCreated)
                        //{
                        //    BeginInvoke(asell);
                        //}


                    }
                    else
                    {

                        Action asell = () =>
                        {
                            dgv_sell.DataSource = s.Data;

                            DataGridViewColumn column = dgv_sell.Columns[0];

                            if (column is DataGridViewLinkColumn)
                            {

                            }
                            else
                            {
                                DataGridViewLinkColumn dbt = new DataGridViewLinkColumn();
                                dbt.Text = "取消";

                                dbt.UseColumnTextForLinkValue = true;
                                dgv_sell.Columns.Insert(0, dbt);
                                dgv_sell.Columns[0].Name = "取消";
                            }
                            dgv_sell.Restore();

                        };
                        if (this.dgv_sell.IsHandleCreated)
                        {
                            BeginInvoke(asell);
                        }


                    }
                }
                sparams = $"&account-id=&symbol=&side=buy&size=500";
                s = await OpenOrders(sparams, 46471, "");

                if (s.Status == "ok")
                {
                    if (s.Data.Count == 0)
                    {

                        Logs("无未成交的委托买单！");

                        new Thread(() =>
                        {

                            Action action = () =>
                            {
                                this.lblOpenOrders.Text = this.lblOpenOrders.Text + "无未成交的委托买单！";
                                dgv_buy.DataSource = null;
                                try
                                {
                                    if (dgv_buy.Columns.Count != 0)
                                    {
                                        DataGridViewColumn column = dgv_buy.Columns[0];

                                        if (column is DataGridViewLinkColumn)
                                        {
                                            dgv_buy.Columns.Remove(column);
                                        }
                                    }

                                }
                                catch
                                {

                                }

                            };
                            if (this.lblOpenOrders.IsHandleCreated)
                            {
                                BeginInvoke(action);
                            }

                        }).Start();

                        Action abuy = () =>
                        {
                            dgv_buy.DataSource = null;
                        };
                        if (this.dgv_buy.IsHandleCreated)
                        {
                            BeginInvoke(abuy);
                        }
                    }
                    else
                    {
                        foreach (var tmp in s.Data)
                        {
                            Logs($"[未成交委托买单] 币种:{tmp.Symbol}，下单时间：{tmp.CreatedAt},订单状态:{tmp.State},下单数量：{tmp.Amount}");

                        }

                        Action abuy = () =>
                        {
                            dgv_buy.DataSource = s.Data;

                            DataGridViewColumn column = dgv_buy.Columns[0];

                            if (column is DataGridViewLinkColumn)
                            {

                            }
                            else
                            {
                                DataGridViewLinkColumn dbt = new DataGridViewLinkColumn();
                                dbt.Text = "取消";

                                dbt.UseColumnTextForLinkValue = true;
                                dgv_buy.Columns.Insert(0, dbt);
                                dgv_buy.Columns[0].Name = "取消";
                            }
                            dgv_buy.Restore();
                        };
                        if (this.dgv_buy.IsHandleCreated)
                        {
                            BeginInvoke(abuy);
                        }


                    }
                }
            };

            //设置是执行一次（false）还是一直执行(true)； 
            t.AutoReset = true;

            //是否执行System.Timers.Timer.Elapsed注册事件；   
            t.Enabled = true;
            //Console.ReadKey();
        }

        private void HangQin()
        {
            System.Timers.Timer thq = new System.Timers.Timer(60000);

            //到达时间的时候执行注册事件
            thq.Elapsed += async (obj, ee) =>
            {

                Application.DoEvents();
                string sparams = "https://marketapi.blockmeta.com/ticker?need_kline=huobipro-btc_usd&format_type=data&symbols=huobipro-btc_usd,okex-btc_usd,bitfinex-btc_usd,bitstamp-btc_usd,binance-btc_usd,gate-btc_usd";
                var result = await api.GetHangQin(sparams);

                if (result != null)
                {
                    new Thread(() =>
                    {

                        Action action = () =>
                        {
                            List<TickerDgv> lgv = new List<TickerDgv>();
                            if (result.Tickers != null)
                            {
                                if (result.Tickers.Count >= 1)
                                {
                                    foreach (var hq in result.Tickers)
                                    {
                                        TickerDgv tg = new TickerDgv();
                                        tg.Exchange = hq.Exchange;
                                        //tg.Pair = hq.Pair;
                                        //tg.ConvertCny =  * hq.Pair;
                                        tg.Last = Math.Round(hq.Tk.Last * hq.ConvertCny, 0);
                                        lgv.Add(tg);
                                    }
                                }
                                this.dgv_btcHangQin.AutoGenerateColumns = true;
                                this.dgv_btcHangQin.DataSource = new BindingList<TickerDgv>(lgv);
                                this.dgv_btcHangQin.Columns[0].HeaderCell.Value = "交易所";
                                this.dgv_btcHangQin.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                                this.dgv_btcHangQin.Columns[1].HeaderCell.Value = "价格（人民币）";
                                this.dgv_btcHangQin.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                                this.dgv_btcHangQin.Refresh();
                            }
                        };
                        Task.Run(() =>
                        {
                            if (this.dgv_btcHangQin.IsHandleCreated)
                            {
                                BeginInvoke(action);
                            }
                        });
                    }, 1024).Start();
                }



            };

            //设置是执行一次（false）还是一直执行(true)； 
            thq.AutoReset = true;

            //是否执行System.Timers.Timer.Elapsed注册事件；   
            thq.Enabled = true;
            //Console.ReadKey();
        }

        private void GuanZhu()
        {
            //实例化Timer类，设置间隔时间为10000毫秒； 
            System.Timers.Timer tgz = new System.Timers.Timer(30000);

            //到达时间的时候执行注册事件
            tgz.Elapsed += (obj, ee) =>
            {
                Application.DoEvents();
                try
                {
                    //await  GzRefresh();
                    Task.Run(async () =>
                    {
                        if (!SetView.guanzhu)
                            await GzRefresh();
                    });


                }
                catch
                {

                }
            };
            tgz.AutoReset = true;

            //是否执行System.Timers.Timer.Elapsed注册事件；   
            tgz.Enabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sbz">币种</param>
        /// <param name="result">比特币成交价</param>
        /// <param name="lbalance"></param>
        /// <param name="ic"></param>
        /// <param name="ii"></param>
        /// <returns></returns>
        static Core.SpinLock spinLock = new Core.SpinLock(1);
        static Core.SpinLock spinLockAi = new Core.SpinLock(1); //策略选
        private async Task<JgReturn> NewMethod(string sbz, double result, IList<List> lbalance, int ic, int ii, List<Jy> ls)
        {
            JgReturn jr = new JgReturn();
            jr.Cjj = 0d;
            jr.Dtotal = 0d;
            //Monitor.Enter(o);
            Application.DoEvents();
            //spinLock.Enter();
            double dtotal = 0d;
            try
            {

                #region 资产币种成交价
                var rbz = await api.GetTradeDetail($"{sbz}btc");

                double dba = lbalance.Where(a => a.Currency == $"{sbz}").Sum(a => a.Balance);//分为可用数量和锁定数量，故sum;

                var lbtc = lbalance.Where(a => a.Currency == $"{sbz}").ToList();


                foreach (var vbtc in lbtc)
                {
                    if (vbtc.Balance * rbz * result >= 5) //只对当前资产大于5元的币种进行统计
                    {
                        if (vbtc.Type == "trade")
                        {
                            dtotal = dtotal + vbtc.Balance * rbz * result;
                            Logs($"【资产】{sbz}可用：{vbtc.Balance.ToString("f10")},对应人民币为：{vbtc.Balance * rbz * result}元");
                        }
                        else if (vbtc.Type == "frozen")
                        {

                            Logs($"【资产】{sbz}冻结：{vbtc.Balance.ToString("f10")},对应人民币为：{vbtc.Balance * rbz * result}元");
                        }




                        if (!Lcbbz.Contains(sbz))
                        {
                            lock (SequenceLock)
                            {
                                Lcbbz.Add(sbz);
                            }

                        }

                    }
                }


                #region 获取开盘价
                var rsbz = 0d;
                string sparam = $"symbol={sbz}btc&period=1day&size=1";
                rsbz = await KLine(sbz, result, sparam);
                #endregion
                if (rsbz != 0)
                {
                    double dzdfbz = (rbz - rsbz) / rsbz * 100;
                    if (dzdfbz >= 5 || rbz * result * dba >= 50)//币种涨跌幅>=5 或者 币总>=50元
                    {
                        Logs($"{ic - ii}币种：{sbz},余额：{dba}");
                        Logs($"币种：{sbz},最新成交价btc：{rbz.ToString("f10")},对应Rmb:{(rbz * result).ToString("f2")},余额：{dba}，合计Rmb：{(rbz * result * dba).ToString("f2")}元");
                        Logs($"{sbz}/btc 涨跌幅：{dzdfbz.ToString("f2")}");
                    }
                    Jy obj = new Jy
                    {
                        Sbz = sbz,
                        Cjj = rbz.ToString("f10"),
                        CjjRmb = rbz * result >= 0.1 ? (rbz * result).ToString("f4") : (rbz * result).ToString("f6"),
                        OwnerYe = dba.ToString(),
                        OwnerRmb = rbz * result * dba,
                        Zdf = dzdfbz
                    };

                    jr.Cjj = rbz * result;
                    if (jr.Cjj != 0d)
                    {
                        ls.Add(obj);
                    }

                    lock (SequenceLock)
                    {
                        string sql = $"select  * from Jy where sbz='{obj.Sbz}' order by dt desc limit 0,1";
                        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

                        SQLiteDataReader reader = SQLiteHelper.ExecuteReader(sconn, command) as SQLiteDataReader;

                        var dt2 = DateTime.Now; ;
                        if (reader.HasRows)
                        {

                            while (reader.Read())
                            {
                                dt2 = DateTime.Parse(reader["dt"].ToString());
                            }

                            var dt1 = DateTime.Now;

                            TimeSpan span = dt1 - dt2;
                            //if (span.TotalDays > 60)
                            //{

                            //}
                            //else if (span.TotalDays > 30)
                            //{

                            //}
                            //else if (span.TotalDays > 14)
                            //{

                            //}
                            //else if (span.TotalDays > 7)
                            //{

                            //}
                            //else if (span.TotalDays > 1)
                            //{

                            //}
                            //else if (span.TotalHours > 1)
                            //{
                            //    //return string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));
                            //}
                            //else 
                            if (span.TotalMinutes > 20)
                            {
                                Logs(string.Format("[xxxxxxxxxxxxxxx]{0}没有写进数据了，请检查！", (int)Math.Floor(span.TotalMinutes)));
                            }

                            if (span.TotalMinutes >= 10) //10分钟存一次
                            {
                                SaveBz(sbz, result, obj, ref sql, ref command);
                            }
                            else if (span.TotalSeconds >= 1)
                            {
                                //return string.Format("{0}秒前",
                                //(int)Math.Floor(span.TotalSeconds));
                            }

                            else
                            {
                                //return "1秒前";
                            }

                        }
                        else
                        {
                            SaveBz(sbz, result, obj, ref sql, ref command);
                        }
                    }


                }
                else
                {
                    Logs($"【超时】{ic - ii}币种：{sbz}获取开盘价时出错");

                }

                //}
                //}
                jr.Dtotal = dtotal;
                return jr;
                #endregion
            }
            catch (System.Threading.SynchronizationLockException ex)
            {
                jr.Dtotal = dtotal;
                return jr;
            }
            finally
            {
                // Monitor.Exit(o);
                //spinLock.Exit();
            }
        }

        /// <summary>
        /// 获取kline数据中的开盘价，并持久化kline
        /// </summary>
        /// <param name="sbz"></param>
        /// <param name="result"></param>
        /// <param name="rsbz"></param>
        /// <param name="sparam"></param>
        /// <returns></returns>
        private async Task<double> KLine(string sbz, double result, string sparam)
        {
            var rsbz = 0d;
            string symbol = "";//交易对
            string[] sArray = sparam.Split(new char[1] { '&' });
            foreach (string e in sArray)
            {
                if (e.Contains("symbol="))
                {
                    symbol = e.Replace("symbol=", "");
                    break;
                }
            }


            if (SetView.vw_kline)
            {
                string sql = $"select* from  KLine where symbol='{symbol}' and  rq='{DateTime.Now.ToShortDateString()}' order by dt desc limit 0,1";

                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

                SQLiteDataReader reader = SQLiteHelper.ExecuteReader(sconn, command) as SQLiteDataReader;

                var dt2 = DateTime.Now; ;
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        rsbz = double.Parse(reader["open"].ToString());
                        Logs($"[info]优先读取数据库，币种：{sbz}从数据库中提取开盘价{rsbz}");
                        dt2 = DateTime.Parse(reader["ts"].ToString());

                        var dt1 = DateTime.Now;

                        TimeSpan span = dt1 - dt2;

                        if (span.TotalMinutes > 20)
                        {
                            Logs(string.Format("[info]表kline,{0}分钟没有写进数据了，请检查！", (int)Math.Floor(span.TotalMinutes)));
                        }
                        if (span.TotalMinutes >= 10) //10分钟存一次
                        {
                            rsbz = await GetMarketHistoryKlineStore(sbz, result, sparam, rsbz, symbol);
                        }
                    }
                }
                else
                {
                    if (rsbz == 0)
                    {
                        rsbz = await GetMarketHistoryKlineStore(sbz, result, sparam, rsbz, symbol);
                    }
                }
            }
            else
            {
                rsbz = await GetMarketHistoryKlineStore(sbz, result, sparam, rsbz, symbol);
            }

            return rsbz;
        }

        private async Task<double> GetMarketHistoryKlineStore(string sbz, double result, string sparam, double rsbz, string symbol)
        {
            MarketHistoryKline mk = new MarketHistoryKline();
            //var rsbz = await api.GetMarketHistoryKline(sparam, mk); //开盘价

            var tmprs = await api.GetMarketHistoryKline(sparam);
            mk = tmprs;

            try
            {
                rsbz = tmprs.DataMd?[0].Open ?? 0;
            }
            catch
            {

            }
            #region 开盘价数据持久化
            if (rsbz != 0)
            {
                lock (SequenceLock)
                {
                    string sql = $"select* from  KLine where symbol='{symbol}' and  rq='{DateTime.Now.ToShortDateString()}' order by dt desc limit 0,1";

                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

                    SQLiteDataReader reader = SQLiteHelper.ExecuteReader(sconn, command) as SQLiteDataReader;

                    var dt2 = DateTime.Now; ;
                    if (reader.HasRows)
                    {

                        while (reader.Read())
                        {
                            dt2 = DateTime.Parse(reader["ts"].ToString());
                        }

                        var dt1 = DateTime.Now;

                        TimeSpan span = dt1 - dt2;

                        if (span.TotalMinutes > 20)
                        {
                            Logs(string.Format("[xxxxxxxxxxxxxxx]表kline,{0}分钟没有写进数据了，请检查！", (int)Math.Floor(span.TotalMinutes)));
                        }

                        if (span.TotalMinutes >= 10) //10分钟存一次
                        {
                            SaveKline(symbol, result, mk, "", command);
                        }
                        else if (span.TotalSeconds >= 1)
                        {

                        }

                        else
                        {

                        }

                    }
                    else
                    {
                        SaveKline(symbol, result, mk, "", command);
                    }
                }
            }
            else
            {
                string sql = $"select* from  KLine where symbol='{symbol}' and  rq='{DateTime.Now.ToShortDateString()}' order by dt desc limit 0,1";

                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

                SQLiteDataReader reader = SQLiteHelper.ExecuteReader(sconn, command) as SQLiteDataReader;

                var dt2 = DateTime.Now; ;
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        rsbz = double.Parse(reader["open"].ToString());
                    }

                    Logs($"[info]币种：{sbz}从数据库中提取开盘价{rsbz}");
                }
            }
            #endregion
            return rsbz;
        }

        /// <summary>
        /// 持久化币种数据
        /// </summary>
        /// <param name="sbz"></param>
        /// <param name="result"></param>
        /// <param name="obj"></param>
        /// <param name="sql"></param>
        /// <param name="command"></param>
        private void SaveBz(string sbz, double result, Jy obj, ref string sql, ref SQLiteCommand command)
        {
            if (double.Parse(obj.CjjRmb) > 0) //成交价等于0，是非正常数据
            {
                var tmpb = FormHb.hcs.Data.Where(a => a.QuoteCurrency == "btc" && a.BaseCurrency == sbz).Select(a => new { a.PricePrecision, a.AmountPrecision }).ToList();
                //Logs($"BTC可交易币种总数为：{tmpb.Count}个");

                string sformat = $"f{tmpb[0].PricePrecision}";
                string scjj = double.Parse(obj.Cjj).ToString(sformat);

                sformat = $"f{tmpb[0].AmountPrecision}";
                string sye = double.Parse(obj.OwnerYe).ToString(sformat);

                //btc varchar(20),usdt varchar(20),dt v
                string sdt = DateTime.Now.ToString("s");// DateTime.Now.ToString("yyyy - MM - dd HH: mm: ss: ffff");
                string srq = DateTime.Now.ToShortDateString();
                sql = $"insert into Jy (sbz, cjj,cjjrmb,ownerye,ownerrmb,zdf,btc,usdt,dt,rq) values ('{obj.Sbz}', '{scjj}','{obj.CjjRmb}','{sye}','{obj.OwnerRmb.ToString("f3")}','{Math.Round(obj.Zdf, 3)}','{Math.Round(result, 0)}','{otcusdt}','{sdt}','{srq}')";
                command = new SQLiteCommand(sql, m_dbConnection);

                int i = SQLiteHelper.ExecuteNonQuery(sconn, command);
            }
        }


        /// <summary>
        /// 持久化k线数据
        /// </summary>
        /// <param name="sbz"></param>
        /// <param name="result"></param>
        /// <param name="obj"></param>
        /// <param name="sql"></param>
        /// <param name="command"></param>
        private void SaveKline(string sbz, double result, MarketHistoryKline obj, string sql, SQLiteCommand command)
        {
            try
            {
                if (obj.DataMd[0].Open > 0)
                {

                    string sdt = DateTime.Now.ToString("s");// DateTime.Now.ToString("yyyy - MM - dd HH: mm: ss: ffff");
                    string srq = DateTime.Now.ToShortDateString();
                    sql = string.Format(@"INSERT INTO KLine (
                      symbol,
                      ts,
                      kid,
                      amount,
                      count,
                      open,
                      close,
                      low,
                      high,
                      vol,
                      openrmb,
                      dt,
                      rq
                  )
                  VALUES (
                      '{0}',
                      '{1}',
                      '{2}',
                      '{3}',
                      '{4}',
                      '{5}',
                      '{6}',
                      '{7}',
                      '{8}',
                      '{9}',
                      '{10}','{11}','{12}'
                  )", sbz, obj.Ts, obj.DataMd[0].Id, obj.DataMd[0].Amount, obj.DataMd[0].Count, obj.DataMd[0].Open, obj.DataMd[0].Close, obj.DataMd[0].Low, obj.DataMd[0].High, obj.DataMd[0].Vol, result, sdt, srq);

                    command = new SQLiteCommand(sql, m_dbConnection);

                    int i = SQLiteHelper.ExecuteNonQuery(sconn, command);
                    if (i == 1)
                    {
                        Logs($"{sbz}更新kline");
                    }
                }
            }
            catch
            {

            }

        }


        private async void Button2_Click(object sender, EventArgs e)
        {
            //    CommonSymbols hcs = api.GetCommonSymbols();
            //    //main主区，innovation创新区，bifurcation分叉区
            //    //var tmpm = hcs.Data.Where(a => a.SymbolPartition == "main").GroupBy(a => a.QuoteCurrency).Select(g => new { name = g.Key}).ToList();

            //    //var tmpmb = hcs.Data.Where(a => a.SymbolPartition == "main" && a.QuoteCurrency=="btc");

            //    //var tmpi = hcs.Data.Where(a => a.SymbolPartition == "innovation");

            //    var tmpb = hcs.Data.Where(a => a.QuoteCurrency == "btc").ToList();
            //    Logs($"BTC可交易币种总数为：{tmpb.Count}");




        }

        private async Task<OpenOrders> OpenOrders(string sparams, double sjg, string snocancel = "")
        {
            string sfx = "";
            if (sparams.Contains("side=buy"))
            {
                sfx = "买";
            }
            else
            {
                sfx = "卖";
            }
            var s = await api.GetOpenOrders(sparams);

            if (s.Status == "ok")
            {
                if (s.Data.Count >= 1)
                {
                    foreach (var tmp in s.Data)
                    {
                        if (sfx == "卖")
                        {
                            Logs($"[未成交委托卖单] 币种:{tmp.Symbol}，下单时间：{tmp.CreatedAt},订单状态:{tmp.State},下单数量：{tmp.Amount},已成交数量:{tmp.FilledAmount},手续费：{tmp.FilledFees},下单价格：{(tmp.Price * sjg)}");
                        }
                        else
                        {
                            Logs($"[未成交委托买单] 币种:{tmp.Symbol}，下单时间：{tmp.CreatedAt},订单状态:{tmp.State},下单数量：{tmp.Amount},已成交数量:{tmp.FilledAmount},手续费：{tmp.FilledFees},下单价格：{(tmp.Price * sjg)}");
                        }
                        var result = await api.GetDataTime();

                        TimeSpan span = result - tmp.CreatedAt;
                        //if (span.TotalDays > 60)
                        //{

                        //}
                        //else if (span.TotalDays > 30)
                        //{

                        //}
                        //else if (span.TotalDays > 14)
                        //{

                        //}
                        //else if (span.TotalDays > 7)
                        //{

                        //}
                        //else if (span.TotalDays > 1)
                        //{

                        //}
                        //else if (span.TotalHours > 1)
                        //{
                        //    //return string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));
                        //}
                        //else 
                        if (span.TotalMinutes >= 5)
                        {
                            //return string.Format("{0}分钟前", (int)Math.Floor(span.TotalMinutes));
                            if (snocancel == "")
                            {
                                if (snocancel.Contains(tmp.Symbol))
                                {

                                }
                                else
                                {
                                    //var cresult = await api.OrderPlaceCancel(tmp.Id.ToString());
                                    ////取消订单
                                    ////Assert.AreEqual(cresult.Status, "ok");

                                    //var sresult = await api.OrderPlaceDetail(tmp.Id.ToString());
                                    //查询订单详情
                                }                                                       //Assert.AreEqual(sresult.Status, "ok");
                            }
                        }
                        else if (span.TotalSeconds >= 1)
                        {
                            //return string.Format("{0}秒前",
                            //(int)Math.Floor(span.TotalSeconds));
                        }

                        else
                        {
                            //return "1秒前";
                        }
                        Logs($"已下单{span.TotalMinutes}分{span.TotalSeconds}秒，5分钟未成交会取消!");

                    }
                }
                else
                {

                }
            }

            return s;
        }

        /// <summary>
        /// 美化
        /// </summary>
        /// <param name="dataGridView"></param>
        private void dataGridView(DataGridView dataGridView)
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.LightCyan;
            dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView.BackgroundColor = System.Drawing.Color.White;
            dataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;//211, 223, 240
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(223)))), ((int)(((byte)(240)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Navy;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.GridColor = System.Drawing.SystemColors.GradientInactiveCaption;
            dataGridView.ReadOnly = true;
            dataGridView.RowHeadersVisible = false;
            dataGridView.RowTemplate.Height = 23;
            dataGridView.RowTemplate.ReadOnly = true;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        #region 加入checkBox
        /// <summary>
        /// 加入checkBox
        /// </summary>
        /// <param name="datagridview"></param>
        private void chekBox(DataGridView datagridview)
        {
            DataGridViewCheckBoxColumn newColumn = new DataGridViewCheckBoxColumn();//添加CHECKBOX
            newColumn.HeaderText = "选择";
            datagridview.Columns.Insert(0, newColumn);
            datagridview.MultiSelect = true;
            newColumn.InheritedStyle.Alignment = DataGridViewContentAlignment.TopCenter;
            newColumn.Width = 35;
            datagridview.VirtualMode = false;
        }
        #endregion



        //dataGrvieCheckBox是否全选
        private void dataGrviesCheckBoxIsFT(CheckBox checkbox, DataGridView datagridview)
        {
            if (checkbox.Checked == true)
            {
                if (datagridview.Rows.Count > 0)
                {
                    for (int i = 0; i < datagridview.Rows.Count; i++)
                    {

                        ((DataGridViewCheckBoxCell)datagridview.Rows[i].Cells[0]).Value = true;
                    }
                }
            }
            else
            {
                if (datagridview.Rows.Count > 0)
                {
                    for (int i = 0; i < datagridview.Rows.Count; i++)
                    {
                        ((DataGridViewCheckBoxCell)datagridview.Rows[i].Cells[0]).Value = false;
                    }
                }
            }
        }




        //选中删除的记录
        private void button15_Click(object sender, EventArgs e)
        {
            //if (dataFssb.Rows.Count > 0)
            //{
            //    for (int i = 0; i < dataFssb.Rows.Count; i++)
            //    {
            //        string _selectValue = dataFssb.Rows[i].Cells[0].EditedFormattedValue.ToString();
            //        if (_selectValue == "True")
            //        {
            //            //MessageBox.Show(dataGridView2.Rows[i].Cells["fsmx_no"].Value.ToString());


            //        }
            //    }
            //}


        }

        private void Button3_Click(object sender, EventArgs e)
        {
            dataGridView(this.dgv_jy);

            //string message = AesSecret.Encrypt_AES("thisissssss").ToString();
            //Logs(message);

            //message = AesSecret.Decrypt_AES(message).ToString();

            //Logs(message);
            return;
            // MyConsole mc = new MyConsole();
            //Logs(Console.WindowHeight);
            //Logs(Console.BufferHeight);

            //Console.ReadKey();

            //设置窗口宽度高度
            Console.Title = "Haha";//设置窗口标题
            Console.WindowWidth = 120;
            Console.BufferHeight = 1000;
            //Logs(Console.WindowWidth);
            //Logs(Console.WindowHeight);
            //Logs("---------------------");
            //Logs(Console.BufferWidth);
            //Logs(Console.BufferHeight);

            //设置窗口字体颜色和背景颜色：
            Console.BackgroundColor = ConsoleColor.Blue; //设置背景色
            Console.ForegroundColor = ConsoleColor.White; //设置前景色，即字体颜色
            Logs("第一行白蓝.");
            Console.ResetColor(); //将控制台的前景色和背景色设为默认值
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            string str = "第三行 绿暗绿";
            Logs(str.PadRight(Console.BufferWidth - (str.Length % Console.BufferWidth))); //设置一整行的背景色
            Console.ResetColor();


            ShowColor();
            int m = Console.CursorTop;//查看当前行号Console.BufferHeight 
            ShowColor();
            int n = Console.CursorTop;
            ShowColor();
            int o = Console.CursorTop;
            Console.ReadKey();
        }

        //显示出console中支持的背景色及前景色
        static void ShowColor()
        {
            Type type = typeof(ConsoleColor);
            Console.ForegroundColor = ConsoleColor.White;
            foreach (string name in Enum.GetNames(type))
            {
                Console.BackgroundColor = (ConsoleColor)Enum.Parse(type, name);
                //Logs(name);
            }
            Console.BackgroundColor = ConsoleColor.Black;
            foreach (string name in Enum.GetNames(type))
            {
                Console.ForegroundColor = (ConsoleColor)Enum.Parse(type, name);
                //Logs(name);
            }
            foreach (string bc in Enum.GetNames(type))
            {
                Console.BackgroundColor = (ConsoleColor)Enum.Parse(type, bc);
                foreach (string fc in Enum.GetNames(type))
                {
                    Console.ForegroundColor = (ConsoleColor)Enum.Parse(type, fc);
                    //Logs("bc=" + bc + ",fc=" + fc);
                }
                //Logs();
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            bool hasForm = false; //判断窗体是否已经弹出
            FormMm f2 = new FormMm(); //实例化一个弹出窗体对象
            f2.Owner = this;     //将弹出对象的拥有者设置为当前窗体

            if (lform.Count > 0) //如果集合中有元素
            {
                foreach (Form f in lform) //遍历它们
                {
                    if (f.Name == f2.Name) //如果要弹出的窗体对象已经存在
                    {
                        hasForm = true; //表示已经弹出
                        f.Focus();     //焦点定位已经弹出的窗口
                    }
                }
            }

            if (hasForm) //当前已经有窗口弹出
            {
                f2.Close(); //把新实例化的窗体对象给释放掉
            }
            else //如果没有窗口弹出
            {
                lform.Add(f2);  //将新窗体添加进集合
                f2.StartPosition = FormStartPosition.CenterScreen;
                f2.Show();
            }
        }

        public void _Remove(Form f)
        {
            if (lform.Contains(f))
            {
                lform.Remove(f);
            }
            if (hstable.Contains(f.Name))
            {
                hstable.Remove(f.Name);
            }
        }

        private List<Jy> DataBindingByList2()
        {
            List<Jy> l = new List<Jy>();
            for (int i = 0; i < 10; i++)
            {
                l.Add(new Jy { Sbz = i.ToString(), Cjj = i.ToString() + "_List" });
            }
            return l;
        }

        private void Dgv_jy_Click(object sender, EventArgs e)
        {

        }

        private void Dgv_jy_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                DataGridView dv = (DataGridView)sender;
                if (dv.Rows[e.RowIndex].Cells["Sbz"].Value == null) return;
                string sbz = dv.Rows[e.RowIndex].Cells["Sbz"].Value.ToString();
                string je = dv.Rows[e.RowIndex].Cells["OwnerRmb"].Value.ToString();
                double cjjrmp = double.Parse(dv.Rows[e.RowIndex].Cells["CjjRmb"].Value.ToString());
                if (dv.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "关注")
                {
                    if (e.Value == null)
                    {
                        if (Lcbbz.Contains(sbz))
                        {
                            e.Value = true;
                        }
                        return;
                    }

                }
                if (dv.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "涨跌幅")
                {
                    if (e.Value == null)
                    {
                        return;
                    }
                    else
                    {
                        double d = double.Parse(e.Value.ToString());
                        if (d > 0)
                        {
                            e.CellStyle.ForeColor = Color.Red;
                        }
                        else
                        {
                            e.CellStyle.ForeColor = Color.Green;
                        }

                        string sreturn = $"{d.ToString("f2")}%";
                        e.Value = sreturn;
                        return;
                    }
                }

                if (dv.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "成交价/btc")
                {
                    if (e.Value == null)
                    {
                        return;
                    }
                    else
                    {
                        double d = double.Parse(e.Value.ToString());

                        if (FormHb.hcs == null)
                        {
                            return;
                        }
                        var tmpb = FormHb.hcs.Data.Where(a => a.QuoteCurrency == "btc" && a.BaseCurrency == sbz).Select(a => new { a.PricePrecision, a.AmountPrecision }).ToList();
                        //Logs($"BTC可交易币种总数为：{tmpb.Count}个");

                        string sformat = $"f{tmpb[0].PricePrecision}";
                        string sreturn = d.ToString(sformat);
                        e.Value = sreturn;

                        return;
                    }
                }

                if (dv.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "已买数量")
                {
                    double dok;
                    if (double.TryParse(e.Value.ToString(), out dok))
                    {
                        if (dok < 1)
                        {
                            e.Value = "";
                            return;
                        }
                    }
                    else
                    {
                        e.Value = "";
                        return;
                    }

                    if (e.Value == null)
                    {
                        return;
                    }
                    else
                    {
                        double d = double.Parse(e.Value.ToString());
                        if (d == 0)
                        {
                            e.Value = "";
                            return;
                        }
                        if (FormHb.hcs == null)
                        {
                            return;
                        }
                        var tmpb = FormHb.hcs.Data.Where(a => a.QuoteCurrency == "btc" && a.BaseCurrency == sbz).Select(a => new { a.PricePrecision, a.AmountPrecision }).ToList();
                        //Logs($"BTC可交易币种总数为：{tmpb.Count}个");

                        string sformat = $"f{tmpb[0].AmountPrecision}";
                        string sreturn = d.ToString(sformat);
                        e.Value = sreturn;

                        return;
                    }
                }

                if (dv.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "资产（元）")
                {
                    if (e.Value == null)
                    {
                        return;
                    }
                    else
                    {
                        double d = double.Parse(e.Value.ToString());
                        if (d >= 5)
                        {
                            string sreturn = $"{d.ToString("f2")}";
                            e.Value = sreturn;
                        }
                        else
                        {
                            e.Value = "";
                        }
                        return;
                    }
                }

                if (dv.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "昨天涨跌幅")
                {
                    double d = 0d;//涨跌幅
                    double drmb = 0d;
                    string skey = sbz + "昨天";
                    if (BufferCjj[skey] == null)
                    {
                        drmb = TodayJg(sbz);

                        BufferCjj[skey] = drmb;

                    }
                    else
                    {
                        drmb = (double)BufferCjj[skey];
                    }


                    if (drmb == 0)   //异常
                    {
                        e.Value = "";
                        return;
                    }
                    else
                    {
                        d = ((cjjrmp - drmb) / drmb) * 100;
                    }

                    if (d > 0)
                    {
                        e.CellStyle.ForeColor = Color.Red;
                    }
                    else
                    {
                        e.CellStyle.ForeColor = Color.Green;
                    }

                    string sreturn = $"{d.ToString("f2")}%";
                    e.Value = sreturn;
                    return;

                }

                if (dv.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "前天涨跌幅")
                {
                    double d = 0d;//涨跌幅
                    double drmb = 0d;
                    string skey = sbz + "前天";
                    if (BufferCjj[skey] == null)
                    {
                        drmb = QtJg(sbz);
                        BufferCjj[skey] = drmb;
                    }
                    else
                    {
                        drmb = (double)BufferCjj[skey];
                    }



                    if (drmb == 0)   //异常
                    {
                        e.Value = "";
                        return;
                    }
                    else
                    {
                        d = ((cjjrmp - drmb) / drmb) * 100;
                    }

                    if (d > 0)
                    {
                        e.CellStyle.ForeColor = Color.Red;
                    }
                    else
                    {
                        e.CellStyle.ForeColor = Color.Green;
                    }

                    string sreturn = $"{d.ToString("f2")}%";
                    e.Value = sreturn;
                    return;

                }

            }
            catch
            {

            }
        }

        private double QtJg(string sbz)
        {
            double drmb = 0d;
            string sqdt = DateTime.Now.AddDays(-2).ToShortDateString(); //前天
            string sjdt = DateTime.Now.ToShortDateString(); //今天

            string sql = $"select  * from Jy where sbz='{sbz}'  and rq='{sqdt}' order by dt desc limit 0,1";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

            SQLiteDataReader reader = SQLiteHelper.ExecuteReader(sconn, command) as SQLiteDataReader;


            if (reader.HasRows)
            {

                while (reader.Read())
                {
                    drmb = double.Parse(reader["cjjrmb"].ToString());
                }

            }
            else
            {
                sql = $"select  * from Jy where sbz='{sbz}'  and rq='{sjdt}' order by dt asc limit 0,1";
                command = new SQLiteCommand(sql, m_dbConnection);
                reader = SQLiteHelper.ExecuteReader(sconn, command) as SQLiteDataReader;
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        drmb = double.Parse(reader["cjjrmb"].ToString());
                    }
                }
            }

            return drmb;
        }

        private double TodayJg(string sbz)
        {
            double drmb = 0d;
            string sqdt = DateTime.Now.AddDays(-1).ToShortDateString(); //昨天
            string sjdt = DateTime.Now.ToShortDateString(); //今天

            string sql = $"select  * from Jy where sbz='{sbz}'  and rq='{sqdt}' order by dt desc limit 0,1";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

            SQLiteDataReader reader = SQLiteHelper.ExecuteReader(sconn, command) as SQLiteDataReader;


            if (reader.HasRows)
            {

                while (reader.Read())
                {
                    drmb = double.Parse(reader["cjjrmb"].ToString());
                }

            }
            else
            {
                sql = $"select  * from Jy where sbz='{sbz}'  and rq='{sjdt}' order by dt asc limit 0,1";
                command = new SQLiteCommand(sql, m_dbConnection);
                reader = SQLiteHelper.ExecuteReader(sconn, command) as SQLiteDataReader;
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        drmb = double.Parse(reader["cjjrmb"].ToString());
                    }
                }
            }

            return drmb;
        }

        private void Dgv_jy_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                double intGrade = Convert.ToDouble(this.dgv_jy.Rows[e.RowIndex].Cells[4].Value);
                if (intGrade >= 5)
                {
                    dgv_jy.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                }
                //else if (intGrade == 25)
                //{
                //    dgv_jy.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Brown;
                //}
            }

        }



        private void Button5_Click(object sender, EventArgs e)
        {
            this.dgv_jy.AlternatingRowsDefaultCellStyle.BackColor = Color.GreenYellow;
            //注册消息接收事件，接收服务端发送的数据
            client.MessageReceived += (data) =>
            {
                //Task.Run(()=>Logs(data));
                var ss = JsonConvert.DeserializeObject<List<Cjeb>>(data);
                if (this.dgv_zdf.IsHandleCreated)
                {
                    this.Invoke(new Action(() =>
                    {
                        dgv_zdf.AutoGenerateColumns = false;
                        dgv_zdf.DataSource = new BindingList<Cjeb>(ss);
                        dgv_zdf.Restore();
                    }));
                }
            };
            //开始链接
            client.Start();

            //Logs("输入“c”，退出");
            //var input = "";
            //do
            //{
            //    input = Console.ReadLine();
            //    //客户端发送消息到服务端
            //    client.SendMessage(input);
            //} while (input != "c");
            //client.Dispose();

        }


        /// <summary>   
        /// 运行DOS命令   
        /// DOS关闭进程命令(ntsd -c q -p PID )PID为进程的ID   
        /// </summary>   
        /// <param name="command"></param>   
        /// <returns></returns>   
        public static string RunCmd(string command)
        {
            //實例一個Process類，啟動一個獨立進程   
            System.Diagnostics.Process p = new System.Diagnostics.Process();

            //Process類有一個StartInfo屬性，這個是ProcessStartInfo類，包括了一些屬性和方法，下面我們用到了他的幾個屬性：   

            p.StartInfo.FileName = "cmd.exe";           //設定程序名   
            p.StartInfo.Arguments = "/c " + command;    //設定程式執行參數   
            p.StartInfo.UseShellExecute = false;        //關閉Shell的使用   
            p.StartInfo.RedirectStandardInput = true;   //重定向標準輸入   
            p.StartInfo.RedirectStandardOutput = true;  //重定向標準輸出   
            p.StartInfo.RedirectStandardError = true;   //重定向錯誤輸出   
            p.StartInfo.CreateNoWindow = true;          //設置不顯示窗口   

            p.Start();   //啟動   

            //p.StandardInput.WriteLine(command);       //也可以用這種方式輸入要執行的命令   
            //p.StandardInput.WriteLine("exit");        //不過要記得加上Exit要不然下一行程式執行的時候會當機   

            return p.StandardOutput.ReadToEnd();        //從輸出流取得命令執行結果   

        }
        private void Button6_Click(object sender, EventArgs e)
        {
            string exeName = "c:\\chrome.exe";
            string[] exeArray = exeName.Split('\\');

            RunCmd("taskkill /im " + exeArray[exeArray.Length - 1] + " /f ");
        }

        private void FormHb_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void Dgv_jy_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewColumn column = (sender as DataGridView).Columns[e.ColumnIndex];

                if (column is DataGridViewCheckBoxColumn)
                {
                    //判断点的是否是datagridviewcheckbox列并且不是列头
                    if (e.ColumnIndex == 0 && e.RowIndex != -1)
                    {
                        string ss = (sender as DataGridView).Rows[e.RowIndex].Cells["Sbz"].Value.ToString();
                        bool b = false;
                        b = (bool)((sender as DataGridView).Rows[e.RowIndex].Cells[0].Value ?? false);
                        lock (SequenceLock)
                        {
                            if (b)
                            {
                                if (Lcbbz.Contains(ss))
                                {
                                    Lcbbz.Remove(ss);
                                }
                                (sender as DataGridView).Rows[e.RowIndex].Cells[0].Value = !b;
                            }
                            else
                            {
                                if (!Lcbbz.Contains(ss))
                                {
                                    Lcbbz.Add(ss);
                                }
                                (sender as DataGridView).Rows[e.RowIndex].Cells[0].Value = !b;
                            }

                        }


                    }
                }
                else if (column is DataGridViewLinkColumn)
                {
                    JyEventArgs jy = null;
                    string ss = (sender as DataGridView).Name == "dgv_jy" ? (sender as DataGridView).Rows[e.RowIndex].Cells["Sbz"].Value.ToString() : (sender as DataGridView).Rows[e.RowIndex].Cells["Sbz1"].Value.ToString();
                    if (column.Name == "买")
                    {
                        Mm(ss);
                        jy = new JyEventArgs(new JyFx
                        {
                            Sfx = "买",
                            Sbz = (sender as DataGridView).Rows[e.RowIndex].Cells["Sbz"].Value.ToString(),
                            Cjj = (sender as DataGridView).Rows[e.RowIndex].Cells["Cjj"].Value.ToString(),
                            CjjRmb = (sender as DataGridView).Rows[e.RowIndex].Cells["CjjRmb"].Value.ToString(),
                            OwnerYe = (sender as DataGridView).Rows[e.RowIndex].Cells["OwnerYe"].Value.ToString(),
                            OwnerRmb = double.Parse((sender as DataGridView).Rows[e.RowIndex].Cells["OwnerRmb"].Value.ToString()),//帐户余额
                            Zdf = double.Parse((sender as DataGridView).Rows[e.RowIndex].Cells["Zdf"].Value.ToString())
                        });
                        // SendMsgEvent(this, jy);

                        string sname = $"{ss}买卖";
                        if (hstable.Contains(sname))
                        {
                            EventHandler eh = hstable[sname] as EventHandler;
                            eh(this, jy);
                        }

                    }
                    else if (column.Name == "卖")
                    {
                        Mm(ss);
                        jy = new JyEventArgs(new JyFx
                        {
                            Sfx = "卖",
                            Sbz = (sender as DataGridView).Rows[e.RowIndex].Cells["Sbz"].Value.ToString(),
                            Cjj = (sender as DataGridView).Rows[e.RowIndex].Cells["Cjj"].Value.ToString(),
                            CjjRmb = (sender as DataGridView).Rows[e.RowIndex].Cells["CjjRmb"].Value.ToString(),
                            OwnerYe = (sender as DataGridView).Rows[e.RowIndex].Cells["OwnerYe"].Value.ToString(),
                            OwnerRmb = double.Parse((sender as DataGridView).Rows[e.RowIndex].Cells["OwnerRmb"].Value.ToString()),//帐户余额
                            Zdf = double.Parse((sender as DataGridView).Rows[e.RowIndex].Cells["Zdf"].Value.ToString())

                        });
                        string sname = $"{ss}买卖";
                        if (hstable.Contains(sname))
                        {
                            EventHandler eh = hstable[sname] as EventHandler;
                            eh(this, jy);
                        }
                    }


                }
                else
                {
                }
            }
        }

        private void Mm(string sbz)
        {
            try
            {
                bool hasForm = false; //判断窗体是否已经弹出
                FormMm f2 = new FormMm(); //实例化一个弹出窗体对象
                f2.Owner = this;     //将弹出对象的拥有者设置为当前窗体
                f2.Name = $"{sbz}买卖";
                f2.Text = $"{sbz}交易";

                if (lform.Count > 0) //如果集合中有元素
                {
                    foreach (Form f in lform) //遍历它们
                    {
                        if (f.Name == f2.Name) //如果要弹出的窗体对象已经存在
                        {
                            hasForm = true; //表示已经弹出
                            f.Focus();     //焦点定位已经弹出的窗口
                        }
                    }
                }

                if (hasForm) //当前已经有窗口弹出
                {
                    f2.Close(); //把新实例化的窗体对象给释放掉
                }
                else //如果没有窗口弹出
                {
                    lform.Add(f2);  //将新窗体添加进集合
                    if (!hstable.Contains(f2.Name))
                    {
                        hstable.Add(f2.Name, new EventHandler(f2.MainFormMsgChanged));
                    }
                    f2.StartPosition = FormStartPosition.CenterScreen;
                    //if (SendMsgEvent != null)
                    //{
                    //    Delegate[] dels = SendMsgEvent.GetInvocationList();
                    //    foreach (Delegate d in dels)
                    //    {
                    //        //得到方法名
                    //        object delObj = d.GetType().GetProperty("Method").GetValue(d, null);
                    //        string funcName = (string)delObj.GetType().GetProperty("Name").GetValue(delObj, null);
                    //        Debug.Print(funcName);
                    //        SendMsgEvent -= d as EventHandler;
                    //    }

                    //}
                    //if (SendMsgEvent == null)
                    //    SendMsgEvent += f2.MainFormMsgChanged;

                    if (cb_jyshow.Checked)
                    {
                        if (ShowInDock != null)
                            ShowInDock(f2, null);
                    }
                    else
                    {
                        f2.Show();
                    }

                }
            }
            catch { }
        }

        private void Dgv_jy_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!SetView.vw_hangqin) return;
            string sbz = dgv_jy.Rows[e.RowIndex].Cells["Sbz"].Value.ToString();
            string sparams = $"https://marketapi.blockmeta.com/ticker?need_kline=huobipro-{sbz}_usd&format_type=data&symbols=huobipro-{sbz}_usd,okex-{sbz}_usd,bitfinex-{sbz}_usd,bitstamp-{sbz}_usd,binance-{sbz}_usd,gate-{sbz}_usd";
            new Thread(async () =>
            {

                var result = await api.GetHangQin(sparams);

                if (result.Tickers != null)
                {
                    Action action = () =>
                    {
                        List<TickerDgv> lgv = new List<TickerDgv>();
                        if (result.Tickers.Count >= 1)
                        {
                            foreach (var hq in result.Tickers)
                            {
                                TickerDgv tg = new TickerDgv();
                                tg.Exchange = hq.Exchange;
                                //tg.Pair = hq.Pair;
                                //tg.ConvertCny =  * hq.Pair;
                                tg.Last = Math.Round(hq.Tk.Last * hq.ConvertCny, 6);
                                lgv.Add(tg);
                            }
                            dgv_bzhq.AutoGenerateColumns = true;
                            dgv_bzhq.DataSource = new BindingList<TickerDgv>(lgv);
                            dgv_bzhq.Columns[0].HeaderCell.Value = "交易所";
                            dgv_bzhq.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                            dgv_bzhq.Columns[1].HeaderCell.Value = "价格（人民币）";
                            dgv_bzhq.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                            dgv_bzhq.Refresh();
                        }


                    };
                    if (this.dgv_btcHangQin.IsHandleCreated)
                    {
                        BeginInvoke(action);
                    }
                }
                else
                {
                    dgv_bzhq.DataSource = null;
                }
            }).Start();
        }

        private void Cb_hangqin_CheckedChanged(object sender, EventArgs e)
        {


        }

        private void Dgv_bzhq_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
        }

        //数据库连接
        SQLiteConnection m_dbConnection;

        static string sdbpath = System.Windows.Forms.Application.StartupPath + "\\db\\Hb.sqlite";

        static string sconn = $"Data Source={sdbpath};Pooling=true;FailIfMissing=false;Journal Mode=WAL;Version=3;";

        //创建一个空的数据库
        void createNewDatabase()
        {
            SQLiteConnection.CreateFile(spath + "\\db\\Hb.sqlite");
        }

        void connectToDatabase()
        {
            m_dbConnection = new SQLiteConnection($"Data Source={sdbpath};Version=3;");
            m_dbConnection.Open();
        }

        //在指定数据库中创建一个table
        void createTable()
        {
            string sql = "create table Jy (sbz varchar(20), cjj varchar(20),cjjrmb varchar(20),ownerye varchar(20),ownerrmb varchar(20),zdf varchar(20),btc varchar(20),usdt varchar(20),dt varchar(50))";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();

            //string sql = "create table Jy (sbz varchar(20), cjj varchar(20),cjjrmb varchar(20),ownerye varchar(20),ownerrmb varchar(20),zdf varchar(20)";
            //SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            //command.ExecuteNonQuery();
        }

        //插入一些数据
        void fillTable()
        {
            //string sql = "insert into highscores (name, score) values ('Me', 3000)";
            //SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            //command.ExecuteNonQuery();

            //sql = "insert into highscores (name, score) values ('Myself', 6000)";
            //command = new SQLiteCommand(sql, m_dbConnection);
            //command.ExecuteNonQuery();

            //sql = "insert into highscores (name, score) values ('And I', 9001)";
            //command = new SQLiteCommand(sql, m_dbConnection);
            //command.ExecuteNonQuery();


        }

        //使用sql查询语句，并显示结果
        void printHighscores()
        {
            //string sql = "select * from highscores order by score desc";
            //SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            //SQLiteDataReader reader = command.ExecuteReader();
            //while (reader.Read())
            //    Logs("Name: " + reader["name"] + "\tScore: " + reader["score"]);
            //Console.ReadLine();
        }

        private async void Button8_Click(object sender, EventArgs e)
        {
            #region 行情
            //实例化Timer类，设置间隔时间为10000毫秒； 
            System.Timers.Timer thq = new System.Timers.Timer(200);

            //到达时间的时候执行注册事件
            thq.Elapsed += async (obj, ee) =>
            {

                Application.DoEvents();
                string sparams = "https://marketapi.blockmeta.com/ticker?need_kline=huobipro-btc_usd&format_type=data&symbols=huobipro-btc_usd,okex-btc_usd,bitfinex-btc_usd,bitstamp-btc_usd,binance-btc_usd,gate-btc_usd";
                var result = await api.GetHangQin(sparams);

                if (result != null)
                {
                    new Thread(() =>
                    {

                        Action action = () =>
                        {
                            List<TickerDgv> lgv = new List<TickerDgv>();
                            if (result.Tickers != null)
                            {
                                if (result.Tickers.Count >= 1)
                                {
                                    foreach (var hq in result.Tickers)
                                    {
                                        TickerDgv tg = new TickerDgv();
                                        tg.Exchange = hq.Exchange;
                                        //tg.Pair = hq.Pair;
                                        //tg.ConvertCny =  * hq.Pair;
                                        tg.Last = Math.Round(hq.Tk.Last * hq.ConvertCny, 0);
                                        lgv.Add(tg);
                                    }
                                }
                                this.dgv_btcHangQin.AutoGenerateColumns = true;
                                this.dgv_btcHangQin.DataSource = new BindingList<TickerDgv>(lgv);
                                this.dgv_btcHangQin.Columns[0].HeaderCell.Value = "交易所";
                                this.dgv_btcHangQin.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                                this.dgv_btcHangQin.Columns[1].HeaderCell.Value = "价格（人民币）";
                                this.dgv_btcHangQin.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                                this.dgv_btcHangQin.Refresh();
                            }
                        };
                        if (this.dgv_btcHangQin.IsHandleCreated)
                        {
                            BeginInvoke(action);
                        }
                    }).Start();
                }



            };

            //设置是执行一次（false）还是一直执行(true)； 
            thq.AutoReset = true;

            //是否执行System.Timers.Timer.Elapsed注册事件；   
            thq.Enabled = true;
            //Console.ReadKey();
            #endregion

            //createNewDatabase();
            //connectToDatabase();
            //createTable();
            //fillTable();
            //printHighscores();
            //Cache c = new Cache(100 * 1024 * 1024);
            //var options = new Options()
            //{
            //    BlockCache = c,
            //    CreateIfMissing = true,
            //};

            //var db = LevelDB.DB.Open(options,"c:\\works\\mydb2");

            ////开始时间
            //TimeSpan runTime = new TimeSpan(DateTime.Now.Ticks);

            //////循环插入 1000000 k,v
            //for (int i = 0; i < 5; i++)
            //{
            //    db.Put(i.ToString(), i.ToString() + "A");
            //}
            ////var entries = new List<KeyValuePair<string, string>>();
            ////try
            ////{
            ////    foreach (var entry in db)
            ////    {
            ////        db.Delete(entry.Key);
            ////    }
            ////}
            ////catch { }

            //var entries = new List<KeyValuePair<string, string>>();
            //Iterator dd = new LevelDB.Iterator(db, new ReadOptions {  });

            //for (dd.SeekToFirst(); dd.IsValid;dd.Next())
            //{
            //    KeyValuePair<string, string> k = new KeyValuePair<string, string>(dd.Key, dd.Value);

            //    entries.Add( k);

            //}

            ////db.Put("币种日期", "{1111}");

            ////////迭代取所有数据
            ////var entries = new List<KeyValuePair<string, string>>();
            ////try
            ////{
            ////    foreach (var entry in db)
            ////    {
            ////        entries.Add(entry);
            ////    }
            ////}
            ////catch { }

            ////取某一个key 的 值
            //string value = db.Get("1");  //大约 0.00X 几秒

            ////结束时间
            //TimeSpan timeNow = new TimeSpan(DateTime.Now.Ticks);
            ////时间间隔
            //TimeSpan ts = timeNow.Subtract(runTime).Duration();
            //Logs(" 用时：" + ts.TotalSeconds.ToString() + "秒 " + ts.TotalMilliseconds.ToString() + "毫秒");


            //System.Diagnostics.Stopwatch sp = new System.Diagnostics.Stopwatch();
            //sp.Reset();
            //sp.Start();
            //long mCount = 0;
            //while (true)
            //{
            //    db.Put(Guid.NewGuid().ToString(), "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaeraaaaaaaaaaaaaaabbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            //    if (System.Threading.Interlocked.Increment(ref mCount) % 10000 == 0)
            //    {
            //        Logs("{0} has inserted. time use {1}ms.", mCount, sp.ElapsedMilliseconds);
            //    }
            //}
        }

        private async Task GzRefresh()
        {
            MarketHistoryKline mk = new MarketHistoryKline();
            List<Jy> ljyc = new List<Jy>();
            string sbz = "";
            string sBalance = "";
            double dBalance = 0d;
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代码运行时间



            DetailOtc jg = null;
            if (BufferOtc["otc"] != null)
            {
                jg = BufferOtc["otc"] as DetailOtc;
                Logs($"[info]读缓存场外价格");
            }
            else
            {
                jg = await api.GetOtcRmb();
                BufferOtc["otc"] = jg;
                Logs($"[info]写缓存场外价格");
            }

            var result = await api.GetTradeDetail("btcusdt", jg);// 当前比特币成交价

            // var rs = await api.GetMarketHistoryKline($"symbol=btcusdt&period=1day&size=1",mk) * jg.Buy; //开盘价

            //var tmprs = await api.GetMarketHistoryKline($"symbol=btcusdt&period=1day&size=1");
            //mk = tmprs;

            var tmprs = 0d;
            string sparam = $"symbol=btcusdt&period=1day&size=1";
            tmprs = await KLine(sbz, jg.Buy, sparam);

            var rs = tmprs * jg.Buy;
            if (rs == 0)
            {
                Logs($"币种：btcusdt，获取开盘价数据有误！");
                return;
            }
            //while (rs == 0)
            //{
            //    //rs = await api.GetMarketHistoryKline($"symbol=btcusdt&period=1day&size=1",mk) * jg.Buy; //开盘价
            //    tmprs = await api.GetMarketHistoryKline($"symbol=btcusdt&period=1day&size=1");
            //    mk = tmprs;
            //    rs = tmprs.DataMd[0].Open * jg.Buy;
            //}
            while (result == 0)
            {
                Delay(500);
                result = await api.GetTradeDetail("btcusdt", jg);
            }
            double dzdf = (result - rs) / rs * 100;
            Logs($"比特币涨跌幅：{dzdf.ToString("f2")}");

            if (this.lblBtmJg.IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                {
                    this.lblBtmJg.Text = $"比特币价格：{result.ToString("f2")}元,{dzdf.ToString("f2")}%";
                }));
            }
            long sid = 0;


            if (BufferSid["sid"] != null)
            {
                sid = long.Parse(BufferSid["sid"].ToString());
                Logs($"[info]读缓存用户id");
            }
            else
            {
                sid = api.GetSpotAccountId();
                while (sid == 0)
                {
                    Delay(500);
                    sid = api.GetSpotAccountId();
                }
                BufferSid["sid"] = sid;
                Logs($"[info]写缓存用户id");
            }

            //#region 当前帐号未成交订单

            //string sparams = $"account-id={sid}&symbol=sspbtc&side=buy&size=500";
            //await api.GetOpenOrders(sparams);
            //#endregion
            IList<Huobi.Rest.CSharp.Demo.Model.List> lbalance = await api.GetAccountBalance(sid.ToString());

            while (lbalance == null)
            {
                Logs("[info]重新用户获取持仓数据");
                Delay(500);
                spinLock.Enter();
                try
                {
                    lbalance = await api.GetAccountBalance(sid.ToString());
                }
                finally
                {
                    spinLock.Exit();
                }

            }
            //var lbtc = lbalance.Where(a => a.Currency == "btc").ToList();

            //if (lbtc.Count == 2)
            //{
            //    foreach (var vbtc in lbtc)
            //    {
            //        if (vbtc.Type == "trade")
            //        {

            //            sBalance = sBalance + $"【资产】比特币可用：{vbtc.Balance.ToString("f10")},对应人民币为：{(vbtc.Balance * result).ToString("f2")}元";
            //            Logs(sBalance);
            //            dBalance = dBalance + vbtc.Balance * result;
            //        }
            //        else if (vbtc.Type == "frozen")
            //        {
            //            Logs($"【资产】比特币冻结：{vbtc.Balance.ToString("f10")},对应人民币为：{(vbtc.Balance * result).ToString("f2")}元");
            //            sBalance = sBalance + $"【资产】比特币冻结：{vbtc.Balance.ToString("f10")},对应人民币为：{(vbtc.Balance * result).ToString("f2")}元";
            //            dBalance = dBalance + vbtc.Balance * result;
            //        }
            //        else
            //        {
            //            throw new Exception("");
            //        }


            //    }
            //}

            var lbtc = lbalance.Where(a => a.Currency == "btc").ToList();

            if (lbtc.Count == 2)
            {
                foreach (var vbtc in lbtc)
                {
                    if (vbtc.Type == "trade")
                    {
                        sBalance = vbtc.Balance <= 0.000000001 ? "【资产】" : sBalance + $"【资产】比特币可用：{vbtc.Balance.ToString("f10")},对应人民币为：{(vbtc.Balance * result).ToString("f2")}元";
                        Logs(sBalance);
                        dBalance = dBalance + vbtc.Balance * result;
                    }
                    else if (vbtc.Type == "frozen")
                    {
                        Logs($"比特币冻结：{vbtc.Balance.ToString("f10")},对应人民币为：{(vbtc.Balance * result).ToString("f2")}元");
                        sBalance = vbtc.Balance <= 0.000000001 ? sBalance : sBalance + $" 比特币冻结：{vbtc.Balance.ToString("f10")},对应人民币为：{(vbtc.Balance * result).ToString("f2")}元";
                        dBalance = dBalance + vbtc.Balance * result;
                    }
                    else
                    {
                        throw new Exception("");
                    }


                }
            }

            var lusdt = lbalance.Where(a => a.Currency == "usdt").ToList();
            if (lusdt.Count == 2)
            {
                foreach (var vusdt in lusdt)
                {
                    if (vusdt.Type == "trade")
                    {

                        sBalance = vusdt.Balance <= 0.000000001 ? sBalance : sBalance + $" usdt可用：{vusdt.Balance.ToString("f10")},对应人民币为：{(vusdt.Balance * double.Parse(otcusdt)).ToString("f2")}元";
                        Logs(sBalance);
                        dBalance = dBalance + vusdt.Balance * double.Parse(otcusdt);
                    }
                    else if (vusdt.Type == "frozen")
                    {
                        Logs($"，usdt冻结：{vusdt.Balance.ToString("f10")},对应人民币为：{(vusdt.Balance * double.Parse(otcusdt)).ToString("f2")}元");
                        sBalance = vusdt.Balance <= 0.000000001 ? sBalance : sBalance + $" usdt冻结：{vusdt.Balance.ToString("f10")},对应人民币为：{(vusdt.Balance * double.Parse(otcusdt)).ToString("f2")}元";
                        dBalance = dBalance + vusdt.Balance * double.Parse(otcusdt);
                    }
                    else
                    {
                        throw new Exception("");
                    }


                }
            }


            int ic = Lcbbz.Count;
            int ii = 0;
            List<Jy> ljy = new List<Jy>();


            ljy.Clear();
            List<string> tmpl = new List<string>();
            #region
            lock (SequenceLock)
            {
                foreach (string s in Lcbbz)
                {
                    if (!tmpl.Contains(s))
                    {
                        tmpl.Add(s);
                    }
                }
            }

            foreach (var tmpbz in tmpl)
            {
                JgReturn jr;
                sbz = tmpbz;


                //Logs($"[{ii+1}.{DateTime.Now}]准备获取{tmpbz}的数据");

                double d1 = 0d;
                if (SetView.vw_debugger)
                {
                    jr = await NewMethod(sbz, result, lbalance, ic, ii, ljy);
                    d1 = jr.Dtotal;
                }
                else
                {
                    jr = await NewMethod(sbz, result, lbalance, ic, ii, ljy);
                    d1 = jr.Dtotal;
                }
                //while (d1 == 0)
                //{
                //    Delay(500);
                //    jr = await NewMethod(sbz, result, lbalance, ic, ii, ljy);
                //    d1 = jr.Dtotal;
                //}

                //Logs($"[{ii + 1}.{DateTime.Now}]{tmpbz.BaseCurrency}的数据为{d1}");
                dBalance = dBalance + d1;

                ii++;



            }


            ljyc.Clear();
            ljyc = ljy.OrderByDescending(i => i.Zdf).ToList<Jy>(); //.ThenByDescending(i => i.Zdf).ToList<Jy>();
                                                                   //foreach(var jy in ljy)
                                                                   //{
                                                                   //    ljyc.Add(jy);
                                                                   //}
            this.Invoke(new Action(() =>
            {
                Application.DoEvents();
                dgv_gz.AutoGenerateColumns = false;
                dgv_gz.DataSource = new BindingList<Jy>(ljyc);
                //dgv_gz.Refresh();
            }));




            double d = dBalance;

            //new Thread(( d, s) =>
            //{

            //    Action action = (d,s) =>
            //    {
            //        this.txtBalance.Clear();

            //        txtBalance.Text = $"资产合计：{dBalance.ToString("f2")}元," + sBalance;
            //    };
            //    BeginInvoke(action);

            //}).Start();
            if (this.txtBalance1.IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                {

                    txtBalance1.Text = $"资产合计：{dBalance.ToString("f2")}元," + sBalance;

                }));
            }

            #endregion
            stopwatch.Stop(); //  停止监视
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            double hours = timespan.TotalHours; // 总小时
            double minutes = timespan.TotalMinutes;  // 总分钟
            double seconds = timespan.TotalSeconds;  //  总秒数
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数

            Logs(result + "   " + $"{seconds}秒{milliseconds}毫秒\r\n");
        }

        private void TabPage1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Dgv_gz_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                DataGridView dv = (DataGridView)sender;
                if (dv.Rows[e.RowIndex].Cells["Sbz1"].Value == null) return;
                string sbz = dv.Rows[e.RowIndex].Cells["Sbz1"].Value.ToString();
                string je = dv.Rows[e.RowIndex].Cells["OwnerRmb1"].Value.ToString();
                double cjjrmp = double.Parse(dv.Rows[e.RowIndex].Cells["CjjRmb1"].Value.ToString());

                if (dv.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "涨跌幅")
                {
                    if (e.Value == null)
                    {
                        return;
                    }
                    else
                    {
                        double d = double.Parse(e.Value.ToString());
                        if (d > 0)
                        {
                            e.CellStyle.ForeColor = Color.Red;
                        }
                        else
                        {
                            e.CellStyle.ForeColor = Color.Green;
                        }

                        string sreturn = $"{d.ToString("f2")}%";
                        e.Value = sreturn;
                        return;
                    }
                }

                if (dv.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "成交价/btc")
                {
                    if (e.Value == null)
                    {
                        return;
                    }
                    else
                    {
                        double d = double.Parse(e.Value.ToString());

                        if (FormHb.hcs == null)
                        {
                            return;
                        }
                        var tmpb = FormHb.hcs.Data.Where(a => a.QuoteCurrency == "btc" && a.BaseCurrency == sbz).Select(a => new { a.PricePrecision, a.AmountPrecision }).ToList();
                        //Logs($"BTC可交易币种总数为：{tmpb.Count}个");

                        string sformat = $"f{tmpb[0].PricePrecision}";
                        string sreturn = d.ToString(sformat);
                        e.Value = sreturn;

                        return;
                    }
                }

                if (dv.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "已买数量")
                {
                    double dok;
                    if (double.TryParse(e.Value.ToString(), out dok))
                    {
                        if (dok < 1)
                        {
                            e.Value = "";
                            return;
                        }
                    }
                    else
                    {
                        e.Value = "";
                        return;
                    }

                    if (e.Value == null)
                    {
                        return;
                    }
                    else
                    {
                        double d = double.Parse(e.Value.ToString());
                        if (d == 0)
                        {
                            e.Value = "";
                            return;
                        }
                        if (FormHb.hcs == null)
                        {
                            return;
                        }
                        var tmpb = FormHb.hcs.Data.Where(a => a.QuoteCurrency == "btc" && a.BaseCurrency == sbz).Select(a => new { a.PricePrecision, a.AmountPrecision }).ToList();
                        //Logs($"BTC可交易币种总数为：{tmpb.Count}个");

                        string sformat = $"f{tmpb[0].AmountPrecision}";
                        string sreturn = d.ToString(sformat);
                        e.Value = sreturn;

                        return;
                    }
                }

                if (dv.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "资产（元）")
                {
                    if (e.Value == null)
                    {
                        return;
                    }
                    else
                    {
                        double d = double.Parse(e.Value.ToString());
                        if (d >= 5)
                        {
                            string sreturn = $"{d.ToString("f2")}";
                            e.Value = sreturn;
                        }
                        else
                        {
                            e.Value = "";
                        }
                        return;
                    }
                }

                if (dv.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "日涨跌幅")
                {
                    double d = 0d;//涨跌幅
                    double drmb = 0d;
                    string skey = sbz + "昨天";
                    if (BufferCjj[skey] == null)
                    {
                        string sqdt = DateTime.Now.AddDays(-1).ToShortDateString(); //前一天
                        string sjdt = DateTime.Now.ToShortDateString(); //前一天

                        string sql = $"select  * from Jy where sbz='{sbz}'  and rq='{sqdt}' order by dt desc limit 0,1";
                        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

                        SQLiteDataReader reader = SQLiteHelper.ExecuteReader(sconn, command) as SQLiteDataReader;


                        if (reader.HasRows)
                        {

                            while (reader.Read())
                            {
                                drmb = double.Parse(reader["cjjrmb"].ToString());
                            }

                        }
                        else
                        {
                            sql = $"select  * from Jy where sbz='{sbz}'  and rq='{sjdt}' order by dt asc limit 0,1";
                            command = new SQLiteCommand(sql, m_dbConnection);
                            reader = SQLiteHelper.ExecuteReader(sconn, command) as SQLiteDataReader;
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    drmb = double.Parse(reader["cjjrmb"].ToString());
                                }
                            }
                        }

                        BufferCjj[skey] = drmb;
                    }
                    else
                    {
                        drmb = double.Parse(BufferCjj[skey].ToString());
                    }

                    if (drmb == 0)   //异常
                    {
                        e.Value = "";
                        return;
                    }
                    else
                    {
                        d = ((cjjrmp - drmb) / drmb) * 100;
                    }

                    if (d > 0)
                    {
                        e.CellStyle.ForeColor = Color.Red;
                    }
                    else
                    {
                        e.CellStyle.ForeColor = Color.Green;
                    }

                    string sreturn = $"{d.ToString("f2")}%";
                    e.Value = sreturn;
                    return;

                }


            }
            catch
            {

            }
        }

        private void Dgv_jy_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!SetView.vw_introduction) return;
            string sbz = dgv_jy.Rows[e.RowIndex].Cells["Sbz"].Value.ToString();
            FormJs f = new FormJs(sbz);
            f.Show();

        }

        private void TabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            //SolidBrush back = new SolidBrush(Color.FromArgb(45, 45, 48));
            //SolidBrush white = new SolidBrush(Color.FromArgb(122, 193, 255));
            //Rectangle rec = tabControl1.GetTabRect(0);
            //e.Graphics.FillRectangle(back, rec);
            //Rectangle rec1 = tabControl1.GetTabRect(1);
            //e.Graphics.FillRectangle(back, rec1);
            //StringFormat sf = new StringFormat();
            //sf.Alignment = StringAlignment.Center;
            //for (int i = 0; i < tabControl1.TabPages.Count; i++)
            //{
            //    Rectangle rec2 = tabControl1.GetTabRect(i);
            //    e.Graphics.DrawString(tabControl1.TabPages[i].Text, new Font("微软雅黑", 9), white, rec2, sf);
            //}
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            LogInit();
            logger.Trace("trace log message");
            logger.Debug("debug log message");
            logger.Info("info log message");
            logger.Warn("warn log message");
            logger.Error("error log message");
            logger.Fatal("fatal log message");
        }

        private static void LogInit()
        {
            // Step 1. Create configuration object 

            LoggingConfiguration config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 

            ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);

            FileTarget fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 

            consoleTarget.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";
            fileTarget.FileName = "${basedir}/file.txt";
            fileTarget.Layout = "${message}";

            // Step 4. Define rules 

            LoggingRule rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(rule1);

            LoggingRule rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule2);

            // Step 5. Activate the configuration 

            LogManager.Configuration = config;

            // Example usage 

            logger = LogManager.GetLogger("Jli");
        }

        private static void Logs(string s)
        {
            if (SetView.blog)
            {
                logger.Info(s);
            }
        }



        private void Bt_changeview_Click(object sender, EventArgs e)
        {
            try
            {

                //根据INI文件名设置要写入INI文件的节点名称
                //此处的节点名称完全可以根据实际需要进行配置
                strSec = Path.GetFileNameWithoutExtension(strFilePath);
                WritePrivateProfileString(strSec, "accesskey", this.txt_encodeaccesskey.Text, strFilePath);
                WritePrivateProfileString(strSec, "secretkey", this.txt_encodesecretkey.Text, strFilePath);

                //MessageBox.Show("写入成功");

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString());

            }

            setview();
        }

        private void setview()
        {
            if (File.Exists(strFilePath))//读取时先要判读INI文件是否存在
            {
                strSec = Path.GetFileNameWithoutExtension(strFilePath);
                this.txt_encodeaccesskey.Text = ContentValue(strSec, "accesskey");
                this.txt_encodesecretkey.Text = ContentValue(strSec, "secretkey");

                string sfind = ContentValue(strSec, "hostapi");
                int iselect = -1;
                bool bfind = false;
                foreach (var l in cb_apihost.Items)
                {
                    iselect++;
                    dynamic ss = l;
                    if (ss.value == sfind)
                    {
                        bfind = true;
                        break;
                    }
                }
                cb_apihost.SelectedIndex = bfind ? iselect : -1;
            }
            else
            {
                MessageBox.Show("API设置文件不存在！");
            }
            try
            {
                SetView.accesskey = AesSecret.Decrypt_AES(this.txt_encodeaccesskey.Text).ToString();
                SetView.secretkey = AesSecret.Decrypt_AES(this.txt_encodesecretkey.Text).ToString();
                SetView.hostapi = ContentValue(strSec, "hostapi");
            }
            catch
            {

            }
            SetView.vw_hangqin = cb_hangqin.Checked == true ? true : false;
            SetView.vw_debugger = cb_debugger.Checked ? true : false;
            SetView.vw_kline = cb_kline.Checked ? true : false;
            SetView.vw_introduction = cb_introduction.Checked ? true : false;
            SetView.blog = cb_log.Checked ? true : false;
            SetView.guanzhu = cb_stopguanzhu.Checked ? true : false;
            SetView.autochoose = cb_autochoose.Checked ? true : false;
            SetView.autofy = cb_fy.Checked ? true : false;
            if (SetView.autofy)
            {
                SetView.pagesize = int.Parse(this.txt_pagesize.Text.Trim());
            }
            if (SetView.autochoose)
            {
                timer_ChooseBz.Enabled = false;
            }
            else
            {
                timer_ChooseBz.Enabled = true;
            }

            if (SetView.vw_hangqin)
            {
                dgv_bzhq.Visible = true; //币种行情
                dgv_bzhq.DataSource = null;
            }
            else
            {
                dgv_bzhq.Visible = false;
                dgv_bzhq.DataSource = null;
            }
            if (SetView.vw_hangqin)
            {
                splitContainer1.Panel2Collapsed = false;
                //splitContainer1.SplitterDistance = origionWidth;


            }
            else
            {
                splitContainer1.Panel2Collapsed = true;
                //origionWidth = splitContainer1.Panel1.Width;
                //splitContainer1.SplitterDistance = splitContainer1.Width;

            }

        }

        private void Bt_Encode_Click(object sender, EventArgs e)
        {
            if (this.txt_accesskey.Text.Trim() == "") return;
            if (this.txt_secretkey.Text.Trim() == "") return;

            this.txt_encodeaccesskey.Text = AesSecret.Encrypt_AES(this.txt_accesskey.Text).ToString();
            this.txt_encodesecretkey.Text = AesSecret.Encrypt_AES(this.txt_secretkey.Text).ToString();


        }

        private void Cb_apihost_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {

                //根据INI文件名设置要写入INI文件的节点名称
                //此处的节点名称完全可以根据实际需要进行配置
                strSec = Path.GetFileNameWithoutExtension(strFilePath);
                WritePrivateProfileString(strSec, "hostapi", this.cb_apihost.SelectedValue.ToString(), strFilePath);


            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString());

            }
        }

        private void Bt_ph_Click(object sender, EventArgs e)
        {
            string srq1 = "", srq2 = "";
            if (this.dt_rq1.Text.Trim() != "")
            {
                srq1 = DateTime.Parse(this.dt_rq1.Text).ToString("yyyy-MM-dd");
            }

            srq2 = this.dt_rq2.Text.Trim() == "" ? "" : DateTime.Parse(this.dt_rq2.Text).ToString("yyyy-MM-dd");

            string sql = "";
            if (String.IsNullOrEmpty(this.txt_sbz.Text.ToString()))
            {
                if (srq1 != "" && srq2 != "")
                {
                    sql = $"select c.sbz as '币种',c.cjjrmb as '现价',c.mincjj as '最低价',c.maxcjj as '最高价',(c.bdl * 100) || '%' as '最大波动率%',c.bdl as '最大波动率',(round(((cjjrmb -mincjj)/mincjj),4)*100)|| '%'  as '现价涨幅%',round(((cjjrmb -mincjj)/mincjj),4)  as '现价涨幅',(round(((cjjrmb-maxcjj)/maxcjj),4)*100) || '%' as '现价跌幅%',round(((cjjrmb-maxcjj)/maxcjj),4)  as '现价跌幅',c.dt as '更新日期' from (select a.*,b.cjjrmb,b.dt from (select * from (select  sbz,min(CAST(cjjrmb as double)) as 'mincjj',max(CAST(cjjrmb as double)) as 'maxcjj',round(((max(CAST(cjjrmb as double)) -min(CAST(cjjrmb as double)))/min(CAST(cjjrmb as double))),4)  as 'bdl' from Jy where  date(dt)  >='{srq1}' and  date(dt)  <='{srq2}' group by sbz) as a order by bdl desc) a  left outer join (select sbz,CAST(cjjrmb as double) as 'cjjrmb',dt from (select * from Jy  order by dt desc)  group by sbz) b  on b.sbz = a.sbz )c";
                }
                else
                {
                    sql = "select c.sbz as '币种',c.cjjrmb as '现价',c.mincjj as '最低价',c.maxcjj as '最高价',(c.bdl * 100) || '%' as '最大波动率%',c.bdl as '最大波动率',(round(((cjjrmb -mincjj)/mincjj),4)*100)|| '%'  as '现价涨幅%',round(((cjjrmb -mincjj)/mincjj),4)  as '现价涨幅',(round(((cjjrmb-maxcjj)/maxcjj),4)*100) || '%' as '现价跌幅%',round(((cjjrmb-maxcjj)/maxcjj),4)  as '现价跌幅',c.dt as '更新日期' from (select a.*,b.cjjrmb,b.dt from (select * from (select  sbz,min(CAST(cjjrmb as double)) as 'mincjj',max(CAST(cjjrmb as double)) as 'maxcjj',round(((max(CAST(cjjrmb as double)) -min(CAST(cjjrmb as double)))/min(CAST(cjjrmb as double))),4)  as 'bdl' from Jy group by sbz) as a order by bdl desc) a  left outer join (select sbz,CAST(cjjrmb as double) as 'cjjrmb',dt from (select * from Jy  order by dt desc)  group by sbz) b  on b.sbz = a.sbz )c";
                }

            }
            else
            {
                if (srq1 != "" && srq2 != "")
                {
                    sql = $"select c.sbz as '币种',c.cjjrmb as '现价',c.mincjj as '最低价',c.maxcjj as '最高价',(c.bdl * 100) || '%' as '最大波动率%',c.bdl as '最大波动率',(round(((cjjrmb -mincjj)/mincjj),4)*100)|| '%'  as '现价涨幅%',round(((cjjrmb -mincjj)/mincjj),4)  as '现价涨幅',(round(((cjjrmb-maxcjj)/maxcjj),4)*100) || '%' as '现价跌幅%',round(((cjjrmb-maxcjj)/maxcjj),4)  as '现价跌幅',c.dt as '更新日期' from (select a.*,b.cjjrmb,b.dt from (select * from (select  sbz,min(CAST(cjjrmb as double)) as 'mincjj',max(CAST(cjjrmb as double)) as 'maxcjj',round(((max(CAST(cjjrmb as double)) -min(CAST(cjjrmb as double)))/min(CAST(cjjrmb as double))),4)  as 'bdl' from Jy where sbz = '{this.txt_sbz.Text.Trim()}' and  date(dt)  >='{srq1}' and  date(dt) <='{srq2}'  group by sbz) as a order by bdl desc) a  left outer join (select sbz,CAST(cjjrmb as double) as 'cjjrmb',dt from (select * from Jy  order by dt desc)  group by sbz) b  on b.sbz = a.sbz )c";
                }
                else
                {
                    sql = $"select c.sbz as '币种',c.cjjrmb as '现价',c.mincjj as '最低价',c.maxcjj as '最高价',(c.bdl * 100) || '%' as '最大波动率%',c.bdl as '最大波动率',(round(((cjjrmb -mincjj)/mincjj),4)*100)|| '%'  as '现价涨幅%',round(((cjjrmb -mincjj)/mincjj),4)  as '现价涨幅',(round(((cjjrmb-maxcjj)/maxcjj),4)*100) || '%' as '现价跌幅%',round(((cjjrmb-maxcjj)/maxcjj),4)  as '现价跌幅',c.dt as '更新日期' from (select a.*,b.cjjrmb,b.dt from (select * from (select  sbz,min(CAST(cjjrmb as double)) as 'mincjj',max(CAST(cjjrmb as double)) as 'maxcjj',round(((max(CAST(cjjrmb as double)) -min(CAST(cjjrmb as double)))/min(CAST(cjjrmb as double))),4)  as 'bdl' from Jy where sbz = '{this.txt_sbz.Text.Trim()}' group by sbz) as a order by bdl desc) a  left outer join (select sbz,CAST(cjjrmb as double) as 'cjjrmb',dt from (select * from Jy  order by dt desc)  group by sbz) b  on b.sbz = a.sbz )c";
                }

            }

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

            DataSet ds = SQLiteHelper.ExecuteDataSet(sconn, command);

            this.dg_ph.DataSource = ds.Tables[0];

        }

        private void Button9_Click(object sender, EventArgs e)
        {

            int icount = 7;
            DateTime dt = DateTime.Now;
            List<string> scjj;
            string sbz = "";

            string sqdt = DateTime.Now.ToShortDateString(); //今天



            try
            {
                Task.Run(() =>
                {
                    string sql = $"select sbz  from Jy  group by sbz";
                    SQLiteCommand commandBz = new SQLiteCommand(sql, m_dbConnection);

                    SQLiteDataReader readerbz = SQLiteHelper.ExecuteReader(sconn, commandBz) as SQLiteDataReader;
                    if (readerbz.HasRows)
                    {

                        while (readerbz.Read())
                        {

                            #region 策略选币
                            sbz = readerbz["sbz"].ToString();

                            sql = $"select  * from Jy where sbz='{sbz}'  and rq='{sqdt}' order by dt desc limit 0,1";
                            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

                            SQLiteDataReader reader = SQLiteHelper.ExecuteReader(sconn, command) as SQLiteDataReader;


                            if (reader.HasRows)
                            {
                                var dt1 = DateTime.Now;
                                while (reader.Read())
                                {
                                    dt = DateTime.Parse(reader["dt"].ToString());
                                }



                                //TimeSpan span = dt1 - dt;

                                //if (span.TotalMinutes > 20)
                                //{
                                //    //logger.Warn($"[{sbz}]{(int)Math.Floor(span.TotalMinutes)}没有写进数据了，请检查！");
                                //    continue;
                                //}
                                //logger.Info($"策略选币开始运行");
                                sql = $"select sbz, group_concat(cjjrmb) as 'cjj' from (select  * from Jy where sbz ='{sbz}' and rq ='{sqdt}' order by dt desc) group  by sbz";
                                command = new SQLiteCommand(sql, m_dbConnection);

                                DataSet reader1 = SQLiteHelper.ExecuteDataSet(sconn, command);


                                if (reader1.Tables[0].Rows.Count == 1)
                                {
                                    scjj = reader1.Tables[0].Rows[0]["cjj"].ToString().Split(new char[] { ',' }).ToList<string>();
                                    if (scjj.Count != 0)
                                    {
                                        scjj.Reverse();
                                        double d1 = 0d;
                                        bool bok = false;
                                        int inum = 0;
                                        int ierrnum = 0;
                                        foreach (string s in scjj)
                                        {
                                            if (d1 == 0d)
                                            {
                                                d1 = double.Parse(s);
                                            }
                                            else
                                            {
                                                if (double.Parse(s) > d1)
                                                {
                                                    ierrnum = 0;
                                                    inum++;
                                                    d1 = double.Parse(s);

                                                    if (inum >= 4)
                                                    {
                                                        bok = true;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    ierrnum++;
                                                    bok = false;
                                                    if (ierrnum >= 2)
                                                    {
                                                        inum = 0;
                                                        //d1 = double.Parse(s);
                                                    }
                                                }
                                            }
                                        }
                                        if (bok == true)
                                        {
                                            // MessageBox.Show($"{sbz}被策略选中，建议下单买入!");
                                            logger.Warn($"{sbz}被策略选中!,买价：{d1}");
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        //logger.Info($"策略选币运行结束");
                    }

                });



            }
            finally
            {
                //spinLockAi.Exit();
            }
        }



        public static void Delay(int mm)
        {
            DateTime current = DateTime.Now;
            while (current.AddMilliseconds(mm) > DateTime.Now)
            {
                Application.DoEvents();
            }
            return;
        }

        private void Timer_ChooseBz_Tick(object sender, EventArgs e)
        {
            if (SetView.autochoose) return;
            spinLockAi.Enter();
            int icount = 7;
            DateTime dt = DateTime.Now;
            List<string> scjj;
            string sbz = "";

            string sqdt = DateTime.Now.ToShortDateString(); //今天



            try
            {
                Task.Run(() =>
                {
                    string sql = $"select sbz  from Jy  group by sbz";
                    SQLiteCommand commandBz = new SQLiteCommand(sql, m_dbConnection);

                    SQLiteDataReader readerbz = SQLiteHelper.ExecuteReader(sconn, commandBz) as SQLiteDataReader;
                    if (readerbz.HasRows)
                    {

                        while (readerbz.Read())
                        {

                            #region 策略选币
                            sbz = readerbz["sbz"].ToString();

                            sql = $"select  * from Jy where sbz='{sbz}'  and rq='{sqdt}' order by dt desc limit 0,1";
                            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

                            SQLiteDataReader reader = SQLiteHelper.ExecuteReader(sconn, command) as SQLiteDataReader;


                            if (reader.HasRows)
                            {
                                var dt1 = DateTime.Now;
                                while (reader.Read())
                                {
                                    dt = DateTime.Parse(reader["dt"].ToString());
                                }


                                //Application.DoEvents();
                                TimeSpan span = dt1 - dt;

                                if (span.TotalMinutes > 20)
                                {
                                    //logger.Warn($"[{sbz}]{(int)Math.Floor(span.TotalMinutes)}没有写进数据了，请检查！");
                                    continue;
                                }
                                //logger.Info($"策略选币开始运行");
                                sql = $"select sbz, group_concat(cjjrmb) as 'cjj' from (select  * from Jy where sbz ='{sbz}' order by dt desc limit 0,{icount}) group  by sbz";
                                command = new SQLiteCommand(sql, m_dbConnection);

                                DataSet reader1 = SQLiteHelper.ExecuteDataSet(sconn, command);


                                if (reader1.Tables[0].Rows.Count == 1)
                                {
                                    scjj = reader1.Tables[0].Rows[0]["cjj"].ToString().Split(new char[] { ',' }).ToList<string>();
                                    if (scjj.Count == icount)
                                    {
                                        scjj.Reverse();
                                        double d1 = 0d;
                                        bool bok = false;
                                        foreach (string s in scjj)
                                        {
                                            if (d1 == 0d)
                                            {
                                                d1 = double.Parse(s);
                                            }
                                            else
                                            {
                                                if (double.Parse(s) > d1)
                                                {
                                                    d1 = double.Parse(s);
                                                    bok = true;
                                                }
                                                else
                                                {
                                                    bok = false;
                                                    break;
                                                }
                                            }
                                        }
                                        if (bok == true)
                                        {
                                            // MessageBox.Show($"{sbz}被策略选中，建议下单买入!");
                                            logger.Warn($"{sbz}被策略选中!");

                                            if (this.rtb_msg.IsHandleCreated)
                                            {
                                                Action action = () =>
                                                {
                                                    this.rtb_msg.AppendTextColorful($"{sbz}被策略选中!", Color.Green);
                                                };

                                                this.BeginInvoke(action);
                                            }
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        //logger.Info($"策略选币运行结束");
                    }

                });



            }
            finally
            {
                spinLockAi.Exit();
            }

        }

        private void Dgv_gz_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewColumn column = (sender as DataGridView).Columns[e.ColumnIndex];

                if (column is DataGridViewCheckBoxColumn)
                {
                    //判断点的是否是datagridviewcheckbox列并且不是列头
                    if (e.ColumnIndex == 0 && e.RowIndex != -1)
                    {
                        string ss = (sender as DataGridView).Rows[e.RowIndex].Cells["Sbz1"].Value.ToString();
                        bool b = false;
                        b = (bool)((sender as DataGridView).Rows[e.RowIndex].Cells[0].Value ?? false);
                        lock (SequenceLock)
                        {
                            if (b)
                            {
                                //if (Lcbbz.Contains(ss))
                                //{
                                //    Lcbbz.Remove(ss);
                                //}
                                (sender as DataGridView).Rows[e.RowIndex].Cells[0].Value = !b;
                            }
                            else
                            {
                                if (Lcbbz.Contains(ss))
                                {
                                    Lcbbz.Remove(ss);
                                }
                                (sender as DataGridView).Rows[e.RowIndex].Cells[0].Value = !b;
                            }

                        }


                    }
                }
                else if (column is DataGridViewLinkColumn)
                {
                    JyEventArgs jy = null;
                    string ss = (sender as DataGridView).Rows[e.RowIndex].Cells["Sbz1"].Value.ToString();
                    if (column.Name == "关注买")
                    {
                        Mm(ss);
                        jy = new JyEventArgs(new JyFx
                        {
                            Sfx = "买",
                            Sbz = (sender as DataGridView).Rows[e.RowIndex].Cells["Sbz1"].Value.ToString(),
                            Cjj = (sender as DataGridView).Rows[e.RowIndex].Cells["Cjj1"].Value.ToString(),
                            CjjRmb = (sender as DataGridView).Rows[e.RowIndex].Cells["CjjRmb1"].Value.ToString(),
                            OwnerYe = (sender as DataGridView).Rows[e.RowIndex].Cells["OwnerYe1"].Value.ToString(),
                            OwnerRmb = double.Parse((sender as DataGridView).Rows[e.RowIndex].Cells["OwnerRmb1"].Value.ToString()),//帐户余额
                            Zdf = double.Parse((sender as DataGridView).Rows[e.RowIndex].Cells["Zdf1"].Value.ToString())
                        });
                        string sname = $"{ss}买卖";
                        if (hstable.Contains(sname))
                        {
                            EventHandler eh = hstable[sname] as EventHandler;
                            eh(this, jy);
                        }

                    }
                    else if (column.Name == "关注卖")
                    {
                        Mm(ss);
                        jy = new JyEventArgs(new JyFx
                        {
                            Sfx = "卖",
                            Sbz = (sender as DataGridView).Rows[e.RowIndex].Cells["Sbz1"].Value.ToString(),
                            Cjj = (sender as DataGridView).Rows[e.RowIndex].Cells["Cjj1"].Value.ToString(),
                            CjjRmb = (sender as DataGridView).Rows[e.RowIndex].Cells["CjjRmb1"].Value.ToString(),
                            OwnerYe = (sender as DataGridView).Rows[e.RowIndex].Cells["OwnerYe1"].Value.ToString(),
                            OwnerRmb = double.Parse((sender as DataGridView).Rows[e.RowIndex].Cells["OwnerRmb1"].Value.ToString()),//帐户余额
                            Zdf = double.Parse((sender as DataGridView).Rows[e.RowIndex].Cells["Zdf1"].Value.ToString())

                        });
                        //SendMsgEvent(this, jy);
                        string sname = $"{ss}买卖";
                        if (hstable.Contains(sname))
                        {
                            EventHandler eh = hstable[sname] as EventHandler;
                            eh(this, jy);
                        }
                    }


                }
                else
                {
                }
            }
        }

        private void Dgv_jy_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush b = new SolidBrush(this.dgv_jy.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(System.Globalization.CultureInfo.CurrentUICulture), this.dgv_jy.DefaultCellStyle.Font, b, e.RowBounds.Location.X + 13, e.RowBounds.Location.Y + 4);
        }

        /// <summary>
        /// 将集合类转换成DataTable
        /// </summary>
        /// <param name="list">集合</param>
        /// <returns></returns>
        public static DataTable ToDataTable(IList list)
        {
            DataTable result = new DataTable();
            if (list.Count > 0)
            {
                PropertyInfo[] propertys = list[0].GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    result.Columns.Add(pi.Name, pi.PropertyType);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in propertys)
                    {
                        object obj = pi.GetValue(list[i], null);
                        tempList.Add(obj);
                    }
                    object[] array = tempList.ToArray();
                    result.LoadDataRow(array, true);
                }
            }
            return result;
        }

        /// <summary>
        /// 将泛型集合类转换成DataTable
        /// </summary>
        /// <typeparam name="T">集合项类型</typeparam>
        /// <param name="list">集合</param>
        /// <returns>数据集(表)</returns>
        public static DataTable ToDataTable<T>(IList<T> list)
        {
            return ToDataTable<T>(list, null);
        }

        /// <summary>
        /// 将泛型集合类转换成DataTable
        /// </summary>
        /// <typeparam name="T">集合项类型</typeparam>
        /// <param name="list">集合</param>
        /// <param name="propertyName">需要返回的列的列名</param>
        /// <returns>数据集(表)</returns>
        public static DataTable ToDataTable<T>(IList<T> list, params string[] propertyName)
        {
            List<string> propertyNameList = new List<string>();
            if (propertyName != null)
                propertyNameList.AddRange(propertyName);

            DataTable result = new DataTable();
            if (list.Count > 0)
            {
                PropertyInfo[] propertys = list[0].GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    if (propertyNameList.Count == 0)
                    {
                        result.Columns.Add(pi.Name, pi.PropertyType);
                    }
                    else
                    {
                        if (propertyNameList.Contains(pi.Name))
                            result.Columns.Add(pi.Name, pi.PropertyType);
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in propertys)
                    {
                        if (propertyNameList.Count == 0)
                        {
                            object obj = pi.GetValue(list[i], null);
                            tempList.Add(obj);
                        }
                        else
                        {
                            if (propertyNameList.Contains(pi.Name))
                            {
                                object obj = pi.GetValue(list[i], null);
                                tempList.Add(obj);
                            }
                        }
                    }
                    object[] array = tempList.ToArray();
                    result.LoadDataRow(array, true);
                }
            }
            return result;
        }

        private void PagerControl1_OnPageChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        void LoadData()
        {
            try
            {
                DataTable dtTemp = dtjy.Clone();
                DataTable dtInfo = dtjy.Copy();

                int pageindex = pagerControl1.PageIndex;
                int pagesize = pagerControl1.PageSize;
                int count = 0;

                int istart = 0;


                for (int i = (pageindex - 1) * pagesize; i < pageindex * pagesize; i++)
                {
                    if (i >= dtInfo.Rows.Count)
                    {
                        break;
                    }
                    dtTemp.ImportRow(dtInfo.Rows[i]);
                }
                //dg_test.AutoGenerateColumns = true;
                dgv_jy.DataSource = dtTemp;
                this.dgv_jy.Restore();
                pagerControl1.DrawControl(dtInfo.Rows.Count);

            }
            catch
            {

            }




        }

        private void Bt_sellbtc_Click(object sender, EventArgs e)
        {
            string sbz = "btc";
            Mm(sbz);
            JyEventArgs jy = new JyEventArgs(new JyFx
            {
                Sfx = "卖",
                Sbz = "btc"
            });
            // SendMsgEvent(this, jy);

            string sname = $"{sbz}买卖";
            if (hstable.Contains(sname))
            {
                EventHandler eh = hstable[sname] as EventHandler;
                eh(this, jy);
            }
        }

        private void Bt_buybtc_Click(object sender, EventArgs e)
        {
            string sbz = "btc";
            Mm(sbz);
            JyEventArgs jy = new JyEventArgs(new JyFx
            {
                Sfx = "买",
                Sbz = "btc"
            });
            // SendMsgEvent(this, jy);

            string sname = $"{sbz}买卖";
            if (hstable.Contains(sname))
            {
                EventHandler eh = hstable[sname] as EventHandler;
                eh(this, jy);
            }
        }

        private async void Dgv_sell_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewColumn column = (sender as DataGridView).Columns[e.ColumnIndex];

                if (column is DataGridViewLinkColumn)
                {

                    string ss = (sender as DataGridView).Rows[e.RowIndex].Cells["id"].Value.ToString();
                    if (column.Name == "取消")
                    {
                        var cresult = await api.OrderPlaceCancel(ss);
                        ////取消订单
                        ////Assert.AreEqual(cresult.Status, "ok");
                        if (cresult.Status.ToString() == "ok")
                        {

                        }
                        else
                        {
                            MessageBox.Show("取消订单出错!", "错误");
                        }
                    }
                }
                else
                {
                }
            }
        }

        private async void Dgv_buy_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewColumn column = (sender as DataGridView).Columns[e.ColumnIndex];

                if (column is DataGridViewLinkColumn)
                {

                    string ss = (sender as DataGridView).Rows[e.RowIndex].Cells["id"].Value.ToString();
                    if (column.Name == "取消")
                    {
                        var cresult = await api.OrderPlaceCancel(ss);
                        ////取消订单
                        ////Assert.AreEqual(cresult.Status, "ok");
                        if (cresult.Status.ToString() == "ok")
                        {

                        }
                        else
                        {
                            MessageBox.Show("取消订单出错!", "错误");
                        }
                    }
                }
                else
                {
                }
            }
        }
    }
}
