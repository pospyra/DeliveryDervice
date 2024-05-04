using DeliveryDervice.Forms;
using DeliveryDervice.Forms.Courier;
using DeliveryDervice.Forms.Storekeeper;
using System;
using System.Windows.Forms;

namespace DeliveryDervice
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AdminPanel());
        }
    }
}
