using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ShopDesktop
{
    public partial class CustomersForm : Form
    {
        private SQLiteConnection conn;

        public CustomersForm()
        {
            InitializeComponent();
            conn = new SQLiteConnection("Data Source=shop_easy.db;Version=3;");
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string query = "SELECT * FROM Customers";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                dataGridViewCustomers.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string query = "INSERT INTO Customers (Name, Phone, Email) VALUES (@Name, @Phone, @Email)";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Клиент успешно добавлен!");
                ClearInputs();
                LoadCustomers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления клиента: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void ClearInputs()
        {
            txtName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewCustomers.SelectedRows.Count > 0)
                {
                    int id = Convert.ToInt32(dataGridViewCustomers.SelectedRows[0].Cells["Id"].Value);

                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    string query = "DELETE FROM Customers WHERE Id = @Id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Клиент успешно удален!");
                    LoadCustomers();
                }
                else
                {
                    MessageBox.Show("Выберите клиента для удаления.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления клиента: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                foreach (DataGridViewRow row in dataGridViewCustomers.Rows)
                {
                    if (row.IsNewRow) continue;

                    int id = Convert.ToInt32(row.Cells["Id"].Value);
                    string name = row.Cells["Name"].Value.ToString();
                    string phone = row.Cells["Phone"].Value.ToString();
                    string email = row.Cells["Email"].Value?.ToString() ?? "";

                    string query = "UPDATE Customers SET Name = @Name, Phone = @Phone, Email = @Email WHERE Id = @Id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Изменения сохранены!");
                LoadCustomers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения изменений: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
