using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jli.Core
{
    public static class DecimalExtension
    {
        /// <summary>
        /// decimal保留指定位数小数
        /// </summary>
        /// <param name="num">原始数量</param>
        /// <param name="scale">保留小数位数</param>
        /// <returns>截取指定小数位数后的数量字符串</returns>
        public static string ToString(this decimal num, int scale)
        {
            string numToString = num.ToString();

            int index = numToString.IndexOf(".");
            int length = numToString.Length;

            if (index != -1)
            {
                return string.Format("{0}.{1}",
                  numToString.Substring(0, index),
                  numToString.Substring(index + 1, Math.Min(length - index - 1, scale)));
            }
            else
            {
                return num.ToString();
            }
        }
    }
}
