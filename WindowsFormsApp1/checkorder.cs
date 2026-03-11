using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace WindowsFormsApp1
{
    public partial class checkorder : BaseForm
    {
        private bool allowClose = false;

        public checkorder()
        {
            InitializeComponent();
            LoadOrders();
            LoadOrderStatuses();
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
            button1.Enabled = (statusId == 1);
        }

        private void SetupAccess()
        {
            if (Session.CurrentRole != 1)
            {
                comboBox1.Visible = false;
                button4.Visible = false;
                label5.Visible = false;
            }
        }

        private void LoadOrders()
        {
            try
            {
                string query = @"
                SELECT 
                    o.id,
                    o.customer_name AS 'ФИО',
                    o.phone AS 'Телефон',
                    o.address AS 'Адрес',
                    o.final_total AS 'Сумма',
                    o.order_date AS 'Дата',
                    o.status_id,
                    s.name AS 'Статус'
                FROM orders o
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
                        "SELECT id, name FROM order_statuses WHERE is_deleted = 0",
                        conn
                    );

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

                    // 🔹 Заказ + статус
                    string orderQuery = @"
                    SELECT 
                        o.customer_name,
                        o.phone,
                        o.total,
                        o.discount,
                        o.final_total,
                        o.order_date,
                        s.name AS status_name
                    FROM orders o
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
                            customer = r["customer_name"].ToString();
                            phone = r["phone"].ToString();
                            total = Convert.ToDecimal(r["total"]);
                            discount = Convert.ToDecimal(r["discount"]);
                            finalTotal = Convert.ToDecimal(r["final_total"]);
                            orderDate = Convert.ToDateTime(r["order_date"]);
                            status = r["status_name"].ToString();
                        }
                    }

                    // 🔹 ФИО сотрудника
                    string employeeName = "";
                    MySqlCommand empCmd = new MySqlCommand(
                        "SELECT full_name FROM users WHERE login=@l", conn);
                    empCmd.Parameters.AddWithValue("@l", Session.CurrentLogin);
                    object emp = empCmd.ExecuteScalar();
                    if (emp != null) employeeName = emp.ToString();

                    // 🔹 Товары
                    MySqlDataAdapter da = new MySqlDataAdapter(
                        "SELECT product_name, price, quantity, sum FROM order_items WHERE order_id=@id", conn);
                    da.SelectCommand.Parameters.AddWithValue("@id", orderId);

                    DataTable items = new DataTable();
                    da.Fill(items);

                    // 🔹 Word
                    Word.Application word = new Word.Application();
                    Word.Document doc = word.Documents.Add();
                    word.Visible = true;

                    AddLine(doc, "ЧЕК ЗАКАЗА", 16, true, Word.WdParagraphAlignment.wdAlignParagraphCenter);
                    AddLine(doc, $"Дата чека: {DateTime.Now}");
                    AddLine(doc, $"Дата заказа: {orderDate}");
                    AddLine(doc, $"Статус: {status}");
                    AddLine(doc, $"Клиент: {customer}");
                    AddLine(doc, $"Телефон: {phone}");
                    AddLine(doc, $"Сотрудник: {employeeName}");
                    AddLine(doc, "");

                    Word.Table table = doc.Tables.Add(doc.Bookmarks["\\endofdoc"].Range,
                        items.Rows.Count + 1, 4);
                    table.Borders.Enable = 1;

                    table.Cell(1, 1).Range.Text = "Товар";
                    table.Cell(1, 2).Range.Text = "Цена";
                    table.Cell(1, 3).Range.Text = "Кол-во";
                    table.Cell(1, 4).Range.Text = "Сумма";

                    for (int i = 0; i < items.Rows.Count; i++)
                    {
                        table.Cell(i + 2, 1).Range.Text = items.Rows[i]["product_name"].ToString();
                        table.Cell(i + 2, 2).Range.Text = items.Rows[i]["price"].ToString();
                        table.Cell(i + 2, 3).Range.Text = items.Rows[i]["quantity"].ToString();
                        table.Cell(i + 2, 4).Range.Text = items.Rows[i]["sum"].ToString();
                    }

                    AddLine(doc, "");
                    AddLine(doc, $"Сумма без скидки: {total} ₽");
                    AddLine(doc, $"Скидка: {discount} ₽");
                    AddLine(doc, $"ИТОГ: {finalTotal} ₽", 14, true);
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!allowClose && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
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

        private void button3_Click(object sender, EventArgs e)
        {
            Form mainForm;

            switch (Session.CurrentRole)
            {
                case 1: // администратор
                    mainForm = new mainadmin();
                    break;
                case 2: // директор
                    mainForm = new maindir();
                    break;
                case 3: // менеджер
                    mainForm = new mainmanag();
                    break;
                default:
                    MessageBox.Show("Неизвестная роль пользователя", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }
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
            allowClose = true;
            this.Hide();
        }

    }
}