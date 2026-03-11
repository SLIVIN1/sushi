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
    public partial class productadmin : BaseForm
    {
        private DataTable productsTable;
        private DataTable filteredTable; // Для хранения отфильтрованных данных
        private string appFolderPath;
        private int currentProductId = 0;
        private string currentImagePath = "";

        // Поля для пагинации
        private int currentPage = 1;
        private int pageSize = 20;
        private int totalRecords = 0;
        private int totalPages = 1;
        private string currentFilter = "";
        private string currentSortColumn = "";
        private bool sortAscending = true;

        public productadmin()
        {
            InitializeComponent();

            // Инициализация пагинации
            InitializePagination();

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
            dataGridView1.ColumnHeaderMouseClick += DataGridView1_ColumnHeaderMouseClick; // Для сортировки
            dataGridView1.DataBindingComplete += (sender, e) =>
            {
                LoadImagesToGrid();
            };

            LoadCategories();
            LoadData(); // Загружаем все данные
        }

        private void InitializePagination()
        {
           

            // Подписка на события кнопок
            if (buttonPrev != null)
                buttonPrev.Click += ButtonPrev_Click;

            if (buttonNext != null)
                buttonNext.Click += ButtonNext_Click;
        }

        // Обработчик сортировки по колонкам
        private void DataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;

            // Исключаем колонки, по которым не нужно сортировать
            if (columnName == "image_col" || columnName == "id" || columnName == "image_path" ||
                columnName == "is_deleted" || columnName == "category_deleted")
                return;

            // Определяем направление сортировки
            if (currentSortColumn == columnName)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                currentSortColumn = columnName;
                sortAscending = true;
            }

            // Обновляем данные с сортировкой
            ApplyFilterAndSort();
        }

        private void productadmin_Activated(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        // Загрузка всех данных с учетом фильтрации и сортировки
        private void LoadData()
        {
            try
            {
                string query = @"SELECT p.id, p.article, p.name, p.description, p.price, 
                                c.name AS category_name, c.is_deleted AS category_deleted, 
                                p.image_path, p.is_deleted 
                                FROM products p 
                                LEFT JOIN categories c ON p.category_id = c.id 
                                WHERE p.is_deleted = 0";

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    MySqlDataAdapter da = new MySqlDataAdapter(query, connection);
                    productsTable = new DataTable();
                    da.Fill(productsTable);

                    // Подсчет общего количества записей
                    totalRecords = productsTable.Rows.Count;

                    // Применяем фильтрацию и сортировку
                    ApplyFilterAndSort();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        // Применение фильтрации и сортировки
        private void ApplyFilterAndSort()
        {
            if (productsTable == null) return;

            // Создаем копию для фильтрации
            DataView view = new DataView(productsTable);

            // Применяем фильтр по поиску
            if (!string.IsNullOrWhiteSpace(currentFilter))
            {
                string filterExpression = string.Format("article LIKE '%{0}%' OR name LIKE '%{0}%' OR description LIKE '%{0}%'",
                    currentFilter.Replace("'", "''"));
                view.RowFilter = filterExpression;
            }

            // Применяем сортировку
            if (!string.IsNullOrWhiteSpace(currentSortColumn))
            {
                string sortExpression = currentSortColumn + (sortAscending ? " ASC" : " DESC");
                view.Sort = sortExpression;
            }

            // Получаем отфильтрованную таблицу
            filteredTable = view.ToTable();

            // Обновляем пагинацию
            UpdatePagination();
        }

        // Обновление пагинации и отображение текущей страницы
        private void UpdatePagination()
        {
            if (filteredTable == null) return;

            totalRecords = filteredTable.Rows.Count;
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            if (totalPages == 0) totalPages = 1;

            // Корректировка текущей страницы
            if (currentPage > totalPages)
                currentPage = totalPages;
            if (currentPage < 1)
                currentPage = 1;

            // Получаем данные для текущей страницы
            int startIndex = (currentPage - 1) * pageSize;
            int endIndex = Math.Min(startIndex + pageSize - 1, totalRecords - 1);

            DataTable pageTable = new DataTable();

            if (totalRecords > 0)
            {
                // Копируем структуру
                pageTable = filteredTable.Clone();

                // Добавляем строки текущей страницы
                for (int i = startIndex; i <= endIndex; i++)
                {
                    pageTable.ImportRow(filteredTable.Rows[i]);
                }
            }

            // Отображаем данные
            dataGridView1.DataSource = pageTable;
            SetupGridColumns();

            // Обновляем информацию о записях
            int displayedRecords = pageTable.Rows.Count;
            labelPageInfo.Text = $"{displayedRecords} из {totalRecords}";

            // Обновляем кнопки пагинации
            UpdatePaginationControls();
        }

        // Обновление элементов управления пагинацией
        private void UpdatePaginationControls()
        {
            // Обновляем состояние кнопок
            if (buttonPrev != null)
                buttonPrev.Enabled = currentPage > 1;

            if (buttonNext != null)
                buttonNext.Enabled = currentPage < totalPages;

            // Создаем кнопки для страниц
            if (flowLayoutPanelPages != null)
            {
                flowLayoutPanelPages.Controls.Clear();

                // Показываем не более 5 кнопок страниц для компактности
                int startPage = Math.Max(1, currentPage - 2);
                int endPage = Math.Min(totalPages, startPage + 4);

                if (endPage - startPage < 4 && startPage > 1)
                {
                    startPage = Math.Max(1, endPage - 4);
                }

                // Кнопка "Первая страница"
                if (startPage > 1)
                {
                    Button firstButton = CreatePageButton(1);
                    flowLayoutPanelPages.Controls.Add(firstButton);

                    if (startPage > 2)
                    {
                        Label dotsLabel = new Label();
                        dotsLabel.Text = "...";
                        dotsLabel.AutoSize = true;
                        dotsLabel.TextAlign = ContentAlignment.MiddleCenter;
                        dotsLabel.Padding = new Padding(5);
                        flowLayoutPanelPages.Controls.Add(dotsLabel);
                    }
                }

                // Кнопки страниц
                for (int i = startPage; i <= endPage; i++)
                {
                    Button pageButton = CreatePageButton(i);
                    flowLayoutPanelPages.Controls.Add(pageButton);
                }

                // Кнопка "Последняя страница"
                if (endPage < totalPages)
                {
                    if (endPage < totalPages - 1)
                    {
                        Label dotsLabel = new Label();
                        dotsLabel.Text = "...";
                        dotsLabel.AutoSize = true;
                        dotsLabel.TextAlign = ContentAlignment.MiddleCenter;
                        dotsLabel.Padding = new Padding(5);
                        flowLayoutPanelPages.Controls.Add(dotsLabel);
                    }

                    Button lastButton = CreatePageButton(totalPages);
                    flowLayoutPanelPages.Controls.Add(lastButton);
                }
            }
        }

        // Создание кнопки для страницы
        private Button CreatePageButton(int pageNumber)
        {
            Button btn = new Button();
            btn.Text = pageNumber.ToString();
            btn.Tag = pageNumber;
            btn.Width = 35;
            btn.Height = 30;
            btn.Margin = new Padding(2);

            // Выделяем текущую страницу
            if (pageNumber == currentPage)
            {
                btn.BackColor = Color.LightBlue;
                btn.Font = new Font(btn.Font, FontStyle.Bold);
            }
            else
            {
                btn.BackColor = SystemColors.Control;
                btn.Font = new Font(btn.Font, FontStyle.Regular);
            }

            btn.Click += PageButton_Click;
            return btn;
        }

        // Обработчик клика по кнопке страницы
        private void PageButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag != null)
            {
                int pageNumber = (int)btn.Tag;
                GoToPage(pageNumber);
            }
        }

        // Переход на указанную страницу
        private void GoToPage(int pageNumber)
        {
            if (pageNumber < 1 || pageNumber > totalPages || pageNumber == currentPage)
                return;

            currentPage = pageNumber;
            UpdatePagination();
        }

        // Обработчик кнопки "Назад"
        private void ButtonPrev_Click(object sender, EventArgs e)
        {
            GoToPage(currentPage - 1);
        }

        // Обработчик кнопки "Вперед"
        private void ButtonNext_Click(object sender, EventArgs e)
        {
            GoToPage(currentPage + 1);
        }

       
        // Обработчик поиска
        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            // Предполагаем, что у вас есть TextBox для поиска
            // Если его нет, создайте или переименуйте существующий
            TextBox searchBox = sender as TextBox;
            if (searchBox != null)
            {
                currentFilter = searchBox.Text.Trim();
                currentPage = 1; // Сбрасываем на первую страницу
                ApplyFilterAndSort();
            }
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                bool productDeleted = false;
                bool categoryDeleted = false;

                if (row.Cells["is_deleted"] != null && row.Cells["is_deleted"].Value != null)
                    productDeleted = Convert.ToInt32(row.Cells["is_deleted"].Value) == 1;

                if (row.Cells["category_deleted"] != null && row.Cells["category_deleted"].Value != null &&
                    row.Cells["category_deleted"].Value != DBNull.Value)
                    categoryDeleted = Convert.ToInt32(row.Cells["category_deleted"].Value) == 1;

                if (categoryDeleted)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 87);
                    row.DefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Italic);
                }
                else
                {
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
            {
                dataGridView1.Columns["article"].HeaderText = "Артикул";
                dataGridView1.Columns["article"].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            if (dataGridView1.Columns.Contains("name"))
            {
                dataGridView1.Columns["name"].HeaderText = "Название";
                dataGridView1.Columns["name"].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            if (dataGridView1.Columns.Contains("description"))
            {
                dataGridView1.Columns["description"].HeaderText = "Описание";
                dataGridView1.Columns["description"].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            if (dataGridView1.Columns.Contains("price"))
            {
                dataGridView1.Columns["price"].HeaderText = "Цена";
                dataGridView1.Columns["price"].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            if (dataGridView1.Columns.Contains("category_name"))
            {
                dataGridView1.Columns["category_name"].HeaderText = "Категория";
                dataGridView1.Columns["category_name"].SortMode = DataGridViewColumnSortMode.Programmatic;
            }

            // Добавляем колонку для картинок если ее нет
            if (!dataGridView1.Columns.Contains("image_col"))
            {
                DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
                imgCol.Name = "image_col";
                imgCol.HeaderText = "Картинка";
                imgCol.Width = 70;
                imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                imgCol.SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns.Add(imgCol);
            }

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // Остальные методы (IsArticleExistsForOtherProduct, button2_Click и т.д.) 
        // остаются без изменений, но в них нужно добавить вызов LoadData() 
        // после добавления/обновления/удаления записей

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

        private void button2_Click(object sender, EventArgs e) // Обновление
        {
            if (currentProductId == 0)
            {
                MessageBox.Show("Выберите товар");
                return;
            }

            try
            {
                if (IsArticleExistsForOtherProduct(textBox1.Text, currentProductId))
                {
                    MessageBox.Show("Артикул с таким товаром уже используется другим товаром",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox1.Focus();
                    return;
                }

                // Проверка на удаленную категорию
                DataGridViewRow row = dataGridView1.CurrentRow;
                if (row != null && row.Cells["category_deleted"].Value != null &&
                    row.Cells["category_deleted"].Value != DBNull.Value &&
                    Convert.ToInt32(row.Cells["category_deleted"].Value) == 1)
                {
                    MessageBox.Show("Нельзя редактировать товар с удалённой категорией",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                LoadData(); // Перезагружаем все данные
                MessageBox.Show("Обновлено");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e) // Удаление
        {
            if (currentProductId == 0)
            {
                MessageBox.Show("Выберите товар");
                return;
            }

            if (IsProductUsedInOrdersById(currentProductId))
            {
                DialogResult result = MessageBox.Show(
                    "Этот товар используется в заказах. При удалении товар будет скрыт из каталога, " +
                    "но останется в истории заказов.\n\nВы уверены, что хотите удалить товар?",
                    "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                    return;
            }
            else
            {
                if (MessageBox.Show("Удалить товар?", "Подтверждение",
                    MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            try
            {
                string query = "UPDATE products SET is_deleted = 1 WHERE id = @id";

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", currentProductId);
                    cmd.ExecuteNonQuery();
                }

                LoadData(); // Перезагружаем данные
                ClearFields();
                MessageBox.Show("Товар удален", "Успешно",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении товара: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e) // Добавление
        {
            int catId;

            try
            {
                catId = GetCategoryId(comboBox1.Text);
            }
            catch
            {
                MessageBox.Show("Заполните все поля", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show("Артикул с таким товаром уже используется",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                LoadData(); // Перезагружаем данные
                ClearFields();
                MessageBox.Show("Добавлено");
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                MessageBox.Show("Артикул с таким товаром уже используется",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        // Остальные методы (LoadImagesToGrid, GetFullImagePath, и т.д.) 
        // остаются без изменений

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
                currentImagePath = path;

                string fullPath = GetFullImagePath(path);
                if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                {
                    pictureBox1.Image = Image.FromFile(fullPath);
                }
                else
                {
                    pictureBox1.Image = null;
                }

                if (row.Cells["category_deleted"].Value != DBNull.Value &&
                    Convert.ToInt32(row.Cells["category_deleted"].Value) == 1)
                {
                    MessageBox.Show(
                        "Категория этого товара удалена. Товар доступен только для просмотра.",
                        "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning
                    );
                }
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
            string cleaned = new string(textBox1.Text.Where(char.IsDigit).ToArray());

            if (textBox1.Text != cleaned)
            {
                textBox1.Text = cleaned;
                textBox1.SelectionStart = Math.Min(cursor, textBox1.Text.Length);
            }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
                return;

            if (!char.IsDigit(e.KeyChar) && e.KeyChar != ',' && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            if (e.KeyChar == '.')
            {
                e.KeyChar = ',';
            }

            if (e.KeyChar == ',' && textBox4.Text.Contains(","))
            {
                e.Handled = true;
            }
        }

        private void button4_Click(object sender, EventArgs e) // Добавление изображения
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

                    string savedImagePath;
                    if (Path.GetDirectoryName(dlg.FileName) == appFolderPath)
                    {
                        savedImagePath = dlg.FileName;
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

                LoadData(); // Перезагружаем данные
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении изображения: " + ex.Message);
            }
        }

        private string SaveImage()
        {
            try
            {
                if (pictureBox1.Image == null) return "";

                string name = Guid.NewGuid().ToString() + ".jpg";
                string path = Path.Combine(appFolderPath, name);

                using (Image resized = ResizeImageToMaxSize(pictureBox1.Image, 800, 800))
                {
                    resized.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                }

                currentImagePath = name;
                return path;
            }
            catch
            {
                return "";
            }
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

        private void button6_Click(object sender, EventArgs e) // Удаление изображения
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
                    DeleteProductImageFile(currentProductId);

                    string query = "UPDATE products SET image_path = NULL WHERE id = @id";

                    using (MySqlConnection connection = DbConfig.GetConnection())
                    {
                        connection.Open();
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@id", currentProductId);
                        cmd.ExecuteNonQuery();
                    }

                    pictureBox1.Image = null;

                    LoadData(); // Перезагружаем данные
                    MessageBox.Show("Изображение удалено");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении изображения: " + ex.Message);
                }
            }
        }

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
                    {
                        imagePath = result.ToString();
                    }
                }

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

        private void button5_Click(object sender, EventArgs e) // Возврат в главное меню
        {
            mainadmin adminForm = new mainadmin();
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24)
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7);
            }

            adminForm.Opacity = 0;
            adminForm.Show();
            for (double opacity = 0; opacity <= 1.0; opacity += 0.24)
            {
                adminForm.Opacity = opacity;
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