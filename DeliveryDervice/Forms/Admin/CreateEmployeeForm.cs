using SchoolCanteen.Data;
using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace DeliveryDervice.Forms.Admin
{
    public partial class CreateEmployeeForm : Form
    {
        private DatabaseManager databaseManager;
        private AdminPanel adminPanel;
        public CreateEmployeeForm(AdminPanel adminPanel)
        {
            InitializeComponent();

            this.adminPanel = adminPanel;
            databaseManager = new DatabaseManager();

            // Заполнение ComboBox ролями
            LoadRoles();
        }

        private void LoadRoles()
        {
            string query = "SELECT Код, Название FROM Роли;";
            DataTable result = databaseManager.GetData(query);

            // Заполнение ComboBox
            comboBoxRoles.DataSource = result;
            comboBoxRoles.DisplayMember = "Название";
            comboBoxRoles.ValueMember = "Код";
        }

        private int SaveEmployeeToDatabase(int roleCode, string phone, int userId)
        {
            // Формирование запроса для добавления сотрудника
            string query = "INSERT INTO Сотрудники (КодРоли, Телефон, КодПользователя) " +
                           "VALUES (@roleCode, @phone, @userId);";

            // Создание параметров для запроса
            OleDbParameter[] parameters = new OleDbParameter[]
            {
        new OleDbParameter("@roleCode", roleCode),
        new OleDbParameter("@phone", phone),
        new OleDbParameter("@userId", userId)
            };

            // Выполнение запроса для добавления сотрудника
            databaseManager.ExecuteCommand(query, parameters);

            // Запрос для получения максимального значения идентификатора в таблице Сотрудники
            string selectQuery = "SELECT MAX(Код) FROM Сотрудники;";

            // Выполнение запроса для получения максимального идентификатора
            object result = databaseManager.ExecuteScalar(selectQuery);
            int employeeId = Convert.ToInt32(result);

            // Возвращение идентификатора сотрудника
            return employeeId;
        }


        private int AddUser(string fio, string login, string password)
        {
            // Запрос для добавления пользователя
            string insertQuery = @"
        INSERT INTO Пользователи (ФИО, Логин, Пароль)
        VALUES (@fio, @login, @password);
    ";

            // Параметры запроса
            OleDbParameter[] parameters = new OleDbParameter[]
            {
        new OleDbParameter("@fio", fio),
        new OleDbParameter("@login", login),
        new OleDbParameter("@password", password)
            };

            // Выполнение запроса для добавления пользователя
            databaseManager.ExecuteCommand(insertQuery, parameters);

            // Запрос для получения идентификатора последней вставленной записи
            string selectQuery = "SELECT MAX(Код) FROM Пользователи;";

            // Получение идентификатора пользователя
            object result = databaseManager.ExecuteScalar(selectQuery);

            // Преобразование результата в целое число
            int userId = Convert.ToInt32(result);

            // Возвращение идентификатора пользователя
            return userId;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            // Получение данных о сотруднике
            string fio = textBoxFIO.Text;
            string login = textBoxLogin.Text;
            string password = textBoxPassword.Text;
            int roleCode = Convert.ToInt32(comboBoxRoles.SelectedValue);
            string phone = textBoxPhone.Text;

            // Проверка заполненности всех полей
            if (string.IsNullOrEmpty(fio) || string.IsNullOrEmpty(login) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Добавление пользователя и получение идентификатора пользователя
                int userId = AddUser(fio, login, password);

                // Добавление сотрудника и получение идентификатора сотрудника
                int employeeId = SaveEmployeeToDatabase(roleCode, phone, userId);

                // Обновление списка сотрудников в AdminPanel
                adminPanel.FillEmployees();

                // Закрытие формы после успешного добавления сотрудника
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                // Обработка исключений при добавлении сотрудника
                MessageBox.Show($"Ошибка при добавлении сотрудника: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
