namespace Jli
{
    partial class FormLeft
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
            this.bt_huobi = new System.Windows.Forms.Button();
            this.bt_fxh = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // bt_huobi
            // 
            this.bt_huobi.Location = new System.Drawing.Point(42, 39);
            this.bt_huobi.Name = "bt_huobi";
            this.bt_huobi.Size = new System.Drawing.Size(75, 23);
            this.bt_huobi.TabIndex = 0;
            this.bt_huobi.Text = "火币";
            this.bt_huobi.UseVisualStyleBackColor = true;
            this.bt_huobi.Click += new System.EventHandler(this.Bt_huobi_Click);
            // 
            // bt_fxh
            // 
            this.bt_fxh.Location = new System.Drawing.Point(42, 94);
            this.bt_fxh.Name = "bt_fxh";
            this.bt_fxh.Size = new System.Drawing.Size(75, 23);
            this.bt_fxh.TabIndex = 1;
            this.bt_fxh.Text = "非小号";
            this.bt_fxh.UseVisualStyleBackColor = true;
            this.bt_fxh.Click += new System.EventHandler(this.Bt_fxh_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(42, 154);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "巴比特";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // FormLeft
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(173, 450);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.bt_fxh);
            this.Controls.Add(this.bt_huobi);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "FormLeft";
            this.Text = "网址";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormLeft_Paint);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bt_huobi;
        private System.Windows.Forms.Button bt_fxh;
        private System.Windows.Forms.Button button1;
    }
}