﻿#if DEBUG
//#define USE_DIRECT_WIKI_ACCESS
#endif

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
        public bool use_player_pos, draw_mouse, use_half_size_font;
        public bool use_bigger_font;
        public bool use_dx_16_font, use_dx_12_font, use_st_16_font, use_st_12_font,use_st_10_font, use_default_font;
    }

    class Program
    {
        private static string
            FAKE_LINE_BEGIN = "-- WikiDict MARK START --",
            FAKE_DESC_CONTENT = "-- FAKE_DESC_CONTENT --",
            FAKE_TRINKET_DESC_CONTENT = "-- FAKE_TRINKET_CONTENT --",
            FAKE_CARD_DESC_CONTENT = "-- FAKE_CARD_CONTENT --",
            FAKE_PILL_DESC_CONTENT = "-- FAKE_PILL_CONTENT --",
            FAKE_CONFIG_SEG_1 = "-- FAKE_CONFIG_SEG_1 --",
            FAKE_LINE_END = "-- WikiDict MARK END --";
        //this is file version
        private static string[] Versions =
        {
            "当前版本文件结构发生变化，请使用1.0.14及以前版本对游戏中的图鉴进行卸载","-- WIKIDIC_VERSION_1 --" //,"请使用xxx版本卸载","__LATEST_VERSION__"
        };

#if DEBUG
        private static string
            RES_MAIN_LUA_PATCH = @"..\..\..\patch.lua",
            RES_FONT_FOLDER_PATCH = @"..\..\..\wd_font";
#else
        private static string
            RES_MAIN_LUA_PATCH = @"patch.lua",
            RES_FONT_FOLDER_PATCH = "wd_font";
#endif
        //this is network version, instead of file version
        private static int VERSION = 2;
        private static string USER_AGENT = string.Format("IsaacWikiDicInstaller/{0} (https://gitee.com/frto027/isaac-wiki-dictionary; huiji_wiki_user:Frto027; 602706150@qq.com) .NetFramework/4.7.2", VERSION);


        private static string VersionUrl = "https://frto027.gitee.io/wiki-buffer/version.json";
        private static VersionInfo version = null;

        private static string GetVersionUninstallTip(string s)
        {
            System.Diagnostics.Debug.Assert(Versions.Length % 2 == 0, "Versions needs to be odd");
            for(int i=Versions.Length - 1;i >= 0;i-= 2)
            {
                if (s.Contains(Versions[i]))
                {
                    return i == Versions.Length - 1 ? null /* current version */ : Versions[i + 1]/* formal version */;
                }
            }
            return Versions[0];
        }

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
            string lua_text;
            using (FileStream f = new FileStream(lua_path, FileMode.Open))
            {
                using(StreamReader reader = new StreamReader(f))
                {
                    lua_text = reader.ReadToEnd();
                    patched = lua_text.Contains(FAKE_LINE_BEGIN) && lua_text.Contains(FAKE_LINE_END);
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
                    string errorStr = GetVersionUninstallTip(lua_text);
                    if(errorStr != null)
                    {
                        MessageBox.Show(errorStr);
                        return;
                    }
                    CancelPatch(lua_path);
                    MessageBox.Show("图鉴已经移除");
                    return;
                }
            }

            Console.WriteLine("正在下载元数据...");
#if USE_DIRECT_WIKI_ACCESS
#if !DEBUG
            Console.WriteLine("禁止以直接WIKI访问进行Release发布！");
            return 0;
#endif
            version = GetDirectVersion();
            MessageBox.Show("当前信息直接来自无缓存wiki，仅作为调试使用，为了更好的维护WIKI生态，请不要编译发布此版本哦");
#else
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
#endif
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
            Dictionary<int, ItemDesc> pill_descs = new Dictionary<int, ItemDesc>();
            if (getHuijiWikiDesc)
            {
                LinkedList<ItemDesc> huijiItemDesc = GetHuijiWikiItemDesc();
                LinkedList<ItemDesc> huijiTrinketDesc = GetHuijiWikiTrinketDesc();
                LinkedList<ItemDesc> huijiCardDesc = GetHuijiWikiCardDesc();
                LinkedList<ItemDesc> huijiPillDesc = GetHuijiWikiPillDesc();
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
                foreach(var item in huijiPillDesc)
                {
                    pill_descs.Add(item.id, item);
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
            string pill_desc = "";
            foreach (var item in pill_descs)
            {
                string d = item.Value.name + "\n" + item.Value.desc;
                d = d.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\"", "\\\"");
                pill_desc += string.Format("[{0}]=\"{1}\",\n", item.Value.id, d);
            }
            AddPatch(lua_path, desc_dict, trinket_desc,card_desc,pill_desc, options);
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
                    rm_str = rm_str.Substring(rm_str.IndexOf("| ")+2);

                    //file:dlc anr indicator.png Added in Repetance ... -> \n[Added in Repetance]
                    //rm_str = Regex.Replace(rm_str, @"\[\[file:dlc [anr]+ indicator\.png\|[^|]+\|[^|]+\|([^\]]+)\]\]", e => "[" + e.Groups[1].Value + "]");
                    rm_str = Regex.Replace(rm_str, @"\[\[file:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\[\[category:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\<span.*?\>", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\</span\>", (e) => "");
                    rm_str = rm_str.Replace("<br>", "\n");
                    rm_str = Regex.Replace(rm_str, @"\n+", e => "", RegexOptions.Multiline);
                    current.name = Regex.Replace(rm_str, @"\[\[(.*?)(\|(.*?))?\]\]", (e) => e.Groups[3].Value == "" ? e.Groups[1].Value : e.Groups[3].Value).Trim();
                }
                if (id == 1)
                {
                    current.id = int.Parse(node.Substring(node.IndexOf("</span>") + 7));
                }
                if (id == 4)
                {
                    string rm_str = node;
                    rm_str = Regex.Replace(rm_str, @"\[\[file:dlc [anr]+ indicator\.png\|[^|]+\|[^|]+\|([^\]]+)\]\]", e => "\n[" + e.Groups[1].Value + "]");
                    rm_str = Regex.Replace(rm_str, @"\[\[file:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\[\[category:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\<span.*?\>", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\</span\>", (e) => "");
                    rm_str = rm_str.Replace("<br>", "\n");
                    rm_str = Regex.Replace(rm_str, @"\n+", e => "\n",RegexOptions.Multiline);
                    rm_str = rm_str.TrimStart('\n');
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
                    rm_str = rm_str.Substring(rm_str.IndexOf("| ") + 2);
                    //rm_str = Regex.Replace(rm_str, @"\[\[file:dlc [anr]+ indicator\.png\|[^|]+\|[^|]+\|([^\]]+)\]\]", e => "[" + e.Groups[1].Value + "]");
                    rm_str = Regex.Replace(rm_str, @"\[\[file:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\[\[category:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\<span.*?\>", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\</span\>", (e) => "");
                    rm_str = rm_str.Replace("<br>", "\n");
                    rm_str = rm_str.Replace("<br>", "\n");
                    rm_str = Regex.Replace(rm_str, @"\n+", e => "", RegexOptions.Multiline);
                    current.name = Regex.Replace(rm_str, @"\[\[(.*?)(\|(.*?))?\]\]", (e) => e.Groups[3].Value == "" ? e.Groups[1].Value : e.Groups[3].Value).Trim();
                }
                if (id == 1)
                {
                    current.id = int.Parse(node.Substring(node.IndexOf("</span>") + 7));
                }
                if (id == 4)
                {
                    string rm_str = node;
                    rm_str = Regex.Replace(rm_str, @"\[\[file:dlc [anr]+ indicator\.png\|[^|]+\|[^|]+\|([^\]]+)\]\]", e => "[" + e.Groups[1].Value + "]");
                    rm_str = Regex.Replace(rm_str, @"\[\[file:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\[\[category:.*?\]\]", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\<span.*?\>", (e) => "");
                    rm_str = Regex.Replace(rm_str, @"\</span\>", (e) => "");
                    rm_str = rm_str.Replace("<br>", "\n");
                    rm_str = Regex.Replace(rm_str, @"\n+", e => "\n", RegexOptions.Multiline);
                    rm_str = rm_str.TrimStart('\n');
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
                if(id == 6)
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
                if (id == 6)
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
                if (id == 6)
                {
                    string s_remove_file = Regex.Replace(node.InnerText, @"\[\[文件(.*?)(\|.*?)?\]\]", (e) => "").Replace("&nbsp;", "");
                    current.desc = Regex.Replace(s_remove_file, @"\[\[(.*?)(\|(.*?))?\]\]", (e) => e.Groups[3].Value == "" ? e.Groups[1].Value : e.Groups[3].Value);
                }
            }, (a, b) => { }, () => {
                ret.AddLast(current);
            });

            return ret;
        }
        public static LinkedList<ItemDesc> GetHuijiWikiPillDesc()
        {
            var ret = new LinkedList<ItemDesc>();
            Console.WriteLine("正在下载灰机wiki中的药丸信息...");

            ItemDesc current = null;
            //教科书般的lambda表达式
            DownloadItemDesc(version.huijiPillUrl, () => { current = new ItemDesc(); }, (id, node) => {
                if (id == 0)
                {
                    current.name = Regex.Replace(node.LastChild.InnerText, @"\[\[(.*?)(\|(.*?))?\]\]", (e) => e.Groups[3].Value == "" ? e.Groups[1].Value : e.Groups[3].Value);
                }
                if (id == 2)
                {
                    current.id = int.Parse(node.InnerText);
                }
                if (id == 6)
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

        private static void AddPatch(string luaName, string item_desc, string trinket_desc,string card_desc, string pill_desc, DicOptions dicOptions)
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
                        else if (line == FAKE_CONFIG_SEG_1)
                        {
                            patch += "WikiDic.useHuijiWiki = " + (dicOptions.getHuijiWikiDesc ? "true" : "false") + "\n";
                            patch += "WikiDic.useFandomWiki = " + (dicOptions.useFandomWikiDesc ? "true" : "false") + "\n";
                            patch += "WikiDic.usePlayerPos = " + (dicOptions.use_player_pos ? "true" : "false") + "\n";
                            patch += "WikiDic.drawMouse = " + (dicOptions.draw_mouse ? "true" : "false") + "\n";
                            patch += "WikiDic.useDefaultFont = " + (dicOptions.use_default_font ? "true" : "false") + "\n";
                            patch += "WikiDic.useHalfSizeFont = " + (dicOptions.use_half_size_font ? "true" : "false") + "\n";
                            patch += "WikiDic.useBiggerSizeFont = " + (dicOptions.use_bigger_font ? "true" : "false") + "\n";
                        }
                        else if (line == FAKE_TRINKET_DESC_CONTENT)
                        {
                            patch += trinket_desc + "\n";
                        }
                        else if (line == FAKE_CARD_DESC_CONTENT)
                        {
                            patch += card_desc + "\n";
                        }
                        else if (line == FAKE_PILL_DESC_CONTENT) {
                            patch += pill_desc + "\n";
                        }
                        else
                            patch += line + "\n";
                    }
                }
            }
            //patch to next
            next += FAKE_LINE_BEGIN + "\n";
            next += Versions[Versions.Length - 1] + "\n";
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
                string fntName = "st_wdic";
                if (dicOptions.use_st_12_font)
                    fntName = "st_wdic";
                else if (dicOptions.use_st_10_font)
                    fntName = "st10_wdic";
                else if (dicOptions.use_st_16_font)
                    fntName = "st16_wdic";
                else if (dicOptions.use_dx_16_font)
                    fntName = "dx16_wdic";
                else if (dicOptions.use_dx_12_font)
                    fntName = "dx12_wdic";
                //copy font
                Directory.CreateDirectory(luaName + @"\..\..\wd_font");
                var fontDir = new DirectoryInfo(RES_FONT_FOLDER_PATCH);
                foreach (FileInfo font in fontDir.GetFiles())
                {
                    if (!font.Name.StartsWith(fntName))
                        continue;
                    string target_name = font.Name;
                    if (target_name.EndsWith(".fnt"))
                        target_name = "wdic_font.fnt";
                    try
                    {
                        font.CopyTo(luaName + @"\..\..\wd_font\" + target_name);
                    }
                    catch (Exception e) { }
                }

            }
        }

#if DEBUG
        private static VersionInfo GetDirectVersion()
        {
            return new VersionInfo()
            {
                invalid_last = VERSION,
                huijiItemUrl = @"https://isaac.huijiwiki.com/api.php?action=expandtemplates&prop=wikitext&format=json&text={{道具查询|条件=[[Type::道具]]}}",
                huijiTrinketUrl = @"https://isaac.huijiwiki.com/api.php?action=expandtemplates&prop=wikitext&format=json&text={{道具查询|条件=[[Type::饰品]]}}",
                huijiCardUrl = @"https://isaac.huijiwiki.com/api.php?action=expandtemplates&prop=wikitext&format=json&text={{道具查询|条件=[[Type::卡牌]]}}",
                huijiPillUrl = @"https://isaac.huijiwiki.com/api.php?action=expandtemplates&prop=wikitext&format=json&text={{道具查询|条件=[[Type::药丸]]}}",
                fandomItemUrl = @"https://bindingofisaacrebirth.fandom.com/api.php?action=expandtemplates&format=json&prop=wikitext&text={{collectible table}}",
                fandomTrinketUrl = @"https://bindingofisaacrebirth.fandom.com/api.php?action=expandtemplates&format=json&prop=wikitext&text={{trinket table}}",
            };
        }
#endif
    }
}
