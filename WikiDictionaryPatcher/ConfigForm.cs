using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WikiDictionaryPatcher
{
    public partial class ConfigForm : Form
    {
        DicOptions options;
        public ConfigForm(DicOptions options)
        {
            InitializeComponent();
            this.options = options;
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            options.canceled = true;
            Close();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void yseButton_Click(object sender, EventArgs e)
        {
            options.canceled = false;
            options.getHuijiWikiDesc = useHuijiYse.Checked;
            options.useFandomWikiDesc = useFandomYes.Checked;
            options.use_default_font = defaultFont.Checked;
            options.use_half_size_font = halfFontSize.Checked;
            options.use_player_pos = playerPos.Checked;
            options.draw_mouse = drawMouseYes.Checked;
            options.use_bigger_font = biggerFontSize.Checked;
            options.use_st_10_font = stFont10.Checked;
            options.use_st_12_font = stFont12.Checked;
            options.use_st_16_font = stFont16.Checked;
            options.use_dx_16_font = dxFont16.Checked;
            options.use_dx_12_font = dxFont12.Checked;
            options.use_default_font = defaultFont.Checked;
            options.renderQrCode = qrCodeYes.Checked;

            {
                int p = textTransparentBar.Value * 100 / textTransparentBar.Maximum;
                options.textTransparent = p + "/100";
            }
            {
                int p = qrTransparentBar.Value * 100 / qrTransparentBar.Maximum;
                options.qrTransparent = p + "/100";
            }

            Close();
        }

        private void useHuijiNo_CheckedChanged(object sender, EventArgs e)
        {
            //fandom
            groupBox2.Enabled = useHuijiYse.Checked;
            if(useHuijiNo.Checked)
                useFandomYes.Checked = true;
            //font
            defaultFont.Enabled = useHuijiNo.Checked;
            if (useHuijiYse.Checked && defaultFont.Checked)
                stFont12.Checked = true;
        }

        private void mousePos_CheckedChanged(object sender, EventArgs e)
        {
            //draw mouse screen
            groupBox4.Enabled = mousePos.Checked;
            if (playerPos.Checked)
                drawMouseNo.Checked = true;
        }

        private void useFandomYes_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void fullFontSize_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void defaultFont_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            int prog = textTransparentBar.Value * 100 / textTransparentBar.Maximum;
            textTransparentLabel.Text = prog + "%";
        }

        private void textTransparentLabel_Click(object sender, EventArgs e)
        {

        }

        private void qrTransparentBar_Scroll(object sender, EventArgs e)
        {
            int prog = qrTransparentBar.Value * 100 / qrTransparentBar.Maximum;
            qrTransparentLabel.Text = prog + "%";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "游戏内使用键盘Tab键进行操作，同时也支持手柄对应按键进行操作\n"+
                "点击Tab键\t立即关闭开局提示\n"+
                "双击Tab键\t切换二维码开关\n"+
                "长按Tab键\t显示卡牌/符文/药丸，如果此时显示的是道具/饰品信息，二维码会切换会为不透明显示\n"+
                "Tab+Ctrl\t在雅各和以扫的游戏中，显示以扫的卡牌/符文/药丸信息"
                );
        }
    }
}
