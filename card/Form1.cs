using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Globalization;
using System.Net.Mail;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net.Http;
namespace card
{
    public partial class Form1 : Form
    {

        private string a;
        private string ab = "123";

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await Task.Run(() => UpdateNtpTime());
        }

        private void UpdateNtpTime()
        {
            try
            {
                string ntpServer = "pool.ntp.org";
                using (var ntpClient = new UdpClient(ntpServer, 123))
                {
                    var ntpData = new byte[48];
                    ntpData[0] = 0x1B;
                    ntpClient.Send(ntpData, ntpData.Length);
                    var endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);
                    List<byte> responseDataList = new List<byte>();
                    int receivedBytes;
                    do
                    {
                        byte[] receivedData = ntpClient.Receive(ref endPoint);
                        receivedBytes = receivedData.Length;
                        responseDataList.AddRange(receivedData);
                    } while (receivedBytes == 48);
                    byte[] responseData = responseDataList.ToArray();
                    if (responseData.Length < 48)
                    {
                        throw new Exception("無效的 NTP 回應");
                    }
                    ulong intPart = BitConverter.ToUInt32(responseData, 43);
                    ulong fractPart = BitConverter.ToUInt32(responseData, 47);
                    ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                    DateTime ntpTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(milliseconds);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("例外情況: " + ex.ToString());
                MessageBox.Show("取得 NTP 時間時發生錯誤: " + ex.Message);
            }
        }

        private async Task<string> GetNtpTimeString()
        {
            try
            {
                DateTime ntpTime = await FetchNtpTimeAsync();
                return ntpTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                Console.WriteLine("獲取 NTP 時間時發生錯誤: " + ex.Message);
                throw;
            }
        }

        private async Task<DateTime> FetchNtpTimeAsync()
        {
            DateTime ntpTime = DateTime.UtcNow;
            string targetTimeZoneId = "Taipei Standard Time";
            TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(targetTimeZoneId);
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(ntpTime, targetTimeZone);
            return localTime;
        }


        private async void textBox1_TextChanged(object sender, EventArgs e)
        {

        }



        private async void button1_Click(object sender, EventArgs e)
        {

            try
            {
                string ntpTime = await GetNtpTimeString();
                string a = "xxx", b = "xxx";
                string abc = "現在時間\n";
                abc += ntpTime + "\n" + "名字: " + a + "座號: " + b + "已繳交";
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "vEdBisbXcROwnZkBbcRtMS4ZUqTQX9bIW0SQ2VllbZd");
                    var content = new Dictionary<string, string>
                {
                    { "message", abc },
                };

                    using (var response = await httpClient.PostAsync("https://notify-api.line.me/api/notify", new FormUrlEncodedContent(content)))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                var statusCode = (int)ex.StatusCode;
                var errorMessage = ex.Message;
                Console.WriteLine($"API 錯誤: 狀態碼 {statusCode}, 錯誤信息: {errorMessage}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("發生錯誤: " + ex.Message);
            }
        }

    }
}