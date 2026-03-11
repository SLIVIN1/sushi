using System;
using System.Configuration;

namespace WindowsFormsApp1
{
    public static class AppConfigSettings
    {
        public static int InactivityTimeoutSeconds
        {
            get
            {
                string value = ConfigurationManager.AppSettings["InactivityTimeoutSeconds"];
                if (int.TryParse(value, out int timeout) && timeout > 0)
                {
                    return timeout;
                }
                return 30;
            }
        }

        public static bool EnableInactivityLock
        {
            get
            {
                string value = ConfigurationManager.AppSettings["EnableInactivityLock"];
                if (bool.TryParse(value, out bool enabled))
                {
                    return enabled;
                }
                return true;
            }
        }

        public static void UpdateInactivityTimeout(int newTimeout)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["InactivityTimeoutSeconds"].Value = newTimeout.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}