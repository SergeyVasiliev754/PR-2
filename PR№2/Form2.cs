using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace PR_2
{
    public partial class Form2 : Form
    {
        private NpgsqlConnection conn;
        private readonly Color darkBrown = Color.FromArgb(100, 43, 1);
        public Form2()
        {
            InitializeComponent();
            InitializeDatabaseConnection();
            InitializeUI();
            LoadDebtors();
        }

        private void InitializeDatabaseConnection()
        {
            string connString = "Host=localhost;Port=5432;Database=pr2;Username=postgres;Password=123;";
            conn = new NpgsqlConnection(connString);
        }

        private void InitializeUI()
        {
            // Настройка главного контейнера
            this.Text = "Список задолжников библиотеки";
            this.Size = new Size(800, 600);

            // Главный FlowLayoutPanel
            flowLayoutPanelMain.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanelMain.WrapContents = false;
            flowLayoutPanelMain.Dock = DockStyle.Fill;
            flowLayoutPanelMain.Padding = new Padding(10);

            // Контейнер для прокрутки
            Panel scrollablePanel = new Panel
            {
                AutoScroll = true,
                
                Width = 600, 
                Height = 450 // Задаем высоту
            };

            // Контейнер для списка задолжников
            flowLayoutPanelDebtors.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanelDebtors.WrapContents = false;
            flowLayoutPanelDebtors.AutoSize = true;
            flowLayoutPanelDebtors.Dock = DockStyle.Top;

            scrollablePanel.Controls.Add(flowLayoutPanelDebtors);
            flowLayoutPanelMain.Controls.Add(scrollablePanel);

            this.Controls.Add(flowLayoutPanelMain);
        }


        private DateTime CalculateReturnDate(DateTime receiptDate)
        {
            return receiptDate.AddDays(30); 
        }

        private int CalculateOverdueDays(DateTime returnDate)
        {
            TimeSpan diff = DateTime.Today - returnDate;
            return diff.Days > 0 ? diff.Days : 0;
        }

        private Panel CreateDebtorPanel(string fullName, string phone, string author,
                                      string bookName, int quantity, DateTime receiptDate,
                                      int overdueDays)
        {
            // Основная панель
            var panel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                Width = flowLayoutPanelMain.Width - 30,
                Height = 80,
                Margin = new Padding(0, 0, 0, 10),
             
            };

            // Заголовок с ФИО и телефоном
            var lblReader = new Label
            {
                Text = $"{fullName} / {phone}",
                Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            // Информация о книге
            var lblBookInfo = new Label
            {
                Text = $"{author} - {bookName}",
                Location = new Point(10, 35),
                AutoSize = true
            };

            // Дополнительная информация
            var lblDetails = new Label
            {
                Text = $"Количество: {quantity} | Дата получения: {receiptDate:dd.MM.yyyy} | " +
                       $"Дней задолженности: {overdueDays}",
                Location = new Point(10, 60),
                AutoSize = true
            };

            // Добавляем элементы на панель
            panel.Controls.Add(lblReader);
            panel.Controls.Add(lblBookInfo);
            panel.Controls.Add(lblDetails);

            return panel;
        }

        private void LoadDebtors()
        {
            flowLayoutPanelDebtors.Controls.Clear();

            try
            {
                conn.Open();
                string query = @"
                    SELECT 
                        r.last_name || ' ' || r.first_name || ' ' || COALESCE(r.patronymic, '') AS full_name,
                        r.phone,
                        b.author,
                        b.name,
                        v.quantity,
                        v.date_of_receipt
                    FROM vidacha v
                    JOIN reader r ON v.reader_id = r.id
                    JOIN book b ON v.book_id = b.id
                    WHERE v.return_date IS NULL OR v.return_date > CURRENT_DATE";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime receiptDate = reader.GetDateTime(5);
                            DateTime returnDate = CalculateReturnDate(receiptDate);
                            int overdueDays = CalculateOverdueDays(returnDate);

                            var debtorPanel = CreateDebtorPanel(
                                reader.GetString(0), // ФИО
                                reader.GetString(1), // телефон
                                reader.GetString(2), // автор
                                reader.GetString(3), // название книги
                                reader.GetInt32(4),  // количество
                                receiptDate,         // дата получения
                                overdueDays          // дней задолженности
                            );

                            flowLayoutPanelDebtors.Controls.Add(debtorPanel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadDebtors();
        }

        

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            new Form3().Show();
            this.Close();
        }
    }
}