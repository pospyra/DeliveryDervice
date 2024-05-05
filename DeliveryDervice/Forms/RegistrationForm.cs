using SchoolCanteen.Data;
using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace DeliveryDervice.Forms
{
    public partial class RegistrationForm : Form
    {
        private DatabaseManager databaseManager;

        public RegistrationForm()
        {
            InitializeComponent();
            databaseManager = new DatabaseManager();
        }

        private int AddUser(string fio, string login, string password)
        {
            // Проверяем, существует ли уже пользователь с таким логином
            if (IsLoginExists(login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует. Пожалуйста, выберите другой логин.",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0; 
            }

            // Запрос для добавления нового пользователя
            string insertQuery = @"
                INSERT INTO Пользователи (ФИО, Логин, Пароль)
                VALUES (@fio, @login, @password);
            ";

            // Параметры для запроса
            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("@fio", fio),
                new OleDbParameter("@login", login),
                new OleDbParameter("@password", password)
            };

            // Выполняем запрос для добавления пользователя
            databaseManager.ExecuteCommand(insertQuery, parameters);

            // Запрос для получения идентификатора добавленного пользователя
            string selectQuery = "SELECT MAX(Код) FROM Пользователи;";

            // Получаем идентификатор пользователя
            object result = databaseManager.ExecuteScalar(selectQuery);
            int userId = Convert.ToInt32(result);

            // Возвращаем идентификатор пользователя
            return userId;
        }

        private bool IsLoginExists(string login)
        {
            // Запрос для проверки существования пользователя с данным логином
            string query = "SELECT COUNT(*) FROM Пользователи WHERE Логин = @login;";

            // Параметр для запроса
            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("@login", login)
            };

            // Выполняем запрос и получаем результат
            object result = databaseManager.ExecuteScalar(query, parameters);
            int count = Convert.ToInt32(result);

            // Если количество пользователей с таким логином больше 0, возвращаем true
            return count > 0;
        }

        private void AddClient(int userId, string contact)
        {
            // Запрос для добавления нового клиента
            string insertQuery = @"
                                    INSERT INTO Клиенты (КодПользователя, Контакт)
                                    VALUES (@userId, @contact);
                                ";

            // Параметры для запроса
            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("@userId", userId),
                new OleDbParameter("@contact", contact)
            };

            // Выполняем запрос для добавления клиента
            databaseManager.ExecuteCommand(insertQuery, parameters);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Получаем данные из полей формы
            string fio = textBoxFIO.Text;
            string login = textBoxLogin.Text;
            string password = textBoxPassword.Text;
            string contact = textBoxContact.Text;

            // Проверяем заполненность всех полей
            if (string.IsNullOrEmpty(fio) || string.IsNullOrEmpty(login) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(contact))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Добавляем пользователя и получаем его идентификатор
                int userId = AddUser(fio, login, password);
                if (userId == 0)
                {
                    return;
                }

                AddClient(userId, contact);

                // Закрываем форму после успешного добавления клиента
                MessageBox.Show("Вы успешно зарегистрированы.");

                this.Hide();
                new AuthForm().ShowDialog();


            }
            catch (Exception ex)
            {
                // Обработка исключений при добавлении клиента
                MessageBox.Show($"Ошибка при регистрации клиента: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            new AuthForm().ShowDialog();

        }
    }
}
