using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient;

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
                    string query = "SELECT name, email, phone, address FROM courier WHERE courier_id = @courierId";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@courierId", courierId);

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
                                }
                            }
                            else
                            {
                                MessageBox.Show("Nenalezeny informace o kurýrovi.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
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