using SchoolCanteen.Data;
using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace DeliveryDervice.Forms.Admin
{
    public partial class EditEmployeeForm : Form
    {
        private DatabaseManager databaseManager;
        private AdminPanel adminPanel;
        private int employeeId;

        public EditEmployeeForm(AdminPanel adminPanel, int employeeId)
        {
            InitializeComponent();
            this.adminPanel = adminPanel;
            this.employeeId = employeeId;
            databaseManager = new DatabaseManager();

            // Заполнение ComboBox ролями
            LoadRoles();

            // Заполнение данных о сотруднике
            FillData();
        }

        private void LoadRoles()
        {
            string query = "SELECT Код, Название FROM Роли;";
            DataTable result = databaseManager.GetData(query);

            // Заполнение ComboBox ролями
            comboBoxRoles.DataSource = result;
            comboBoxRoles.DisplayMember = "Название";
            comboBoxRoles.ValueMember = "Код";
        }

        private void FillData()
        {
            // Запрос для получения данных о сотруднике
            string query = @"
                SELECT Пользователи.ФИО, Пользователи.Логин, Пользователи.Пароль, Сотрудники.Телефон, Сотрудники.КодРоли
                FROM Сотрудники
                INNER JOIN Пользователи ON Сотрудники.КодПользователя = Пользователи.Код
                WHERE Сотрудники.Код = @EmployeeId;
            ";

            OleDbParameter[] parameters =
            {
                new OleDbParameter("@EmployeeId", employeeId)
            };

            // Получаем данные о сотруднике из базы данных
            DataTable result = databaseManager.GetData(query, parameters);

            if (result.Rows.Count > 0)
            {
                // Если данные найдены, заполняем поля формы
                DataRow row = result.Rows[0];
                textBoxFIO.Text = row["ФИО"].ToString();
                textBoxLogin.Text = row["Логин"].ToString();
                textBoxPassword.Text = row["Пароль"].ToString();
                textBoxPhone.Text = row["Телефон"].ToString();
                comboBoxRoles.SelectedValue = Convert.ToInt32(row["КодРоли"]);
            }
            else
            {
                // Если данные не найдены, выводим сообщение
                MessageBox.Show("Сотрудник не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void UpdateUserData(string fio, string login, string password)
        {
            // Запрос для обновления данных о пользователе
            string query = @"
                UPDATE Пользователи
                SET ФИО = @FIO, Логин = @Login, Пароль = @Password
                WHERE Код IN (SELECT КодПользователя FROM Сотрудники WHERE Код = @EmployeeId);
            ";

            // Параметры запроса
            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("@FIO", fio),
                new OleDbParameter("@Login", login),
                new OleDbParameter("@Password", password),
                new OleDbParameter("@EmployeeId", employeeId)
            };

            // Выполняем запрос для обновления данных о пользователе
            databaseManager.ExecuteCommand(query, parameters);
        }

        private void UpdateEmployeeData(int roleCode, string phone)
        {
            // Запрос для обновления данных о сотруднике
            string query = @"
                UPDATE Сотрудники
                SET КодРоли = @RoleCode, Телефон = @Phone
                WHERE Код = @EmployeeId;
            ";

            // Параметры запроса
            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("@RoleCode", roleCode),
                new OleDbParameter("@Phone", phone),
                new OleDbParameter("@EmployeeId", employeeId)
            };

            // Выполняем запрос для обновления данных о сотруднике
            databaseManager.ExecuteCommand(query, parameters);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Получаем данные из полей формы
            string fio = textBoxFIO.Text;
            string login = textBoxLogin.Text;
            string password = textBoxPassword.Text;
            int roleCode = Convert.ToInt32(comboBoxRoles.SelectedValue);
            string phone = textBoxPhone.Text;

            // Проверяем заполненность всех полей
            if (string.IsNullOrEmpty(fio) || string.IsNullOrEmpty(login) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Обновляем данные о пользователе
                UpdateUserData(fio, login, password);

                // Обновляем данные о сотруднике
                UpdateEmployeeData(roleCode, phone);

                // Обновляем список сотрудников в AdminPanel
                adminPanel.FillEmployees();

                // Закрываем форму после успешного обновления данных
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                // Обработка ошибок при обновлении данных
                MessageBox.Show($"Ошибка при обновлении данных сотрудника: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
