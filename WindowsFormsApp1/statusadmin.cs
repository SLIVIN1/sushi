using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class statusadmin : Form
    {
        private int currentStatusId = 0;

        public statusadmin()
        {
            InitializeComponent();
            LoadStatuses();
            this.Activated += statusadmin_Activated;
            dataGridView1.CellClick += dataGridView1_CellClick;
            textBox1.TextChanged += textBox1_TextChanged_1;
            dataGridView1.Font = new Font("Times New Roman", 14F, FontStyle.Regular);
            dataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#CCE6FF");

        }

        private void statusadmin_Activated(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        // Загрузка статусов
        private void LoadStatuses()
        {
            string query = "SELECT id, name FROM order_statuses";
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
            dataGridView1.Columns["name"].HeaderText = "Статус";
        }


        // Выбор статуса из DataGridView
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            currentStatusId = Convert.ToInt32(row.Cells["id"].Value);
            textBox1.Text = row.Cells["name"].Value.ToString();
        }

        // Очистка поля
        private void ClearFields()
        {
            textBox1.Text = "";
            currentStatusId = 0;
        }

       

        // Проверка повторов
        private bool IsStatusExists(string name)
        {
            using (MySqlConnection conn = DbConfig.GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM order_statuses WHERE name = @name ",
                    conn);

                cmd.Parameters.AddWithValue("@name", name);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

       
        // Добавление
        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название статуса", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 🔴 если статус уже существует и НЕ удалён
                if (IsStatusExists(name))
                {
                    MessageBox.Show("Статус с таким названием уже существует", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ➕ создаём новый статус
                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "INSERT INTO order_statuses (name) VALUES (@name)",
                        connection);

                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Статус успешно добавлен", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadStatuses();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        // Редактирование
        private void button2_Click(object sender, EventArgs e)
        {
            if (currentStatusId == 0)
            {
                MessageBox.Show("Выберите статус для изменения", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            string name = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название статуса", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string oldName = dataGridView1.CurrentRow.Cells["name"].Value.ToString();
            if (name != oldName && IsStatusExists(name))
            {
                MessageBox.Show("Статус с таким названием уже существует", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE order_statuses SET name=@name WHERE id=@id", connection);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@id", currentStatusId);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Статус успешно изменён", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadStatuses();
            ClearFields();
        }

        // Удаление
        private void button3_Click(object sender, EventArgs e)
        {
            if (currentStatusId == 0)
            {
                MessageBox.Show("Выберите статус для удаления");
                return;
            }

            if (StatusHasOrders(currentStatusId))
            {
                MessageBox.Show(
                    "Нельзя удалить статус, так как существуют заказы с этим статусом.",
                    "Удаление невозможно",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (MessageBox.Show(
                "Удалить статус без возможности восстановления?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "DELETE FROM order_statuses WHERE id = @id",
                    connection);

                cmd.Parameters.AddWithValue("@id", currentStatusId);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Статус успешно удалён");

            LoadStatuses();
            ClearFields();
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
        private bool StatusHasOrders(int statusId)
        {
            using (MySqlConnection conn = DbConfig.GetConnection())
            {
                conn.Open();

                string query = @"
            SELECT COUNT(*)
            FROM orders
            WHERE status_id = @id";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", statusId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }
        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(tb.Text)) return;

            int cursor = tb.SelectionStart;
            char[] chars = tb.Text.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];

                // Оставляем только русские буквы и пробел
                if (!((c >= 'А' && c <= 'я') || c == 'ё' || c == 'Ё' || c == ' '))
                {
                    chars[i] = '\0';
                    continue;
                }

                // Нормализация регистра
                if (c >= 'А' && c <= 'я')
                {
                    if (i == 0)
                        chars[i] = char.ToUpper(c);
                    else
                        chars[i] = char.ToLower(c);
                }
            }

            tb.Text = new string(chars).Replace("\0", "");
            tb.SelectionStart = Math.Min(cursor, tb.Text.Length);
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
