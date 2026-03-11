using Microsoft.SqlServer.Server;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class maindir : Form
    {
        private string fullName = "";
        private string roleName = "";
        public maindir()
        {
            InitializeComponent();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            A authForm = new A(); // предполагаем, что класс формы авторизации называется A
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24) // было 0.05
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }

            authForm.Opacity = 0;
            authForm.Show();
            // Быстрое появление - 0.4 секунды
            for (double opacity = 0; opacity <= 1.0; opacity += 0.24) // было 0.05
            {
                authForm.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }
            allowClose = true;
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            director ychetForm = new director();
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24) // было 0.05
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }

            ychetForm.Opacity = 0;
            ychetForm.Show();
            // Быстрое появление - 0.4 секунды
            for (double opacity = 0; opacity <= 1.0; opacity += 0.24) // было 0.05
            {
                ychetForm.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }
            allowClose = true;
            this.Hide();

        }

        private void maindir_Load(object sender, EventArgs e)
        {
            // Загружаем данные пользователя по логину из Session
            LoadUserInfo();

            // Устанавливаем текст лейблов
            label1.Text = $"Здравствуйте, {fullName}";
            label3.Text = roleName;
        }

        private void LoadUserInfo()
        {
            try
            {
                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT full_name, role_id FROM users WHERE login = @login";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@login", Session.CurrentLogin);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            fullName = reader["full_name"].ToString();
                            int roleId = Convert.ToInt32(reader["role_id"]);

                            // Преобразуем role_id в строковое название
                            switch (roleId)
                            {
                                case 1:
                                    roleName = "Администратор";
                                    break;
                                case 2:
                                    roleName = "Директор";
                                    break;
                                case 3:
                                    roleName = "Менеджер";
                                    break;
                                default:
                                    roleName = "Неизвестная роль";
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных пользователя: " + ex.Message);
            }
        }
        private bool allowClose = false;
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!allowClose && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }

    }
}

