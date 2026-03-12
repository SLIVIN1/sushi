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
    public partial class categoryadmin : Form
    {
        private int currentCategoryId = 0;

        public categoryadmin()
        {
            InitializeComponent();
            LoadCategories();
            this.Activated += categoryadmin_Activated;
            dataGridView1.CellClick += dataGridView1_CellClick;
            textBox1.TextChanged += textBox1_TextChanged;
            dataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#CCE6FF");
            dataGridView1.DataBindingComplete += dataGridView1_DataBindingComplete;

        }

        private void categoryadmin_Activated(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        // Загрузка категорий
        private void LoadCategories()
        {
            string query = "SELECT id, name, is_deleted FROM categories";
            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                MySqlDataAdapter da = new MySqlDataAdapter(query, connection);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;

                dataGridView1.Columns["id"].Visible = false;
                dataGridView1.Columns["is_deleted"].Visible = false;
                dataGridView1.Columns["name"].HeaderText = "Категория";
            }

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;

        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                bool isDeleted = Convert.ToInt32(row.Cells["is_deleted"].Value) == 1;

                if (isDeleted)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 87); // светло-серый
                    row.DefaultCellStyle.Font = new Font(
                        dataGridView1.Font,
                        FontStyle.Italic
                    );
                }
                else
                {
                    // 🔄 сброс для активных
                    row.DefaultCellStyle.BackColor = dataGridView1.DefaultCellStyle.BackColor;
                    row.DefaultCellStyle.ForeColor = dataGridView1.DefaultCellStyle.ForeColor;
                    row.DefaultCellStyle.Font = dataGridView1.Font;
                }
            }
        }

        // Выбор категории
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            currentCategoryId = Convert.ToInt32(row.Cells["id"].Value);
            textBox1.Text = row.Cells["name"].Value.ToString();
            bool isDeleted = Convert.ToInt32(row.Cells["is_deleted"].Value) == 1;

            if (isDeleted)
            {
                MessageBox.Show(
                    "Категория удалена и доступна только для восстановления",
                    "Архив",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private void ClearFields()
        {
            textBox1.Text = "";
            currentCategoryId = 0;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(tb.Text)) return;

            int cursor = tb.SelectionStart;
            char[] chars = tb.Text.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetter(chars[i]) && chars[i] != ' ')
                    chars[i] = '\0';

                if (char.IsLetter(chars[i]))
                    chars[i] = i == 0 ? char.ToUpper(chars[i]) : char.ToLower(chars[i]);
            }

            tb.Text = new string(chars).Replace("\0", "");
            tb.SelectionStart = Math.Min(cursor, tb.Text.Length);
        }

        // Проверка повторов
        private bool IsCategoryExists(string name)
        {
            using (MySqlConnection conn = DbConfig.GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM categories WHERE name = @name AND is_deleted = 0",
                    conn);

                cmd.Parameters.AddWithValue("@name", name);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
        private bool IsDeletedCategoryExists(string name)
        {
            using (MySqlConnection conn = DbConfig.GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM categories WHERE name = @name AND is_deleted = 1",
                    conn);

                cmd.Parameters.AddWithValue("@name", name);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
        private void RestoreCategory(string name)
        {
            using (MySqlConnection conn = DbConfig.GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE categories SET is_deleted = 0 WHERE name = @name",
                    conn);

                cmd.Parameters.AddWithValue("@name", name);
                cmd.ExecuteNonQuery();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название категории", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 🔴 уже существует и активна
                if (IsCategoryExists(name))
                {
                    MessageBox.Show("Категория с таким названием уже существует", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ♻️ существует, но удалена → восстанавливаем
                if (IsDeletedCategoryExists(name))
                {
                    RestoreCategory(name);

                    MessageBox.Show("Категория восстановлена", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadCategories();
                    ClearFields();
                    return;
                }

                // ➕ новой ещё не было — создаём
                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "INSERT INTO categories (name, is_deleted) VALUES (@name, 0)",
                        connection);

                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Категория успешно добавлена", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadCategories();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (currentCategoryId == 0)
            {
                MessageBox.Show("Выберите категорию для изменения", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (Convert.ToInt32(dataGridView1.CurrentRow.Cells["is_deleted"].Value) == 1)
            {
                MessageBox.Show("Нельзя редактировать удалённую категорию");
                return;
            }

            string name = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название категории", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string oldName = dataGridView1.CurrentRow.Cells["name"].Value.ToString();
            if (name != oldName && IsCategoryExists(name))
            {
                MessageBox.Show("Категория с таким названием уже существует", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE categories SET name=@name WHERE id=@id", connection);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@id", currentCategoryId);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Категория успешно изменена", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadCategories();
            ClearFields();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (currentCategoryId == 0)
            {
                MessageBox.Show("Выберите категорию для удаления");
                return;
            }

            var result = MessageBox.Show(
                "Категория будет скрыта, но сохранится в истории заказов. Продолжить?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE categories SET is_deleted = 1 WHERE id = @id",
                    connection
                );
                cmd.Parameters.AddWithValue("@id", currentCategoryId);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Категория успешно удалена");
            LoadCategories();
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