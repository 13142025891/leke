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
using System.ComponentModel;
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
            Interval = 20000;
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
                        if (isRun)
                        {
                            RefreshUser();
                        }

                        System.Threading.Thread.Sleep(1000 * 60);
                        //Console.Clear();

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
            this.dataGridView1.DataSource = new BindingList<User>();
            //dataGridView1.DataBind();BindingList<PartSpec
            // this.textBox1.Text = sb.ToString();
            //foreach (var user in lst)
            //{
            //    user.IsRun = true;
            //    user.cancelToken = new CancellationTokenSource();
            //    dic.TryAdd(user.Account, user);
            //}

        }
        private bool RefreshUser()
        {
           
            var list = WeiXinHelper.GetUsers();
            if (list.Count == 0)
            {
                WeiXinHelper.CreateLog("main", " 没有获取到任何用户信息", 2);
                WeiXinHelper.SendText("13142025891", " 没有获取到任何用户信息", false);
                MessageBox.Show("没有获取到任何用户信息 ,请确认!");
                return false;
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
                                    Wap=user.Wap,
                                    BeginTime=user.BeginTime,
                                    Pass = u.Pass,
                                    UserName = u.UserName,
                                    WeiXinId = u.WeiXinId,
                                    cancelToken = new CancellationTokenSource(),
                                    IsComplete = u.IsComplete,
                                    IsMax = u.IsMax,
                                    HasBiaoqian = u.HasBiaoqian

                                };
                                ShowMessage(u,2);
                                ShowMessage(newu, 1);
                                
                                dic.TryAdd(newu.Account, newu);
                                startList.Add(newu.Account);

                            }

                        }
                        else
                        {
                            u.Wap = user.Wap;
                            u.BeginTime = user.BeginTime;
                        }
                    }
                    else
                    {
                        u.IsRun = false;
                        u.cancelToken.Cancel();
                    }

                }
                else
                {
                    user.cancelToken = new CancellationTokenSource();
                    if (user.IsRun == true)
                    {

                        startList.Add(user.Account);
                    }
                    dic.TryAdd(user.Account, user);
                    ShowMessage(user, 1);
                }
            }
            Start(startList);
            //ShowMessage(currentUsers);
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

        private void Refresh(User u)
        {
            var hours = DateTime.Now.Hour;
            if (hours >= Begin)
            {
                u.IsMax = false;
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
            Invoke(new MessageBoxShow(MessageBoxShow_F), new object[] { users,type });
        }

        public delegate void MessageBoxShow(User users, int type);

        void MessageBoxShow_F(User users,int type)
        {
            var currentUsers = this.dataGridView1.DataSource as BindingList<User>;
            if (type == 1)
            {
                currentUsers.Add(users);
            }
            else {
                currentUsers.Remove(users);
            }
            //this.dataGridView1.DataSource = users;
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

                    //var s = new User()
                    //{
                    //    Account = "d"
                    //};
                    //var u= this.dataGridView1.DataSource as  List<User>;
                    //u.Add(s);

                    //this.dataGridView1.DataSource = u;
                    //dataGridView1.Refresh();

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
                WeiXinHelper.SendText("13142025891", "★★★★★" + ex, false);
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
                                            helper.Log(ConsoleColor.Red, u.Account + " 停止");
                                            break;

                                        }

                                        var hours = DateTime.Now.Hour;
                                        if (!u.IsMax && ListHours.Contains(hours) && u.BeginTime <= hours)
                                        {
                                            helper.Login(u);
                                        }

                                        System.Threading.Thread.Sleep(1000 * 60 * 10);
                                        u.HasBiaoqian = false;
                                        Refresh(u);

                                    }

                                }
                                catch (Exception ex)
                                {
                                    WeiXinHelper.CreateLog(u.Account, "★★★★★" + ex, 2);
                                    helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                                    WeiXinHelper.SendText("13142025891", "★★★★★" + ex, false);
                                }
                                return 1;
                            });

                            if (u.Wap)
                            {
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        while (true)
                                        {
                                            if (u.cancelToken.IsCancellationRequested)
                                            {
                                                helper.Log(ConsoleColor.Red, u.Account + " 停止");
                                                break;
                                            }

                                            var hours = DateTime.Now.Hour;
                                            if (!u.IsMax && ListHours.Contains(hours))
                                            {
                                                waphelper.Login(u);
                                            }
                                            u.HasBiaoqian = false;
                                            System.Threading.Thread.Sleep(1000 * 60 * 10);
                                            Refresh(u);
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        WeiXinHelper.CreateLog(u.Account, "★★★★★" + ex, 2);
                                        helper.Log(ConsoleColor.Red, "★★★★★" + ex);
                                        WeiXinHelper.SendText("13142025891", "★★★★★" + ex, false);
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
                    WeiXinHelper.SendText("13142025891", "★★★★★" + ex, false);
                }
                return 1;
            });
        }

        private void StartFlush()
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
            if (e.RowIndex > -1&& e.ColumnIndex==4)
            {
                var acc = this.dataGridView1.Rows[e.RowIndex].Cells["Account"].Value.ToString();

                if (dic.TryGetValue(acc, out User u))
                {
                    DataGridViewButtonCell vCell = (DataGridViewButtonCell)this.dataGridView1.Rows[e.RowIndex].Cells[4];
                    if (u.IsRun)
                    {
                        vCell.Value = "停止";

                    }
                    else
                    {
                        vCell.Value = "开始";
                    }
                }



            }

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                var acc = this.dataGridView1.Rows[e.RowIndex].Cells["Account"].Value.ToString();

                if (dic.TryGetValue(acc, out User u))
                {

                    if (dataGridView1.Columns[e.ColumnIndex].Name == "operation")
                    {
                        DataGridViewButtonCell vCell = (DataGridViewButtonCell)dataGridView1.CurrentCell;
                        //dataGridView1.Columns[e.ColumnIndex].ValueType.

                        if (vCell.FormattedValue.ToString() == "停止")
                        {
                            vCell.Value = "开始";
                            u.cancelToken.Cancel();
                            u.IsRun = false;
                        }
                        else
                        {
                            if (dic.TryRemove(acc, out User uu))
                            {
                                vCell.Value = "停止";
                                //u.cancelToken = new CancellationTokenSource();

                                var newu = new User
                                {
                                    Account = u.Account,
                                    BeginTime = u.BeginTime,
                                    IsRun = true,
                                    Pass = u.Pass,
                                    UserName = u.UserName,
                                    Wap = u.Wap,
                                    WeiXinId = u.WeiXinId,
                                    cancelToken = new CancellationTokenSource(),
                                    IsComplete = u.IsComplete,
                                    IsMax = u.IsMax

                                };
                                var users = this.dataGridView1.DataSource as BindingList<User>;
                                users.Remove(uu);
                                users.Add(newu);
                                dic.TryAdd(newu.Account, newu);
                                Start(new List<string>() { u.Account });
                                //this.dataGridView1.DataSource = users;
                            }

                        }

                    }
                    else
                    {
                        DataGridViewTextBoxCell vCell = (DataGridViewTextBoxCell)this.dataGridView1.Rows[e.RowIndex].Cells[3];

                        var BeginTime = this.dataGridView1.Rows[e.RowIndex].Cells["BeginTime"].Value;
                        var flag = int.TryParse(BeginTime.ToString(), out int be);
                        vCell.Value = be > 23 ? 0 : be;
                        if (!flag)
                        {
                            MessageBox.Show("请输入0-23的数字！");
                        }



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
