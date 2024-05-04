using System.Data.OleDb;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace SchoolCanteen.Data
{
    internal class DatabaseManager
    {
        private OleDbConnection connection;
        private static string connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName}\DB\DaseDelivery.accdb;Persist Security Info=False;";

        public DatabaseManager()
        {
            connection = new OleDbConnection(connectionString);
        }

        public void Fill(string tableName, DataGridView grid)
        {
            DataTable table = new DataTable();
            OleDbDataAdapter adapter = new OleDbDataAdapter();
            OleDbCommand cmd = new OleDbCommand($"SELECT * FROM [{tableName}]", connection);
            adapter.SelectCommand = cmd;
            adapter.Fill(table);
            grid.DataSource = table;
        }

        public DataTable GetData(string query, OleDbParameter[] parameters = null)
        {
            DataTable dt = new DataTable();

            using (OleDbCommand cmd = new OleDbCommand(query, connection))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
                adapter.Fill(dt);
            }

            return dt;
        }

        public void ExecuteCommand(string command, OleDbParameter[] parameters = null)
        {
            using (OleDbCommand cmd = new OleDbCommand(command, connection))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
            }
        }

        public object ExecuteScalar(string query, OleDbParameter[] parameters = null)
        {
            // Создаем соединение с базой данных с помощью connectionString
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                // Открываем соединение
                connection.Open();

                // Создаем команду для выполнения запроса
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    // Если переданы параметры, добавляем их к команде
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    // Выполняем запрос и возвращаем результат (первый столбец первой строки)
                    return command.ExecuteScalar();
                }
            }
        }


        public void ExecuteStoredProcedure(string procedureName, OleDbParameter[] parameters = null)
        {
            using (OleDbCommand cmd = new OleDbCommand(procedureName, connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.Add(param);
                    }
                }

                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}
