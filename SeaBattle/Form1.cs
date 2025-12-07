using System;
using System.Drawing;
using System.Windows.Forms;

namespace SeaBattle
{
    public partial class Form1 : Form
    {
        private const int GRID_SIZE = 10;
        private const int CELL_SIZE = 35;

        private Button[,] playerButtons;
        private Button[,] enemyButtons;

        private TextBox txtIP;
        private TextBox txtPort;
        private Button btnHost;
        private Button btnJoin;
        private Label lblStatus;

        public Form1()
        {
            InitializeComponent();

            this.Text = "Sea Battle";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            CreateInterface();
        }

        private void CreateInterface()
        {
            playerButtons = new Button[GRID_SIZE, GRID_SIZE];
            enemyButtons = new Button[GRID_SIZE, GRID_SIZE];

            Panel top = new Panel();
            top.Height = 60;
            top.Dock = DockStyle.Top;

            Label lblIp = new Label();
            lblIp.Text = "IP:";
            lblIp.Location = new Point(10, 20);
            lblIp.AutoSize = true;

            txtIP = new TextBox();
            txtIP.Location = new Point(40, 16);
            txtIP.Width = 140;

            Label lblPort = new Label();
            lblPort.Text = "Port:";
            lblPort.Location = new Point(190, 20);
            lblPort.AutoSize = true;

            txtPort = new TextBox();
            txtPort.Location = new Point(235, 16);
            txtPort.Width = 80;

            btnHost = new Button();
            btnHost.Text = "Создать игру";
            btnHost.Location = new Point(330, 12);

            btnJoin = new Button();
            btnJoin.Text = "Подключиться";
            btnJoin.Location = new Point(480, 12);
            btnJoin.Width = 100;

            lblStatus = new Label();
            lblStatus.Text = "Статус: ожидание";
            lblStatus.Location = new Point(650, 20);
            lblStatus.AutoSize = true;

            btnHost.Click += BtnHost_Click;
            btnJoin.Click += BtnJoin_Click;

            top.Controls.Add(lblIp);
            top.Controls.Add(txtIP);
            top.Controls.Add(lblPort);
            top.Controls.Add(txtPort);
            top.Controls.Add(btnHost);
            top.Controls.Add(btnJoin);
            top.Controls.Add(lblStatus);

            this.Controls.Add(top);

            Panel fieldArea = new Panel();
            fieldArea.Dock = DockStyle.Fill;

            Panel playerPanel = CreateGrid(playerButtons);
            Panel enemyPanel = CreateGrid(enemyButtons);

            playerPanel.Location = new Point(150, 100);
            enemyPanel.Location = new Point(550, 100);

            fieldArea.Controls.Add(playerPanel);
            fieldArea.Controls.Add(enemyPanel);

            this.Controls.Add(fieldArea);
        }

        private Panel CreateGrid(Button[,] buttons)
        {
            Panel panel = new Panel();
            panel.Width = GRID_SIZE * CELL_SIZE;
            panel.Height = GRID_SIZE * CELL_SIZE;

            for (int i = 0; i < GRID_SIZE; i++)
            {
                for (int j = 0; j < GRID_SIZE; j++)
                {
                    Button btn = new Button();
                    btn.Width = CELL_SIZE;
                    btn.Height = CELL_SIZE;
                    btn.Left = j * CELL_SIZE;
                    btn.Top = i * CELL_SIZE;
                    btn.BackColor = Color.LightBlue;
                    btn.Tag = new Point(i, j);
                    btn.Click += GridButton_Click;

                    buttons[i, j] = btn;
                    panel.Controls.Add(btn);
                }
            }

            return panel;
        }

        private void GridButton_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            if (btn.BackColor == Color.LightBlue)
                btn.BackColor = Color.Gray;
            else
                btn.BackColor = Color.LightBlue;
        }

        private void BtnHost_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Статус: host";
        }

        private void BtnJoin_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Статус: join";
        }
    }
}
