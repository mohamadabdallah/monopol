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

namespace MonopolyCommunicate
{
    /// <summary>
    /// Interaction logic for Communicate.xaml
    /// </summary>

    public partial class Communicate : System.Windows.Window
    {

        public Communicate(string Communicate)
        {
            InitializeComponent();
            richTextBox1.AppendText(Communicate);
        }

    }
}