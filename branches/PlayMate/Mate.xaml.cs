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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlayMate
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>

    public partial class Mate : System.Windows.Controls.UserControl
    {
        private List<UserControl> Fields;

        public Mate(string path)
        {
            Fields = new List<UserControl>();
            InitializeComponent();
            ConstructMate(path,Fields);
        }

        /// <summary>
        /// Sk³adanie planszy
        /// </summary>
        /// <param name="path">miejsce ze zdjêciami pól</param>
        public void ConstructMate(string path, List<UserControl> fields)
        {
        }
    }
}