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
            this.Header.Fill = _Header;
            this.Image = _Image;
            this.City.Content = _City;
            this.Price.Content = _Price;
            InitializeComponent();
        }

    }
}