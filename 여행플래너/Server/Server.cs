using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Packet;
using System.IO;
using ClassLibrary;

namespace Server
{
    public partial class Server : Form
    {
        delegate void AppendTextDelegate(Control ctrl, string s);
        AppendTextDelegate _textAppender;

        public List<NetworkStream> m_Stream;
        public List<TcpClient> m_Client;
        private List<bool> m_Connect;
        private List<Thread> m_ThReader;

        public IPAddress IPAddr;
        public int port;

        public bool m_bStop = false;
        private TcpListener m_listener;
        private Thread m_ThServer;
        public bool m_bConnect = false;
        int cnt = 0;

        List<string> files;

        List<List<TravelData>> travelDatas;

        public Server()
        {
            InitializeComponent();
            _textAppender = new AppendTextDelegate(AppendText);
            travelDatas = new List<List<TravelData>>();

            DirectoryInfo directory = new DirectoryInfo(Application.StartupPath + @"\save\");
            
            if (!directory.Exists) { directory.Create(); }
            int i = 0;
            foreach (var item in directory.GetFiles())
            {
                travelDatas.Add(new List<TravelData>());
                TravelData temp = new TravelData();
                temp.text = item.Name.Split('.')[0];
                temp.Type = (int)PacketType.채널;
                travelDatas[i].Add(temp);
                foreach (string str in File.ReadAllLines(Application.StartupPath + @"\save\" + item.Name))
                {
                    string[] a = str.Split('|');
                    TravelData t = new TravelData();
                    t.text = item.Name.Split('.')[0];
                    t.nickname = a[0];
                    t.Type = (int)PacketType.날짜;
                    if (a.Length > 1 && a[1] != "0")
                    {
                        if (a[1] == "1")
                        {
                            t.tts = "숙소";
                            t.btnType = 1;
                            MyAccommodation ac = new MyAccommodation();
                            ac.name = a[2];
                            ac.checkInTime = a[3];
                            ac.checkOutTime = a[4];
                            ac.fee = a[5];
                            ac.phoneNumber = a[6];
                            ac.address = a[7];
                            ac.other = a[8];
                            t.locationX = int.Parse(a[9]);
                            t.locationY = int.Parse(a[10]);
                            t.a = ac;
                        }
                        else if (a[1] == "2")
                        {
                            t.tts = "교통";
                            t.btnType = 2;
                            MyVehicle ve = new MyVehicle();
                            ve.name = a[2];
                            ve.departTime = a[3];
                            ve.arriveTime = a[4];
                            ve.takeTime = a[5];
                            ve.fee = a[6];
                            ve.other = a[7];
                            t.locationX = int.Parse(a[8]);
                            t.locationY = int.Parse(a[9]);
                            t.v = ve;
                        }
                        else if (a[1] == "3")
                        {
                            t.tts = "식사";
                            t.btnType = 3;
                            MyRestaurant re = new MyRestaurant();
                            re.name = a[2];
                            re.openingTime = a[3];
                            re.fee = a[4];
                            re.address = a[5];
                            re.phoneNumber = a[6];
                            re.other = a[7];
                            t.locationX = int.Parse(a[8]);
                            t.locationY = int.Parse(a[9]);
                            t.r = re;
                        }
                        else if (a[1] == "4")
                        {
                            t.tts = "관광지";
                            t.btnType = 4;
                            MySite si = new MySite();
                            si.name = a[2];
                            si.openingTime = a[3];
                            si.fee = a[4];
                            si.address = a[5];
                            si.phoneNumber = a[6];
                            si.other = a[7];
                            t.locationX = int.Parse(a[8]);
                            t.locationY = int.Parse(a[9]);
                            t.s = si;
                        }
                        t.Type = (int)PacketType.위젯;
                        
                    }

                    travelDatas[i].Add(t);
                }
                i++;
            }

        }

        void AppendText(Control ctrl, string s)
        {
            if (ctrl.InvokeRequired) ctrl.Invoke(_textAppender, ctrl, s);
            else
            {
                string source = ctrl.Text;
                ctrl.Text = source + Environment.NewLine + s;
            }
        }

        private void Server_Load(object sender, EventArgs e)
        {
            IPAddr = IPAddress.Any;
            txtIP.Text = IPAddr.ToString();
            port = 8888;
            txtPort.Text = port.ToString();

            m_Client = new List<TcpClient>();
            m_ThReader = new List<Thread>();
            m_Stream = new List<NetworkStream>();
            m_Connect = new List<bool>();

            files = new List<string>();

        }

        private void btnServer_Click(object sender, EventArgs e)
        {
            if (btnServer.Text == "서버켜기")
            {
                m_ThServer = new Thread(new ThreadStart(ServerStart));
                m_ThServer.Start();

                btnServer.Text = "서버끊기";
                btnServer.ForeColor = Color.Red;
            }
            else
            {
                ServerStop();
                btnServer.Text = "서버켜기";
                btnServer.ForeColor = Color.Black;
            }
        }

        public void ServerStart()
        {
            try
            {
                m_listener = new TcpListener(IPAddr, port);
                m_listener.Start();
                m_bStop = true;
                while (m_bStop)
                {
                    TcpClient hClient = m_listener.AcceptTcpClient();
                    m_Client.Add(hClient);

                    if (hClient.Connected)
                    {
                        NetworkStream hStream = hClient.GetStream();
                        hStream.Flush();
                        bool m_bConnect = true;
                        m_Connect.Add(m_bConnect);
                        m_Stream.Add(hStream);
                        Thread hThReader = new Thread(() => Receive(cnt++));
                        m_ThReader.Add(hThReader);
                        hThReader.Start();
                    }
                    else
                    {
                        hClient = m_listener.AcceptTcpClient();
                    }
                }
            }
            catch
            {
            }
        }
        public void ServerStop()
        {
            if (!m_bStop)
                return;

            foreach(var temps in travelDatas)
            {
                foreach (var temp in temps)
                {
                    if (temp.Type != (int)PacketType.채널)
                    {
                        string file_name = Application.StartupPath + @"\save\" + temp.text + ".txt";
                        string info = "";
                        if (temp.btnType == 1)
                        {
                            info = temp.a.name + "|" + temp.a.checkInTime + "|" + temp.a.checkOutTime + "|" + temp.a.fee + "|" + temp.a.phoneNumber + "|" + temp.a.address + "|" + temp.a.other + "|" + temp.locationX + "|" + temp.locationY;
                        }
                        else if (temp.btnType == 2)
                        {
                            info = temp.v.name + "|" + temp.v.departTime + "|" + temp.v.arriveTime + "|" + temp.v.takeTime + "|" + temp.v.fee + "|" + temp.v.other + "|" + temp.locationX + "|" + temp.locationY;
                        }
                        else if (temp.btnType == 3)
                        {
                            info = temp.r.name + "|" + temp.r.openingTime + "|" + temp.r.fee + "|" + temp.r.address + "|" + temp.r.phoneNumber + "|" + temp.r.other + "|" + temp.locationX + "|" + temp.locationY;
                        }
                        else if (temp.btnType == 4)
                        {
                            info = temp.s.name + "|" + temp.s.openingTime + "|" + temp.s.fee + "|" + temp.s.address + "|" + temp.s.phoneNumber + "|" + temp.s.other + "|" + temp.locationX + "|" + temp.locationY;
                        }

                        // 파일에 정보 추가
                        using (StreamWriter outputFile = new StreamWriter(file_name, true))
                        {
                            outputFile.WriteLine(temp.nickname + "|" + temp.btnType.ToString() + "|" + info);
                            outputFile.Close();
                        }

                    }

                }
            }
            

            m_listener.Stop();
            if (m_Stream != null)
                for (int i = 0; i < m_Stream.Count; i++)
                    if (m_Stream[i] != null)
                        m_Stream[i].Close();
            for (int i = 0; i < m_ThReader.Count; i++)
                m_ThReader[i].Abort();
            m_ThServer.Abort();
        }

        public void SendOther(NetworkStream stream, ref byte[] sendBuffer)
        {
            for (int i = 0; i < m_Stream.Count; i++)
            {
                if (m_Stream[i] != stream && m_Stream[i] != null)
                {
                    m_Stream[i].Write(sendBuffer, 0, sendBuffer.Length);
                    m_Stream[i].Flush();
                }
            }
            for (int j = 0; j < 1024 * 4; j++)
            {
                sendBuffer[j] = 0;
            }
        }
        public void SendBack(NetworkStream stream, ref byte[] sendBuffer)
        {
            stream.Write(sendBuffer, 0, sendBuffer.Length);
            stream.Flush();

            for (int i = 0; i < 1024 * 4; i++)
            {
                sendBuffer[i] = 0;
            }
        }

        public void Receive(int o)
        {
            byte[] readBuffer = new byte[1024 * 4];
            byte[] sendBuffer = new byte[1024 * 4];
            Array.Clear(readBuffer, 0, readBuffer.Length);
            Array.Clear(sendBuffer, 0, sendBuffer.Length);

            int nRead = 0;
            while (m_Connect[o])
            {
                try
                {
                    nRead = 0;
                    nRead = m_Stream[o].Read(readBuffer, 0, 1024 * 4);
                }
                catch
                {
                    m_Connect[o] = false;
                    m_Stream[o] = null;
                }

                TravelPacket packet = (TravelPacket)TravelPacket.Deserialize(readBuffer);
                switch ((int)packet.Type)
                {
                    case (int)PacketType.초기화:
                        {
                            bool next = false;
                            foreach (List<TravelData> data in travelDatas)
                            {
                                foreach (TravelData item in data)
                                {
                                    TravelPacket.Serialize(item).CopyTo(sendBuffer, 0);
                                    SendBack(m_Stream[o], ref sendBuffer);
                                }
                            }
                            break;
                        }
                    case (int)PacketType.채팅:
                        {
                            

                            readBuffer.CopyTo(sendBuffer, 0);
                            SendOther(m_Stream[o], ref sendBuffer);
                            break;
                        }
                    case (int)PacketType.정보:
                        {
                            TravelData travelData = (TravelData)TravelPacket.Deserialize(readBuffer);

                            readBuffer.CopyTo(sendBuffer, 0);
                            SendOther(m_Stream[o], ref sendBuffer);

                            foreach (var temps in travelDatas)
                            {
                                foreach (var temp in temps)
                                {
                                    if (temp.widgetIndex == travelData.widgetIndex)
                                    {
                                        if(travelData.btnType == 1)
                                        {
                                            temp.a = travelData.a;
                                        }
                                        else if (travelData.btnType == 2)
                                        {
                                            temp.v = travelData.v;
                                        }
                                        else if (travelData.btnType == 3)
                                        {
                                            temp.r = travelData.r;
                                        }
                                        else if (travelData.btnType == 4)
                                        {
                                            temp.s = travelData.s;
                                        }

                                    }
                                }
                            }
                            break;
                        }
                    case (int)PacketType.채널:
                        {
                            TravelData temp = (TravelData)TravelPacket.Deserialize(readBuffer);
                            string file_name = Application.StartupPath + @"\save\" + temp.text + ".txt";
                            
                            readBuffer.CopyTo(sendBuffer, 0);
                            SendOther(m_Stream[o], ref sendBuffer);

                            // 파일 생성
                            File.WriteAllText(file_name, "");
                            travelDatas.Add(new List<TravelData>());
                            travelDatas[travelDatas.Count - 1].Add(temp);


                            break;
                        }
                    case (int)PacketType.날짜:
                        {
                            TravelData temp = (TravelData)TravelPacket.Deserialize(readBuffer);
                            string file_name = Application.StartupPath + @"\save\" + temp.text + ".txt";

                            readBuffer.CopyTo(sendBuffer, 0);
                            SendOther(m_Stream[o], ref sendBuffer);
                            
                            // 파일에 정보 추가
                            //using (StreamWriter outputFile = new StreamWriter(file_name, true))
                            //{
                            //    outputFile.WriteLine(temp.nickname);
                            //    outputFile.Close();
                            //}

                            foreach (var item in travelDatas)
                            {
                                if (item[0].text == temp.text)
                                    item.Add(temp);
                            }

                            break;
                        }
                    case (int)PacketType.위젯:
                        {
                            TravelData temp = (TravelData)TravelPacket.Deserialize(readBuffer);                            

                            readBuffer.CopyTo(sendBuffer, 0);
                            SendOther(m_Stream[o], ref sendBuffer);

                            foreach (var item in travelDatas)
                            {
                                if (item[0].text == temp.text)
                                    item.Add(temp);
                            }

                            break;
                        }
                    case (int)PacketType.이동:
                        {
                            TravelData travelData = (TravelData)TravelPacket.Deserialize(readBuffer);

                            readBuffer.CopyTo(sendBuffer, 0);
                            SendOther(m_Stream[o], ref sendBuffer);
                            foreach(var temps in travelDatas)
                            {
                                foreach(var temp in temps)
                                {
                                    if (temp.widgetIndex == travelData.widgetIndex)
                                    {
                                        temp.locationX = travelData.locationX;
                                        temp.locationY = travelData.locationY;
                                        
                                    }
                                }
                            }
                            break;
                        }
                    case (int)PacketType.종료:
                        {
                            m_Connect[o] = false;
                            m_Stream[o].Close();
                            m_Stream[o] = null;
                            break;
                        }

                }

            }

        }

        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            ServerStop();
        }

        private void PanelBack_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void panHeader_MouseMove(object sender, MouseEventArgs e)
        {
            var s = sender as Panel;
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            s.Parent.Left = this.Left + (e.X - ((Point)s.Tag).X);
            s.Parent.Top = this.Top + (e.Y - ((Point)s.Tag).Y);
        }

        private void panHeader_MouseDown(object sender, MouseEventArgs e)
        {
            var s = sender as Panel;
            s.Tag = new Point(e.X, e.Y);
        }

        private void Label4_Click(object sender, EventArgs e)
        {

        }

        private void Button7_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
