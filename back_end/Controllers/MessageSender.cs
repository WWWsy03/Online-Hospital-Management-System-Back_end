namespace back_end.Controllers
{
    public class MessageSender
    {
        public static async Task<bool> SendSmsAsync(string PhoneNumber, string context)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                //创建链接到SMS API的链接
                string account = "C58169395";
                string password = "776f675d18a48ed284eaf032364e06f0";
                string mobile = PhoneNumber;
                string content = context;
                string url = "http://106.ihuyi.com/webservice/sms.php?method=Submit";
                string post = string.Format("&account={0}&password={1}&mobile={2}&content={3}", account, password, mobile, content);
                string request = url + post;
                HttpResponseMessage response = await httpClient.GetAsync(request);

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
