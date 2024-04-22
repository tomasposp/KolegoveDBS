using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interakční logika pro Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        public Register()
        {
            InitializeComponent();


        }
        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
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
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder bu = new StringBuilder();
            if (NameTB.Text == "" || EmailTB.Text == "" || NumberTB.Text == "" || AddressTB.Text == "" || PasswordTB.Password == "")
            {
                MessageBox.Show("Pro registraci vyplňte všechny pole");
                return;
            }

            string server = "localhost";
            string database = "kolegovedb";
            string uid = "root";
            string password = "admin";
            string constring = "Server=" + server + "; database=" + database + "; uid=" + uid + "; pwd=" + password;
            using (MySqlConnection con = new MySqlConnection(constring))
            {
                con.Open();
                string selectQuery = "SELECT COUNT(*) FROM customer WHERE email = @email " +
                    " UNION ALL " +
                    " SELECT COUNT(*) FROM courier WHERE email = @email"
                    + " UNION ALL " + " SELECT COUNT(*) FROM administrator Where email = @email ;";
                using (MySqlCommand cmd = new MySqlCommand(selectQuery, con))
                {
                    cmd.Parameters.AddWithValue("@email", EmailTB.Text);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int count = reader.GetInt32(0);
                            if (count > 0)
                            {
                                MessageBox.Show("Tato emailová adresa je již zaregistrovaná!");
                                return;
                            }
                        }
                    }
                }
                string query = "INSERT INTO customer(name, email, phone, address, password, registration_date) VALUES(@Name, @Email, @Phone, @Address, @Password, @RegistrationDate)";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Name", NameTB.Text);
                    cmd.Parameters.AddWithValue("@Email", EmailTB.Text);
                    cmd.Parameters.AddWithValue("@Phone", NumberTB.Text);
                    cmd.Parameters.AddWithValue("@Address", AddressTB.Text);
                    cmd.Parameters.AddWithValue("@Password", ToSha256(PasswordTB.Password));
                    cmd.Parameters.AddWithValue("@RegistrationDate", DateTime.Now);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Záznam byl úspěšně vložen do databáze.");
                        MainWindow win = new MainWindow(0, 0, 0, 0);
                        win.Top = this.Top;
                        win.Left = this.Left;
                        win.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Nepodařilo se vložit záznam do databáze.");
                    }
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow win = new MainWindow(0, 0, 0, 0);
            win.Top = this.Top;
            win.Left = this.Left;
            win.Show();
            this.Close();
        }

    }
}
