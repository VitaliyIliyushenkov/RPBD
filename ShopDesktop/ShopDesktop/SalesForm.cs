using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;


namespace ShopDesktop
{
    public partial class SalesForm : Form
    {
        private SQLiteConnection conn;

        public SalesForm()
        {
            InitializeComponent();
            conn = new SQLiteConnection("Data Source=shop_easy.db;Version=3;");
            LoadSales();
            LoadProducts();
            LoadCustomers();
        }

        private void LoadSales()
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string query = @"
                SELECT Sales.Id, Sales.Date, Products.Name AS Product, Sales.Quantity, Sales.Total 
                FROM Sales 
                INNER JOIN Products ON Sales.ProductId = Products.Id";

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                dataGridViewSales.DataSource = dataTable;
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

        private void LoadCustomers()
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string query = "SELECT Id, Name FROM Customers";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                comboBoxCustomer.DisplayMember = "Name";
                comboBoxCustomer.ValueMember = "Id";
                comboBoxCustomer.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки клиентов: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }


        private void LoadProducts()
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string query = "SELECT Id, Name FROM Products";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                comboBoxProduct.DisplayMember = "Name";
                comboBoxProduct.ValueMember = "Id";
                comboBoxProduct.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки продуктов: " + ex.Message);
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

                int productId = Convert.ToInt32(comboBoxProduct.SelectedValue);
                int quantity = Convert.ToInt32(txtQuantity.Text.Trim());
                double price = GetProductPrice(productId);
                double total = quantity * price;

                string checkStockQuery = "SELECT Stock FROM Products WHERE Id = @ProductId";
                using (SQLiteCommand checkCmd = new SQLiteCommand(checkStockQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@ProductId", productId);
                    int stock = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (stock < quantity)
                    {
                        MessageBox.Show("Недостаточно товара на складе!");
                        return;
                    }
                }

                string query = "INSERT INTO Sales (Date, ProductId, CustomerId, Quantity, Total) VALUES (@Date, @ProductId, @CustomerId, @Quantity, @Total)";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    cmd.Parameters.AddWithValue("@CustomerId", Convert.ToInt32(comboBoxCustomer.SelectedValue));
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@Total", total);
                    cmd.ExecuteNonQuery();
                }

                string updateStockQuery = "UPDATE Products SET Stock = Stock - @Quantity WHERE Id = @ProductId";
                using (SQLiteCommand updateCmd = new SQLiteCommand(updateStockQuery, conn))
                {
                    updateCmd.Parameters.AddWithValue("@Quantity", quantity);
                    updateCmd.Parameters.AddWithValue("@ProductId", productId);
                    updateCmd.ExecuteNonQuery();
                }

                MessageBox.Show("Продажа успешно добавлена!");
                ClearInputs();
                LoadSales();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления продажи: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private double GetProductPrice(int productId)
        {
            try
            {
                string query = "SELECT Price FROM Products WHERE Id = @ProductId";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    return Convert.ToDouble(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка получения цены продукта: " + ex.Message);
                return 0;
            }
        }

        private void ClearInputs()
        {
            txtQuantity.Clear();
            comboBoxProduct.SelectedIndex = -1;
            comboBoxCustomer.SelectedIndex = -1;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewSales.SelectedRows.Count > 0)
                {
                    int id = Convert.ToInt32(dataGridViewSales.SelectedRows[0].Cells["Id"].Value);

                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    string query = "DELETE FROM Sales WHERE Id = @Id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Продажа успешно удалена!");
                    LoadSales();
                }
                else
                {
                    MessageBox.Show("Выберите продажу для удаления.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления продажи: " + ex.Message);
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

                foreach (DataGridViewRow row in dataGridViewSales.Rows)
                {
                    if (row.IsNewRow) continue;

                    int id = Convert.ToInt32(row.Cells["Id"].Value);
                    string date = row.Cells["Date"].Value.ToString();
                    int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                    double total = Convert.ToDouble(row.Cells["Total"].Value);

                    string query = "UPDATE Sales SET Date = @Date, Quantity = @Quantity, Total = @Total WHERE Id = @Id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Date", date);
                        cmd.Parameters.AddWithValue("@Quantity", quantity);
                        cmd.Parameters.AddWithValue("@Total", total);
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Изменения сохранены!");
                LoadSales();
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
            ShowSalesAnalytics();
        }


