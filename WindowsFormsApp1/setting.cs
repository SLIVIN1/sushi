using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class setting : Form
    {
        public setting()
        {
            InitializeComponent();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!allowClose && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }
        private bool allowClose = false;

        private void button2_Click(object sender, EventArgs e)
        {

            vosstan vosstan = new vosstan();
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24) // было 0.05
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }

            vosstan.Opacity = 0;
            vosstan.Show();
            // Быстрое появление - 0.4 секунды
            for (double opacity = 0; opacity <= 1.0; opacity += 0.24) // было 0.05
            {
                vosstan.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }
            allowClose = true;
            this.Hide();
        }

private void button1_Click(object sender, EventArgs e)
    {
        string host = textBox1.Text.Trim();
        string login = textBox2.Text.Trim();
        string password = textBox3.Text.Trim();
        string database = textBox4.Text.Trim();

        if (host.ToLower() == "localhost")
            host = "127.0.0.1";

        string newConn =
            $"Server={host};Port=3306;Database={database};Uid={login};Pwd={password};";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(newConn))
            {
                conn.Open();
            }

            Properties.Settings.Default.ConnectionString = newConn;
            Properties.Settings.Default.Save();

            MessageBox.Show("Настройки сохранены", "OK",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            A auth = new A();

            for (double opacity = 1.0; opacity > 0; opacity -= 0.24)
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7);
            }

            auth.Opacity = 0;
            auth.Show();

            for (double opacity = 0; opacity <= 1.0; opacity += 0.24)
            {
                auth.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7);
            }

            allowClose = true;
            this.Hide();
        }
            catch
            {
                MessageBox.Show(
                    "Проверьте правильность:\n\n" +
                    "• адреса сервера\n" +
                    "• логина\n" +
                    "• пароля\n" +
                    "• имени базы данных\n\n" +
                    "Подключение к БД не удалось.",
                    "Ошибка подключения",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

    private void setting_Load(object sender, EventArgs e)
        {
            string conn = Properties.Settings.Default.ConnectionString;

            // Парсим строку
            var builder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(conn);

            // 🔥 127.0.0.1 → localhost
            textBox1.Text = builder.Server == "127.0.0.1" ? "localhost" : builder.Server;
            textBox2.Text = builder.UserID;
            textBox3.Text = builder.Password;
            textBox4.Text = builder.Database;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.";

            string result = "";

            foreach (char c in textBox1.Text)
            {
                if (allowed.Contains(c))
                {
                    result += c;
                }
            }

            if (textBox1.Text != result)
            {
                int pos = textBox1.SelectionStart - 1;

                textBox1.Text = result;

                if (pos < 0)
                    pos = 0;

                textBox1.SelectionStart = pos;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            string allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            string result = "";

            foreach (char c in textBox2.Text)
            {
                if (allowed.Contains(c))
                {
                    result += c;
                }
            }

            if (textBox2.Text != result)
            {
                int pos = textBox2.SelectionStart - 1;

                textBox2.Text = result;

                if (pos < 0)
                    pos = 0;

                textBox2.SelectionStart = pos;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)

        {

            string result = "";

            foreach (char c in textBox3.Text)

            {

                // Разрешаем только НЕ русские символы

                if (!(c >= 'А' && c <= 'я') && c != 'ё' && c != 'Ё')

                {

                    result += c;

                }

            }

            if (textBox3.Text != result)

            {

                int pos = textBox3.SelectionStart - 1;

                textBox3.Text = result;

                if (pos < 0)

                    pos = 0;

                textBox3.SelectionStart = pos;

            }

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            string allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            string result = "";

            foreach (char c in textBox4.Text)
            {
                if (allowed.Contains(c))
                {
                    result += c;
                }
            }

            if (textBox4.Text != result)
            {
                int pos = textBox4.SelectionStart - 1;

                textBox4.Text = result;

                if (pos < 0)
                    pos = 0;

                textBox4.SelectionStart = pos;
            }
        }
    }
}
