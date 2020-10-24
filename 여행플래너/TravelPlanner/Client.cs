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
using ClassLibrary;
using System.Threading;
using Packet;

namespace TravelPlanner
{
    public partial class Client : Form
    {
        //----------Invoke----------
        delegate void AppendTextDelegate(Control ctrl, string s);
        AppendTextDelegate _textAppender;
        //----------Invoke----------
        //----------통신------------
        public NetworkStream m_Stream;
        private byte[] sendBuffer = new byte[1024 * 4];
        private byte[] readBuffer = new byte[1024 * 4];
        public IPAddress IPAddr;
        public int port;
        public string nickname;
        public bool m_bConnect = false;
        TcpClient m_Client;
        private Thread m_ThReader;

        int currChan = -1;
        int currDay = -1;
        //----------통신------------
        //----------위젯 부분----------
        List<Button> channelBtns;
        List<string> channelNames;
        List<List<Button>> days;
        List<List<List<Button>>> widgets;

        DoubleClickButton[] newPanelButton;
        int widgetNumber = 0;               // 각 위젯에 번호를 부여
        int accommodationWidgetNumber = 0;
        int vehicleWidgetNumber = 0;
        int restaurantWidgetNumber = 0;
        int siteWidgetNumber = 0;
        MyAccommodation[] myaccommodation;
        MyVehicle[] myvehicle;
        MyRestaurant[] myrestaurant;
        MySite[] mysite;

        bool buttonMoveFlag = false;
        //----------위젯 부분----------

        //----------UI부분-------------
        List<Label> chatLog;
        bool isMaximized = false;       // 폼이 최대화 되었는지를 확인하는 변수
        int cse;
        //----------UI부분-------------

