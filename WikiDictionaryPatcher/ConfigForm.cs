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
            options.getHuijiWikiDesc = useHuijiYse.Checked;
            options.useFandomWikiDesc = useFandomYes.Checked;
            options.use_default_font = defaultFont.Checked;
            options.use_half_size_font = halfFontSize.Checked;
            options.use_player_pos = playerPos.Checked;
            options.draw_mouse = drawMouseYes.Checked;
            options.use_bigger_font = biggerFontSize.Checked;
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
            Close();
        }

        private void useHuijiNo_CheckedChanged(object sender, EventArgs e)
        {
            //fandom
            groupBox2.Enabled = useHuijiYse.Checked;
            if(useHuijiNo.Checked)
                useFandomYes.Checked = true;
            //font
            groupBox6.Enabled = useHuijiNo.Checked;
            if (useHuijiYse.Checked)
                dxFont.Checked = true;
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
    }
}
