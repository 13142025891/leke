using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace leke
{
    public class Program
    {
        public static string cookie;
        static void Main(string[] args)
        {
            

            if (args.Length > 0)
            {
                try
                {
                    ServiceBase[] serviceToRun = new ServiceBase[] { new WindowsService() };
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
