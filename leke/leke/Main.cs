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
        public static DateTime refrenshTime;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            refrenshTime = DateTime.Now.AddDays(-1);
            Interval = 20000;
            Begin = 8;
            End = 22;
            dic = new ConcurrentDictionary<string, User>();
            a1 = new MessageBoxShow(ShowMessage);
            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        if (isRun)
                        {
                            RefreshUser();
                        }

                        System.Threading.Thread.Sleep(1000 * 60 * 10);
                        Refresh();
                        Console.Clear();

                    }
                }
                catch (Exception ex)
                {
                    helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                    WeiXinHelper.CreateLog("main", "★★★★★" + ex, 2);
                    WeiXinHelper.SendText("13142025891", "★★★★★" + ex, false);
                }
            });
            // var str = File.ReadAllText("Users.json", Encoding.Default);
            //var lst = JsonConvert.DeserializeObject<List<User>>(str);
            //var sb = new StringBuilder();

            this.dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;



        }
        private bool RefreshUser()
        {

            var list = WeiXinHelper.GetUsers();
            if (list.Count == 0)
            {
                WeiXinHelper.CreateLog("main", " 没有获取到任何用户信息", 2);
                WeiXinHelper.SendText("", " 没有获取到任何用户信息", true);
                MessageBox.Show("没有获取到任何用户信息 ,请确认!");
                return false;
            }
            else
            {
                WeiXinHelper.CreateLog("main", " 拉取用户数据成功", 1);
            }

            var startList = new List<string>();
            foreach (var user in list)
            {
                User u = null;
                if (dic.TryGetValue(user.Account, out u))
                {

                    if (user.IsRun)
                    {
                        if (!u.IsRun)
                        {
                            if (dic.TryRemove(user.Account, out u))
                            {

                                var newu = new User
                                {
                                    Account = u.Account,
                                    IsRun = true,
                                    Wap = user.Wap,
                                    BeginTime = user.BeginTime,
                                    Pass = u.Pass,
                                    UserName = u.UserName,
                                    WeiXinId = u.WeiXinId,
                                    cancelToken = new CancellationTokenSource(),
                                    IsComplete = u.IsComplete,
                                    IsMax = u.IsMax,
                                    IsWapMax = u.IsWapMax,
                                    HasBiaoqian = u.HasBiaoqian,
                                    EndTime = user.EndTime,
                                    days = user.days,
                                    PC = user.PC


                                };
                                ShowMessage(u, 2);
                                ShowMessage(newu, 1);

                                dic.TryAdd(newu.Account, newu);
                                startList.Add(newu.Account);

                            }

                        }
                        else
                        {
                            if (user.PC != u.PC)
                            {
                                u.IsRun = false;
                                u.cancelToken.Cancel();
                                if (dic.TryRemove(user.Account, out u))
                                {

                                    var newu = new User
                                    {
                                        Account = u.Account,
                                        IsRun = true,
                                        Wap = user.Wap,
                                        BeginTime = user.BeginTime,
                                        Pass = u.Pass,
                                        UserName = u.UserName,
                                        WeiXinId = u.WeiXinId,
                                        cancelToken = new CancellationTokenSource(),
                                        IsComplete = u.IsComplete,
                                        IsMax = u.IsMax,
                                        IsWapMax = u.IsWapMax,
                                        HasBiaoqian = u.HasBiaoqian,
                                        EndTime = user.EndTime,
                                        days = user.days,
                                        PC = user.PC


                                    };
                                    ShowMessage(u, 2);
                                    ShowMessage(newu, 1);

                                    dic.TryAdd(newu.Account, newu);
                                    startList.Add(newu.Account);

                                }
                            }
                            else
                            {
                                u.Wap = user.Wap;
                                u.PC = user.PC;
                                u.BeginTime = user.BeginTime;
                                u.EndTime = user.EndTime;
                                u.days = user.days;
                            }

                        }
                    }
                    else
                    {
                        u.IsRun = false;
                        u.cancelToken.Cancel();
                        if (user.group != WeiXinHelper.userGroup)
                        {
                            if (dic.TryRemove(user.Account, out u))
                            {
                                ShowMessage(u, 2);
                            }
                        }
                    }

                }
                else
                {
                    user.cancelToken = new CancellationTokenSource();
                    if (user.IsRun == true)
                    {

                        startList.Add(user.Account);
                    }
                    if (user.group == WeiXinHelper.userGroup)
                    {
                        dic.TryAdd(user.Account, user);
                        ShowMessage(user, 1);
                    }

                }
            }
            Start(startList);
            ShowMessage(null, 3);
            return true;
        }
        private void initUser()
        {
            var list = WeiXinHelper.GetUsers();
            if (list.Count == 0)
            {
                WeiXinHelper.CreateLog("main", " 没有获取到任何用户信息", 2);
                WeiXinHelper.SendText("13142025891", " 没有获取到任何用户信息", false);
            }


        }

        private void Refresh()
        {
            var hours = DateTime.Now.Hour;
            if (hours == Begin)
            {
                if (refrenshTime.Date != DateTime.Now.Date)
                {

                    foreach (var d in dic.Keys)
                    {
                        if (dic.TryGetValue(d, out User u))
                        {
                            u.IsMax = false;
                            u.HasBiaoqian = false;
                            u.IsWapMax = false;
                        }
                    }
                    refrenshTime = DateTime.Now;
                    WeiXinHelper.SendText("", $" {Begin}点初始化所有线程！", false);
                }
            }

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
        public void ShowMessage(User users, int type)
        {
            Invoke(new MessageBoxShow(MessageBoxShow_F), new object[] { users, type });
        }

        public delegate void MessageBoxShow(User users, int type);

        void MessageBoxShow_F(User users, int type)
        {
            var currentUsers = this.dataGridView1.DataSource as BindingList<User>;
            if (type == 1)
            {
                currentUsers.Add(users);
            }
            else if (type == 2)
            {
                currentUsers.Remove(users);
            }
            else
            {
                this.dataGridView1.Refresh();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
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
                    this.dataGridView1.DataSource = new BindingList<User>();
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

                    isRun = true;
                    this.button1.Enabled = false;
                    this.button2.Enabled = true;
                    var flag = RefreshUser();
                    if (!flag)
                    {
                        return;
                    }
                    helper.Log(ConsoleColor.Red, $"开始任务，开始时间{Begin}，结束时间 {End}");
                    helper.Log(ConsoleColor.Yellow, $"开始启动程序，请等待。。。");





                }
            }
            catch (Exception ex)
            {
                WeiXinHelper.CreateLog("main", "★★★★★" + ex, 2);
                helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                WeiXinHelper.SendText("", "★★★★★" + ex, true);
            }


        }
        private void Start(List<string> list)
        {
            Task.Run(() =>
            {
                try
                {
                    foreach (var d in list)
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
                                            helper.Log(ConsoleColor.Red, u.Account + "wap 停止,退出线程");
                                            WeiXinHelper.CreateLog(u.Account, "wap 停止,退出线程", 1);
                                            break;
                                        }

                                        var hours = DateTime.Now.Hour;
                                        if (!u.IsWapMax && ListHours.Contains(hours) && u.BeginTime <= hours)
                                        {
                                            waphelper.Login(u);
                                        }
                                        helper.Log(ConsoleColor.Red, u.Account + "wap 不再程序执行时间内或者 今天已经max!");
                                        WeiXinHelper.CreateLog(u.Account, "wap 不再程序执行时间内或者 今天已经max!", 1);
                                        u.HasBiaoqian = false;
                                        System.Threading.Thread.Sleep(1000 * 60 * 5);
                                        //Refresh(u);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    WeiXinHelper.CreateLog(u.Account, "★★★★★" + ex, 2);
                                    helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                                    WeiXinHelper.SendText("", "★★★★★" + ex, true);
                                }
                                return 1;
                            });

                            System.Threading.Thread.Sleep(1000 * 10);
                            if (u.PC)
                            {
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        while (true)
                                        {
                                            if (u.cancelToken.IsCancellationRequested)
                                            {
                                                helper.Log(ConsoleColor.Red, u.Account + " 停止,退出线程");
                                                WeiXinHelper.CreateLog(u.Account, "停止,退出线程", 1);
                                                break;

                                            }

                                            var hours = DateTime.Now.Hour;
                                            if (!u.IsMax && ListHours.Contains(hours) && u.BeginTime <= hours)
                                            {
                                                helper.Login(u);
                                            }
                                            helper.Log(ConsoleColor.Red, u.Account + " 不再程序执行时间内或者 今天已经max!");
                                            WeiXinHelper.CreateLog(u.Account, "不再程序执行时间内或者 今天已经max!", 1);
                                            System.Threading.Thread.Sleep(1000 * 10 * 5);
                                            u.HasBiaoqian = false;
                                            //Refresh(u);

                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        WeiXinHelper.CreateLog(u.Account, "★★★★★" + ex, 2);
                                        helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                                        WeiXinHelper.SendText("", "★★★★★" + ex, true);
                                    }
                                    return 1;
                                });
                            }

                            System.Threading.Thread.Sleep(10000);
                        }

                    }

                }
                catch (Exception ex)
                {
                    WeiXinHelper.CreateLog("main", "★★★★★" + ex, 2);
                    helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                    WeiXinHelper.SendText("", "★★★★★" + ex, true);
                }
                return 1;
            });
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
                        WeiXinHelper.SendText(u.Account, $"{u.Account} 已停止刷任务。", true);
                    }
                }
                isRun = false;
                helper.Log(ConsoleColor.Yellow, $"程序程序!");
                this.button1.Enabled = true;
                this.button2.Enabled = false;
            }


        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex > -1 && e.ColumnIndex == 4)
            {
                var acc = this.dataGridView1.Rows[e.RowIndex].Cells["Account"].Value.ToString();

                if (dic.TryGetValue(acc, out User u))
                {
                    DataGridViewTextBoxCell vCell = (DataGridViewTextBoxCell)this.dataGridView1.Rows[e.RowIndex].Cells["status"];
                    if (u.IsRun)
                    {
                        vCell.Value = "运行";

                    }
                    else
                    {
                        vCell.Value = "停止";
                    }
                }



            }

        }



        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = false;
        }
    }
}
