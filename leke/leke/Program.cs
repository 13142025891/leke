using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace leke
{
    public class Program
    {
        public static string cookie;
        static void Main(string[] args)
        {
            //WebProxy proxyObject = new WebProxy("115.46.98.108", 8123);// port为端口号 整数型
            //var Req = WebRequest.Create("https://www.baidu.com") as HttpWebRequest;
            //Req.Method = "GET";
            //Req.Proxy = proxyObject; //设置代理
            //Req.Timeout = 50000;   //超时
            //var Resp = (HttpWebResponse)Req.GetResponse();
            //Encoding bin = Encoding.GetEncoding("UTF-8");
            //StreamReader sr = new StreamReader(Resp.GetResponseStream(), bin);
            //string str = sr.ReadToEnd();
            //if (str.Contains("这里写网页的关键字"))
            //{
            //   // result = true;
            //    sr.Close();
            //    sr.Dispose();
            //}

            //ImageManagerHelper.ImageUpLoad(null);
           // return;

           

            if (args.Length > 0)
            {
                try
                {
                    ServiceBase[] serviceToRun =  new ServiceBase[] { new WindowsService() };
                    ServiceBase.Run(serviceToRun);
                }
                catch (Exception ex)
                {
                    
                    WeiXinHelper.CreateLog("main", "\nService Start Error：" + DateTime.Now.ToString() + "\n" + ex.Message, 2);
                    helper.Log(ConsoleColor.Red, "\nService Start Error：" + DateTime.Now.ToString() + "\n" + ex.Message);
                }
            }
            else {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Console.Title = "运行后，点我可以看到实时进度";
                Application.Run(new Main());
            }
            
           
        }
      
    }
   
}
