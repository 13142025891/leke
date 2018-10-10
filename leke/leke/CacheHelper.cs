using leke.entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace leke
{
   public static class CacheHelper
    {
        private static readonly ConcurrentDictionary<string, DateTime> Queue = new ConcurrentDictionary<string, DateTime>();



        private static Msg _token;

        /// <summary>
        ///公共配置Config
        /// </summary>
       
        public static Msg Token
        {
            get
            {
                return Get(CacheKeys.Token, () =>
                {

                    _token = WeiXinHelper.GetQYAccessToken();


                }, ref _token, 60);
            }
        }










        /// <summary>
        /// 1.并发访问只会有一个真正的去请求，其它都在等结果。
        /// 2.成功一次后，以后都不会慢，至多数据几秒内不新鲜，因为数据过期后依然返回的是老数据，并且同时开了个线程在拿新数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="value"></param>
        /// <param name="expireMinutes">更新频率，此设置是用来保证新鲜度的，设置长或短不会影响性能！</param>
        /// <returns></returns>
        public static T Get<T>(CacheKeys key, Action action, ref T value, int expireMinutes)
        {
            bool hasError = false;
            if (key.NoCache()) //沒數據就嘗試去填充數據
            {
                var startTime = DateTime.Now;
                var isFirst = Queue.TryAdd(key.GetName(), startTime); //第一個進來的填充就行，其它的等結果吧
                if (isFirst)
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            if (key.NoCache())
                            {
                                action();
                                var cacheInfo = new CacheInfo
                                {
                                    Key = key.GetName(),
                                    Desc = key.GetDescription(),
                                    Count = 1,
                                    CreateTime = DateTime.UtcNow,
                                    ExpireTime = DateTime.UtcNow.AddMinutes(expireMinutes),
                                    BuildTime = (DateTime.Now - startTime)
                                };

                                HttpRuntime.Cache.Insert(cacheInfo.Key, cacheInfo, null, cacheInfo.ExpireTime.Value, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, CacheOnRemovedCallback);
                                // PkgHelper.WritePerfLog("CacheHelper", (DateTime.Now - startTime).Milliseconds, key.GetName(), $"items：{1} ", "Offline.Service");
                            }
                        }
                        catch (Exception ex)
                        {
                            hasError = true;
                            var director = AppDomain.CurrentDomain?.BaseDirectory?.ToLower() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(director) && director.Contains("scheduler"))
                            {
                                // TASK 报这个错误是正常的，不记日志
                            }
                            else
                            {
                              
                            }
                        }
                        finally
                        {
                            Queue.TryRemove(key.GetName(), out startTime);
                        }
                    });
                }
            }

            DateTime end = DateTime.Now.AddMinutes(5);
            while (DateTime.Now < end)
            {
                if (value != null || hasError)
                {
                    break;
                }
                Thread.Sleep(500);
            }
            return value;
        }
        /// <summary>
        /// 定义在从 System.Web.Caching.Cache 移除缓存项时通知应用程序的回调方法。
        /// </summary>
        /// <param name="key">从缓存中移除的键（当初由Add, Insert传入的）。</param>
        /// <param name="value">与从缓存中移除的键关联的缓存项（当初由Add, Insert传入的）。</param>
        /// <param name="reason">从缓存移除项的原因。 </param>
        private static void CacheOnRemovedCallback(string key, object value, CacheItemRemovedReason reason)
        {
            // PkgHelper.WritePerfLog("CacheHelper", 1, key, $"Remove Reason：{reason}", "Offline.Service");
        }
        //第一次調用時便加載到靜態變量里，以后不用再執行此操作，提速
        //不能將其改為public,不然有隱患
        private static readonly Dictionary<CacheKeys, Tuple<string, string>> AllKeyName = FillAllKeyName();

        private static Dictionary<CacheKeys, Tuple<string, string>> FillAllKeyName()
        {
            var dict = new Dictionary<CacheKeys, Tuple<string, string>>();
            foreach (var item in Enum.GetValues(typeof(CacheKeys)))
            {
                var key = (CacheKeys)item;
                var keyName = Enum.GetName(typeof(CacheKeys), item);

                var obj = Attribute.GetCustomAttribute(typeof(CacheKeys).GetField(keyName), typeof(DescriptionAttribute)) as DescriptionAttribute;
                var desc = obj == null ? null : obj.Description;

                dict.Add(key, new Tuple<string, string>("CacheHelper_" + keyName, desc));
            }
            return dict;
        }
        public static bool NoCache(this CacheKeys input)
        {
            return HttpRuntime.Cache[input.GetName()] == null;
        }

        public static string GetName(this CacheKeys input)
        {
            return AllKeyName[input].Item1;
        }

        public static string GetDescription(this CacheKeys input)
        {
            return AllKeyName[input].Item2;
        }

        public static T GetValue<T>(this CacheKeys input)
        {
            if (HttpRuntime.Cache[input.GetName()] is T)
            {
                return (T)HttpRuntime.Cache[input.GetName()];
            }
            else
            {
                return default(T);
            }
        }
    }
    public class CacheInfo
    {
        public string Key { get; set; }
        public string Desc { get; set; }
        public int? Count { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? ExpireTime { get; set; }
        public TimeSpan? BuildTime { get; set; }

        private bool? _HasValue;

        public bool HasValue
        {
            get
            {
                if (!_HasValue.HasValue)
                {
                    _HasValue = CreateTime.HasValue || (!string.IsNullOrWhiteSpace(Key) && HttpRuntime.Cache[Key] != null);
                }
                return _HasValue.Value;
            }
            set { _HasValue = value; }
        }
    }
    public enum CacheKeys
    {
        /// <summary>
        /// 通用配置
        /// </summary>
        [Description("token")]
        Token,

       
    }
}
