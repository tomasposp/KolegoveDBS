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
                    string query = "SELECT c.name, c.email, c.phone, c.address, (SELECT IFNULL(SUM(o.amount), 0) FROM orders o WHERE co.order_id = o.order_id) AS total_amount, GROUP_CONCAT(o.order_list) AS orders FROM customer c LEFT JOIN customer_orders co ON c.customer_id = co.customer_id LEFT JOIN orders o ON co.order_id = o.order_id WHERE c.customer_id = @customerId GROUP BY c.name, c.email, c.phone, c.address, co.order_id; ";

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
                                            OrderList.Content += $"Cena: {amount}                                                         ";

                                            StringBuilder orderBuilder = new StringBuilder();
                                            foreach (string order in orderResponse.Orders)
                                            {
                                                orderBuilder.Append(order + ", ");
                                            }
                                            OrderList.Content += orderBuilder.ToString().TrimEnd(',', ' ') + Environment.NewLine;
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
            MainWindow win = new MainWindow(1, customerId,0,0);
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