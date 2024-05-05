using SchoolCanteen.Data;
using System;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;

namespace DeliveryDervice.Forms.Admin
{
    public partial class CreateProductForm : Form
    {
        private AdminPanel adminPanel;
        private DatabaseManager databaseManager;
        public CreateProductForm(AdminPanel adminPanel)
        {
            InitializeComponent();
            databaseManager = new DatabaseManager();
            this.adminPanel = adminPanel;

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                string productName = textBox1.Text;
                decimal productPrice = Convert.ToDecimal(textBox3.Text);
                int productQuantity = Convert.ToInt32(textBox4.Text);

                byte[] photoData = null;
                if (pictureBox1.Image != null)
                {
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        pictureBox1.Image.Save(ms, pictureBox1.Image.RawFormat);
                        photoData = ms.ToArray();
                    }
                }

                string query = @"
                                INSERT INTO Товары (Название, Цена, Количество, Фото)
                                VALUES (@productName, @productPrice, @productQuantity, @photoData);";

                OleDbParameter[] parameters = new OleDbParameter[]
                {
                    new OleDbParameter("@productName", productName),
                    new OleDbParameter("@productPrice", productPrice),
                    new OleDbParameter("@productQuantity", productQuantity),
                    new OleDbParameter("@photoData", photoData ?? (object)DBNull.Value) 
                };

                // Выполнение запроса для вставки нового товара
                databaseManager.ExecuteCommand(query, parameters);

                // Обновление данных о товарах в AdminPanel
                adminPanel.FillProducts();

                // Закрытие формы после успешного добавления товара
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrEmpty(textBox1.Text) ||
                string.IsNullOrEmpty(textBox3.Text) ||
                string.IsNullOrEmpty(textBox4.Text))
            {
                return false;
            }

            return true;
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
