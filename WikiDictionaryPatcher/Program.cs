using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WikiDictionaryPatcher
{
    class Program
    {
        private static string
            FAKE_LINE_BEGIN = "-- WikiDict MARK START --",
            FAKE_DESC_CONTENT = "-- FAKE_DESC_CONTENT --",
            FAKE_LINE_END = "-- WikiDict MARK END --";
#if DEBUG
        private static string
            RES_MAIN_LUA_PATCH = @"..\..\..\patch.lua",
            RES_FONT_FOLDER_PATCH = @"..\..\..\wd_font";
#else
        private static string
            RES_MAIN_LUA_PATCH = @"patch.lua",
            RES_FONT_FOLDER_PATCH = "wd_font";
#endif
        [STAThread]
        static void Main(string[] args)
        {
            MessageBox.Show("版权声明：此图鉴程序著作权归属@frto027(bilibili/github/gitee：frto027、贴吧id：frt-027)所有，且保留追究责任的权利，任何形式的转载需注明出处。\n图鉴中展示的物品条例版权归原作者所有。");
            MessageBox.Show("此版本图鉴数据来源：灰机wiki(https://isaac.huijiwiki.com/)");
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "以撒主程序(isaac-ng.exe)|";
            dialog.Title = "请选择以撒的主程序isaac-ng.exe(支持的游戏版本：胎衣+或忏悔)";
            dialog.CheckFileExists = true;
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show("已取消操作");
                return;
            }

            if (!(dialog.FileName?.EndsWith("isaac-ng.exe") ?? false))
            {
                MessageBox.Show("文件名字不正确(需要选中以撒主文件isaac-ng.exe)");
                return;
            }

            bool patched = false;
            string lua_path = dialog.FileName + "\\..\\resources\\scripts\\main.lua";

            using (FileStream f = new FileStream(lua_path, FileMode.Open))
            {
                using(StreamReader reader = new StreamReader(f))
                {
                    string s = reader.ReadToEnd();
                    patched = s.Contains(FAKE_LINE_BEGIN) && s.Contains(FAKE_LINE_END);
                    //Console.WriteLine(s);
                }
            }

            if (patched)
            {
                if(MessageBox.Show("检测到已经添加图鉴，是否移除图鉴？您可以移除图鉴后再次添加，以达到与wiki同步的效果。","是否移除图鉴？",MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
                else
                {
                    CancelPatch(lua_path);
                    MessageBox.Show("图鉴已经移除");
                    return;
                }
            }

            if(MessageBox.Show("是否下载wiki信息并添加游戏图鉴？","询问",MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            //爬！
            Console.WriteLine("正在下载灰机wiki中的道具信息...");
            WebRequest request = HttpWebRequest.Create("https://isaac.huijiwiki.com/wiki/%E9%81%93%E5%85%B7");
            string webPage = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();

            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(webPage);

            var table = Dfs(html.DocumentNode, d => d.Name == "table" && d.InnerText.StartsWith("名称"));
            if (table == null)
            {
                MessageBox.Show("没有在灰机wiki道具页上发现道具表格，这意味着此工具和wiki不匹配。当前版本的工具已经无法使用。");
                return;
            }

            string desc_dict = "";
            
            foreach (HtmlNode tr in table.ChildNodes)
            {
                if (tr.Name != "tr")
                    continue;
                //skip table head
                if (Dfs(tr, d => d.Name == "th") != null)
                    continue;

                string item_name = "未知";
                string item_desc = "未知";
                int item_id = -1;

                int td_i = 0;
                foreach (HtmlNode td in tr.ChildNodes)
                {
                    if (td.Name != "td")
                        continue;
                    if (td_i == 0)
                    {
                        string chinese = null;
                        foreach (var item in td.ChildNodes)
                        {
                            if (chinese != null)
                                chinese += item.InnerText;
                            else if (item.Name == "br")
                                chinese = "";
                        }
                        item_name = chinese;
                    }
                    if (td_i == 2)
                        item_id = int.Parse(td.InnerText);
                    if (td_i == 5)
                        item_desc = td.InnerText.Replace("&#160;", "");
                    td_i++;
                }

                //Console.WriteLine(item_id + "\t" + item_name + "\t" + item_desc);
                desc_dict += string.Format("[{0}] = \"{1}\",\n", item_id, item_name + "\\n" + item_desc.Replace("\n", "\\n"));
            }

            Console.WriteLine(desc_dict);
            AddPatch(lua_path, desc_dict);
            MessageBox.Show("操作完成");
        }
        private static HtmlNode Dfs(HtmlNode node, Func<HtmlNode, bool> f)
        {
            if (f(node))
                return node;
            foreach (HtmlNode child in node.ChildNodes)
            {
                var target = Dfs(child, f);
                if (target != null)
                    return target;
            }
            return null;
        }

        private static void CancelPatch(string fileName)
        {
            string next = "";
            bool isPatching = false;
            using (FileStream f = new FileStream(fileName, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(f))
                {
                    string line;
                    while((line = reader.ReadLine()) != null)
                    {
                        if (isPatching)
                        {
                            if (line == FAKE_LINE_END)
                                isPatching = false;
                        }
                        else if(line == FAKE_LINE_BEGIN) {
                            isPatching = true;
                        }
                        else
                        {
                            next += line + "\n";
                        }
                    }
                }
            }
            using (FileStream f = new FileStream(fileName, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(f))
                {
                    writer.Write(next);
                }
            }
            //delete fonts
            var dirInfo = new DirectoryInfo(fileName + @"\..\..\wd_font");
            if(dirInfo.Exists)
            {
                foreach(var file in dirInfo.GetFiles())
                {
                    if(new FileInfo(RES_FONT_FOLDER_PATCH + "\\" + file.Name).Exists)
                    {
                        file.Delete();
                    }
                }
            }
            try
            {
                dirInfo.Delete();
            }
            catch (IOException) { }
        }

        private static void AddPatch(string luaName, string item_desc)
        {
            string next = "";
            bool isPatching = false;
            using (FileStream f = new FileStream(luaName, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(f))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (isPatching)
                        {
                            if (line == FAKE_LINE_END)
                                isPatching = false;
                        }
                        else if (line == FAKE_LINE_BEGIN)
                        {
                            isPatching = true;
                        }
                        else
                        {
                            next += line + "\n";
                        }
                    }
                }
            }
            //read lua patch
            string patch = "";
            using (FileStream f = new FileStream(RES_MAIN_LUA_PATCH, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(f))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line == FAKE_DESC_CONTENT)
                            patch += item_desc + "\n";
                        else
                            patch += line + "\n";
                    }
                }
            }
            //patch to next
            next += FAKE_LINE_BEGIN + "\n";
            next += patch + "\n";
            next += FAKE_LINE_END + "\n";

            using (FileStream f = new FileStream(luaName, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(f))
                {
                    writer.Write(next);
                }
            }

            Directory.CreateDirectory(luaName + @"\..\..\wd_font");
            var fontDir = new DirectoryInfo(RES_FONT_FOLDER_PATCH);
            foreach(FileInfo font in fontDir.GetFiles())
            {
                font.CopyTo(luaName + @"\..\..\wd_font\" + font.Name);
            }
        }

    }
}
