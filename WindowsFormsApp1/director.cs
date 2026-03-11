using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace WindowsFormsApp1
{
    public partial class director : BaseForm
    {
        DataTable ordersTable = new DataTable();
        private bool allowClose = false;
        public director()
        {
            InitializeComponent();

            InitializeDateTimePickers(); // <-- обязательно!
            InitializeComboBoxes();
            SetupDataGridView();
            this.Activated += director_Activated;
            // Восстановление состояния фильтров
            dateTimePicker1.Value = DirectorState.DateFrom;
            dateTimePicker2.Value = DirectorState.DateTo;
            textBox1.Text = DirectorState.OrderId;
            comboBox1.SelectedIndex = DirectorState.StatusIndex;
            comboBox2.SelectedIndex = DirectorState.SortIndex;
            // Применяем фильтры сразу после восстановления состояния
            ApplyFilters();
        }

        private void director_Activated(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }
        private void InitializeDateTimePickers()
        {
            dateTimePicker1.Value = DateTime.Today;
            dateTimePicker1.MinDate = DateTime.Today.AddMonths(-6);
            dateTimePicker1.MaxDate = DateTime.Today;
            dateTimePicker1.ValueChanged += DateTimePicker1_ValueChanged;

            dateTimePicker2.Value = DateTime.Today;
            dateTimePicker2.MinDate = DateTime.Today.AddMonths(-1);
            dateTimePicker2.MaxDate = DateTime.Today;
            dateTimePicker2.ValueChanged += DateTimePicker2_ValueChanged;
        }

        private void InitializeComboBoxes()
        {
            // Заполняем ComboBox для фильтрации по статусам
            comboBox1.Items.Add("Все статусы");
            comboBox1.SelectedIndex = 0;
            LoadStatuses();

            // Заполняем ComboBox для сортировки
            comboBox2.Items.Add("По убыванию цены");
            comboBox2.Items.Add("По возрастанию цены");
            comboBox2.SelectedIndex = 0;

            // Обработчики событий
            comboBox1.SelectedIndexChanged += (s, e) => ApplyFilters();
            comboBox2.SelectedIndexChanged += (s, e) => ApplyFilters();
        }

        

        private void LoadStatuses()
        {
            try
            {
                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT id, name FROM order_statuses WHERE is_deleted = 0 ORDER BY name";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBox1.Items.Add(reader["name"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки статусов: " + ex.Message);
            }
        }

        private void DateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            if (dateTimePicker2.Value.Date < dateTimePicker1.Value.Date)
            {
                MessageBox.Show(
                    "Дата окончания не может быть меньше даты начала периода",
                    "Некорректный период",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                dateTimePicker2.Value = dateTimePicker1.Value.Date;
                return;
            }

            ApplyFilters();
        }

        private void DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker2.MinDate = dateTimePicker1.Value.Date;

            // если вдруг уже выбрана некорректная дата — исправляем
            if (dateTimePicker2.Value.Date < dateTimePicker1.Value.Date)
            {
                dateTimePicker2.Value = dateTimePicker1.Value.Date;
            }

            ApplyFilters();
        }

        // ================== ФИЛЬТРАЦИЯ И СОРТИРОВКА ==================
        private void ApplyFilters()
        {
            DateTime dateFrom = dateTimePicker1.Value.Date;
            DateTime dateTo = dateTimePicker2.Value.Date.AddDays(1);

            string statusFilter = "";
            if (comboBox1.SelectedIndex > 0)
            {
                statusFilter = "AND s.name = @status";
            }

            string orderBy = "o.order_date DESC"; // сортировка по умолчанию
            if (comboBox2.SelectedIndex == 0) // По убыванию цены
            {
                orderBy = "o.final_total DESC";
            }
            else if (comboBox2.SelectedIndex == 1) // По возрастанию цены
            {
                orderBy = "o.final_total ASC";
            }

            string searchFilter = "";
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                searchFilter = "AND o.id LIKE @orderId"; // live search
            }

            try
            {
                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();

                    string sql = $@"
                SELECT
                    o.id            AS 'ID',
                    o.customer_name AS 'Клиент',
                    o.phone         AS 'Телефон',
                    o.total         AS 'Сумма без скидки',
                    o.discount      AS 'Скидка ₽',
                    o.final_total   AS 'Итого',
                    s.name          AS 'Статус',
                    o.order_date    AS 'Дата заказа'
                FROM orders o
                LEFT JOIN order_statuses s ON o.status_id = s.id
                WHERE o.order_date >= @from AND o.order_date < @to
                {statusFilter}
                {searchFilter}
                ORDER BY {orderBy}";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@from", dateFrom);
                    cmd.Parameters.AddWithValue("@to", dateTo);

                    if (comboBox1.SelectedIndex > 0)
                    {
                        cmd.Parameters.AddWithValue("@status", comboBox1.SelectedItem.ToString());
                    }

                    if (!string.IsNullOrWhiteSpace(textBox1.Text))
                    {
                        cmd.Parameters.AddWithValue("@orderId", "%" + textBox1.Text + "%");
                    }

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    ordersTable.Clear();
                    da.Fill(ordersTable);

                    dataGridView1.DataSource = ordersTable;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // 🔹 теперь ID виден
                    if (dataGridView1.Columns.Contains("ID"))
                    {
                        dataGridView1.Columns["ID"].Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказов:\n" + ex.Message);
            }
        }

      

        private void button3_Click(object sender, EventArgs e)
        {
            maindir f = new maindir();
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24)
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7);
            }

            f.Opacity = 0;
            f.Show();

            for (double opacity = 0; opacity <= 1.0; opacity += 0.24)
            {
                f.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7);
            }

            allowClose = true;
            this.Hide();
        }

        // ================== ЗАГРУЗКА ЗАКАЗОВ ==================
        private void LoadOrders()
        {
            ApplyFilters();
        }

        private static void SetMoneyFormatSafe(Excel.Worksheet ws, int colIndex, int headerRow, int dataRowCount)
        {
            if (ws == null) throw new ArgumentNullException(nameof(ws));
            if (colIndex <= 0 || dataRowCount <= 0) return;

            int firstDataRow = headerRow + 1;
            int lastDataRow = headerRow + dataRowCount;

            Excel.Range rng = ws.Range[ws.Cells[firstDataRow, colIndex], ws.Cells[lastDataRow, colIndex]];

            // 1) Сначала делаем ячейки числовыми (если вдруг пришли строки)
            rng.Value2 = rng.Value2;

            // 2) Самый совместимый формат (без "₽")
            rng.NumberFormat = "#,##0.00";
        }


        // ================== EXCEL ==================
        private void button1_Click(object sender, EventArgs e)
        {
            if (ordersTable.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для отчёта");
                return;
            }

            int ordersCount = ordersTable.Rows.Count;
            decimal totalSum = 0;
            decimal discountSum = 0;
            decimal finalSum = 0;

            foreach (DataRow row in ordersTable.Rows)
            {
                totalSum += Convert.ToDecimal(row["Сумма без скидки"]);
                discountSum += Convert.ToDecimal(row["Скидка ₽"]);
                finalSum += Convert.ToDecimal(row["Итого"]);
            }

            // 🔹 ФИО сотрудника
            string employeeName = "Неизвестно";
            using (MySqlConnection conn = DbConfig.GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT full_name FROM users WHERE login = @login", conn);
                cmd.Parameters.AddWithValue("@login", Session.CurrentLogin);

                object res = cmd.ExecuteScalar();
                if (res != null)
                    employeeName = res.ToString();
            }

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook wb = xlApp.Workbooks.Add();
            Excel.Worksheet ws = wb.ActiveSheet;
            xlApp.Visible = true;

            // ===== ЗАГОЛОВОК =====
            ws.Range["A1", "H1"].Merge(); // Теперь 8 колонок (ID добавился)
            ws.Cells[1, 1] = "ОТЧЁТ ПО ЗАКАЗАМ";
            ws.Cells[1, 1].Font.Bold = true;
            ws.Cells[1, 1].Font.Size = 16;
            ws.Cells[1, 1].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

            ws.Cells[3, 1] = "Дата создания:";
            ws.Cells[3, 2] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            ws.Cells[4, 1] = "Сотрудник:";
            ws.Cells[4, 2] = employeeName;

            ws.Cells[5, 1] = "Период:";
            ws.Cells[5, 2] = $"с {dateTimePicker1.Value:dd.MM.yyyy} по {dateTimePicker2.Value:dd.MM.yyyy}";

            // Добавляем информацию о фильтрах
            int currentRow = 6;
            if (comboBox1.SelectedIndex > 0)
            {
                ws.Cells[currentRow, 1] = "Статус:";
                ws.Cells[currentRow, 2] = comboBox1.SelectedItem.ToString();
                currentRow++;
            }

            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                ws.Cells[currentRow, 1] = "ID заказа:";
                ws.Cells[currentRow, 2] = textBox1.Text;
                currentRow++;
            }

            // ===== ТАБЛИЦА =====
            int startRow = currentRow + 2;

            for (int i = 0; i < ordersTable.Columns.Count; i++)
            {
                ws.Cells[startRow, i + 1] = ordersTable.Columns[i].ColumnName;
                ws.Cells[startRow, i + 1].Font.Bold = true;
                ws.Cells[startRow, i + 1].Interior.Color = 0xD9D9D9;
            }

            for (int r = 0; r < ordersTable.Rows.Count; r++)
            {
                for (int c = 0; c < ordersTable.Columns.Count; c++)
                {
                    ws.Cells[startRow + r + 1, c + 1] = ordersTable.Rows[r][c];
                }
            }

            ws.Columns.AutoFit();

            // Форматируем денежные колонки
            int totalColIndex = -1;
            int discountColIndex = -1;
            int finalTotalColIndex = -1;

            for (int i = 0; i < ordersTable.Columns.Count; i++)
            {
                string colName = ordersTable.Columns[i].ColumnName;
                if (colName == "Сумма без скидки") totalColIndex = i + 1;
                else if (colName == "Скидка ₽") discountColIndex = i + 1;
                else if (colName == "Итого") finalTotalColIndex = i + 1;
            }

            SetMoneyFormatSafe(ws, totalColIndex, startRow, ordersTable.Rows.Count);
            SetMoneyFormatSafe(ws, discountColIndex, startRow, ordersTable.Rows.Count);
            SetMoneyFormatSafe(ws, finalTotalColIndex, startRow, ordersTable.Rows.Count);

            // ===== ИТОГИ =====
            int итогRow = startRow + ordersTable.Rows.Count + 3;

            ws.Cells[итогRow, 1] = "Количество заказов:";
            ws.Cells[итогRow, 2] = ordersCount;

            ws.Cells[итогRow + 1, 1] = "Сумма без скидки:";
            ws.Cells[итогRow + 1, 2] = totalSum;

            ws.Cells[итогRow + 2, 1] = "Скидка:";
            ws.Cells[итогRow + 2, 2] = discountSum;

            ws.Cells[итогRow + 3, 1] = "ИТОГО:";
            ws.Cells[итогRow + 3, 2] = finalSum;

            Excel.Range итог = ws.Range[ws.Cells[итогRow + 1, 2], ws.Cells[итогRow + 3, 2]];
            итог.NumberFormat = "#,##0.00";
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
        private void SetupDataGridView()
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // выделяем всю строку
            dataGridView1.MultiSelect = false; // только одна строка
            dataGridView1.ReadOnly = true; // запрет редактирования
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.RowHeadersVisible = false; // скрыть левый столбец с номерами
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите заказ");
                return;
            }

            if (dataGridView1.SelectedRows[0].Cells["ID"].Value != null)
            {
                long orderId = Convert.ToInt64(dataGridView1.SelectedRows[0].Cells["ID"].Value);

                podrobno f = new podrobno(orderId);

                // ===== Сохраняем фильтры перед закрытием =====
                DirectorState.DateFrom = dateTimePicker1.Value;
                DirectorState.DateTo = dateTimePicker2.Value;
                DirectorState.OrderId = textBox1.Text;
                DirectorState.StatusIndex = comboBox1.SelectedIndex;
                DirectorState.SortIndex = comboBox2.SelectedIndex;

                allowClose = true;
                f.Show();
                this.Close(); // теперь закрываем
            }
            else
            {
                MessageBox.Show("Не удалось получить ID выбранного заказа");
            }
        }

        // ================== КНОПКА СБРОСА ФИЛЬТРОВ ==================
        private void button4_Click_1(object sender, EventArgs e)
        {
            // Сброс всех фильтров
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            textBox1.Text = "";

            // Возврат к стандартным датам
            dateTimePicker1.Value = DateTime.Today;
            dateTimePicker2.Value = DateTime.Today;

            ApplyFilters();
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            int cursor = textBox1.SelectionStart;

            // Оставляем только цифры
            string cleaned = new string(textBox1.Text.Where(char.IsDigit).ToArray());
            if (textBox1.Text != cleaned)
            {
                textBox1.Text = cleaned;
                textBox1.SelectionStart = Math.Min(cursor, textBox1.Text.Length);
            }

            // Вызываем ApplyFilters вместо прямого изменения RowFilter
            ApplyFilters();
        }
    }
    
}