namespace Analysys {
    /// <summary>
    /// 用到的正则表达式集合
    /// </summary>
    public class RegexString {
        //xwhen 效验
        public const string REG_XWHEN = @"^\s*[1-9][0-9]{12}";
        public const string REG_HTTP_URL = @"^\s*[Hh][Tt][Tt][Pp].+";
    }
}