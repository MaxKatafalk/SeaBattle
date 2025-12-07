using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;

namespace SeaBattle
{
    public partial class Form1 : Form
    {
        private const int GRID_SIZE = 10;
        private const int CELL_SIZE = 30;

        private Button[,] playerButtons;
        private Button[,] enemyButtons;

        private TextBox txtIP;
        private TextBox txtPort;
        private Button btnHost;
        private Button btnJoin;
        private Label lblStatus;

        private TcpListener listener;
        private TcpClient client;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread readThread;

        private bool connected = false;
        private bool myTurn = false;
        private bool isHost = false;

        public Form1()
        {
            InitializeComponent();
            CreateUI();
        }

        private void CreateUI()
        {
            this.Text = "Sea Battle";
            this.Size = new Size(1000, 650);

            Label lblIp = new Label();
            lblIp.Text = "IP:";
            lblIp.Location = new Point(10, 10);
            lblIp.AutoSize = true;

            txtIP = new TextBox();
            txtIP.Location = new Point(40, 8);
            txtIP.Width = 120;
            txtIP.Text = "127.0.0.1";

            Label lblPort = new Label();
            lblPort.Text = "Port:";
            lblPort.Location = new Point(180, 10);
            lblPort.AutoSize = true;

            txtPort = new TextBox();
            txtPort.Location = new Point(225, 8);
            txtPort.Width = 80;
            txtPort.Text = "12345";

            btnHost = new Button();
            btnHost.Text = "Host";
            btnHost.Location = new Point(320, 6);
            btnHost.Click += BtnHost_Click;

            btnJoin = new Button();
            btnJoin.Text = "Join";
            btnJoin.Location = new Point(400, 6);
            btnJoin.Click += BtnJoin_Click;

            lblStatus = new Label();
            lblStatus.Text = "Статус: ожидание";
            lblStatus.Location = new Point(500, 10);
            lblStatus.AutoSize = true;

            this.Controls.Add(lblIp);
            this.Controls.Add(txtIP);
            this.Controls.Add(lblPort);
            this.Controls.Add(txtPort);
            this.Controls.Add(btnHost);
            this.Controls.Add(btnJoin);
            this.Controls.Add(lblStatus);

            CreateBoards();
        }

        private void CreateBoards()
        {
            playerButtons = new Button[GRID_SIZE, GRID_SIZE];
            enemyButtons = new Button[GRID_SIZE, GRID_SIZE];

            Panel myPanel = new Panel();
            myPanel.Location = new Point(100, 80);
            myPanel.Size = new Size(GRID_SIZE * CELL_SIZE, GRID_SIZE * CELL_SIZE);
            this.Controls.Add(myPanel);

            Panel enemyPanel = new Panel();
            enemyPanel.Location = new Point(550, 80);
            enemyPanel.Size = new Size(GRID_SIZE * CELL_SIZE, GRID_SIZE * CELL_SIZE);
            this.Controls.Add(enemyPanel);

            for (int i = 0; i < GRID_SIZE; i++)
            {
                for (int j = 0; j < GRID_SIZE; j++)
                {
                    Button b1 = new Button();
                    b1.Size = new Size(CELL_SIZE, CELL_SIZE);
                    b1.Location = new Point(j * CELL_SIZE, i * CELL_SIZE);
                    b1.BackColor = Color.LightBlue;
                    myPanel.Controls.Add(b1);
                    playerButtons[i, j] = b1;

                    Button b2 = new Button();
                    b2.Size = new Size(CELL_SIZE, CELL_SIZE);
                    b2.Location = new Point(j * CELL_SIZE, i * CELL_SIZE);
                    b2.BackColor = Color.LightBlue;
                    b2.Tag = i + "," + j;
                    b2.Click += EnemyButton_Click;
                    enemyPanel.Controls.Add(b2);
                    enemyButtons[i, j] = b2;
                }
            }
        }

        private void BtnHost_Click(object sender, EventArgs e)
        {
            if (connected) return;

            int port = int.Parse(txtPort.Text);
            isHost = true;

            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Thread t = new Thread(() =>
            {
                client = listener.AcceptTcpClient();
                SetupConnection();
                connected = true;
                myTurn = true;

                this.Invoke(new Action(() =>
                {
                    lblStatus.Text = "Ваш ход";
                }));
            });
            t.IsBackground = true;
            t.Start();

            lblStatus.Text = "Ожидание подключения...";
        }

        private void BtnJoin_Click(object sender, EventArgs e)
        {
            if (connected) return;

            string ip = txtIP.Text;
            int port = int.Parse(txtPort.Text);

            isHost = false;
            client = new TcpClient();
            client.Connect(ip, port);
            SetupConnection();

            connected = true;
            myTurn = false;

            lblStatus.Text = "Ход соперника";
        }

        private void SetupConnection()
        {
            stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8);
            writer.AutoFlush = true;

            readThread = new Thread(ReadLoop);
            readThread.IsBackground = true;
            readThread.Start();
        }

        private void EnemyButton_Click(object sender, EventArgs e)
        {
            if (!connected) return;
            if (!myTurn) return;

            Button btn = (Button)sender;
            string[] p = btn.Tag.ToString().Split(',');
            int x = int.Parse(p[0]);
            int y = int.Parse(p[1]);

            writer.WriteLine("SHOT:" + x + ":" + y);

            btn.BackColor = Color.Orange;
            myTurn = false;
            lblStatus.Text = "Ход соперника";
        }

        private void ReadLoop()
        {
            while (true)
            {
                string msg = reader.ReadLine();
                if (msg == null) break;

                if (msg.StartsWith("SHOT:"))
                {
                    string[] p = msg.Split(':');
                    int x = int.Parse(p[1]);
                    int y = int.Parse(p[2]);

                    this.Invoke(new Action(() =>
                    {
                        playerButtons[x, y].BackColor = Color.Red;
                        myTurn = true;
                        lblStatus.Text = "Ваш ход";
                    }));
                }
            }
        }
    }
}
