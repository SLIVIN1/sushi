// ConfigHelper.cs
using System;
using System.Configuration;
using System.Drawing;

public static class ConfigHelper
{
    public static int GetInactivityTimeout()
    {
        try
        {
            return int.Parse(ConfigurationManager.AppSettings["InactivityTimeout"] ?? "30");
        }
        catch
        {
            return 30;
        }
    }

    public static void SaveInactivityTimeout(int seconds)
    {
        try
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("InactivityTimeout");
            config.AppSettings.Settings.Add("InactivityTimeout", seconds.ToString());
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}");
        }
    }
}