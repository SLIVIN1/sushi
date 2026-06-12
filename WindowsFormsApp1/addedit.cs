using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class addedit : Form
    {
        private string appFolderPath;
        private int currentProductId = 0;
        private string currentImagePath = "";
        private bool isEditMode = false;
        private string selectedImageFilePath = ""; // Путь к выбранному файлу изображения

        // Конструктор для ДОБАВЛЕНИЯ
        public addedit()
        {
            InitializeComponent();
            isEditMode = false;
            currentProductId = 0;
            InitializeForm();
            this.Text = "Добавление товара";
            button1.Text = "Добавить";
            button1.Visible = true;
            button2.Visible = false;
        }

        // Конструктор для РЕДАКТИРОВАНИЯ
        public addedit(int productId)
        {
            InitializeComponent();
            isEditMode = true;
            currentProductId = productId;
            InitializeForm();
            this.Text = "Редактирование товара";
            button1.Visible = false;
            button2.Text = "Изменить";
            button2.Visible = true;

            LoadProductData(productId);
        }

        private void InitializeForm()
        {
            textBox1.KeyPress += textBox1_KeyPress;
            textBox1.TextChanged += textBox1_TextChanged;
            textBox4.KeyPress += textBox4_KeyPress;
            textBox2.TextChanged += textBox2_TextChanged;
            textBox3.TextChanged += textBox3_TextChanged;

            appFolderPath = Path.Combine(Application.StartupPath, "ProductImages");
            if (!Directory.Exists(appFolderPath))
                Directory.CreateDirectory(appFolderPath);

            // Настройка PictureBox
            pictureBox1.BackColor = Color.WhiteSmoke;

            LoadCategories();
        }

        #region === Загрузка данных товара ===

        private void LoadProductData(int productId)
        {
            try
            {
                string query = @"SELECT p.article, p.name, p.description, p.price, 
                                c.name AS category_name, p.image_path
                                FROM products p 
                                LEFT JOIN categories c ON p.category_id = c.id 
                                WHERE p.id = @id";

                string article = "";
                string name = "";
                string description = "";
                string priceStr = "";
                string categoryName = "";
                string imgPath = "";

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", productId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            article = reader["article"]?.ToString() ?? "";
                            name = reader["name"]?.ToString() ?? "";
                            description = reader["description"]?.ToString() ?? "";
                            priceStr = reader["price"]?.ToString() ?? "";
                            categoryName = reader["category_name"]?.ToString() ?? "";
                            imgPath = reader["image_path"]?.ToString() ?? "";
                        }
                        else
                        {
                            MessageBox.Show("Товар не найден", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            this.Close();
                            return;
                        }
                    }
                }

                // Заполняем поля формы
                textBox1.Text = article;
                textBox2.Text = name;
                textBox3.Text = description;
                textBox4.Text = priceStr;

                if (comboBox1.Items.Contains(categoryName))
                    comboBox1.SelectedItem = categoryName;
                else
                    comboBox1.Text = categoryName;

                currentImagePath = imgPath;

                // Загружаем фото в PictureBox
                ShowImageInPictureBox(imgPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных товара: " + ex.Message);
                this.Close();
            }
        }

        #endregion

        #region === Отображение изображения в PictureBox ===

        /// <summary>
        /// Показывает изображение из папки ProductImages по имени файла
        /// </summary>
        private void ShowImageInPictureBox(string imageName)
        {
            // Освобождаем предыдущее изображение
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }

            if (string.IsNullOrEmpty(imageName))
            {
                return;
            }

            string fullPath = GetFullImagePath(imageName);

           
            if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
                return;

            try
            {
                byte[] bytes = File.ReadAllBytes(fullPath);
                MemoryStream ms = new MemoryStream(bytes);
                pictureBox1.Image = Image.FromStream(ms);
            }
            catch (Exception ex)
            {
                pictureBox1.Image = null;
            }
        }


        /// <summary>
        /// Показывает изображение из произвольного пути (при выборе через OpenFileDialog)
        /// </summary>
        private void ShowImageFromFile(string filePath)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;

            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                MemoryStream ms = new MemoryStream(bytes);
                pictureBox1.Image = Image.FromStream(ms);
            }
            catch
            {
                pictureBox1.Image = null;
            }
        }

        #endregion

        #region === Загрузка категорий ===

        private void LoadCategories()
        {
            try
            {
                string query = "SELECT name FROM categories";
                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    comboBox1.Items.Clear();
                    while (reader.Read())
                        comboBox1.Items.Add(reader["name"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message);
            }
        }

        #endregion

        #region === Кнопка "Добавить товар" (button1) ===

        private void button1_Click(object sender, EventArgs e)
        {
            if (!ValidateFields()) return;

            // ❗ Проверка длины артикула
            if (textBox1.Text.Trim().Length < 6)
            {
                MessageBox.Show(
                    "Артикул должен содержать минимум 6 символов",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                textBox1.Focus();
                return;
            }

            int catId;
            try
            {
                catId = GetCategoryId(comboBox1.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                decimal price = decimal.Parse(textBox4.Text);

                if (IsArticleExists(textBox1.Text.Trim()))
                {
                    MessageBox.Show("Товар с таким артикулом уже существует",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox1.Focus();
                    return;
                }

                string imgPathForDb = "";
                if (!string.IsNullOrEmpty(selectedImageFilePath) && File.Exists(selectedImageFilePath))
                {
                    imgPathForDb = SaveImageFile(selectedImageFilePath);
                }

                string query = @"INSERT INTO products 
            (article, name, description, price, category_id, image_path)
            VALUES 
            (@art, @name, @desc, @price, @catId, @img);
            SELECT LAST_INSERT_ID();";

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@art", textBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@name", textBox2.Text.Trim());
                    cmd.Parameters.AddWithValue("@desc", textBox3.Text.Trim());
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@catId", catId);
                    cmd.Parameters.AddWithValue("@img", imgPathForDb);

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                        currentProductId = Convert.ToInt32(result);
                }

                MessageBox.Show("Товар успешно добавлен!", "Успешно",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                MessageBox.Show("Товар с таким артикулом уже существует",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении: " + ex.Message);
            }
        }
        #endregion

        #region === Кнопка "Сохранить изменения" (button2) ===

        private void button2_Click(object sender, EventArgs e)
        {
            if (currentProductId == 0)
            {
                MessageBox.Show("Товар не выбран", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateFields()) return;

            // ❗ Проверка длины артикула
            if (textBox1.Text.Trim().Length < 6)
            {
                MessageBox.Show(
                    "Артикул должен содержать минимум 6 символов",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                textBox1.Focus();
                return;
            }

            try
            {
                if (IsArticleExistsForOtherProduct(textBox1.Text.Trim(), currentProductId))
                {
                    MessageBox.Show("Этот артикул уже используется другим товаром",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox1.Focus();
                    return;
                }

                decimal price = decimal.Parse(textBox4.Text);
                int catId = GetCategoryId(comboBox1.Text);

                string query = @"UPDATE products SET 
            article=@art, name=@name, description=@desc, 
            price=@price, category_id=@catId 
            WHERE id=@id";

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@art", textBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@name", textBox2.Text.Trim());
                    cmd.Parameters.AddWithValue("@desc", textBox3.Text.Trim());
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@catId", catId);
                    cmd.Parameters.AddWithValue("@id", currentProductId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Товар успешно обновлён!", "Успешно",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении: " + ex.Message);
            }
        }
        #endregion

        #region === Кнопка "Добавить изображение" (button4) ===

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Картинки|*.jpg;*.jpeg;*.png;*.bmp";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string originalFileName = Path.GetFileName(dlg.FileName);

                    // Контроль дубликатов по имени файла в БД
                    if (IsImageAlreadyUsed(originalFileName, currentProductId))
                    {
                        MessageBox.Show(
                            $"Изображение \"{originalFileName}\" уже используется другим товаром.\n" +
                            "Каждое изображение может быть привязано только к одному товару.",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Контроль дубликатов по содержимому файла (MD5)
                    string fileHash = GetFileHash(dlg.FileName);
                    if (IsImageHashAlreadyUsed(fileHash, currentProductId))
                    {
                        MessageBox.Show(
                            "Это изображение (с таким же содержимым) уже используется другим товаром.\n" +
                            "Каждое изображение может быть привязано только к одному товару.",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Показываем фото в PictureBox
                    ShowImageFromFile(dlg.FileName);

                    // Запоминаем путь к выбранному файлу
                    selectedImageFilePath = dlg.FileName;

                    // Если режим РЕДАКТИРОВАНИЯ — сразу сохраняем в папку и обновляем БД
                    if (isEditMode && currentProductId > 0)
                    {
                        string savedFileName = SaveImageFile(dlg.FileName);

                        if (!string.IsNullOrEmpty(savedFileName))
                        {
                            currentImagePath = savedFileName;
                            UpdateProductImage(currentProductId, savedFileName);
                            MessageBox.Show("Изображение добавлено", "Успешно",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при сохранении изображения");
                        }
                    }
                    // Если режим ДОБАВЛЕНИЯ — изображение сохранится при нажатии "Добавить"
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки изображения: " + ex.Message);
                }
            }
        }

        #endregion

        #region === Кнопка "Удалить изображение" (button6) ===

        private void button6_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentImagePath) &&
                string.IsNullOrEmpty(selectedImageFilePath) &&
                pictureBox1.Image == null)
            {
                MessageBox.Show("У товара нет изображения", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show("Удалить изображение у товара?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Если режим редактирования — удаляем файл и обновляем БД
                    if (isEditMode && currentProductId > 0)
                    {
                        DeleteProductImageFile(currentProductId);

                        string query = "UPDATE products SET image_path = NULL WHERE id = @id";
                        using (MySqlConnection connection = DbConfig.GetConnection())
                        {
                            connection.Open();
                            MySqlCommand cmd = new MySqlCommand(query, connection);
                            cmd.Parameters.AddWithValue("@id", currentProductId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Очищаем PictureBox
                    if (pictureBox1.Image != null)
                    {
                        pictureBox1.Image.Dispose();
                        pictureBox1.Image = null;
                    }

                    currentImagePath = "";
                    selectedImageFilePath = "";

                    MessageBox.Show("Изображение удалено", "Успешно",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении изображения: " + ex.Message);
                }
            }
        }

        #endregion

        #region === Кнопка "Назад" (button5) ===

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region === Сохранение файла изображения ===

        /// <summary>
        /// Сохраняет файл изображения в папку ProductImages
        /// Возвращает имя сохранённого файла (только имя, без пути)
        /// </summary>
        private string SaveImageFile(string sourceFilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceFilePath) || !File.Exists(sourceFilePath))
                    return "";

                // Если файл уже в папке ProductImages — просто возвращаем имя
                if (Path.GetDirectoryName(sourceFilePath) == appFolderPath)
                    return Path.GetFileName(sourceFilePath);

                // Генерируем уникальное имя
                string extension = Path.GetExtension(sourceFilePath);
                string newFileName = Guid.NewGuid().ToString() + extension;
                string destPath = Path.Combine(appFolderPath, newFileName);

                // Загружаем, ресайзим и сохраняем
                using (Image original = LoadImageWithoutLock(sourceFilePath))
                {
                    if (original == null) return "";

                    using (Image resized = ResizeImageToMaxSize(original, 800, 800))
                    {
                        resized.Save(destPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }

                return newFileName;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Загружает изображение из файла без блокировки
        /// </summary>
        private Image LoadImageWithoutLock(string filePath)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                MemoryStream ms = new MemoryStream(bytes);
                return Image.FromStream(ms);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region === Контроль одинаковых изображений ===

        /// <summary>
        /// Проверяет, используется ли изображение с таким именем другим товаром
        /// </summary>
        private bool IsImageAlreadyUsed(string imageName, int excludeProductId)
        {
            try
            {
                string query = @"SELECT COUNT(*) FROM products 
                                WHERE image_path = @img 
                                AND id != @id 
                                AND image_path IS NOT NULL 
                                AND image_path != ''";

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@img", imageName);
                    cmd.Parameters.AddWithValue("@id", excludeProductId);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// MD5-хеш файла
        /// </summary>
        private string GetFileHash(string filePath)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        /// <summary>
        /// Проверяет, есть ли файл с таким же содержимым у другого товара
        /// </summary>
        private bool IsImageHashAlreadyUsed(string newFileHash, int excludeProductId)
        {
            try
            {
                string query = @"SELECT image_path FROM products 
                                WHERE id != @id 
                                AND image_path IS NOT NULL 
                                AND image_path != ''";

                List<string> existingImages = new List<string>();

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", excludeProductId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string imgPath = reader["image_path"].ToString();
                            if (!string.IsNullOrEmpty(imgPath))
                                existingImages.Add(imgPath);
                        }
                    }
                }

                foreach (string imgName in existingImages)
                {
                    string fullPath = GetFullImagePath(imgName);
                    if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                    {
                        string existingHash = GetFileHash(fullPath);
                        if (existingHash == newFileHash)
                            return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region === Валидация полей ===

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Введите артикул", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Введите название товара", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox2.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("Введите цену", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox4.Focus();
                return false;
            }

            if (!decimal.TryParse(textBox4.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену (больше 0)", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox4.Focus();
                return false;
            }

            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите категорию", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBox1.Focus();
                return false;
            }

            return true;
        }

        #endregion

        #region === Работа с БД ===

        private int GetCategoryId(string name)
        {
            string query = "SELECT id FROM categories WHERE name=@name";

            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", name);

                object result = cmd.ExecuteScalar();
                if (result == null)
                    throw new Exception("Категория не найдена или удалена");

                return Convert.ToInt32(result);
            }
        }

        private bool IsArticleExists(string article)
        {
            string query = "SELECT COUNT(*) FROM products WHERE article = @art ";

            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@art", article);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private bool IsArticleExistsForOtherProduct(string article, int excludeProductId)
        {
            string query = "SELECT COUNT(*) FROM products WHERE article = @art AND id != @id ";

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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении изображения: " + ex.Message);
            }
        }

        #endregion

        #region === Работа с файлами изображений ===

        private void DeleteProductImageFile(int productId)
        {
            try
            {
                string query = "SELECT image_path FROM products WHERE id = @id";
                string imagePath = "";

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", productId);
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        imagePath = result.ToString();
                }

                if (!string.IsNullOrEmpty(imagePath))
                {
                    string fullPath = GetFullImagePath(imagePath);
                    if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                        File.Delete(fullPath);
                }
            }
            catch
            {
                // Игнорируем ошибки при удалении файла
            }
        }

        private string GetFullImagePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";

            // Если это уже полный путь и файл существует
            if (Path.IsPathRooted(path) && File.Exists(path))
                return path;

            // Пробуем найти в папке ProductImages
            string productImagePath = Path.Combine(appFolderPath, Path.GetFileName(path));
            if (File.Exists(productImagePath))
                return productImagePath;

            // Пробуем относительный путь от StartupPath
            string relativePath = Path.Combine(Application.StartupPath, path.TrimStart('/', '\\'));
            if (File.Exists(relativePath))
                return relativePath;

            // ОТЛАДКА

            return "";
        }


        private Image ResizeImageToMaxSize(Image img, int maxWidth, int maxHeight)
        {
            int width = img.Width;
            int height = img.Height;

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

        #endregion

        #region === Обработчики текстовых полей ===

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;
            if (!char.IsLetterOrDigit(e.KeyChar))
                e.Handled = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int cursor = textBox1.SelectionStart;
            string cleaned = new string(textBox1.Text.Where(char.IsDigit).ToArray());

            if (textBox1.Text != cleaned)
            {
                textBox1.Text = cleaned;
                textBox1.SelectionStart = Math.Min(cursor, textBox1.Text.Length);
            }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            if (!char.IsDigit(e.KeyChar) && e.KeyChar != ',' && e.KeyChar != '.')
            {
                e.Handled = true;
                return;
            }

            if (e.KeyChar == '.')
                e.KeyChar = ',';

            if (e.KeyChar == ',' && textBox4.Text.Contains(","))
                e.Handled = true;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            CapitalizeFirstLetter(textBox2);
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            CapitalizeFirstLetter(textBox3);
        }

        private void CapitalizeFirstLetter(TextBox tb)
        {
            if (string.IsNullOrEmpty(tb.Text)) return;

            int cursor = tb.SelectionStart;
            string text = tb.Text;
            tb.Text = char.ToUpper(text[0]) + text.Substring(1);
            tb.SelectionStart = cursor;
        }

        #endregion

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {
            FilterNoEnglish(sender as TextBox);
        }

        private void textBox3_TextChanged_1(object sender, EventArgs e)
        {
            FilterNoEnglish(sender as TextBox);
        }

        private void FilterNoEnglish(TextBox tb)
        {
            if (tb == null || string.IsNullOrEmpty(tb.Text)) return;

            int cursor = tb.SelectionStart;

            string result = new string(tb.Text
                .Where(c => !(c >= 'A' && c <= 'Z') && !(c >= 'a' && c <= 'z'))
                .ToArray());

            if (tb.Text != result)
            {
                tb.Text = result;
                tb.SelectionStart = Math.Min(cursor, tb.Text.Length);
            }
        }

       
    }
}