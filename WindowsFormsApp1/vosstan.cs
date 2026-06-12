using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{

    public partial class vosstan : Form
    {
        public vosstan()
        {
            InitializeComponent();
            LoadTables();
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
        private bool allowClose = false;
        // ===================== ЗАГРУЗКА ТАБЛИЦ =====================
        private void LoadTables()
        {
            try
            {
                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand("SHOW TABLES", conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    comboBox1.Items.Clear();
                    comboBox2.Items.Clear();

                    while (reader.Read())
                    {
                        comboBox1.Items.Add(reader[0].ToString());
                        comboBox2.Items.Add(reader[0].ToString());
                    }
                }
            }
            catch
            {
                comboBox1.Items.Clear();
                comboBox2.Items.Clear();
            }
        }

        // ===================== СТОЛБЦЫ БЕЗ ID =====================
        private List<string> GetColumnsWithoutId(string table)
        {
            List<string> columns = new List<string>();

            using (MySqlConnection conn = DbConfig.GetConnection())
            {
                conn.Open();

                string query = @"SELECT COLUMN_NAME
                         FROM INFORMATION_SCHEMA.COLUMNS
                         WHERE TABLE_SCHEMA = DATABASE()
                         AND TABLE_NAME = @table
                         AND EXTRA NOT LIKE '%auto_increment%'";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@table", table);

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    columns.Add(reader.GetString(0));
                }
            }

            return columns;
        }

        // ===================== ВЫБОР CSV =====================
        private void textBox1_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "CSV files (*.csv)|*.csv";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dialog.FileName;
            }
        }

       
        // ===================== ПАРСИНГ CSV СТРОКИ =====================
        private List<string> ParseCsvLine(string line, char delimiter)
        {
            List<string> result = new List<string>();
            StringBuilder current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString().Trim());
            return result;
        }

        // ===================== ОПРЕДЕЛЕНИЕ РАЗДЕЛИТЕЛЯ =====================
        private char DetectDelimiter(string headerLine)
        {
            int semicolons = headerLine.Count(c => c == ';');
            int commas = headerLine.Count(c => c == ',');
            return semicolons > commas ? ';' : ',';
        }


        private void vosstan_Load(object sender, EventArgs e)
        {
            LoadTables();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Плавное закрытие с возвратом на форму A
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24)
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7);
            }

            A authForm = new A();
            authForm.Show();
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            setting nastroika = new setting();
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24) // было 0.05
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }

            nastroika.Opacity = 0;
            nastroika.Show();
            // Быстрое появление - 0.4 секунды
            for (double opacity = 0; opacity <= 1.0; opacity += 0.24) // было 0.05
            {
                nastroika.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }
            allowClose = true;
            this.Hide();
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "SQL Backup|*.sql";
            sfd.FileName = "backup_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".sql";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = sfd.FileName;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Выберите путь");
                return;
            }

            progressBar1.Value = 20;
            string file = textBox1.Text;

            try
            {
                await Task.Run(() =>
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = @"C:\Program Files\MySQL\MySQL Workbench 8.0 CE\mysqldump.exe",
                        Arguments = DbConfig.GetMysqlDumpArgs(file),
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (var p = System.Diagnostics.Process.Start(psi))
                    {
                        p.WaitForExit();
                        if (p.ExitCode != 0)
                            throw new Exception(p.StandardError.ReadToEnd());
                    }
                });

                progressBar1.Value = 100;
                MessageBox.Show("Backup создан:\n" + file);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }


        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "SQL Backup|*.sql";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = ofd.FileName;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Выберите файл");
                return;
            }

            DialogResult r = MessageBox.Show(
                "⚠ Вся база будет перезаписана!",
                "ОПАСНО",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (r != DialogResult.Yes) return;

            try
            {
                string sql = File.ReadAllText(textBox2.Text);

                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();
                    new MySqlCommand(sql, conn).ExecuteNonQuery();
                }

                MessageBox.Show("БД восстановлена");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV|*.csv";

            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Выберите таблицу");
                return;
            }

            sfd.FileName = comboBox1.SelectedItem.ToString() + ".csv";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = sfd.FileName;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null || string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("Выберите таблицу и путь");
                return;
            }

            string table = comboBox1.SelectedItem.ToString();

            try
            {
                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();

                    var cols = GetColumnsWithoutId(table);

                    string query = $"SELECT {string.Join(",", cols)} FROM `{table}`";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    using (StreamWriter sw = new StreamWriter(textBox4.Text))
                    {
                        sw.WriteLine(string.Join(",", cols));

                        while (reader.Read())
                        {
                            List<string> row = new List<string>();

                            for (int i = 0; i < cols.Count; i++)
                                row.Add(reader[i]?.ToString());

                            sw.WriteLine(string.Join(",", row));
                        }
                    }
                }

                MessageBox.Show("Экспорт готов");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string CreateRecordKey(List<string> values, List<string> columns)
        {
            if (columns.Contains("login"))
            {
                int loginIndex = columns.IndexOf("login");
                if (loginIndex >= 0 && loginIndex < values.Count)
                    return "login_" + values[loginIndex].Trim();
            }
            return string.Join("|", values.Select(v => v.Trim()));
        }

        // ===================== ТИПЫ СТОЛБЦОВ ИЗ БД =====================
        private Dictionary<string, string> GetColumnTypes(string table)
        {
            var types = new Dictionary<string, string>();

            using (MySqlConnection conn = DbConfig.GetConnection())
            {
                conn.Open();

                string query = @"SELECT COLUMN_NAME, DATA_TYPE 
                         FROM INFORMATION_SCHEMA.COLUMNS 
                         WHERE TABLE_SCHEMA = DATABASE() 
                         AND TABLE_NAME = @table";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@table", table);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        types[reader.GetString(0)] = reader.GetString(1);
                    }
                }
            }

            return types;
        }

        // ===================== КОНВЕРТАЦИЯ ЗНАЧЕНИЙ ПО ТИПУ =====================
        private object ConvertValueByType(string value, string columnType)
        {
            try
            {
                if (string.IsNullOrEmpty(value) || value.ToUpper() == "NULL")
                    return DBNull.Value;

                // Целые числа
                if (columnType.Contains("int") || columnType.Contains("tinyint") ||
                    columnType.Contains("smallint") || columnType.Contains("mediumint") ||
                    columnType.Contains("bigint"))
                {
                    if (long.TryParse(value, out long longResult))
                        return longResult;
                    return 0;
                }

                // Дробные числа
                if (columnType.Contains("decimal") || columnType.Contains("float") ||
                    columnType.Contains("double") || columnType.Contains("numeric"))
                {
                    string normalized = value.Replace(',', '.');
                    if (decimal.TryParse(normalized, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal decResult))
                        return decResult;
                    return 0;
                }

                // Логический тип
                if (columnType.Contains("bit") || columnType.Contains("bool") || columnType.Contains("boolean"))
                {
                    string low = value.ToLower().Trim();
                    return (low == "1" || low == "true" || low == "yes" || low == "on" || low == "да");
                }

                // Дата и время
                if (columnType.Contains("date") || columnType.Contains("time") ||
                    columnType.Contains("datetime") || columnType.Contains("timestamp"))
                {
                    if (DateTime.TryParse(value, out DateTime dateResult))
                        return dateResult;
                    return DBNull.Value;
                }

                // Всё остальное — строка
                return value;
            }
            catch
            {
                return value;
            }
        }


        private void button9_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV|*.csv";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = ofd.FileName;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem == null)
            {
                MessageBox.Show("Выберите таблицу для импорта");
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox3.Text) || !File.Exists(textBox3.Text))
            {
                MessageBox.Show("Выберите существующий CSV файл");
                return;
            }

            string table = comboBox2.SelectedItem.ToString();
            string file = textBox3.Text;
            int inserted = 0;
            int skipped = 0;
            int errors = 0;

            try
            {
                var dbColumns = GetColumnsWithoutId(table);
                var columnTypes = GetColumnTypes(table);

                if (dbColumns.Count == 0)
                {
                    MessageBox.Show("Не удалось получить столбцы таблицы");
                    return;
                }

                var lines = File.ReadAllLines(file, Encoding.UTF8);
                if (lines.Length < 2)
                {
                    MessageBox.Show("CSV файл пуст");
                    return;
                }

                char delimiter = DetectDelimiter(lines[0]);
                var csvHeaders = ParseCsvLine(lines[0], delimiter);

                if (csvHeaders.Count != dbColumns.Count)
                {
                    MessageBox.Show($"Несовпадение столбцов!\n" +
                                    $"CSV: {csvHeaders.Count}, БД: {dbColumns.Count}");
                    return;
                }

                // ============== СОБИРАЕМ ДУБЛИ ИЗ БД ==============
                HashSet<string> existingRecords = new HashSet<string>();

                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();

                    // Для таблицы users — уникальный ключ login
                    if (table.ToLower() == "users" && dbColumns.Contains("login"))
                    {
                        MySqlCommand cmd = new MySqlCommand("SELECT login FROM users", conn);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                string login = r["login"]?.ToString() ?? "";
                                if (!string.IsNullOrEmpty(login))
                                    existingRecords.Add("login_" + login);
                            }
                        }
                    }
                    else
                    {
                        // Для остальных таблиц — составной ключ из всех полей
                        string colList = string.Join(",", dbColumns.Select(c => $"`{c}`"));
                        MySqlCommand cmd = new MySqlCommand($"SELECT {colList} FROM `{table}`", conn);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                List<string> row = new List<string>();
                                for (int i = 0; i < dbColumns.Count; i++)
                                    row.Add(r[i]?.ToString() ?? "");
                                existingRecords.Add(string.Join("|", row));
                            }
                        }
                    }
                }

                // ============== СОБИРАЕМ ДУБЛИ ВНУТРИ САМОГО CSV ==============
                // (чтобы в одном файле тоже не было повторов)
                HashSet<string> seenInFile = new HashSet<string>();

                // ============== ВСТАВКА ==============
                string colNames = string.Join(",", dbColumns.Select(c => $"`{c}`"));
                string paramNames = string.Join(",", dbColumns.Select(c => "@" + c));
                string query = $"INSERT INTO `{table}` ({colNames}) VALUES ({paramNames})";

                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        try
                        {
                            var values = ParseCsvLine(lines[i], delimiter);
                            if (values.Count != dbColumns.Count) { errors++; continue; }

                            // Ключ для проверки дубля
                            string recordKey = CreateRecordKey(values, dbColumns);

                            // Проверка 1: уже есть в БД?
                            if (existingRecords.Contains(recordKey)) { skipped++; continue; }

                            // Проверка 2: уже встречался в этом CSV?
                            if (!seenInFile.Add(recordKey)) { skipped++; continue; }

                            // Вставляем
                            using (MySqlCommand cmd = new MySqlCommand(query, conn))
                            {
                                for (int j = 0; j < dbColumns.Count; j++)
                                {
                                    string colName = dbColumns[j];
                                    string colType = columnTypes.ContainsKey(colName)
                                        ? columnTypes[colName].ToLower()
                                        : "varchar";
                                    object converted = ConvertValueByType(values[j].Trim(), colType);
                                    cmd.Parameters.AddWithValue("@" + colName, converted ?? DBNull.Value);
                                }
                                cmd.ExecuteNonQuery();
                                inserted++;

                                // Запоминаем, что эта запись теперь в БД
                                existingRecords.Add(recordKey);
                            }
                        }
                        catch (Exception)
                        {
                            errors++;
                        }
                    }
                }

                MessageBox.Show(
                    $"Импорт завершён!\n\n" +
                    $"Добавлено: {inserted}\n" +
                    $"Пропущено (дубли): {skipped}\n" +
                    $"Ошибок: {errors}",
                    "Результат",
                    MessageBoxButtons.OK,
                    errors > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка импорта: " + ex.Message);
            }
        }

    }
}
