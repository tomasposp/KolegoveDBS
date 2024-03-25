using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KolegoveDBS
{
    /// <summary>
    /// Interakční logika pro Profile.xaml
    /// </summary>
    public partial class Profile : Window
    {
        private int customerId;
        public Profile(int customerId)
        {

            InitializeComponent();
            this.customerId = customerId;

            StringBuilder bu = new StringBuilder();
            string server = "localhost";
            string database = "kolegovedb";
            string uid = "root";
            string password = "admin";
            string constring = "Server=" + server + "; database=" + database + "; uid=" + uid + "; pwd=" + password;
            using (MySqlConnection con = new MySqlConnection(constring))
            {
                con.Open();
                string query = "SELECT * FROM customer WHERE customer_id = @customerId";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@customerId", customerId);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string name = reader.GetString(1);
                        Name.Content = name;
                        string email = reader.GetString(2);
                        Email.Content = email;
                        string phone= reader.GetString(3);
                        Phone.Content = phone;
                        string address = reader.GetString(4);
                        Address.Content = address;
                    }
                }
            }

        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow win = new MainWindow(1, customerId);
            win.Top = this.Top;
            win.Left = this.Left;
            win.Show();
            this.Close();
        }
    }
}
