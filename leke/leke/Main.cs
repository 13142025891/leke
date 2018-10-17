﻿using leke.entity;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace leke
{
    public partial class Main : Form
    {
        public static bool isRun;
        public static int Interval;
        public static int Begin;
        public static int End;
        public static List<int> ListHours;
        public static ConcurrentDictionary<string, User> dic;
        public static MessageBoxShow a1;
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Interval = 10000;
            Begin = 8;
            End = 2;
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
        private void initHours()
        {
            ListHours = new List<int>();
            for (var i = Begin; i < 24; i++)
            {
                ListHours.Add(i);
                if (i == End)
                {
                    break;
                }

            }
            if (Begin > End)
            {
                for (var i = 0; i <= End; i++)
                {
                    ListHours.Add(i);
                }
            }
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
               
                if (int.TryParse(textBox2.Text, out int interval))
                {
                    Interval = interval;
                }
                Interval = Interval < 10000 ? 10000 : Interval;
                if (int.TryParse(textBox3.Text, out int begin))
                {
                    Begin = begin;
                }
                if (Begin < 0 || Begin > 23)
                {
                    Begin = 8;
                }
                if (int.TryParse(textBox4.Text, out int end))
                {
                    End = end;
                }
                if (End < 0 || End > 23)
                {
                    Begin = 2;
                }
                initHours();
                var users = textBox1.Text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var flag = true;

                helper.Log(ConsoleColor.Red, $"开始任务，开始时间{Begin}，结束时间 {End}");
                foreach (var user in users)
                {
                    var list = user.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (list == null || list.Count != 2)
                    {
                        helper.Log(ConsoleColor.Red, $"{user} 格式不正确，请检查！");
                        flag = false;
                        break;
                    }
                    dic.TryAdd(list[0], new User { Account = list[0], Pass = list[1] ,cancelToken=new CancellationTokenSource()});
                }
                if (flag && dic.Keys.Count > 0)
                {
                    helper.Log(ConsoleColor.Yellow, $"开始启动程序，请等待。。。");
                    this.button1.Enabled = false;
                    this.button2.Enabled = true;
                    Task.Run(() =>
                    {
                        try
                        {
                            foreach (var d in dic.Keys)
                            {
                                if (dic.TryGetValue(d, out User u))
                                {

                                    Task.Run(() =>
                                    {
                                        try
                                        {
                                            while (true)
                                            {
                                                if (u.cancelToken.IsCancellationRequested)
                                                {
                                                    break;
                                                }
                                                var hours = DateTime.Now.Hour;
                                                if (!u.IsMax && ListHours.Contains(hours))
                                                {
                                                    helper.Login(u);
                                                }
                                                if (u.IsMax)
                                                {
                                                    System.Threading.Thread.Sleep(1000 * 60 * 60);
                                                }
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            WeiXinHelper.CreateLog(u.Account, "★★★★★" + ex, 2);
                                            helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                                        }
                                        return 1;
                                    });
                                    Task.Run(() =>
                                    {
                                        try
                                        {
                                            while (true)
                                            {
                                                if (u.cancelToken.IsCancellationRequested)
                                                {
                                                    break;
                                                }
                                                var hours = DateTime.Now.Hour;
                                                if (!u.IsMax && ListHours.Contains(hours))
                                                {
                                                    waphelper.Login(u);
                                                }
                                                if (u.IsMax)
                                                {
                                                    System.Threading.Thread.Sleep(1000 * 60 * 60);
                                                }
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            WeiXinHelper.CreateLog(u.Account, "★★★★★" + ex, 2);
                                            helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                                        }
                                        return 1;
                                    });
                                    System.Threading.Thread.Sleep(5000);
                                }

                            }

                        }
                        catch (Exception ex)
                        {
                            WeiXinHelper.CreateLog("main", "★★★★★" + ex, 2);
                            helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                        }
                        return 1;
                    });



                   
                    isRun = true;
                }
                else
                {
                    helper.Log(ConsoleColor.Red, "没有数据！");
                }
                
            }


        }
        public void Srart(User u)
        {

        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (isRun)
            {
                foreach (var d in dic.Keys)
                {
                    if (dic.TryGetValue(d, out User u))
                    {
                        u.cancelToken.Cancel();
                        WeiXinHelper.SendText(u.Account, $"{u.Account} 已停止刷任务。",true);
                    }
                }
                isRun = false;
                helper.Log(ConsoleColor.Yellow, $"程序程序!");
                this.button1.Enabled = true;
                this.button2.Enabled = false;
            }


        }
    }
}
