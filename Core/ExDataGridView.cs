using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jli.Core
{
    public class ExDataGridView : System.Windows.Forms.DataGridView
    {
        int x = 0;

        int y = 0;

        int row = 0;

        internal bool bflag = false;

        public ExDataGridView()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            UpdateStyles();
            //SetStyle(this);
        }
        private void SetStyle(DataGridView dataGridView)
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
        protected override void OnScroll(ScrollEventArgs e)
        {

            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                x = e.NewValue;
            }
            else if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                y = e.NewValue;
            }
            base.OnScroll(e);
        }

        protected override void OnClick(EventArgs e)
        {

            if (SelectedRows.Count == 0)
            {
                row = 0;
            }
            else
            {
                row = SelectedRows[0].Index;//获取选中行    // this.CurrentRow
            }

            base.OnClick(e);
        }

        //protected override void OnSelectionChanged(EventArgs e)
        //{

        //    base.OnSelectionChanged(e);
        //}

        public void Restore()
        {

            try
            {
                this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                this.FirstDisplayedScrollingRowIndex = this.x;
                this.HorizontalScrollingOffset = this.y;
                this.MultiSelect = false;

                this.Rows[row].Selected = true;
                this.CurrentCell = this.Rows[row].Cells[0];


            }
            catch
            {

            }
        }


    }

    //类必须为static的
    public static class ExControl
    {
        //扩展方法必须为静态的
        //扩展方法的第一个参数必须由this来修饰（第一个参数是被扩展的对象）
        public static bool Restore(this ExDataGridView gv)
        {
            gv.Restore();

            return true;
        }

    }

    public static class ExtensionMethods
    {
        public static void DoubleBuffered(this DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
        }
    }
}
