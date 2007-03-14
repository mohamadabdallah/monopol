using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections;

namespace PlayMate.Fields
{
    /// <summary>
    /// Interaction logic for ShowField.xaml
    /// </summary>

    public partial class ShowField : System.Windows.Window
    {

        public ShowField(Brush _Header,string _City,Image _Image,string _Price)
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.WindowStyle = WindowStyle.ToolWindow;
          
            InitializeComponent();
            Header.Fill = _Header ;
            Image.Source = _Image.Source;
            City.Content = _City;
            Price.Content = _Price;
            button1.Visibility = Visibility.Hidden;
            button2.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Pokazanie przycisków do dialogu
        /// </summary>
        public void ShowDialogButtons()
        {
            button1.Visibility = Visibility.Visible;
            button2.Visibility = Visibility.Visible;
        }

        private void Ok_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}