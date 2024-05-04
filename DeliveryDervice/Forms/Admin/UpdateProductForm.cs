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

namespace DeliveryDervice.Forms.Admin
{
    public partial class UpdateProductForm : Form
    {
        private DatabaseManager databaseManager;
        private AdminPanel adminPanel;
        private int productId;
        public UpdateProductForm(int productId, AdminPanel adminPanel)
        {
            InitializeComponent();

            databaseManager = new DatabaseManager();
            this.productId = productId;
            this.adminPanel = adminPanel;

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            // Заполните поля данными о товаре по идентификатору
            LoadProductData();
        }

        private void LoadProductData()
        {
            string query = "SELECT Название, Цена, Количество, Фото FROM Товары WHERE Код = @productId;";
            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("@productId", productId)
            };

            DataTable result = databaseManager.GetData(query, parameters);

            if (result.Rows.Count > 0)
            {
                DataRow row = result.Rows[0];

                // Заполнение полей формы данными о товаре
                textBox1.Text = row["Название"].ToString();
                textBox3.Text = Convert.ToDecimal(row["Цена"]).ToString();
                textBox4.Text = row["Количество"].ToString();

                byte[] photoData = row["Фото"] as byte[];
                if (photoData != null)
                {
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(photoData))
                    {
                        pictureBox1.Image = Image.FromStream(ms);
                    }
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string productName = textBox1.Text;
            decimal productPrice = Convert.ToDecimal(textBox3.Text);
            int productQuantity = Convert.ToInt32(textBox4.Text);

            // Предполагается, что productId уже передан в форму
            int productId = this.productId; // Это должно быть передано из другой части программы

            byte[] photoData = null;
            if (pictureBox1.Image != null)
            {
                // Убедитесь, что pictureBox1.Image содержит изображение перед сохранением
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    try
                    {
                        // Попробуйте сохранить изображение в потоке
                        pictureBox1.Image.Save(ms, pictureBox1.Image.RawFormat);
                        photoData = ms.ToArray();
                    }
                    catch (Exception ex)
                    {
                        // Если возникает ошибка, обработайте ее (например, выведите сообщение пользователю)
                        MessageBox.Show($"Ошибка сохранения изображения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                // Если изображение отсутствует, установите photoData в null
                photoData = null;
            }


            string query = @"
        UPDATE Товары
        SET Название = @productName, Цена = @productPrice, Количество = @productQuantity, Фото = @photoData
        WHERE Код = @productId;";

            // Параметр для @photoData
            OleDbParameter photoDataParameter;
            if (photoData != null)
            {
                photoDataParameter = new OleDbParameter("@photoData", photoData);
            }
            else
            {
                photoDataParameter = new OleDbParameter("@photoData", DBNull.Value);
            }

            // Создаем параметры запроса
            OleDbParameter[] parameters = new OleDbParameter[]
            {
        new OleDbParameter("@productName", productName),
        new OleDbParameter("@productPrice", productPrice),
        new OleDbParameter("@productQuantity", productQuantity),
        photoDataParameter,
        new OleDbParameter("@productId", productId) // Параметр для productId
            };

            // Выполняем запрос
            databaseManager.ExecuteCommand(query, parameters);

            // Обновляем данные о товарах в AdminPanel
            adminPanel.FillProducts();

            // Закрываем форму
            this.DialogResult = DialogResult.OK;
            this.Close();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png|All Files|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.Image = Image.FromFile(openFileDialog.FileName);
                }
            }
        }
    }
}
