using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            try
            {
                var columns = GetColumnsWithoutId(table);
                var lines = File.ReadAllLines(file).Skip(1);
                using (MySqlConnection conn = DbConfig.GetConnection())
                {
                    conn.Open();

                    foreach (var line in lines)
                    {
                        var values = line.Split(',');

                        if (values.Length != columns.Count)
                        {
                            MessageBox.Show(
                                $"Ошибка: CSV содержит {values.Length} значений, а таблица ожидает {columns.Count}"
                            );
                            return;
                        }

                        string columnList = string.Join(",", columns);

                        string paramList = string.Join(",", columns.Select(c => "@" + c));

                        string query = $"INSERT INTO {table} ({columnList}) VALUES ({paramList})";

                        MySqlCommand cmd = new MySqlCommand(query, conn);

                        for (int i = 0; i < columns.Count; i++)
                        {
                            cmd.Parameters.AddWithValue("@" + columns[i], values[i]);
                        }

                        cmd.ExecuteNonQuery();

                        inserted++;
                    }
                }

                MessageBox.Show($"Импорт завершен. Добавлено записей: {inserted}");
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
    }
}