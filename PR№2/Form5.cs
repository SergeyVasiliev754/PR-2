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
    public partial class Form5 : Form
    {
        private NpgsqlConnection conn;
        private DataTable readersTable;
        private int currentReaderId = -1;
        public Form5()
        {
            InitializeComponent();
            InitializeComponent();
            InitializeDatabaseConnection();
            InitializeUI();
            LoadReaders();
        }
        private void InitializeDatabaseConnection()
        {
            string connString = "Host=localhost;Port=5432;Database=pr2;Username=postgres;Password=123;";
            conn = new NpgsqlConnection(connString);
        }

        private void InitializeUI()
        {
            // Настройка формы
            this.Text = "Учет читателей библиотеки";
            this.Size = new Size(900, 700);
           

            // Главный контейнер
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                RowStyles =
                {
                    new RowStyle(SizeType.Percent, 60F), // Для GridView
                    new RowStyle(SizeType.Absolute, 250F), // Для формы редактирования
                    new RowStyle(SizeType.Absolute, 50F) // Для кнопок
                }
            };

            // DataGridView для отображения читателей
            dataGridViewReaders = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            dataGridViewReaders.SelectionChanged += DataGridViewReaders_SelectionChanged;

            // Панель для формы редактирования
            var editPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            InitializeEditControls(editPanel);

            // Панель для кнопок
            var buttonPanel = new Panel { Dock = DockStyle.Fill };
            InitializeButtons(buttonPanel);

            // Добавление элементов на главную панель
            mainPanel.Controls.Add(dataGridViewReaders, 0, 0);
            mainPanel.Controls.Add(editPanel, 0, 1);
            mainPanel.Controls.Add(buttonPanel, 0, 2);

            this.Controls.Add(mainPanel);
        }

        private void InitializeEditControls(Panel container)
        {
            var editPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(15),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                BackColor = Color.White
            };

            // Настройка столбцов и строк
            editPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            editPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            for (int i = 0; i < 7; i++)
            {
                editPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            }
            editPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Поля формы редактирования
            var lblLastName = new Label { Text = "Фамилия:", TextAlign = ContentAlignment.MiddleRight };
            txtLastName = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };

            var lblFirstName = new Label { Text = "Имя:", TextAlign = ContentAlignment.MiddleRight };
            txtFirstName = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };

            var lblPatronymic = new Label { Text = "Отчество:", TextAlign = ContentAlignment.MiddleRight };
            txtPatronymic = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };

            var lblBirthDate = new Label { Text = "Дата рождения:", TextAlign = ContentAlignment.MiddleRight };
            dtpBirthDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };

            var lblWorkPlace = new Label { Text = "Место работы/учёбы:", TextAlign = ContentAlignment.MiddleRight };
            txtWorkPlace = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };

            var lblPhone = new Label { Text = "Телефон:", TextAlign = ContentAlignment.MiddleRight };
            txtPhone = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };

            var lblEmail = new Label { Text = "Email:", TextAlign = ContentAlignment.MiddleRight };
            txtEmail = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };

            var lblAddress = new Label { Text = "Адрес:", TextAlign = ContentAlignment.MiddleRight };
            txtAddress = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3), Multiline = true, Height = 60 };

            // Размещение элементов
            editPanel.Controls.Add(lblLastName, 0, 0);
            editPanel.Controls.Add(txtLastName, 1, 0);
            editPanel.Controls.Add(lblFirstName, 0, 1);
            editPanel.Controls.Add(txtFirstName, 1, 1);
            editPanel.Controls.Add(lblPatronymic, 0, 2);
            editPanel.Controls.Add(txtPatronymic, 1, 2);
            editPanel.Controls.Add(lblBirthDate, 0, 3);
            editPanel.Controls.Add(dtpBirthDate, 1, 3);
            editPanel.Controls.Add(lblWorkPlace, 0, 4);
            editPanel.Controls.Add(txtWorkPlace, 1, 4);
            editPanel.Controls.Add(lblPhone, 0, 5);
            editPanel.Controls.Add(txtPhone, 1, 5);
            editPanel.Controls.Add(lblEmail, 0, 6);
            editPanel.Controls.Add(txtEmail, 1, 6);
            editPanel.Controls.Add(lblAddress, 0, 7);
            editPanel.Controls.Add(txtAddress, 1, 7);

            container.Controls.Add(editPanel);
        }

        private void InitializeButtons(Panel container)
        {
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 5, 20, 0)
            };

            // Кнопка сохранения
            btnSave = new Button
            {
                Text = "Сохранить изменения",
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                Width = 180,
                Height = 40,
                Margin = new Padding(10, 0, 0, 0),
                BackColor = Color.FromArgb(100, 43, 1), // Установка фона на #642B01
                ForeColor = Color.White // Установка цвета текста на белый
            };
            btnSave.Click += BtnSave_Click;

            // Кнопка добавления
            btnAdd = new Button
            {
                Text = "Добавить нового",
                Font = new Font("Segoe UI", 10),
                Width = 180,
                Height = 40,
                Margin = new Padding(10, 0, 0, 0),
                BackColor = Color.FromArgb(100, 43, 1), // Установка фона на #642B01
                ForeColor = Color.White // Установка цвета текста на белый
            };
            btnAdd.Click += BtnAdd_Click;

            // Кнопка обновления
            btnRefresh = new Button
            {
                Text = "Обновить",
                Font = new Font("Segoe UI", 10),
                Width = 180,
                Height = 40,
                BackColor = Color.FromArgb(100, 43, 1), // Установка фона на #642B01
                ForeColor = Color.White // Установка цвета текста на белый
            };
            btnRefresh.Click += BtnRefresh_Click;

            // Кнопка "Назад"
            Button btnBack = new Button
            {
                Text = "Назад",
                Font = new Font("Segoe UI", 10),
                Width = 180,
                Height = 40,
                BackColor = Color.FromArgb(100, 43, 1), // Установка фона на #642B01
                ForeColor = Color.White // Установка цвета текста на белый
            };
            btnBack.Click += BtnBack_Click;

          
            buttonPanel.Controls.Add(btnBack);


            buttonPanel.Controls.Add(btnSave);
            buttonPanel.Controls.Add(btnAdd);
            buttonPanel.Controls.Add(btnRefresh);

            container.Controls.Add(buttonPanel);
        }



        private void LoadReaders()
        {
            try
            {
                conn.Open();
                string query = @"
                    SELECT id, last_name, first_name, patronymic, 
                           date_of_birth, work_place, phone, adress, email 
                    FROM reader 
                    ORDER BY last_name, first_name";

                readersTable = new DataTable();
                new NpgsqlDataAdapter(query, conn).Fill(readersTable);

                dataGridViewReaders.DataSource = readersTable;
                ConfigureGridView();

                if (dataGridViewReaders.Rows.Count > 0)
                {
                    dataGridViewReaders.Rows[0].Selected = true;
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

        private void ConfigureGridView()
        {
            dataGridViewReaders.Columns["id"].Visible = false;

            dataGridViewReaders.Columns["last_name"].HeaderText = "Фамилия";
            dataGridViewReaders.Columns["first_name"].HeaderText = "Имя";
            dataGridViewReaders.Columns["patronymic"].HeaderText = "Отчество";
            dataGridViewReaders.Columns["date_of_birth"].HeaderText = "Дата рождения";
            dataGridViewReaders.Columns["work_place"].HeaderText = "Место работы/учёбы";
            dataGridViewReaders.Columns["phone"].HeaderText = "Телефон";
            dataGridViewReaders.Columns["adress"].HeaderText = "Адрес";
            dataGridViewReaders.Columns["email"].HeaderText = "Email";

            dataGridViewReaders.Columns["date_of_birth"].DefaultCellStyle.Format = "dd.MM.yyyy";
        }

        private void DataGridViewReaders_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewReaders.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridViewReaders.SelectedRows[0];
                currentReaderId = Convert.ToInt32(row.Cells["id"].Value);

                txtLastName.Text = row.Cells["last_name"].Value.ToString();
                txtFirstName.Text = row.Cells["first_name"].Value.ToString();
                txtPatronymic.Text = row.Cells["patronymic"].Value?.ToString() ?? "";
                dtpBirthDate.Value = (DateTime)row.Cells["date_of_birth"].Value;
                txtWorkPlace.Text = row.Cells["work_place"].Value?.ToString() ?? "";
                txtPhone.Text = row.Cells["phone"].Value?.ToString() ?? "";
                txtEmail.Text = row.Cells["email"].Value?.ToString() ?? "";
                txtAddress.Text = row.Cells["adress"].Value?.ToString() ?? "";
            }
        }
        private void BtnBack_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3(); 
            form3.Show();
            this.Hide(); 
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // Очищаем поля для нового читателя
            currentReaderId = -1;
            txtLastName.Text = "";
            txtFirstName.Text = "";
            txtPatronymic.Text = "";
            dtpBirthDate.Value = DateTime.Now;
            txtWorkPlace.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            txtAddress.Text = "";

            // Снимаем выделение в GridView
            dataGridViewReaders.ClearSelection();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Фамилия и имя обязательны для заполнения!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                conn.Open();

                if (currentReaderId > 0)
                {
                    // Редактирование существующего читателя
                    string updateQuery = @"
                        UPDATE reader SET
                            last_name = @last_name,
                            first_name = @first_name,
                            patronymic = @patronymic,
                            date_of_birth = @date_of_birth,
                            work_place = @work_place,
                            phone = @phone,
                            adress = @adress,
                            email = @email
                        WHERE id = @id";

                    using (var cmd = new NpgsqlCommand(updateQuery, conn))
                    {
                        FillParameters(cmd);
                        cmd.Parameters.AddWithValue("@id", currentReaderId);
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
                else
                {
                    // Добавление нового читателя
                    string insertQuery = @"
                        INSERT INTO reader (
                            last_name, first_name, patronymic, date_of_birth, 
                            work_place, phone, adress, email)
                        VALUES (
                            @last_name, @first_name, @patronymic, @date_of_birth, 
                            @work_place, @phone, @adress, @email)";

                    using (var cmd = new NpgsqlCommand(insertQuery, conn))
                    {
                        FillParameters(cmd);
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                // Обновляем список читателей
                LoadReaders();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void FillParameters(NpgsqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@last_name", txtLastName.Text.Trim());
            cmd.Parameters.AddWithValue("@first_name", txtFirstName.Text.Trim());
            cmd.Parameters.AddWithValue("@patronymic", string.IsNullOrWhiteSpace(txtPatronymic.Text) ?
                (object)DBNull.Value : txtPatronymic.Text.Trim());
            cmd.Parameters.AddWithValue("@date_of_birth", dtpBirthDate.Value);
            cmd.Parameters.AddWithValue("@work_place", string.IsNullOrWhiteSpace(txtWorkPlace.Text) ?
                (object)DBNull.Value : txtWorkPlace.Text.Trim());
            cmd.Parameters.AddWithValue("@phone", string.IsNullOrWhiteSpace(txtPhone.Text) ?
                (object)DBNull.Value : txtPhone.Text.Trim());
            cmd.Parameters.AddWithValue("@adress", string.IsNullOrWhiteSpace(txtAddress.Text) ?
                (object)DBNull.Value : txtAddress.Text.Trim());
            cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(txtEmail.Text) ?
                (object)DBNull.Value : txtEmail.Text.Trim());
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadReaders();
        }

        // Объявления элементов управления
        private DataGridView dataGridViewReaders;
        private TextBox txtLastName;
        private TextBox txtFirstName;
        private TextBox txtPatronymic;
        private DateTimePicker dtpBirthDate;
        private TextBox txtWorkPlace;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private TextBox txtAddress;
        private Button btnSave;
        private Button btnAdd;
        private Button btnRefresh;

        private void Form5_Load(object sender, EventArgs e)
        {

        }
    }
}