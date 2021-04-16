using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
            FAKE_CARD_DESC_CONTENT = "-- FAKE_CARD_CONTENT --",
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
        private static int VERSION = 1;
        private static string USER_AGENT = string.Format("IsaacWikiDicInstaller/{0} (https://gitee.com/frto027/isaac-wiki-dictionary; huiji_wiki_user:Frto027; 602706150@qq.com) .NetFramework/4.7.2", VERSION);


        private static string VersionUrl = "https://frto027.gitee.io/wiki-buffer/version.json";
        private static VersionInfo version = null;

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

            Console.WriteLine("正在下载元数据...");

            HttpWebRequest request = HttpWebRequest.CreateHttp(VersionUrl);
            request.UserAgent = USER_AGENT;
            using (var stream = request.GetResponse().GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    try
                    {
                        version = Newtonsoft.Json.JsonConvert.DeserializeObject<VersionInfo>(reader.ReadToEnd());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            if (version == null)
            {
                MessageBox.Show("元数据下载失败，请检查网络或更新版本。");
                return;
            }
            if (VERSION < version.invalid_last)
            {
                MessageBox.Show("当前版本过旧，已无法使用。请重新到软件发布页面下载新版本。");
                return;
            }

            DicOptions options = new DicOptions() { canceled = false };

            new ConfigForm(options).ShowDialog();

            if (options.canceled)
                return;

            bool getHuijiWikiDesc = options.getHuijiWikiDesc;
            bool useFandomWiki = options.useFandomWikiDesc;


            Dictionary<int, ItemDesc> descs = new Dictionary<int, ItemDesc>();
            Dictionary<int, ItemDesc> trinket_descs = new Dictionary<int, ItemDesc>();
            Dictionary<int, ItemDesc> card_descs = new Dictionary<int, ItemDesc>();
            if (getHuijiWikiDesc)
            {
                LinkedList<ItemDesc> huijiItemDesc = GetHuijiWikiItemDesc();
                LinkedList<ItemDesc> huijiTrinketDesc = GetHuijiWikiTrinketDesc();
                LinkedList<ItemDesc> huijiCardDesc = GetHuijiWikiCardDesc();
                foreach(var item in huijiItemDesc)
                {
                    descs.Add(item.id, item);
                }
                foreach (var item in huijiTrinketDesc)
                {
                    trinket_descs.Add(item.id, item);
                }
                foreach(var item in huijiCardDesc)
                {
                    card_descs.Add(item.id, item);
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
            string card_desc = "";
            foreach (var item in card_descs)
            {
                string d = item.Value.name + "\n" + item.Value.desc;
                d = d.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\"", "\\\"");
                card_desc += string.Format("[{0}]=\"{1}\",\n", item.Value.id, d);
            }
            AddPatch(lua_path, desc_dict, trinket_desc,card_desc, options);
            MessageBox.Show("操作完成");
        }

        private static void DownloadItemDesc(string url,Action beginCallback,Action<int,HtmlNode> onTd, Action<int,string>onText ,Action endCallback)
        {
            HttpWebRequest request = HttpWebRequest.CreateHttp(url);
            request.UserAgent = USER_AGENT;
            string r = null;
            using(var s = request.GetResponse().GetResponseStream())
            {
                using(var sr = new StreamReader(s))
                {
                    try
                    {
                        r = sr.ReadToEnd();
                    }catch(Exception e)
                    {
                        Console.WriteLine(e);
                        MessageBox.Show("网络请求异常");
                        return;
                    }
                }
            }
            ExpandTemplateResult template = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandTemplateResult>(r);
            string wikitext = template.expandtemplates.wikitext;


            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(wikitext);

            var table = Dfs(html.DocumentNode,n=>n.Name == "table");
            if(table != null)
            {
                foreach (HtmlNode tr in table.ChildNodes)
                {
                    if (tr.Name != "tr")
                        continue;
                    //skip table head
                    if (Dfs(tr, d => d.Name == "th") != null)
                        continue;

                    int td_i = 0;
                    beginCallback();
                    foreach (HtmlNode td in tr.ChildNodes)
                    {
                        if (td.Name != "td")
                            continue;
                        onTd(td_i, td);
                        td_i++;
                    }
                    endCallback();
                }
                return;
            }


            //parse as another way

            var splited = wikitext.Split('\n');// Regex.Replace(wikitext, @"\[\[(.*?)(\|.*?)?\]\]", (e) => e.Groups[1].Value).Split('|');
            int index = -1;
            foreach(var str in splited)
            {
                if (str.StartsWith("|- "))
                {
                    beginCallback();
                    index = 0;
                }else if (str.StartsWith("|-"))
                {
                    if(index != 5 && index != 6)
                    {
                        MessageBox.Show("表格解析错误，请更新版本。");
                        return;
                    }
                    endCallback();
                    index = 0;
                }
                else if(str.StartsWith("| "))
                {
                    onText(index, str.Substring(2));
                    index++;
                }
            }
        }

        private static LinkedList<ItemDesc> GetFandomWikiItemDesc()
        {
            var ret = new LinkedList<ItemDesc>();
            Console.WriteLine("正在下载fandom wiki中的道具信息...");

            ItemDesc current = null;
            //翻译wikitext好累啊
            DownloadItemDesc(version.fandomItemUrl, () => { current = new ItemDesc(); },(a,b)=> { }, (id, node) => {
                if (id == 0)
                {
                    string rm_str = node;
                    rm_str = Regex.Replace(rm_str, @"\[\[file:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\[\[category:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\<span.*?\>", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\</span\>", (e) => "");
                    rm_str = rm_str.Replace("<br>", "\n");
                    current.name = Regex.Replace(rm_str, @"\[\[(.*?)(\|(.*?))?\]\]", (e) => e.Groups[3].Value == "" ? e.Groups[1].Value : e.Groups[3].Value).Trim();
                }
                if (id == 1)
                {
                    current.id = int.Parse(node.Substring(node.IndexOf("</span>") + 7));
                }
                if (id == 4)
                {
                    string rm_str = node;
                    rm_str = Regex.Replace(rm_str, @"\[\[file:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\[\[category:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\<span.*?\>", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\</span\>", (e) => "");
                    rm_str = rm_str.Replace("<br>", "\n");
                    current.desc = Regex.Replace(rm_str, @"\[\[(.*?)(\|(.*?))?\]\]", (e) => e.Groups[3].Value == "" ? e.Groups[1].Value : e.Groups[3].Value);
                }
            }, () => {
                ret.AddLast(current);
            });

            return ret;
        }

        private static LinkedList<ItemDesc> GetFandomWikiTrinketDesc()
        {
            var ret = new LinkedList<ItemDesc>();
            Console.WriteLine("正在下载fandom wiki中的饰品信息...");

            ItemDesc current = null;

            DownloadItemDesc(version.fandomTrinketUrl, () => { current = new ItemDesc(); }, (a, b) => { }, (id, node) => {
                if (id == 0)
                {
                    string rm_str = node;
                    rm_str = Regex.Replace(rm_str, @"\[\[file:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\[\[category:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\<span.*?\>", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\</span\>", (e) => "");
                    rm_str = rm_str.Replace("<br>", "\n");
                    current.name = Regex.Replace(rm_str, @"\[\[(.*?)(\|(.*?))?\]\]", (e) => e.Groups[3].Value == "" ? e.Groups[1].Value : e.Groups[3].Value).Trim();
                }
                if (id == 1)
                {
                    current.id = int.Parse(node.Substring(node.IndexOf("</span>") + 7));
                }
                if (id == 4)
                {
                    string rm_str = node;
                    rm_str = Regex.Replace(rm_str, @"\[\[file:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\[\[category:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\<span.*?\>", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\</span\>", (e) => "");
                    rm_str = rm_str.Replace("<br>", "\n");
                    current.desc = Regex.Replace(rm_str, @"\[\[(.*?)(\|(.*?))?\]\]", (e) => e.Groups[3].Value == "" ? e.Groups[1].Value : e.Groups[3].Value);
                }
            }, () => {
                ret.AddLast(current);
            });

            return ret;
        }

        public static LinkedList<ItemDesc> GetHuijiWikiItemDesc()
        {
            var ret = new LinkedList<ItemDesc>();
            Console.WriteLine("正在下载灰机wiki中的道具信息...");

            ItemDesc current = null;
            //教科书般的lambda表达式
            DownloadItemDesc(version.huijiItemUrl,()=> { current = new ItemDesc(); }, (id, node) => {
                if(id == 0)
                {
                    current.name = Regex.Replace(node.LastChild.InnerText, @"\[\[(.*?)(\|.*?)?\]\]", (e) => e.Groups[1].Value);
                }
                if(id == 2)
                {
                    current.id = int.Parse(node.InnerText);
                }
                if(id == 5)
                {
                    string s_remove_file = Regex.Replace(node.InnerText, @"\[\[文件(.*?)(\|.*?)?\]\]", (e) => "").Replace("&nbsp;","");
                    current.desc = Regex.Replace(s_remove_file, @"\[\[(.*?)(\|.*?)?\]\]", (e) => e.Groups[1].Value);
                }
            },(a, b) => { },() => {
                ret.AddLast(current);
            });

            return ret;
        }

        public static LinkedList<ItemDesc> GetHuijiWikiTrinketDesc()
        {
            var ret = new LinkedList<ItemDesc>();
            Console.WriteLine("正在下载灰机wiki中的饰品信息...");

            ItemDesc current = null;
            //教科书般的lambda表达式
            DownloadItemDesc(version.huijiTrinketUrl, () => { current = new ItemDesc(); }, (id, node) => {
                if (id == 0)
                {
                    current.name = Regex.Replace(node.LastChild.InnerText, @"\[\[(.*?)(\|(.*?))?\]\]", (e) => e.Groups[3].Value == "" ? e.Groups[1].Value : e.Groups[3].Value);
                }
                if (id == 2)
                {
                    current.id = int.Parse(node.InnerText);
                }
                if (id == 5)
                {
                    string s_remove_file = Regex.Replace(node.InnerText, @"\[\[文件(.*?)(\|.*?)?\]\]", (e) => "").Replace("&nbsp;", "");
                    current.desc = Regex.Replace(s_remove_file, @"\[\[(.*?)(\|(.*?))?\]\]", (e) => e.Groups[3].Value == "" ? e.Groups[1].Value : e.Groups[3].Value);
                }
            }, (a, b) => { }, () => {
                ret.AddLast(current);
            });

            return ret;
        }
        public static LinkedList<ItemDesc> GetHuijiWikiCardDesc()
        {
            var ret = new LinkedList<ItemDesc>();
            Console.WriteLine("正在下载灰机wiki中的卡牌信息...");

            ItemDesc current = null;
            //教科书般的lambda表达式
            DownloadItemDesc(version.huijiCardUrl, () => { current = new ItemDesc(); }, (id, node) => {
                if (id == 0)
                {
                    current.name = Regex.Replace(node.LastChild.InnerText, @"\[\[(.*?)(\|(.*?))?\]\]", (e) => e.Groups[3].Value == "" ? e.Groups[1].Value : e.Groups[3].Value);
                }
                if (id == 2)
                {
                    current.id = int.Parse(node.InnerText);
                }
                if (id == 5)
                {
                    string s_remove_file = Regex.Replace(node.InnerText, @"\[\[文件(.*?)(\|.*?)?\]\]", (e) => "").Replace("&nbsp;", "");
                    current.desc = Regex.Replace(s_remove_file, @"\[\[(.*?)(\|(.*?))?\]\]", (e) => e.Groups[3].Value == "" ? e.Groups[1].Value : e.Groups[3].Value);
                }
            }, (a, b) => { }, () => {
                ret.AddLast(current);
            });

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

        private static void AddPatch(string luaName, string item_desc, string trinket_desc,string card_desc, DicOptions dicOptions)
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
                        }else if(line == FAKE_CARD_DESC_CONTENT)
                        {
                            patch += card_desc + "\n";
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
