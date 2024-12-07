using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ShopDesktop
{
    public partial class CategoryForm : Form
    {
        private SQLiteConnection conn;

        public CategoryForm()
        {
            InitializeComponent();
            conn = new SQLiteConnection("Data Source=shop_easy.db;Version=3;");
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string query = "SELECT * FROM Category";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                dataGridViewCategory.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message);
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

                string query = "INSERT INTO Category (Name) VALUES (@Name)";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", txtCategoryName.Text.Trim());
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Категория добавлена!");
                LoadCategories();
                txtCategoryName.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления категории: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewCategory.SelectedRows.Count > 0)
                {
                    int id = Convert.ToInt32(dataGridViewCategory.SelectedRows[0].Cells["Id"].Value);

                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    string query = "DELETE FROM Category WHERE Id = @Id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Категория удалена!");
                    LoadCategories();
                }
                else
                {
                    MessageBox.Show("Выберите категорию для удаления.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления категории: " + ex.Message);
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

                DataTable changes = ((DataTable)dataGridViewCategory.DataSource).GetChanges();
                if (changes != null)
                {
                    string query = "UPDATE Category SET Name = @Name WHERE Id = @Id";
                    foreach (DataRow row in changes.Rows)
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Name", row["Name"]);
                            cmd.Parameters.AddWithValue("@Id", row["Id"]);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Изменения сохранены!");
                    ((DataTable)dataGridViewCategory.DataSource).AcceptChanges();
                }
                else
                {
                    MessageBox.Show("Нет изменений для сохранения.");
                }
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