        private void ShowSalesAnalytics()
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string monthlySalesQuery = @"
                SELECT strftime('%Y-%m', Date) AS Month, SUM(Quantity) AS TotalQuantity
                FROM Sales
                GROUP BY strftime('%Y-%m', Date)
                ORDER BY Month";
                SQLiteCommand cmdMonthlySales = new SQLiteCommand(monthlySalesQuery, conn);
                SQLiteDataReader readerMonthlySales = cmdMonthlySales.ExecuteReader();
                Dictionary<string, int> monthlySales = new Dictionary<string, int>();
                while (readerMonthlySales.Read())
                {
                    monthlySales.Add(readerMonthlySales["Month"].ToString(), Convert.ToInt32(readerMonthlySales["TotalQuantity"]));
                }
                readerMonthlySales.Close();

                string popularProductsQuery = @"
                SELECT p.Name AS Product, SUM(s.Quantity) AS TotalQuantity
                FROM Sales s
                JOIN Products p ON s.ProductId = p.Id
                GROUP BY p.Name
                ORDER BY TotalQuantity DESC";
                SQLiteCommand cmdPopularProducts = new SQLiteCommand(popularProductsQuery, conn);
                SQLiteDataReader readerPopularProducts = cmdPopularProducts.ExecuteReader();
                Dictionary<string, int> popularProducts = new Dictionary<string, int>();
                while (readerPopularProducts.Read())
                {
                    popularProducts.Add(readerPopularProducts["Product"].ToString(), Convert.ToInt32(readerPopularProducts["TotalQuantity"]));
                }
                readerPopularProducts.Close();

                string topCustomersQuery = @"
                SELECT c.Name AS Customer, SUM(s.Total) AS TotalSpent
                FROM Sales s
                JOIN Customers c ON s.CustomerId = c.Id
                GROUP BY c.Name
                ORDER BY TotalSpent DESC
                LIMIT 3";
                SQLiteCommand cmdTopCustomers = new SQLiteCommand(topCustomersQuery, conn);
                SQLiteDataReader readerTopCustomers = cmdTopCustomers.ExecuteReader();
                List<string> topCustomers = new List<string>();
                int rank = 1;
                while (readerTopCustomers.Read())
                {
                    string customerInfo = $"{rank}. {readerTopCustomers["Customer"]}: {readerTopCustomers["TotalSpent"]:C}";
                    topCustomers.Add(customerInfo);
                    rank++;
                }
                readerTopCustomers.Close();

                Form analyticsForm = new Form
                {
                    Text = "Аналитика продаж",
                    Size = new Size(1000, 800),
                    BackColor = Color.White
                };

                var chartContainer = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    Height = 400,
                    ColumnCount = 2,
                    RowCount = 1
                };

                var monthlySalesChart = CreateBarChart(monthlySales, "Месяц", "Количество проданного товара");
                var lblMonthlySalesChart = new Label
                {
                    Text = "Месяц/Количество проданного товара",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Bottom
                };

                var monthlySalesPanel = new Panel { Dock = DockStyle.Fill };
                monthlySalesPanel.Controls.Add(monthlySalesChart);
                monthlySalesPanel.Controls.Add(lblMonthlySalesChart);

                var popularProductsChart = CreateBarChart(popularProducts, "Товар", "Количество");
                var lblPopularProductsChart = new Label
                {
                    Text = "Популярность товаров",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Bottom
                };

                var popularProductsPanel = new Panel { Dock = DockStyle.Fill };
                popularProductsPanel.Controls.Add(popularProductsChart);
                popularProductsPanel.Controls.Add(lblPopularProductsChart);

                chartContainer.Controls.Add(monthlySalesPanel);
                chartContainer.Controls.Add(popularProductsPanel);

                var topContainer = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                    Padding = new Padding(20)
                };

                topContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                topContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                var lblTopCustomers = new Label
                {
                    Text = "Топ-3 покупателей",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };

                var txtTopCustomers = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    Text = string.Join(Environment.NewLine, topCustomers),
                    Dock = DockStyle.Fill,
                    ScrollBars = ScrollBars.Vertical
                };

                topContainer.Controls.Add(lblTopCustomers, 0, 0);
                topContainer.Controls.Add(txtTopCustomers, 0, 1);

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


        private LiveCharts.WinForms.CartesianChart CreateBarChart(Dictionary<string, int> data, string xTitle, string yTitle)
        {
            var cartesianChart = new LiveCharts.WinForms.CartesianChart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var seriesCollection = new LiveCharts.SeriesCollection
            {
                new LiveCharts.Wpf.ColumnSeries
                {
                    Values = new LiveCharts.ChartValues<int>(data.Values),
                    DataLabels = true
                }
            };

            cartesianChart.Series = seriesCollection;
            cartesianChart.AxisX.Add(new LiveCharts.Wpf.Axis
            {
                Title = xTitle,
                Labels = data.Keys.ToArray()
            });

            cartesianChart.AxisY.Add(new LiveCharts.Wpf.Axis
            {
                Title = yTitle
            });

            return cartesianChart;
        }

    }
}
