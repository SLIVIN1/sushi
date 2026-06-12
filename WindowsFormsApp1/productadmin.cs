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
    public partial class productadmin : Form
    {
        private DataTable productsTable;
        private DataTable filteredTable;
        private string appFolderPath;
        private int currentProductId = 0;
        private Dictionary<string, Image> imageCache = new Dictionary<string, Image>();

        // Пагинация
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

            InitializePagination();
            this.Activated += productadmin_Activated;
            dataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#B3D9FF");

            appFolderPath = Path.Combine(Application.StartupPath, "ProductImages");
            if (!Directory.Exists(appFolderPath))
                Directory.CreateDirectory(appFolderPath);

            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
            dataGridView1.CellClick += dataGridView1_CellClick;
            dataGridView1.ColumnHeaderMouseClick += DataGridView1_ColumnHeaderMouseClick;

            LoadData();
        }

        #region === Загрузка и отображение данных ===

        private void LoadData()
        {
            try
            {
                string query = @"SELECT p.id, p.article, p.name, p.description, p.price, 
                                c.name AS category_name, 
                                p.image_path
                                FROM products p 
                                LEFT JOIN categories c ON p.category_id = c.id";

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    MySqlDataAdapter da = new MySqlDataAdapter(query, connection);
                    productsTable = new DataTable();
                    da.Fill(productsTable);
                    totalRecords = productsTable.Rows.Count;
                    ApplyFilterAndSort();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void ApplyFilterAndSort()
        {
            if (productsTable == null) return;

            DataView view = new DataView(productsTable);

            if (!string.IsNullOrWhiteSpace(currentFilter))
            {
                string filterExpression = string.Format(
                    "article LIKE '%{0}%' OR name LIKE '%{0}%' OR description LIKE '%{0}%'",
                    currentFilter.Replace("'", "''"));
                view.RowFilter = filterExpression;
            }

            if (!string.IsNullOrWhiteSpace(currentSortColumn))
            {
                view.Sort = currentSortColumn + (sortAscending ? " ASC" : " DESC");
            }

            filteredTable = view.ToTable();
            UpdatePagination();
        }

        private void SetupGridColumns()
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.RowTemplate.Height = 60;
            if (dataGridView1.Columns.Contains("id"))
                dataGridView1.Columns["id"].Visible = false;
            if (dataGridView1.Columns.Contains("image_path"))
                dataGridView1.Columns["image_path"].Visible = false;

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

        #endregion

        #region === Пагинация ===

        private void InitializePagination()
        {
            if (buttonPrev != null)
                buttonPrev.Click += ButtonPrev_Click;
            if (buttonNext != null)
                buttonNext.Click += ButtonNext_Click;
        }

        private void UpdatePagination()
        {
            if (filteredTable == null) return;

            totalRecords = filteredTable.Rows.Count;
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            if (totalPages == 0) totalPages = 1;
            if (currentPage > totalPages) currentPage = totalPages;
            if (currentPage < 1) currentPage = 1;

            int startIndex = (currentPage - 1) * pageSize;
            int endIndex = Math.Min(startIndex + pageSize - 1, totalRecords - 1);

            DataTable pageTable = filteredTable.Clone();

            if (totalRecords > 0)
            {
                for (int i = startIndex; i <= endIndex; i++)
                    pageTable.ImportRow(filteredTable.Rows[i]);
            }

            dataGridView1.DataSource = pageTable;
            SetupGridColumns();

            labelPageInfo.Text = $"{pageTable.Rows.Count} из {totalRecords}";
            UpdatePaginationControls();
        }

        private void UpdatePaginationControls()
        {
            if (buttonPrev != null) buttonPrev.Enabled = currentPage > 1;
            if (buttonNext != null) buttonNext.Enabled = currentPage < totalPages;

            if (flowLayoutPanelPages != null)
            {
                flowLayoutPanelPages.Controls.Clear();

                int startPage = Math.Max(1, currentPage - 2);
                int endPage = Math.Min(totalPages, startPage + 4);
                if (endPage - startPage < 4 && startPage > 1)
                    startPage = Math.Max(1, endPage - 4);

                if (startPage > 1)
                {
                    flowLayoutPanelPages.Controls.Add(CreatePageButton(1));
                    if (startPage > 2)
                    {
                        Label dots = new Label { Text = "...", AutoSize = true, Padding = new Padding(5) };
                        flowLayoutPanelPages.Controls.Add(dots);
                    }
                }

                for (int i = startPage; i <= endPage; i++)
                    flowLayoutPanelPages.Controls.Add(CreatePageButton(i));

                if (endPage < totalPages)
                {
                    if (endPage < totalPages - 1)
                    {
                        Label dots = new Label { Text = "...", AutoSize = true, Padding = new Padding(5) };
                        flowLayoutPanelPages.Controls.Add(dots);
                    }
                    flowLayoutPanelPages.Controls.Add(CreatePageButton(totalPages));
                }
            }
        }

        private Button CreatePageButton(int pageNumber)
        {
            Button btn = new Button
            {
                Text = pageNumber.ToString(),
                Tag = pageNumber,
                Width = 35,
                Height = 30,
                Margin = new Padding(2),
                BackColor = pageNumber == currentPage ? Color.LightBlue : SystemColors.Control,
                Font = new Font(Font, pageNumber == currentPage ? FontStyle.Bold : FontStyle.Regular)
            };
            btn.Click += (s, e) => GoToPage((int)((Button)s).Tag);
            return btn;
        }

        private void GoToPage(int page)
        {
            if (page < 1 || page > totalPages || page == currentPage) return;
            currentPage = page;
            UpdatePagination();
        }

        private void ButtonPrev_Click(object sender, EventArgs e) => GoToPage(currentPage - 1);
        private void ButtonNext_Click(object sender, EventArgs e) => GoToPage(currentPage + 1);

        #endregion

        #region === События DataGridView ===

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name != "image_col") return;

            string path = dataGridView1.Rows[e.RowIndex].Cells["image_path"].Value?.ToString();
            if (string.IsNullOrEmpty(path)) return;

            string fullPath = GetFullImagePath(path);
            if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath)) return;

            if (imageCache.ContainsKey(fullPath))
            {
                e.Value = imageCache[fullPath];
                return;
            }

            using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                Image img = Image.FromStream(fs);
                Image resized = ResizeImage(img, 60, 60);
                imageCache[fullPath] = resized;
                e.Value = resized;
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            currentProductId = Convert.ToInt32(row.Cells["id"].Value);
        }

        private void DataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (columnName == "image_col" || columnName == "id" || columnName == "image_path" )
                return;

            if (currentSortColumn == columnName)
                sortAscending = !sortAscending;
            else
            {
                currentSortColumn = columnName;
                sortAscending = true;
            }

            ApplyFilterAndSort();
        }

        private void productadmin_Activated(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            // Перезагружаем данные при возврате на форму
            LoadData();
        }

        #endregion

        #region === Кнопки ===

        // button1 — Добавить новый товар (переход на addedit в режиме добавления)
        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            addedit form = new addedit(); // Режим добавления — без параметров
            form.ShowDialog();
            this.Show();
            LoadData(); // Обновляем таблицу после возврата
        }

        // button2 — Редактировать выбранный товар (переход на addedit в режиме редактирования)
        private void button2_Click(object sender, EventArgs e)
        {
            if (currentProductId == 0)
            {
                MessageBox.Show("Выберите товар для редактирования",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.Hide();
            addedit form = new addedit(currentProductId); // Режим редактирования — передаём ID
            form.ShowDialog();
            this.Show();
            LoadData();
        }

        // button3 — Удалить товар
        private void button3_Click(object sender, EventArgs e)
        {
            if (currentProductId == 0)
            {
                MessageBox.Show("Выберите товар для удаления",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (IsProductUsedInOrdersById(currentProductId))
            {
                MessageBox.Show(
                    "Нельзя удалить товар, который используется в заказах.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            else
            {
                if (MessageBox.Show("Удалить товар?", "Подтверждение",
                    MessageBoxButtons.YesNo) == DialogResult.No) return;
            }

            try
            {
                string query = "DELETE FROM products WHERE id = @id";

                using (MySqlConnection connection = DbConfig.GetConnection())
                {
                    connection.Open();

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", currentProductId);
                    cmd.ExecuteNonQuery();
                }

                currentProductId = 0;
                LoadData();

                MessageBox.Show("Товар удалён",
                    "Успешно",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении: " + ex.Message);
            }
        }

        // button5 — Возврат в главное меню
        private void button5_Click(object sender, EventArgs e)
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

        #endregion

        #region === Вспомогательные методы ===

        private bool IsProductUsedInOrdersById(int productId)
        {
            using (MySqlConnection connection = DbConfig.GetConnection())
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(
                    @"SELECT COUNT(*) FROM order_items oi
                      INNER JOIN orders o ON oi.order_id = o.id
                      WHERE oi.product_id = @productId", connection);
                cmd.Parameters.AddWithValue("@productId", productId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private string GetFullImagePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";

            if (Path.IsPathRooted(path) && File.Exists(path)) return path;

            if (path.StartsWith("/") || path.StartsWith("\\"))
            {
                string fullPath = Application.StartupPath + path.Replace("/", "\\");
                if (File.Exists(fullPath)) return fullPath;
            }

            string productImagePath = Path.Combine(appFolderPath, Path.GetFileName(path));
            if (File.Exists(productImagePath)) return productImagePath;

            return "";
        }

        private Image ResizeImage(Image img, int w, int h)
        {
            Bitmap bmp = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmp))
                g.DrawImage(img, 0, 0, w, h);
            return bmp;
        }

        #endregion

        #region === Поиск ===

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            TextBox searchBox = sender as TextBox;
            if (searchBox != null)
            {
                currentFilter = searchBox.Text.Trim();
                currentPage = 1;
                ApplyFilterAndSort();
            }
        }

        #endregion

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