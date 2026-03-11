using MySql.Data.MySqlClient;


namespace WindowsFormsApp1
{
    public static class DbConfig
    {
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(Properties.Settings.Default.ConnectionString);
        }
    }
}