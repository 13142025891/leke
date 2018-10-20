

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace  leke
{
    public static class ImageManagerHelper
    {

        private const string UploadMethod = "upload";
        private const string DeleteMethod = "delete";
        /// <summary>
        /// 上传图片文件
        /// </summary>
        /// <param name="imgBytes">字节数组</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="timeOut">超时时间 默认不设置超时</param>
        /// <returns></returns>
        public static async Task<bool> ImageUpLoad(byte[] imgBytes, string fileName = "upload.jpg", int? timeOut = null)
        {
            
            try
            {
                var httpHandler = new HttpClientHandler();
                httpHandler.AllowAutoRedirect = false;
                using (HttpClient http = new HttpClient(httpHandler))
                {
                    if (timeOut.HasValue)
                    {
                        http.Timeout = TimeSpan.FromSeconds(timeOut.Value);
                    }
                    HttpResponseMessage message = null;
                    using (Stream dataStream = new MemoryStream(imgBytes))
                    {
                        using (var content = new MultipartFormDataContent(Guid.NewGuid().ToString().Replace("-", "")))
                        {
                            var imageContent = new StreamContent(dataStream);
                            imageContent.Headers.Add("Content-Disposition", $"form-data; name=\"Filedata\"; filename=\"{fileName}\"");
                            content.Add(imageContent);
                            var task = http.PostAsync($"{UploadMethod}", content);
                            message = task.Result;
                        }
                        if (message != null && message.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            using (message)
                            {
                                var resultStr = await message.Content.ReadAsStringAsync();
                                if (resultStr.StartsWith("{"))
                                {
                                    //return resultStr.FromJson<FileUploadRS>();
                                }
                                else
                                {
                                    //LogHelper.Error(() => $"ImageUpLoadl: error {resultStr}", LogTag.Default);
                                }
                            }
                        }
                        else
                        {
                           // LogHelper.Error(() => $"ImageUpLoadl: error 服务器出错 \n\r StatusCode:{message.StatusCode}", LogTag.Default);
                        }
                    }
                }
            }
            catch (AggregateException e)
            {
                if (!(e.InnerException is System.Threading.Tasks.TaskCanceledException))
                {
                    //LogHelper.Error(() => $"ImageUpLoadl: error 上传超时", LogTag.Default, e);
                }
                else
                {
                    //LogHelper.Error(() => $"ImageUpLoadl: error {e.Message}", LogTag.Default, e);
                }

            }
            catch (Exception e)
            {
                //LogHelper.Error(() => $"ImageUpLoadl: error {e.Message}", LogTag.Default, e);
            }
            return true;
        }


    }
}
