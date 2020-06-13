using Jli.Core;

namespace Jli
{
    partial class FormMm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.bt_getbalance1 = new System.Windows.Forms.Button();
            this.lblmsg_sell = new System.Windows.Forms.Label();
            this.lblmsg_buy = new System.Windows.Forms.Label();
            this.bt_getbalance = new System.Windows.Forms.Button();
            this.rtb_msg = new System.Windows.Forms.RichTextBox();
            this.cb_jylx = new System.Windows.Forms.ComboBox();
            this.l_bz = new System.Windows.Forms.Label();
            this.cb_usdt = new System.Windows.Forms.RadioButton();
            this.cb_btc = new System.Windows.Forms.RadioButton();
            this.panel3 = new System.Windows.Forms.Panel();
            this.dgv_mmbuy = new Jli.Core.ExDataGridView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dgv_mmsell = new Jli.Core.ExDataGridView();
            this.bt_sell = new System.Windows.Forms.Button();
            this.tb_sell = new System.Windows.Forms.TrackBar();
            this.bt_buy = new System.Windows.Forms.Button();
            this.tb_buy = new System.Windows.Forms.TrackBar();
            this.txtSellXl = new System.Windows.Forms.TextBox();
            this.txtBuyXl = new System.Windows.Forms.TextBox();
            this.txtSellJg = new System.Windows.Forms.TextBox();
            this.txtBuyJg = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbl_selljg = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_mmbuy)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_mmsell)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_sell)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_buy)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.bt_getbalance1);
            this.panel1.Controls.Add(this.lblmsg_sell);
            this.panel1.Controls.Add(this.lblmsg_buy);
            this.panel1.Controls.Add(this.bt_getbalance);
            this.panel1.Controls.Add(this.rtb_msg);
            this.panel1.Controls.Add(this.cb_jylx);
            this.panel1.Controls.Add(this.l_bz);
            this.panel1.Controls.Add(this.cb_usdt);
            this.panel1.Controls.Add(this.cb_btc);
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.bt_sell);
            this.panel1.Controls.Add(this.tb_sell);
            this.panel1.Controls.Add(this.bt_buy);
            this.panel1.Controls.Add(this.tb_buy);
            this.panel1.Controls.Add(this.txtSellXl);
            this.panel1.Controls.Add(this.txtBuyXl);
            this.panel1.Controls.Add(this.txtSellJg);
            this.panel1.Controls.Add(this.txtBuyJg);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.lbl_selljg);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1225, 847);
            this.panel1.TabIndex = 0;
            // 
            // bt_getbalance1
            // 
            this.bt_getbalance1.Location = new System.Drawing.Point(382, 421);
            this.bt_getbalance1.Name = "bt_getbalance1";
            this.bt_getbalance1.Size = new System.Drawing.Size(86, 23);
            this.bt_getbalance1.TabIndex = 17;
            this.bt_getbalance1.Text = "获取持仓";
            this.bt_getbalance1.UseVisualStyleBackColor = true;
            this.bt_getbalance1.Click += new System.EventHandler(this.Bt_getbalance1_Click);
            // 
            // lblmsg_sell
            // 
            this.lblmsg_sell.AutoSize = true;
            this.lblmsg_sell.ForeColor = System.Drawing.Color.Blue;
            this.lblmsg_sell.Location = new System.Drawing.Point(600, 237);
            this.lblmsg_sell.Name = "lblmsg_sell";
            this.lblmsg_sell.Size = new System.Drawing.Size(23, 15);
            this.lblmsg_sell.TabIndex = 16;
            this.lblmsg_sell.Text = "0%";
            // 
            // lblmsg_buy
            // 
            this.lblmsg_buy.AutoSize = true;
            this.lblmsg_buy.ForeColor = System.Drawing.Color.Blue;
            this.lblmsg_buy.Location = new System.Drawing.Point(259, 237);
            this.lblmsg_buy.Name = "lblmsg_buy";
            this.lblmsg_buy.Size = new System.Drawing.Size(23, 15);
            this.lblmsg_buy.TabIndex = 16;
            this.lblmsg_buy.Text = "0%";
            // 
            // bt_getbalance
            // 
            this.bt_getbalance.Location = new System.Drawing.Point(123, 421);
            this.bt_getbalance.Name = "bt_getbalance";
            this.bt_getbalance.Size = new System.Drawing.Size(115, 23);
            this.bt_getbalance.TabIndex = 15;
            this.bt_getbalance.Text = "获取帐户资金";
            this.bt_getbalance.UseVisualStyleBackColor = true;
            this.bt_getbalance.Click += new System.EventHandler(this.Bt_getbalance_Click);
            // 
            // rtb_msg
            // 
            this.rtb_msg.Location = new System.Drawing.Point(12, 540);
            this.rtb_msg.Name = "rtb_msg";
            this.rtb_msg.Size = new System.Drawing.Size(1185, 287);
            this.rtb_msg.TabIndex = 14;
            this.rtb_msg.Text = "";
            // 
            // cb_jylx
            // 
            this.cb_jylx.FormattingEnabled = true;
            this.cb_jylx.Location = new System.Drawing.Point(55, 79);
            this.cb_jylx.Name = "cb_jylx";
            this.cb_jylx.Size = new System.Drawing.Size(143, 23);
            this.cb_jylx.TabIndex = 13;
            // 
            // l_bz
            // 
            this.l_bz.AutoSize = true;
            this.l_bz.Font = new System.Drawing.Font("微软雅黑", 22.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.l_bz.ForeColor = System.Drawing.Color.DodgerBlue;
            this.l_bz.Location = new System.Drawing.Point(260, 13);
            this.l_bz.Name = "l_bz";
            this.l_bz.Size = new System.Drawing.Size(96, 50);
            this.l_bz.TabIndex = 12;
            this.l_bz.Text = "BTC";
            // 
            // cb_usdt
            // 
            this.cb_usdt.AutoSize = true;
            this.cb_usdt.Location = new System.Drawing.Point(530, 80);
            this.cb_usdt.Name = "cb_usdt";
            this.cb_usdt.Size = new System.Drawing.Size(60, 19);
            this.cb_usdt.TabIndex = 11;
            this.cb_usdt.Text = "USDT";
            this.cb_usdt.UseVisualStyleBackColor = true;
            this.cb_usdt.CheckedChanged += new System.EventHandler(this.Cb_usdt_CheckedChanged);
            // 
            // cb_btc
            // 
            this.cb_btc.AutoSize = true;
            this.cb_btc.Checked = true;
            this.cb_btc.Location = new System.Drawing.Point(416, 80);
            this.cb_btc.Name = "cb_btc";
            this.cb_btc.Size = new System.Drawing.Size(52, 19);
            this.cb_btc.TabIndex = 10;
            this.cb_btc.TabStop = true;
            this.cb_btc.Text = "BTC";
            this.cb_btc.UseVisualStyleBackColor = true;
            this.cb_btc.CheckedChanged += new System.EventHandler(this.Cb_btc_CheckedChanged);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.dgv_mmbuy);
            this.panel3.Location = new System.Drawing.Point(690, 286);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(507, 236);
            this.panel3.TabIndex = 7;
            // 
            // dgv_mmbuy
            // 
            this.dgv_mmbuy.AllowUserToAddRows = false;
            this.dgv_mmbuy.AllowUserToDeleteRows = false;
            this.dgv_mmbuy.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_mmbuy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_mmbuy.Location = new System.Drawing.Point(0, 0);
            this.dgv_mmbuy.Name = "dgv_mmbuy";
            this.dgv_mmbuy.ReadOnly = true;
            this.dgv_mmbuy.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.Color.ForestGreen;
            this.dgv_mmbuy.RowTemplate.Height = 27;
            this.dgv_mmbuy.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_mmbuy.Size = new System.Drawing.Size(507, 236);
            this.dgv_mmbuy.TabIndex = 0;
            this.dgv_mmbuy.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Dgv_mmbuy_CellDoubleClick);
            this.dgv_mmbuy.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.Dgv_mmbuy_CellFormatting);
            this.dgv_mmbuy.Scroll += new System.Windows.Forms.ScrollEventHandler(this.Dgv_mmbuy_Scroll);
            this.dgv_mmbuy.Click += new System.EventHandler(this.Dgv_mmbuy_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dgv_mmsell);
            this.panel2.Location = new System.Drawing.Point(690, 13);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(507, 236);
            this.panel2.TabIndex = 6;
            // 
            // dgv_mmsell
            // 
            this.dgv_mmsell.AllowUserToAddRows = false;
            this.dgv_mmsell.AllowUserToDeleteRows = false;
            this.dgv_mmsell.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_mmsell.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_mmsell.Location = new System.Drawing.Point(0, 0);
            this.dgv_mmsell.Name = "dgv_mmsell";
            this.dgv_mmsell.ReadOnly = true;
            this.dgv_mmsell.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.Color.Red;
            this.dgv_mmsell.RowTemplate.Height = 27;
            this.dgv_mmsell.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_mmsell.Size = new System.Drawing.Size(507, 236);
            this.dgv_mmsell.TabIndex = 0;
            this.dgv_mmsell.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Dgv_mmsell_CellDoubleClick);
            this.dgv_mmsell.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.Dgv_mmsell_CellFormatting);
            this.dgv_mmsell.Scroll += new System.Windows.Forms.ScrollEventHandler(this.Dgv_mmsell_Scroll);
            this.dgv_mmsell.Click += new System.EventHandler(this.Dgv_mmsell_Click);
            // 
            // bt_sell
            // 
            this.bt_sell.BackColor = System.Drawing.Color.SandyBrown;
            this.bt_sell.Location = new System.Drawing.Point(473, 333);
            this.bt_sell.Name = "bt_sell";
            this.bt_sell.Size = new System.Drawing.Size(87, 31);
            this.bt_sell.TabIndex = 5;
            this.bt_sell.Text = "卖";
            this.bt_sell.UseVisualStyleBackColor = false;
            this.bt_sell.Click += new System.EventHandler(this.Bt_sell_Click);
            // 
            // tb_sell
            // 
            this.tb_sell.Location = new System.Drawing.Point(396, 237);
            this.tb_sell.Maximum = 4;
            this.tb_sell.Name = "tb_sell";
            this.tb_sell.Size = new System.Drawing.Size(183, 56);
            this.tb_sell.TabIndex = 4;
            this.tb_sell.Scroll += new System.EventHandler(this.Tb_sell_Scroll);
            // 
            // bt_buy
            // 
            this.bt_buy.BackColor = System.Drawing.Color.PaleGreen;
            this.bt_buy.Location = new System.Drawing.Point(123, 333);
            this.bt_buy.Name = "bt_buy";
            this.bt_buy.Size = new System.Drawing.Size(88, 31);
            this.bt_buy.TabIndex = 5;
            this.bt_buy.Text = "买";
            this.bt_buy.UseVisualStyleBackColor = false;
            this.bt_buy.Click += new System.EventHandler(this.Bt_buy_Click);
            // 
            // tb_buy
            // 
            this.tb_buy.Location = new System.Drawing.Point(55, 237);
            this.tb_buy.Maximum = 4;
            this.tb_buy.Name = "tb_buy";
            this.tb_buy.Size = new System.Drawing.Size(183, 56);
            this.tb_buy.TabIndex = 4;
            this.tb_buy.Scroll += new System.EventHandler(this.Tb_buy_Scroll);
            // 
            // txtSellXl
            // 
            this.txtSellXl.Location = new System.Drawing.Point(461, 176);
            this.txtSellXl.Name = "txtSellXl";
            this.txtSellXl.Size = new System.Drawing.Size(173, 25);
            this.txtSellXl.TabIndex = 3;
            // 
            // txtBuyXl
            // 
            this.txtBuyXl.Location = new System.Drawing.Point(123, 179);
            this.txtBuyXl.Name = "txtBuyXl";
            this.txtBuyXl.Size = new System.Drawing.Size(173, 25);
            this.txtBuyXl.TabIndex = 3;
            // 
            // txtSellJg
            // 
            this.txtSellJg.Location = new System.Drawing.Point(461, 128);
            this.txtSellJg.Name = "txtSellJg";
            this.txtSellJg.Size = new System.Drawing.Size(173, 25);
            this.txtSellJg.TabIndex = 3;
            // 
            // txtBuyJg
            // 
            this.txtBuyJg.Location = new System.Drawing.Point(123, 131);
            this.txtBuyJg.Name = "txtBuyJg";
            this.txtBuyJg.Size = new System.Drawing.Size(173, 25);
            this.txtBuyJg.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Coral;
            this.label4.Location = new System.Drawing.Point(390, 179);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 15);
            this.label4.TabIndex = 2;
            this.label4.Text = "卖出量";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.ForestGreen;
            this.label2.Location = new System.Drawing.Point(52, 182);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "买入量";
            // 
            // lbl_selljg
            // 
            this.lbl_selljg.AutoSize = true;
            this.lbl_selljg.ForeColor = System.Drawing.Color.Coral;
            this.lbl_selljg.Location = new System.Drawing.Point(390, 131);
            this.lbl_selljg.Name = "lbl_selljg";
            this.lbl_selljg.Size = new System.Drawing.Size(52, 15);
            this.lbl_selljg.TabIndex = 1;
            this.lbl_selljg.Text = "卖出价";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.ForestGreen;
            this.label1.Location = new System.Drawing.Point(52, 134);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "买入价";
            // 
            // FormMm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1225, 847);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "FormMm";
            this.Text = "交易";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMm_FormClosed);
            this.Load += new System.EventHandler(this.FormMm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_mmbuy)).EndInit();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_mmsell)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_sell)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_buy)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button bt_sell;
        private System.Windows.Forms.Button bt_buy;
        private System.Windows.Forms.TrackBar tb_buy;
        private System.Windows.Forms.TextBox txtSellXl;
        private System.Windows.Forms.TextBox txtBuyXl;
        private System.Windows.Forms.TextBox txtSellJg;
        private System.Windows.Forms.TextBox txtBuyJg;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbl_selljg;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private ExDataGridView dgv_mmbuy;
        private ExDataGridView dgv_mmsell;
        private System.Windows.Forms.RadioButton cb_btc;
        private System.Windows.Forms.RadioButton cb_usdt;
        private System.Windows.Forms.Label l_bz;
        private System.Windows.Forms.ComboBox cb_jylx;
        private System.Windows.Forms.RichTextBox rtb_msg;
        private System.Windows.Forms.Button bt_getbalance;
        private System.Windows.Forms.Label lblmsg_buy;
        private System.Windows.Forms.Button bt_getbalance1;
        private System.Windows.Forms.Label lblmsg_sell;
        private System.Windows.Forms.TrackBar tb_sell;
    }
}