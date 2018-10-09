﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace leke
{
    public class Program
    {
        public static string cookie;
        static void Main(string[] args)
        {


            new Program().btnASPNET_Click();
            
            Console.ReadKey();
        }
        private void btnASPNET_Click()
        {
            //username=13142025891&password=anye520fei

            Dictionary<string, string> postParams = new Dictionary<string, string>();
            postParams.Add("username", "13142025891");
            postParams.Add("password", "anye520fei");


            var str = GetAspNetCodeResponseDataFromWebSite(postParams, "http://s.58leke.com/index.php?s=/Index/login.html", "http://s.58leke.com/index.php?s=/Index/buyers.html");
            if (str.code == "1")
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd")}   {str.msgs}");
                while (true)
                {
                    var r = Getdingdan();
                    if (r.code == "1")
                    {
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd")}   {r.msgs}");
                    }
                    System.Threading.Thread.Sleep(5000);
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("刷到任务了！");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd")}   {str.msgs}");
                btnASPNET_Click();
            }
            
        }


        private Msg GetAspNetCodeResponseDataFromWebSite(Dictionary<string, string> postParams, string getViewStateAndEventValidationLoginUrl, string getDataUrl)
        {

            try
            {
                CookieContainer cookieContainer = new CookieContainer();

                ///////////////////////////////////////////////////
                // 1.打开 MyLogin.aspx 页面，获得 GetVeiwState & EventValidation
                ///////////////////////////////////////////////////                
                // 设置打开页面的参数
                HttpWebRequest request = WebRequest.Create(getViewStateAndEventValidationLoginUrl) as HttpWebRequest;
                request.Method = "GET";
                request.KeepAlive = false;
                request.AllowAutoRedirect = false;

                // 接收返回的页面
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                System.IO.Stream responseStream = response.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(responseStream, Encoding.UTF8);
                string srcString = reader.ReadToEnd();


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
                request = WebRequest.Create(getViewStateAndEventValidationLoginUrl) as HttpWebRequest;
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
                response = request.GetResponse() as HttpWebResponse;
                //cookie = response.Cookies;
                cookie = response.Headers.Get("Set-Cookie");
                responseStream = response.GetResponseStream();
                reader = new System.IO.StreamReader(responseStream, Encoding.UTF8);
                srcString = reader.ReadToEnd();
                var jArray = JsonConvert.DeserializeObject<Msg>(srcString);
               
                return jArray;
            }
            catch (WebException we)
            {
                Console.WriteLine(we.Message);
                return new Msg();
            }
        }


      
        
        private Msg Getdingdan()
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
    public class Msg
    {
        public string code { get; set; }
        public string msgs { get; set; }
    }
}
