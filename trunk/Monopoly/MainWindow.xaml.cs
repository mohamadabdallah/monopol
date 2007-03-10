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
using PlayMate;

namespace Monopoly
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

    public partial class MainWindow : System.Windows.Window
    {
        //plansza:)
        private Mate plansza;

        public MainWindow()
        {
            InitializeComponent();
            plansza = new Mate("");
            _matePanel.Children.Add(plansza);
            
        }

    }
}