using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace leke.entity
{
    public class User
    {
        public static User Clone(User u)
        {
            return new User
            {
                Account = u.Account,
                BeginTime = u.BeginTime,
                IsRun = true,
                Pass = u.Pass,
                UserName = u.UserName,
                Wap = u.Wap,
                WeiXinId = u.WeiXinId,
                cancelToken = new CancellationTokenSource()



            };
        }

        public static List<User> Clone(WeixinUser u)
        {
            var list = new List<User>();
            var userList = u.position.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (userList.Count > 0)
            {

                for (var i = 0; i < userList.Count; i++)
                {
                    var peizhi = u.alias.Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries)[i].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var str = userList[i].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var model = new User
                    {
                        Account = str[0],
                        BeginTime = Convert.ToInt32(peizhi[2]),
                        IsRun = peizhi[0] == "0" ? false : true,
                        Pass = str[1],
                        UserName = u.name,
                        Wap = peizhi[1] == "0" ? false : true,
                        WeiXinId = u.userid,
                        group = u.telephone
                    };

                    var time = DateTime.Now;
                    var days = 0;
                    if (!string.IsNullOrEmpty(u.email))
                    {
                        var email = u.email.Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        var times = email[0].Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        time = Convert.ToDateTime(times[0]);
                        days = Convert.ToInt32(times[1]);
                        model.EndTime = time.ToString("yyyy-MM-dd");
                        model.days = days.ToString();
                        if (time.AddDays(days + 1) < DateTime.Now)
                        {
                            model.IsRun = false;
                            WeiXinHelper.SendText(u.userid, $"{u.userid}  今天已经到期，明天将不再刷任务，继续购买请联系 ！", false);
                        }
                    }
                    if (u.telephone != WeiXinHelper.userGroup)
                    {
                        model.IsRun = false;
                    }
                    list.Add(model);
                }
            }
            return list;


        }
        public string Account { get; set; }
        public string Pass { get; set; }
        public bool IsComplete { get; set; }
        public CancellationTokenSource cancelToken { get; set; }
        public string UserName { get; set; }
        public bool IsMax { get; set; }
        public bool IsWapMax { get; set; }
        public bool HasBiaoqian { get; set; }
        public string WeiXinId { get; set; }

        public bool Wap { get; set; }
        public int BeginTime { get; set; }
        public bool IsRun { get; set; }

        public string EndTime { get; set; }
        public string days { get; set; }
        public string group { get; set; }
    }
    //{"challenge": "5b7adfba154c5a4b75a4daadb37faa04", "success": 1, "new_captcha": true, "gt": "46873be54fcede66ffe12752fe8beb10"}

    public class Gt
    {
        public string challenge { get; set; }
        public string success { get; set; }
        public bool new_captcha { get; set; }
        public string gt { get; set; }
    }

    public class Validate
    {
        public string status { get; set; }
        public string challenge { get; set; }
        public string validate { get; set; }

        public string msg { get; set; }
    }

    public class WeinUserResult
    {
        public string errcode { get; set; }
        public List<WeixinUser> userlist { get; set; }
    }

    public class WeixinUser
    {



        /// <summary>
        /// 名字，如果乐客账号一个用户不能填满，就使用name加_ 组合多个账号属于同一个人
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 微信别面：乐客 作为账号权限同一账号|,多账号@
        /// 第一位：是否运行：1运行，1停止，第二位：是否刷标签任务 1刷，0不刷，第三位 ：每天几点开始 配置0-23之间的数字
        /// </summary>
        public string alias { get; set; }

        public string userid { get; set; }

        /// <summary>
        /// 微信 电话，乐客 分组，多个程序部署在不同机器上 获取不同的用户
        /// </summary>
        public string telephone { get; set; }


        /// <summary>
        /// 微信的职务，存储账号密码 用|隔开，多个用,
        /// </summary>
        public string position { get; set; }

        /// <summary>
        /// 购买日期 和天数  2018.10.20_30
        /// </summary>
        public string email { get; set; }

    }

}
