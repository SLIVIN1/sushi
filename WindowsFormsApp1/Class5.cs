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
        private Form notificationForm = null;

        public BaseForm()
        {
            timeoutSeconds = AppConfigSettings.InactivityTimeoutSeconds;
            enabled = AppConfigSettings.EnableInactivityLock;

            // Устанавливаем форму по центру экрана
            this.StartPosition = FormStartPosition.CenterScreen;

            if (enabled)
            {
                activityTimer = new Timer();
                activityTimer.Interval = 1000;
                activityTimer.Tick += CheckInactivity;

                this.MouseMove += (s, e) => ResetTimer();
                this.KeyPress += (s, e) => ResetTimer();
                this.MouseClick += (s, e) => ResetTimer();
                this.KeyDown += (s, e) => ResetTimer();

                this.Load += (s, e) =>
                {
                    lastActivityTime = DateTime.Now;
                    activityTimer.Start();
                };

                this.FormClosed += (s, e) =>
                {
                    activityTimer?.Stop();
                    activityTimer?.Dispose();
                    CloseNotification();
                };
            }
        }

        private void ResetTimer()
        {
            lastActivityTime = DateTime.Now;
            CloseNotification();
        }

        private void CloseNotification()
        {
            if (notificationForm != null && !notificationForm.IsDisposed)
            {
                try
                {
                    notificationForm.Close();
                    notificationForm.Dispose();
                }
                catch { }
                notificationForm = null;
            }
        }

        private void CheckInactivity(object sender, EventArgs e)
        {
            if (!enabled || isLocked) return;

            TimeSpan inactiveTime = DateTime.Now - lastActivityTime;

            if (inactiveTime.TotalSeconds >= timeoutSeconds - 5 &&
                inactiveTime.TotalSeconds < timeoutSeconds &&
                notificationForm == null)
            {
                ShowWarning();
            }

            if (inactiveTime.TotalSeconds >= timeoutSeconds)
            {
                this.Invoke(new Action(() => {
                    ShowLoginForm();
                }));
            }
        }

        private void ShowWarning()
        {
            if (notificationForm != null) return;

            notificationForm = new Form
            {
                Text = "Предупреждение",
                Size = new System.Drawing.Size(300, 150),
                StartPosition = FormStartPosition.CenterParent, // По центру родительской формы
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                TopMost = true,
                ControlBox = false
            };

            Label lblMessage = new Label
            {
                Text = "⚠ Внимание!\n\nЧерез 5 секунд произойдет автоматический выход\nиз-за длительного бездействия.",
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Size = new System.Drawing.Size(280, 80),
                Location = new System.Drawing.Point(10, 10)
            };

            Button btnOk = new Button
            {
                Text = "Продолжить работу",
                Size = new System.Drawing.Size(150, 30),
                Location = new System.Drawing.Point(75, 80)
            };
            btnOk.Click += (s, args) => {
                lastActivityTime = DateTime.Now;
                CloseNotification();
            };

            notificationForm.Controls.Add(lblMessage);
            notificationForm.Controls.Add(btnOk);
            notificationForm.Show();
        }

        protected virtual void ShowLoginForm()
        {
            activityTimer.Stop();
            isLocked = true;
            CloseNotification();

            MessageBox.Show(
                "Вы были возвращены на экран регистрации из-за длительного бездействия.",
                "Информация",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // ИСПРАВЛЕНО: Было new A(), теперь new LoginForm()
            using (var loginForm = new A())
            {
                bool wasVisible = this.Visible;
                this.Hide();

                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    isLocked = false;
                    lastActivityTime = DateTime.Now;

                    if (wasVisible)
                    {
                        this.Show();
                        MessageBox.Show("Добро пожаловать обратно!", "Успешно",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    activityTimer.Start();
                }
                else
                {
                    Application.Exit();
                }
            }
        }

        public void UpdateInactivityTimeout(int newTimeout)
        {
            timeoutSeconds = newTimeout;
            AppConfigSettings.UpdateInactivityTimeout(newTimeout);
        }

        public bool IsLocked => isLocked;

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // BaseForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "BaseForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.BaseForm_Load);
            this.ResumeLayout(false);

        }

        private void BaseForm_Load(object sender, EventArgs e)
        {
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        }
    }
}