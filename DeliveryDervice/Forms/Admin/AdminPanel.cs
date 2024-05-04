using DeliveryDervice.Forms.Admin;
using SchoolCanteen.Data;
using System;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;

namespace DeliveryDervice.Forms
{
    public partial class AdminPanel : Form
    {
        private DatabaseManager databaseManager;
        public AdminPanel()
        {
            InitializeComponent();
            databaseManager = new DatabaseManager();

        }
        private void AdminPanel_Load(object sender, EventArgs e)
        {
            FillClients();
            FillEmployees();
            FillOrders();
            FillProducts();

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }
        public void FillClients()
        {
            string query = @"
                            SELECT Клиенты.КодКлиента as [Код], Пользователи.ФИО as [Имя], Клиенты.Контакт as [Контакт]
                            FROM Клиенты
                            INNER JOIN Пользователи ON Клиенты.КодПользователя = Пользователи.Код;
                        ";

            DataTable result = databaseManager.GetData(query);
            dataGridView2.DataSource = result;
            dataGridView2.Columns["Код"].Width = 60;
        }



        public void FillProducts()
        {
            string query = "SELECT Код as [Код], Название as [Название], Цена as [Цена], Количество as [Количество] " +
                           "FROM Товары;";
            DataTable result = databaseManager.GetData(query);
            dataGridView3.DataSource = result;
            dataGridView3.Columns["Код"].Width = 60;
            dataGridView3.Columns["Название"].Width = 150;
            dataGridView3.Columns["Цена"].Width = 100;
            dataGridView3.Columns["Количество"].Width = 100;
        }

        public void FillEmployees()
        {
            string query = @"
                            SELECT Сотрудники.Код as [Код], Пользователи.ФИО as [Имя], Сотрудники.Телефон as [Телефон], Роли.Название as [Роль]
                            FROM (Сотрудники
                            INNER JOIN Пользователи ON Сотрудники.КодПользователя = Пользователи.Код)
                            INNER JOIN Роли ON Сотрудники.КодРоли = Роли.Код;
                        ";

            DataTable result = databaseManager.GetData(query);
            dataGridView1.DataSource = result;
            dataGridView1.Columns["Код"].Width = 60;
            dataGridView1.Columns["Имя"].Width = 100;
            dataGridView1.Columns["Телефон"].Width = 100;
            dataGridView1.Columns["Роль"].Width = 100;
        }

        public void FillOrders()
        {
            // Определяем SQL-запрос для выборки данных о заказах
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
            СтатусыЗаказов ON Заказы.КодСтатуса = СтатусыЗаказов.Код;
    ";

            // Получаем данные в виде таблицы данных
            DataTable result = databaseManager.GetData(query);

            // Присваиваем таблицу данных источнику данных для dataGridView4
            dataGridView4.DataSource = result;

            // Настраиваем ширину колонок
            dataGridView4.Columns["Код"].Width = 60;
            dataGridView4.Columns["Клиент"].Width = 150;
            dataGridView4.Columns["Статус"].Width = 100;
            dataGridView4.Columns["Дата заказа"].Width = 100;
            dataGridView4.Columns["Адрес доставки"].Width = 200;
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

            dataGridView5.DataSource = result;

            dataGridView5.Columns["Код"].Width = 60;
            dataGridView5.Columns["Название товара"].Width = 150;
            dataGridView5.Columns["Количество"].Width = 100;
            dataGridView5.Columns["Цена за единицу"].Width = 100;
            dataGridView5.Columns["Общая стоимость"].Width = 120;
        }


