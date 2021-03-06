﻿using leke.entity;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace leke
{
    public class WeiXinHelper
    {

        static string corpid = System.Configuration.ConfigurationManager.AppSettings["corpid"].ToString();
        static string corpsecret = System.Configuration.ConfigurationManager.AppSettings["secret"].ToString();
        static string messageSendURI = System.Configuration.ConfigurationManager.AppSettings["messageSendURI"].ToString();
        static string getUsers = System.Configuration.ConfigurationManager.AppSettings["getUsers"].ToString();
       public static string userGroup = System.Configuration.ConfigurationManager.AppSettings["userGroup"].ToString();

        static string adminUser = System.Configuration.ConfigurationManager.AppSettings["adminUser"].ToString();
        
        //public static ConcurrentQueue<string> queue;
        //static WeiXinHelper()
        //{
        //    Task.Run(()=> {
        //        while (true)
        //        {
        //            if (queue.TryDequeue(out string par))
        //            { }
        //        }



        //    });
        //}

        /// <summary>
        /// 获取企业号的accessToken
        /// </summary>
        /// <param name="corpid">企业号ID</param>
        /// <param name="corpsecret">管理组密钥</param>
        /// <returns></returns>
        public static Msg GetQYAccessToken()
        {
            string getAccessTokenUrl = System.Configuration.ConfigurationManager.AppSettings["getAccessTokenUrl"].ToString();
            string accessToken = "";

            string respText = "";

            //获取josn数据
            string url = string.Format(getAccessTokenUrl, corpid, corpsecret);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (Stream resStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(resStream, Encoding.Default);
                respText = reader.ReadToEnd();
                resStream.Close();
            }

            try
            {
                JavaScriptSerializer Jss = new JavaScriptSerializer();
                Dictionary<string, object> respDic = (Dictionary<string, object>)Jss.DeserializeObject(respText);
                //通过键access_token获取值
                accessToken = respDic["access_token"].ToString();
            }
            catch (Exception) { }

            return new Msg { msgs = accessToken };
        }

        /// <summary>
        /// Post数据接口
        /// </summary>
        /// <param name="postUrl">接口地址</param>
        /// <param name="paramData">提交json数据</param>
        /// <param name="dataEncode">编码方式</param>
        /// <returns></returns>
        static string PostWebRequest(string postUrl, string paramData, Encoding dataEncode)
        {
            string ret = string.Empty;
            try
            {
                byte[] byteArray = dataEncode.GetBytes(paramData); //转化
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                webReq.Method = "POST";
                webReq.ContentType = "application/x-www-form-urlencoded";

                webReq.ContentLength = byteArray.Length;
                Stream newStream = webReq.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);//写入参数
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.Default);
                ret = sr.ReadToEnd();
                sr.Close();
                response.Close();
                newStream.Close();
            }
            catch (WebException ex)
            {
                WeiXinHelper.CreateLog("main", "★★★★★" + ex, 2);
                //helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                WeiXinHelper.SendText("13142025891", "★★★★★" + ex, false);
                //Login(u);
                return ex.Message;

            }
            catch (Exception ex)
            {
                WeiXinHelper.CreateLog("main", "★★★★★" + ex, 2);
                //helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                WeiXinHelper.SendText("13142025891", "★★★★★" + ex, false);
                return ex.Message;
            }
            
            return ret;
        }

        /// <summary>
        /// 推送信息
        /// </summary>
        /// <param name="corpid">企业号ID</param>
        /// <param name="corpsecret">管理组密钥</param>
        /// <param name="paramData">提交的数据json</param>
        /// <param name="dataEncode">编码方式</param>
        /// <returns></returns>
        public static void SendText(string empCode, string message,bool isAdmin)
        {
            if (isAdmin && (empCode != "13142025891" || empCode == ""))
            {
                if (empCode != "")
                {
                    empCode = empCode + "|";
                }
                empCode = empCode + adminUser;
            }
          
            string accessToken = "";
            string postUrl = "";
            string param = "";
            string postResult = "";

            accessToken = CacheHelper.Token.msgs;
            postUrl = string.Format(messageSendURI, accessToken);
            CorpSendText paramData = new CorpSendText(message);
            foreach (string item in empCode.Split('|'))
            {
                //paramData.touser = GetOAUserId(item);//在实际应用中需要判断接收消息的成员是否在系统账号中存在。
                paramData.touser = item;
                param = JsonConvert.SerializeObject(paramData);
                if (paramData.touser != null)
                {
                    postResult = PostWebRequest(postUrl, param, Encoding.UTF8);
                }
                else
                {
                    postResult = "账号" + paramData.touser + "在OA中不存在!";
                }
                CreateLog("weixin",DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss") + ":\t" + item + "\t" + param + "\t" + postResult,0);
            }
        }

        public static void CreateLog(string dire,string strlog,int Type)
        {
            string str1 = "QYWeixin_log-" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            if (Type == 1)
            {
                str1 = "info-" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            }
            else if (Type == 2)
            {
                str1 = "error-" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            }
            else if (Type == 3)
            {
                str1 = "validata-" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            }

            if (Type != 0)
            {
                strlog = $"{DateTime.Now.ToString("yyyyMMdd HH:mm:ss")} {strlog}";
            }
            //BS CS应用日志自适应
            string path = System.Web.HttpContext.Current == null ? Path.GetFullPath(".") + $"\\log\\{dire}\\" : System.Web.HttpContext.Current.Server.MapPath("temp");
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = Path.Combine(path, str1);
                StreamWriter sw = File.AppendText(path);
                sw.WriteLine(strlog);
                sw.Flush();
                sw.Close();

            }
            catch
            {
            }
        }


        public static List<User> GetUsers()
        {
            string url = string.Format(getUsers, CacheHelper.Token.msgs);
            var list = new List<User>();
           

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var respText = "";
                using (Stream resStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(resStream, Encoding.UTF8);
                    respText = reader.ReadToEnd();
                    resStream.Close();
                }
                var jArray = JsonConvert.DeserializeObject<WeinUserResult>(respText);
              
                if (jArray != null&& jArray.userlist!=null&& jArray.userlist.Count > 0)
                {
                    jArray.userlist.ForEach(p=> {
                         list.AddRange(User.Clone(p));
                       });
                }

            }
            catch (WebException ex)
            {
                WeiXinHelper.CreateLog("main", "★★★★★" + ex, 2);
                //helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                WeiXinHelper.SendText("13142025891", "★★★★★" + ex, false);
                //Login(u);


            }
            catch (Exception ex)
            {
                WeiXinHelper.CreateLog("main", "★★★★★" + ex, 2);
                //helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                WeiXinHelper.SendText("13142025891", "★★★★★" + ex, false);
            }
          
            return list;
        }

    }
}
