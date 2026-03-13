using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
    public partial class vosstan : Form
    {
        public vosstan()
        {
            InitializeComponent();
        }


        // ===================== ЗАГРУЗКА ТАБЛИЦ =====================
        // Добавьте в начало класса:
        private void LogError(string message, int lineNumber = 0)
        {
            string logPath = Path.Combine(Application.StartupPath, "import_errors.log");
            string lineInfo = lineNumber > 0 ? $" [Строка {lineNumber}]" : "";
            File.AppendAllText(logPath, $"{DateTime.Now}{lineInfo}: {message}\n");
        }
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

        // ===================== ВОССТАНОВЛЕНИЕ БД =====================

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



        // ===================== ИМПОРТ CSV =====================

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
        // ===================== ПАРСИНГ CSV СТРОКИ =====================
        /// <summary>
        /// Разбирает строку CSV с учётом кавычек и разделителя
        /// </summary>
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
                    // Если внутри кавычек встречаем двойные кавычки - это экранирование
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++; // Пропускаем следующую кавычку
                    }
                    else
                    {
                        // Переключаем состояние кавычек
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    // Разделитель вне кавычек - добавляем значение
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    // Обычный символ - добавляем
                    current.Append(c);
                }
            }

            // Добавляем последнее значение
            result.Add(current.ToString().Trim());

            return result;
        }

        /// <summary>
        /// Определяет разделитель CSV (запятая или точка с запятой)
        /// </summary>
        private char DetectDelimiter(string headerLine)
        {
            int semicolons = headerLine.Count(c => c == ';');
            int commas = headerLine.Count(c => c == ',');
            return semicolons > commas ? ';' : ',';
        }
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
            int errors = 0;

            try
            {
                // Очищаем лог перед началом
                string logPath = Path.Combine(Application.StartupPath, "import_errors.log");
                if (File.Exists(logPath))
                    File.Delete(logPath);

                // Столбцы из БД (без id)
                var dbColumns = GetColumnsWithoutId(table);

                // Получаем типы данных столбцов
                var columnTypes = GetColumnTypes(table);

                // Выводим информацию о таблице
                LogError($"=== НАЧАЛО ИМПОРТА ===");
                LogError($"Таблица: {table}");
                LogError($"Столбцы БД: {string.Join(", ", dbColumns)}");
                LogError($"Типы столбцов: {string.Join(", ", columnTypes.Select(kv => $"{kv.Key}={kv.Value}"))}");

                if (dbColumns.Count == 0)
                {
                    MessageBox.Show("Не удалось получить столбцы таблицы");
                    return;
                }

                // Читаем файл
                var allLines = File.ReadAllLines(file, Encoding.UTF8);
                LogError($"Всего строк в файле: {allLines.Length}");

                if (allLines.Length < 2)
                {
                    MessageBox.Show("CSV файл пуст или содержит только заголовок");
                    return;
                }

                // Определяем разделитель по первой строке (заголовку)
                char delimiter = DetectDelimiter(allLines[0]);
                LogError($"Определен разделитель: '{delimiter}'");

                // Парсим заголовок CSV
                var csvHeaders = ParseCsvLine(allLines[0], delimiter);
                LogError($"Заголовки CSV: {string.Join(", ", csvHeaders)}");

                // Проверяем количество столбцов
                if (csvHeaders.Count != dbColumns.Count)
                {
                    string errorMsg = $"Несовпадение столбцов!\n\n" +
                        $"CSV файл: {csvHeaders.Count} столбцов ({string.Join(", ", csvHeaders)})\n\n" +
                        $"Таблица '{table}': {dbColumns.Count} столбцов ({string.Join(", ", dbColumns)})";

                    LogError(errorMsg);
                    MessageBox.Show(errorMsg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Формируем SQL запрос один раз
                string columnList = string.Join(",", dbColumns.Select(c => $"`{c}`"));
                string paramList = string.Join(",", dbColumns.Select(c => "@" + c));
                string query = $"INSERT IGNORE INTO `{table}` ({columnList}) VALUES ({paramList})";

                LogError($"SQL запрос: {query}");

                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();

                    // Построчный импорт (пропускаем заголовок)
                    for (int lineIndex = 1; lineIndex < allLines.Length; lineIndex++)
                    {
                        string line = allLines[lineIndex].Trim();

                        // Пропускаем пустые строки
                        if (string.IsNullOrEmpty(line))
                        {
                            LogError($"Строка {lineIndex + 1} пустая, пропущена", lineIndex + 1);
                            continue;
                        }

                        try
                        {
                            var values = ParseCsvLine(line, delimiter);
                            LogError($"Строка {lineIndex + 1}: распарсено {values.Count} значений", lineIndex + 1);
                            LogError($"Значения: {string.Join(" | ", values)}", lineIndex + 1);

                            if (values.Count != dbColumns.Count)
                            {
                                errors++;
                                LogError($"Несовпадение количества значений: ожидалось {dbColumns.Count}, получено {values.Count}", lineIndex + 1);
                                continue;
                            }

                            using (MySqlCommand cmd = new MySqlCommand(query, conn))
                            {
                                for (int i = 0; i < dbColumns.Count; i++)
                                {
                                    string columnName = dbColumns[i];
                                    string stringValue = values[i].Trim();

                                    LogError($"Столбец {i + 1}: '{columnName}' = '{stringValue}'", lineIndex + 1);

                                    // Обработка NULL
                                    if (string.IsNullOrEmpty(stringValue) || stringValue.ToUpper() == "NULL")
                                    {
                                        cmd.Parameters.AddWithValue("@" + columnName, DBNull.Value);
                                        LogError($"  -> NULL", lineIndex + 1);
                                        continue;
                                    }

                                    // Получаем тип столбца
                                    string columnType = columnTypes.ContainsKey(columnName)
                                        ? columnTypes[columnName].ToLower()
                                        : "string";

                                    LogError($"  Тип столбца: {columnType}", lineIndex + 1);

                                    // Преобразование значения в зависимости от типа столбца
                                    object convertedValue = ConvertValueByType(stringValue, columnType, lineIndex + 1);

                                    if (convertedValue == null)
                                    {
                                        cmd.Parameters.AddWithValue("@" + columnName, DBNull.Value);
                                        LogError($"  -> Преобразовано в NULL", lineIndex + 1);
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("@" + columnName, convertedValue);
                                        LogError($"  -> Преобразовано в: {convertedValue} (тип: {convertedValue.GetType()})", lineIndex + 1);
                                    }
                                }

                                cmd.ExecuteNonQuery();
                                inserted++;
                                LogError($"  -> УСПЕШНО вставлено!", lineIndex + 1);
                            }
                        }
                        catch (Exception ex)
                        {
                            errors++;
                            LogError($"ОШИБКА: {ex.Message}", lineIndex + 1);

                            // Показываем первую ошибку в MessageBox для быстрой диагностики
                            if (errors == 1)
                            {
                                MessageBox.Show($"Первая ошибка в строке {lineIndex + 1}:\n{ex.Message}\n\nПодробности в файле import_errors.log",
                                    "Ошибка импорта", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }

                string result = $"Импорт завершён!\n\nДобавлено записей: {inserted}\nОшибок: {errors}";

                if (errors > 0)
                {
                    result += $"\n\nПодробности ошибок сохранены в файле:\nimport_errors.log";
                }

                MessageBox.Show(result, "Результат", MessageBoxButtons.OK,
                    errors > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogError($"Критическая ошибка: {ex.Message}");
                MessageBox.Show("Ошибка импорта:\n" + ex.Message + "\n\nПодробности в import_errors.log");
            }
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

        // ===================== ПРЕОБРАЗОВАНИЕ ЗНАЧЕНИЙ ПО ТИПУ =====================
        private object ConvertValueByType(string value, string columnType, int lineNumber = 0)
        {
            try
            {
                // Числовые типы
                if (columnType.Contains("int") || columnType.Contains("tinyint") ||
                    columnType.Contains("smallint") || columnType.Contains("mediumint") ||
                    columnType.Contains("bigint"))
                {
                    if (int.TryParse(value, out int intResult))
                        return intResult;
                    else
                    {
                        LogError($"Не удалось преобразовать '{value}' в число", lineNumber);
                        return 0; // или DBNull.Value, если нужно
                    }
                }

                // Типы с плавающей точкой
                if (columnType.Contains("decimal") || columnType.Contains("float") ||
                    columnType.Contains("double") || columnType.Contains("numeric"))
                {
                    // Заменяем запятую на точку для корректного парсинга
                    string normalizedValue = value.Replace(',', '.');

                    // Пробуем разные культуры
                    if (decimal.TryParse(normalizedValue, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal decimalResult))
                        return decimalResult;

                    if (decimal.TryParse(normalizedValue, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.CurrentCulture, out decimalResult))
                        return decimalResult;

                    LogError($"Не удалось преобразовать '{value}' в десятичное число", lineNumber);
                    return 0;
                }

                // Логический тип
                if (columnType.Contains("bit") || columnType.Contains("bool") || columnType.Contains("boolean"))
                {
                    string lowerVal = value.ToLower().Trim();
                    if (lowerVal == "1" || lowerVal == "true" || lowerVal == "yes" ||
                        lowerVal == "on" || lowerVal == "да" || lowerVal == "t" || lowerVal == "y")
                        return true;

                    if (lowerVal == "0" || lowerVal == "false" || lowerVal == "no" ||
                        lowerVal == "off" || lowerVal == "нет" || lowerVal == "f" || lowerVal == "n")
                        return false;

                    LogError($"Не удалось преобразовать '{value}' в логическое значение", lineNumber);
                    return false;
                }

                // Типы даты и времени
                if (columnType.Contains("date") || columnType.Contains("time") ||
                    columnType.Contains("year") || columnType.Contains("timestamp") ||
                    columnType.Contains("datetime"))
                {
                    // Пробуем разные форматы дат
                    string[] formats = {
                "yyyy-MM-dd", "dd.MM.yyyy", "MM/dd/yyyy", "dd/MM/yyyy",
                "yyyy-MM-dd HH:mm:ss", "dd.MM.yyyy HH:mm:ss",
                "MM/dd/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss"
            };

                    if (DateTime.TryParseExact(value, formats,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out DateTime dateResult))
                        return dateResult;

                    if (DateTime.TryParse(value, out dateResult))
                        return dateResult;

                    LogError($"Не удалось преобразовать '{value}' в дату/время", lineNumber);
                    return DBNull.Value;
                }

                // Для всех остальных типов возвращаем как строку
                return value;
            }
            catch (Exception ex)
            {
                LogError($"Ошибка преобразования типа {columnType} для значения '{value}': {ex.Message}", lineNumber);
                return value;
            }
        }
        private void vosstan_Load(object sender, EventArgs e)
        {
            LoadTables();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            A authForm = new A(); // предполагаем, что класс формы авторизации называется A
            for (double opacity = 1.0; opacity > 0; opacity -= 0.24) // было 0.05
            {
                this.Opacity = opacity;
                Application.DoEvents();
                System.Threading.Thread.Sleep(7); // было 20
            }
        }
    }
}