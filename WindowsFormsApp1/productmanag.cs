using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class productmanag : BaseForm
    {
        private DataTable productsTable;
        private DataTable filteredTable; // Для хранения отфильтрованных данных
        private string appFolderPath;

        // Поля для пагинации
        private int currentPage = 1;
        private int pageSize = 20;
        private int totalRecords = 0;
        private int totalPages = 1;
        private string currentFilter = "";
        private string currentSortColumn = "";
        private bool sortAscending = true;

        public productmanag()
        {
            InitializeComponent();

            // Инициализация пагинации
            InitializePagination();

            textBox1.TextChanged += TextBoxSearch_TextChanged;
            comboBox1.SelectedIndexChanged += FilterChanged;
            comboBox2.SelectedIndexChanged += FilterChanged;

            this.Activated += productmanag_Activated;
            dataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#CCE6FF");
            dataGridView1.ColumnHeaderMouseClick += DataGridView1_ColumnHeaderMouseClick; // Для сортировки

            appFolderPath = Path.Combine(Application.StartupPath, "ProductImages");
            if (!Directory.Exists(appFolderPath))
                Directory.CreateDirectory(appFolderPath);
        }

        private void InitializePagination()
        {
            // Подписка на события кнопок
            if (buttonPrev != null)
                buttonPrev.Click += ButtonPrev_Click;

            if (buttonNext != null)
                buttonNext.Click += ButtonNext_Click;
        }

        private void productmanag_Activated(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        private void productmanag_Load(object sender, EventArgs e)
        {
            LoadCategories();
            LoadProducts();

            // Настройка сортировки
            if (comboBox2 != null)
            {
                comboBox2.Items.Clear();
                comboBox2.Items.Add("Без сортировки");
                comboBox2.Items.Add("По возрастанию цены");
                comboBox2.Items.Add("По убыванию цены");
                comboBox2.SelectedIndex = 0;
            }
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
            ApplyFilters();
        }

        private void LoadCategories()
        {
            try
            {
                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT id, name FROM categories WHERE is_deleted = 0";
                    MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Все категории
                    DataRow allRow = dt.NewRow();
                    allRow["id"] = 0;
                    allRow["name"] = "Все категории";
                    dt.Rows.InsertAt(allRow, 0);

                    comboBox1.DataSource = dt;
                    comboBox1.DisplayMember = "name";
                    comboBox1.ValueMember = "id";
                    comboBox1.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message);
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT p.id, p.article, p.name, p.description, p.price, 
                                  c.name AS category_name, c.is_deleted AS category_deleted, 
                                  p.image_path, p.is_deleted 
                                  FROM products p 
                                  LEFT JOIN categories c ON p.category_id = c.id 
                                  WHERE p.is_deleted = 0";

                    MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    productsTable = dt;

                    // Подсчет общего количества записей
                    totalRecords = productsTable.Rows.Count;

                    // Применяем фильтрацию
                    ApplyFilters();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки продуктов: " + ex.Message);
            }
        }

        private void SetupGrid()
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.RowTemplate.Height = 60;

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
            if (dataGridView1.Columns.Contains("category_deleted"))
                dataGridView1.Columns["category_deleted"].Visible = false;
            if (dataGridView1.Columns.Contains("is_deleted"))
                dataGridView1.Columns["is_deleted"].Visible = false;
            if (dataGridView1.Columns.Contains("image_path"))
                dataGridView1.Columns["image_path"].Visible = false;
            if (dataGridView1.Columns.Contains("id"))
                dataGridView1.Columns["id"].Visible = false;

            // Колонка изображения
            if (!dataGridView1.Columns.Contains("image_col"))
            {
                DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
                imgCol.Name = "image_col";
                imgCol.HeaderText = "Картинка";
                imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                imgCol.Width = 70;
                imgCol.SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns.Add(imgCol);
            }

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Подсветка удаленных категорий
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                if (row.Cells["category_deleted"] != null &&
                    row.Cells["category_deleted"].Value != null &&
                    row.Cells["category_deleted"].Value != DBNull.Value &&
                    Convert.ToInt32(row.Cells["category_deleted"].Value) == 1)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 87);
                    row.DefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Italic);
                }
            }
        }

        private void LoadImagesToGrid()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;
                string path = row.Cells["image_path"].Value?.ToString();
                row.Cells["image_col"].Value = LoadImage(path, 60, 60);
            }
        }

        private Image LoadImage(string path, int w, int h)
        {
            if (string.IsNullOrEmpty(path)) return null;
            string fullPath = GetFullImagePath(path);
            if (!File.Exists(fullPath)) return null;

            try
            {
                using (var img = Image.FromFile(fullPath))
                {
                    return ResizeImage(img, w, h);
                }
            }
            catch
            {
                return null;
            }
        }

        private string GetFullImagePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path) && File.Exists(path)) return path;

            string localPath = Path.Combine(appFolderPath, Path.GetFileName(path));
            if (File.Exists(localPath)) return localPath;

            return "";
        }

        private Image ResizeImage(Image img, int w, int h)
        {
            Bitmap bmp = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmp))
                g.DrawImage(img, 0, 0, w, h);
            return bmp;
        }

        // Объединенный метод фильтрации
        private void ApplyFilters()
        {
            if (productsTable == null) return;

            // Создаем DataView из исходной таблицы
            DataView view = new DataView(productsTable);
            string filter = "";

            // Фильтр по названию (поиск)
            if (!string.IsNullOrWhiteSpace(currentFilter))
            {
                filter = $"[name] LIKE '%{currentFilter.Replace("'", "''")}%' OR " +
                        $"[article] LIKE '%{currentFilter.Replace("'", "''")}%' OR " +
                        $"[description] LIKE '%{currentFilter.Replace("'", "''")}%'";
            }

            // Фильтр по категории
            if (comboBox1.SelectedIndex > 0 && comboBox1.SelectedValue != null)
            {
                int categoryId = Convert.ToInt32(comboBox1.SelectedValue);
                // Нам нужно имя категории для фильтрации
                string categoryName = comboBox1.Text;

                if (!string.IsNullOrEmpty(filter))
                    filter += " AND ";

                filter += $"[category_name] = '{categoryName.Replace("'", "''")}'";
            }

            // Применяем фильтр
            view.RowFilter = filter;

            // Применяем сортировку из comboBox2
            string sortExpression = "";

            // Сортировка из выпадающего списка
            if (comboBox2.SelectedIndex == 1)
                sortExpression = "[price] ASC";
            else if (comboBox2.SelectedIndex == 2)
                sortExpression = "[price] DESC";

            // Если есть сортировка по колонке (клик по заголовку), она имеет приоритет
            if (!string.IsNullOrWhiteSpace(currentSortColumn))
            {
                sortExpression = currentSortColumn + (sortAscending ? " ASC" : " DESC");
            }

            view.Sort = sortExpression;

            // Получаем отфильтрованную таблицу
            filteredTable = view.ToTable();

            // Обновляем пагинацию
            UpdatePagination();
        }

        // Обновление пагинации
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
            SetupGrid();

            // Загружаем изображения
            LoadImagesToGrid();

            // Обновляем информацию о записях
            int displayedRecords = pageTable.Rows.Count;
            if (labelPage != null)
                labelPage.Text = $"{displayedRecords} из {totalRecords}";

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
            btn.FlatStyle = FlatStyle.Standard;

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

        // Обработчик изменения текста поиска
        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            TextBox searchBox = sender as TextBox;
            if (searchBox != null)
            {
                currentFilter = searchBox.Text.Trim();
                currentPage = 1; // Сбрасываем на первую страницу
                ApplyFilters();
            }
        }

        // Обработчик изменения фильтров (категория, сортировка)
        private void FilterChanged(object sender, EventArgs e)
        {
            currentPage = 1; // Сбрасываем на первую страницу
            ApplyFilters();
        }

        private void button1_Click_1(object sender, EventArgs e) // Сброс фильтров
        {
            textBox1.Text = "";
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            currentFilter = "";
            currentSortColumn = "";
            sortAscending = true;
            currentPage = 1;
            ApplyFilters();
        }

        private void button2_Click_1(object sender, EventArgs e) // Возврат в главное меню
        {
            mainmanag managForm = new mainmanag();
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24)
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7);
            }

            managForm.Opacity = 0;
            managForm.Show();

            for (double opacity = 0; opacity <= 1.0; opacity += 0.24)
            {
                managForm.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7);
            }
            allowClose = true;
            this.Hide();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Этот метод больше не используется напрямую для фильтрации,
            // так как мы используем TextBoxSearch_TextChanged
            // Но оставляем логику форматирования текста
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

            string newText = new string(chars).Replace("\0", "");
            if (tb.Text != newText)
            {
                tb.Text = newText;
                tb.SelectionStart = Math.Min(cursor, tb.Text.Length);
            }
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