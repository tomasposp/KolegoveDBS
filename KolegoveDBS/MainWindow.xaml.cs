using MySql.Data.MySqlClient;
using System.Reflection;
using System.Reflection.PortableExecutable;
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
        public MainWindow() {
            InitializeComponent();

            Initialize();
        }
        public MainWindow(int login, int userId)
        {
            this.login = login;
            this.userId = userId;
            InitializeComponent();

            Initialize();

        }

        private void Initialize() {
            if (login == 0)
            {
                SignOut.Visibility = Visibility.Hidden;
                Login.Visibility = Visibility.Visible;
                Register.Visibility = Visibility.Visible;
                Profile.Visibility = Visibility.Hidden;
            }
            if (login == 1)
            {
                SignOut.Visibility = Visibility.Visible;
                Login.Visibility = Visibility.Hidden;
                Register.Visibility = Visibility.Hidden;
                Profile.Visibility = Visibility.Visible;
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
                    }
                }
            }
        }
        private void RestaurantInfo_Click(object sender, RoutedEventArgs e)
        {
            int restaurantId = (int)((Button)sender).Tag;
            RestaurantInfo win = new RestaurantInfo(restaurantId);
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
    }
}