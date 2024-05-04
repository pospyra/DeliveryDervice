using DeliveryDervice.Data;
using SchoolCanteen.Data;
using System;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;

namespace DeliveryDervice.Forms
{
    public partial class MainPanel : Form
    {
        private DatabaseManager databaseManager;
        decimal totalAmount = 0;
        public MainPanel()
        {
            InitializeComponent();
            databaseManager = new DatabaseManager();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            textBox2.Text = 1.ToString();
        }

        private void MainPanel_Load(object sender, EventArgs e)
        {
            InitializeDataGridView2();
            FillProducts();
        }

        private void InitializeDataGridView2()
        {
            dataGridView2.Columns.Add("Код", "Номер товара");
            dataGridView2.Columns.Add("Название", "Название товара");
            dataGridView2.Columns.Add("Цена", "Цена");
            dataGridView2.Columns.Add("Количество", "Количество");
            dataGridView2.Columns.Add("Всего", "Всего");


            dataGridView2.Columns["Код"].Width = 60;
            dataGridView2.Columns["Название"].Width = 150;
            dataGridView2.Columns["Цена"].Width = 100;
            dataGridView2.Columns["Количество"].Width = 100;
            dataGridView2.Columns["Всего"].Width = 100;
        }

        public void FillProducts()
        {
            string query = "SELECT Код AS [Код], Название AS [Название], Цена AS [Цена], Количество AS [Количество] " +
                           "FROM Товары;";

            DataTable result = databaseManager.GetData(query);

            dataGridView1.DataSource = result;

            dataGridView1.Columns["Код"].Width = 60;
            dataGridView1.Columns["Название"].Width = 150;
            dataGridView1.Columns["Цена"].Width = 100;
            dataGridView1.Columns["Количество"].Width = 100;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Пожалуйста, выберите количество", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dataGridView1.SelectedRows.Count > 0)
            {
                int count = int.Parse(textBox2.Text);

                int remainingQuantity = int.Parse(dataGridView1.SelectedRows[0].Cells["Количество"].Value.ToString());

                if (count <= 0 || count > remainingQuantity)
                {
                    label4.Text = $"Осталось {remainingQuantity}";
                    return;
                }
                else
                {
                    label4.Text = string.Empty;
                }

                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                int productId = Convert.ToInt32(selectedRow.Cells["Код"].Value);
                string productName = selectedRow.Cells["Название"].Value.ToString();
                decimal price = Convert.ToDecimal(selectedRow.Cells["Цена"].Value);

                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    if (row.Cells["Код"].Value != null && row.Cells["Код"].Value.ToString() == productId.ToString())
                    {
                        MessageBox.Show("Товар уже добавлен в заказ.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                decimal totalCost = price * count;

                dataGridView2.Rows.Add(productId, productName, price, count, totalCost);

                int newRemainingQuantity = remainingQuantity - count;
                dataGridView1.SelectedRows[0].Cells["Количество"].Value = newRemainingQuantity;

                CalculateTotalAmount();
            }
            else
            {
                MessageBox.Show("Выберите товар");
                return;
            }
        }

        private void CalculateTotalAmount()
        {
            // Инициализируем общую сумму
            totalAmount = 0;

            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.Cells["Цена"].Value != null && row.Cells["Количество"].Value != null)
                {
                    decimal price = Convert.ToDecimal(row.Cells["Цена"].Value);
                    int quantity = Convert.ToInt32(row.Cells["Количество"].Value);

                    totalAmount += price * quantity;
                }
            }

            label6.Text = $"Сумма к оплате: {totalAmount} руб.";
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                int selectedIndex = dataGridView2.SelectedRows[0].Index;

                if (selectedIndex != -1 && !dataGridView2.Rows[selectedIndex].IsNewRow)
                {
                    decimal price = Convert.ToDecimal(dataGridView2.SelectedRows[selectedIndex].Cells["Цена"].Value);
                    int quantity = Convert.ToInt32(dataGridView2.SelectedRows[selectedIndex].Cells["Количество"].Value);
                    int productId = Convert.ToInt32(dataGridView2.SelectedRows[selectedIndex].Cells["Код"].Value);

                    totalAmount -= price * quantity;

                    label6.Text = $"Сумма к оплате: {totalAmount} руб.";

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (Convert.ToInt32(row.Cells["Код"].Value) == productId)
                        {
                            int currentQuantity = Convert.ToInt32(row.Cells["Количество"].Value);
                            row.Cells["Количество"].Value = currentQuantity + quantity;
                            break;
                        }
                    }

                    dataGridView2.Rows.RemoveAt(selectedIndex);
                    CalculateTotalAmount();
                }
                else
                {
                    MessageBox.Show("Выберите строку заказа для удаления.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Выберите строку заказа для удаления.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (dataGridView2.Rows.Count == 1 && dataGridView2.Rows[0].IsNewRow)
            {
                MessageBox.Show("Заказ пустой.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("Пожалуйста, заполните адрес.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; 
            }


            SaveOrderDetailsToDatabase();

            dataGridView2.Rows.Clear();
        }

        private int GetRemainingProductQuantity(int productId)
        {
            string query = "SELECT Количество FROM Товары WHERE Код = @productId;";

            OleDbParameter[] parameters =
            {
                new OleDbParameter("@productId", productId)
            };

            object result = databaseManager.ExecuteScalar(query, parameters);

            return Convert.ToInt32(result);
        }

        private int AddOrderAndGetId()
        {
            // Получаем текущее время и адрес доставки из поля textBox3
            DateTime orderTime = DateTime.Now;
            string deliveryAddress = textBox3.Text;

            // Замените значение на то, что соответствует вашей программе
            int clientId = 1;

            // Общая стоимость заказа
            decimal totalCost = totalAmount;

            // Формируем запрос для добавления заказа в таблицу Заказы
            string insertOrderQuery = @"
                                        INSERT INTO Заказы (КодКлиента, КодСтатуса, ДатаЗаказа, АдресДоставки, Сумма)
                                        VALUES (@ClientId, 1, @OrderTime, @DeliveryAddress, @TotalCost);
                                    ";

            // Параметры для запроса
            OleDbParameter[] parameters =
            {
                new OleDbParameter("@ClientId", OleDbType.Integer) { Value = clientId },
                new OleDbParameter("@OrderTime", OleDbType.Date) { Value = orderTime },
                new OleDbParameter("@DeliveryAddress", OleDbType.VarChar) { Value = deliveryAddress },
                new OleDbParameter("@TotalCost", OleDbType.Decimal) { Value = totalCost }
            };

            // Выполняем запрос вставки
            databaseManager.ExecuteCommand(insertOrderQuery, parameters);

            // Формируем запрос для получения последнего вставленного идентификатора
            string getLastInsertIdQuery = "SELECT DMAX('Код', 'Заказы')";

            // Выполняем запрос для получения идентификатора последней вставки
            object orderIdObj = databaseManager.ExecuteScalar(getLastInsertIdQuery);

            // Проверяем, что результат запроса не равен null
            if (orderIdObj == null)
            {
                MessageBox.Show("Не удалось получить идентификатор последнего вставленного заказа.");
                return -1;
            }

            // Преобразуем результат в целое число
            int orderId = Convert.ToInt32(orderIdObj);

            // Возвращаем идентификатор заказа
            return orderId;
        }



        private void SaveOrderDetailsToDatabase()
        {
            int orderId = AddOrderAndGetId();

            if (orderId > 0)
            {
                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    if (row.Cells["Код"].Value != null)
                    {
                        int productId = Convert.ToInt32(row.Cells["Код"].Value);
                        int orderedQuantity = Convert.ToInt32(row.Cells["Количество"].Value);

                        int availableQuantity = GetRemainingProductQuantity(productId);

                        if (orderedQuantity <= availableQuantity)
                        {
                            string insertOrderDetailQuery = @"
                                                               INSERT INTO ДеталиЗаказов (КодЗаказа, КодТовара, Количество)
                                                               VALUES (@OrderId, @ProductId, @Quantity);";

                            OleDbParameter[] detailParameters =
                            {
                                new OleDbParameter("@OrderId", orderId),
                                new OleDbParameter("@ProductId", productId),
                                new OleDbParameter("@Quantity", orderedQuantity)
                            };

                            databaseManager.ExecuteCommand(insertOrderDetailQuery, detailParameters);

                            UpdateProductQuantity(productId, orderedQuantity);
                        }
                        else
                        {
                            MessageBox.Show($"Недостаточно товара или превышен лимит. Доступно {availableQuantity} штук.",
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return; // Прекращаем выполнение метода
                        }
                    }
                }

                MessageBox.Show("Заказ оформлен.");
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении заказа.");
            }
        }
        private void UpdateProductQuantity(int productId, int orderedQuantity)
        {
            int remainingQuantity = GetRemainingProductQuantity(productId);

            int newRemainingQuantity = remainingQuantity - orderedQuantity;

            if (newRemainingQuantity < 0)
            {
                throw new InvalidOperationException($"Недостаточно товара с Код {productId} для заказа.");
            }

            string query = "UPDATE Товары SET Количество = @newRemainingQuantity WHERE Код = @productId;";

            OleDbParameter[] parameters =
            {
                new OleDbParameter("@newRemainingQuantity", newRemainingQuantity),
                new OleDbParameter("@productId", productId)
            };

            databaseManager.ExecuteCommand(query, parameters);
        }

        private void dataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView2.Columns["Количество"].Index)
            {

                DataGridViewRow row = dataGridView2.Rows[e.RowIndex];

                int productId = Convert.ToInt32(row.Cells["Код"].Value);
                int newQuantity = Convert.ToInt32(row.Cells["Количество"].Value);

                if (newQuantity == 0)
                {
                    MessageBox.Show($"Если вы хотите убрать товар из заказа, нажмите кнопку 'Удалить' ");
                    return;
                }

                decimal price = Convert.ToDecimal(row.Cells["Цена"].Value);

                decimal totalValue = price * newQuantity;
                row.Cells["Всего"].Value = totalValue;

                UpdateProductQuantityInDataGridView1(productId, newQuantity);

                CalculateTotalAmount();
            }
        }


