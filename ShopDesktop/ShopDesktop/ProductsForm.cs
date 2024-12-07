using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace ShopDesktop
{
    public partial class ProductsForm : Form
    {
        private SQLiteConnection conn;
        public ProductsForm()
        {
            InitializeComponent();
            conn = new SQLiteConnection("Data Source=shop_easy.db;Version=3;");
            LoadProducts();
            LoadCategories();
        }

        private void LoadProducts()
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string query = "SELECT * FROM Products";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                dataGridViewProducts.DataSource = dataTable;

                dataGridViewProducts.Columns["Id"].ReadOnly = true;
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

        private void LoadCategories()
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string query = "SELECT Id, Name FROM Category";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                comboBoxCategory.DisplayMember = "Name";
                comboBoxCategory.ValueMember = "Id";
                comboBoxCategory.DataSource = dataTable;
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

                string query = "INSERT INTO Products (Name, Price, Stock, CategoryId) VALUES (@Name, @Price, @Stock, @CategoryId)";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Price", Convert.ToDouble(txtPrice.Text.Trim()));
                    cmd.Parameters.AddWithValue("@Stock", Convert.ToInt32(txtStock.Text.Trim()));
                    cmd.Parameters.AddWithValue("@CategoryId", Convert.ToInt32(comboBoxCategory.SelectedValue));
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Продукт успешно добавлен!");
                ClearInputs();
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления продукта: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void ClearInputs()
        {
            txtName.Clear();
            txtPrice.Clear();
            txtStock.Clear();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewProducts.SelectedRows.Count > 0)
                {
                    int id = Convert.ToInt32(dataGridViewProducts.SelectedRows[0].Cells["Id"].Value);

                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    string query = "DELETE FROM Products WHERE Id = @Id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Продукт успешно удален!");
                    LoadProducts();
                }
                else
                {
                    MessageBox.Show("Выберите продукт для удаления.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления продукта: " + ex.Message);
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

                foreach (DataGridViewRow row in dataGridViewProducts.Rows)
                {
                    if (row.IsNewRow) continue;

                    int id = Convert.ToInt32(row.Cells["Id"].Value);
                    string name = row.Cells["Name"].Value.ToString();
                    double price = Convert.ToDouble(row.Cells["Price"].Value);
                    int stock = Convert.ToInt32(row.Cells["Stock"].Value);
                    string category = row.Cells["CategoryId"].Value.ToString();

                    string query = "UPDATE Products SET Name = @Name, Price = @Price, Stock = @Stock, CategoryId = @CategoryId WHERE Id = @Id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Price", price);
                        cmd.Parameters.AddWithValue("@Stock", stock);
                        cmd.Parameters.AddWithValue("@CategoryId", category);
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Изменения успешно сохранены!");
                LoadProducts();
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

        private void btnAnalytics_Click(object sender, EventArgs e)
        {
            ShowAnalytics();
        }

        private void ShowAnalytics()
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string productQuantityQuery = "SELECT Name, Stock FROM Products";
                SQLiteCommand cmdProductQuantity = new SQLiteCommand(productQuantityQuery, conn);
                SQLiteDataReader readerProductQuantity = cmdProductQuantity.ExecuteReader();
                Dictionary<string, int> productQuantities = new Dictionary<string, int>();
                while (readerProductQuantity.Read())
                {
                    productQuantities.Add(readerProductQuantity["Name"].ToString(), Convert.ToInt32(readerProductQuantity["Stock"]));
                }
                readerProductQuantity.Close();

                string categoryQuantityQuery = @"
                SELECT c.Name AS Category, SUM(p.Stock) AS TotalStock
                FROM Products p
                JOIN Category c ON p.CategoryId = c.Id
                GROUP BY c.Name";
                SQLiteCommand cmdCategoryQuantity = new SQLiteCommand(categoryQuantityQuery, conn);
                SQLiteDataReader readerCategoryQuantity = cmdCategoryQuantity.ExecuteReader();
                Dictionary<string, int> categoryQuantities = new Dictionary<string, int>();
                while (readerCategoryQuantity.Read())
                {
                    categoryQuantities.Add(readerCategoryQuantity["Category"].ToString(), Convert.ToInt32(readerCategoryQuantity["TotalStock"]));
                }
                readerCategoryQuantity.Close();

                string topExpensiveQuery = "SELECT Name, Price FROM Products ORDER BY Price DESC LIMIT 5";
                SQLiteCommand cmdTopExpensive = new SQLiteCommand(topExpensiveQuery, conn);
                SQLiteDataReader readerTopExpensive = cmdTopExpensive.ExecuteReader();
                List<string> expensiveProducts = new List<string>();
                int rank = 1;
                while (readerTopExpensive.Read())
                {
                    string productInfo = $"{rank}. {readerTopExpensive["Name"]}: {readerTopExpensive["Price"]:C}";
                    expensiveProducts.Add(productInfo); 
                    rank++;
                }
                readerTopExpensive.Close();

                string topCheapQuery = "SELECT Name, Price FROM Products ORDER BY Price ASC LIMIT 5";
                SQLiteCommand cmdTopCheap = new SQLiteCommand(topCheapQuery, conn);
                SQLiteDataReader readerTopCheap = cmdTopCheap.ExecuteReader();
                List<string> cheapProducts = new List<string>();
                rank = 1;
                while (readerTopCheap.Read())
                {
                    string productInfo = $"{rank}. {readerTopCheap["Name"]}: {readerTopCheap["Price"]:C}";
                    cheapProducts.Add(productInfo);
                    rank++; 
                }
                readerTopCheap.Close();

                Form analyticsForm = new Form
                {
                    Text = "Аналитика",
                    Size = new Size(600, 500),
                    BackColor = Color.White
                };

                var chartContainer = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    Height = 200,
                    ColumnCount = 2,
                    RowCount = 1
                };

                var pieChartProducts = CreatePieChart(productQuantities);
                var lblProductsChart = new Label
                {
                    Text = "Количество/Название товара",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Bottom
                };

                var productsPanel = new Panel { Dock = DockStyle.Fill };
                productsPanel.Controls.Add(pieChartProducts);
                productsPanel.Controls.Add(lblProductsChart);

                var pieChartCategories = CreatePieChart(categoryQuantities);
                var lblCategoriesChart = new Label
                {
                    Text = "Количество/Категория",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Bottom
                };

                var categoriesPanel = new Panel { Dock = DockStyle.Fill };
                categoriesPanel.Controls.Add(pieChartCategories);
                categoriesPanel.Controls.Add(lblCategoriesChart);

                chartContainer.Controls.Add(productsPanel);
                chartContainer.Controls.Add(categoriesPanel);

                var topContainer = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 2,
                    Padding = new Padding(20)
                };

                topContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                topContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                topContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                topContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                var lblExpensive = new Label
                {
                    Text = "Топ 5 самых дорогих товаров",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                var lblCheap = new Label
                {
                    Text = "Топ 5 самых дешевых товаров",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };

                var txtExpensive = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    Text = string.Join(Environment.NewLine, expensiveProducts),
                    Dock = DockStyle.Fill,
                    ScrollBars = ScrollBars.Vertical
                };

                var txtCheap = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    Text = string.Join(Environment.NewLine, cheapProducts),
                    Dock = DockStyle.Fill,
                    ScrollBars = ScrollBars.Vertical
                };

                topContainer.Controls.Add(lblExpensive, 0, 0);
                topContainer.Controls.Add(lblCheap, 1, 0);
                topContainer.Controls.Add(txtExpensive, 0, 1);
                topContainer.Controls.Add(txtCheap, 1, 1);

                analyticsForm.Controls.Add(topContainer);
                analyticsForm.Controls.Add(chartContainer);
                analyticsForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки аналитики: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private LiveCharts.WinForms.PieChart CreatePieChart(Dictionary<string, int> data)
        {
            var pieChart = new LiveCharts.WinForms.PieChart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var seriesCollection = new LiveCharts.SeriesCollection();

            foreach (var item in data)
            {
                seriesCollection.Add(new LiveCharts.Wpf.PieSeries
                {
                    Title = item.Key,
                    Values = new LiveCharts.ChartValues<int> { item.Value },
                    DataLabels = true
                });
            }

            pieChart.Series = seriesCollection;
            return pieChart;
        }

    }
}
