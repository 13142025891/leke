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
            return   new User
            {
                Account = u.Account,
                BeginTime = u.BeginTime,
                IsRun = true,
                Pass = u.Pass,
                UserName = u.UserName,
                Wap = u.Wap,
                WeiXinId = u.WeiXinId,
                cancelToken = new CancellationTokenSource(),
                
                

            };
        }
        public string Account { get; set; }
        public string Pass { get; set; }
        public bool IsComplete { get; set; }
        public CancellationTokenSource cancelToken { get; set; }
        public string UserName { get; set; }
        public bool IsMax { get; set; }

        public string WeiXinId { get; set; }

        public bool Wap { get; set; }
        public int BeginTime { get; set; }
        public bool IsRun { get; set; }
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
        
        public string msg{get; set; }
    }

  

}
