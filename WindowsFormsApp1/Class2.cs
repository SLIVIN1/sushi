// Class2.cs (исправленный + добавили флаг "заказ оформлен")
using System.Data;

namespace WindowsFormsApp1
{
    internal class Class2
    {
        public static DataTable CartTable;
        public static long CurrentOrderId = 0;

        // если true — заказ уже оформлен, кнопку button3 больше не нажимаем
        public static bool OrderCreated = false;

        // кнопки/контролы
        public static bool Button1Enabled = true;
        public static bool Button2Enabled = true;
        public static bool Button3Enabled = true;
        public static bool Button4Enabled = false;
        public static bool Button6Enabled = true;
        public static bool Button6Visible = false;
        public static bool NumericUpDown1Enabled = true;

        // поля клиента (текст)
        public static string CustomerName = "";
        public static string CustomerPhone = "";
        public static string CustomerAddress = "";

        // поля клиента (доступность)
        public static bool CustomerNameEnabled = true;
        public static bool CustomerPhoneEnabled = true;
        public static bool CustomerAddressEnabled = true;

        public static void ResetOrderState()
        {
            CartTable?.Clear();
            CurrentOrderId = 0;
            OrderCreated = false;

            Button1Enabled = true;
            Button2Enabled = true;
            Button3Enabled = true;
            Button4Enabled = false;
            Button6Enabled = true;
            Button6Visible = false;
            NumericUpDown1Enabled = true;

            CustomerName = "";
            CustomerPhone = "";
            CustomerAddress = "";

            CustomerNameEnabled = true;
            CustomerPhoneEnabled = true;
            CustomerAddressEnabled = true;
        }
    }
}
