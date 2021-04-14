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
    class ItemDesc
    {
        public int id;
        public string name;
        public string desc;
    }

    public class DicOptions
    {
        public bool canceled;
        public bool getHuijiWikiDesc, useFandomWikiDesc;
        public bool use_player_pos, draw_mouse, use_default_font, use_half_size_font;
        public bool use_bigger_font;
    }

    class Program
    {
        private static string
            FAKE_LINE_BEGIN = "-- WikiDict MARK START --",
            FAKE_DESC_CONTENT = "-- FAKE_DESC_CONTENT --",
            FAKE_TRINKET_DESC_CONTENT = "-- FAKE_TRINKET_CONTENT --",
            FAKE_CONFIG_SEG_1 = "-- FAKE_CONFIG_SEG_1 --",
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
            MessageBox.Show("此版本图鉴数据来源(致谢)：\n灰机wiki(https://isaac.huijiwiki.com/)\nBinding of Isaac: Rebirth Wiki is a Fandom Gaming Community(https://bindingofisaacrebirth.fandom.com/)");
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
            DicOptions options = new DicOptions() { canceled = false };

            new ConfigForm(options).ShowDialog();

            if (options.canceled)
                return;
            /*
            if (MessageBox.Show("是否下载wiki信息并添加游戏图鉴？","询问",MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            bool getHuijiWikiDesc = MessageBox.Show("是否下载灰机wiki信息？（点否将下载英文维基fandomWiki上的内容）", "询问", MessageBoxButtons.YesNo) == DialogResult.Yes;
            bool useFandomWiki = true;
            if (getHuijiWikiDesc)
            {
                useFandomWiki = MessageBox.Show("是否下载英文维基fandomWiki上的内容，作为补充内容？", "询问", MessageBoxButtons.YesNo) == DialogResult.Yes;
            }
            */

            bool getHuijiWikiDesc = options.getHuijiWikiDesc;
            bool useFandomWiki = options.useFandomWikiDesc;


            Dictionary<int, ItemDesc> descs = new Dictionary<int, ItemDesc>();
            Dictionary<int, ItemDesc> trinket_descs = new Dictionary<int, ItemDesc>();
            if (getHuijiWikiDesc)
            {
                LinkedList<ItemDesc> huijiItemDesc = GetHuijiWikiItemDesc();
                LinkedList<ItemDesc> huijiTrinketDesc = GetHuijiWikiTrinketDesc();

                foreach(var item in huijiItemDesc)
                {
                    descs.Add(item.id, item);
                }
                foreach (var item in huijiTrinketDesc)
                {
                    trinket_descs.Add(item.id, item);
                }
            }


            if (useFandomWiki)
            {
                LinkedList<ItemDesc> fandomDesc = GetFandomWikiItemDesc();
                LinkedList<ItemDesc> fandomTrinketDescs = GetFandomWikiTrinketDesc();

                foreach (var item in fandomDesc)
                {
                    if (!descs.ContainsKey(item.id))
                        descs.Add(item.id, item);
                    else
                    {
                        if (descs[item.id].desc == null || descs[item.id].desc == "")
                            descs[item.id].desc = item.desc;
                    }
                }
                foreach (var item in fandomTrinketDescs)
                {
                    if (!trinket_descs.ContainsKey(item.id))
                        trinket_descs.Add(item.id, item);
                    else
                    {
                        if (trinket_descs[item.id].desc == null || trinket_descs[item.id].desc == "")
                            trinket_descs[item.id].desc = item.desc;
                    }
                }
            }

            string desc_dict = "";
            foreach(var item in descs)
            {
                string d = item.Value.name + "\n" + item.Value.desc;
                d = d.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\"", "\\\"");
                desc_dict += string.Format("[{0}]=\"{1}\",\n", item.Value.id, d);
            }
            string trinket_desc = "";
            foreach (var item in trinket_descs)
            {
                string d = item.Value.name + "\n" + item.Value.desc;
                d = d.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\"", "\\\"");
                trinket_desc += string.Format("[{0}]=\"{1}\",\n", item.Value.id, d);
            }
            //=======trinket

            /*
            bool use_player_pos =
                MessageBox.Show("点击“是”显示玩家最近的道具，点击“否”使用鼠标拾取道具。", "道具选择方式？", MessageBoxButtons.YesNo) == DialogResult.Yes;
            bool mouse_cursor = false;
            if (!use_player_pos)
            {
                mouse_cursor = MessageBox.Show("是否要在屏幕上显示一个鼠标指针，以方便全屏时观察鼠标位置？", "询问", MessageBoxButtons.YesNo) == DialogResult.Yes;
            }
            bool use_default_font = false;
            if(!getHuijiWikiDesc && useFandomWiki)
            {
                use_default_font = MessageBox.Show("您选择了只使用英文维基，是否要使用游戏默认的英文字体呢？如果点击是，则使用游戏的默认字体，点击否，则使用图鉴程序自带的等线中文字体。","字体询问",MessageBoxButtons.YesNo) == DialogResult.Yes;
            }

            bool use_half_size_font = MessageBox.Show("是否使用全尺寸字体？点否将使得图鉴信息变为一半大小进行显示(半尺寸大小显示时，请关闭游戏的FILTER/滤光器选项，以免字体被平滑，影响辨识度)。", "尺寸询问？", MessageBoxButtons.YesNo) == DialogResult.No;
            */


            AddPatch(lua_path, desc_dict, trinket_desc, options);
            MessageBox.Show("操作完成");
        }

        private static LinkedList<ItemDesc> GetFandomWikiItemDesc()
        {
            Console.WriteLine("正在下载fandom wiki中的道具信息...");
            WebRequest request = HttpWebRequest.Create("http://frto027.gitee.io/wiki-buffer/fandom-item.html");
            string webPage = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();

            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(webPage);

            var tables = new LinkedList<HtmlNode>();

            Dfs(html.DocumentNode, d =>
            {
                if (d.Name == "table" && d.InnerText.Substring(0, 20).Contains("Name"))
                    tables.AddLast(d);
                return false;
            });
            var ret = new LinkedList<ItemDesc>();
            foreach(var table in tables)
            {
                foreach (HtmlNode tr in Dfs(table,n=>n.Name == "tbody")?.ChildNodes)
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
                            item_name = td.InnerText.Replace("\n"," ");
                        if (td_i == 1)
                            item_id = int.Parse(td.InnerText.Replace("\n","").Substring("5.100.".Length));
                        if (td_i == 4)
                            item_desc = td.InnerText.Replace("&#160;", "");
                        td_i++;
                    }

                    ret.AddLast(new ItemDesc() { id = item_id, name = item_name, desc = item_desc });
                    //Console.WriteLine(item_id + "\t" + item_name + "\t" + item_desc);
                    Console.WriteLine(string.Format("[{0}] = \"{1}\",", item_id, item_name + "\\n" + item_desc.Replace("\n", "\\n")));
                }
            }
            return ret;
        }
        private static LinkedList<ItemDesc> GetFandomWikiTrinketDesc()
        {
            Console.WriteLine("正在下载fandom wiki中的饰品信息...");
            WebRequest request = HttpWebRequest.Create("http://frto027.gitee.io/wiki-buffer/fandom-trinket.html");
            string webPage = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();

            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(webPage);

            var tables = new LinkedList<HtmlNode>();

            Dfs(html.DocumentNode, d =>
            {
                if (d.Name == "table" && d.InnerText.Substring(0, 20).Contains("Name"))
                    tables.AddLast(d);
                return false;
            });
            var ret = new LinkedList<ItemDesc>();
            foreach (var table in tables)
            {
                foreach (HtmlNode tr in Dfs(table, n => n.Name == "tbody")?.ChildNodes)
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
                            item_name = td.InnerText.Replace("\n", " ");
                        if (td_i == 1)
                            item_id = int.Parse(td.InnerText.Replace("\n", "").Substring("5.350.".Length));
                        if (td_i == 4)
                            item_desc = td.InnerText.Replace("&#160;", "");
                        td_i++;
                    }

                    ret.AddLast(new ItemDesc() { id = item_id, name = item_name, desc = item_desc });
                    //Console.WriteLine(item_id + "\t" + item_name + "\t" + item_desc);
                    Console.WriteLine(string.Format("[{0}] = \"{1}\",", item_id, item_name + "\\n" + item_desc.Replace("\n", "\\n")));
                }
            }
            return ret;
        }
        public static LinkedList<ItemDesc> GetHuijiWikiItemDesc()
        {
            var ret = new LinkedList<ItemDesc>();
            Console.WriteLine("正在下载灰机wiki中的道具信息...");
            WebRequest request = HttpWebRequest.Create("http://frto027.gitee.io/wiki-buffer/huiji-item.html");
            string webPage = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();

            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(webPage);

            var table = Dfs(html.DocumentNode, d => d.Name == "table" && d.InnerText.StartsWith("名称"));
            if (table == null)
            {
                MessageBox.Show("没有在灰机wiki道具页上发现饰品表格，这意味着此工具和wiki不匹配。当前版本的工具已经无法使用。");
                return null;
            }

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

                ret.AddLast(new ItemDesc() { id = item_id, name = item_name, desc = item_desc });
                //Console.WriteLine(item_id + "\t" + item_name + "\t" + item_desc);
                Console.WriteLine(string.Format("[{0}] = \"{1}\",", item_id, item_name + "\\n" + item_desc.Replace("\n", "\\n")));
            }
            return ret;
        }
        public static LinkedList<ItemDesc> GetHuijiWikiTrinketDesc()
        {
            var ret = new LinkedList<ItemDesc>();
            Console.WriteLine("正在下载灰机wiki中的饰品信息...");
            WebRequest request = HttpWebRequest.Create("http://frto027.gitee.io/wiki-buffer/huiji-trinket.html");
            string webPage = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();

            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(webPage);

            var table = Dfs(html.DocumentNode, d => d.Name == "table" && d.InnerText.StartsWith("名称"));
            if (table == null)
            {
                MessageBox.Show("没有在灰机wiki道具页上发现道具表格，这意味着此工具和wiki不匹配。当前版本的工具已经无法使用。");
                return null;
            }

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

                ret.AddLast(new ItemDesc() { id = item_id, name = item_name, desc = item_desc });
                //Console.WriteLine(item_id + "\t" + item_name + "\t" + item_desc);
                Console.WriteLine(string.Format("[{0}] = \"{1}\",", item_id, item_name + "\\n" + item_desc.Replace("\n", "\\n")));
            }
            return ret;
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

        private static void AddPatch(string luaName, string item_desc, string trinket_desc, DicOptions dicOptions)
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
                        else if (line == FAKE_CONFIG_SEG_1) {
                            patch += "WikiDic.useHuijiWiki = " + (dicOptions.getHuijiWikiDesc ? "true" : "false") + "\n";
                            patch += "WikiDic.useFandomWiki = " + (dicOptions.useFandomWikiDesc ? "true" : "false") + "\n";
                            patch += "WikiDic.usePlayerPos = " + (dicOptions.use_player_pos ? "true" : "false") + "\n";
                            patch += "WikiDic.drawMouse = " + (dicOptions.draw_mouse ? "true" : "false") + "\n";
                            patch += "WikiDic.useDefaultFont = " + (dicOptions.use_default_font ? "true" : "false") + "\n";
                            patch += "WikiDic.useHalfSizeFont = " + (dicOptions.use_half_size_font ? "true" : "false") + "\n";
                            patch += "WikiDic.useBiggerSizeFont = " + (dicOptions.use_bigger_font ? "true" : "false") + "\n";
                        }
                        else if(line == FAKE_TRINKET_DESC_CONTENT)
                        {
                            patch += trinket_desc + "\n";
                        }else
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
            //patch font
            if (!dicOptions.use_default_font)
            {
                Directory.CreateDirectory(luaName + @"\..\..\wd_font");
                var fontDir = new DirectoryInfo(RES_FONT_FOLDER_PATCH);
                foreach (FileInfo font in fontDir.GetFiles())
                {
                    font.CopyTo(luaName + @"\..\..\wd_font\" + font.Name);
                }
            }
        }

    }
}
