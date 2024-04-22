using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace KolegoveDBS
{
    public partial class RestaurantInfo : Window
    {
        private int selectedId;
        public int login = 0;
        public int userId;
        public string payment_method;
        public decimal tip;

        public RestaurantInfo(int id, int userId, int login)
        {
            InitializeComponent();
            this.selectedId = id;
            this.userId = userId;
            this.login = login;
            LoadRestaurantInfo();
            if (login == 0)
            {
                orderBtn.Visibility = Visibility.Hidden;

            }
            if (login == 1)
            {
                orderBtn.Visibility = Visibility.Visible;
            }
        }
        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void LoadRestaurantInfo()
        {


            StringBuilder bu = new StringBuilder();
            string server = "localhost";
            string database = "kolegovedb";
            string uid = "root";
            string password = "admin";
            string constring = "Server=" + server + "; database=" + database + "; uid=" + uid + "; pwd=" + password;

            using (MySqlConnection con = new MySqlConnection(constring))
            {
                con.Open();
                string query = "SELECT r.*, m.* FROM restaurant r LEFT JOIN menu m ON r.restaurant_id = m.restaurant_id WHERE r.restaurant_id = @selectedId";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@selectedId", selectedId);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        decimal rating;
                        string restaurantName = reader.GetString(1);
                        Name.Content = restaurantName;
                        string address = reader.GetString(2);
                        Address.Content = address;
                        if (!reader.IsDBNull(4))
                        {
                            rating = reader.GetDecimal(4);
                        }
                        else
                        {
                            rating = 0;
                        }

                        Rating.Content = rating.ToString();


                        byte[] imageBytes = (byte[])reader["picture"];
                        if (imageBytes != null && imageBytes.Length > 0)
                        {
                            BitmapImage imageSource = new BitmapImage();
                            using (MemoryStream stream = new MemoryStream(imageBytes))
                            {
                                imageSource.BeginInit();
                                imageSource.StreamSource = stream;
                                imageSource.CacheOption = BitmapCacheOption.OnLoad;
                                imageSource.EndInit();
                            }

                            RestaurantImage.Source = imageSource;
                        }

                        StackPanel menuPanel = new StackPanel();

                        string menu = reader.GetString(7);
                        decimal price = reader.GetDecimal(8);

                        string[] menuItems = menu.Split('\n');
                        foreach (string menuItem in menuItems)
                        {
                            CheckBox checkBox = new CheckBox();
                            checkBox.Content = menuItem + " - " + price.ToString();
                            menuPanel.Children.Add(checkBox);
                        }

                        MenuStackPanel.Children.Add(menuPanel);
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {



            try
            {

                decimal totalPrice = 0;
                List<string> selectedItems = new List<string>();

                if (PaymentMethod.Text != "")
                {
                    payment_method = PaymentMethod.Text;
                }
                else
                {
                    MessageBox.Show("Prosím vyplňte metodu platby");
                    return;
                }
                if (Tip.Text != "")
                {

                    tip = Convert.ToDecimal(Tip.Text);

                }
                else
                {
                    MessageBox.Show("Prosím vyplňte tuzér");
                    return;

                }

                foreach (UIElement element in MenuStackPanel.Children)
                {
                    if (element is StackPanel menuPanel)
                    {
                        foreach (UIElement childElement in menuPanel.Children)
                        {
                            if (childElement is CheckBox checkBox && checkBox.IsChecked == true)
                            {
                                string[] menuItemAndPrice = checkBox.Content.ToString().Split('-');
                                string menuItem = menuItemAndPrice[0].Trim();
                                decimal price = decimal.Parse(menuItemAndPrice[1].Trim());

                                selectedItems.Add(menuItem);
                                totalPrice += price;
                            }
                        }
                    }
                }
                if (selectedItems.Count == 0)
                {
                    return;
                }

                var orderObject = new
                {
                    orders = selectedItems
                };

                string jsonMenu = JsonConvert.SerializeObject(orderObject);


                AddJsonToDatabase(jsonMenu, totalPrice, login, userId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba: " + ex.Message);
            }

        }

        private void AddJsonToDatabase(string jsonMenu, decimal totalPrice, int login, int userId)
        {
            PaymentChoiceWindow paymentWindow = new PaymentChoiceWindow(Convert.ToDecimal(totalPrice + tip + 2));
            paymentWindow.ShowDialog();
            int orderId;
            decimal totalAmount;
            string status = "";
            if (paymentWindow.paid)
            {
                status = "paid";
            }
            else
            {
                status = "pending";
            }



            string server = "localhost";
            string database = "kolegovedb";
            string uid = "root";
            string password = "admin";
            string constring = "Server=" + server + "; database=" + database + "; uid=" + uid + "; pwd=" + password;

            using (MySqlConnection con = new MySqlConnection(constring))
            {
                con.Open();
                string insertOrderQuery = "INSERT INTO orders (restaurant_id, order_list, amount, delivery_fee) VALUES (@restaurantId, @jsonMenu, @amount, @delivery_fee)";
                using (MySqlCommand cmd = new MySqlCommand(insertOrderQuery, con))
                {
                    cmd.Parameters.AddWithValue("@restaurantId", selectedId);
                    cmd.Parameters.AddWithValue("@jsonMenu", jsonMenu);
                    cmd.Parameters.AddWithValue("@amount", totalPrice);
                    cmd.Parameters.AddWithValue("@delivery_fee", 2.00);
                    cmd.ExecuteNonQuery();

                    orderId = (int)cmd.LastInsertedId;
                }

                string insertCustomerOrdersQuery = "INSERT INTO customer_orders (customer_id, order_id) VALUES (@customerId, @orderId)";
                using (MySqlCommand cmd = new MySqlCommand(insertCustomerOrdersQuery, con))
                {
                    cmd.Parameters.AddWithValue("@customerId", userId);
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    cmd.ExecuteNonQuery();
                }
                string calculateTotalAmountQuery = "Select CalculateTotalAmount(@order_id)";
                using (MySqlCommand cmd = new MySqlCommand(calculateTotalAmountQuery, con))
                {
                    cmd.Parameters.AddWithValue("@order_id", orderId);
                    totalAmount = Convert.ToDecimal(cmd.ExecuteScalar());

                }


                string insertPaymentQuery = "INSERT INTO payment values (@payment_id,@order_id, @payment_method, @payment_date, @total_amount, @payment_status, @tip)";
                using (MySqlCommand cmd = new MySqlCommand(insertPaymentQuery, con))
                {
                    cmd.Parameters.AddWithValue("@payment_id", cmd.LastInsertedId);
                    cmd.Parameters.AddWithValue("@order_id", orderId);
                    cmd.Parameters.AddWithValue("@payment_method", payment_method);
                    if (status == "paid")
                    {
                        cmd.Parameters.AddWithValue("@payment_date", DateTime.Now.ToString());
                    }
                    else if (status == "pending")
                    {
                        cmd.Parameters.AddWithValue("@payment_date", null);
                    }
                    cmd.Parameters.AddWithValue("@total_amount", totalAmount + tip);
                    cmd.Parameters.AddWithValue("@payment_status", status);
                    cmd.Parameters.AddWithValue("@tip", tip);
                    cmd.ExecuteNonQuery();
                }

            }
            Close();


        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void Tip_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)

        {
        }
    }
}