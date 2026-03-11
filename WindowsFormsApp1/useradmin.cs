using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class useradmin : Form
    {

        private int currentUserId = 0;
        public useradmin()
        {
            InitializeComponent();
            LoadRoles();
            LoadUsers();
            this.Activated += useradmin_Activated;
            dataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#CCE6FF");
            dataGridView1.CellClick += dataGridView1_CellClick;
            textBox2.UseSystemPasswordChar = true; // Скрываем символами *

        }
        private void useradmin_Activated(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }
        private void LoadUsers()
        {
            string query = @"SELECT u.id, u.full_name, u.login, u.password_hash, r.name AS role
                             FROM users u
                             JOIN roles r ON u.role_id = r.id";

            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                MySqlDataAdapter da = new MySqlDataAdapter(query, connection);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
            }

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;

            dataGridView1.Columns["id"].Visible = false;
            dataGridView1.Columns["password_hash"].Visible = false;

            dataGridView1.Columns["full_name"].HeaderText = "ФИО";
            dataGridView1.Columns["login"].HeaderText = "Логин";
            dataGridView1.Columns["role"].HeaderText = "Роль";
        }

        // ===================== LOAD ROLES =====================
        private void LoadRoles()
        {
            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT name FROM roles", connection);
                MySqlDataReader r = cmd.ExecuteReader();

                comboBox1.Items.Clear();
                while (r.Read())
                    comboBox1.Items.Add(r["name"].ToString());
            }
        }

        // ===================== GRID CLICK =====================
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex < 0) return;

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

            currentUserId = Convert.ToInt32(row.Cells["id"].Value);

            string loginInRow = row.Cells["login"].Value.ToString(); // <-- исправлено

            textBox3.Text = row.Cells["full_name"].Value.ToString();
            textBox1.Text = loginInRow;

            // Подставляем хэш в поле пароля (сейчас показывается как ***)
            textBox2.Text = row.Cells["password_hash"].Value.ToString();
            comboBox1.Text = row.Cells["role"].Value.ToString();

            // 🔒 Блокируем поля и кнопки, если выбран текущий пользователь
            bool isCurrentUser = (loginInRow == Session.CurrentLogin);
            textBox1.Enabled = !isCurrentUser;       // нельзя редактировать логин
            textBox2.Enabled = !isCurrentUser;       // нельзя редактировать пароль
            textBox3.Enabled = !isCurrentUser;       // нельзя редактировать ФИО
            comboBox1.Enabled = !isCurrentUser;      // нельзя менять роль
            button2.Enabled = !isCurrentUser;       // кнопка "редактировать"
            button3.Enabled = !isCurrentUser;       // кнопка "удалить"
        }


        private void button5_Click(object sender, EventArgs e)
        {
            mainadmin adminForm = new mainadmin();
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24) // было 0.05
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }

            adminForm.Opacity = 0;
            adminForm.Show();
            // Быстрое появление - 0.4 секунды
            for (double opacity = 0; opacity <= 1.0; opacity += 0.24) // было 0.05
            {
                adminForm.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }
            allowClose = true;
            this.Hide();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (string.IsNullOrWhiteSpace(tb.Text))
                return;

            int cursor = tb.SelectionStart;

            char[] chars = tb.Text.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                // ❌ Запрет цифр
                if (char.IsDigit(chars[i]))
                {
                    chars[i] = '\0';
                    continue;
                }

                // ✅ Разрешение: буквы (только русские), пробел и дефис
                if (char.IsLetter(chars[i]))
                {
                    // Проверка на русскую букву (кириллица)
                    bool isRussianLetter = (chars[i] >= 'А' && chars[i] <= 'Я') ||
                                          (chars[i] >= 'а' && chars[i] <= 'я') ||
                                          chars[i] == 'Ё' || chars[i] == 'ё';

                    if (!isRussianLetter)
                    {
                        chars[i] = '\0';
                        continue;
                    }

                    // ✅ Заглавная буква для первого символа или после пробела/дефиса
                    if (i == 0 || chars[i - 1] == ' ' || chars[i - 1] == '-')
                    {
                        chars[i] = char.ToUpper(chars[i]);
                    }
                    else
                    {
                        chars[i] = char.ToLower(chars[i]);
                    }
                }
                // ❌ Запрет всего, кроме русских букв, пробела и дефиса
                else if (chars[i] != ' ' && chars[i] != '-')
                {
                    chars[i] = '\0';
                    continue;
                }
            }

            tb.Text = new string(chars).Replace("\0", "");
            tb.SelectionStart = Math.Min(cursor, tb.Text.Length);
        }
        private bool IsLoginExists(string login)
        {
            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();
                MySqlCommand cmd =
                    new MySqlCommand("SELECT COUNT(*) FROM users WHERE login=@l", connection);
                cmd.Parameters.AddWithValue("@l", login);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private int GetRoleId(string role)
        {
            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();
                MySqlCommand cmd =
                    new MySqlCommand("SELECT id FROM roles WHERE name=@r", connection);
                cmd.Parameters.AddWithValue("@r", role);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private void ClearFields()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            comboBox1.Text = "";
            currentUserId = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string fio = textBox3.Text.Trim();
            string login = textBox1.Text.Trim();
            string pass = textBox2.Text;           // пароль лучше не Trim()
            string roleName = comboBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(fio) ||
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(pass) ||
                string.IsNullOrWhiteSpace(roleName))
            {
                MessageBox.Show("Заполните все поля", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (IsLoginExists(login))
            {
                MessageBox.Show("Логин уже используется", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int roleId;
            try
            {
                roleId = GetRoleId(roleName);
                if (roleId <= 0) throw new Exception();
            }
            catch
            {
                MessageBox.Show("Выберите корректную роль", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hash = HashPassword(pass);

            string query = @"INSERT INTO users (full_name, login, password_hash, role_id)
                     VALUES (@fio, @login, @pass, @role)";

            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@fio", fio);
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@pass", hash);
                    cmd.Parameters.AddWithValue("@role", roleId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Пользователь успешно добавлен", "Успех",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadUsers();
            ClearFields();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (currentUserId == 0)
            {
                MessageBox.Show("Выберите пользователя для удаления", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (textBox1.Text == Session.CurrentLogin)
            {
                MessageBox.Show("Нельзя удалить пользователя, под которым выполнен вход");
                return;
            }

            if (MessageBox.Show("Удалить пользователя?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand("DELETE FROM users WHERE id=@id", connection))
                {
                    cmd.Parameters.AddWithValue("@id", currentUserId);
                    cmd.ExecuteNonQuery();
                }
            }

            LoadUsers();
            ClearFields();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            if (currentUserId == 0)
            {
                MessageBox.Show("Выберите пользователя для редактирования", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newFullName = textBox3.Text.Trim();
            string newLogin = textBox1.Text.Trim();
            string newPassword = textBox2.Text; // Если пусто — оставляем старый
            string newRole = comboBox1.Text;

            // Проверка логина на уникальность
            string oldLogin = dataGridView1.CurrentRow.Cells["login"].Value.ToString();
            if (newLogin != oldLogin && IsLoginExists(newLogin))
            {
                MessageBox.Show("Логин уже используется другим пользователем", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int roleId = GetRoleId(newRole);
            string query;

            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();

                if (!string.IsNullOrEmpty(newPassword))
                {
                    string hash = HashPassword(newPassword);
                    query = @"UPDATE users 
                      SET full_name=@fio, login=@login, password_hash=@pass, role_id=@role
                      WHERE id=@id";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@fio", newFullName);
                        cmd.Parameters.AddWithValue("@login", newLogin);
                        cmd.Parameters.AddWithValue("@pass", hash);
                        cmd.Parameters.AddWithValue("@role", roleId);
                        cmd.Parameters.AddWithValue("@id", currentUserId);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    query = @"UPDATE users 
                      SET full_name=@fio, login=@login, role_id=@role
                      WHERE id=@id";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@fio", newFullName);
                        cmd.Parameters.AddWithValue("@login", newLogin);
                        cmd.Parameters.AddWithValue("@role", roleId);
                        cmd.Parameters.AddWithValue("@id", currentUserId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // ⚡ Если редактируется текущий пользователь, обновляем сессию
            if (oldLogin == Session.CurrentLogin)
            {
                Session.CurrentLogin = newLogin;
            }

            MessageBox.Show("Пользователь успешно обновлён", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadUsers();
            ClearFields();
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
    



