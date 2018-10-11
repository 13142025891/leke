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
    public class helper
    {
        public const string logUrl = "http://s.58leke.com/index.php?s=/Index/login.html";

        public static void Do(User u, string cookie)
        {
            Log(ConsoleColor.Green, $"{u.Account} 已开始刷任务，请等待。。。");
            WeiXinHelper.SendText(u.Account, $"{u.Account} 已经登录成功，开始刷任务请等待。。。");
            while (true)
            {
                if (!Main.isRun)
                {
                    Log(ConsoleColor.White, $"{u.Account} 已停止！");
                    WeiXinHelper.SendText(u.Account, $"{u.Account} 已停止刷任务。");
                    break;
                }
                
                var r = new Msg();
                try
                {
                    r = Getdingdan(u.Account, cookie);
                    if (r.code == "1")
                    {
                        //u.IsComplete = true;
                        //Main.a1.Invoke($"{u.Account}   {r.msgs}");
                        Log(ConsoleColor.Green, $"{u.Account}   {r.msgs}");
                        WeiXinHelper.SendText(u.Account, $"{u.Account}  已经刷到任务，马上去做吧！");
                        WeiXinHelper.CreateLog(u.Account, $"{u.Account}  已经刷到任务，马上去做吧！", 1);
                        System.Threading.Thread.Sleep(1000 * 60*5);
                    }
                    else if (r.msgs.Contains("您还有进行中的任务没完成")|| r.msgs.Contains("评价"))
                    {
                        //Main.a1.Invoke($"{u.Account}   {r.msgs}");
                        WeiXinHelper.SendText(u.Account, $"{u.Account}   {r.msgs}，快去完成吧别错过了！");
                        WeiXinHelper.CreateLog(u.Account, $"{u.Account}   {r.msgs}，快去完成吧别错过了！", 1);
                        if (!u.IsComplete)
                        {
                            Log(ConsoleColor.Yellow, $"{u.Account}   {r.msgs}");
                        }
                        u.IsComplete = true;
                        System.Threading.Thread.Sleep(1000 * 60*5);
                    }
                    else if (r.msgs.Contains("已上限"))
                    {
                        //Main.a1.Invoke($"{u.Account}   {r.msgs}");
                       
                        Log(ConsoleColor.Yellow, $"{u.Account}   {r.msgs}");
                        WeiXinHelper.CreateLog(u.Account, $"{u.Account}   {r.msgs}", 1);
                        WeiXinHelper.SendText(u.Account, $"{u.Account}   {r.msgs}！加油！");
                         u.IsComplete = false;
                        
                        System.Threading.Thread.Sleep(1000 * 60*60*3);
                    }
                    else
                    {
                        //Log(ConsoleColor.Yellow, $"{u.Account}   {r.msgs}");
                        WeiXinHelper.CreateLog(u.Account, $"{u.Account}   {r.msgs}", 1);
                        u.IsComplete = false;
                        System.Threading.Thread.Sleep(Main.Interval);
                    }
                }
                catch (WebException er)
                {
                    Log(ConsoleColor.Red, $"{u.Account} 刷任务出错，error: {er.Message} ");
                    WeiXinHelper.CreateLog(u.Account, $"{u.Account} 刷任务出错，error: {er.Message} ", 2);
                    System.Threading.Thread.Sleep(1000 * 5);
                }
                catch(Exception e)
                {
                    Log(ConsoleColor.Red, $"{u.Account} 返回出错，error: {e.Message} ");
                    WeiXinHelper.CreateLog(u.Account, $"{u.Account} 返回出错，error: {e.Message} ", 2);
                    System.Threading.Thread.Sleep(5000);
                }


            }

        }

        public static void Log(ConsoleColor color, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}  {message}");
        }

        public static void Login(User u)
        {
            string account = u.Account;
            string pass = u.Pass;
            try
            {



                if (!Main.isRun)
                {
                    Log(ConsoleColor.White, $"{account} 已停止！");
                    WeiXinHelper.SendText(u.Account, $"{u.Account} 已停止刷任务。");
                    return;
                }

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
                    System.Threading.Thread.Sleep(3000);
                    Log(ConsoleColor.Yellow, $"{account} 开始重新登录。。。");
                    Login(u);
                }

            }
            catch (WebException er)
            {
                Log(ConsoleColor.Red, $"{account} 登录失败，等待重新登录。。。，error: {er.Message} ");
                WeiXinHelper.CreateLog(account, $"{account} 登录失败，等待重新登录。。。，error: {er.Message} ", 2);
                System.Threading.Thread.Sleep(3000);
                Log(ConsoleColor.Yellow, $"{account} 开始重新登录。。。");
                WeiXinHelper.CreateLog(account, $"{account} 开始重新登录。。。", 2);
                Login(u);
                

            }
            catch (Exception e)
            {
                Log(ConsoleColor.Red, $"{u.Account} 出错，error: {e.Message} ");
                WeiXinHelper.CreateLog(u.Account, $"{u.Account} 返回出错，error: {e.Message} ", 2);
                System.Threading.Thread.Sleep(3000);
            }
        }




        private static Msg Getdingdan(string account, string cookie)
        {
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            postParams.Add("task_type", "1");
            postParams.Add("app", "1");
            postParams.Add("pc", "2");
            postParams.Add("maxmoney", "2000");
            postParams.Add("hasCaptcha", "0");
            postParams.Add("captcha_code", "");

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
            HttpWebRequest request = WebRequest.Create("http://s.58leke.com/index.php?s=/Indexajax/taskset.html ") as HttpWebRequest;
            request.Method = "POST";
            request.KeepAlive = false;
            request.ContentType = "application/x-www-form-urlencoded";
            //request.CookieContainer = cookieContainer;
            request.ContentLength = postData.Length;
            request.AllowAutoRedirect = false;
            request.Headers.Add("Cookie", cookie);
            // 提交请求数据
            System.IO.Stream outputStream = request.GetRequestStream();
            outputStream.Write(postData, 0, postData.Length);
            outputStream.Close();

            // 接收返回的页面
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            System.IO.Stream responseStream = response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("gb2312"));
            var srcString = reader.ReadToEnd();
            var jArray = JsonConvert.DeserializeObject<Msg>(srcString);
            return jArray;
        }
    }
}