        private void dataGridView4_SelectionChanged(object sender, System.EventArgs e)
        {
            if (dataGridView4.SelectedRows.Count > 0)
            {
                int orderId = Convert.ToInt32(dataGridView4.SelectedRows[0].Cells["Код"].Value);
                FillOrderDetails(orderId);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new CreateEmployeeForm(this).ShowDialog();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (dataGridView3.SelectedRows.Count > 0)
            {
                int productCode = Convert.ToInt32(dataGridView3.SelectedRows[0].Cells["Код"].Value);

                DialogResult result = MessageBox.Show($"Вы уверены, что хотите удалить товар с кодом {productCode}?",
                                                      "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    DeleteProduct(productCode);

                    FillProducts();
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите строку для удаления.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DeleteProduct(int productCode)
        {
            string query = "DELETE FROM Товары WHERE Код = @productCode;";

            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("@productCode", productCode)
            };

            databaseManager.ExecuteCommand(query, parameters);
        }

        private void DeleteEmployee(int empCode)
        {
            string query = "DELETE FROM Сотрудники WHERE Код = @empCode;";

            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("@empCode", empCode)
            };

            databaseManager.ExecuteCommand(query, parameters);
        }

        private void DeleteOrder(int orderCode)
        {
            string query = "DELETE FROM Заказы WHERE Код = @orderCode;";

            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("@orderCode", orderCode)
            };

            databaseManager.ExecuteCommand(query, parameters);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int empCode = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Код"].Value);

                DialogResult result = MessageBox.Show($"Вы уверены, что хотите удалить сотрудника с кодом {empCode}?",
                                                      "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    DeleteEmployee(empCode);

                    FillEmployees();
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите строку для удаления.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (dataGridView4.SelectedRows.Count > 0)
            {
                int orderCode = Convert.ToInt32(dataGridView4.SelectedRows[0].Cells["Код"].Value);

                string orderStatus = dataGridView4.SelectedRows[0].Cells["Статус"].Value.ToString();


                if (orderStatus == "Отменено" || orderStatus == "Получено")
                {
                    DialogResult result = MessageBox.Show($"Вы уверены, что хотите удалить заказ с кодом {orderCode}?",
                                                          "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        DeleteOrder(orderCode);

                        FillOrders();
                    }
                }
                else
                {
                    MessageBox.Show("Вы можете удалить только заказы со статусом 'Отменено' или 'Получено'.",
                                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заказ для удаления.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            new CreateProductForm(this).ShowDialog();
        }

        private void dataGridView3_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView3.SelectedRows.Count > 0)
            {
                int productCode = Convert.ToInt32(dataGridView3.SelectedRows[0].Cells["Код"].Value);

                byte[] photoData = GetProductPhoto(productCode);

                if (photoData != null)
                {
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(photoData))
                    {
                        pictureBox1.Image = Image.FromStream(ms);
                    }
                }
                else
                {
                    pictureBox1.Image = null;
                }
            }
            else
            {
                pictureBox1.Image = null;
            }
        }

        private byte[] GetProductPhoto(int productCode)
        {
            string query = "SELECT Фото FROM Товары WHERE Код = @productCode;";

            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("@productCode", productCode)
            };

            DataTable result = databaseManager.GetData(query, parameters);

            if (result.Rows.Count > 0)
            {
                return result.Rows[0]["Фото"] as byte[];
            }

            return null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView3.SelectedRows.Count > 0)
            {
                int productId = Convert.ToInt32(dataGridView3.SelectedRows[0].Cells["Код"].Value);

                UpdateProductForm updateProductForm = new UpdateProductForm(productId, this);

                updateProductForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите товар для редактирования.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox2.Text;

            string query = @"
                            SELECT 
                                Код AS [Код],
                                Название AS [Название],
                                Цена AS [Цена],
                                Количество AS [Количество]
                            FROM 
                                Товары
                            WHERE 
                                Название LIKE @searchText;
                        ";

            OleDbParameter[] parameters = new OleDbParameter[]
            {
                 new OleDbParameter("@searchText", "%" + searchText + "%")
            };

            DataTable result = databaseManager.GetData(query, parameters);

            dataGridView3.DataSource = result;

            dataGridView3.Columns["Код"].Width = 60;
            dataGridView3.Columns["Название"].Width = 150;
            dataGridView3.Columns["Цена"].Width = 100;
            dataGridView3.Columns["Количество"].Width = 100;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int empId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Код"].Value);

                EditEmployeeForm updateProductForm = new EditEmployeeForm(this, empId);

                updateProductForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите сотрудника для редактирования.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
