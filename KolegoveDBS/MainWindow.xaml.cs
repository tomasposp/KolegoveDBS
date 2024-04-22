using MySql.Data.MySqlClient;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace KolegoveDBS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int login = 0;
        public int userId;
        public int admin;
        public int courier;
        public MainWindow()
        {
            InitializeComponent();

            Initialize();
        }
        public MainWindow(int login, int userId, int admin, int courier)
        {
            this.login = login;
            this.userId = userId;
            this.admin = admin;
            this.courier = courier;
            InitializeComponent();

            Initialize();

        }

        private void Initialize()
        {
            if (login == 0)
            {
                RefreshBtn.Visibility = Visibility.Hidden;
                SignOut.Visibility = Visibility.Hidden;
                Login.Visibility = Visibility.Visible;
                Register.Visibility = Visibility.Visible;
                Profile.Visibility = Visibility.Hidden;
                AdminLbl.Visibility = Visibility.Hidden;
                CourierLbl.Visibility = Visibility.Hidden;
                CourierProfile.Visibility = Visibility.Hidden;
                stackPanelButtons.Visibility = Visibility.Visible;
                stackPanelDelete.Visibility = Visibility.Hidden;
                stackPanelCouriers.Visibility = Visibility.Hidden;
            }
            if (login == 1)
            {
                RefreshBtn.Visibility = Visibility.Hidden;
                SignOut.Visibility = Visibility.Visible;
                Login.Visibility = Visibility.Hidden;
                Register.Visibility = Visibility.Hidden;
                Profile.Visibility = Visibility.Visible;
                AdminLbl.Visibility = Visibility.Hidden;
                CourierLbl.Visibility = Visibility.Hidden;
                stackPanelCouriers.Visibility = Visibility.Hidden;
                if (courier == 1)
                {
                    CourierLbl.Visibility = Visibility.Visible;
                    Profile.Visibility = Visibility.Hidden;
                    CourierProfile.Visibility = Visibility.Visible;
                    stackPanelButtons.Visibility = Visibility.Hidden;
                    stackPanelCouriers.Visibility = Visibility.Visible;
                    RefreshBtn.Visibility = Visibility.Visible;
                    InitializeCourier();
                }
                if (courier == 0)
                {
                    RefreshBtn.Visibility = Visibility.Hidden;
                    CourierProfile.Visibility = Visibility.Hidden;
                    Profile.Visibility = Visibility.Visible;
                    stackPanelButtons.Visibility = Visibility.Visible;
                    stackPanelCouriers.Visibility = Visibility.Hidden;
                }
                if (admin == 1)
                {
                    RefreshBtn.Visibility = Visibility.Visible;
                    AdminLbl.Visibility = Visibility.Visible;
                    stackPanelDelete.Visibility = Visibility.Visible;
                    Profile.Visibility = Visibility.Hidden;
                    CourierProfile.Visibility = Visibility.Hidden;
                }

            }
            stackPanelButtons.Children.Clear();

            StringBuilder bu = new StringBuilder();
            string server = "localhost";
            string database = "kolegovedb";
            string uid = "root";
            string password = "admin";
            string constring = "Server=" + server + "; database=" + database + "; uid=" + uid + "; pwd=" + password;
            using (MySqlConnection con = new MySqlConnection(constring))
            {
                con.Open();
                string query = "SELECT * FROM restaurant";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string restaurantName = reader.GetString(1);
                        Button btn = new Button();
                        btn.Content = restaurantName;
                        btn.Tag = reader.GetInt32(0);
                        stackPanelButtons.Children.Add(btn);
                        btn.Click += RestaurantInfo_Click;
                        if (admin == 1)
                        {
                            Button btnDel = new Button();

                            btnDel.Tag = reader.GetInt32(0);
                            btnDel.Content = "X";
                            stackPanelDelete.Children.Add(btnDel);
                            btnDel.Click += Delete_Click;

                        }

                    }
                }
            }

        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button deleteButton = sender as Button;
                string server = "localhost";
                string database = "kolegovedb";
                string uid = "root";
                string password = "admin";
                string constring = "Server=" + server + "; database=" + database + "; uid=" + uid + "; pwd=" + password;
                using (MySqlConnection con = new MySqlConnection(constring))
                {
                    con.Open();
                    string deleteMenu = "Delete from menu where restaurant_id = @restaurantId";
                    using (MySqlCommand cmd = new MySqlCommand(deleteMenu, con))
                    {
                        cmd.Parameters.AddWithValue("@restaurantId", deleteButton.Tag);
                        cmd.ExecuteNonQuery();
                    }
                    string deleteQuery = "Delete from restaurant where restaurant_id = @restaurantId";
                    using (MySqlCommand cmd = new MySqlCommand(deleteQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@restaurantId", deleteButton.Tag);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Restaurace byla úspěšně smazaná!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        private void InitializeCourier()
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
                string query = "SELECT o.*, r.* FROM orders o LEFT JOIN restaurant r ON o.restaurant_id = r.restaurant_id WHERE o.courier_id IS NULL";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string restaurant = reader.GetString(8);
                        string address = reader.GetString(9);
                        decimal amount = reader.GetDecimal(1);
                        int orderId = reader.GetInt32(0);
                        Button btn = new Button();
                        btn.Content = "Restaurace: " + restaurant + ", Adresa: " + address + ", cena: " + amount;
                        btn.Tag = orderId;
                        btn.Click += DeliveryInfo_Click;
                        stackPanelCouriers.Children.Add(btn);


                    }
                }
            }
        }



        private void DeliveryInfo_Click(object sender, RoutedEventArgs e)
        {
            int orderId = (int)((Button)sender).Tag;
            DeliveryInfo win = new DeliveryInfo(orderId, userId, login);
            win.Show();
        }

        private void RestaurantInfo_Click(object sender, RoutedEventArgs e)
        {
            int restaurantId = (int)((Button)sender).Tag;
            RestaurantInfo win = new RestaurantInfo(restaurantId, userId, login);
            win.Show();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            Register win = new Register();
            win.Top = this.Top;
            win.Left = this.Left;
            win.Show();
            this.Close();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            Login win = new Login();
            win.Top = this.Top;
            win.Left = this.Left;
            win.Show();
            this.Close();
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            login = 0;
            userId = 0;
            admin = 0;
            Initialize();
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            Profile win = new Profile(userId);
            win.Top = this.Top;
            win.Left = this.Left;
            win.Show();
            this.Close();
        }

        private void CourierProfile_Click(object sender, RoutedEventArgs e)
        {
            CourierProfile win = new CourierProfile(userId);
            win.Top = this.Top;
            win.Left = this.Left;
            win.Show();
            this.Close();
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            stackPanelCouriers.Children.Clear();
            stackPanelButtons.Children.Clear();
            stackPanelDelete.Children.Clear();
            Initialize();
            InitializeCourier();
        }
    }
}