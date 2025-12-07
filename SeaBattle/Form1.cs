using System;
using System.Collections.Generic;
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

        private int[] shipSizes = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };

        private List<Ship> playerShips = new List<Ship>();
        private List<Ship> enemyShips = new List<Ship>();

        private Button[,] playerButtons;
        private Button[,] enemyButtons;

        private Button btnAuto;
        private Button btnRotate;
        private Button btnReady;

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

        private bool shipsPlaced = false;
        private bool opponentShipsPlaced = false;

        private Ship selectedShip = null;
        private bool horizontalPlacement = true;

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
            this.Controls.Add(lblIp);

            txtIP = new TextBox();
            txtIP.Location = new Point(40, 8);
            txtIP.Width = 120;
            txtIP.Text = "127.0.0.1";
            this.Controls.Add(txtIP);

            Label lblPort = new Label();
            lblPort.Text = "Port:";
            lblPort.Location = new Point(180, 10);
            lblPort.AutoSize = true;
            this.Controls.Add(lblPort);

            txtPort = new TextBox();
            txtPort.Location = new Point(225, 8);
            txtPort.Width = 80;
            txtPort.Text = "12345";
            this.Controls.Add(txtPort);

            btnHost = new Button();
            btnHost.Text = "Host";
            btnHost.Location = new Point(320, 6);
            btnHost.Click += BtnHost_Click;
            this.Controls.Add(btnHost);

            btnJoin = new Button();
            btnJoin.Text = "Join";
            btnJoin.Location = new Point(400, 6);
            btnJoin.Click += BtnJoin_Click;
            this.Controls.Add(btnJoin);

            btnAuto = new Button();
            btnAuto.Text = "Авто";
            btnAuto.Location = new Point(480, 40);
            btnAuto.Click += BtnAuto_Click;
            this.Controls.Add(btnAuto);

            btnRotate = new Button();
            btnRotate.Text = "Повернуть";
            btnRotate.Location = new Point(560, 40);
            btnRotate.Click += BtnRotate_Click;
            this.Controls.Add(btnRotate);

            btnReady = new Button();
            btnReady.Text = "Готов";
            btnReady.Location = new Point(640, 40);
            btnReady.Click += BtnReady_Click;
            btnReady.Enabled = false;
            this.Controls.Add(btnReady);

            lblStatus = new Label();
            lblStatus.Text = "Статус: ожидание";
            lblStatus.Location = new Point(500, 10);
            lblStatus.AutoSize = true;
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
                    b1.Tag = new Point(i, j);
                    b1.Click += PlayerButton_Click;
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

        private void BtnAuto_Click(object sender, EventArgs e)
        {
            AutoPlaceShips();
            lblStatus.Text = "Корабли расставлены автоматически. Выберите корабль для перемещения или нажмите 'Готов'.";
            btnReady.Enabled = true;
        }

        private void AutoPlaceShips()
        {
            playerShips.Clear();
            selectedShip = null;
            shipsPlaced = false;
            opponentShipsPlaced = false;

            for (int i = 0; i < GRID_SIZE; i++)
                for (int j = 0; j < GRID_SIZE; j++)
                    playerButtons[i, j].BackColor = Color.LightBlue;

            Random rnd = new Random();
            foreach (int size in shipSizes)
            {
                bool placed = false;
                while (!placed)
                {
                    bool horizontal = rnd.Next(2) == 0;
                    int x = rnd.Next(GRID_SIZE - (horizontal ? 0 : size - 1));
                    int y = rnd.Next(GRID_SIZE - (horizontal ? size - 1 : 0));

                    Point[] cells = new Point[size];
                    for (int k = 0; k < size; k++)
                        cells[k] = horizontal ? new Point(x, y + k) : new Point(x + k, y);

                    if (CanPlaceShip(playerShips, cells))
                    {
                        Ship s = new Ship(cells, horizontal);
                        playerShips.Add(s);
                        foreach (Point p in cells)
                            playerButtons[p.X, p.Y].BackColor = Color.Green;
                        placed = true;
                    }
                }
            }
        }

        private void BtnRotate_Click(object sender, EventArgs e)
        {
            if (selectedShip == null)
            {
                horizontalPlacement = !horizontalPlacement;
                lblStatus.Text = "Ориентация переключена. Выберите корабль.";
                return;
            }

            Point anchor = selectedShip.Cells[0];
            bool newOrientation = !selectedShip.IsHorizontal;

            int size = selectedShip.Cells.Length;
            Point[] newCells = new Point[size];
            for (int i = 0; i < size; i++)
            {
                int nx = newOrientation ? anchor.X : anchor.X + i;
                int ny = newOrientation ? anchor.Y + i : anchor.Y;
                if (nx >= GRID_SIZE || ny >= GRID_SIZE)
                {
                    lblStatus.Text = "Нельзя повернуть здесь!";
                    return;
                }
                newCells[i] = new Point(nx, ny);
            }

            if (!CanPlaceShip(playerShips.FindAll(s => s != selectedShip), newCells))
            {
                lblStatus.Text = "Нельзя повернуть здесь!";
                return;
            }

            ClearShipFromButtons(selectedShip);
            selectedShip.Cells = newCells;
            selectedShip.IsHorizontal = newOrientation;
            PaintShipOnButtons(selectedShip, Color.Green);
            selectedShip = null;
            lblStatus.Text = "Корабль повернут. Выберите следующий корабль или нажмите 'Готов'.";
        }

        private void BtnReady_Click(object sender, EventArgs e)
        {
            if (playerShips.Count < shipSizes.Length)
            {
                MessageBox.Show("Сначала расставьте все корабли!");
                return;
            }

            shipsPlaced = true;
            btnReady.Enabled = false;
            selectedShip = null;
            lblStatus.Text = "Вы готовы. Ждём противника.";

            try
            {
                if (connected && writer != null)
                {
                    writer.WriteLine("PLACED");
                }
            }
            catch { }

            if (opponentShipsPlaced)
            {
                if (isHost)
                {
                    myTurn = true;
                    lblStatus.Text = "Оба готовы. Ваш ход";
                }
                else
                {
                    myTurn = false;
                    lblStatus.Text = "Оба готовы. Ход соперника";
                }
            }
        }


        private void PlayerButton_Click(object sender, EventArgs e)
        {
            if (shipsPlaced)
            {
                lblStatus.Text = "Нельзя менять — вы уже нажали 'Готов'.";
                return;
            }

            Button btn = (Button)sender;
            Point pos = (Point)btn.Tag;

            if (selectedShip == null)
            {
                foreach (Ship s in playerShips)
                {
                    foreach (Point p in s.Cells)
                    {
                        if (p == pos)
                        {
                            selectedShip = s;
                            PaintShipOnButtons(selectedShip, Color.Yellow);
                            lblStatus.Text = "Корабль выбран. Выберите клетку для новой позиции.";
                            return;
                        }
                    }
                }

                lblStatus.Text = "Корабль не выбран. Кликните по зелёной клетке (кораблю).";
            }
            else
            {
                int size = selectedShip.Cells.Length;
                Point[] newCells = new Point[size];
                bool orientation = selectedShip.IsHorizontal;
                for (int i = 0; i < size; i++)
                {
                    int nx = orientation ? pos.X : pos.X + i;
                    int ny = orientation ? pos.Y + i : pos.Y;
                    if (nx >= GRID_SIZE || ny >= GRID_SIZE)
                    {
                        lblStatus.Text = "Нельзя разместить здесь (вне поля).";
                        return;
                    }
                    newCells[i] = new Point(nx, ny);
                }

                if (!CanPlaceShip(playerShips.FindAll(s => s != selectedShip), newCells))
                {
                    lblStatus.Text = "Нельзя разместить здесь (столкновение).";
                    return;
                }

                ClearShipFromButtons(selectedShip);
                selectedShip.Cells = newCells;
                PaintShipOnButtons(selectedShip, Color.Green);
                selectedShip = null;
                lblStatus.Text = "Корабль перемещён. Выберите следующий корабль или нажмите 'Готов'.";
            }
        }

        private void PaintShipOnButtons(Ship s, Color color)
        {
            foreach (Point p in s.Cells)
            {
                playerButtons[p.X, p.Y].BackColor = color;
            }
        }

        private void ClearShipFromButtons(Ship s)
        {
            foreach (Point p in s.Cells)
            {
                playerButtons[p.X, p.Y].BackColor = Color.LightBlue;
            }
        }

        private bool CanPlaceShip(List<Ship> ships, Point[] cells)
        {
            foreach (Ship s in ships)
            {
                foreach (Point c in s.Cells)
                {
                    foreach (Point n in cells)
                    {
                        if (Math.Abs(c.X - n.X) <= 1 && Math.Abs(c.Y - n.Y) <= 1)
                            return false;
                    }
                }
            }
            return true;
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

                this.Invoke(new Action(() =>
                {
                    AutoPlaceShips();
                    btnReady.Enabled = true;
                    lblStatus.Text = "Подключено. Корабли расставлены. Выберите корабль или нажмите 'Готов'.";
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

            AutoPlaceShips();
            btnReady.Enabled = true;
            lblStatus.Text = "Подключено. Корабли расставлены. Выберите корабль или нажмите 'Готов'.";
        }

        private void SetupConnection()
        {
            stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8);
            writer.AutoFlush = true;

            try
            {
                if (shipsPlaced && writer != null)
                {
                    writer.WriteLine("PLACED");
                }
            }
            catch { }

            readThread = new Thread(ReadLoop);
            readThread.IsBackground = true;
            readThread.Start();
        }

        private void EnemyButton_Click(object sender, EventArgs e)
        {
            if (!connected)
            {
                lblStatus.Text = "Нет подключения.";
                return;
            }

            if (!shipsPlaced)
            {
                lblStatus.Text = "Сначала нажмите 'Готов'.";
                return;
            }

            if (!opponentShipsPlaced)
            {
                lblStatus.Text = "Ждём готовности противника.";
                return;
            }

            if (!myTurn)
            {
                lblStatus.Text = "Сейчас не ваш ход.";
                return;
            }

            Button btn = (Button)sender;
            if (!btn.Enabled) return; // запрещаем стрелять по уже заблокированной клетке

            string[] p = btn.Tag.ToString().Split(',');
            int x = int.Parse(p[0]);
            int y = int.Parse(p[1]);

            btn.Enabled = false;
            btn.BackColor = Color.Orange;
            lblStatus.Text = "Выстрел отправлен. Ждём результат...";

            try
            {
                writer.WriteLine($"SHOT:{x}:{y}");
                myTurn = false; // ждём ответа
            }
            catch
            {
                lblStatus.Text = "Ошибка отправки выстрела.";
            }
        }

        private void HighlightSunkShip(Ship ship)
        {
            foreach (Point p in ship.Cells)
            {
                playerButtons[p.X, p.Y].BackColor = Color.DarkRed;
                playerButtons[p.X, p.Y].Enabled = false;
            }

            for (int i = 0; i < ship.Cells.Length; i++)
            {
                Point p = ship.Cells[i];
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = p.X + dx;
                        int ny = p.Y + dy;
                        if (nx >= 0 && ny >= 0 && nx < GRID_SIZE && ny < GRID_SIZE)
                        {
                            var btn = playerButtons[nx, ny];
                            if (btn.BackColor == Color.LightBlue)
                            {
                                btn.BackColor = Color.Gray;
                                btn.Enabled = false;
                            }
                        }
                    }
                }
            }
        }

        private void MarkSunkOnEnemy(Point[] shipCells)
        {
            foreach (Point p in shipCells)
            {
                enemyButtons[p.X, p.Y].BackColor = Color.DarkRed;
                enemyButtons[p.X, p.Y].Enabled = false;
            }

            foreach (Point p in shipCells)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = p.X + dx;
                        int ny = p.Y + dy;
                        if (nx >= 0 && ny >= 0 && nx < GRID_SIZE && ny < GRID_SIZE)
                        {
                            var btn = enemyButtons[nx, ny];
                            if (btn.BackColor == Color.LightBlue)
                            {
                                btn.BackColor = Color.Gray;
                                btn.Enabled = false;
                            }
                        }
                    }
                }
            }
        }


        private void ReadLoop()
        {
            while (true)
            {
                string msg;
                try
                {
                    msg = reader.ReadLine();
                    if (msg == null) break;
                }
                catch
                {
                    break;
                }

                if (msg.StartsWith("SHOT:"))
                {
                    string[] parts = msg.Split(':');
                    int x = int.Parse(parts[1]);
                    int y = int.Parse(parts[2]);
                    Point shot = new Point(x, y);

                    bool hit = false;
                    Ship hitShip = null;

                    foreach (var ship in playerShips)
                    {
                        if (ship.Hit(shot))
                        {
                            hit = true;
                            hitShip = ship;
                            break;
                        }
                    }

                    // пометка на вашем поле и проверка на потопление
                    this.Invoke(new Action(() =>
                    {
                        playerButtons[x, y].BackColor = hit ? Color.Red : Color.Gray;
                        playerButtons[x, y].Enabled = false;

                        if (hitShip != null && hitShip.IsSunk())
                        {
                            HighlightSunkShip(hitShip); // покраска и блокировка вокруг
                        }

                        lblStatus.Text = hit ? "Противник попал." : "Противник промахнулся.";

                        if (!hit)
                            myTurn = true; // если промах — теперь ваш ход
                    }));

                    // отправляем результат: RESULT:x:y:hit(1/0):sunk(1/0):cellsList
                    try
                    {
                        int hitFlag = hit ? 1 : 0;
                        int sunkFlag = (hitShip != null && hitShip.IsSunk()) ? 1 : 0;
                        string cellsList = "";
                        if (sunkFlag == 1)
                        {
                            // кодируем клетки потопленного корабля как "x,y|x,y|..."
                            StringBuilder sb = new StringBuilder();
                            foreach (Point cp in hitShip.Cells)
                            {
                                sb.Append(cp.X).Append(',').Append(cp.Y).Append('|');
                            }
                            if (sb.Length > 0) sb.Length--; // убрать последний '|'
                            cellsList = sb.ToString();
                        }

                        writer.WriteLine($"RESULT:{x}:{y}:{hitFlag}:{sunkFlag}:{cellsList}");
                    }
                    catch { }
                }
                else if (msg.StartsWith("RESULT:"))
                {
                    string[] parts = msg.Split(':');
                    int x = int.Parse(parts[1]);
                    int y = int.Parse(parts[2]);
                    bool hit = parts[3] == "1";
                    bool sunk = parts.Length > 4 && parts[4] == "1";
                    string cellsList = parts.Length > 5 ? parts[5] : "";

                    this.Invoke(new Action(() =>
                    {
                        enemyButtons[x, y].BackColor = hit ? Color.Red : Color.Gray;
                        enemyButtons[x, y].Enabled = false;

                        if (sunk && !string.IsNullOrEmpty(cellsList))
                        {
                            // раскодируем клетки и пометим потопленный корабль и его ореол
                            string[] coords = cellsList.Split('|');
                            List<Point> shipCells = new List<Point>();
                            foreach (string c in coords)
                            {
                                string[] xy = c.Split(',');
                                int sx = int.Parse(xy[0]);
                                int sy = int.Parse(xy[1]);
                                shipCells.Add(new Point(sx, sy));
                            }
                            MarkSunkOnEnemy(shipCells.ToArray());
                        }

                        // атакующий получает повторный ход только если попал
                        myTurn = hit;
                        lblStatus.Text = myTurn ? "Ваш ход (попадание)" : "Ход соперника";
                    }));
                }
                else if (msg == "PLACED")
                {
                    opponentShipsPlaced = true;

                    this.Invoke(new Action(() =>
                    {
                        if (shipsPlaced)
                        {
                            if (isHost)
                            {
                                myTurn = true;
                                lblStatus.Text = "Оба готовы. Ваш ход";
                            }
                            else
                            {
                                myTurn = false;
                                lblStatus.Text = "Оба готовы. Ход соперника";
                            }
                        }
                        else
                        {
                            lblStatus.Text = "Противник готов. Нажмите 'Готов', чтобы начать бой.";
                        }
                    }));
                }
            }

            // отключение
            this.Invoke(new Action(() =>
            {
                connected = false;
                lblStatus.Text = "Отключено";
            }));
        }



    }
}

