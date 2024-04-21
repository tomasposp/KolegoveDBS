using MySql.Data.MySqlClient;
using System.Text;
using System.Windows;

namespace KolegoveDBS
{
    public partial class DeliveryInfo : Window
    {
        private int selectedId;
        public int login = 0;
        public int userId;

        public DeliveryInfo(int id, int userId, int login)
        {
            InitializeComponent();
            this.selectedId = id;
            this.login = login;
            this.userId = userId;

            StringBuilder bu = new StringBuilder();
            string server = "localhost";
            string database = "kolegovedb";
            string uid = "root";
            string password = "admin";
            string constring = "Server=" + server + "; database=" + database + "; uid=" + uid + "; pwd=" + password;

            using (MySqlConnection con = new MySqlConnection(constring))
            {
                con.Open();
                string query = @"
                    SELECT c.name, c.phone, c.address 
                    FROM customer c 
                    INNER JOIN customer_orders co ON c.customer_id = co.customer_id 
                    INNER JOIN orders o ON co.order_id = o.order_id 
                    WHERE o.order_id = @orderId AND o.courier_id IS NULL";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@orderId", selectedId);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string customerName = reader.GetString(0);
                        nameLbl.Content = customerName;
                        string phone = reader.GetString(1);
                        phoneLbl.Content = phone;
                        string address = reader.GetString(2);
                        addressLbl.Content = address;
                    }
                    else
                    {
                        MessageBox.Show("Data not found.");
                    }
                }
            }
        }

        private void takeBtn_Click(object sender, RoutedEventArgs e)
        {
            string server = "localhost";
            string database = "kolegovedb";
            string uid = "root";
            string password = "admin";
            string constring = "Server=" + server + "; database=" + database + "; uid=" + uid + "; pwd=" + password;

            using (MySqlConnection con = new MySqlConnection(constring))
            {
                con.Open();
                string query = @"
            UPDATE orders
            SET courier_id = @userId
            WHERE order_id = @orderId";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@orderId", selectedId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Delivery ID updated successfully.");
                        
                    }
                    else
                    {
                        MessageBox.Show("Failed to update delivery ID.");
                    }
                }
            }
        }
    }
}