using MySql.Data.MySqlClient;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class A : Form
    {
        // ====== НАСТРОЙКИ ======
        int failedAttempts = 0;
        private bool isLoggingIn = false;
        bool captchaRequired = false;
        string adminLogin = ConfigurationManager.AppSettings["AdminLogin"];
        string adminPassword = ConfigurationManager.AppSettings["AdminPassword"];
        int captchaIndex = 0;
        CaptchaItem[] captchas;
        System.Windows.Forms.Timer blockTimer = new System.Windows.Forms.Timer();
        public A()
        {
            InitializeComponent();
            captchas = new CaptchaItem[]
            {
                new CaptchaItem { Picture = pictureBox2, Answer = "A7B0" },
                new CaptchaItem { Picture = pictureBox3, Answer = "B73j" },
                new CaptchaItem { Picture = pictureBox4, Answer = "L4Y5" }
            };
            blockTimer.Tick += BlockTimer_Tick; // ✅ подписка ОДИН раз
        }

        class CaptchaItem
        {
            public PictureBox Picture;
            public string Answer;
        }
        private void A_Load(object sender, EventArgs e)
        {
            txtLogin.Focus();
            txtPassword.UseSystemPasswordChar = true;
            InactivityManager.Stop();
            HideCaptcha();
            SetLoginControlsEnabled(true);
            txtLogin.Text = AuthState.LoginText;
            txtPassword.Text = AuthState.PasswordText;
        }

        // ================= ВХОД =================

        private void button1_Click(object sender, EventArgs e)
        {
            Login(txtLogin.Text, txtPassword.Text);
        }

        private void Login(string login, string password)
        {

            if (isLoggingIn) return;              // защита от двойного клика/Enter
            if (captchaRequired)
            {
                MessageBox.Show("Сначала введите капчу");
                return;
            }

            if (login == adminLogin && password == adminPassword)
            {
                vosstan form = new vosstan();
                form.Show();
                this.Hide();
                return;
            }
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            try
            {
                isLoggingIn = true;
                button1.Enabled = false;

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();

                    string query = @"SELECT id, full_name, role_id, password_hash
                             FROM users
                             WHERE BINARY login = @login";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@login", login);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string dbHash = reader["password_hash"].ToString();
                                string inputHash = GetSha256(password);

                                if (dbHash == inputHash)
                                {
                                    failedAttempts = 0;
                                    Session.CurrentLogin = login;
                                    Session.CurrentRole = Convert.ToInt32(reader["role_id"]);

                                    OpenMainForm(
                                        Convert.ToInt32(reader["id"]),
                                        reader["full_name"].ToString(),
                                        Convert.ToInt32(reader["role_id"])
                                    );
                                    return;
                                }
                                else
                                {
                                    failedAttempts++;
                                    MessageBox.Show("Неверный пароль");
                                    ResetPasswordField();
                                }
                            }
                            else
                            {
                                failedAttempts++; // <-- ВАЖНО: неверный логин тоже считается попыткой
                                MessageBox.Show("Неверный логин");
                                ResetPasswordField();
                            }
                        }
                    }
                }

                if (failedAttempts >= 2)
                {
                    captchaIndex = 0;
                    BlockForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к БД:\n" + ex.Message);
            }
            finally
            {
                isLoggingIn = false;
                if (!captchaRequired) button1.Enabled = true; // если капча не требуется — вернули кнопку
            }
        }

        // ================= КАПЧА =================

        private void ShowCaptcha()
        {
            foreach (var c in captchas)
                c.Picture.Visible = false;

            captchas[captchaIndex].Picture.Visible = true;

            textBox1.Visible = true;
            button3.Visible = true;
            textBox1.Clear();
            textBox1.Focus();
        }

        private void HideCaptcha()
        {
            foreach (var c in captchas)
                c.Picture.Visible = false;

            textBox1.Visible = false;
            button3.Visible = false;
        }

        private void NextCaptcha()
        {
            captchaIndex++;
            if (captchaIndex >= captchas.Length)
                captchaIndex = 0;

            ShowCaptcha();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == captchas[captchaIndex].Answer)
            {
                captchaRequired = false;
                failedAttempts = 0;

                HideCaptcha();
                SetLoginControlsEnabled(true);

                txtPassword.Focus();
                MessageBox.Show("Капча введена верно");
            }
            else
            {
                MessageBox.Show("Капча неверна");
                NextCaptcha();
            }
        }

        // ================= БЛОКИРОВКА =================

        private void BlockForm()
        {
            ResetPasswordField();
            SetLoginControlsEnabled(false);

            this.Enabled = false;

            blockTimer.Interval = 10000;
            blockTimer.Start();
        }

        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            blockTimer.Stop();
            this.Enabled = true;

            captchaRequired = true;
            ShowCaptcha();
        }

        // ================= ВСПОМОГАТЕЛЬНОЕ =================

        private void SetLoginControlsEnabled(bool enabled)
        {
            txtLogin.Enabled = enabled;
            txtPassword.Enabled = enabled;
            button1.Enabled = enabled;
        }

        private void ResetPasswordField()
        {
            txtPassword.Clear();
        }

        private string GetSha256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        // ================= ОТКРЫТИЕ ФОРМ =================

        private void OpenMainForm(int userId, string fullName, int roleId)
        {
            Form mainForm = null;

            switch (roleId)
            {
                case 1: mainForm = new mainadmin(); break;
                case 2: mainForm = new maindir(); break;
                case 3: mainForm = new mainmanag(); break;
                default:
                    MessageBox.Show("Неизвестная роль пользователя");
                    return;
            }

            InactivityManager.Start();

            for (double opacity = 1.0; opacity > 0; opacity -= 0.24) // было 0.05
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }

            mainForm.Opacity = 0;
            mainForm.Show();

            // Быстрое появление - 0.4 секунды
            for (double opacity = 0; opacity <= 1.0; opacity += 0.24) // было 0.05
            {
                mainForm.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }

            this.Hide();
        }

        // ================= КНОПКИ UI =================

        private void ButtonShowPassword_Click_1(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы точно хотите выйти?", "Выход",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
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

        private void button4_Click(object sender, EventArgs e)
        {
            AuthState.LoginText = txtLogin.Text;
            AuthState.PasswordText = txtPassword.Text;
            setting nastroika = new setting();
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24) // было 0.05
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }

            nastroika.Opacity = 0;
            nastroika.Show();
            // Быстрое появление - 0.4 секунды
            for (double opacity = 0; opacity <= 1.0; opacity += 0.24) // было 0.05
            {
                nastroika.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }
            allowClose = true;
            this.Hide();
        }

      
    }
}