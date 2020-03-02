using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Analysys
{
    public class AnalysysDotNetSdk
    {
        private string SDK_VERSION = "4.3.0";
        private ICollecter collecter;
        private string appId;
        private bool autoDelParam;
        private Dictionary<string, object> egBaseProperties;
        private Dictionary<string, object> xcontextSuperProperties;
        private DEBUG debugMode = (int) DEBUG.CLOSE;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="collecter">消息收集器</param>
        /// <param name="appId">用户AppId</param>
        public AnalysysDotNetSdk(ICollecter collecter, string appId) : this(collecter, appId, false)
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="collecter">消息收集器</param>
        /// <param name="appId">用户AppId</param>
        /// <param name="autoDelParam">是否自动删除校验不通过的属性 </param>
        public AnalysysDotNetSdk(ICollecter collecter, string appId, bool autoDelParam)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new Exception("appId is empty");
            }

            this.collecter = collecter ?? throw new Exception("collecter is null");
            this.appId = appId;
            this.egBaseProperties = new Dictionary<string, object>(3);
            this.xcontextSuperProperties = new Dictionary<string, object>();
            this.autoDelParam = autoDelParam;
            ValidHandle.SetDelNotValidParam(autoDelParam);
            InitBaseProperties();
        }

        public void SetDebugMode(DEBUG debug)
        {
            this.debugMode = debug;
        }

        private bool IsDebug()
        {
            switch (debugMode)
            {
                case DEBUG.OPENNOSAVE:
                case DEBUG.OPENANDSAVE:
                    return true;
                default:
                    return false;
            }
        }

        public void InitBaseProperties()
        {
            this.egBaseProperties.Clear();
            this.egBaseProperties.Add("$lib", PlatForm.DotNet.GetEnumDescription());
            this.egBaseProperties.Add("$lib_version", SDK_VERSION);
        }

        public void RegisterSuperProperties(Dictionary<string, object> superProperties)
        {
            int maxNum = 100;
            if (superProperties.Count > maxNum)
                Console.WriteLine("Too many super properties. max number is 100.");
            ValidHandle.CheckParam("", superProperties);
            IEnumerator<KeyValuePair<string, object>> dem = superProperties.GetEnumerator();
            while (dem.MoveNext())
            {
                this.xcontextSuperProperties.Add(dem.Current.Key, dem.Current.Value);
            }

            if (IsDebug())
            {
                Console.WriteLine("RegisterSuperProperties success");
            }
        }

        public void RegisterSuperProperty(string key, string value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (IsDebug())
                {
                    Console.WriteLine("RegisterSuperProperty Key cannot be null.");
                }

                return;
            }

            Dictionary<string, object> superProperties = new Dictionary<string, object>();
            superProperties.Add(key, value);
            RegisterSuperProperties(superProperties);
        }

        public void UnRegisterSuperProperty(string key)
        {
            if (this.xcontextSuperProperties.ContainsKey(key))
            {
                this.xcontextSuperProperties.Remove(key);
            }

            if (IsDebug())
            {
                Console.WriteLine(string.Format("UnRegisterSuperProperty Key[{0}] success", key));
            }
        }

        public object GetSuperProperty(string key)
        {
            if (this.xcontextSuperProperties.ContainsKey(key))
            {
                bool flag = this.xcontextSuperProperties.TryGetValue(key, out object superPropertie);
                Console.WriteLine(string.Format("GetSuperProperty: key{0},value{1}", key,
                    flag ? superPropertie.ToString() : ""));
                return flag ? superPropertie : null;
            }

            Console.WriteLine("GetSuperProperty : SuperProperties not Contain Key" + key);
            return null;
        }

        public Dictionary<string, object> GetSuperProperties()
        {
            if (this.xcontextSuperProperties == null || this.xcontextSuperProperties.Count == 0)
            {
                Console.WriteLine("GetSuperProperties: GetSuperProperties is null or count is 0.");
            }

            foreach (KeyValuePair<string, object> property in this.xcontextSuperProperties)
            {
                Console.WriteLine(string.Format("GetSuperProperties ,key:{0},value:{1}", property.Key,
                    property.Value.ToString()));
            }

            return this.xcontextSuperProperties;
        }

        public void ClearSuperProperties()
        {
            this.xcontextSuperProperties.Clear();
            if (IsDebug())
            {
                Console.WriteLine("ClearSuperProperties success");
            }
        }

        public void Flush()
        {
            this.collecter.Flush();
        }

        public void Shutdown()
        {
            this.collecter.Close();
        }

        /// <summary>
        /// 设置用户的属性 </summary>
        /// <param name="distinctId"> 用户ID </param>
        /// <param name="isLogin"> 用户ID是否是登录 ID </param>
        /// <param name="properties"> 用户属性 </param>
        /// <param name="platform"> 平台类型 </param>
        public void ProfileSet(string distinctId, bool isLogin, Dictionary<string, object> properties,
            string platform)
        {
            Upload(distinctId, isLogin, EventName.P_SET.GetEnumDescription(), properties, platform, null);
        }

        /// <summary>
        /// 设置用户的属性 </summary>
        /// <param name="distinctId"> 用户ID </param>
        /// <param name="isLogin"> 用户ID是否是登录 ID </param>
        /// <param name="properties"> 用户属性 </param>
        /// <param name="platform"> 平台类型 </param>
        public void ProfileSet(string distinctId, bool isLogin, Dictionary<string, object> properties,
            string platform, string xwhen)
        {
            Upload(distinctId, isLogin, EventName.P_SET.GetEnumDescription(), properties, platform, xwhen);
        }

        /// <summary>
        /// 首次设置用户的属性,该属性只在首次设置时有效 </summary>
        /// <param name="distinctId"> 用户ID </param>
        /// <param name="isLogin"> 用户ID是否是登录 ID </param>
        /// <param name="properties"> 用户属性 </param>
        /// <param name="platform"> 平台类型 </param>
        public void ProfileSetOnce(string distinctId, bool isLogin, Dictionary<string, object> properties,
            string platform, string xwhen)
        {
            Upload(distinctId, isLogin, EventName.P_SET_ONE.GetEnumDescription(), properties, platform, xwhen);
        }

        /// <summary>
        /// 首次设置用户的属性,该属性只在首次设置时有效 </summary>
        /// <param name="distinctId"> 用户ID </param>
        /// <param name="isLogin"> 用户ID是否是登录 ID </param>
        /// <param name="properties"> 用户属性 </param>
        /// <param name="platform"> 平台类型 </param>
        public void ProfileSetOnce(string distinctId, bool isLogin, Dictionary<string, object> properties,
            string platform)
        {
            Upload(distinctId, isLogin, EventName.P_SET_ONE.GetEnumDescription(), properties, platform, null);
        }

        /// <summary>
        /// 为用户的一个或多个数值类型的属性累加一个数值 </summary>
        /// <param name="distinctId"> 用户ID </param>
        /// <param name="isLogin"> 用户ID是否是登录 ID </param>
        /// <param name="properties"> 用户属性 </param>
        /// <param name="platform"> 平台类型 </param>
        public void ProfileIncrement(string distinctId, bool isLogin, Dictionary<string, object> properties,
            string platform)
        {
            Upload(distinctId, isLogin, EventName.P_IN.GetEnumDescription(), properties, platform, null);
        }

        /// <summary>
        /// 为用户的一个或多个数值类型的属性累加一个数值 </summary>
        /// <param name="distinctId"> 用户ID </param>
        /// <param name="isLogin"> 用户ID是否是登录 ID </param>
        /// <param name="properties"> 用户属性 </param>
        /// <param name="platform"> 平台类型 </param>
        public void ProfileIncrement(string distinctId, bool isLogin, Dictionary<string, object> properties,
            string platform, string xwhen)
        {
            Upload(distinctId, isLogin, EventName.P_IN.GetEnumDescription(), properties, platform, xwhen);
        }

        /// <summary>
        /// 追加用户列表类型的属性
        /// </summary>
        /// <param name="distinctId">用户ID</param>
        /// <param name="isLogin">用户ID是否是登录 ID</param>
        /// <param name="properties">用户属性</param>
        /// <param name="platform">平台类型</param>
        public void ProfileAppend(string distinctId, bool isLogin, Dictionary<string, object> properties,
            string platform)
        {
            ProfileAppend(distinctId, isLogin, properties, platform, null);
        }

        /// <summary>
        /// 追加用户列表类型的属性
        /// </summary>
        /// <param name="distinctId">用户ID</param>
        /// <param name="isLogin">用户ID是否是登录 ID</param>
        /// <param name="properties">用户属性</param>
        /// <param name="platform">平台类型</param>
        public void ProfileAppend(string distinctId, bool isLogin, Dictionary<string, object> properties,
            string platform, string xwhen)
        {
            Upload(distinctId, isLogin, EventName.P_APP.GetEnumDescription(), properties, platform, xwhen);
        }

        /// <summary>
        /// 删除用户某一个属性 </summary>
        /// <param name="distinctId"> 用户ID </param>
        /// <param name="isLogin"> 用户ID是否是登录 ID </param>
        /// <param name="property"> 用户属性名称 </param>
        /// <param name="platform"> 平台类型 </param>
        public void ProfileUnSet(string distinctId, bool isLogin, string property, string platform)
        {
            Dictionary<String, Object> properties = new Dictionary<string, object>(2);
            properties.Add(property, "");
            Upload(distinctId, isLogin, EventName.P_UN.GetEnumDescription(), properties, platform, null);
        }

        /// <summary>
        /// 删除用户某一个属性 </summary>
        /// <param name="distinctId"> 用户ID </param>
        /// <param name="isLogin"> 用户ID是否是登录 ID </param>
        /// <param name="property"> 用户属性名称 </param>
        /// <param name="platform"> 平台类型 </param>
        public void ProfileUnSet(string distinctId, bool isLogin, string property, string platform, string xwhen)
        {
            Dictionary<String, Object> properties = new Dictionary<string, object>(2);
            properties.Add(property, "");
            Upload(distinctId, isLogin, EventName.P_UN.GetEnumDescription(), properties, platform, xwhen);
        }

        /// <summary>
        /// 删除用户所有属性 </summary>
        /// <param name="distinctId"> 用户ID </param>
        /// <param name="isLogin"> 用户ID是否是登录 ID </param>
        /// <param name="platform"> 平台类型 </param>
        /// <exception cref="AnalysysException"> 自定义异常 </exception>
        public void ProfileDelete(string distinctId, bool isLogin, string platform)
        {
            Upload(distinctId, isLogin, EventName.P_DEL.GetEnumDescription(), new Dictionary<string, object>(1),
                platform, null);
        }

        /// <summary>
        /// 删除用户所有属性 </summary>
        /// <param name="distinctId"> 用户ID </param>
        /// <param name="isLogin"> 用户ID是否是登录 ID </param>
        /// <param name="platform"> 平台类型 </param>
        /// <exception cref="AnalysysException"> 自定义异常 </exception>
        public void ProfileDelete(string distinctId, bool isLogin, string platform, string xwhen)
        {
            Upload(distinctId, isLogin, EventName.P_DEL.GetEnumDescription(), new Dictionary<string, object>(1),
                platform, xwhen);
        }

        /// <summary>
        /// 关联用户匿名ID和登录ID
        /// </summary>
        /// <param name="aliasId">用户登录ID</param>
        /// <param name="distinctId">用户匿名ID</param>
        /// <param name="platform">平台类型</param>
        public void Alias(string aliasId, string distinctId, string platform)
        {
            Alias(aliasId, distinctId, platform, null);
        }

        public void Alias(string aliasId, string distinctId, string platform, string xwhen)
        {
            Dictionary<string, object> param = new Dictionary<string, object>(2);
            param.Add("$original_id", distinctId);
            Upload(aliasId, true, EventName.ALIAS.GetEnumDescription(), param, platform, xwhen);
        }

        /// <summary>
        /// 追踪用户多个属性的事件
        /// </summary>
        /// <param name="distinctId">用户ID</param>
        /// <param name="isLogin">用户ID是否是登录 ID</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="properties">事件属性</param>
        /// <param name="platform">平台类型k</param>
        public void Track(string distinctId, bool isLogin, string eventName, Dictionary<string, object> properties,
            string platform)
        {
            Upload(distinctId, isLogin, eventName, properties, platform, null);
        }

        /// <summary>
        /// 追踪用户多个属性的事件
        /// </summary>
        /// <param name="distinctId">用户ID</param>
        /// <param name="isLogin">用户ID是否是登录 ID</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="properties">事件属性</param>
        /// <param name="platform">平台类型k</param>
        public void Track(string distinctId, bool isLogin, string eventName, Dictionary<string, object> properties,
            string platform, string xwhen)
        {
            Upload(distinctId, isLogin, eventName, properties, platform, xwhen);
        }

        private void Upload(string distinctId, bool isLogin, string eventName, Dictionary<string, object> properties,
            string platform, string xwhen)
        {
            if (eventName == null)
            {
                eventName = "";
            }

            ValidHandle.CheckProperty(distinctId, eventName, properties, this.xcontextSuperProperties.Count);
            Dictionary<string, object> eventMap = new Dictionary<string, object>(8);
            eventMap.Add("xwho", distinctId);

            if (xwhen != null && xwhen.Trim().Length > 0)
            {
                if (xwhen.Trim().Length != 13 || !Regex.IsMatch(xwhen, RegexString.REG_XWHEN))
                {
                    Console.WriteLine($"The param xwhen {xwhen.Trim()} not a millisecond timestamp.");
                }

                try
                {
                    long when = long.Parse(xwhen.Trim());
                    eventMap.Add("xwhen", when);
                }
                catch (Exception e)
                {
                    Console.WriteLine("The param xwhen %s not a timestamp." + xwhen.Trim());
                }
            }
            else
            {
                if (EventName.ALIAS.GetEnumDescription().StartsWith(eventName))
                {
                    //为了防止alise事件和别的事件的xwhen相同
                    eventMap.Add("xwhen", TimeHelper.CurrentTimeMillis() - 3);
                }
                else
                {
                    eventMap.Add("xwhen", TimeHelper.CurrentTimeMillis());
                }
            }

            eventMap.Add("xwhat", eventName);
            eventMap.Add("appid", appId);
            Dictionary<string, object> newProperties = new Dictionary<string, object>(16);
            string profile = "$profile";
            if (!eventName.StartsWith(profile) && !eventName.StartsWith(EventName.ALIAS.GetEnumDescription()))
            {
                AddDictionary(ref newProperties, xcontextSuperProperties);
            }

            newProperties.Add("$debug", (int) debugMode);
            if (properties != null)
            {
                AddDictionary(ref newProperties, properties);
            }

            AddDictionary(ref newProperties, egBaseProperties);
            newProperties.Add("$is_login", isLogin);
            string newPlatForm = GetPlatForm(platform);
            if (newPlatForm != null && newPlatForm.Trim().Length > 0)
            {
                newProperties.Add("$platform", newPlatForm);
            }

            eventMap.Add("xcontext", newProperties);
            this.collecter.Debug(IsDebug());
            bool ret = this.collecter.Send(eventMap);
            if (eventName.StartsWith(profile) && IsDebug() && ret)
            {
                Console.WriteLine(string.Format("{0} success.", eventName.Substring(1)));
            }
        }

        private string GetPlatForm(string platform)
        {
            if (PlatForm.JS.GetEnumDescription().Equals(platform, StringComparison.CurrentCultureIgnoreCase))
            {
                return PlatForm.JS.GetEnumDescription();
            }

            if (PlatForm.WeChat.GetEnumDescription().Equals(platform, StringComparison.CurrentCultureIgnoreCase))
            {
                return PlatForm.WeChat.GetEnumDescription();
            }

            if (PlatForm.Android.GetEnumDescription().Equals(platform, StringComparison.CurrentCultureIgnoreCase))
            {
                return PlatForm.Android.GetEnumDescription();
            }

            if (PlatForm.iOS.GetEnumDescription().Equals(platform, StringComparison.CurrentCultureIgnoreCase))
            {
                return PlatForm.iOS.GetEnumDescription();
            }

            if (PlatForm.DotNet.GetEnumDescription().Equals(platform, StringComparison.CurrentCultureIgnoreCase))
            {
                return PlatForm.DotNet.GetEnumDescription();
            }

            if (!string.IsNullOrEmpty(platform))
            {
                Console.WriteLine(string.Format(
                    "Warning: param platform:{0}  Your input are not:iOS/Android/JS/WeChat/DotNet.",
                    platform == null ? "null" : platform));
            }

            if (PlatForm.Java.GetEnumDescription().Equals(platform, StringComparison.CurrentCultureIgnoreCase))
            {
                return PlatForm.Java.GetEnumDescription();
            }

            if (PlatForm.Python.GetEnumDescription().Equals(platform, StringComparison.CurrentCultureIgnoreCase))
            {
                return PlatForm.Python.GetEnumDescription();
            }

            if (PlatForm.Node.GetEnumDescription().Equals(platform, StringComparison.CurrentCultureIgnoreCase))
            {
                return PlatForm.Node.GetEnumDescription();
            }

            if (PlatForm.PHP.GetEnumDescription().Equals(platform, StringComparison.CurrentCultureIgnoreCase))
            {
                return PlatForm.PHP.GetEnumDescription();
            }

            if (platform == null || platform.Trim().Length == 0)
            {
                return PlatForm.DotNet.GetEnumDescription();
            }

            return platform;
        }


        private void AddDictionary(ref Dictionary<string, object> src1, Dictionary<string, object> src2)
        {
            if (src2 == null) return;
            if (src1 == null) src1 = new Dictionary<string, object>();
            foreach (var kv in src2)
            {
                if (src1.ContainsKey(kv.Key))
                {
                    src1.Remove(kv.Key);
                }
                src1.Add(kv.Key,kv.Value);
            }
        }
    }
}