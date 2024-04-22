using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Data;
using System.Windows.Controls;
using System.Windows.Documents;

namespace KolegoveDBS
{
    public partial class CourierProfile : Window
    {
        private int courierId;

        public CourierProfile(int courierId)
        {
            InitializeComponent();
            this.courierId = courierId;
            LoadCourierInfo();
        }

        private async void LoadCourierInfo()
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
                    string courierQuery = "SELECT name, email, phone, address FROM courier WHERE courier_id = @courierId";

                    using (MySqlCommand courierCmd = new MySqlCommand(courierQuery, con))
                    {
                        courierCmd.Parameters.AddWithValue("@courierId", courierId);

                        using (MySqlDataReader courierReader = (MySqlDataReader)await courierCmd.ExecuteReaderAsync())
                        {
                            if (courierReader.HasRows)
                            {
                                while (await courierReader.ReadAsync())
                                {
                                    string name = courierReader.GetString("name");
                                    Name.Content = name;

                                    string email = courierReader.GetString("email");
                                    Email.Content = email;

                                    string phone = courierReader.GetString("phone");
                                    Phone.Content = phone;

                                    string address = courierReader.GetString("address");
                                    Address.Content = address;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Nenalezeny informace o kurýrovi.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }

                    string ordersQuery = "SELECT o.*, p.payment_status, p.payment_date FROM orders o LEFT JOIN payment p ON o.order_id = p.order_id WHERE p.payment_status = 'pending' AND o.courier_id = @courierId";

                    using (MySqlCommand ordersCmd = new MySqlCommand(ordersQuery, con))
                    {
                        ordersCmd.Parameters.AddWithValue("@courierId", courierId);

                        using (MySqlDataReader ordersReader = (MySqlDataReader)await ordersCmd.ExecuteReaderAsync())
                        {
                            if (ordersReader.HasRows)
                            {
                                while (await ordersReader.ReadAsync())
                                {
                                    int orderId = ordersReader.GetInt32("order_id");
                                    string payment_status = ordersReader.GetString("payment_status");

                                    var updateButton = new Button();
                                    updateButton.Content = "Platba přijata";
                                    updateButton.Tag = orderId; 
                                    updateButton.Click += UpdatePaymentStatusButton_Click;

                                    string orderText = $"Order ID: {orderId}, Payment Status: {payment_status}";
                                    OrderList.Children.Add(new TextBlock(new Run(orderText)));
                                    OrderList.Children.Add(updateButton);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání informací o kurýrovi: " + ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UpdatePaymentStatusToDatabase(int orderId)
        {
            string newPaymentStatus = "paid";

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

                    string updateQuery = "UPDATE payment SET payment_status = @newPaymentStatus WHERE order_id = @orderId";

                    using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, con))
                    {
                        updateCmd.Parameters.AddWithValue("@newPaymentStatus", newPaymentStatus);
                        updateCmd.Parameters.AddWithValue("@orderId", orderId);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Stav platby byl úspěšně aktualizován v databázi.", "Úspěch", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Stav platby nebyl aktualizován v databázi.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při aktualizaci stavu platby v databázi: " + ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UpdatePaymentStatusButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                int orderId = (int)button.Tag;
                UpdatePaymentStatusToDatabase(orderId);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow win = new MainWindow(1, courierId, 0, 1);
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