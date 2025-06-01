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
    public partial class Form1 : Form
    {
        private NpgsqlConnection connection;
        public Form1()
        {
            InitializeComponent();
            string connectionString = "Host=localhost;Port=5432;Database=pr2;Username=postgres;Password=123;";
            connection = new NpgsqlConnection(connectionString);

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }


        
        
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                connection.Open();


                string username = txtUsername.Text;
                string password = txtPassword.Text;


                string query = "SELECT * from users WHERE username = @username AND password = @password";
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);

                NpgsqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {

                    reader.Close();
                    Hide();
                    new Form3().Show();
                    MessageBox.Show("Добро пожаловать!");
                }
                else
                {

                    MessageBox.Show("Неверный логин или пароль.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {

                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }
    }
}
