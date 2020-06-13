using Jli.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Jli
{
    public partial class FormLeft : DockContent
    {
        public FormLeft()
        {
            InitializeComponent();
        }

        private void Bt_fxh_Click(object sender, EventArgs e) //非小号
        {
            BrowserHelper.OpenBrowserUrl("https://www.feixiaohao.com/");
        }

        private void Bt_huobi_Click(object sender, EventArgs e) //火币
        {
            BrowserHelper.OpenBrowserUrl("https://www.huobi.co/");
        }

        private void Button1_Click(object sender, EventArgs e) //巴比特
        {
            BrowserHelper.OpenBrowserUrl("https://www.chainnode.com/");
        }

        private void FormLeft_Paint(object sender, PaintEventArgs e)
        {
            ////浮雕文字
            //Brush backBrush = Brushes.Black;
            //Brush foreBrush = Brushes.Red;
            //Font font = new Font("宋体", Convert.ToInt16(20), FontStyle.Regular);
            //Graphics g = this.CreateGraphics();
            //string text = "早上大跌要买,早上大涨要卖。\r\n下午大涨不追,下午大跌次日买。\r\n早上大跌不割,不涨不跌睡觉。";
            //SizeF size = g.MeasureString(text, font);
            //Single posX = (this.Width - Convert.ToInt16(size.Width)) / 2;
            //Single posY = (this.Height - Convert.ToInt16(size.Height)) / 2;
            //g.DrawString(text, font, backBrush, posX + 1, posY + 1);
            //g.DrawString(text, font, foreBrush, posX, posY);
           
        }
    }
}
