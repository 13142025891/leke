using leke.entity;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace leke
{
    public partial class Main : Form
    {
        public static bool isRun;
        public static int Interval;
        public static ConcurrentDictionary<string, User> dic;
        public static MessageBoxShow a1;
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Interval = 6000;
            dic = new ConcurrentDictionary<string, User>();
            a1 = new MessageBoxShow(ShowMessage);
            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        System.Threading.Thread.Sleep(1000*60*10);
                        Console.Clear();

                    }
                }
                catch (Exception ex)
                {
                    helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                    WeiXinHelper.CreateLog("main", "★★★★★" + ex, 2);
                }
            });
            var str = File.ReadAllText("Users.json");
            var lst = JsonConvert.DeserializeObject<List<User>>(str);
            var sb = new StringBuilder();
            foreach (var m in lst)
            {
                sb.AppendLine($"{m.Account},{m.Pass}");
            }
            this.textBox1.Text = sb.ToString();
        }
        public void ShowMessage(string msg)
        {
            Invoke(new MessageBoxShow(MessageBoxShow_F), new object[] { msg });
        }

        public delegate void MessageBoxShow(string msg);

        void MessageBoxShow_F(string msg)
        {
            
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (CacheHelper.Token == null || string.IsNullOrEmpty(CacheHelper.Token.msgs))
            {
                helper.Log(ConsoleColor.Red, "企业微信，通信出错请检查！");
                WeiXinHelper.CreateLog("weixin", $"企业微信获取token出错", 2);
                return;
            }
            

            if (!isRun)
            {
                Console.Clear();
                dic.Clear();
                isRun = true;
                if (int.TryParse(textBox2.Text, out int interval))
                {
                    Interval = interval;
                }
                var users = textBox1.Text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var flag = true;
                foreach (var user in users)
                {
                    var list = user.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (list == null || list.Count != 2)
                    {
                        helper.Log(ConsoleColor.Red, $"{user} 格式不正确，请检查！");
                        flag = false;
                        break;
                    }
                    dic.TryAdd(list[0], new User { Account = list[0], Pass = list[1] });
                }
                if (flag && dic.Keys.Count > 0)
                {
                    helper.Log(ConsoleColor.Yellow, $"开始启动程序，请等待。。。");
                    this.button1.Enabled = false;
                    this.button2.Enabled = true;
                    foreach (var d in dic.Keys)
                    {
                        if (dic.TryGetValue(d, out User u))
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    helper.Login(u);
                                }
                                catch (Exception ex)
                                {
                                    WeiXinHelper.CreateLog(u.Account, "★★★★★" + ex, 2);
                                    helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                                }
                            });
                            //System.Threading.Thread.Sleep(3000);
                        }

                    }
                }
                else
                {
                    helper.Log(ConsoleColor.Red, "没有数据！");
                }

            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isRun)
            {
                isRun = false;
                helper.Log(ConsoleColor.Yellow, $"开始停止程序，请等待。。。");
                this.button1.Enabled = true;
                this.button2.Enabled = false;
            }


        }
    }
}
