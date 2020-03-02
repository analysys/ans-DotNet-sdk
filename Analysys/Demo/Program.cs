using Analysys;
using System;
using System.Collections.Generic;

namespace Demo
{
    class Program
    {
        private const string APP_KEY = "your app key";
        private const string ANALYSYS_SERVICE_URL = @"http://ip:port";

        static void Main(string[] args)
        {
            AnalysysDotNetSdk analysys = new AnalysysDotNetSdk(new SyncCollecter(ANALYSYS_SERVICE_URL), APP_KEY);
            //批量
            // AnalysysDotNetSdk analysys = new AnalysysDotNetSdk(new BatchCollecter(ANALYSYS_SERVICE_URL), APP_KEY);
            //落文件
            // AnalysysDotNetSdk analysys = new AnalysysDotNetSdk(new LogCollecter(@"{your_save_dir}"), APP_KEY);
            
            try
            {
                string distinctId = "1234567890987654321";
                string platForm = "android"; //Android平台
                analysys.SetDebugMode(DEBUG.OPENNOSAVE); //设置debug模式
                //浏览商品
                Dictionary<string, object> trackPropertie = new Dictionary<string, object>();
                trackPropertie["$ip"] = "122.122.122.122"; //IP地址
                List<string> bookList = new List<string>();
                bookList.Add("Thinking in Java");
                trackPropertie["productName"] = bookList; //商品列表
                trackPropertie["productType"] = "Java书籍"; //商品类别
                trackPropertie["producePrice"] = 80; //商品价格
                trackPropertie["shop"] = "xx网上书城"; //店铺名称
                analysys.Track(distinctId, false, "ViewProduct", trackPropertie, platForm);

                //用户注册登录
                string registerId = "ABCDEF123456789";
                analysys.Alias(registerId, distinctId, platForm);

                //设置公共属性
                Dictionary<string, object> superPropertie = new Dictionary<string, object>();
                superPropertie["sex"] = "male"; //性别
                superPropertie["age"] = 23; //年龄
                analysys.RegisterSuperProperties(superPropertie);
                //用户信息
                Dictionary<string, object> profiles = new Dictionary<string, object>();
                profiles["$city"] = "北京"; //城市
                profiles["$province"] = "北京"; //省份
                profiles["nickName"] = "昵称123"; //昵称
                profiles["userLevel"] = 0; //用户级别
                profiles["userPoint"] = 0; //用户积分
                List<string> interestList = new List<string>();
                interestList.Add("户外活动");
                interestList.Add("足球赛事");
                interestList.Add("游戏");
                profiles["interest"] = interestList; //用户兴趣爱好
                analysys.ProfileSet(registerId, true, profiles, platForm);

                //用户注册时间
                Dictionary<string, object> profile_age = new Dictionary<string, object>();
                profile_age["registerTime"] = "20180101101010";
                analysys.ProfileSetOnce(registerId, true, profile_age, platForm);

                //重新设置公共属性
                analysys.ClearSuperProperties();
                superPropertie.Clear();
                superPropertie = new Dictionary<string, object>();
                superPropertie["userLevel"] = 0; //用户级别
                superPropertie["userPoint"] = 0; //用户积分
                analysys.RegisterSuperProperties(superPropertie);

                //再次浏览商品
                trackPropertie.Clear();
                trackPropertie["$ip"] = "122.122.122.122"; //IP地址
                List<string> abookList = new List<string>();
                abookList.Add("Thinking in Java");
                trackPropertie["productName"] = bookList; //商品列表
                trackPropertie["productType"] = "Java书籍"; //商品类别
                trackPropertie["producePrice"] = 80; //商品价格
                trackPropertie["shop"] = "xx网上书城"; //店铺名称
                analysys.Track(registerId, true, "ViewProduct", trackPropertie, platForm);

                //订单信息
                trackPropertie.Clear();
                trackPropertie["orderId"] = "ORDER_12345";
                trackPropertie["price"] = 80;
                analysys.Track(registerId, true, "Order", trackPropertie, platForm);

                //支付信息
                trackPropertie.Clear();
                trackPropertie["orderId"] = "ORDER_12345";
                trackPropertie["productName"] = "Thinking in Java";
                trackPropertie["productType"] = "Java书籍";
                trackPropertie["producePrice"] = 80;
                trackPropertie["shop"] = "xx网上书城";
                trackPropertie["productNumber"] = 1;
                trackPropertie["price"] = 80;
                trackPropertie["paymentMethod"] = "AliPay";
                analysys.Track(registerId, true, "Payment", trackPropertie, platForm);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                analysys.Flush();
            }
            Console.WriteLine("Demo 运行结束，点击任意按键结束！");
            Console.Read();
        }
    }
}