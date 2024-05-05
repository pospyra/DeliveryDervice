using DeliveryDervice.Data;
using SchoolCanteen.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DeliveryDervice.Forms
{
    public partial class MyOrdersForm : Form
    {
        private DatabaseManager databaseManager;
        public MyOrdersForm()
        {
            InitializeComponent();
            databaseManager = new DatabaseManager();
            FillOrders();
        }

        public void FillOrders()
        {
            int currentUserId = DataStorage.CurrentUserId;

            string query = @"
                            SELECT 
                                Заказы.Код AS [Код], 
                                Пользователи.ФИО AS [Клиент],  
                                СтатусыЗаказов.Название AS [Статус], 
                                Заказы.ДатаЗаказа AS [Дата заказа], 
                                Заказы.АдресДоставки AS [Адрес доставки]
                            FROM 
                                ((Заказы
                                INNER JOIN Клиенты ON Заказы.КодКлиента = Клиенты.КодКлиента)
                                INNER JOIN Пользователи ON Клиенты.КодПользователя = Пользователи.Код)
                            INNER JOIN 
                                СтатусыЗаказов ON Заказы.КодСтатуса = СтатусыЗаказов.Код
                            WHERE Заказы.КодКлиента = @currentUserId;";

            OleDbParameter[] parameters =
            {
                new OleDbParameter("@currentUserId", currentUserId)
            };

            DataTable result = databaseManager.GetData(query, parameters);

            dataGridView1.DataSource = result;

            dataGridView1.Columns["Код"].Width = 60;
            dataGridView1.Columns["Клиент"].Width = 150;
            dataGridView1.Columns["Статус"].Width = 100;
            dataGridView1.Columns["Дата заказа"].Width = 100;
            dataGridView1.Columns["Адрес доставки"].Width = 200;
        }

        public void FillOrderDetails(int orderId)
        {
            string query = @"
                        SELECT
                            ДеталиЗаказов.Код as [Код],
                            Товары.Название as [Название товара],
                            ДеталиЗаказов.Количество as [Количество],
                            Товары.Цена as [Цена за единицу],
                            (ДеталиЗаказов.Количество * Товары.Цена) as [Общая стоимость]
                        FROM
                            ДеталиЗаказов
                        INNER JOIN
                            Товары ON ДеталиЗаказов.КодТовара = Товары.Код
                        WHERE
                            ДеталиЗаказов.КодЗаказа = @orderId;
                    ";

            OleDbParameter[] parameters = new OleDbParameter[]
            {
                 new OleDbParameter("@orderId", orderId)
            };

            DataTable result = databaseManager.GetData(query, parameters);

            dataGridView2.DataSource = result;

            dataGridView2.Columns["Код"].Width = 60;
            dataGridView2.Columns["Название товара"].Width = 150;
            dataGridView2.Columns["Количество"].Width = 100;
            dataGridView2.Columns["Цена за единицу"].Width = 100;
            dataGridView2.Columns["Общая стоимость"].Width = 120;
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int orderId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Код"].Value);
                FillOrderDetails(orderId);
            }
        }
    }
}

