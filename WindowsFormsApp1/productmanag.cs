using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace WindowsFormsApp1
{
    public partial class productmanag : Form
    {
        private DataTable productsTable;
        private string appFolderPath;

        public productmanag()
        {
            InitializeComponent();
            textBox1.TextChanged += (s, e) => ApplyFilters();
            comboBox1.SelectedIndexChanged += (s, e) => ApplyFilters();
            comboBox2.SelectedIndexChanged += (s, e) => ApplyFilters();
            this.Activated += productmanag_Activated;
            dataGridView1.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#CCE6FF");
            appFolderPath = Path.Combine(Application.StartupPath, "ProductImages");
            if (!Directory.Exists(appFolderPath))
                Directory.CreateDirectory(appFolderPath);
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
            comboBox2.Items.Clear();
            comboBox2.Items.Add("Без сортировки");
            comboBox2.Items.Add("По возрастанию цены");
            comboBox2.Items.Add("По убыванию цены");
            comboBox2.SelectedIndex = 0;
        }

        private void LoadCategories()
        {
            try
            {
                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT id, name FROM categories";
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
                    string sql = @"SELECT p.id, p.article, p.name, p.description, p.price, c.name AS category_name, c.is_deleted AS category_deleted, p.image_path, p.is_deleted FROM products p LEFT JOIN categories c ON p.category_id = c.id WHERE p.is_deleted = 0";

                    MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    productsTable = dt;
                    dataGridView1.DataSource = productsTable;

                    SetupGrid();
                    LoadImagesToGrid();
                    foreach (DataGridViewColumn col in dataGridView1.Columns)
                    {
                        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
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
                dataGridView1.Columns["article"].HeaderText = "Артикул";
            if (dataGridView1.Columns.Contains("name"))
                dataGridView1.Columns["name"].HeaderText = "Название";
            if (dataGridView1.Columns.Contains("description"))
                dataGridView1.Columns["description"].HeaderText = "Описание";
            if (dataGridView1.Columns.Contains("price"))
                dataGridView1.Columns["price"].HeaderText = "Цена";
            if (dataGridView1.Columns.Contains("category_name"))
                dataGridView1.Columns["category_name"].HeaderText = "Категория";
            if (dataGridView1.Columns.Contains("category_deleted"))
                dataGridView1.Columns["category_deleted"].Visible = false;
            if (dataGridView1.Columns.Contains("is_deleted"))
                dataGridView1.Columns["is_deleted"].Visible = false;
            if (dataGridView1.Columns.Contains("image_path"))
                dataGridView1.Columns["image_path"].Visible = false;

            // Колонка изображения
            if (!dataGridView1.Columns.Contains("image_col"))
            {
                DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
                imgCol.Name = "image_col";
                imgCol.HeaderText = "Картинка";
                imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                imgCol.Width = 70;
                dataGridView1.Columns.Add(imgCol);
            }

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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

            using (var img = Image.FromFile(fullPath))
            {
                return ResizeImage(img, w, h);
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

        private void ApplyFilters()
        {
            if (productsTable == null) return;

            string filter = "";

            // По названию
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
                filter += $"[name] LIKE '%{textBox1.Text}%'";

            // По категории
            if (comboBox1.SelectedIndex > 0)
            {
                if (!string.IsNullOrEmpty(filter)) filter += " AND ";
                filter += $"[category_name] = '{comboBox1.Text}'";
            }

            DataView dv = productsTable.DefaultView;
            dv.RowFilter = filter;

            // Сортировка
            switch (comboBox2.SelectedIndex)
            {
                case 1: dv.Sort = "[price] ASC"; break;
                case 2: dv.Sort = "[price] DESC"; break;
                default: dv.Sort = ""; break;
            }

            dataGridView1.DataSource = dv;
            LoadImagesToGrid();
        }
   
        private void button1_Click_1(object sender, EventArgs e)
        {
            textBox1.Text = "";
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            ApplyFilters();
        }

        private void button2_Click_1(object sender, EventArgs e)
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
            this.Hide();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
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

            tb.Text = new string(chars).Replace("\0", "");
            tb.SelectionStart = Math.Min(cursor, tb.Text.Length);
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
