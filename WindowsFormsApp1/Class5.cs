using System;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public static class InactivityManager
    {
        // ===== Получение времени последнего ввода от Windows =====
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        private static Timer timer;
        private static int timeoutSeconds;
        private static bool isLocked = false;

        /// <summary>
        /// Запуск отслеживания бездействия
        /// </summary>
        public static void Start()
        {
            // Читаем таймаут из App.config
            string setting = ConfigurationManager.AppSettings["InactivityTimeout"];
            timeoutSeconds = int.TryParse(setting, out int val) ? val : 30;

            isLocked = false;

            // Таймер проверяет каждую секунду
            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        /// <summary>
        /// Остановка отслеживания
        /// </summary>
        public static void Stop()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
        }

        /// <summary>
        /// Получить время бездействия в секундах
        /// </summary>
        private static int GetIdleSeconds()
        {
            LASTINPUTINFO info = new LASTINPUTINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);

            if (GetLastInputInfo(ref info))
            {
                uint idleMillis = (uint)Environment.TickCount - info.dwTime;
                return (int)(idleMillis / 1000);
            }

            return 0;
        }

        /// <summary>
        /// Проверка каждую секунду
        /// </summary>
        private static void Timer_Tick(object sender, EventArgs e)
        {
            if (isLocked) return;

            int idleSeconds = GetIdleSeconds();

            if (idleSeconds >= timeoutSeconds)
            {
                isLocked = true;
                timer.Stop();
                LockSystem();
            }
        }

        /// <summary>
        /// Блокировка — закрыть все формы и показать авторизацию
        /// </summary>
        private static void LockSystem()
        {
            MessageBox.Show(
                $"Система заблокирована из-за бездействия ({timeoutSeconds} сек.)",
                "Блокировка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            // Закрываем все открытые формы кроме главной
            for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
            {
                Form form = Application.OpenForms[i];
                if (form != null)
                {
                    form.Hide();
                }
            }

            // Очищаем сессию
            Session.CurrentLogin = null;
            Session.CurrentRole = 0;

            // Открываем форму авторизации
            A loginForm = new A();
            loginForm.Show();
        }
    }
}