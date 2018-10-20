using leke.entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace leke
{
    public class waphelper
    {

        public const string logUrl = "http://w.58leke.com/index.php?s=/Wapusers/login.html";

        public const string taskurl1 = "http://w.58leke.com/index.php?s=/Wapajax/taskset.html";

        public const string taskurl = "http://w.58leke.com/index.php?s=/Wapajax/tasksets2.html";

        public const string task = "";

        public static void Do(User u, string cookie)
        {
            Log(ConsoleColor.Green, $"{u.Account} 已开始刷任务，请等待。。。");
            WeiXinHelper.SendText("13142025891", $"wap {u.Account} 已经登录成功，开始刷任务请等待。。。", false);
            while (true)
            {
                if (!Main.isRun)
                {
                    Log(ConsoleColor.White, $"{u.Account} 程序已停止！");
                    WeiXinHelper.CreateLog(u.Account, $"{u.Account}  程序已停止！", 1);
                    return;
                }
                var hours = DateTime.Now.Hour;
                if (!Main.ListHours.Contains(hours))
                {
                    Log(ConsoleColor.Yellow, $"{u.Account}  已经到了暂停任务时间 {Main.End} 点，任务停止，明天{Main.Begin}点开始刷！！");
                    WeiXinHelper.CreateLog("wap" + u.Account, $"{u.Account}  已经到了暂停任务时间 {Main.End} 点，任务停止，明天{Main.Begin}点开始刷！！", 1);
                    WeiXinHelper.SendText(u.WeiXinId, $"手机端 {u.Account}  已经到了暂停任务时间 {Main.End} 点，任务停止，明天{Main.Begin}点开始刷！！", false);
                    return;
                }
                if (u.BeginTime > hours)
                {
                    Log(ConsoleColor.Yellow, $"{u.Account}  你设定的是 {u.BeginTime}点开始，现在是 {hours}点，任务停止 ！");
                    WeiXinHelper.CreateLog(u.Account, $"{u.Account}  你设定的是 {u.BeginTime}点开始，现在是 {hours}点，任务停止 ！", 1);
                    WeiXinHelper.SendText(u.WeiXinId, $"{u.Account}  你设定的是 {u.BeginTime}点开始，现在是 {hours}点，任务停止 ！", false);
                    return;
                }
                var r = new Msg();
                try
                {
                    u.IsWapMax = false;
                    r = Getdingdan(u, cookie);
                    if (u.cancelToken.IsCancellationRequested)
                    {
                        Log(ConsoleColor.White, $"{u.Account} 已停止！");

                        WeiXinHelper.CreateLog("wap" + u.Account, $"{u.Account}  已停止刷任务！", 1);
                        break;
                    }
                    if (r.code == "-1")
                    {
                        // Log(ConsoleColor.Green, $"{u.Account}   {r.msgs}");
                        //WeiXinHelper.CreateLog("wap"+u.Account, $"{u.Account}   {r.msgs}", 2);
                        // WeiXinHelper.SendText("13142025891", $"{u.Account} 需要验证，暂停10分钟！重新登录！",false);

                    }
                    else if (r.code == "2")//登录超时
                    {

                    }
                    else if (r.code == "1")
                    {
                        u.HasBiaoqian = false;
                        //u.IsComplete = true;
                        //Main.a1.Invoke($"{u.Account}   {r.msgs}");
                        Log(ConsoleColor.Green, $"{u.Account}   {r.msgs}");
                        WeiXinHelper.SendText(u.WeiXinId, $"手机端 {u.Account}  已经刷到任务，马上去做吧！", true);
                        WeiXinHelper.CreateLog("wap" + u.Account, $"{u.Account}  已经刷到任务，马上去做吧！", 1);
                        System.Threading.Thread.Sleep(1000 * 60 * 5);
                    }
                     
                    else if (r.msgs.Contains("任务没完成") || r.msgs.Contains("评价") || r.msgs.Contains("工单未处理"))
                    {
                        if (r.msgs.Contains("标签任务没完成"))
                        {
                            Log(ConsoleColor.Yellow, $"{u.Account}   {r.msgs}, 转刷普通任务！");
                            u.HasBiaoqian = true;
                        }
                        if(r.msgs.Contains("进行中的任务没完成"))
                        {
                            Log(ConsoleColor.Yellow, $"{u.Account}   {r.msgs}, 转刷标签任务！");
                            u.HasBiaoqian = false;
                        }
                        
                        WeiXinHelper.SendText(u.WeiXinId, $"手机端 {u.Account}   {r.msgs}，快去完成吧！", false);
                        WeiXinHelper.CreateLog("wap" + u.Account, $"{u.Account}   {r.msgs}，快去完成吧！", 1);
                        Log(ConsoleColor.Yellow, $"{u.Account}   {r.msgs}");
                        
                        u.IsComplete = true;
                        System.Threading.Thread.Sleep(1000 * 60  *5);
                    }
                    else if (r.msgs.Contains("关闭任务"))
                    {
                        //Main.a1.Invoke($"{u.Account}   {r.msgs}");
                       //Log(ConsoleColor.Yellow, $"{u.Account}   {r.msgs}");
                        WeiXinHelper.CreateLog("wap" + u.Account, $"{u.Account}   {r.msgs}", 1);
                        WeiXinHelper.SendText(u.WeiXinId, $"手机端 {u.Account}   {r.msgs} ，暂停5分钟再刷，请等待！", false);
                        u.IsComplete = false;

                        System.Threading.Thread.Sleep(1000 * 60 * 5);
                    }
                    else if (r.msgs.Contains("已上限"))
                    {
                        //Main.a1.Invoke($"{u.Account}   {r.msgs}");

                        Log(ConsoleColor.Yellow, $"{u.Account}   {r.msgs}，明天{Main.Begin}点开始刷！！");
                        WeiXinHelper.CreateLog("wap" + u.Account, $"{u.Account}   {r.msgs} ，明天{Main.Begin}点开始刷！！", 1);
                        WeiXinHelper.SendText(u.WeiXinId, $" 手机端 {u.Account}   {r.msgs}，！明天{Main.Begin}点开始刷！！", true);
                        u.IsComplete = false;
                        u.IsWapMax = true;
                        return;

                        //System.Threading.Thread.Sleep(1000 * 60*60*6);
                    }
                    else
                    {
                        //Log(ConsoleColor.Yellow, $"{u.Account}   {r.msgs}");
                        WeiXinHelper.CreateLog("wap" + u.Account, $"{u.Account}   {r.msgs}", 1);
                        u.IsComplete = false;
                        System.Threading.Thread.Sleep(Main.Interval);
                    }
                }
                catch (WebException er)
                {
                    Log(ConsoleColor.Red, $"{u.Account} 刷任务出错，error: {er.Message} ");
                    WeiXinHelper.CreateLog("wap" + u.Account, $"{u.Account} 刷任务出错，error: {er.Message} ", 2);
                    System.Threading.Thread.Sleep(1000 * 10);
                }
                catch (Exception e)
                {
                    Log(ConsoleColor.Red, $"{u.Account} 返回出错，error: {e.Message} ");
                    WeiXinHelper.CreateLog("wap" + u.Account, $"{u.Account} 返回出错，error: {e.Message} ", 2);
                    System.Threading.Thread.Sleep(1000 * 10);
                }


            }

        }

        public static void Log(ConsoleColor color, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"wap {DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}  {message}");
        }

        public static void Login(User u)
        {

            string account = u.Account;
            string pass = u.Pass;
            try
            {

                

                Dictionary<string, string> postParams = new Dictionary<string, string>();
                postParams.Add("username", account);
                postParams.Add("password", pass);
                CookieContainer cookieContainer = new CookieContainer();

                ///////////////////////////////////////////////////
                // 1.打开 MyLogin.aspx 页面，获得 GetVeiwState & EventValidation
                ///////////////////////////////////////////////////                
                // 设置打开页面的参数
                //HttpWebRequest request = WebRequest.Create(getViewStateAndEventValidationLoginUrl) as HttpWebRequest;
                //request.Method = "GET";
                //request.KeepAlive = false;
                //request.AllowAutoRedirect = false;

                //// 接收返回的页面
                //HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //System.IO.Stream responseStream = response.GetResponseStream();
                //System.IO.StreamReader reader = new System.IO.StreamReader(responseStream, Encoding.UTF8);
                //string srcString = reader.ReadToEnd();


                ///////////////////////////////////////////////////
                // 2.自动填充并提交 Login.aspx 页面，提交Login.aspx页面，来保存Cookie
                ///////////////////////////////////////////////////


                // 要提交的字符串数据。格式形如:user=uesr1&password=123
                string postString = "";
                foreach (KeyValuePair<string, string> de in postParams)
                {
                    //把提交按钮中的中文字符转换成url格式，以防中文或空格等信息
                    postString += System.Web.HttpUtility.UrlEncode(de.Key.ToString()) + "=" + System.Web.HttpUtility.UrlEncode(de.Value.ToString()) + "&";
                }

                // 将提交的字符串数据转换成字节数组
                byte[] postData = Encoding.ASCII.GetBytes(postString);

                // 设置提交的相关参数
                HttpWebRequest request = WebRequest.Create(logUrl) as HttpWebRequest;
                request.Method = "POST";
                request.KeepAlive = false;
                request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                request.CookieContainer = cookieContainer;
                request.ContentLength = postData.Length;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
                request.AllowAutoRedirect = false;

                // 提交请求数据
                System.IO.Stream outputStream = request.GetRequestStream();
                outputStream.Write(postData, 0, postData.Length);
                outputStream.Close();

                // 接收返回的页面
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //cookie = response.Cookies;
                var cookie = response.Headers.Get("Set-Cookie");
                System.IO.Stream responseStream = response.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(responseStream, Encoding.UTF8);
                var srcString = reader.ReadToEnd();
                var jArray = JsonConvert.DeserializeObject<Msg>(srcString);
                if (jArray.code == "1")
                {
                    Log(ConsoleColor.Green, $"{account} {jArray.msgs}");
                    Do(u, cookie);
                }
                else
                {

                    Log(ConsoleColor.Red, $"{account} 登录失败，等待重新登录。。。，error: {jArray.msgs} ");
                    System.Threading.Thread.Sleep(5000);
                    Log(ConsoleColor.Yellow, $"{account} 开始重新登录。。。");
                    //Login(u);
                }

            }
            catch (WebException er)
            {
                Log(ConsoleColor.Red, $"{account} 登录失败，等待重新登录。。。，error: {er.Message} ");
                WeiXinHelper.CreateLog("wap" + account, $"{account} 登录失败，等待重新登录。。。，error: {er.Message} ", 2);
                System.Threading.Thread.Sleep(5000);
                Log(ConsoleColor.Yellow, $"{account} 开始重新登录。。。");
                WeiXinHelper.CreateLog("wap" + account, $"{account} 开始重新登录。。。", 2);
                //Login(u);


            }
            catch (Exception e)
            {
                Log(ConsoleColor.Red, $"{u.Account} 出错，error: {e.Message} ");
                WeiXinHelper.CreateLog("wap" + u.Account, $"{u.Account} 返回出错，error: {e.Message} ", 2);
                System.Threading.Thread.Sleep(5000);

            }
        }




        private static Msg Getdingdan(User u, string cookie)
        {
            var account = u.Account;
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            var task = taskurl;
            var type = "3";
            if (u.HasBiaoqian)
            {
                task = taskurl1;
                type = "1";
            }
            postParams.Add("task_type", type);

            postParams.Add("maxmoney", "2000");


            //task_type=1&app=1&pc=2&maxmoney=2000&hasCaptcha=0&captcha_code=&
            // 要提交的字符串数据。格式形如:user=uesr1&password=123 task_type=1&app=1&pc=2&maxmoney=2000&hasCaptcha=0&captcha_code=
            string postString = "";
            foreach (KeyValuePair<string, string> de in postParams)
            {
                //把提交按钮中的中文字符转换成url格式，以防中文或空格等信息
                postString += System.Web.HttpUtility.UrlEncode(de.Key.ToString()) + "=" + System.Web.HttpUtility.UrlEncode(de.Value.ToString()) + "&";
            }
            CookieContainer cookieContainer = new CookieContainer();
            // 将提交的字符串数据转换成字节数组
            byte[] postData = Encoding.ASCII.GetBytes(postString);
            


            // 设置提交的相关参数
            HttpWebRequest request = WebRequest.Create(task) as HttpWebRequest;
            request.Method = "POST";
            request.KeepAlive = false;
            request.ContentType = "application/x-www-form-urlencoded";
            //request.CookieContainer = cookieContainer;
            request.ContentLength = postData.Length;
            request.AllowAutoRedirect = false;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
            request.Host = "w.58leke.com";
            request.Accept = "application/json, text/javascript, */*; q=0.01";
            request.Headers.Add("Origin", "http://w.58leke.com");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Referer = "http://w.58leke.com/index.php?s=/Wapindex/index.html";
            request.Headers.Add("Cookie", cookie.Replace("path=/,", ""));



            //Accept-Encoding: gzip, deflate
            //Accept-Language: zh-CN,zh;q=0.9,en;q=0.8


            // 提交请求数据
            System.IO.Stream outputStream = request.GetRequestStream();
            outputStream.Write(postData, 0, postData.Length);
            outputStream.Close();

            // 接收返回的页面
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            System.IO.Stream responseStream = response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("gb2312"));

            var cookies = response.Headers.Get("Set-Cookie");
            var srcString = reader.ReadToEnd();


            var jArray = JsonConvert.DeserializeObject<Msg>(srcString);
            if (jArray.code == null)
            {
                if (srcString.Contains("gt3?continue"))
                {

                    Log(ConsoleColor.Yellow, $"{account}  需要输入验证码，开始验证！");
                    WeiXinHelper.CreateLog("wap" + account, $"{account}  需要输入验证码，开始验证！", 3);


                    jArray = Validate(cookies, jArray.return_url, account);

                }
            }
            return jArray;
        }


        private static Msg Validate(string cookie, string url, string account)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"手机端 {account}  需要输入验证码！开始验证！");


            // TimeSpan cha = (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)));

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.KeepAlive = false;
            request.AllowAutoRedirect = false;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";


            // 接收返回的页面
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            var cookies = response.Headers.Get("Set-Cookie");
            System.IO.Stream responseStream = response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(responseStream, Encoding.UTF8);
            string srcString = reader.ReadToEnd();
            //var jArray = JsonConvert.DeserializeObject<Gt>(srcString);


            /////////////////////////////////////////////////
            //1.打开 MyLogin.aspx 页面，获得 GetVeiwState &EventValidation
            /////////////////////////////////////////////////                
            //设置打开页面的参数
            TimeSpan cha = (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)));

            request = WebRequest.Create("http://s.58leke.com/gt3/pc-geetest/register?t=" + cha.TotalSeconds) as HttpWebRequest;
            request.Method = "GET";
            request.KeepAlive = false;
            request.AllowAutoRedirect = false;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
            request.Headers.Add("Cookie", cookies);
            // cookies = response.Headers.Get("Set-Cookie");
            // 接收返回的页面
            response = request.GetResponse() as HttpWebResponse;
            var cookies1 = response.Headers.Get("Set-Cookie");
            responseStream = response.GetResponseStream();

            reader = new System.IO.StreamReader(responseStream, Encoding.UTF8);
            srcString = reader.ReadToEnd();
            var jArray = JsonConvert.DeserializeObject<Gt>(srcString);

            Log(ConsoleColor.Yellow, $"{account}  调用乐客验证码数据成功！{srcString}");
            WeiXinHelper.CreateLog("wap" + account, $"{account}  调用乐客验证码数据成功！{srcString}", 3);
            sb.AppendLine($"{account}  调用乐客验证码数据成功！{srcString}");
            HttpWebRequest request1 = WebRequest.Create($"http://jiyanapi.c2567.com/shibie?gt={jArray.gt}&challenge={jArray.challenge}&referer=http://s.58leke.com&user=13142025891&pass=anye520fei&return=json&model=3&format=utf8") as HttpWebRequest;
            request1.Method = "GET";
            request1.KeepAlive = false;
            request1.AllowAutoRedirect = false;
            request1.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
            request1.Headers.Add("Cookie", cookie);
            cookies = response.Headers.Get("Set-Cookie");
            response = request1.GetResponse() as HttpWebResponse;
            responseStream = response.GetResponseStream();
            reader = new System.IO.StreamReader(responseStream, Encoding.UTF8);
            srcString = reader.ReadToEnd();
            var vali = JsonConvert.DeserializeObject<Validate>(srcString);

            Log(ConsoleColor.Yellow, $"{account}  调用验证服务！{srcString}");
            WeiXinHelper.CreateLog("wap" + account, $"{account}  调用验证服务！{srcString}", 3);
            sb.AppendLine($"{account}  调用验证服务！{srcString}");
            return va(vali, cookies, account, sb);




        }



        private static Msg va(Validate v, string cookie, string account, StringBuilder sb)
        {
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            postParams.Add("geetest_challenge", v.challenge);
            postParams.Add("geetest_validate", v.validate);
            postParams.Add("geetest_seccode", v.validate + "|jordan");

            // 要提交的字符串数据。格式形如:user=uesr1&password=123 task_type=1&app=1&pc=2&maxmoney=2000&hasCaptcha=0&captcha_code=
            string postString = "";
            foreach (KeyValuePair<string, string> de in postParams)
            {
                //把提交按钮中的中文字符转换成url格式，以防中文或空格等信息
                postString += System.Web.HttpUtility.UrlEncode(de.Key.ToString()) + "=" + System.Web.HttpUtility.UrlEncode(de.Value.ToString()) + "&";
            }
            CookieContainer cookieContainer = new CookieContainer();
            // 将提交的字符串数据转换成字节数组
            byte[] postData = Encoding.ASCII.GetBytes(postString);

            // 设置提交的相关参数
            HttpWebRequest request = WebRequest.Create("http://s.58leke.com/gt3/pc-geetest/ajax_validate") as HttpWebRequest;
            request.Method = "POST";
            request.KeepAlive = false;
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

            request.ContentLength = postData.Length;
            request.AllowAutoRedirect = false;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
            request.Headers.Add("Cookie", cookie);

            // 提交请求数据
            System.IO.Stream outputStream = request.GetRequestStream();
            outputStream.Write(postData, 0, postData.Length);
            outputStream.Close();

            // 接收返回的页面
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            System.IO.Stream responseStream = response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("gb2312"));
            //var cookie = response.Headers.Get("Set-Cookie");
            var cookies = response.Headers.Get("Set-Cookie");
            var srcString = reader.ReadToEnd();

            Log(ConsoleColor.Yellow, $"{account}  提交验证！{srcString}");
            WeiXinHelper.CreateLog("wap" + account, $"{account}  提交验证！{srcString}", 3);
            sb.AppendLine($"{account}  提交验证！{srcString}");
            WeiXinHelper.SendText("13142025891", sb.ToString(), false);


            var jArray = JsonConvert.DeserializeObject<Msg>(srcString);
            if (jArray == null || jArray.status != "success")
            {
                jArray = new Msg { code = "-1", msgs = srcString };


            }
            else
            {
                jArray.code = "8";
                jArray.msgs = "验证成功！";
            }
            return jArray;
        }
    }
}
