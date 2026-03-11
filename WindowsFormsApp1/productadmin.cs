using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class productadmin : Form
    {
        private DataTable productsTable;
        private string appFolderPath;
        private int currentProductId = 0;
        private string currentImagePath = ""; 

        public productadmin()
        {
            InitializeComponent();
            textBox1.KeyPress += textBox1_KeyPress;
            textBox1.TextChanged += textBox1_TextChanged;
            textBox4.KeyPress += textBox4_KeyPress;
            textBox2.TextChanged += textBox2_TextChanged;
            this.Activated += productadmin_Activated;
            dataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#B3D9FF");
            textBox3.TextChanged += textBox3_TextChanged;
            appFolderPath = Path.Combine(Application.StartupPath, "ProductImages");
            if (!Directory.Exists(appFolderPath))
            {
                Directory.CreateDirectory(appFolderPath);
            }
            dataGridView1.DataBindingComplete += dataGridView1_DataBindingComplete;
            dataGridView1.CellClick += dataGridView1_CellClick;
            dataGridView1.DataBindingComplete += (sender, e) =>
            {
                LoadImagesToGrid();
            };

            LoadData();
            LoadCategories();
        }

        private void productadmin_Activated(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        // Добавляем флаг is_deleted в загрузку данных
        private void LoadData()
        {
            try
            {
                string query = "SELECT p.id, p.article, p.name, p.description, p.price, c.name AS category_name, c.is_deleted AS category_deleted, p.image_path, p.is_deleted FROM products p LEFT JOIN categories c ON p.category_id = c.id WHERE p.is_deleted = 0"; // Показываем только неудаленные товары

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    MySqlDataAdapter da = new MySqlDataAdapter(query, connection);
                    productsTable = new DataTable();
                    da.Fill(productsTable);

                    dataGridView1.DataSource = productsTable;
                    SetupGridColumns();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                bool productDeleted =
                    row.Cells["is_deleted"] != null &&
                    Convert.ToInt32(row.Cells["is_deleted"].Value) == 1;

                bool categoryDeleted =
                    row.Cells["category_deleted"] != null &&
                    row.Cells["category_deleted"].Value != DBNull.Value &&
                    Convert.ToInt32(row.Cells["category_deleted"].Value) == 1;

                if (categoryDeleted)
                {
                    // 🧂 СВЕТЛО-СЕРЫЙ "СОУС"
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 87);
                    row.DefaultCellStyle.Font = new Font(
                        dataGridView1.Font,
                        FontStyle.Italic
                    );
                }
                else
                {
                    // сброс
                    row.DefaultCellStyle.BackColor = dataGridView1.DefaultCellStyle.BackColor;
                    row.DefaultCellStyle.ForeColor = dataGridView1.DefaultCellStyle.ForeColor;
                    row.DefaultCellStyle.Font = dataGridView1.Font;
                }
            }
        }
        private void SetupGridColumns()
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.RowTemplate.Height = 60;
            if (dataGridView1.Columns.Contains("category_deleted"))
                dataGridView1.Columns["category_deleted"].Visible = false;
            if (dataGridView1.Columns.Contains("id"))
                dataGridView1.Columns["id"].Visible = false;

            if (dataGridView1.Columns.Contains("image_path"))
                dataGridView1.Columns["image_path"].Visible = false;

            if (dataGridView1.Columns.Contains("is_deleted"))
                dataGridView1.Columns["is_deleted"].Visible = false;

            // Заголовки
            if (dataGridView1.Columns.Contains("article"))
                dataGridView1.Columns["article"].HeaderText = "Артикул";
            if (dataGridView1.Columns.Contains("name"))
                dataGridView1.Columns["name"].HeaderText = "Название";
            if (dataGridView1.Columns.Contains("description"))
                dataGridView1.Columns["description"].HeaderText = "Описание";
            if (dataGridView1.Columns.Contains("price"))
                dataGridView1.Columns["price"].HeaderText = "Цена";
            if (dataGridView1.Columns.Contains("category_name"))
                dataGridView1.Columns["category_name"].HeaderText = "Категория";

            // Добавляем колонку для картинок если ее нет
            if (!dataGridView1.Columns.Contains("image_col"))
            {
                DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
                imgCol.Name = "image_col";
                imgCol.HeaderText = "Картинка";
                imgCol.Width = 70;
                imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                dataGridView1.Columns.Add(imgCol);
            }

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // Добавляем проверку уникальности артикула при UPDATE
        private bool IsArticleExistsForOtherProduct(string article, int excludeProductId)
        {
            string query = "SELECT COUNT(*) FROM products WHERE article = @art AND id != @id AND is_deleted = 0";

            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@art", article);
                cmd.Parameters.AddWithValue("@id", excludeProductId);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (currentProductId == 0)
            {
                MessageBox.Show("Выберите товар");
                return;
            }

            try
            {
                // Проверка артикула при обновлении
                if (IsArticleExistsForOtherProduct(textBox1.Text, currentProductId))
                {
                    MessageBox.Show(
                        "Артикул с таким товаром уже используется другим товаром",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    textBox1.Focus();
                    return;
                }
                if (Convert.ToInt32(dataGridView1.CurrentRow.Cells["category_deleted"].Value) == 1)
                {
                    MessageBox.Show(
                        "Нельзя редактировать товар с удалённой категорией",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }
                decimal price = decimal.Parse(textBox4.Text);
                int catId = GetCategoryId(comboBox1.Text);

                string query = "UPDATE products SET article=@art, name=@name, description=@desc, " +
                              "price=@price, category_id=@catId WHERE id=@id";

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@art", textBox1.Text);
                    cmd.Parameters.AddWithValue("@name", textBox2.Text);
                    cmd.Parameters.AddWithValue("@desc", textBox3.Text);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@catId", catId);
                    cmd.Parameters.AddWithValue("@id", currentProductId);
                    cmd.ExecuteNonQuery();
                }

                LoadData();
                MessageBox.Show("Обновлено");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        // Мягкое удаление товара
        private void button3_Click(object sender, EventArgs e)
        {
            if (currentProductId == 0)
            {
                MessageBox.Show("Выберите товар");
                return;
            }

            string productName = textBox2.Text.Trim();

            if (IsProductUsedInOrdersById(currentProductId))
            {
                // Предупреждение о том, что товар используется в заказах
                DialogResult result = MessageBox.Show(
                    "Этот товар используется в заказах. При удалении товар будет скрыт из каталога, " +
                    "но останется в истории заказов.\n\n" +
                    "Вы уверены, что хотите удалить товар?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.No)
                {
                    return;
                }
            }
            else
            {
                if (MessageBox.Show("Удалить товар?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
            }

            try
            {
                // Мягкое удаление - устанавливаем флаг is_deleted = 1
                string query = "UPDATE products SET is_deleted = 1 WHERE id = @id";

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", currentProductId);
                    cmd.ExecuteNonQuery();
                }

                LoadData();
                ClearFields();
                MessageBox.Show("Товар удален", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении товара: " + ex.Message);
            }
        }

        // Метод для удаления файла изображения
        private void DeleteProductImageFile(int productId)
        {
            try
            {
                // Получаем путь к изображению из БД
                string query = "SELECT image_path FROM products WHERE id = @id";
                string imagePath = "";

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", productId);
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        imagePath = result.ToString();
                    }
                }

                // Если путь существует, пытаемся удалить файл
                if (!string.IsNullOrEmpty(imagePath))
                {
                    string fullPath = GetFullImagePath(imagePath);
                    if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки при удалении файла
            }
        }

        // Обновляем метод для сохранения изображения
        private void UpdateProductImage(int productId, string imagePath)
        {
            try
            {
                string query = "UPDATE products SET image_path = @img WHERE id = @id";

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@img", imagePath);
                    cmd.Parameters.AddWithValue("@id", productId);
                    cmd.ExecuteNonQuery();
                }

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении изображения: " + ex.Message);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (currentProductId == 0)
            {
                MessageBox.Show("Сначала выберите товар из таблицы");
                return;
            }

            DialogResult result = MessageBox.Show("Удалить изображение у выбранного товара?",
                "Подтверждение удаления", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Сначала удаляем файл изображения
                    DeleteProductImageFile(currentProductId);

                    // Затем обновляем запись в БД
                    string query = "UPDATE products SET image_path = NULL WHERE id = @id";

                    using (MySqlConnection connection = DbConfig.GetConnection())
                    {
                        connection.Open();
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@id", currentProductId);
                        cmd.ExecuteNonQuery();
                    }

                    // Очищаем PictureBox
                    pictureBox1.Image = null;

                    LoadData();
                    MessageBox.Show("Изображение удалено");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении изображения: " + ex.Message);
                }
            }
        }

        // Добавляем проверку, используется ли товар в заказах (по ID товара)
        private bool IsProductUsedInOrdersById(int productId)
        {
            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(
                    @"SELECT COUNT(*) FROM order_items oi
                      INNER JOIN orders o ON oi.order_id = o.id
                      WHERE oi.product_id = @productId",
                    connection);
                cmd.Parameters.AddWithValue("@productId", productId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // Другие методы остаются без изменений...
        private void LoadImagesToGrid()
        {
            try
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    if (row.Cells["image_path"] != null && row.Cells["image_path"].Value != null)
                    {
                        string path = row.Cells["image_path"].Value.ToString();

                        if (!string.IsNullOrEmpty(path))
                        {
                            string fullPath = GetFullImagePath(path);

                            if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                            {
                                try
                                {
                                    Image img = Image.FromFile(fullPath);
                                    row.Cells["image_col"].Value = ResizeImage(img, 60, 60);
                                }
                                catch
                                {
                                    row.Cells["image_col"].Value = null;
                                }
                            }
                            else
                            {
                                row.Cells["image_col"].Value = null;
                            }
                        }
                        else
                        {
                            row.Cells["image_col"].Value = null;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибку
            }
        }

        private string GetFullImagePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";

            if (Path.IsPathRooted(path) && File.Exists(path))
                return path;

            if (path.StartsWith("/") || path.StartsWith("\\"))
            {
                string fullPath = Application.StartupPath + path.Replace("/", "\\");
                if (File.Exists(fullPath))
                    return fullPath;
            }

            string productImagePath = Path.Combine(appFolderPath, Path.GetFileName(path));
            if (File.Exists(productImagePath))
                return productImagePath;

            return "";
        }

        private Image ResizeImage(Image img, int w, int h)
        {
            Bitmap bmp = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(img, 0, 0, w, h);
            }
            return bmp;
        }

        private void LoadCategories()
        {
            try
            {
                string query = "SELECT name FROM categories WHERE is_deleted = 0";

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    comboBox1.Items.Clear();
                    while (reader.Read())
                    {
                        comboBox1.Items.Add(reader["name"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                currentProductId = Convert.ToInt32(row.Cells["id"].Value);

                textBox1.Text = row.Cells["article"].Value?.ToString() ?? "";
                textBox2.Text = row.Cells["name"].Value?.ToString() ?? "";
                textBox3.Text = row.Cells["description"].Value?.ToString() ?? "";
                textBox4.Text = row.Cells["price"].Value?.ToString() ?? "";
                comboBox1.Text = row.Cells["category_name"].Value?.ToString() ?? "";

                string path = row.Cells["image_path"].Value?.ToString() ?? "";
                currentImagePath = path; // <-- добавляем сюда
                string fullPath = GetFullImagePath(path);
                if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                {
                    pictureBox1.Image = Image.FromFile(fullPath);
                }
                else
                {
                    pictureBox1.Image = null;
                }
                bool categoryDeleted = row.Cells["category_deleted"].Value != DBNull.Value & Convert.ToInt32(row.Cells["category_deleted"].Value) == 1;

                if (categoryDeleted)
                {
                    MessageBox.Show(
                        "Категория этого товара удалена. Товар доступен только для просмотра.",
                        "Внимание",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int catId;

            try
            {
                catId = GetCategoryId(comboBox1.Text);
            }
            catch
            {
                MessageBox.Show(
                    "Заполните все поля",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                    string.IsNullOrWhiteSpace(textBox2.Text) ||
                    string.IsNullOrWhiteSpace(textBox4.Text) ||
                    comboBox1.SelectedIndex == -1)
                {
                    MessageBox.Show("Заполните все поля");
                    return;
                }

                if (!decimal.TryParse(textBox4.Text, out decimal price) || price <= 0)
                {
                    MessageBox.Show("Неправильная цена");
                    return;
                }

                if (IsArticleExists(textBox1.Text))
                {
                    MessageBox.Show(
                        "Артикул с таким товаром уже используется",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    textBox1.Focus();
                    return;
                }

                string imgPath = "";

                string query = @"INSERT INTO products 
            (article, name, description, price, category_id, image_path, is_deleted)
            VALUES 
            (@art, @name, @desc, @price, @catId, @img, 0)";

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@art", textBox1.Text);
                    cmd.Parameters.AddWithValue("@name", textBox2.Text);
                    cmd.Parameters.AddWithValue("@desc", textBox3.Text);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@catId", catId);
                    cmd.Parameters.AddWithValue("@img", imgPath);
                    cmd.ExecuteNonQuery();
                }

                LoadData();
                ClearFields();
                MessageBox.Show("Добавлено");
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                MessageBox.Show(
                    "Артикул с таким товаром уже используется",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (currentProductId == 0)
            {
                MessageBox.Show("Сначала выберите товар из таблицы");
                return;
            }

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Картинки|*.jpg;*.jpeg;*.png;*.bmp";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pictureBox1.Image = Image.FromFile(dlg.FileName);

                    // Проверяем, нужно ли копировать
                    string savedImagePath;
                    if (Path.GetDirectoryName(dlg.FileName) == appFolderPath)
                    {
                        savedImagePath = dlg.FileName; // уже в папке, копировать не надо
                    }
                    else
                    {
                        savedImagePath = SaveImage();
                    }

                    if (!string.IsNullOrEmpty(savedImagePath))
                    {
                        UpdateProductImage(currentProductId, Path.GetFileName(savedImagePath));
                        currentImagePath = Path.GetFileName(savedImagePath);
                        MessageBox.Show("Изображение добавлено");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при сохранении изображения");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки: " + ex.Message);
                }
            }
        }

        private Image ResizeImageToMaxSize(Image img, int maxWidth, int maxHeight)
        {
            int width = img.Width;
            int height = img.Height;

            // Если размер меньше максимального, не изменяем
            if (width <= maxWidth && height <= maxHeight)
                return new Bitmap(img);

            double ratioX = (double)maxWidth / width;
            double ratioY = (double)maxHeight / height;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(width * ratio);
            int newHeight = (int)(height * ratio);

            Bitmap newImg = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(newImg))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, newWidth, newHeight);
            }
            return newImg;
        }

        private string SaveImage()
        {
            try
            {
                if (pictureBox1.Image == null) return "";

                // Генерируем уникальное имя
                string name = Guid.NewGuid().ToString() + ".jpg";
                string path = Path.Combine(appFolderPath, name);

                // Сжимаем изображение до максимального размера
                using (Image resized = ResizeImageToMaxSize(pictureBox1.Image, 800, 800))
                {
                    resized.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                }

                // ВАЖНО: сохраняем относительное имя (как вы и кладёте в БД)
                currentImagePath = name;
                return path;
            }
            catch
            {
                return "";
            }
        }

        private int GetCategoryId(string name)
        {
            string query = "SELECT id FROM categories WHERE name=@name AND is_deleted = 0";

            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", name);

                object result = cmd.ExecuteScalar();

                if (result == null)
                    throw new Exception("Нельзя использовать скрытую категорию");

                return Convert.ToInt32(result);
            }
        }

        private bool IsArticleExists(string article)
        {
            string query = "SELECT COUNT(*) FROM products WHERE article = @art AND is_deleted = 0";

            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@art", article);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private void ClearFields()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            comboBox1.Text = "";
            pictureBox1.Image = null;
            currentProductId = 0;
        }

        private void CapitalizeFirstLetter(TextBox tb)
        {
            if (string.IsNullOrEmpty(tb.Text)) return;

            int cursor = tb.SelectionStart;
            string text = tb.Text;

            tb.Text = char.ToUpper(text[0]) + text.Substring(1);
            tb.SelectionStart = cursor;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
                return;

            if (!char.IsLetterOrDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int cursor = textBox1.SelectionStart;
            // Оставляем только цифры
            string cleaned = new string(textBox1.Text.Where(char.IsDigit).ToArray());

            if (textBox1.Text != cleaned)
            {
                textBox1.Text = cleaned;
                textBox1.SelectionStart = Math.Min(cursor, textBox1.Text.Length);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            CapitalizeFirstLetter(textBox2);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            CapitalizeFirstLetter(textBox3);
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
                return;

            // Разрешаем цифры и запятую/точку для десятичных значений
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != ',' && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // Заменяем точку на запятую
            if (e.KeyChar == '.')
            {
                e.KeyChar = ',';
            }

            // Проверяем, что запятая только одна
            if (e.KeyChar == ',' && textBox4.Text.Contains(","))
            {
                e.Handled = true;
            }
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