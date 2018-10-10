using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace leke.entity
{
    /// <summary>
    /// 企业号发送消息的基础消息内容
    /// </summary>
    public class CorpSendBase
    {
        /// <summary>
        /// UserID列表（消息接收者，多个接收者用‘|’分隔）。特殊情况：指定为@all，则向关注该企业应用的全部成员发送
        /// </summary>
        public string touser { get; set; }
        /// <summary>
        /// PartyID列表，多个接受者用‘|’分隔。当touser为@all时忽略本参数
        /// </summary>
        public string toparty { get; set; }
        /// <summary>
        /// TagID列表，多个接受者用‘|’分隔。当touser为@all时忽略本参数
        /// </summary>
        public string totag { get; set; }
        /// <summary>
        /// 消息类型
        /// </summary>
        public string msgtype { get; set; }
        /// <summary>
        /// 企业应用的id，整型。可在应用的设置页面查看
        /// </summary>
        public string agentid { get; set; }
        /// <summary>
        /// 表示是否是保密消息，0表示否，1表示是，默认0
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string safe { get; set; }
        public CorpSendBase()
        {
            this.agentid = System.Configuration.ConfigurationManager.AppSettings["CorpSendBaseAgentID"].ToString();
            this.safe = "0";
        }
    }

    public class Text
    {
        private string _content;
        /// <summary>
        /// 要发送的文本内容字段，必须小写，企业微信API不识别大写。
        /// </summary>
        public string content
        {
            get { return _content; }
            set { _content = value; }
        }

    }
    public class CorpSendText : CorpSendBase
    {
        private Text _text;
        /// <summary>
        /// 要发送的文本，必须小写，企业微信API不识别大写。
        /// </summary>
        public Text text
        {
            get { return _text; }
            set { this._text = value; }
        }


        public CorpSendText(string content)
        {
            base.msgtype = "text";
            this.text = new Text
            {
                content = content
            };
        }
    }

}