        public Client()
        {
            InitializeComponent();
            _textAppender = new AppendTextDelegate(AppendText);
            //----------UI부분-------------
            chatLog = new List<Label>();
            cse = 1;
            //----------UI부분-------------

            //----------위젯 부분----------
            newPanelButton = new DoubleClickButton[400];
            myaccommodation = new MyAccommodation[100];
            myvehicle = new MyVehicle[100];
            myrestaurant = new MyRestaurant[100];
            mysite = new MySite[100];

            for (int i = 0; i < 400; i++)
                newPanelButton[i] = new DoubleClickButton();

            for (int i = 0; i < 100; i++)
                myaccommodation[i] = new MyAccommodation();
            for (int i = 0; i < 100; i++)
                myvehicle[i] = new MyVehicle();
            for (int i = 0; i < 100; i++)
                myrestaurant[i] = new MyRestaurant();
            for (int i = 0; i < 100; i++)
                mysite[i] = new MySite();

            channelBtns = new List<Button>();
            days = new List<List<Button>>();
            widgets = new List<List<List<Button>>>();
            channelNames = new List<string>();
            //----------위젯 부분----------



            //시스템 트레이
            this.FormClosing += Client_FormClosing;
            this.notifyIcon1.DoubleClick += notifyIcon1_DoubleClick;
            this.종료ToolStripMenuItem.Click += 종료ToolStripMenuItem_Click;
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

        private void Client_Load(object sender, EventArgs e)
        {
            IPAddr = IPAddress.Loopback;
            port = 8888;
        }

        //----------통신------------
        public void Connect()
        {
            IPAddr = IPAddress.Parse(txtIP.Text);
            m_Client = new TcpClient();
            try
            {
                m_Client.Connect(IPAddr, port);
            }
            catch
            {
                Close();
            }
            m_bConnect = true;
            m_Stream = m_Client.GetStream();
            m_ThReader = new Thread(Receive);
            m_ThReader.Start();

            currChan = 0;
            currDay = 0;
            TravelPacket temp = new TravelPacket();
            temp.Type = (int)PacketType.초기화;
            TravelPacket.Serialize(temp).CopyTo(sendBuffer, 0);
            Send();
        }

        public void Disconnect()
        {
            if (!m_bConnect)
                return;
            m_ThReader.Abort();
            TravelPacket temp = new TravelPacket();
            temp.Type = (int)PacketType.종료;
            TravelPacket.Serialize(temp).CopyTo(sendBuffer, 0);
            Send();
            m_bConnect = false;
            m_Stream.Close();
        }

        public void Send()
        {
            m_Stream.Write(sendBuffer, 0, sendBuffer.Length);
            m_Stream.Flush();

            for (int i = 0; i < 1024 * 4; i++)
            {
                sendBuffer[i] = 0;
            }
        }

        public void Receive()
        {
            int nRead = 0;
            while (m_bConnect)
            {
                try
                {
                    nRead = 0;
                    nRead = m_Stream.Read(readBuffer, 0, 1024 * 4);
                }
                catch
                {
                    m_bConnect = false;
                    m_Stream = null;
                }

                TravelPacket packet = (TravelPacket)TravelPacket.Deserialize(readBuffer);
                switch ((int)packet.Type)
                {
                    case (int)PacketType.초기화:
                        {

                            break;
                        }
                    case (int)PacketType.채팅:
                        {
                            Invoke(new MethodInvoker(delegate ()
                            {
                                TravelData temp = (TravelData)TravelPacket.Deserialize(readBuffer);
                                otherChat(temp.tts, temp.nickname);

                            }));
                            break;
                        }
                    case (int)PacketType.채널:
                        {
                            Invoke(new MethodInvoker(delegate ()
                            {
                                TravelData temp = (TravelData)TravelPacket.Deserialize(readBuffer);
                                //채널버튼생성
                                channelNames.Add(temp.text);
                                RoundButton chanBtn = new RoundButton();
                                channel_Create(ref chanBtn);

                                int indexC = channelBtns.Count - 1;
                                int indexD = 0;

                                //날짜버튼생성
                                days.Add(new List<Button>());
                                Button dayBtn = new Button();
                                day_Create(ref dayBtn, indexC);
                                days[indexC].Add(dayBtn);

                                chanBtn.BackColor = Color.FromArgb(41, 43, 47);
                                chanBtn.FlatAppearance.BorderSize = 0;
                                chanBtn.FlatStyle = FlatStyle.Flat;

                                switch (cse)
                                {
                                    case 1:
                                        chanBtn.Image = Properties.Resources.i1;
                                        cse++;
                                        break;
                                    case 2:
                                        chanBtn.Image = Properties.Resources.i2;
                                        cse++;
                                        break;
                                    case 3:
                                        chanBtn.Image = Properties.Resources.i3;
                                        cse++;
                                        break;
                                    case 4:
                                        chanBtn.Image = Properties.Resources.i4;
                                        cse++;
                                        break;
                                    case 5:
                                        chanBtn.Image = Properties.Resources.i5;
                                        cse++;
                                        break;
                                    case 6:
                                        chanBtn.Image = Properties.Resources.i6;
                                        cse++;
                                        break;
                                    case 7:
                                        chanBtn.Image = Properties.Resources.i1;
                                        cse = 2;
                                        break;
                                }

                                dayBtn.Image = Properties.Resources.folder;
                                dayBtn.BackColor = Color.White;
                                dayBtn.ForeColor = Color.Black;
                                dayBtn.Font = new Font("맑은 고딕", 7F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(129)));

                                panel5.Controls.Add(dayBtn);

                                if (currChan != indexC)
                                    dayBtn.Visible = false;
                                //위젯공간생성
                                widgets.Add(new List<List<Button>>());
                                widgets[indexC].Add(new List<Button>());
                                if (currChan == -1)
                                {
                                    currChan = 0;
                                    currDay = 0;
                                }

                            }));

                            break;
                        }
                    case (int)PacketType.날짜:
                        {
                            Invoke(new MethodInvoker(delegate ()
                            {
                                TravelData temp = (TravelData)TravelPacket.Deserialize(readBuffer);
                                int indexC = 0;
                                for (int i = 0; i < channelNames.Count; i++)
                                    if (channelNames[i] == temp.text)
                                        indexC = i;

                                //날짜버튼 생성
                                Button dayBtn = new Button();
                                day_Create(ref dayBtn, indexC);
                                days[indexC].Add(dayBtn);
                                currDay = days[indexC].Count - 1;

                                dayBtn.Image = Properties.Resources.folder;
                                dayBtn.BackColor = Color.White;
                                dayBtn.ForeColor = Color.Black;
                                dayBtn.Font = new Font("맑은 고딕", 7F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(129)));

                                panel5.Controls.Add(dayBtn);
                                if (currChan != indexC)
                                    dayBtn.Visible = false;

                                //위젯공간생성
                                widgets.Add(new List<List<Button>>());
                                widgets[indexC].Add(new List<Button>());


                            }));
                            break;
                        }
                    case (int)PacketType.위젯:
                        {
                            Invoke(new MethodInvoker(delegate ()
                            {
                                TravelData temp = (TravelData)TravelPacket.Deserialize(readBuffer);
                                int indexC = 0;
                                int indexD = 0;
                                for (int i = 0; i < channelNames.Count; i++)
                                    if (channelNames[i] == temp.text)
                                        indexC = i;
                                for (int i = 0; i < days[indexC].Count; i++)
                                    if (days[indexC][i].Text == temp.nickname)
                                        indexD = i;
                                ButtonCreat(temp.tts);
                                widgets[indexC][indexD].Add(newPanelButton[widgetNumber-1]);
                                newPanelButton[widgetNumber - 1].Location = new Point(temp.locationX, temp.locationY);
                                if (currDay != temp.day || currChan != temp.chan)
                                    newPanelButton[widgetNumber - 1].Visible = false;


                                for (int widgetIndex = 0; widgetIndex < widgetNumber; widgetIndex++)    // 위젯 번호 검색
                                {
                                    if (newPanelButton[widgetNumber - 1] == newPanelButton[widgetIndex])
                                    {
                                        if (newPanelButton[widgetNumber - 1].BackColor == Color.FromArgb(42, 182, 199))
                                        {
                                            for (int accIndex = 0; accIndex < accommodationWidgetNumber; accIndex++)    // 숙소위젯 인덱스 검색
                                            {
                                                if (myaccommodation[accIndex].getWidgetNumber() == widgetIndex)    // 클릭한 숙소위젯의 위젯No가 widgetIndex와 일치하면
                                                {
                                                    myaccommodation[accIndex].name = temp.a.name;
                                                    myaccommodation[accIndex].checkInTime = temp.a.checkInTime;
                                                    myaccommodation[accIndex].checkOutTime = temp.a.checkOutTime;
                                                    myaccommodation[accIndex].fee = temp.a.fee;
                                                    myaccommodation[accIndex].phoneNumber = temp.a.phoneNumber;
                                                    myaccommodation[accIndex].address = temp.a.address;
                                                    myaccommodation[accIndex].other = temp.a.other;
                                                    break;
                                                }
                                            }
                                        }
                                        else if (newPanelButton[widgetNumber - 1].BackColor == Color.FromArgb(243, 153, 41))
                                        {
                                            for (int vehicleIndex = 0; vehicleIndex < vehicleWidgetNumber; vehicleIndex++)    // 교통위젯 인덱스 검색
                                            {
                                                if (myvehicle[vehicleIndex].getWidgetNumber() == widgetIndex)    // 클릭한 교통위젯의 위젯No가 widgetIndex와 일치하면
                                                {

                                                    myvehicle[vehicleIndex].name = temp.v.name;
                                                    myvehicle[vehicleIndex].departTime = temp.v.departTime;
                                                    myvehicle[vehicleIndex].arriveTime = temp.v.arriveTime;
                                                    myvehicle[vehicleIndex].takeTime = temp.v.takeTime;
                                                    myvehicle[vehicleIndex].fee = temp.v.fee;
                                                    myvehicle[vehicleIndex].other = temp.v.other;


                                                    break;
                                                }
                                            }
                                        }
                                        else if (newPanelButton[widgetNumber - 1].BackColor == Color.FromArgb(227, 51, 72))
                                        {
                                            for (int restaurantIndex = 0; restaurantIndex < restaurantWidgetNumber; restaurantIndex++)    // 식사위젯 인덱스 검색
                                            {
                                                if (myrestaurant[restaurantIndex].getWidgetNumber() == widgetIndex)    // 클릭한 식사위젯의 위젯No가 widgetIndex와 일치하면
                                                {

                                                    myrestaurant[restaurantIndex].name = temp.r.name;
                                                    myrestaurant[restaurantIndex].openingTime = temp.r.openingTime;
                                                    myrestaurant[restaurantIndex].fee = temp.r.fee;
                                                    myrestaurant[restaurantIndex].address = temp.r.address;
                                                    myrestaurant[restaurantIndex].phoneNumber = temp.r.phoneNumber;
                                                    myrestaurant[restaurantIndex].other = temp.r.other;
                                                    break;
                                                }
                                            }
                                        }
                                        else if (newPanelButton[widgetNumber - 1].BackColor == Color.FromArgb(178, 204, 55))
                                        {
                                            for (int siteIndex = 0; siteIndex < siteWidgetNumber; siteIndex++)    // 관광지위젯 인덱스 검색
                                            {
                                                if (mysite[siteIndex].getWidgetNumber() == widgetIndex)    // 클릭한 관광지위젯의 위젯No가 widgetIndex와 일치하면
                                                {

                                                    mysite[siteIndex].name = temp.s.name;
                                                    mysite[siteIndex].openingTime = temp.s.openingTime;
                                                    mysite[siteIndex].fee = temp.s.fee;
                                                    mysite[siteIndex].address = temp.s.address;
                                                    mysite[siteIndex].phoneNumber = temp.s.phoneNumber;
                                                    mysite[siteIndex].other = temp.s.other;

                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }


                            }));

                            day_Toggle(days[currChan][currDay]);

                            break;
                        }
                    case (int)PacketType.이동:
                        {
                            Invoke(new MethodInvoker(delegate ()
                            {
                                TravelData temp = (TravelData)TravelPacket.Deserialize(readBuffer);
                                widgets[temp.chan][temp.day][temp.index].Location = new Point(temp.locationX, temp.locationY);
                            }));
                            break;
                        }
                    case (int)PacketType.정보:
                        {
                            Invoke(new MethodInvoker(delegate ()
                            {
                                TravelData temp = (TravelData)TravelPacket.Deserialize(readBuffer);

                                if (temp.btnType == 1)
                                {
                                    myaccommodation[temp.color].name = temp.a.name;
                                    myaccommodation[temp.color].checkInTime = temp.a.checkInTime;
                                    myaccommodation[temp.color].checkOutTime = temp.a.checkOutTime;
                                    myaccommodation[temp.color].fee = temp.a.fee;
                                    myaccommodation[temp.color].phoneNumber = temp.a.phoneNumber;
                                    myaccommodation[temp.color].address = temp.a.address;
                                    myaccommodation[temp.color].other = temp.a.other;
                                }
                                else if (temp.btnType == 2)
                                {
                                    myvehicle[temp.color].name = temp.v.name;
                                    myvehicle[temp.color].departTime = temp.v.departTime;
                                    myvehicle[temp.color].arriveTime = temp.v.arriveTime;
                                    myvehicle[temp.color].takeTime = temp.v.takeTime;
                                    myvehicle[temp.color].fee = temp.v.fee;
                                    myvehicle[temp.color].other = temp.v.other;
                                }
                                else if (temp.btnType == 3)
                                {
                                    myrestaurant[temp.color].name = temp.r.name;
                                    myrestaurant[temp.color].openingTime = temp.r.openingTime;
                                    myrestaurant[temp.color].fee = temp.r.fee;
                                    myrestaurant[temp.color].address = temp.r.address;
                                    myrestaurant[temp.color].phoneNumber = temp.r.phoneNumber;
                                    myrestaurant[temp.color].other = temp.r.other;
                                }
                                else if (temp.btnType == 4)
                                {
                                    mysite[temp.color].name = temp.s.name;
                                    mysite[temp.color].openingTime = temp.s.openingTime;
                                    mysite[temp.color].fee = temp.s.fee;
                                    mysite[temp.color].address = temp.s.address;
                                    mysite[temp.color].phoneNumber = temp.s.phoneNumber;
                                    mysite[temp.color].other = temp.s.other;
                                }

                            }));
                            break;
                        }
                }
            }
        }


        private void btnConnect_Click(object sender, EventArgs e)
        {
            panel6.Visible = false;
            panel7.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            nickname = txtName.Text;
            txtIP.Visible = false;
            txtName.Visible = false;
            button9.Visible = false;

            currChan = 0;
            currDay = 0;
            Connect();
        }
        //----------통신------------

        //----------채팅------------
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!m_bConnect)
            {
                MsgBoxHelper.Warn("서버가 연결되지 않았습니다.");
                txtSend.Text = "";
                return;
            }
            // 보낼 텍스트
            string tts = txtSend.Text.Trim();
            if (string.IsNullOrEmpty(tts))
            {
                MsgBoxHelper.Warn("텍스트가 입력되지 않았습니다!");
                txtSend.Focus();
                return;
            }
            userChat(tts);
            TravelData temp = new TravelData();
            temp.Type = (int)PacketType.채팅;
            temp.nickname = txtName.Text;
            temp.tts = tts;
            TravelPacket.Serialize(temp).CopyTo(sendBuffer, 0);
            Send();
            txtSend.Clear();
        }

        private void userChat(string content)
        {
            int monY = Screen.PrimaryScreen.Bounds.Height;

            panel8.VerticalScroll.Value = panel8.VerticalScroll.Maximum; // 채팅이 들어오면 스크롤 내림

            Label a = new Label();
            a.Text = content;
            a.BackColor = Color.FromArgb(255, 235, 51);
            a.Font = new Font("맑은 고딕", 9F, FontStyle.Regular, GraphicsUnit.Point, (byte)(129));
            if (this.WindowState == FormWindowState.Maximized)
            {
                a.Location = new Point(135, monY - 330);
            }
            else
                a.Location = new Point(135, 340);

            a.MaximumSize = new Size(90, 0);
            a.AutoSize = true;
            a.Visible = false;
            panel8.Controls.Add(a);
            if(a.Height > 15)
                a.Location = new Point(135, 345 - a.Height);
            a.Visible = true;
            panel8.Controls.Add(a);

            Label b = new Label();
            b.Text = nickname; //nickname
            b.BackColor = Color.Transparent;
            b.ForeColor = Color.Black;
            b.Font = new Font("맑은 고딕", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(129)));
            b.Location = new Point(190, a.Location.Y - 25);
            b.AutoSize = true;
            panel8.Controls.Add(b);

            int X = a.Location.Y + a.Height - b.Location.Y + 12;
            foreach (Label la in chatLog)
            {
                la.Location = new Point(la.Location.X, la.Location.Y - X); 
            }
            chatLog.Add(a);
            chatLog.Add(b);
        }

