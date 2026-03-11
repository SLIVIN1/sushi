using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public static class DirectorState
    {
        public static DateTime DateFrom = DateTime.Today;
        public static DateTime DateTo = DateTime.Today;
        public static string OrderId = "";
        public static int StatusIndex = 0; // для comboBox1
        public static int SortIndex = 0;
    }
}
