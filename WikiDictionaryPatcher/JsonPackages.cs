using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiDictionaryPatcher
{
    public class VersionInfo
    {
        public int invalid_last;
        public string
            huijiItemUrl, huijiTrinketUrl,huijiCardUrl, huijiPillUrl, fandomItemUrl, fandomTrinketUrl;
        public string huijiUrlPrefix;
    }

    public class ExpandTemplateResult
    {
        public ExpandTemplateInfo expandtemplates;
    }
    public class ExpandTemplateInfo
    {
        public string wikitext;
    }
}
