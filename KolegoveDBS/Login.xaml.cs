using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KolegoveDBS
{
    /// <summary>
    /// Interakční logika pro Login.xaml
    /// </summary>
    public partial class Login : Window
    {

        public Login()
        {
            InitializeComponent();
        }
        private string ToSha256(string s)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(s));

            var sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString();

        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string server = "localhost";
            string database = "kolegovedb";
            string uid = "root";
            string password = "admin";
            string constring = "Server=" + server + "; database=" + database + "; uid=" + uid + "; pwd=" + password;

            using (MySqlConnection con = new MySqlConnection(constring))
            {
                con.Open();
                string query = "SELECT COUNT(*), MAX(customer_id) FROM customer WHERE email = @Email AND password = @Password";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", EmailTB.Text);
                    cmd.Parameters.AddWithValue("@Password", ToSha256(PasswordTB.Text));
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int count = reader.GetInt32(0);
                            if (count > 0)
                            {
                                MessageBox.Show("Přihlášení úspěšné!");
                                MainWindow win = new MainWindow(1, reader.GetInt32(1), 0, 0);
                                win.Top = this.Top;
                                win.Left = this.Left;
                                win.Show();
                                this.Close();
                            }
                            else
                            {

                                MessageBox.Show("Neplatné přihlašovací údaje. Zkuste to znovu.");
                            }
                        }
                    }
                }
            }

            using (MySqlConnection con = new MySqlConnection(constring))
            {
                con.Open();
                string query = "SELECT COUNT(*), MAX(admin_id) FROM administrator WHERE email = @Email AND password = @Password";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", EmailTB.Text);
                    cmd.Parameters.AddWithValue("@Password", ToSha256(PasswordTB.Text));
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int count = reader.GetInt32(0);
                            if (count > 0)
                            {
                                MessageBox.Show("Přihlášení úspěšné!");
                                MainWindow win = new MainWindow(1, reader.GetInt32(1), 1, 0);
                                win.Top = this.Top;
                                win.Left = this.Left;
                                win.Show();
                                this.Close();
                            }
                            else
                            {
                                //MessageBox.Show("Neplatné přihlašovací údaje. Zkuste to znovu.");
                            }
                        }
                    }
                }
            }

            using (MySqlConnection con = new MySqlConnection(constring))
            {
                con.Open();
                string query = "SELECT COUNT(*), MAX(courier_id) FROM courier WHERE email = @Email AND password = @Password";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", EmailTB.Text);
                    cmd.Parameters.AddWithValue("@Password", ToSha256(PasswordTB.Text));
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int count = reader.GetInt32(0);
                            if (count > 0)
                            {
                                MessageBox.Show("Přihlášení úspěšné!");
                                MainWindow win = new MainWindow(1, reader.GetInt32(1), 0, 1);
                                win.Top = this.Top;
                                win.Left = this.Left;
                                win.Show();
                                this.Close();
                            }
                            else
                            {
                                //MessageBox.Show("Neplatné přihlašovací údaje. Zkuste to znovu.");
                            }
                        }
                    }
                }
            }

        }
    }
}

