namespace back_end.Controllers
{
    public class MessageSender
    {
        public static async Task<bool> SendSmsAsync(string PhoneNumber, string context)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                //创建链接到SMS API的链接
                string Uid = "JieChu";
                string Key = "A6680534D182D0450B51BDF759C6477C";
                string smsMob = PhoneNumber;
                string smsText = context;
                string url = "http://utf8.api.smschinese.cn/";
                string post = string.Format($"?Uid={Uid}&Key={Key}&smsMob={smsMob}&smsText={smsText}");
                string request = url + post;
                //Console.WriteLine(request);
                HttpResponseMessage response = await httpClient.GetAsync(request);
                //Console.WriteLine(response);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // 此处添加处理响应内容的逻辑，比如将responseBody转为int判断是否成功
                    return true;
                }
                else
                {
                    // 请求失败，添加相应的错误处理逻辑
                    return false;
                }
            }
        }
    }
}
