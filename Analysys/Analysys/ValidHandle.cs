using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Analysys
{
    public class ValidHandle
    {
        private static bool delNotValidParam = false;

        private const string KEY_PATTERN =
            "^((?!^xwhat$|^xwhen$|^xwho$|^appid$|^xcontext$|^\\$lib$|^\\$lib_version$)^[$a-zA-Z][$a-zA-Z0-9_]{0,98})$";

        private const string KEY_PATTERN_CONTEXT =
            "^((?!^xwhat$|^xwhen$|^xwho$|^appid$|^xcontext$|^\\$lib$|^\\$lib_version$)^[$a-zA-Z][$a-zA-Z0-9_]{0,124})$";

        public static void SetDelNotValidParam(bool delNotValidParam)
        {
            ValidHandle.delNotValidParam = delNotValidParam;
        }

        /**
         * 属性参数格式校验
         * @param eventName 事件名称
         * @param properties 属性
         * @throws AnalysysException 自定义异常
         */
        public static void CheckParam(string eventName, Dictionary<string, object> properties)
        {
            if (properties == null)
            {
                properties = new Dictionary<string, object>(1);
            }

            Dictionary<string, object> tempProperties = new Dictionary<string, object>(properties);
            foreach (KeyValuePair<string, object> property in tempProperties)
            {
                if (string.IsNullOrEmpty(property.Key))
                {
                    Console.WriteLine("Warn: The property key is null or empty.");
                    continue;
                }

                if (property.Value == null || (property.Value is string && string.IsNullOrEmpty((string) property.Value)))
                {
                    if(EventName.P_UN.GetEnumDescription().Equals(eventName)){
                        continue;
                    }
                    Console.WriteLine($"Warn: The property {property.Key} value is null or empty.");
                    continue;
                }

                try
                {
                    CheckParamImpl(eventName, property, properties);
                }
                catch (Exception e)
                {
                    if (delNotValidParam)
                    {
                        properties.Remove(property.Key);
                        Console.WriteLine(e);
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        public static bool CheckParamImpl(string eventName, KeyValuePair<string, object> property,
            Dictionary<string, object> properties)
        {
            int valueLength = 8192;
            int valueWarnLength = 255;
            int keyLength = 99;
            int valueListLen = 100;
            
            string piEventName = "$profile_increment";
            string paEventName = "$profile_append";
            int temp = 0; //没有任何意思，只是为了满足语法的要求
            //key约束 符合java命名规则： 开头约束:字母或者$ 字符类型:大小写字母、数字、下划线和 $ 最大长度125字符
            if (property.Key.Length > keyLength)
            {
                Console.WriteLine(string.Format("The property key {0} is too long, max length is {1}.", property.Key,
                    keyLength));
            }

            if (!Regex.Match(property.Key, KEY_PATTERN_CONTEXT).Success)
            {
                Console.WriteLine(string.Format("The property key {0} is invalid.", property.Key));
            }

            if (!(property.Value is int) &&
                !(property.Value is bool) &&
                !(property.Value is double) &&
                !(property.Value is float) &&
                !(property.Value is long) &&
                !(property.Value is string) &&
                !(property.Value is Array) &&
                !(property.Value.GetType().GetGenericTypeDefinition() == typeof(List<>)))
            {
                Console.WriteLine(string.Format("The property {0} is not Number, String, Boolean, List<String>.",
                    property.Key));
            }

            if (property.Value is string && property.Value.ToString().Length > valueWarnLength)
            {
                Console.WriteLine(string.Format("The property {0} String value  is too long, max length is {1}.",
                    property.Key, valueWarnLength));
            }

            if (property.Value is string && property.Value.ToString().Length > valueLength)
            {
                properties[property.Key] = property.Value.ToString().Substring(0, valueLength - 1) + "$";
            }

            //数组集合约束 数组或集合内最多包含100条,若为字符串数组或集合,每条最大长度255个字符
            if (property.Value.GetType().IsGenericType &&
                property.Value.GetType().GetGenericTypeDefinition() == typeof(List<>))
            {
                List<string> valueList = null;
                try
                {
                    valueList = (List<string>) property.Value;
                }
                catch
                {
                    Console.WriteLine(string.Format("The property {0} should be a list of String.", property.Key));
                }

                if (valueList.Count > valueListLen)
                {
                    valueList = valueList.GetRange(0, valueListLen);
                    properties[property.Key] = valueList;
                    Console.WriteLine(string.Format("The property {0} value {1}, max number should be {2}.",
                        property.Key, property.Value, valueListLen));
                }

                for (int index = 0; index < valueList.Count; index++)
                {
                    if (valueList[index] == null)
                    {
                        Console.WriteLine($"Warn:The property {property.Key} some value is null.");
                        continue;
                    }

                    if (!(valueList[index] is string))
                    {
                        Console.WriteLine($"The property {property.Key} should be a list of String.");
                    }

                    if (valueList[index].ToString().Length > valueWarnLength)
                    {
                        Console.WriteLine(string.Format(
                            "Warn:The property {0} some value is too long, max length is {1}.", property.Key,
                            valueWarnLength));
                    }

                    if (valueList[index].ToString().Length > valueLength)
                    {
                        valueList[index] = valueList[index].ToString().Substring(0, valueLength - 1) + "$";
                    }
                }

                properties[property.Key] = valueList;
            }

            if (piEventName.Equals(eventName) && !(int.TryParse(property.Value.ToString(), out temp)))
            {
                Console.WriteLine(string.Format("The property value of {0} should be a Number.", property.Key));
            }

            if (paEventName.Equals(eventName))
            {
                if (!(property.Value.GetType().IsGenericType &&
                      property.Value.GetType().GetGenericTypeDefinition() == typeof(List<>)) &&
                    !(property.Value is Array))
                {
                    Console.WriteLine(
                        string.Format("The property value of {0} should be a List<String>.", property.Key));
                }
            }

            return true;
        }

        /**
         * 格式校验
         * @param distinctId 用户标识
         * @param eventName 事件名称
         * @param properties 属性
         * @param commProLen 公共属性长度
         * @throws AnalysysException 自定义异常
         */
        public static void CheckProperty(string distinctId, string eventName, Dictionary<string, object> properties,
            int commProLen)
        {
            string aliasEventName = "$alias";
            string profileEventName = "$profile";
            string originalId = "$original_id";
            int eventNameLen = 99;
            int connonParamLen = 5;
            int idLength = 255;
            int totalParamLen = 300;
            if (properties == null)
            {
                properties = new Dictionary<string, object>(1);
            }

            if (string.IsNullOrEmpty(distinctId) || distinctId.Length == 0)
            {
                Console.WriteLine(string.Format("aliasId {0} is empty.", distinctId));
            }

            if (distinctId.Length > idLength)
            {
                Console.WriteLine(string.Format("aliasId {0} is too long, max length is {1}.", distinctId, idLength));
            }

            if (aliasEventName.Equals(eventName))
            {
                if (properties[originalId] == null || properties[originalId].ToString().Length == 0)
                {
                    Console.WriteLine(string.Format("original_id {0} is empty.", properties[originalId].ToString()));
                }

                if (properties[originalId].ToString().Length > idLength)
                {
                    Console.WriteLine(string.Format("original_id {0} is too long, max length is {1}.",
                        properties[originalId].ToString(), idLength));
                }
            }

            if (string.IsNullOrEmpty(eventName) || eventName.Length == 0)
            {
                Console.WriteLine("EventName is empty.");
                return;
            }

            if (eventName.Length > eventNameLen)
            {
                Console.WriteLine(string.Format("EventName {0} is too long, max length is {1}.", eventName,
                    eventNameLen));
            }

            if (!Regex.Match(eventName, KEY_PATTERN).Success)
            {
                Console.WriteLine(string.Format("EventName {0} is invalid.", eventName));
            }

            //xcontext属性值不大于300个
            if (!eventName.StartsWith(profileEventName) && !eventName.StartsWith(aliasEventName))
            {
                if (properties.Count + commProLen + connonParamLen > totalParamLen)
                {
                    SubProperties(properties, totalParamLen - connonParamLen - commProLen);
                    Console.WriteLine(string.Format("Warn: Too many attributes. max number is {0}.",
                        (totalParamLen - commProLen - connonParamLen)));
                }
            }
            else
            {
                if (properties.Count + connonParamLen > totalParamLen)
                {
                    SubProperties(properties, totalParamLen - connonParamLen);
                    Console.WriteLine(string.Format("Warn: Too many attributes. max number is {0}.",
                        (totalParamLen - connonParamLen)));
                }
            }

            CheckParam(eventName, properties);
        }

        private static void SubProperties(Dictionary<string, object> properties, int len)
        {
            Dictionary<string, object> tempProperties = new Dictionary<string, object>(properties);
            try
            {
                int index = 0;
                foreach (KeyValuePair<string, object> property in tempProperties)
                {
                    if (++index <= len)
                    {
                        continue;
                    }
                    else
                    {
                        properties.Remove(property.Key);
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}