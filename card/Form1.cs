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
using System.Data;
using System.IO;
using System.ComponentModel.Design.Serialization;

/*
作者：黃振益
想試看看的話可以執行並參考#39~42的數值
想傳到自己的line的話,在這裡申請的api https://notify-bot.line.me/zh_TW/ 並貼到#105 or #121就可以了
*/


namespace card
{
    public partial class Form1 : Form
    {
        private HttpClient httpClient;
        public Form1()
        {
            InitializeComponent();
            textBox1.KeyDown += TextBox1_KeyDown;
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await Task.Run(() => UpdateNtpTime());
        }

        Dictionary<string, string> idMappings = new Dictionary<string, string>
        {
            { "1962338564", "蔡雅鈞  座號:32"},
            { "3528143108", "吳惟閎  座號:03"},
            { "3502290436", "洪凱綸  座號:11"},
            { "1961852420", "黃振益  座號:23"}
        };
        private void UpdateNtpTime()
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

        public async Task<string> GetNtpTimeString()
        {
            DateTime ntpTime = await FetchNtpTimeAsync();
            string ntptime = ("yyyy-MM-dd HH:mm:ss");
            return ntpTime.ToString();
        }

        private async Task<DateTime> FetchNtpTimeAsync()
        {
            DateTime ntpTime = DateTime.UtcNow;
            string targetTimeZoneId = "Taipei Standard Time";
            TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(targetTimeZoneId);
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(ntpTime, targetTimeZone);
            return localTime;
        }

        public async void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {

            if (textBox1.Text.Length == 10 && e.KeyCode == Keys.Enter)
            {
                DateTime now = DateTime.Now;
                DateTime latetime = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0);
                string id = textBox1.Text;
                string mappedText = idMappings.ContainsKey(id) ? idMappings[id] : "未知";
                if (mappedText == "未知")
                {
                    string ntpTime = await GetNtpTimeString();
                    string abc = "現在時間\n";
                    abc += ntpTime + "\n" + $"名字: {mappedText}";
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "vEdBisbXcROwnZkBbcRtMS4ZUqTQX9bIW0SQ2VllbZd");//notify vEdBisbXcROwnZkBbcRtMS4ZUqTQX9bIW0SQ2VllbZd
                    var content = new Dictionary<string, string>
                    {
                        { "message", abc },
                    };
                    using (var response = await httpClient.PostAsync("https://notify-api.line.me/api/notify", new FormUrlEncodedContent(content)))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    textBox1.Clear();
                }
                else if (now > latetime)
                {
                    string ntpTime = await GetNtpTimeString();
                    string abc = "現在時間\n";
                    abc += ntpTime + "\n" + $"名字: {mappedText}\n超過{latetime}\n已遲到";
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "vEdBisbXcROwnZkBbcRtMS4ZUqTQX9bIW0SQ2VllbZd");//專題群組 LPAr3fUyRDx3k4j7QkCptTE5K80TocuesPsDR0OxWur
                    var content = new Dictionary<string, string>
                    {
                        { "message", abc },
                    };
                    using (var response = await httpClient.PostAsync("https://notify-api.line.me/api/notify", new FormUrlEncodedContent(content)))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    textBox1.Clear();
                }
                else
                {
                    string ntpTime = await GetNtpTimeString();
                    string abc = "現在時間\n";
                    abc += ntpTime + "\n" + $"名字: {mappedText}\n未超過{latetime}\n打卡成功";
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "vEdBisbXcROwnZkBbcRtMS4ZUqTQX9bIW0SQ2VllbZd");
                    var content = new Dictionary<string, string>
                    {
                        { "message", abc },
                    };
                    using (var response = await httpClient.PostAsync("https://notify-api.line.me/api/notify", new FormUrlEncodedContent(content)))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    textBox1.Clear();
                }
            }
            else if (textBox1.Text.Length < 10 && e.KeyCode == Keys.Enter)
            {
                textBox1.Clear();
                MessageBox.Show("未知的值");
            }
        }
    }
}