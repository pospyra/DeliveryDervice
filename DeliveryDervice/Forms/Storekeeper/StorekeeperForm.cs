using DeliveryDervice.Data;
using SchoolCanteen.Data;
using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace DeliveryDervice.Forms.Storekeeper
{
    public partial class StorekeeperForm : Form
    {
        private DatabaseManager databaseManager;
        public StorekeeperForm()
        {
            InitializeComponent();
            databaseManager = new DatabaseManager();
            FillOrders(checkBox1.Checked);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            FillOrders(checkBox1.Checked);
        }

        public void FillOrders(bool isShowAllOrders)
        {
            string query;

            if (isShowAllOrders)
            {
                query = @"
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
                                СтатусыЗаказов ON Заказы.КодСтатуса = СтатусыЗаказов.Код;
                        ";
            }
            else
            {
                query = @"
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
                        WHERE Заказы.КодСтатуса = 1;
                    ";
            }

            DataTable result = databaseManager.GetData(query);

            dataGridView1.DataSource = result;

            dataGridView1.Columns["Код"].Width = 60;
            dataGridView1.Columns["Клиент"].Width = 150;
            dataGridView1.Columns["Статус"].Width = 100;
            dataGridView1.Columns["Дата заказа"].Width = 100;
            dataGridView1.Columns["Адрес доставки"].Width = 200;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {

                // Получаем идентификатор заказа из выбранной строки
                int orderId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Код"].Value);

                // Код статуса "Отправлено" в базе данных (код 2)
                int statusShipped = 2;

                // Обновление статуса заказа в базе данных
                string updateStatusQuery = @"
                                            UPDATE Заказы
                                            SET КодСтатуса = @StatusShipped
                                            WHERE Код = @OrderId;
                                        ";

                OleDbParameter[] parameters =
                {
                    new OleDbParameter("@StatusShipped", statusShipped),
                    new OleDbParameter("@OrderId", orderId)
                };

                // Выполняем запрос для обновления статуса заказа
                databaseManager.ExecuteCommand(updateStatusQuery, parameters);

                // Добавляем запись о сотруднике, который работал над заказом
                AddEmployeeOrderRecord(orderId);

                MessageBox.Show("Статус заказа успешно изменен на 'Отправлено'.");

                // Обновляем список заказов
                FillOrders(checkBox1.Checked);

            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заказ для изменения статуса.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddEmployeeOrderRecord(int orderId)
        {
            // Получаем текущего пользователя (сотрудника)
            int currentEmployeeId = DataStorage.CurrentUserId;

            // Получаем роль сотрудника из таблицы Сотрудники по его идентификатору
            string employeeRole = GetEmployeeRole(currentEmployeeId);

            // SQL-запрос для добавления записи о сотруднике, который работал над заказом
            string insertEmployeeOrderQuery = @"
                                                INSERT INTO СотрудникиЗаказов (КодЗаказа, КодСотрудника, РольСотрудника)
                                                VALUES (@OrderId, @EmployeeId, @EmployeeRole);
                                            ";

            // Параметры для запроса
            OleDbParameter[] parameters =
            {
                new OleDbParameter("@OrderId", orderId),
                new OleDbParameter("@EmployeeId", currentEmployeeId),
                new OleDbParameter("@EmployeeRole", employeeRole)
            };

            // Выполняем запрос на добавление записи о сотруднике, который работал над заказом
            databaseManager.ExecuteCommand(insertEmployeeOrderQuery, parameters);
        }

        private string GetEmployeeRole(int employeeId)
        {
            // SQL-запрос для объединения таблиц Сотрудники и Роли по КодРоли и извлечения названия роли
            string query = @"
                            SELECT Роли.Название
                            FROM Сотрудники
                            INNER JOIN Роли ON Сотрудники.КодРоли = Роли.Код
                            WHERE Сотрудники.Код = @EmployeeId;
                        ";

            // Параметр для запроса
            OleDbParameter[] parameters =
            {
                new OleDbParameter("@EmployeeId", employeeId)
            };

            // Выполняем запрос и получаем результат (название роли)
            object result = databaseManager.ExecuteScalar(query, parameters);

            // Проверяем, что результат не равен null, и возвращаем его как строку
            return result?.ToString();
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
