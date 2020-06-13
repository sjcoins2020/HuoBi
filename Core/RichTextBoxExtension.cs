using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jli.Core
{
    public static class RichTextBoxExtension
    {
        public static void ClearData(this RichTextBox rtBox, bool bclear = true)
        {
            if (bclear)
            {
                rtbStrList.Clear();
                rtBox.Clear();
            }
        }

        public static void AppendTextColorful(this RichTextBox rtBox, string sid,string text, Color color, bool addNewLine = true)
        {
            if (addNewLine)
            {
                text += Environment.NewLine;
            }
            rtBox.SelectionStart = rtBox.TextLength;
            rtBox.SelectionLength = 0;
            rtBox.SelectionColor = color;
            //rtBox.Clear();
            if(BAdd(sid))
            {
                rtBox.AppendText(text);
            }

            #region 删除首行
            if (rtBox.Lines.Length > 11)
            {
                rtBox.SelectionStart = 0;
                rtBox.SelectionLength = rtBox.GetFirstCharIndexFromLine(1);
                rtBox.SelectedText = "";
            }
            #endregion

            //#region 跳转到指定行
          
            //rtBox.Focus();
            //rtBox.ScrollToCaret();
            //#endregion

        }

        public static void AppendTextColorful(this RichTextBox rtBox, string text, Color color, bool addNewLine = true)
        {
            if (addNewLine)
            {
                text += Environment.NewLine;
            }
            rtBox.SelectionStart = rtBox.TextLength;
            rtBox.SelectionLength = 0;
            rtBox.SelectionColor = color;
            rtBox.AppendText(text);
            //rtBox.SelectionColor = rtBox.ForeColor;
        }

        static List<string> rtbStrList = new List<string>();
        public static bool BAdd(string sid)//参数：要插入的数据
        {
            if (!rtbStrList.Contains(sid))
            {
                rtbStrList.Insert(0, sid);
                if (rtbStrList.Count > 10)//移除最后一句
                {
                    rtbStrList.RemoveAt(10);
                }
                return true;
            }
            else
            {
                return false;
            }


        }
    }
}
