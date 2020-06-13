using Huobi.Rest.CSharp.Demo;
using Huobi.Rest.CSharp.Demo.Model;
using Huobi.Rest.CSharp.Demo.Model.New;
using Jli.Common;
using Jli.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Jli
{
    public partial class FormMm : DockContent
    {

        string sbz = "";
        string sfx = "";
        bool bbtc = true;
        bool busdt = false;
        //System.Timers.Timer thq = new System.Timers.Timer(3000);

        Thread tmain;
        Core.SpinLock spinLockm = new Core.SpinLock(500);
        static Core.SpinLock spinLock = new Core.SpinLock(500);
        bool _bstate = false;
        bool bstate
        {
            get
            {
                return _bstate;
            }
            set
            {
                _bstate = value;
            }
        }



        double dbtc; //可用比特币;
        double dusdt;//可用usdt;
        double dsbz;//可卖币种余额;
        double dsbzlock;//可卖币种锁定数量
        decimal dxsamount;//可卖数量的小数位数
        public FormMm()
        {
            InitializeComponent();
        }

        public void InitControl()
        {
            if (sbz == "btc")
            {
                this.cb_usdt.Checked = true;
                this.cb_usdt.Enabled = true;
                this.cb_usdt.Visible = true;
                this.cb_btc.Checked = false;
                this.cb_btc.Enabled = false;
                this.cb_btc.Visible = false;
                bbtc = false;
                busdt = true;
            }
            else
            {
                var s = FormHb.hcs.Data.Where(a => a.QuoteCurrency == "usdt" && a.BaseCurrency == sbz).Select(a => new { a.PricePrecision, a.AmountPrecision }).ToList();
                if (s.Count != 1)
                {
                    this.cb_usdt.Checked = false;
                    this.cb_usdt.Enabled = false;
                    this.cb_usdt.Visible = false;
                    bbtc = true;
                    busdt = false;
                }
                else
                {
                    bbtc = true;
                    busdt = false;
                    this.cb_btc.Checked = true;
                    this.cb_usdt.Checked = false;
                    this.cb_usdt.Enabled = true;
                    this.cb_usdt.Visible = true;
                }
            }
        }

        private void SetBz(string sbz)
        {
            if (this.l_bz.InvokeRequired)
            {
                this.Invoke(new Action(() => { this.l_bz.Text = sbz; }));
            }
            else
            {
                this.l_bz.Text = sbz;
            }
        }

        private void FormMm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.Owner is FormHb)
            {
                (this.Owner as FormHb)._Remove(this);
            }
        }

        internal void MainFormMsgChanged(object sender, EventArgs e)
        {


            //取到主窗体的传来的文本
            JyEventArgs arg = e as JyEventArgs;
            if (arg.Message.Sfx == "买")
            {
                this.txtBuyJg.Text = arg.Message.Cjj ?? "";
                this.txtBuyXl.Text = "";
                this.txtSellJg.Text = "";
                this.txtSellXl.Text = "";
                this.bt_sell.Enabled = false;

            }
            else if (arg.Message.Sfx == "卖")
            {
                this.txtBuyJg.Text = "";
                this.txtBuyXl.Text = "";
                this.txtSellJg.Text = arg.Message.Cjj ?? "";
                this.txtSellXl.Text = "";
                this.bt_buy.Enabled = false;
            }
            sbz = arg.Message.Sbz;
            sfx = arg.Message.Sfx;

            InitControl();

            if (sbz == "btc")
            {
                SetBz(sbz + @"/usdc");
            }
            else
            {
                if (bbtc)
                {
                    SetBz(sbz + @"/btc");
                }
                else
                {
                    SetBz(sbz + @"/usdt");
                }
            }

            if (sfx == "买")
            {
                this.bt_buy.BackColor = Color.PaleGreen;
                this.bt_sell.BackColor = Color.LightGray;
            }
            else if (sfx == "卖")
            {
                this.bt_sell.BackColor = Color.PeachPuff;
                this.bt_buy.BackColor = Color.LightGray;
            }

            this.rtb_msg.ClearData(true);

            if (tmain == null)
            {
                tmain = new Thread(new ThreadStart(
                             new Action(async () =>
                             {
                                 while (true)
                                 {
                                     bstate = true;
                                     //this.Invoke(new Action(() => {
                                     //    dgv_mmsell.DataSource = null;
                                     //    dgv_mmbuy.DataSource = null;
                                     //}));
                                     try
                                     {
                                         if (sbz == "") continue;

                                         //spinLockm.Enter();

                                         DetailOtc jg = await FormHb.api.GetOtcRmb();
                                         var btjg = bbtc ? await FormHb.api.GetTradeDetail("btcusdt", jg) : jg.Buy;// 当前比特币成交价
                                         if (!bstate) continue;
                                         while (btjg == 0)
                                         {
                                             Console.WriteLine("[买卖提醒]获取比特币价格失败！正在重试！");
                                             btjg = bbtc ? await FormHb.api.GetTradeDetail("btcusdt", jg) : jg.Buy;
                                         }

                                         string sfind = $"{sbz}";
                                         sfind = bbtc ? sfind + "btc" : sfind + "usdt";
                                         if (!bstate) continue;
                                         var resultbz = await FormHb.api.GetTradeDetailSbz(sfind, double.Parse(btjg.ToString()));// 当前币种成交价
                                         if (resultbz != null)
                                         {
                                             foreach (Datum d in resultbz)
                                             {

                                                 if (this.rtb_msg.IsHandleCreated)
                                                 {
                                                     if (!bstate)
                                                     {
                                                         continue;
                                                     }
                                                     Action action = () =>
                                                     {
                                                         if (d.Direction == "buy")
                                                         {
                                                             this.rtb_msg.AppendTextColorful(d.Id, $"成交时间：{d.Ts.ToString("T")} 成交单价：{d.Price.ToString("0.###############")},成交单价/Rmb：{(d.Price * btjg).ToString("f6")};成交数量：{d.Amount};金额：{(d.Price * btjg * d.Amount).ToString("f4")}", Color.Green);
                                                         }
                                                         else
                                                         {
                                                             this.rtb_msg.AppendTextColorful(d.Id, $"成交时间：{d.Ts.ToString("T")} 成交单价：{d.Price.ToString("0.###############")},成交单价/Rmb：{(d.Price * btjg).ToString("f6")};成交数量：{d.Amount};金额：{(d.Price * btjg * d.Amount).ToString("f4")}", Color.Red);
                                                         }

                                                     };

                                                     this.BeginInvoke(action);
                                                 }
                                             }
                                         }

                                         //MarketDepth md;
                                         var result = bbtc ? await FormHb.api.GetMarketDepthForm(sbz, "step0") : await FormHb.api.GetMarketDepthForm(sbz, "step0", "usdt");

                                         if (!bstate) continue;

                                         List<DetailJy> lbids = new List<DetailJy>();//买盘
                                         List<DetailJy> lasks = new List<DetailJy>();//卖盘
                                         if (result.Status == "ok")
                                         {
                                             foreach (var d in result.Tick.Asks)
                                             {
                                                 var dj = new DetailJy
                                                 {
                                                     Price = d[0],
                                                     Amount = d[1],
                                                     Jg = Math.Round(d[0] * d[1] * btjg, 2),
                                                     Dj = Math.Round(d[0] * btjg, 6)
                                                 };
                                                 lasks.Add(dj);
                                             }
                                             foreach (var d in result.Tick.Bids)
                                             {
                                                 var dj = new DetailJy
                                                 {
                                                     Price = d[0],
                                                     Amount = d[1],
                                                     Jg = Math.Round(d[0] * d[1] * btjg, 2),
                                                     Dj = Math.Round(d[0] * btjg, 6)
                                                 };
                                                 lbids.Add(dj);
                                             }
                                             try
                                             {
                                                 if (this.dgv_mmbuy.IsHandleCreated && this.dgv_mmsell.IsHandleCreated)
                                                 {
                                                     if (this != null && !this.IsDisposed)
                                                     {
                                                         if (!bstate) continue;
                                                         Invoke(new Action(() =>
                                                         {
                                                             Application.DoEvents();
                                                             this.dgv_mmsell.AutoGenerateColumns = true;
                                                             this.dgv_mmbuy.AutoGenerateColumns = true;

                                                             this.dgv_mmsell.DataSource = new BindingList<DetailJy>(lasks.OrderByDescending(a => a.Price).ToList());


                                                             this.dgv_mmbuy.DataSource = new BindingList<DetailJy>(lbids.OrderByDescending(a => a.Price).ToList());


                                                             this.dgv_mmsell.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                                                             this.dgv_mmsell.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                                                             this.dgv_mmsell.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                                                             this.dgv_mmsell.Columns[0].HeaderCell.Value = "价格";
                                                             this.dgv_mmsell.Columns[1].HeaderCell.Value = "数量";
                                                             this.dgv_mmsell.Columns[2].HeaderCell.Value = "总价(元）";
                                                             this.dgv_mmsell.Columns[3].HeaderCell.Value = "单价(元）";

                                                             this.dgv_mmbuy.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                                                             this.dgv_mmbuy.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                                                             this.dgv_mmbuy.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                                                             this.dgv_mmbuy.Columns[0].HeaderCell.Value = "价格";
                                                             this.dgv_mmbuy.Columns[1].HeaderCell.Value = "数量";
                                                             this.dgv_mmbuy.Columns[2].HeaderCell.Value = "总价(元）";
                                                             this.dgv_mmbuy.Columns[3].HeaderCell.Value = "单价(元）";


                                                             bool bflag = this.dgv_mmsell.bflag;
                                                             if (!bflag)
                                                             {
                                                                 this.dgv_mmsell.Scroll -= new System.Windows.Forms.ScrollEventHandler(this.Dgv_mmsell_Scroll);
                                                                 this.dgv_mmsell.Click -= new System.EventHandler(this.Dgv_mmsell_Click);



                                                                 try
                                                                 {
                                                                     this.dgv_mmsell.FirstDisplayedScrollingRowIndex = dgv_mmsell.Rows[dgv_mmsell.Rows.Count - 1].Index; //滚动条位置
                                                                     dgv_mmsell.Rows[dgv_mmsell.Rows.Count - 1].Selected = true;
                                                                 }
                                                                 catch
                                                                 {

                                                                 }
                                                                 finally
                                                                 {
                                                                     this.dgv_mmsell.Scroll += new System.Windows.Forms.ScrollEventHandler(this.Dgv_mmsell_Scroll);
                                                                     this.dgv_mmsell.Click += new System.EventHandler(this.Dgv_mmsell_Click);
                                                                 }
                                                             }
                                                             else
                                                             {
                                                                 this.dgv_mmsell.Restore();
                                                             }

                                                             bflag = dgv_mmbuy.bflag;
                                                             if (!bflag)
                                                             {
                                                                 this.dgv_mmbuy.Scroll -= new System.Windows.Forms.ScrollEventHandler(this.Dgv_mmbuy_Scroll);
                                                                 this.dgv_mmbuy.Click -= new System.EventHandler(this.Dgv_mmbuy_Click);


                                                                 try
                                                                 {
                                                                     this.dgv_mmbuy.FirstDisplayedScrollingRowIndex = dgv_mmbuy.Rows[0].Index;
                                                                     dgv_mmbuy.Rows[0].Selected = true;

                                                                 }
                                                                 catch
                                                                 {

                                                                 }
                                                                 finally
                                                                 {
                                                                     this.dgv_mmbuy.Scroll += new System.Windows.Forms.ScrollEventHandler(this.Dgv_mmbuy_Scroll);
                                                                     this.dgv_mmbuy.Click += new System.EventHandler(this.Dgv_mmbuy_Click);
                                                                 }
                                                             }
                                                             else
                                                             {
                                                                 this.dgv_mmbuy.Restore();
                                                             }

                                                         }));
                                                     }

                                                 }
                                             }
                                             catch
                                             {

                                             }
                                         }
                                         else
                                         {
                                             lbids.Clear();
                                             lasks.Clear();

                                         }
                                     }
                                     finally
                                     {

                                         //spinLockm.Exit();
                                     }
                                 }
                             }
                             )));
                tmain.IsBackground = true;
                tmain.Start();
            }
            else
            {
                //MessageBox.Show("事件已存在"); 
            }

        }

        private void Dgv_mmsell_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (dgv_mmsell.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "价格")
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
                        var tmpb = bbtc ? FormHb.hcs.Data.Where(a => a.QuoteCurrency == "btc" && a.BaseCurrency == sbz).Select(a => new { a.PricePrecision, a.AmountPrecision }).ToList() : FormHb.hcs.Data.Where(a => a.QuoteCurrency == "usdt" && a.BaseCurrency == sbz).Select(a => new { a.PricePrecision, a.AmountPrecision }).ToList();
                        //Console.WriteLine($"BTC可交易币种总数为：{tmpb.Count}个");

                        string sformat = $"f{tmpb[0].PricePrecision}";
                        string sreturn = d.ToString(sformat);
                        e.Value = sreturn;

                        return;
                    }
                }
                else if (dgv_mmsell.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "数量")
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
                        var tmpb = bbtc ? FormHb.hcs.Data.Where(a => a.QuoteCurrency == "btc" && a.BaseCurrency == sbz).Select(a => new { a.PricePrecision, a.AmountPrecision }).ToList() : FormHb.hcs.Data.Where(a => a.QuoteCurrency == "usdt" && a.BaseCurrency == sbz).Select(a => new { a.PricePrecision, a.AmountPrecision }).ToList();
                        //Console.WriteLine($"BTC可交易币种总数为：{tmpb.Count}个");

                        string sformat = $"f{tmpb[0].AmountPrecision}";
                        string sreturn = d.ToString(sformat);
                        e.Value = sreturn;

                        return;
                    }
                }
            }
            catch
            {

            }


        }

        private void Dgv_mmbuy_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgv_mmbuy.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "价格")
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
                    var tmpb = bbtc ? FormHb.hcs.Data.Where(a => a.QuoteCurrency == "btc" && a.BaseCurrency == sbz).Select(a => new { a.PricePrecision, a.AmountPrecision }).ToList() : FormHb.hcs.Data.Where(a => a.QuoteCurrency == "usdt" && a.BaseCurrency == sbz).Select(a => new { a.PricePrecision, a.AmountPrecision }).ToList();
                    //Console.WriteLine($"BTC可交易币种总数为：{tmpb.Count}个");

                    string sformat = $"f{tmpb[0].PricePrecision}";
                    string sreturn = d.ToString(sformat);
                    e.Value = sreturn;

                    return;
                }
            }
            else if (dgv_mmbuy.Columns[e.ColumnIndex].HeaderCell.Value.ToString() == "数量")
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
                    var tmpb = bbtc ? FormHb.hcs.Data.Where(a => a.QuoteCurrency == "btc" && a.BaseCurrency == sbz).Select(a => new { a.PricePrecision, a.AmountPrecision }).ToList() : FormHb.hcs.Data.Where(a => a.QuoteCurrency == "usdt" && a.BaseCurrency == sbz).Select(a => new { a.PricePrecision, a.AmountPrecision }).ToList();
                    //Console.WriteLine($"BTC可交易币种总数为：{tmpb.Count}个");

                    string sformat = $"f{tmpb[0].AmountPrecision}";
                    string sreturn = d.ToString(sformat);
                    e.Value = sreturn;

                    return;
                }
            }
        }

        private void Bt_sell_Click(object sender, EventArgs e)
        {
            OrderPlaceRequest req = new OrderPlaceRequest();
            req.account_id = FormHb.suserid.ToString();
            req.amount = txtSellXl.Text.Trim();
            req.price = txtSellJg.Text.Trim();
            req.source = "api";
            if (sbz == "btc")
            {
                req.symbol = "btcusdt";
            }
            else
            {
                req.symbol = bbtc ? $"{sbz}btc" : $"{sbz}usdt";
            }

            req.type = "sell-limit";
            var result = FormHb.api.OrderPlace(req);
            if (result.Status == "ok")
            {
                MessageBox.Show($"{result.Data},下单成功");
            }
            else
            {
                if (result.Status.ToString().Contains("order amount precision error"))
                {
                    string str = result.Status.ToString();
                    str = Regex.Replace(str, @"[^\d.\d]", "");
                    // 如果是数字，则转换为decimal类型
                    if (Regex.IsMatch(str, @"^[+-]?\d*[.]?\d*$"))
                    {
                        dxsamount = decimal.Parse(str);

                    }

                    MessageBox.Show($"错误：卖出量只能保留{dxsamount}位小数");
                    return;
                }
                MessageBox.Show($"错误：{result.Status.ToString()}");
            }
        }

        private async void FormMm_Load(object sender, EventArgs e)
        {
            ArrayList lists = new ArrayList();

            lists.Add(new
            {
                name = "限价交易",
                value = "0"
            });
            lists.Add(new
            {
                name = "市场价",
                value = "1"
            });


            this.cb_jylx.DisplayMember = "name";

            this.cb_jylx.ValueMember = "value";

            this.cb_jylx.DataSource = lists;




            ////到达时间的时候执行注册事件
            //thq.Elapsed += async (obj, ee) =>
            //{


            //};

            ////设置是执行一次（false）还是一直执行(true)； 
            //thq.AutoReset = true;

            ////是否执行System.Timers.Timer.Elapsed注册事件；   
            //thq.Enabled = true;
            ////Console.ReadKey();
        }

        private void FormMm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //thq.AutoReset = false;

            ////是否执行System.Timers.Timer.Elapsed注册事件；   
            //thq.Enabled = false;
        }

        private async void Dgv_mmsell_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            this.txtSellJg.Text = "";
            string price = double.Parse(dgv_mmsell.Rows[e.RowIndex].Cells["Price"].Value.ToString()).ToString("0.###############");
            if (sfx == "买")
            {
                this.txtBuyJg.Text = price;
            }
            else if (sfx == "卖")
            {
                this.txtSellJg.Text = price;
            }

            if (sbz == "btc")
            {
                await getbalance();
            }
            else
            {
                await getbalance();
                await getbalance1();
            }
        }


        private void Cb_usdt_CheckedChanged(object sender, EventArgs e)
        {
            //spinLockm.Enter();
            try
            {

                if (cb_usdt.Checked)
                {
                    try
                    {
                        bbtc = false;
                        busdt = true;

                    }
                    finally
                    {

                    }

                }
                else
                {
                    try
                    {
                        bbtc = true;
                        busdt = false;
                    }
                    finally
                    {

                    }

                }
                txtSellJg.Text = "";
                txtBuyJg.Text = "";
                this.rtb_msg.ClearData(true);
                if (bbtc)
                {
                    SetBz(sbz + @"/btc");
                }
                else
                {
                    SetBz(sbz + @"/usdt");
                }
                bstate = false;
            }
            finally
            {
                //spinLockm.Exit();
            }

        }

        private void Cb_btc_CheckedChanged(object sender, EventArgs e)
        {
            //spinLockm.Enter();
            //try
            //{


            if (cb_btc.Checked)
            {
                try
                {
                    bbtc = true;
                    busdt = false;
                }
                finally
                {

                }

            }
            else
            {
                try
                {
                    bbtc = false;
                    busdt = true;
                }
                finally
                {

                }

            }
            txtSellJg.Text = "";
            txtBuyJg.Text = "";
            this.rtb_msg.ClearData(true);

            if (bbtc)
            {
                SetBz(sbz + @"/btc");
            }
            else
            {
                SetBz(sbz + @"/usdt");
            }

            bstate = false;
            //}
            //finally
            //{
            //    //spinLockm.Exit();
            //}
        }

        private void Bt_buy_Click(object sender, EventArgs e)
        {
            OrderPlaceRequest req = new OrderPlaceRequest();
            req.account_id = FormHb.suserid.ToString();
            req.amount = txtBuyXl.Text.Trim();
            req.price = txtBuyJg.Text.Trim();
            req.source = "api";
            if (sbz == "btc")
            {
                req.symbol = "btcusdt";
            }
            else
            {
                req.symbol = bbtc ? $"{sbz}btc" : $"{sbz}usdt";
            }

            req.type = "buy-limit";
            var result = FormHb.api.OrderPlace(req);
            if (result.Status == "ok")
            {
                MessageBox.Show($"{result.Data},下单成功");
            }
            else
            {
                MessageBox.Show($"错误：{result.Status.ToString()}");
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

        private async void Bt_getbalance_Click(object sender, EventArgs e)
        {
            await getbalance();

            MessageBox.Show($"可用比特币：{((decimal)dbtc).ToString()},可用usdt:{((decimal)dusdt).ToString()}");

        }


        /// <summary>
        ///  获取交易币种为BTC时的 btc和usdt的可用数量
        /// </summary>
        /// <returns></returns>
        private async Task getbalance()
        {
            long sid;

            if (FormHb.BufferSid["sid"] != null)
            {
                sid = long.Parse(FormHb.BufferSid["sid"].ToString());

            }
            else
            {
                sid = FormHb.api.GetSpotAccountId();
                while (sid == 0)
                {
                    Delay(500);
                    sid = FormHb.api.GetSpotAccountId();
                }
                FormHb.BufferSid["sid"] = sid;
            }

            IList<Huobi.Rest.CSharp.Demo.Model.List> lbalance = await FormHb.api.GetAccountBalance(sid.ToString());

            while (lbalance == null)
            {
                Delay(1000);

                spinLock.Enter();
                try
                {
                    lbalance = await FormHb.api.GetAccountBalance(sid.ToString());
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
                        dbtc = vbtc.Balance;
                    }
                    else if (vbtc.Type == "frozen")
                    {

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
                        dusdt = vusdt.Balance;
                    }
                    else if (vusdt.Type == "frozen")
                    {

                    }
                    else
                    {
                        throw new Exception("");
                    }

                }
            }
        }

        private void Tb_buy_Scroll(object sender, EventArgs e)
        {
            double dbl = 0d;
            double djg = 0;
            if (double.TryParse(txtBuyJg.Text.Trim(), out djg))
            {
                if (tb_buy.Value.ToString() == "0")
                {
                    lblmsg_buy.Text = "0%";
                    dbl = 0d;
                }
                else if (tb_buy.Value.ToString() == "1")
                {
                    lblmsg_buy.Text = "25%";
                    dbl = 0.25;
                }
                else if (tb_buy.Value.ToString() == "2")
                {
                    lblmsg_buy.Text = "50%";
                    dbl = 0.5;
                }
                else if (tb_buy.Value.ToString() == "3")
                {
                    lblmsg_buy.Text = "75%";
                    dbl = 0.75;
                }
                else if (tb_buy.Value.ToString() == "4")
                {
                    lblmsg_buy.Text = "100%";
                    dbl = 1;
                }

                double jg = djg;

                if (sbz == "btc")
                {
                    Decimal tmpd = (Decimal)((dusdt * dbl) / jg);
                    txtBuyXl.Text = tmpd.ToString(4);
                }
                else
                {
                    if (cb_btc.Checked)
                    {
                        Decimal tmpd = (Decimal)((dbtc * dbl) / jg);
                        txtBuyXl.Text = tmpd.ToString(4);
                    }
                    else if (cb_usdt.Checked)
                    {
                        Decimal tmpd = (Decimal)((dusdt * dbl) / jg);
                        txtBuyXl.Text = tmpd.ToString(4);
                    }
                    else
                    {
                        MessageBox.Show("参考币种没有选择！", "提示");
                    }
                }
            }
            else
            {
                MessageBox.Show("买入价格有误！", "提出");
            }

        }

        private async void Bt_getbalance1_Click(object sender, EventArgs e)
        {
            await getbalance1();

            MessageBox.Show($"可用{this.sbz}：{((decimal)dsbz).ToString()},锁定：{((decimal)dsbzlock).ToString()}");

        }

        /// <summary>
        /// 获取交易币种非BTC时的可用数量和锁库数量
        /// </summary>
        /// <returns></returns>
        private async Task getbalance1()
        {
            long sid;

            if (FormHb.BufferSid["sid"] != null)
            {
                sid = long.Parse(FormHb.BufferSid["sid"].ToString());

            }
            else
            {
                sid = FormHb.api.GetSpotAccountId();
                while (sid == 0)
                {
                    Delay(500);
                    sid = FormHb.api.GetSpotAccountId();
                }
                FormHb.BufferSid["sid"] = sid;
            }

            IList<Huobi.Rest.CSharp.Demo.Model.List> lbalance = await FormHb.api.GetAccountBalance(sid.ToString());

            while (lbalance == null)
            {
                Delay(1000);

                spinLock.Enter();
                try
                {
                    lbalance = await FormHb.api.GetAccountBalance(sid.ToString());
                }
                finally
                {
                    spinLock.Exit();
                }

            }
            var lsbz = lbalance.Where(a => a.Currency == this.sbz).ToList();

            if (lsbz.Count == 2)
            {
                foreach (var vsbz in lsbz)
                {
                    if (vsbz.Type == "trade")
                    {
                        dsbz = vsbz.Balance;
                    }
                    else if (vsbz.Type == "frozen")
                    {
                        dsbzlock = vsbz.Balance;
                    }
                    else
                    {
                        throw new Exception("");
                    }


                }
            }
        }

        private void Tb_sell_Scroll(object sender, EventArgs e)
        {
            double dbl = 0d;
            double dsl = 0;
            if (sbz == "btc")
            {
                dsl = dbtc;
            }
            else
            {
                dsl = dsbz;
            }
            if (dsl != 0)
            {
                if (tb_sell.Value.ToString() == "0")
                {
                    lblmsg_sell.Text = "0%";
                    dbl = 0d;
                }
                else if (tb_sell.Value.ToString() == "1")
                {
                    lblmsg_sell.Text = "25%";
                    dbl = 0.25;
                }
                else if (tb_sell.Value.ToString() == "2")
                {
                    lblmsg_sell.Text = "50%";
                    dbl = 0.5;
                }
                else if (tb_sell.Value.ToString() == "3")
                {
                    lblmsg_sell.Text = "75%";
                    dbl = 0.75;
                }
                else if (tb_sell.Value.ToString() == "4")
                {
                    lblmsg_sell.Text = "100%";
                    dbl = 1;
                }



                Decimal tmpd = (Decimal)((dsl * dbl));
                if (dxsamount <= 0)
                {
                    txtSellXl.Text = tmpd.ToString(4);
                }
                else
                {
                    txtSellXl.Text = tmpd.ToString((int)dxsamount);
                }


            }
            else
            {
                MessageBox.Show("可卖数量为0！", "提示");
            }
        }

        private void Dgv_mmsell_Click(object sender, EventArgs e)
        {
            dgv_mmsell.bflag = true;
        }

        private void Dgv_mmsell_Scroll(object sender, ScrollEventArgs e)
        {
            dgv_mmsell.bflag = true;
        }

        private void Dgv_mmbuy_Click(object sender, EventArgs e)
        {
            dgv_mmbuy.bflag = true;
        }

        private void Dgv_mmbuy_Scroll(object sender, ScrollEventArgs e)
        {
            dgv_mmbuy.bflag = true;
        }

        private void Dgv_mmbuy_DoubleClick(object sender, EventArgs e)
        {

        }

        private async void Dgv_mmbuy_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            this.txtSellJg.Text = "";
            string price = double.Parse(dgv_mmbuy.Rows[e.RowIndex].Cells["Price"].Value.ToString()).ToString("0.###############");
            if (sfx == "买")
            {
                this.txtBuyJg.Text = price;
            }
            else if (sfx == "卖")
            {
                this.txtSellJg.Text = price;
            }

            if (sbz == "btc")
            {
                await getbalance();
            }
            else
            {
                await getbalance();
                await getbalance1();
            }
        }
    }
}
