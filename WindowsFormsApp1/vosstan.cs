using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class vosstan : Form
    {
        public vosstan()
        {
            InitializeComponent();
        }

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

                    while (reader.Read())
                    {
                        comboBox1.Items.Add(reader[0].ToString());
                    }
                }
            }
            catch
            {
                MessageBox.Show("Не удалось загрузить таблицы");
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

        // ===================== ВОССТАНОВЛЕНИЕ СТРУКТУРЫ =====================
        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "structure.sql");

                if (!File.Exists(path))
                {
                    MessageBox.Show("Файл структуры БД не найден");
                    return;
                }

                string script = File.ReadAllText(path);

                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand(script, conn);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Структура базы данных успешно восстановлена");

                LoadTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка восстановления БД:\n" + ex.Message);
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

        // ===================== ПОЛУЧЕНИЕ ТИПОВ СТОЛБЦОВ =====================
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

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    types[reader.GetString(0)] = reader.GetString(1);
                }
            }

            return types;
        }

        // ===================== ПРЕОБРАЗОВАНИЕ ЗНАЧЕНИЙ =====================
        private object ConvertValueByType(string value, string columnType)
        {
            try
            {
                if (string.IsNullOrEmpty(value) || value.ToUpper() == "NULL")
                    return DBNull.Value;

                // Числовые типы
                if (columnType.Contains("int") || columnType.Contains("tinyint") ||
                    columnType.Contains("smallint") || columnType.Contains("mediumint") ||
                    columnType.Contains("bigint"))
                {
                    if (int.TryParse(value, out int intResult))
                        return intResult;
                    return 0;
                }

                // Типы с плавающей точкой
                if (columnType.Contains("decimal") || columnType.Contains("float") ||
                    columnType.Contains("double") || columnType.Contains("numeric"))
                {
                    string normalizedValue = value.Replace(',', '.');

                    if (decimal.TryParse(normalizedValue, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal decimalResult))
                        return decimalResult;

                    if (decimal.TryParse(normalizedValue, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.CurrentCulture, out decimalResult))
                        return decimalResult;

                    return 0;
                }

                // Логический тип
                if (columnType.Contains("bit") || columnType.Contains("bool") || columnType.Contains("boolean"))
                {
                    string lowerVal = value.ToLower().Trim();
                    if (lowerVal == "1" || lowerVal == "true" || lowerVal == "yes" ||
                        lowerVal == "on" || lowerVal == "да")
                        return true;
                    return false;
                }

                // Типы даты и времени
                if (columnType.Contains("date") || columnType.Contains("time") ||
                    columnType.Contains("datetime") || columnType.Contains("timestamp"))
                {
                    if (DateTime.TryParse(value, out DateTime dateResult))
                        return dateResult;
                    return DBNull.Value;
                }

                // Для всех остальных типов возвращаем как строку
                return value;
            }
            catch
            {
                return value;
            }
        }

        // ===================== СОЗДАНИЕ КЛЮЧА ДЛЯ ПРОВЕРКИ ДУБЛИКАТОВ =====================
        private string CreateRecordKey(List<string> values, List<string> columns)
        {
            // Для таблицы users используем login как уникальный ключ
            if (columns.Contains("login"))
            {
                int loginIndex = columns.IndexOf("login");
                if (loginIndex >= 0 && loginIndex < values.Count)
                    return "login_" + values[loginIndex].Trim();
            }

            // Для других таблиц можно создать составной ключ из всех полей
            return string.Join("|", values.Select(v => v.Trim()));
        }

        // ===================== ИМПОРТ CSV =====================
        private void button2_Click_1(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Выберите таблицу");
                return;
            }

            if (!File.Exists(textBox1.Text))
            {
                MessageBox.Show("CSV файл не найден");
                return;
            }

            string table = comboBox1.SelectedItem.ToString();
            string file = textBox1.Text;
            int inserted = 0;
            int skipped = 0;
            int errors = 0;

            try
            {
                // Столбцы из БД (без id)
                var dbColumns = GetColumnsWithoutId(table);
                var columnTypes = GetColumnTypes(table);

                if (dbColumns.Count == 0)
                {
                    MessageBox.Show("Не удалось получить столбцы таблицы");
                    return;
                }

                // Читаем файл
                var allLines = File.ReadAllLines(file, Encoding.UTF8);

                if (allLines.Length < 2)
                {
                    MessageBox.Show("CSV файл пуст или содержит только заголовок");
                    return;
                }

                // Определяем разделитель
                char delimiter = DetectDelimiter(allLines[0]);

                // Парсим заголовок
                var csvHeaders = ParseCsvLine(allLines[0], delimiter);

                // Проверяем количество столбцов
                if (csvHeaders.Count != dbColumns.Count)
                {
                    string errorMsg = $"Несовпадение столбцов!\n\n" +
                        $"CSV файл: {csvHeaders.Count} столбцов ({string.Join(", ", csvHeaders)})\n\n" +
                        $"Таблица '{table}': {dbColumns.Count} столбцов ({string.Join(", ", dbColumns)})";

                    MessageBox.Show(errorMsg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Получаем существующие записи из БД для проверки дубликатов
                HashSet<string> existingRecords = new HashSet<string>();

                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();

                    // Для users используем login как уникальный ключ
                    if (table.ToLower() == "users" && dbColumns.Contains("login"))
                    {
                        string selectQuery = "SELECT login FROM users";
                        MySqlCommand selectCmd = new MySqlCommand(selectQuery, conn);
                        MySqlDataReader reader = selectCmd.ExecuteReader();

                        while (reader.Read())
                        {
                            string login = reader["login"]?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(login))
                                existingRecords.Add("login_" + login);
                        }
                        reader.Close();
                    }
                    else
                    {
                        // Для других таблиц получаем все данные для сравнения
                        string selectQuery = $"SELECT {string.Join(",", dbColumns.Select(c => $"`{c}`"))} FROM `{table}`";
                        MySqlCommand selectCmd = new MySqlCommand(selectQuery, conn);
                        MySqlDataReader reader = selectCmd.ExecuteReader();

                        while (reader.Read())
                        {
                            List<string> rowValues = new List<string>();
                            for (int i = 0; i < dbColumns.Count; i++)
                            {
                                rowValues.Add(reader[i]?.ToString() ?? "");
                            }
                            existingRecords.Add(string.Join("|", rowValues));
                        }
                        reader.Close();
                    }
                }

                // Формируем SQL запрос
                string columnList = string.Join(",", dbColumns.Select(c => $"`{c}`"));
                string paramList = string.Join(",", dbColumns.Select(c => "@" + c));
                string query = $"INSERT INTO `{table}` ({columnList}) VALUES ({paramList})";

                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();

                    // Построчный импорт
                    for (int lineIndex = 1; lineIndex < allLines.Length; lineIndex++)
                    {
                        string line = allLines[lineIndex].Trim();

                        if (string.IsNullOrEmpty(line))
                            continue;

                        try
                        {
                            var values = ParseCsvLine(line, delimiter);

                            if (values.Count != dbColumns.Count)
                            {
                                errors++;
                                continue;
                            }

                            // Создаем ключ для проверки дубликата
                            string recordKey = CreateRecordKey(values, dbColumns);

                            // Проверяем, существует ли уже такая запись
                            if (existingRecords.Contains(recordKey))
                            {
                                skipped++;
                                continue;
                            }

                            using (MySqlCommand cmd = new MySqlCommand(query, conn))
                            {
                                for (int i = 0; i < dbColumns.Count; i++)
                                {
                                    string columnName = dbColumns[i];
                                    string stringValue = values[i].Trim();
                                    string columnType = columnTypes.ContainsKey(columnName)
                                        ? columnTypes[columnName].ToLower()
                                        : "string";

                                    object convertedValue = ConvertValueByType(stringValue, columnType);
                                    cmd.Parameters.AddWithValue("@" + columnName, convertedValue ?? DBNull.Value);
                                }

                                cmd.ExecuteNonQuery();
                                inserted++;

                                // Добавляем новую запись в HashSet, чтобы не проверять её снова
                                existingRecords.Add(recordKey);
                            }
                        }
                        catch (Exception)
                        {
                            errors++;
                        }
                    }
                }

                string result = $"Импорт завершён!\n\n" +
                               $"Добавлено новых записей: {inserted}\n" +
                               $"Пропущено (уже существуют): {skipped}\n" +
                               $"Ошибок: {errors}";

                MessageBox.Show(result, "Результат", MessageBoxButtons.OK,
                    errors > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка импорта:\n" + ex.Message);
            }
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
    }
}