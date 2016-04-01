using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Survey
{
    public partial class UI :Form
    {
        SpWebClient client = new SpWebClient();
        Encoding schoolEncoding = Encoding.GetEncoding("big5");
        List<string> idList = new List<string>();
        List<string> cmdList = new List<string>();
        string surveyURL = "http://netreg.isu.edu.tw/wapp/wap_13/wap_130100.asp";
        string patten = "<td><INPUT id=crcode[^>]+>([^<]+)</td><td>([^<]+)</td><td>([^<]+)</td><td>([^<]+)</td><INPUT id=surtype name=surtype.{33}command.{0,100}value=\x22([^\x22]+)\x22";
        public const string ChromeUserAgent = "Mozilla/5.0 (Linux; U; Android 4.0.3; ko-kr; LG-L160L Build/IML74K) AppleWebkit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30";
        public UI()
        {
            InitializeComponent();
        }

        bool send(string classCode,string cmdCode,string Choose)
        {
            string source = "";
            client.Headers.Add("Referer", "http://netreg.isu.edu.tw/wapp/wap_13/wap_130100.asp");
            client.Headers.Add("Accept-Encoding", "gzip, deflate");
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            client.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.Headers.Add("user-agent", ChromeUserAgent);
            String data = "crcode=" + classCode + "&surtype=0&command=" + cmdCode + "&submit1=%AD%D7%A7%EF%B0%DD%A8%F7";
            source = (client.UploadString(surveyURL, data));

            data = "cr_code=" + classCode;
            data += "&X01X06M1/Y=Y";
            data += "&X01X04M1/Y=Y";
            data += "&X08X10M1/Y=Y";
            /*foreach (Match item in new Regex(@"name=\x22([^\x22]+)\x22 type=checkbox value=\x22Y\x22(.*?)&nbsp").Matches(source))
            {
                if (!item.Groups[2].Value.Contains("其他"))
                    data += "&" + item.Groups[1].Value + "=Y";
            }*/
            foreach (Match item in new Regex(@"name=\x22([^\x22]+)\x22 type=radio value=\x22([^\x22]+)\x22>" + Choose).Matches(source))
                data += "&" + item.Groups[1].Value + "=" + item.Groups[2].Value;
            foreach (Match item in new Regex(@"name=\x22([^\x22]+)\x22 type=input value=\x22(.*?)\x22").Matches(source))
                data += "&" + item.Groups[1].Value + "=" + item.Groups[2].Value;

            data += "&submit1=%B6%F1%A6n%B0e%A5X";
            foreach (Match item in new Regex(@"name=([^ ]+) type=hidden value=\x22(.*?)\x22").Matches(source))
                data += "&" + item.Groups[1].Value + "=" + item.Groups[2].Value;

            client.Headers.Add("Referer", "http://netreg.isu.edu.tw/wapp/wap_13/wap_130100.asp");
            client.Headers.Add("Accept-Encoding", "gzip, deflate");
            client.Headers.Add("Accept-Language", "en,zh-TW;q=0.8,zh;q=0.6");
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            client.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.Headers.Add("user-agent", ChromeUserAgent);
            data = data.Replace("/", "%2F").Replace("#", "%23");
            source = (client.UploadString(surveyURL, (data)));

            if (source.Contains("您可填寫的課程意見評量表")) return true;
            return false;
        }


        void prepare()
        {
            this.Invoke(new MethodInvoker(() => {
                String source = client.DownloadString(surveyURL);
                source = source.Replace("\x09", "").Replace("\r\n", "");
                idList.Clear();
                cmdList.Clear();
                checkedListBox1.Items.Clear();
                foreach (Match item in new Regex(patten).Matches(source))
                {
                    var id = item.Groups[1].Value;
                    var name = item.Groups[2].Value.Replace("&nbsp;", ""); ;
                    var teacher = item.Groups[3].Value.Replace("&nbsp;", "");
                    var detail = item.Groups[4].Value.Replace("&nbsp;", ""); ;
                    var cmd = item.Groups[5].Value;
                    idList.Add(id);
                    cmdList.Add(cmd);
                    checkedListBox1.Items.Add(teacher + "\t" + detail + "\t-\t" + name);
                }
            }));
            
        }
        private void LoginBtn_Click(object sender, EventArgs e)
        {
            string source = "";
            NameValueCollection values = new NameValueCollection();
            
            values.Add("language", "zh_TW");
            values.Add("lange_sel", "zh_TW");
            values.Add("logon_id", this.textBox1.Text);
            values.Add("txtpasswd", this.textBox2.Text);
            values.Add("submit1", "submit1");
            client.Headers.Add("user-agent", ChromeUserAgent);
            client.Headers.Add("Referer", "http://netreg.isu.edu.tw/Wapp/wap_indexmain.asp?call_from=logout");
            source = System.Text.Encoding.Default.GetString(client.UploadValues("http://netreg.isu.edu.tw/Wapp/wap_check.asp", values));
            source = client.DownloadString("http://netreg.isu.edu.tw/Wapp/left.asp", schoolEncoding);

            Regex regName = new Regex("<span class=\"myFontClass\">([^<]+)</span></font></a>");
            if (regName.IsMatch(source))
            {
                Text = "義守期中意見問卷機器人v2 - " + regName.Match(source).Groups[1].Value + ", powered by 馬聖豪";
                Properties.Settings.Default.Save();
                this.LoginBtn.Enabled = false;
                prepare();
            }
            else
                MessageBox.Show("登入失敗！");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.button1.Enabled = false;
            (new Thread(() => {
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    if (checkedListBox1.GetItemChecked(i))
                        send(idList[i], cmdList[i], comboBox1.SelectedItem.ToString());
                }
                prepare();
            }) { IsBackground = true }).Start();
            
            this.button1.Enabled = true;
        }

        private void UI_Load(object sender, EventArgs e)
        {
            Form.CheckForIllegalCrossThreadCalls = false;
            comboBox1.SelectedIndex = 0;
        }


    }
}
