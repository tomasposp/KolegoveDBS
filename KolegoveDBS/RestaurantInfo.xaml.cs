using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interakční logika pro RestaurantInfo.xaml
    /// </summary>
    public partial class RestaurantInfo : Window
    {
        private int selectedId;
        public RestaurantInfo(int id)
        {
            InitializeComponent();
            this.selectedId = id;
            
            StringBuilder bu = new StringBuilder();
            string server = "localhost";
            string database = "kolegovedb";
            string uid = "root";
            string password = "admin";
            string constring = "Server=" + server + "; database=" + database + "; uid=" + uid + "; pwd=" + password;
            using (MySqlConnection con = new MySqlConnection(constring))
            {
                con.Open();
                string query = "SELECT * FROM restaurant WHERE restaurant_id = @selectedId";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@selectedId", selectedId);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string restaurantName = reader.GetString(1);
                        Name.Content = restaurantName;
                        string address = reader.GetString(2);
                        Address.Content = address;
                    }
                }
            }
        }
    }
}
