using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace WindowsFormsApp1
{
    public partial class checkorder : Form
    {
        private bool allowClose = false;
        private const int ROLE_ADMIN = 1;
        private const int ROLE_MANAGER = 3;

        public checkorder()
        {
            InitializeComponent();
            LoadOrders();
            LoadOrderStatuses();
            dataGridView1.Font = new Font("Times New Roman", 14F, FontStyle.Regular);
            this.Activated += checkorder_Activated;
            SetupAccess();
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            button1.Enabled = false;
            dataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#CCE6FF");
            dataGridView1.CellClick += dataGridView1_CellClick;
        }

        private void checkorder_Activated(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                button1.Enabled = false;
                return;
            }

            int statusId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["status_id"].Value);

            // 🔥 ПЕЧАТЬ ТОЛЬКО ДЛЯ МЕНЕДЖЕРА И ТОЛЬКО ЕСЛИ СТАТУС 3
            button1.Enabled = (Session.CurrentRole == ROLE_MANAGER && statusId == 3);
        }

        private void SetupAccess()
        {
            if (Session.CurrentRole == ROLE_ADMIN)
            {
                // ✅ Админ — видит всё, меняет статусы
                comboBox1.Visible = true;
                button4.Visible = true;
                label5.Visible = true;
                button1.Visible = false;
            }
            else if (Session.CurrentRole == ROLE_MANAGER)
            {
                // ✅ Менеджер — видит всё, печатает чеки
                comboBox1.Visible = true;
                button4.Visible = true;
                label5.Visible = true;
                button1.Visible = true;
            }
        }

        private void LoadOrders()
        {
            try
            {
                string query = @"
                SELECT 
                    o.id,
                    c.name AS 'ФИО',
                    c.phone AS 'Телефон',
                    c.address AS 'Адрес',
                    o.final_total AS 'Сумма',
                    o.order_date AS 'Дата',
                    o.status_id,
                    s.name AS 'Статус'
                FROM orders o
                LEFT JOIN customers c ON o.customer_id = c.id
                LEFT JOIN order_statuses s ON o.status_id = s.id
                ORDER BY o.order_date DESC";

                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView1.DataSource = dt;
                    dataGridView1.Columns["id"].Visible = false;
                    dataGridView1.Columns["status_id"].Visible = false;

                    dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dataGridView1.MultiSelect = false;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказов: " + ex.Message);
            }
        }

        private void LoadOrderStatuses()
        {
            try
            {
                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(
                        "SELECT id, name FROM order_statuses", conn);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboBox1.DataSource = dt;
                    comboBox1.DisplayMember = "name";
                    comboBox1.ValueMember = "id";
                    comboBox1.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки статусов: " + ex.Message);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dataGridView1.Rows[e.RowIndex];
            if (row.Cells["status_id"].Value != null)
            {
                comboBox1.SelectedValue = Convert.ToInt32(row.Cells["status_id"].Value);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null || dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите заказ и статус");
                return;
            }

            int statusId = (int)comboBox1.SelectedValue;
            long orderId = Convert.ToInt64(dataGridView1.SelectedRows[0].Cells["id"].Value);

            try
            {
                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand(
                        "UPDATE orders SET status_id=@s WHERE id=@id", conn);

                    cmd.Parameters.AddWithValue("@s", statusId);
                    cmd.Parameters.AddWithValue("@id", orderId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Статус обновлён");

                if (statusId == 3 && Session.CurrentRole == ROLE_MANAGER)
                {
                    DialogResult result = MessageBox.Show(
                        "Заказ завершён. Распечатать чек?",
                        "Печать",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (result == DialogResult.Yes)
                    {
                        button1_Click(null, null);
                    }
                }

                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка обновления статуса: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите заказ");
                return;
            }

            long orderId = Convert.ToInt64(dataGridView1.SelectedRows[0].Cells["id"].Value);

            try
            {
                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();

                    string orderQuery = @"
                    SELECT 
                        c.name, c.phone,
                        o.total, o.discount, o.final_total,
                        o.order_date,
                        s.name AS status_name
                    FROM orders o
                    LEFT JOIN customers c ON o.customer_id = c.id
                    LEFT JOIN order_statuses s ON o.status_id = s.id
                    WHERE o.id = @id";

                    MySqlCommand cmd = new MySqlCommand(orderQuery, conn);
                    cmd.Parameters.AddWithValue("@id", orderId);

                    string customer = "", phone = "", status = "";
                    decimal total = 0, discount = 0, finalTotal = 0;
                    DateTime orderDate = DateTime.Now;

                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            customer = r["name"].ToString();
                            phone = r["phone"].ToString();
                            total = Convert.ToDecimal(r["total"]);
                            discount = Convert.ToDecimal(r["discount"]);
                            finalTotal = Convert.ToDecimal(r["final_total"]);
                            orderDate = Convert.ToDateTime(r["order_date"]);
                            status = r["status_name"].ToString();
                        }
                    }

                    string employeeName = "";
                    MySqlCommand empCmd = new MySqlCommand(
                        "SELECT full_name FROM users WHERE login=@l", conn);
                    empCmd.Parameters.AddWithValue("@l", Session.CurrentLogin);
                    object emp = empCmd.ExecuteScalar();
                    if (emp != null) employeeName = emp.ToString();

                    MySqlDataAdapter da = new MySqlDataAdapter(@"
                        SELECT 
                            p.name AS product_name,
                            oi.price, oi.quantity, oi.sum
                        FROM order_items oi
                        LEFT JOIN products p ON oi.product_id = p.id
                        WHERE oi.order_id=@id", conn);
                    da.SelectCommand.Parameters.AddWithValue("@id", orderId);

                    DataTable items = new DataTable();
                    da.Fill(items);

                    Word.Application word = new Word.Application();
                    Word.Document doc = word.Documents.Add();
                    word.Visible = true;

                    doc.PageSetup.LeftMargin = 20;
                    doc.PageSetup.RightMargin = 20;

                    AddLine(doc, "SUSHI", 14, true, Word.WdParagraphAlignment.wdAlignParagraphCenter);
                    AddLine(doc, "ул. Примерная 10", 10, false, Word.WdParagraphAlignment.wdAlignParagraphCenter);
                    AddLine(doc, "----------------------------");
                    AddLine(doc, $"Дата: {orderDate}");
                    AddLine(doc, $"Статус: {status}");
                    AddLine(doc, $"Клиент: {customer}");
                    AddLine(doc, $"Телефон: {phone}");
                    AddLine(doc, $"Кассир: {employeeName}");
                    AddLine(doc, "----------------------------");

                    foreach (DataRow r in items.Rows)
                    {
                        string name = r["product_name"].ToString();
                        decimal price = Convert.ToDecimal(r["price"]);
                        int qty = Convert.ToInt32(r["quantity"]);
                        decimal sum = Convert.ToDecimal(r["sum"]);

                        AddLine(doc, name);
                        AddLine(doc, $"   {qty} x {price:0.00}        {sum:0.00}");
                    }

                    AddLine(doc, "----------------------------");
                    AddLine(doc, $"СУММА:   {total:0.00} ₽");
                    AddLine(doc, $"СКИДКА:  {discount:0.00} ₽");
                    AddLine(doc, $"ИТОГ:    {finalTotal:0.00} ₽", 12, true);
                    AddLine(doc, "----------------------------");
                    AddLine(doc, "СПАСИБО ЗА ПОКУПКУ", 12, true,
                        Word.WdParagraphAlignment.wdAlignParagraphCenter);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void AddLine(Word.Document doc, string text, int size = 11,
            bool bold = false,
            Word.WdParagraphAlignment align = Word.WdParagraphAlignment.wdAlignParagraphLeft)
        {
            Word.Paragraph p = doc.Content.Paragraphs.Add();
            p.Range.Text = text;
            p.Range.Font.Size = size;
            p.Range.Font.Bold = bold ? 1 : 0;
            p.Alignment = align;
            p.Range.InsertParagraphAfter();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form mainForm;

            switch (Session.CurrentRole)
            {
                case 1: mainForm = new mainadmin(); break;
                case 3: mainForm = new mainmanag(); break;
                default:
                    MessageBox.Show("Доступ запрещён", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }

            for (double opacity = 1.0; opacity > 0; opacity -= 0.24)
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7);
            }

            mainForm.Opacity = 0;
            mainForm.Show();

            for (double opacity = 0; opacity <= 1.0; opacity += 0.24)
            {
                mainForm.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7);
            }
            allowClose = true;
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите заказ");
                return;
            }

            long orderId = Convert.ToInt64(
                dataGridView1.SelectedRows[0].Cells["id"].Value
            );

            podrobno f = new podrobno(orderId);
            allowClose = true;
            f.Show();
            this.Close();
        }
    }
}
