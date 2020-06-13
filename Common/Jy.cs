using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jli.Common
{
    public class Jy
    {
        public string Sbz { get; set; } //币种
        public string Cjj { get; set; } //最新成交价
        public string CjjRmb { get; set; }//最新成交价对应的人民币价格
        public string OwnerYe { get; set; }//帐户持币数量
        public double OwnerRmb { get; set; }//帐户余额
        public double Zdf { get; set; } //涨跌幅 
    };

    public class JyFx :Jy
    {
        public string Sfx { get; set; }
    }
}
