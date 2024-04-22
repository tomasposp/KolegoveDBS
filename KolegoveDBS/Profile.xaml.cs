using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

namespace KolegoveDBS
{
    public partial class Profile : Window
    {
        private int customerId;

        public Profile(int customerId)
        {
            InitializeComponent();
            this.customerId = customerId;
            LoadCustomerInfo();
        }

        private async void LoadCustomerInfo()
        {
            string server = "localhost";
            string database = "kolegovedb";
            string uid = "root";
            string password = "admin";
            string constring = $"Server={server}; database={database}; uid={uid}; pwd={password}";

            try
            {
                using (MySqlConnection con = new MySqlConnection(constring))
                {
                    await con.OpenAsync();
                    string query = "SELECT c.name, c.email, c.phone, c.address, (SELECT IFNULL(SUM(o.amount), 0) FROM orders o WHERE co.order_id = o.order_id) AS total_amount, GROUP_CONCAT(o.order_list) AS orders, o.order_id FROM customer c LEFT JOIN customer_orders co ON c.customer_id = co.customer_id LEFT JOIN orders o ON co.order_id = o.order_id WHERE c.customer_id = @customerId GROUP BY c.name, c.email, c.phone, c.address, co.order_id, o.order_id; ";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@customerId", customerId);

                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    string name = reader.GetString("name");
                                    Name.Content = name;

                                    string email = reader.GetString("email");
                                    Email.Content = email;

                                    string phone = reader.GetString("phone");
                                    Phone.Content = phone;

                                    string address = reader.GetString("address");
                                    Address.Content = address;

                                    decimal amount = reader.GetDecimal("total_amount");

                                    string orderListJson = reader.IsDBNull("orders") ? "" : reader.GetString("orders").Trim();

                                    if (!string.IsNullOrEmpty(orderListJson))
                                    {
                                        List<OrderResponse> orderResponseList = JsonConvert.DeserializeObject<List<OrderResponse>>("[" + orderListJson + "]");

                                        foreach (OrderResponse orderResponse in orderResponseList)
                                        {
                                            OrderList.Content += $"Cena: {amount}                           ";

                                            StringBuilder orderBuilder = new StringBuilder();
                                            foreach (string order in orderResponse.Orders)
                                            {
                                                orderBuilder.Append(order + ", ");
                                            }
                                            OrderList.Content += orderBuilder.ToString().TrimEnd(',', ' ') + Environment.NewLine;

                                            int orderId = reader.GetInt32("order_id");

                                            OrderList.Content += "\n";

                                            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

                                            TextBox textBox = new TextBox { Text = "", Margin = new Thickness(2,2,2,10), Width = 30, Height = 20 };
                                            TextBox textBoxComment = new TextBox { Text = "", Margin = new Thickness(2, 2, 2, 10), Width = 50, Height = 20 };

                                            TextBox textBoxRest = new TextBox { Text = "", Margin = new Thickness(2, 2, 2, 10), Width = 30, Height = 20 };
                                            TextBox textBoxCommentRest = new TextBox { Text = "", Margin = new Thickness(2, 2, 2, 10), Width = 50, Height = 20 };

                                            Button saveButton = new Button { Content = "Uložit", Margin = new Thickness(2, 2, 2, 10) };

                                            saveButton.Click += async (s, e) =>
                                            {
                                                string hodnoceni = textBox.Text;
                                                string comment = textBoxComment.Text;
                                                string hodnoceniRest = textBoxRest.Text;
                                                string commentRest = textBoxCommentRest.Text;
                                                try
                                                {
                                                    using (MySqlConnection ratingCon = new MySqlConnection(constring))
                                                    {
                                                        await ratingCon.OpenAsync();
                                                        string courierQuery = "SELECT courier_id FROM orders WHERE order_id = @orderId";
                                                        int id = 0;
                                                        using (MySqlCommand courierCmd = new MySqlCommand(courierQuery, ratingCon))
                                                        {
                                                            courierCmd.Parameters.AddWithValue("@orderId", orderId);
                                                            using (MySqlDataReader reader2 = (MySqlDataReader)await courierCmd.ExecuteReaderAsync())
                                                            {
                                                                if (reader2.HasRows)
                                                                {
                                                                    while (await reader2.ReadAsync())
                                                                    {
                                                                        id = reader2.GetInt32(0);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        string ratingQuery = "INSERT INTO ratingcourier (courier_id, rating, customer_id, comment, rating_date) VALUES (@courierId, @rating, @customerId, @comment, @ratingDate)";
                                                        using (MySqlCommand ratingCmd = new MySqlCommand(ratingQuery, ratingCon))
                                                        {
                                                            ratingCmd.Parameters.AddWithValue("@courierId", id);
                                                            ratingCmd.Parameters.AddWithValue("@rating", hodnoceni);
                                                            ratingCmd.Parameters.AddWithValue("@comment", comment);
                                                            ratingCmd.Parameters.AddWithValue("@customerId", customerId);
                                                            string ratingDate = DateTime.Now.ToString("yyyy-MM-dd");
                                                            ratingCmd.Parameters.AddWithValue("@ratingDate", ratingDate);
                                                            await ratingCmd.ExecuteNonQueryAsync();
                                                           
                                                        }
                                                    }
                                                    using (MySqlConnection ratingConRest = new MySqlConnection(constring))
                                                    {
                                                        await ratingConRest.OpenAsync();
                                                        string restaurantQuerty = "SELECT restaurant_id FROM orders WHERE order_id = @orderId";
                                                        int id = 0;
                                                        using (MySqlCommand restaurantCmd = new MySqlCommand(restaurantQuerty, ratingConRest))
                                                        {
                                                            restaurantCmd.Parameters.AddWithValue("@orderId", orderId);
                                                            using (MySqlDataReader reader2 = (MySqlDataReader)await restaurantCmd.ExecuteReaderAsync())
                                                            {
                                                                if (reader2.HasRows)
                                                                {
                                                                    while (await reader2.ReadAsync())
                                                                    {
                                                                        id = reader2.GetInt32(0);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        string ratingQueryRest = "INSERT INTO ratingrestaurant (restaurant_id, rating, customer_id, comment, rating_date) VALUES (@retaurantId, @rating, @customerId, @comment, @ratingDate)";
                                                        using (MySqlCommand ratingCmdRest = new MySqlCommand(ratingQueryRest, ratingConRest))
                                                        {
                                                            ratingCmdRest.Parameters.AddWithValue("@retaurantId", id);
                                                            ratingCmdRest.Parameters.AddWithValue("@rating", hodnoceniRest);
                                                            ratingCmdRest.Parameters.AddWithValue("@comment", commentRest);
                                                            ratingCmdRest.Parameters.AddWithValue("@customerId", customerId);
                                                            string ratingDate = DateTime.Now.ToString("yyyy-MM-dd");
                                                            ratingCmdRest.Parameters.AddWithValue("@ratingDate", ratingDate);
                                                            await ratingCmdRest.ExecuteNonQueryAsync();
                                                            MessageBox.Show("Hodnocení úspěšně uloženo!");
                                                        }
                                                    }

                                                }
                                                catch (Exception ex)
                                                {
                                                    MessageBox.Show("Chyba při ukládání hodnocení: " + ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                                                }
                                            };


                                            stackPanel.Children.Add(textBox);
                                            stackPanel.Children.Add(textBoxComment);
                                            stackPanel.Children.Add(textBoxRest);
                                            stackPanel.Children.Add(textBoxCommentRest);
                                            stackPanel.Children.Add(saveButton);

                                            this.ContentPanel.Children.Add(stackPanel);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                OrderList.Content = "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání informací o zákazníkovi: " + ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow win = new MainWindow(1, customerId, 0, 0);
            win.Top = this.Top;
            win.Left = this.Left;
            win.Show();
            this.Close();
        }

        public class OrderResponse
        {
            public List<string> Orders { get; set; }
        }
    }
}