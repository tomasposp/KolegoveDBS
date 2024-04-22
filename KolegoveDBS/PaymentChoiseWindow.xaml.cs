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
    /// Interakční logika pro PaymentChoiceWindow.xaml
    /// </summary>
    public partial class PaymentChoiceWindow : Window
    {
        public Boolean paid = false;


        public PaymentChoiceWindow(decimal totalAmount)
        {
            InitializeComponent();
            Total.Text = totalAmount.ToString();

        }

        private void PayNow_Click(object sender, RoutedEventArgs e)
        {
            paid = true;
            MessageBox.Show("Děkujeme za nákup!");
            Close();
        }

        private void PayLater_Click(object sender, RoutedEventArgs e)
        {
            paid = false;
            MessageBox.Show("Děkujeme za nákup!");
            Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}