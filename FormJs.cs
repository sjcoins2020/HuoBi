using Huobi.Rest.CSharp.Demo.Model.Extend.HbIntroductionJsonTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jli
{
    public partial class FormJs : Form
    {
        string sbz = "";
        public FormJs()
        {
            InitializeComponent();
            
        }

        public FormJs(string sbz):this()
        {
            this.sbz = sbz;
           
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            string sparams = $"https://www.huobi.co/-/x/hb/p/api/contents/pro/currency_introduction?r=76mdmiullhw&currency={sbz}&lang=zh-cn";
            var result = await FormHb.api.GetIntroduction(sparams);

            if (result != null)
            {
                new Thread(() =>
                {

                    Action action = () =>
                    {
                        HbIntroData  lgv = new  HbIntroData();
                        lgv = result?.Data;

                        propertyGrid1.SelectedObject = lgv;


                    };
                    if (this.propertyGrid1.IsHandleCreated)
                    {
                        BeginInvoke(action);
                    }
                }, 1024).Start();
            }
        }

        private void FormJs_Load(object sender, EventArgs e)
        {
            this.Text = $"{sbz}";
            Button1_Click(this, null);
        }
    }
}