        private void UpdateProductQuantityInDataGridView1(int productId, int newQuantity)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (Convert.ToInt32(row.Cells["Код"].Value) == productId)
                {
                    int currentQuantity = GetCurrentProductQuantityFromDB(productId);

                    int updatedQuantity = currentQuantity - newQuantity;

                    if (updatedQuantity < 0)
                    {
                        MessageBox.Show("Вы заказали больше товара, чем есть на складе. Пожалуйста, уменьшите количество.",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        return;
                    }

                    else
                    {
                        row.Cells["Количество"].Value = updatedQuantity;

                        break;
                    }
                }
            }
        }


        private int GetCurrentProductQuantityFromDB(int productId)
        {
            string query = "SELECT Количество FROM Товары WHERE Код = @productId;";

            OleDbParameter[] parameters =
            {
                new OleDbParameter("@productId", productId)
            };

            object result = databaseManager.ExecuteScalar(query, parameters);

            return Convert.ToInt32(result);
        }

        private void button4_MouseEnter(object sender, EventArgs e)
        {
            button4.BackColor = Color.White;
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int productCode = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Код"].Value);

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

        private void button4_Click(object sender, EventArgs e)
        {
            new MyOrdersForm().ShowDialog();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox1.Text;

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

            dataGridView1.DataSource = result;
                        
            dataGridView1.Columns["Код"].Width = 60;
            dataGridView1.Columns["Название"].Width = 150;
            dataGridView1.Columns["Цена"].Width = 100;
            dataGridView1.Columns["Количество"].Width = 100;
        }
    }
}