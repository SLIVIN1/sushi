using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class statusadmin : BaseForm
    {
        private int currentStatusId = 0;

        public statusadmin()
        {
            InitializeComponent();
            LoadStatuses();
            this.Activated += statusadmin_Activated;
            dataGridView1.CellClick += dataGridView1_CellClick;
            textBox1.TextChanged += textBox1_TextChanged_1;
            dataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#CCE6FF");
            dataGridView1.DataBindingComplete += dataGridView1_DataBindingComplete;

        }

        private void statusadmin_Activated(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        // Загрузка статусов
        private void LoadStatuses()
        {
            string query = "SELECT id, name, is_deleted FROM order_statuses";
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
            dataGridView1.Columns["is_deleted"].Visible = false;
            dataGridView1.Columns["name"].HeaderText = "Статус";
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (Convert.ToInt32(row.Cells["is_deleted"].Value) == 1)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 87); // светло-серый
                    row.DefaultCellStyle.ForeColor = Color.DimGray;
                    row.DefaultCellStyle.Font = new Font(
                        dataGridView1.Font,
                        FontStyle.Italic
                    );
                }
                else
                {
                    // 🔄 сброс стиля для активных
                    row.DefaultCellStyle.BackColor = dataGridView1.DefaultCellStyle.BackColor;
                    row.DefaultCellStyle.ForeColor = dataGridView1.DefaultCellStyle.ForeColor;
                    row.DefaultCellStyle.Font = dataGridView1.Font;
                }
            }
        }

        // Выбор статуса из DataGridView
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            currentStatusId = Convert.ToInt32(row.Cells["id"].Value);
            textBox1.Text = row.Cells["name"].Value.ToString();
            bool isDeleted = Convert.ToInt32(row.Cells["is_deleted"].Value) == 1;

            if (isDeleted)
            {
                textBox1.Text = row.Cells["name"].Value.ToString();
                MessageBox.Show(
                    "Этот статус удалён и доступен только для восстановления",
                    "Архив",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
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
                    "SELECT COUNT(*) FROM order_statuses WHERE name = @name AND is_deleted = 0",
                    conn);

                cmd.Parameters.AddWithValue("@name", name);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool IsDeletedStatusExists(string name)
        {
            using (MySqlConnection conn = DbConfig.GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM order_statuses WHERE name = @name AND is_deleted = 1",
                    conn);

                cmd.Parameters.AddWithValue("@name", name);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
        private void RestoreStatus(string name)
        {
            using (MySqlConnection conn = DbConfig.GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE order_statuses SET is_deleted = 0 WHERE name = @name",
                    conn);

                cmd.Parameters.AddWithValue("@name", name);
                cmd.ExecuteNonQuery();
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

                // ♻️ если статус существует, но удалён — восстанавливаем
                if (IsDeletedStatusExists(name))
                {
                    RestoreStatus(name);

                    MessageBox.Show("Статус восстановлен", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadStatuses();
                    ClearFields();
                    return;
                }

                // ➕ создаём новый статус
                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "INSERT INTO order_statuses (name, is_deleted) VALUES (@name, 0)",
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
            if (Convert.ToInt32(dataGridView1.CurrentRow.Cells["is_deleted"].Value) == 1)
            {
                MessageBox.Show("Нельзя редактировать удалённый статус");
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

            if (MessageBox.Show(
                "Статус будет скрыт, но сохранится в истории заказов. Продолжить?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE order_statuses SET is_deleted = 1 WHERE id = @id",
                    connection);
                cmd.Parameters.AddWithValue("@id", currentStatusId);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Статус успешно скрыт");
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
       
        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(tb.Text)) return;

            int cursor = tb.SelectionStart;
            char[] chars = tb.Text.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetter(chars[i]) && chars[i] != ' ') // Запрет цифр и символов
                    chars[i] = '\0';

                if (char.IsLetter(chars[i]))
                {
                    if (i == 0)
                        chars[i] = char.ToUpper(chars[i]); // Заглавная первая буква
                    else
                        chars[i] = char.ToLower(chars[i]);
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
