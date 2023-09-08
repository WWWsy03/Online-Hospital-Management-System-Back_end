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
                Console.WriteLine(request);
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

        public static async Task<bool> SendSmsAsync4VC(string PhoneNumber, string VerifyCode)
        {
            using (HttpClient httpClient = new HttpClient())
            {
            //http://106.ihuyi.com/webservice/sms.php?method=Submit&account=C58169395&password=776f675d18a48ed284eaf032364e06f0&mobile=18282281462&content=您的验证码是：123
            //    4。请不要把验证码泄露给其他人。
                //创建链接到SMS API的链接
                string account = "C58169395";
                string password = "776f675d18a48ed284eaf032364e06f0";
                string mobile = PhoneNumber;
                string content = "您的验证码是："+VerifyCode+"。请不要把验证码泄露给其他人。";
                string url = "http://106.ihuyi.com/webservice/sms.php?method=Submit&";
                string post = string.Format($"account={account}&password={password}&mobile={mobile}&content={content}");
                string request = url + post;
                Console.WriteLine(request);
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