        private void otherChat(string content, string nickname)
        {
            int monY = Screen.PrimaryScreen.Bounds.Height;

            panel8.VerticalScroll.Value = panel8.VerticalScroll.Maximum; // 채팅이 들어오면 스크롤 내림

            Label a = new Label();
            a.Text = content;
            a.BackColor = Color.FromArgb(255, 255, 255);
            a.Font = new Font("맑은 고딕", 9F, FontStyle.Regular, GraphicsUnit.Point, (byte)(129));
            if (this.WindowState == FormWindowState.Maximized)
            {
                a.Location = new Point(10, monY - 330);
            }
            else
                a.Location = new Point(10, 340);
            a.MaximumSize = new Size(90, 0);
            a.AutoSize = true;
            a.Visible = false;
            panel8.Controls.Add(a);
            if (a.Height > 15)
                a.Location = new Point(10, 345 - a.Height);
            a.Visible = true;
            panel8.Controls.Add(a);

            Label b = new Label();
            b.Text = nickname; //nickname
            b.BackColor = Color.Transparent;
            b.ForeColor = Color.Black;
            b.Font = new Font("맑은 고딕", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(129)));
            b.Location = new Point(10, a.Location.Y - 25);
            b.AutoSize = true;
            panel8.Controls.Add(b);

