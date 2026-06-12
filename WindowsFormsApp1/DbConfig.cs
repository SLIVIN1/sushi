using MySql.Data.MySqlClient;
using System;


namespace WindowsFormsApp1
{
    public static class DbConfig
    {
        // ===== Соединение =====
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(Properties.Settings.Default.ConnectionString);
        }

        public static string GetConnectionString()
        {
            return Properties.Settings.Default.ConnectionString;
        }

        // ===== Разбор через MySqlConnectionStringBuilder (надёжнее Split) =====
        public static string GetServer() => Builder().Server;
        public static string GetDatabase() => Builder().Database;
        public static string GetUser() => Builder().UserID;
        public static string GetPassword() => Builder().Password;

        private static MySqlConnectionStringBuilder Builder()
        {
            return new MySqlConnectionStringBuilder(Properties.Settings.Default.ConnectionString);
        }

        // ===== Сохранение =====
        public static void SaveSettings(string server, string db, string user, string password = null)
        {
            // Если пароль пустой — оставляем старый
            string pwd = string.IsNullOrWhiteSpace(password) ? GetPassword() : password;

            var builder = new MySqlConnectionStringBuilder
            {
                Server = server,
                Database = db,
                UserID = user,
                Password = pwd
            };

            Properties.Settings.Default.ConnectionString = builder.ConnectionString;
            Properties.Settings.Default.Save();
        }

        // ===== Аргументы для mysqldump =====
        public static string GetMysqlDumpArgs(string resultFile)
        {
            var b = Builder();
            string pwdPart = string.IsNullOrEmpty(b.Password) ? "" : $"-p{b.Password}";
            return $"--user={b.UserID} {pwdPart} {b.Database} --result-file=\"{resultFile}\"";
        }
    }
}