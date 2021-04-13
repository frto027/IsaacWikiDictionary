namespace WikiDictionaryPatcher
{
    partial class ConfigForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.useHuijiNo = new System.Windows.Forms.RadioButton();
            this.useHuijiYse = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.useFandomNo = new System.Windows.Forms.RadioButton();
            this.useFandomYes = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.mousePos = new System.Windows.Forms.RadioButton();
            this.playerPos = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.drawMouseNo = new System.Windows.Forms.RadioButton();
            this.drawMouseYes = new System.Windows.Forms.RadioButton();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.halfFontSize = new System.Windows.Forms.RadioButton();
            this.fullFontSize = new System.Windows.Forms.RadioButton();
            this.yseButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.defaultFont = new System.Windows.Forms.RadioButton();
            this.dxFont = new System.Windows.Forms.RadioButton();
            this.biggerFontSize = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.useHuijiNo);
            this.groupBox1.Controls.Add(this.useHuijiYse);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(271, 55);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "是否下载灰机wiki信息？";
            // 
            // useHuijiNo
            // 
            this.useHuijiNo.AutoSize = true;
            this.useHuijiNo.Location = new System.Drawing.Point(145, 20);
            this.useHuijiNo.Name = "useHuijiNo";
            this.useHuijiNo.Size = new System.Drawing.Size(35, 16);
            this.useHuijiNo.TabIndex = 2;
            this.useHuijiNo.Text = "否";
            this.useHuijiNo.UseVisualStyleBackColor = true;
            this.useHuijiNo.CheckedChanged += new System.EventHandler(this.useHuijiNo_CheckedChanged);
            // 
            // useHuijiYse
            // 
            this.useHuijiYse.AutoSize = true;
            this.useHuijiYse.Checked = true;
            this.useHuijiYse.Location = new System.Drawing.Point(18, 20);
            this.useHuijiYse.Name = "useHuijiYse";
            this.useHuijiYse.Size = new System.Drawing.Size(35, 16);
            this.useHuijiYse.TabIndex = 1;
            this.useHuijiYse.TabStop = true;
            this.useHuijiYse.Text = "是";
            this.useHuijiYse.UseVisualStyleBackColor = true;
            this.useHuijiYse.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.useFandomNo);
            this.groupBox2.Controls.Add(this.useFandomYes);
            this.groupBox2.Location = new System.Drawing.Point(289, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(271, 55);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "是否下载英文fandom wiki信息？";
            // 
            // useFandomNo
            // 
            this.useFandomNo.AutoSize = true;
            this.useFandomNo.Checked = true;
            this.useFandomNo.Location = new System.Drawing.Point(145, 20);
            this.useFandomNo.Name = "useFandomNo";
            this.useFandomNo.Size = new System.Drawing.Size(35, 16);
            this.useFandomNo.TabIndex = 2;
            this.useFandomNo.TabStop = true;
            this.useFandomNo.Text = "否";
            this.useFandomNo.UseVisualStyleBackColor = true;
            // 
            // useFandomYes
            // 
            this.useFandomYes.AutoSize = true;
            this.useFandomYes.Location = new System.Drawing.Point(18, 20);
            this.useFandomYes.Name = "useFandomYes";
            this.useFandomYes.Size = new System.Drawing.Size(35, 16);
            this.useFandomYes.TabIndex = 1;
            this.useFandomYes.Text = "是";
            this.useFandomYes.UseVisualStyleBackColor = true;
            this.useFandomYes.CheckedChanged += new System.EventHandler(this.useFandomYes_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.label1.Location = new System.Drawing.Point(81, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(479, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "如果您同时下载灰机wiki和fandom wiki的信息，对于二者都有的内容，优先使用灰机wiki";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.mousePos);
            this.groupBox3.Controls.Add(this.playerPos);
            this.groupBox3.Location = new System.Drawing.Point(12, 95);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(271, 55);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "道具选择方式";
            // 
            // mousePos
            // 
            this.mousePos.AutoSize = true;
            this.mousePos.Location = new System.Drawing.Point(145, 20);
            this.mousePos.Name = "mousePos";
            this.mousePos.Size = new System.Drawing.Size(71, 16);
            this.mousePos.TabIndex = 2;
            this.mousePos.Text = "鼠标选择";
            this.mousePos.UseVisualStyleBackColor = true;
            this.mousePos.CheckedChanged += new System.EventHandler(this.mousePos_CheckedChanged);
            // 
            // playerPos
            // 
            this.playerPos.AutoSize = true;
            this.playerPos.Checked = true;
            this.playerPos.Location = new System.Drawing.Point(18, 20);
            this.playerPos.Name = "playerPos";
            this.playerPos.Size = new System.Drawing.Size(83, 16);
            this.playerPos.TabIndex = 1;
            this.playerPos.TabStop = true;
            this.playerPos.Text = "与玩家最近";
            this.playerPos.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.drawMouseNo);
            this.groupBox4.Controls.Add(this.drawMouseYes);
            this.groupBox4.Enabled = false;
            this.groupBox4.Location = new System.Drawing.Point(289, 95);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(271, 55);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "在屏幕上绘制鼠标（供全屏使用）";
            // 
            // drawMouseNo
            // 
            this.drawMouseNo.AutoSize = true;
            this.drawMouseNo.Checked = true;
            this.drawMouseNo.Location = new System.Drawing.Point(145, 20);
            this.drawMouseNo.Name = "drawMouseNo";
            this.drawMouseNo.Size = new System.Drawing.Size(35, 16);
            this.drawMouseNo.TabIndex = 2;
            this.drawMouseNo.TabStop = true;
            this.drawMouseNo.Text = "否";
            this.drawMouseNo.UseVisualStyleBackColor = true;
            // 
            // drawMouseYes
            // 
            this.drawMouseYes.AutoSize = true;
            this.drawMouseYes.Location = new System.Drawing.Point(18, 20);
            this.drawMouseYes.Name = "drawMouseYes";
            this.drawMouseYes.Size = new System.Drawing.Size(35, 16);
            this.drawMouseYes.TabIndex = 1;
            this.drawMouseYes.Text = "是";
            this.drawMouseYes.UseVisualStyleBackColor = true;
            this.drawMouseYes.CheckedChanged += new System.EventHandler(this.radioButton8_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.biggerFontSize);
            this.groupBox5.Controls.Add(this.halfFontSize);
            this.groupBox5.Controls.Add(this.fullFontSize);
            this.groupBox5.Location = new System.Drawing.Point(289, 156);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(271, 55);
            this.groupBox5.TabIndex = 4;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "字体大小";
            // 
            // halfFontSize
            // 
            this.halfFontSize.AutoSize = true;
            this.halfFontSize.Location = new System.Drawing.Point(206, 20);
            this.halfFontSize.Name = "halfFontSize";
            this.halfFontSize.Size = new System.Drawing.Size(59, 16);
            this.halfFontSize.TabIndex = 2;
            this.halfFontSize.Text = "半尺寸";
            this.halfFontSize.UseVisualStyleBackColor = true;
            // 
            // fullFontSize
            // 
            this.fullFontSize.AutoSize = true;
            this.fullFontSize.Checked = true;
            this.fullFontSize.Location = new System.Drawing.Point(121, 20);
            this.fullFontSize.Name = "fullFontSize";
            this.fullFontSize.Size = new System.Drawing.Size(59, 16);
            this.fullFontSize.TabIndex = 1;
            this.fullFontSize.Text = "全尺寸";
            this.fullFontSize.UseVisualStyleBackColor = true;
            this.fullFontSize.CheckedChanged += new System.EventHandler(this.fullFontSize_CheckedChanged);
            // 
            // yseButton
            // 
            this.yseButton.Location = new System.Drawing.Point(208, 217);
            this.yseButton.Name = "yseButton";
            this.yseButton.Size = new System.Drawing.Size(75, 23);
            this.yseButton.TabIndex = 5;
            this.yseButton.Text = "确认安装";
            this.yseButton.UseVisualStyleBackColor = true;
            this.yseButton.Click += new System.EventHandler(this.yseButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(302, 217);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "取消安装";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.defaultFont);
            this.groupBox6.Controls.Add(this.dxFont);
            this.groupBox6.Enabled = false;
            this.groupBox6.Location = new System.Drawing.Point(12, 156);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(271, 55);
            this.groupBox6.TabIndex = 5;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "字体";
            // 
            // defaultFont
            // 
            this.defaultFont.AutoSize = true;
            this.defaultFont.Location = new System.Drawing.Point(145, 20);
            this.defaultFont.Name = "defaultFont";
            this.defaultFont.Size = new System.Drawing.Size(119, 16);
            this.defaultFont.TabIndex = 2;
            this.defaultFont.Text = "游戏默认(仅英文)";
            this.defaultFont.UseVisualStyleBackColor = true;
            // 
            // dxFont
            // 
            this.dxFont.AutoSize = true;
            this.dxFont.Checked = true;
            this.dxFont.Location = new System.Drawing.Point(18, 20);
            this.dxFont.Name = "dxFont";
            this.dxFont.Size = new System.Drawing.Size(47, 16);
            this.dxFont.TabIndex = 1;
            this.dxFont.TabStop = true;
            this.dxFont.Text = "等线";
            this.dxFont.UseVisualStyleBackColor = true;
            // 
            // biggerFontSize
            // 
            this.biggerFontSize.AutoSize = true;
            this.biggerFontSize.Location = new System.Drawing.Point(29, 20);
            this.biggerFontSize.Name = "biggerFontSize";
            this.biggerFontSize.Size = new System.Drawing.Size(71, 16);
            this.biggerFontSize.TabIndex = 3;
            this.biggerFontSize.Text = "更大尺寸";
            this.biggerFontSize.UseVisualStyleBackColor = true;
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(572, 263);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.yseButton);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "ConfigForm";
            this.Text = "配置图鉴信息";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton useHuijiNo;
        private System.Windows.Forms.RadioButton useHuijiYse;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton useFandomNo;
        private System.Windows.Forms.RadioButton useFandomYes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton mousePos;
        private System.Windows.Forms.RadioButton playerPos;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton drawMouseNo;
        private System.Windows.Forms.RadioButton drawMouseYes;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.RadioButton halfFontSize;
        private System.Windows.Forms.RadioButton fullFontSize;
        private System.Windows.Forms.Button yseButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.RadioButton defaultFont;
        private System.Windows.Forms.RadioButton dxFont;
        private System.Windows.Forms.RadioButton biggerFontSize;
    }
}