            int X = a.Location.Y + a.Height - b.Location.Y + 12;
            foreach (Label la in chatLog)
            {
                la.Location = new Point(la.Location.X, la.Location.Y - X);
            }

            chatLog.Add(a);
            chatLog.Add(b);
        }

        private void Panel8_MouseEnter(object sender, EventArgs e)
        {//채팅방 스크롤
            panel8.Focus();
        }

        private void Panel8_MouseDown(object sender, MouseEventArgs e)
        {//채팅방 스크롤
            panel8.Focus();
        }

        private void Panel8_ControlAdded(object sender, ControlEventArgs e)
        {
            panel8.ScrollControlIntoView(e.Control);
        }
        //----------채널------------
        Label pop;
        private void channel_MouseHover(object sender, EventArgs e)
        {
            Button thisButton = (Button)sender;
            int n = 0;
            for (int i = 0; i < channelBtns.Count; i++)
            {
                if (channelBtns[i] == thisButton)
                    n = i;
            }

            pop = new Label();
            pop.AutoSize = true;
            pop.BackColor = Color.FromArgb(32, 34, 37);
            pop.BorderStyle = BorderStyle.None;
            pop.Font = new Font("맑은 고딕", 12F);
            pop.ForeColor = Color.White;
            pop.ImageAlign = ContentAlignment.MiddleLeft;
            pop.Padding = new Padding(10, 5, 10, 5);
            pop.Location = new Point(channelBtns[n].Location.X + panel4.Location.X + 60, channelBtns[n].Location.Y + panel4.Location.Y + 30);
            pop.Text = channelNames[n];
            pop.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(pop);
            pop.BringToFront();
        }

        private void channel_MouseLeave(object sender, EventArgs e)
        {
            this.Controls.Remove(pop);
        }

        public void channel_Create(ref RoundButton chanBtn)
        {
            chanBtn.Width = 55;
            chanBtn.Height = 55;
            chanBtn.Location = new Point(10, 60 * channelBtns.Count());
            chanBtn.Click += new EventHandler(chan_Click);
            chanBtn.MouseHover += new EventHandler(channel_MouseHover);
            chanBtn.MouseLeave += new EventHandler(channel_MouseLeave);
            channelBtns.Add(chanBtn);
            panel4.Controls.Add(chanBtn);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (!m_bConnect)
            {
                MsgBoxHelper.Warn("서버가 연결되지 않았습니다.");
                txtSend.Text = "";
                return;
            }

            channelName f = new channelName();
            if (f.ShowDialog() == DialogResult.Cancel)
                channelNames.Add(f.Str);
            //채널버튼생성
            RoundButton chanBtn = new RoundButton();

            chanBtn.BackColor = Color.FromArgb(41, 43, 47);
            chanBtn.FlatAppearance.BorderSize = 0;
            chanBtn.FlatStyle = FlatStyle.Flat;

            switch (cse)
            {
                case 1:
                    chanBtn.Image = Properties.Resources.i1;
                    cse++;
                    break;
                case 2:
                    chanBtn.Image = Properties.Resources.i2;
                    cse++;
                    break;
                case 3:
                    chanBtn.Image = Properties.Resources.i3;
                    cse++;
                    break;
                case 4:
                    chanBtn.Image = Properties.Resources.i4;
                    cse++;
                    break;
                case 5:
                    chanBtn.Image = Properties.Resources.i5;
                    cse++;
                    break;
                case 6:
                    chanBtn.Image = Properties.Resources.i6;
                    cse++;
                    break;
                case 7:
                    chanBtn.Image = Properties.Resources.i1;
                    cse = 2;
                    break;
            }


            channel_Create(ref chanBtn);



            currDay = 0;
            currChan = channelBtns.Count() - 1;
            //날짜버튼생성
            days.Add(new List<Button>());
            Button dayBtn = new Button();
            day_Create(ref dayBtn, currChan);
            days[currChan].Add(dayBtn);
            currDay = days[currChan].Count - 1;

            dayBtn.Image = Properties.Resources.folder;
            dayBtn.BackColor = Color.White;
            dayBtn.ForeColor = Color.Black;
            dayBtn.Font = new Font("맑은 고딕", 7F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(129)));

