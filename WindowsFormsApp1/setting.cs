using Mysqlx.Crud;
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
    public partial class setting : BaseForm
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

            A auth = new A();
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24) // было 0.05
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }

            auth.Opacity = 0;
            auth.Show();
            // Быстрое появление - 0.4 секунды
            for (double opacity = 0; opacity <= 1.0; opacity += 0.24) // было 0.05
            {
                auth.Opacity = opacity;
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

            Properties.Settings.Default.ConnectionString = newConn;
            Properties.Settings.Default.Save();

            MessageBox.Show("Настройки сохранены", "OK",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            A auth = new A();
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24) // было 0.05
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }

            auth.Opacity = 0;
            auth.Show();
            // Быстрое появление - 0.4 секунды
            for (double opacity = 0; opacity <= 1.0; opacity += 0.24) // было 0.05
            {
                auth.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }
            allowClose = true;
            this.Hide();


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
    }
}
