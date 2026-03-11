using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class ordermanag : BaseForm
    {
        private DataTable menuTable;
        private DataTable cartTable;
        private long currentOrderId
        {
            get => Class2.CurrentOrderId;
            set => Class2.CurrentOrderId = value;
        }
        public ordermanag()
        {
            InitializeComponent();

            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker1.Enabled = false;
            textBox2.KeyPress += textBox2_KeyPress;
            this.Activated += ordermanag_Activated;
            dataGridView2.SelectionChanged += dataGridView2_SelectionChanged;
            dataGridView2.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#CCE6FF");
            dataGridView2.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#CCE6FF");
            button6.Visible = false;
            dataGridView1.CellClick += dataGridView1_CellClick;
            button4.Enabled = false;
            numericUpDown1.Minimum = 1;
            numericUpDown1.Value = 1;

            // ДОБАВЬТЕ ЭТУ СТРОКУ:
            maskedTextBox2.KeyPress += maskedTextBox2_KeyPress;

            InitCart();
            LoadMenu();
            RestoreUiState();
        }
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
                return;

            // Разрешаем пробел
            if (e.KeyChar == ' ')
                return;

            // Проверяем, что символ - русская буква (кириллица)
            bool isRussianLetter = (e.KeyChar >= 'А' && e.KeyChar <= 'я') ||
                                  e.KeyChar == 'Ё' || e.KeyChar == 'ё';

            if (!isRussianLetter)
                e.Handled = true;
        }
        private void maskedTextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем Backspace и Delete
            if (char.IsControl(e.KeyChar))
                return;

            // Запрещаем пробел
            if (e.KeyChar == ' ')
            {
                e.Handled = true;
                return;
            }

            // Разрешаем ТОЛЬКО цифры
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private void InitCart()
        {
            if (Class2.CartTable == null)
            {
                Class2.CartTable = new DataTable();
                Class2.CartTable.Columns.Add("name", typeof(string));
                Class2.CartTable.Columns.Add("price", typeof(decimal));
                Class2.CartTable.Columns.Add("qty", typeof(int));
                Class2.CartTable.Columns.Add("sum", typeof(decimal));
            }

            cartTable = Class2.CartTable;

            dataGridView2.DataSource = cartTable;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.MultiSelect = false;
            dataGridView2.RowTemplate.Height = 50;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            if (dataGridView2.Columns.Contains("name")) dataGridView2.Columns["name"].HeaderText = "Название";
            if (dataGridView2.Columns.Contains("price")) dataGridView2.Columns["price"].HeaderText = "Цена";
            if (dataGridView2.Columns.Contains("qty")) dataGridView2.Columns["qty"].HeaderText = "Количество";
            if (dataGridView2.Columns.Contains("sum")) dataGridView2.Columns["sum"].HeaderText = "Сумма";

            CalculateTotal(); // 🔥 пересчёт при возврате
        }

        private void LoadMenu()
        {
            try
            {
                string query = @"SELECT p.id, p.article, p.name, p.description, p.price, c.name AS category_name, c.is_deleted AS category_deleted, p.image_path, p.is_deleted FROM products p LEFT JOIN categories c ON p.category_id = c.id WHERE p.is_deleted = 0";


                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    menuTable = new DataTable();
                    da.Fill(menuTable);

                    dataGridView1.DataSource = menuTable;
                    SetupMenuGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки меню: " + ex.Message);
            }
        }

        private void SetupMenuGrid()
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.RowTemplate.Height = 50;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Скрываем ID
            if (dataGridView1.Columns.Contains("id")) dataGridView1.Columns["id"].Visible = false;

            // Русские шапки
            if (dataGridView1.Columns.Contains("article")) dataGridView1.Columns["article"].Visible = false;
            if (dataGridView1.Columns.Contains("name")) dataGridView1.Columns["name"].HeaderText = "Название";
            if (dataGridView1.Columns.Contains("description")) dataGridView1.Columns["description"].HeaderText = "Описание";
            if (dataGridView1.Columns.Contains("price")) dataGridView1.Columns["price"].HeaderText = "Цена";
            if (dataGridView1.Columns.Contains("category_name")) dataGridView1.Columns["category_name"].HeaderText = "Категория";
            if (dataGridView1.Columns.Contains("category_deleted"))
                dataGridView1.Columns["category_deleted"].Visible = false;
            if (dataGridView1.Columns.Contains("is_deleted"))
                dataGridView1.Columns["is_deleted"].Visible = false;
            if (dataGridView1.Columns.Contains("image_path"))
                dataGridView1.Columns["image_path"].Visible = false;
        }

        private void AddSelectedToCart()
        {
            if (dataGridView1.SelectedRows.Count == 0) return;

            DataGridViewRow row = dataGridView1.SelectedRows[0];
            string productName = row.Cells["name"].Value.ToString();
            decimal productPrice = Convert.ToDecimal(row.Cells["price"].Value);
            int quantity = (int)numericUpDown1.Value;

            // Проверка на повторное добавление
            DataRow existing = cartTable.Rows.Cast<DataRow>().FirstOrDefault(r => r["name"].ToString() == productName);
            if (existing != null)
            {
                MessageBox.Show("Этот товар уже добавлен в корзину");
                return;
            }

            cartTable.Rows.Add(productName, productPrice, quantity, productPrice * quantity);
            CalculateTotal();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            textBox1.Text = row.Cells["name"].Value?.ToString() ?? "";
            numericUpDown1.Value = numericUpDown1.Minimum;

        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                button6.Visible = true;

                DataGridViewRow selectedRow = dataGridView2.SelectedRows[0];
                numericUpDown1.Value = Convert.ToInt32(selectedRow.Cells["qty"].Value);
            }
            else
            {
                button6.Visible = false;
                numericUpDown1.Value = numericUpDown1.Minimum;
            }
        }

        private void CalculateTotal()
        {
            decimal total = cartTable.Rows.Cast<DataRow>().Sum(r => (decimal)r["sum"]);
            decimal discount = total >= 3500 ? total * 0.15m : 0;
            decimal final = total - discount;

            label9.Text = $"Сумма: {total} ₽";
            label10.Text = $"Скидка: {discount} ₽";
            label12.Text = $"Итого: {final} ₽";
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count == 0) return;
            cartTable.Rows.RemoveAt(dataGridView2.SelectedRows[0].Index);
            CalculateTotal();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count == 0) return;

            DataGridViewRow selectedRow = dataGridView2.SelectedRows[0];
            DataRow row = cartTable.Rows[selectedRow.Index];

            int newQty = (int)numericUpDown1.Value;
            row["qty"] = newQty;
            row["sum"] = Convert.ToDecimal(row["price"]) * newQty;

            CalculateTotal();
            numericUpDown1.Value = numericUpDown1.Minimum; // обычно 1
            dataGridView2.ClearSelection();
        }
        // ordermanag.cs — обновите методы сохранения/восстановления (внутри класса ordermanag)

        // ordermanag.cs — в SaveUiState / RestoreUiState добавьте сохранение button3
        private void SaveUiState()
        {
            Class2.Button1Enabled = button1.Enabled;
            Class2.Button2Enabled = button2.Enabled;
            Class2.Button3Enabled = button3.Enabled;   // <-- добавили
            Class2.Button4Enabled = button4.Enabled;
            Class2.Button6Enabled = button6.Enabled;
            Class2.Button6Visible = button6.Visible;
            Class2.NumericUpDown1Enabled = numericUpDown1.Enabled;

            Class2.CustomerName = textBox2.Text;
            Class2.CustomerPhone = maskedTextBox2.Text;
            Class2.CustomerAddress = textBox4.Text;

            Class2.CustomerNameEnabled = textBox2.Enabled;
            Class2.CustomerPhoneEnabled = maskedTextBox2.Enabled;
            Class2.CustomerAddressEnabled = textBox4.Enabled;
        }

        private void RestoreUiState()
        {
            button1.Enabled = Class2.Button1Enabled;
            button2.Enabled = Class2.Button2Enabled;
            button3.Enabled = Class2.Button3Enabled;   // <-- добавили
            button4.Enabled = Class2.Button4Enabled;
            button6.Enabled = Class2.Button6Enabled;
            button6.Visible = Class2.Button6Visible;
            numericUpDown1.Enabled = Class2.NumericUpDown1Enabled;

            textBox2.Text = Class2.CustomerName;
            maskedTextBox2.Text = Class2.CustomerPhone;
            textBox4.Text = Class2.CustomerAddress;

            textBox2.Enabled = Class2.CustomerNameEnabled;
            maskedTextBox2.Enabled = Class2.CustomerPhoneEnabled;
            textBox4.Enabled = Class2.CustomerAddressEnabled;

            // железно блокируем button3, если заказ уже оформлен
            if (Class2.OrderCreated || Class2.CurrentOrderId != 0)
                button3.Enabled = false;
        }


        private void button3_Click(object sender, EventArgs e)
        {
            // Проверка заполненности данных клиента
            if (string.IsNullOrWhiteSpace(textBox2.Text) ||
                !maskedTextBox2.MaskCompleted ||
                string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("Заполните все данные клиента");
                return;
            }

            // Проверка, что корзина не пустая
            if (cartTable.Rows.Count == 0)
            {
                MessageBox.Show("Корзина пуста");
                return;
            }

            try
            {
                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        string insertOrder = @" INSERT INTO orders (customer_name, phone, address, total, discount, final_total, order_date, status_id) VALUES (@name, @phone, @address, @total, @discount, @final, @date, @status); SELECT LAST_INSERT_ID();";

                        decimal total = cartTable.Rows.Cast<DataRow>().Sum(r => (decimal)r["sum"]);
                        decimal discount = total >= 3500 ? total * 0.15m : 0;
                        decimal finalTotal = total - discount;

                        using (MySqlCommand cmdOrder = new MySqlCommand(insertOrder, conn, transaction))
                        {
                            cmdOrder.Parameters.AddWithValue("@name", textBox2.Text);
                            cmdOrder.Parameters.AddWithValue("@phone", maskedTextBox2.Text);
                            cmdOrder.Parameters.AddWithValue("@address", textBox4.Text);
                            cmdOrder.Parameters.AddWithValue("@total", total);
                            cmdOrder.Parameters.AddWithValue("@discount", discount);
                            cmdOrder.Parameters.AddWithValue("@final", finalTotal);
                            cmdOrder.Parameters.AddWithValue("@date", dateTimePicker1.Value);
                            cmdOrder.Parameters.AddWithValue("@status", 2);

                            currentOrderId = Convert.ToInt64(cmdOrder.ExecuteScalar());
                        }

                        foreach (DataRow row in cartTable.Rows)
                        {
                            string insertItem = @"INSERT INTO order_items (order_id, product_name, price, quantity, sum) VALUES (@order_id, @name, @price, @qty, @sum);";

                            using (MySqlCommand cmdItem = new MySqlCommand(insertItem, conn, transaction))
                            {
                                cmdItem.Parameters.AddWithValue("@order_id", currentOrderId);
                                cmdItem.Parameters.AddWithValue("@name", row["name"]);
                                cmdItem.Parameters.AddWithValue("@price", row["price"]);
                                cmdItem.Parameters.AddWithValue("@qty", row["qty"]);
                                cmdItem.Parameters.AddWithValue("@sum", row["sum"]);
                                cmdItem.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                }

                button4.Enabled = true;
                button1.Enabled = false;
                button2.Enabled = false;
                Class2.OrderCreated = true;   // <-- важно
                button3.Enabled = false; 
                button6.Enabled = false;
                numericUpDown1.Enabled = false;
                textBox2.Enabled = false;          // <-- блокируем поля клиента
                maskedTextBox2.Enabled = false;
                textBox4.Enabled = false;
                SaveUiState();

                MessageBox.Show("Заказ оформлен");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении заказа: " + ex.Message);
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            if (currentOrderId == 0)
            {
                MessageBox.Show("Заказ ещё не оформлен");
                return;
            }
            
            podrobno f = new podrobno(currentOrderId);
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24) // было 0.05
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }

            f.Opacity = 0;
            f.Show();
            // Быстрое появление - 0.4 секунды
            for (double opacity = 0; opacity <= 1.0; opacity += 0.24) // было 0.05
            {
                f.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }
            allowClose = true;
            Class2.ResetOrderState();
            this.Hide();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            mainmanag managForm = new mainmanag();
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24) // было 0.05
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }

            managForm.Opacity = 0;
            managForm.Show();
            // Быстрое появление - 0.4 секунды
            for (double opacity = 0; opacity <= 1.0; opacity += 0.24) // было 0.05
            {
                managForm.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }
            allowClose = true;
            Class2.ResetOrderState();
            this.Hide();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int pos = tb.SelectionStart;

            // Проходим по каждому символу и делаем заглавной букву после пробела
            char[] chars = tb.Text.ToCharArray();
            bool newWord = true;

            for (int i = 0; i < chars.Length; i++)
            {
                if (char.IsLetter(chars[i]))
                {
                    if (newWord)
                    {
                        chars[i] = char.ToUpper(chars[i]);
                        newWord = false;
                    }
                    else
                    {
                        chars[i] = char.ToLower(chars[i]);
                    }
                }
                else
                {
                    newWord = true; // следующий символ — начало нового слова
                }
            }

            tb.Text = new string(chars);
            tb.SelectionStart = Math.Min(pos, tb.Text.Length);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (menuTable == null) return;

            dataGridView2.ClearSelection();
            button6.Visible = false;
            numericUpDown1.Value = numericUpDown1.Minimum;

            string searchText = textBox1.Text.Trim().Replace("'", "''"); // экранируем апострофы

            // Создаем фильтр для колонки "name" (название)
            DataView dv = menuTable.DefaultView;
            dv.RowFilter = $"name LIKE '%{searchText}%'";

            dataGridView1.DataSource = dv;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            discount skidki = new discount();
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24) // было 0.05
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }

            skidki.Opacity = 0;
            skidki.Show();
            // Быстрое появление - 0.4 секунды
            for (double opacity = 0; opacity <= 1.0; opacity += 0.24) // было 0.05
            {
                skidki.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }
            allowClose = true;
            SaveUiState();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddSelectedToCart();
            textBox1.Clear(); // ✅ очистка поиска
            numericUpDown1.Value = numericUpDown1.Minimum; // обычно 1
            dataGridView1.ClearSelection();
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
        private void ordermanag_Activated(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            dataGridView2.ClearSelection();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            int cursorPos = tb.SelectionStart;

            // 1. Убираем всё кроме русских букв, цифр, пробелов и точек
            string text = Regex.Replace(tb.Text, @"[^А-Яа-я0-9.\s]", "");

            // 2. Делаем первую букву заглавной и после точки тоже
            char[] chars = text.ToCharArray();
            bool makeUpper = true;

            for (int i = 0; i < chars.Length; i++)
            {
                if (char.IsLetter(chars[i]) && Regex.IsMatch(chars[i].ToString(), @"[А-Яа-я]"))
                {
                    if (makeUpper)
                    {
                        chars[i] = char.ToUpper(chars[i]);
                        makeUpper = false;
                    }
                    else
                    {
                        chars[i] = char.ToLower(chars[i]);
                    }
                }

                if (chars[i] == '.')
                    makeUpper = true;
            }

            string result = new string(chars);

            // 3. Обновляем текст ТОЛЬКО если изменился
            if (tb.Text != result)
            {
                tb.Text = result;
                tb.SelectionStart = Math.Min(cursorPos, tb.Text.Length);
            }
        }
    }
}
