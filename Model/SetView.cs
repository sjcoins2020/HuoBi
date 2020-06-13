using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jli.Model
{
    public static class SetView
    {
        internal static string accesskey { get; set; }

        internal static string secretkey { get; set; }

        internal static string hostapi { get; set; }

        public static bool blog { get; set; }

        public static bool vw_hangqin { get; set; }

        public static bool vw_debugger { get; set; }

        public static bool vw_kline { get; set; }

        public static bool vw_introduction { get; set; }

        public static bool guanzhu { get; set; }
        public static bool autochoose { get; set; }

        public static bool autofy { get; set; }  //是否分页显示行情数据

        public static int pagesize { get; set; } //分页的大小

    }
}
