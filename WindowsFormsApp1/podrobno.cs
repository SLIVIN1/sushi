using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class podrobno : BaseForm
    {
        private long orderId;

        public podrobno(long orderId)
        {
            InitializeComponent();
            this.orderId = orderId;
            this.Activated += podrobno_Activated;
            dataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#CCE6FF");
            LoadOrder();
        }

        private void podrobno_Activated(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }
        private void LoadOrder()
        {
            try
            {
                string query = @"
                SELECT
                    oi.product_name AS 'Товар',
                    oi.quantity     AS 'Количество',
                    oi.price        AS 'Цена',
                    oi.sum          AS 'Сумма',
                    o.order_date    AS 'Дата заказа',
                    o.customer_name AS 'ФИО',
                    o.address       AS 'Адрес'
                FROM order_items oi
                JOIN orders o ON oi.order_id = o.id
                WHERE o.id = @orderId";

                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@orderId", orderId);

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    da.Fill(table);

                    dataGridView1.DataSource = table;

                    dataGridView1.ReadOnly = true;
                    dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dataGridView1.MultiSelect = false;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView1.RowTemplate.Height = 40;

                    // 🔹 считаем суммы
                    CalculateTotals(table);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказа: " + ex.Message);
            }
        }

        private void CalculateTotals(DataTable table)
        {
            decimal totalSum = 0;

            foreach (DataRow row in table.Rows)
            {
                if (row["Сумма"] != DBNull.Value)
                    totalSum += Convert.ToDecimal(row["Сумма"]);
            }

            decimal discount = 0;

            // ✅ фиксированная скидка
            if (totalSum >= 3500)
                discount = totalSum * 0.15m;

            decimal finalSum = totalSum - discount;

            // 🔹 вывод в label
            label1.Text = $"Общая сумма: {totalSum:F2} ₽";
            label2.Text = $"Скидка (15%): {discount:F2} ₽";
            label3.Text = $"Итого к оплате: {finalSum:F2} ₽";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form targetForm = null;

            // Определяем форму в зависимости от роли
            // 1 — админ, 2 — менеджер, 3 — директор
            if (Session.CurrentRole == 1 || Session.CurrentRole == 3)
            {
                targetForm = new checkorder();
            }
            else if (Session.CurrentRole == 2)
            {
                targetForm = new director();
            }
            else
            {
                MessageBox.Show("Неизвестная роль пользователя");
                return;
            }

            // Плавное скрытие текущей формы
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24)
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7);
            }

            // Показ целевой формы с плавным появлением
            targetForm.Opacity = 0;
            targetForm.Show();
            for (double opacity = 0; opacity <= 1.0; opacity += 0.24)
            {
                targetForm.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7);
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