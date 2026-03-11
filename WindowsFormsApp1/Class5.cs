using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class BaseForm : Form
    {
        private Timer activityTimer;
        private DateTime lastActivityTime;
        private int timeoutSeconds;
        private bool enabled;
        private bool isLocked = false;

        public BaseForm()
        {
            // Получаем настройки из App.config
            timeoutSeconds = AppConfigSettings.InactivityTimeoutSeconds;
            enabled = AppConfigSettings.EnableInactivityLock;

            if (enabled)
            {
                InitializeTimer();
                SubscribeToEvents();
            }

            this.Load += BaseForm_Load;
            this.FormClosed += BaseForm_FormClosed;
        }

        private void InitializeTimer()
        {
            activityTimer = new Timer();
            activityTimer.Interval = 1000; // Проверка каждую секунду
            activityTimer.Tick += CheckInactivity;
        }

        private void SubscribeToEvents()
        {
            // Подписываемся на события мыши и клавиатуры для всей формы
            this.MouseMove += OnUserActivity;
            this.KeyPress += OnUserActivity;
            this.MouseClick += OnUserActivity;
            this.KeyDown += OnUserActivity;

            // Подписываемся на события всех контролов
            SubscribeControls(this);
        }

        private void SubscribeControls(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                control.MouseMove += OnUserActivity;
                control.KeyPress += OnUserActivity;
                control.MouseClick += OnUserActivity;
                control.KeyDown += OnUserActivity;

                // Рекурсивно подписываемся на дочерние контролы
                if (control.HasChildren)
                {
                    SubscribeControls(control);
                }
            }
        }

        private void OnUserActivity(object sender, EventArgs e)
        {
            lastActivityTime = DateTime.Now;
        }

        private void CheckInactivity(object sender, EventArgs e)
        {
            if (!enabled || isLocked) return;

            TimeSpan inactiveTime = DateTime.Now - lastActivityTime;

            if (inactiveTime.TotalSeconds >= timeoutSeconds)
            {
                ShowLoginForm();
            }
        }

        protected virtual void ShowLoginForm()
        {
            // Останавливаем таймер
            activityTimer.Stop();
            isLocked = true;

            // Создаем и показываем форму авторизации
            using (var loginForm = new A())
            {
                // Сохраняем состояние видимости
                bool wasVisible = this.Visible;

                // Скрываем текущую форму
                this.Hide();

                // Показываем форму авторизации
                var result = loginForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // Успешная авторизация
                    isLocked = false;
                    lastActivityTime = DateTime.Now;

                    // Показываем форму обратно
                    if (wasVisible)
                    {
                        this.Show();
                    }

                    // Запускаем таймер заново
                    activityTimer.Start();
                }
                else
                {
                    // Если пользователь закрыл форму авторизации без входа
                    Application.Exit();
                }
            }
        }

        private void BaseForm_Load(object sender, EventArgs e)
        {
            if (enabled)
            {
                lastActivityTime = DateTime.Now;
                activityTimer.Start();
            }
        }

        private void BaseForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            activityTimer?.Stop();
            activityTimer?.Dispose();
        }

        // Метод для обновления таймаута (можно вызвать из любой формы)
        public void UpdateInactivityTimeout(int newTimeout)
        {
            timeoutSeconds = newTimeout;
            AppConfigSettings.UpdateInactivityTimeout(newTimeout);
        }

        // Свойство для проверки статуса
        public bool IsLocked => isLocked;
    }
}