            panel5.Controls.Add(dayBtn);
            //위젯공간생성
            widgets.Add(new List<List<Button>>());
            widgets[currChan].Add(new List<Button>());
            //메인화면전환
            day_Toggle(chanBtn);
            Button temp = new Button();
            temp.Text = "0001";
            dayWidget_Toggle(temp);
            //서버에 전송
            TravelData data = new TravelData();
            data.Type = (int)PacketType.채널;
            data.text = f.Str;
            TravelPacket.Serialize(data).CopyTo(sendBuffer, 0);
            Send();
        }

        private void chan_Click(object sender, EventArgs e)
        {
            Button thisButton = (Button)sender;
            day_Toggle(thisButton);
            Button temp = new Button();
            temp.Text = "0001";
            dayWidget_Toggle(temp);

            panel10.Visible = true;
            panel10.Height = thisButton.Height;
            panel10.Top = thisButton.Top;
        }
        //----------채널------------
        //----------날짜------------
        public void day_Create(ref Button dayBtn, int index)
        {
            dayBtn.Text = "Day" + (days[index].Count + 1).ToString();
            dayBtn.Size = new Size(50, 40);
            dayBtn.Location = new Point(days[index].Count * 55, 0);
            dayBtn.Click += new EventHandler(day_Click);

        }

        private void roundButton1_Click(object sender, EventArgs e)
        {
            if (!m_bConnect)
                return;
            //날짜버튼 생성
            Button dayBtn = new Button();
            day_Create(ref dayBtn, currChan);
            days[currChan].Add(dayBtn);
            currDay = days[currChan].Count - 1;

            dayBtn.Image = Properties.Resources.folder;
            dayBtn.BackColor = Color.White;
            dayBtn.ForeColor = Color.Black;
            dayBtn.Font = new Font("맑은 고딕", 7F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(129)));

            panel5.Controls.Add(dayBtn);
            //위젯공간 및 화면전환
            widgets[currChan].Add(new List<Button>());
            dayWidget_Toggle(dayBtn);
            //서버에 전송
            TravelData data = new TravelData();
            data.Type = (int)PacketType.날짜;
            data.text = channelNames[currChan];
            data.nickname = dayBtn.Text;
            TravelPacket.Serialize(data).CopyTo(sendBuffer, 0);
            Send();
        }

        public void day_Toggle(Button thisButton)
        {
            for (int j = 0; j < days.Count; j++)
            {
                for (int i = 0; i < days[j].Count; i++)
                {
                    days[j][i].Visible = false;
                }
            }

            for (int i = 0; i < channelBtns.Count; i++)
            {
                if (channelBtns[i] == thisButton)
                    currChan = i;
            }

            for (int i = 0; i < days[currChan].Count; i++)
                days[currChan][i].Visible = true;

        }

        private void day_Click(object sender, EventArgs e)
        {
            Button thisButton = (Button)sender;
            dayWidget_Toggle(thisButton);
        }
        //----------날짜------------

        //----------위젯------------
        public void ButtonCreat(String text)
        {

            panelDraw.Controls.Add(newPanelButton[widgetNumber]);

            /* 위젯 초기 위치 설정 */
            if (widgetNumber == 0)
                newPanelButton[0].Location = new Point(40, 340);
            else
                newPanelButton[widgetNumber].Location = new Point(newPanelButton[widgetNumber - 1].Location.X + 100, newPanelButton[widgetNumber - 1].Location.Y);
            newPanelButton[widgetNumber].Size = new Size(160, 60);
            newPanelButton[widgetNumber].Text = text;
            if (text == "숙소")
            {
                myaccommodation[accommodationWidgetNumber].setWidgetNumber(widgetNumber);
                myaccommodation[accommodationWidgetNumber].setindexNo(accommodationWidgetNumber);
                accommodationWidgetNumber++;
                newPanelButton[widgetNumber].BackColor = Color.FromArgb(42, 182, 199);
                newPanelButton[widgetNumber].Image = Properties.Resources.home;
            }
            else if (text == "교통")
            {
                myvehicle[vehicleWidgetNumber].setWidgetNumber(widgetNumber);
                myvehicle[vehicleWidgetNumber].setindexNo(vehicleWidgetNumber);
                vehicleWidgetNumber++;
                newPanelButton[widgetNumber].BackColor = Color.FromArgb(243, 153, 41);
                newPanelButton[widgetNumber].Image = Properties.Resources.train;
            }
            else if (text == "식사")
            {
                myrestaurant[restaurantWidgetNumber].setWidgetNumber(widgetNumber);
                myrestaurant[restaurantWidgetNumber].setindexNo(restaurantWidgetNumber);
                restaurantWidgetNumber++;
                newPanelButton[widgetNumber].BackColor = Color.FromArgb(227, 51, 72);
                newPanelButton[widgetNumber].Image = Properties.Resources.flavor;
            }
            else if (text == "관광지")
            {
                mysite[siteWidgetNumber].setWidgetNumber(widgetNumber);
                mysite[siteWidgetNumber].setindexNo(siteWidgetNumber);
                siteWidgetNumber++;
                newPanelButton[widgetNumber].BackColor = Color.FromArgb(178, 204, 55);
                newPanelButton[widgetNumber].Image = Properties.Resources.camera;
            }
            //공통 속성
            newPanelButton[widgetNumber].ImageAlign = ContentAlignment.TopCenter;
            newPanelButton[widgetNumber].FlatStyle = FlatStyle.Popup;
            newPanelButton[widgetNumber].TextAlign = ContentAlignment.BottomCenter;
            newPanelButton[widgetNumber].ForeColor = Color.White;
            newPanelButton[widgetNumber].Font = new Font("맑은 고딕", 10.2F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(129)));
            //
            newPanelButton[widgetNumber].AllowDrop = true;
            newPanelButton[widgetNumber].ContextMenuStrip = contextMenuStrip1;

            newPanelButton[widgetNumber].MouseMove += new MouseEventHandler(btn_MouseMove);
            newPanelButton[widgetNumber].MouseDown += new MouseEventHandler(btn_MouseDown);
            newPanelButton[widgetNumber].MouseUp += new MouseEventHandler(btn_MouseUp);
            newPanelButton[widgetNumber].Click += new EventHandler(btnNewWidget_Click);
            newPanelButton[widgetNumber].DoubleClick += new EventHandler(btnNewWidget_DoubleClick);

            widgetNumber++;
        }

        private void btnWidget_Click(object sender, EventArgs e)
        {
            if (currChan == -1)
                return;
            if (!m_bConnect)
                return;
            Button thisButton = (Button)sender;
            ButtonCreat(thisButton.Text);
            widgets[currChan][currDay].Add(newPanelButton[widgetNumber-1]);

            TravelData newButton = new TravelData();
            TravelData data = new TravelData();
            newButton.Type = (int)PacketType.위젯;
            newButton.text = channelNames[currChan];
            newButton.nickname = days[currChan][currDay].Text;
            newButton.tts = thisButton.Text;
            newButton.index = widgets[currChan][currDay].Count - 1;
            newButton.widgetIndex = currChan * 3 + currDay * 5 + widgets[currChan][currDay].Count-1;
            newButton.locationX = newPanelButton[widgetNumber - 1].Location.X;
            newButton.locationY = newPanelButton[widgetNumber - 1].Location.Y;

            for (int widgetIndex = 0; widgetIndex < widgetNumber; widgetIndex++)    // 위젯 번호 검색
            {
                if (newPanelButton[widgetNumber - 1] == newPanelButton[widgetIndex])
                {
                    if (newPanelButton[widgetNumber - 1].BackColor == Color.FromArgb(42, 182, 199))
                    {
                        for (int accIndex = 0; accIndex < accommodationWidgetNumber; accIndex++)    // 숙소위젯 인덱스 검색
                        {
                            if (myaccommodation[accIndex].getWidgetNumber() == widgetIndex)    // 클릭한 숙소위젯의 위젯No가 widgetIndex와 일치하면
                            {
                                newButton.btnType = 1;
                                newButton.a = myaccommodation[accIndex];
                                break;
                            }
                        }
                    }
                    else if (newPanelButton[widgetNumber - 1].BackColor == Color.FromArgb(243, 153, 41))
                    {
                        for (int vehicleIndex = 0; vehicleIndex < vehicleWidgetNumber; vehicleIndex++)    // 교통위젯 인덱스 검색
                        {
                            if (myvehicle[vehicleIndex].getWidgetNumber() == widgetIndex)    // 클릭한 교통위젯의 위젯No가 widgetIndex와 일치하면
                            {
                                newButton.btnType = 2;
                                newButton.v = myvehicle[vehicleIndex];
                                break;
                            }
                        }
                    }
                    else if (newPanelButton[widgetNumber - 1].BackColor == Color.FromArgb(227, 51, 72))
                    {
                        for (int restaurantIndex = 0; restaurantIndex < restaurantWidgetNumber; restaurantIndex++)    // 식사위젯 인덱스 검색
                        {
                            if (myrestaurant[restaurantIndex].getWidgetNumber() == widgetIndex)    // 클릭한 식사위젯의 위젯No가 widgetIndex와 일치하면
                            {
                                newButton.btnType = 3;
                                newButton.r = myrestaurant[restaurantIndex];
                                break;
                            }
                        }
                    }
                    else if (newPanelButton[widgetNumber - 1].BackColor == Color.FromArgb(178, 204, 55))
                    {
                        for (int siteIndex = 0; siteIndex < siteWidgetNumber; siteIndex++)    // 관광지위젯 인덱스 검색
                        {
                            if (mysite[siteIndex].getWidgetNumber() == widgetIndex)    // 클릭한 관광지위젯의 위젯No가 widgetIndex와 일치하면
                            {
                                newButton.btnType = 4;
                                newButton.s = mysite[siteIndex];
                                break;
                            }
                        }
                    }
                }
            }
            
            TravelPacket.Serialize(newButton).CopyTo(sendBuffer, 0);
            Send();

        }

        private void btn_MouseMove(object sender, MouseEventArgs e)
        {
            Button thisButton = (Button)sender;
            if (buttonMoveFlag)
            {
                thisButton.Location = new Point(thisButton.Location.X + e.X - gapX, thisButton.Location.Y + e.Y - gapY);
                panelDraw.Update();
            }
        }

        int gapX, gapY;
        private void btn_MouseDown(object sender, MouseEventArgs e)
        {
            if (!buttonMoveFlag)
            {
                buttonMoveFlag = true;
                gapX = e.Location.X;
                gapY = e.Location.Y;
            }
        }

        private void btn_MouseUp(object sender, MouseEventArgs e)
        {
            Button thisButton = (Button)sender;
            for (int i = 0; i < widgets[currChan][currDay].Count; i++)
            {
                if (widgets[currChan][currDay][i] == thisButton)
                {
                    TravelData temp = new TravelData();
                    temp.Type = (int)PacketType.이동;
                    temp.locationX = thisButton.Location.X;
                    temp.locationY = thisButton.Location.Y;
                    temp.chan = currChan;
                    temp.day = currDay;
                    temp.index = i;
                    temp.widgetIndex = currChan * 3 + currDay * 5 + i;
                    TravelPacket.Serialize(temp).CopyTo(sendBuffer, 0);
                    Send();
                    
                }
            }
            if (buttonMoveFlag)
            {
                buttonMoveFlag = false;
                thisButton.Location = new Point(thisButton.Location.X + e.X - gapX, thisButton.Location.Y + e.Y - gapY);
                

            }
        }

        public void accForm_SendContext(MyAccommodation widget, Button button)
        {
            int accNumber = 0;
            for (int widgetIndex = 0; widgetIndex < widgetNumber; widgetIndex++)
            {
                if (myaccommodation[accNumber].getWidgetNumber() == widgetIndex)
                {
                    if (myaccommodation[accNumber].getIndexNo() != widgetIndex)
                    {
                        myaccommodation[accNumber].address = widget.address;
                        myaccommodation[accNumber].checkInTime = widget.checkInTime;
                        myaccommodation[accNumber].checkOutTime = widget.checkOutTime;
                        myaccommodation[accNumber].fee = widget.fee;
                        myaccommodation[accNumber].name = widget.name;
                        myaccommodation[accNumber].other = widget.other;
                        myaccommodation[accNumber].phoneNumber = widget.phoneNumber;
                        for (int i = 0; i < widgets[currChan][currDay].Count; i++)
                        {
                            if (widgets[currChan][currDay][i] == button)
                            {
                                TravelData temp = new TravelData();
                                temp.btnType = 1;
                                temp.a = widget;
                                temp.Type = (int)PacketType.정보;
                                temp.chan = currChan;
                                temp.day = currDay;
                                temp.index = i;
                                temp.color = accNumber;
                                temp.widgetIndex = currChan * 3 + currDay * 5 + i;
                                TravelPacket.Serialize(temp).CopyTo(sendBuffer, 0);
                                Send();
                            }
                        }
                        
                    }
                }
                else
                    accNumber++;
            }
        }

        public void vehicleForm_SendContext(MyVehicle widget, Button button)
        {
            int vehicleNumber = 0;
            for (int widgetIndex = 0; widgetIndex < widgetNumber; widgetIndex++)
            {
                if (myvehicle[vehicleNumber].getWidgetNumber() == widgetIndex)
                {
                    if (myvehicle[vehicleNumber].getIndexNo() != widgetIndex)
                    {
                        myvehicle[vehicleNumber].name = widget.name;
                        myvehicle[vehicleNumber].departTime = widget.departTime;
                        myvehicle[vehicleNumber].arriveTime = widget.arriveTime;
                        myvehicle[vehicleNumber].takeTime = widget.takeTime;
                        myvehicle[vehicleNumber].fee = widget.fee;
                        myvehicle[vehicleNumber].other = widget.other;
                        for (int i = 0; i < widgets[currChan][currDay].Count; i++)
                        {
                            if (widgets[currChan][currDay][i] == button)
                            {
                                TravelData temp = new TravelData();
                                temp.btnType = 2;
                                temp.v = widget;
                                temp.Type = (int)PacketType.정보;
                                temp.chan = currChan;
                                temp.day = currDay;
                                temp.index = i;
                                temp.color = vehicleNumber;
                                temp.widgetIndex = currChan * 3 + currDay * 5 + i;
                                TravelPacket.Serialize(temp).CopyTo(sendBuffer, 0);
                                Send();
                            }
                        }
                    }
                }
                else
                    vehicleNumber++;
            }
        }

        public void restaurantForm_SendContext(MyRestaurant widget, Button button)
        {
            int restaurantNumber = 0;

            for (int widgetIndex = 0; widgetIndex < widgetNumber; widgetIndex++)
            {
                if (myrestaurant[restaurantNumber].getWidgetNumber() == widgetIndex)
                {
                    if (myrestaurant[restaurantNumber].getIndexNo() != widgetIndex)
                    {
                        myrestaurant[restaurantNumber].name = widget.name;
                        myrestaurant[restaurantNumber].openingTime = widget.openingTime;
                        myrestaurant[restaurantNumber].fee = widget.fee;
                        myrestaurant[restaurantNumber].address = widget.address;
                        myrestaurant[restaurantNumber].phoneNumber = widget.phoneNumber;
                        myrestaurant[restaurantNumber].other = widget.other;
                        MessageBox.Show("가냐");
                        TravelData temp = new TravelData();
                        temp.btnType = 3;
                        temp.r = widget;
                        temp.Type = (int)PacketType.정보;
                        temp.chan = currChan;
                        temp.day = currDay;
                        temp.index = i;
                        temp.color = restaurantNumber;
                        temp.widgetIndex = currChan * 3 + currDay * 5 + i;
                        TravelPacket.Serialize(temp).CopyTo(sendBuffer, 0);
                        Send();
                    }
                }
                else
                    restaurantNumber++;
            }
        }

        public void siteForm_SendContext(MySite widget, Button button)
        {
            int siteNumber = 0;

            for (int widgetIndex = 0; widgetIndex < widgetNumber; widgetIndex++)
            {
                if (mysite[siteNumber].getWidgetNumber() == widgetIndex)
                {
                    if (mysite[siteNumber].getIndexNo() != widgetIndex)
                    {
                        mysite[siteNumber].name = widget.name;
                        mysite[siteNumber].openingTime = widget.openingTime;
                        mysite[siteNumber].fee = widget.fee;
                        mysite[siteNumber].address = widget.address;
                        mysite[siteNumber].phoneNumber = widget.phoneNumber;
                        mysite[siteNumber].other = widget.other;
                        for (int i = 0; i < widgets[currChan][currDay].Count; i++)
                        {
                            if (widgets[currChan][currDay][i] == button)
                            {
                                TravelData temp = new TravelData();
                                temp.btnType = 4;
                                temp.s = widget;
                                temp.Type = (int)PacketType.정보;
                                temp.chan = currChan;
                                temp.day = currDay;
                                temp.index = i;
                                temp.color = siteNumber;
                                temp.widgetIndex = currChan * 3 + currDay * 5 + i;
                                TravelPacket.Serialize(temp).CopyTo(sendBuffer, 0);
                                Send();
                            }
                        }
                    }
                }
                else
                    siteNumber++;
            }
        }

        private void btnNewWidget_DoubleClick(object sender, EventArgs e)
        {
            Button thisButton = (Button)sender;

            for (int widgetIndex = 0; widgetIndex < widgetNumber; widgetIndex++)    // 위젯 번호 검색
            {
                if (thisButton == newPanelButton[widgetIndex])
                {
                    if (thisButton.BackColor == Color.FromArgb(42, 182, 199)) 
                    {
                        for (int accIndex = 0; accIndex < accommodationWidgetNumber; accIndex++)    // 숙소위젯 인덱스 검색
                        {
                            if (myaccommodation[accIndex].getWidgetNumber() == widgetIndex)    // 클릭한 숙소위젯의 위젯No가 widgetIndex와 일치하면
                            {
                                Form2 accForm = new Form2(myaccommodation[accIndex], thisButton);
                                accForm.Owner = this;
                                accForm.SendContext += new Form2.SendContextDele(accForm_SendContext);
                                accForm.Show();
                                return;
                            }
                        }
                    }
                    else if (thisButton.BackColor == Color.FromArgb(243, 153, 41))
                    {
                        for (int vehicleIndex = 0; vehicleIndex < vehicleWidgetNumber; vehicleIndex++)    // 교통위젯 인덱스 검색
                        {
                            if (myvehicle[vehicleIndex].getWidgetNumber() == widgetIndex)    // 클릭한 교통위젯의 위젯No가 widgetIndex와 일치하면
                            {
                                Form3 vehicleForm = new Form3(myvehicle[vehicleIndex], thisButton);
                                vehicleForm.Owner = this;
                                vehicleForm.SendContext += new Form3.SendContextDele(vehicleForm_SendContext);
                                vehicleForm.Show();
                                return;
                            }
                        }
                    }
                    else if (thisButton.BackColor == Color.FromArgb(227, 51, 72))
                    {
                        for (int restaurantIndex = 0; restaurantIndex < restaurantWidgetNumber; restaurantIndex++)    // 식사위젯 인덱스 검색
                        {
                            if (myrestaurant[restaurantIndex].getWidgetNumber() == widgetIndex)    // 클릭한 식사위젯의 위젯No가 widgetIndex와 일치하면
                            {
                                Form4 restaurantForm = new Form4(myrestaurant[restaurantIndex], thisButton);
                                restaurantForm.Owner = this;
                                restaurantForm.SendContext += new Form4.SendContextDele(restaurantForm_SendContext);
                                restaurantForm.Show();
                                return;
                            }
                        }
                    }
                    else if (thisButton.BackColor == Color.FromArgb(178, 204, 55))
                    {
                        for (int siteIndex = 0; siteIndex < siteWidgetNumber; siteIndex++)    // 관광지위젯 인덱스 검색
                        {
                            if (mysite[siteIndex].getWidgetNumber() == widgetIndex)    // 클릭한 관광지위젯의 위젯No가 widgetIndex와 일치하면
                            {
                                Form5 siteForm = new Form5(mysite[siteIndex], thisButton);
                                siteForm.Owner = this;
                                siteForm.SendContext += new Form5.SendContextDele(siteForm_SendContext);
                                siteForm.Show();
                                return;
                            }
                        }
                    }
                }
            }
        }
        private void btnNewWidget_Click(object sender, EventArgs e)
        {
            Button thisButton = (Button)sender;
            selectedButton = thisButton;
        }


        public void dayWidget_Toggle(Button thisButton)
        {
            for (int k = 0; k < widgets.Count; k++)
            {
                for (int j = 0; j < widgets[k].Count; j++)
                {
                    for (int i = 0; i < widgets[k][j].Count; i++)
                    {
                        widgets[k][j][i].Visible = false;
                    }
                }
            }
            currDay = int.Parse(thisButton.Text[3].ToString()) - 1;
            for (int i = 0; i < widgets[currChan][currDay].Count; i++)
            {
                widgets[currChan][currDay][i].Visible = true;
            }
        }
        //----------위젯------------






        //----------프로그램 기능------------
        Button selectedButton;
        private void 삭제ToolStripMenuItem_Click(object sender, EventArgs e)
        {//위젯 삭제
            if (panelDraw.Controls.Contains(selectedButton))
            {
                selectedButton.MouseMove -= new MouseEventHandler(btn_MouseMove);
                selectedButton.MouseDown -= new MouseEventHandler(btn_MouseDown);
                selectedButton.MouseUp -= new MouseEventHandler(btn_MouseUp);
                newPanelButton[widgetNumber].DoubleClick -= new EventHandler(btnNewWidget_DoubleClick);
                
                panelDraw.Controls.Remove(selectedButton);
                selectedButton.Dispose();
            }
        }
        
        private void Button5_Click(object sender, EventArgs e)
        {//닫기 버튼
            this.Close();
        }
        private void button6_Click(object sender, EventArgs e)
        {//최대화 버튼
            if (isMaximized == false)
            {
                this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;  // 폼을 최대화시켰을 때 작업표시줄을 안 가리게 만든다.
                this.WindowState = FormWindowState.Maximized;
                isMaximized = true;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                isMaximized = false;
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {//최소화 버튼
            this.WindowState = FormWindowState.Minimized;
        }

        private void panHeader_MouseDown(object sender, MouseEventArgs e)
        {//폼이동
            var s = sender as Panel;
            s.Tag = new Point(e.X, e.Y);
        }

        private void panHeader_MouseMove(object sender, MouseEventArgs e)
        {//폼이동
            var s = sender as Panel;
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            s.Parent.Left = this.Left + (e.X - ((Point)s.Tag).X);
            s.Parent.Top = this.Top + (e.Y - ((Point)s.Tag).Y);
        }
        
        void 종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {//완전종료
            notifyIcon1.Visible = false;
            Application.Exit();
            Close();
        }
        void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {//시스템 트레이
            this.Visible = true;
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {//폼 종료
            e.Cancel = true;
            this.Visible = false;
            Disconnect();
        }
        //----------프로그램 기능------------

    }





    public class DoubleClickButton : Button
    {
        public DoubleClickButton()
        {
            SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
        }
    }
}
