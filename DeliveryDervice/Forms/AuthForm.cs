using DeliveryDervice.Data;
using DeliveryDervice.Forms.Courier;
using DeliveryDervice.Forms.Storekeeper;
using SchoolCanteen.Data;
using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace DeliveryDervice.Forms
{
    public partial class AuthForm : Form
    {
        private DatabaseManager databaseManager;

        public AuthForm()
        {
            InitializeComponent();
            databaseManager = new DatabaseManager();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            // Открытие формы регистрации при нажатии кнопки "Регистрация"
            new RegistrationForm().ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Получение введенных пользователем логина и пароля
            string login = textBoxLogin.Text;
            string password = textBoxPassword.Text;

            // Проверка, что логин и пароль не пустые
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Проверка учетных данных пользователя в таблице пользователей
                string query = @"
                    SELECT Код AS UserId
                    FROM Пользователи
                    WHERE Логин = @login AND Пароль = @password;
                ";

                OleDbParameter[] parameters = {
                    new OleDbParameter("@login", login),
                    new OleDbParameter("@password", password)
                };

                object result = databaseManager.ExecuteScalar(query, parameters);

                if (result != null)
                {
                    // Получение идентификатора пользователя
                    int userId = Convert.ToInt32(result);

                    // Поиск сотрудника по КодПользователя
                    query = @"
                        SELECT КодРоли
                        FROM Сотрудники
                        WHERE КодПользователя = @UserId;
                    ";

                    parameters = new OleDbParameter[] {
                        new OleDbParameter("@UserId", userId)
                    };

                    object roleResult = databaseManager.ExecuteScalar(query, parameters);

                    if (roleResult != null)
                    {
                        // Если найден сотрудник, проверяем его роль
                        int roleCode = Convert.ToInt32(roleResult);

                        // Поиск сотрудника по коду пользователя
                        query = @"
                            SELECT Код
                            FROM Сотрудники
                            WHERE КодПользователя = @UserId;
                        ";

                        parameters = new OleDbParameter[] {
                            new OleDbParameter("@UserId", userId)
                        };

                        object employeeResult = databaseManager.ExecuteScalar(query, parameters);

                        if (employeeResult != null)
                        {
                            // Сохраняем идентификатор сотрудника
                            int employeeId = Convert.ToInt32(employeeResult);
                            DataStorage.CurrentUserId = employeeId;

                            // Открываем соответствующую форму в зависимости от роли сотрудника
                            if (roleCode == 2)
                            {
                                // Администратор (код роли 2)
                                AdminPanel adminPanel = new AdminPanel();
                                adminPanel.ShowDialog();
                            }
                            else if (roleCode == 3)
                            {
                                // Курьер (код роли 3)
                                CourierForm courierPanel = new CourierForm();
                                courierPanel.ShowDialog();
                            }
                            else if (roleCode == 4)
                            {
                                // Кладовщик (код роли 4)
                                StorekeeperForm storekeeperPanel = new StorekeeperForm();
                                storekeeperPanel.ShowDialog();
                            }
                            else
                            {
                                // Обработка других ролей сотрудников (добавьте здесь нужную логику)
                                MessageBox.Show($"Сотрудник с ролью код {roleCode} не поддерживается.");
                            }
                        }
                        }
                        else
                        {
                            // Поиск клиента по коду пользователя
                            query = @"
                                SELECT КодКлиента
                                FROM Клиенты
                                WHERE КодПользователя = @UserId;
                            ";

                            parameters = new OleDbParameter[] {
                                new OleDbParameter("@UserId", userId)
                            };

                            object clientResult = databaseManager.ExecuteScalar(query, parameters);

                            if (clientResult != null)
                            {
                            int clientId = Convert.ToInt32(clientResult);
                            DataStorage.CurrentUserId = clientId;
                            // Если сотрудник не найден, считаем пользователя клиентом и открываем MainPanel
                            MainPanel mainPanel = new MainPanel();
                                mainPanel.ShowDialog();
                            }

                            // Закрываем текущую форму после успешной авторизации
                            this.Hide();
                        }
                    }

                    else
                    {
                        // Если учетные данные неверные, показываем сообщение об ошибке
                        MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            
            catch (Exception ex)
            {
                // Обработка исключений при попытке авторизации
                MessageBox.Show($"Ошибка при авторизации: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
