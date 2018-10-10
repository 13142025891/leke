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
    public class Program
    {
        public static string cookie;
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Console.Title = "运行后，点我可以看到实时进度";
            Application.Run(new Main());
           
        }
      
    }
   
}
