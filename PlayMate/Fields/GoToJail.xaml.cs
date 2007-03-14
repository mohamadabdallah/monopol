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

namespace PlayMate.Fields
{
    /// <summary>
    /// Interaction logic for GoToJail.xaml
    /// </summary>

    public partial class GoToJail : System.Windows.Controls.UserControl, FieldInterface
    {
        private string _name;
        private double _price;
        private Image _image;
        private Brush _headerColor;

        #region Accessors

        /// <summary>
        /// Nazwa miasta
        /// </summary>
        public string CityName
        {
            get { return this._name; }
            set { this._name = value; }
        }

        /// <summary>
        /// Cena miasta
        /// </summary>
        public double Price
        {
            get { return this._price; }
            set { this._price = value; }
        }

        /// <summary>
        /// Zdjecie miasta
        /// </summary>
        public Image Image
        {
            get { return this._image; }
            set { this._image = value; }
        }

        /// <summary>
        /// Kolor nag³ówka
        /// </summary>
        public Brush HeaderColor
        {
            get { return this._headerColor; }
            set { this._headerColor = value; }
        }

        public Pawn Pawn
        {
            set { PlaceForPawn.Children.Add(value); }
        }
        #endregion


        public GoToJail(Image image)
        {
            _image = new Image();
            InitializeComponent();
            _image.Source = image.Source;
            ImageBorder.Child = _image;
        }

        /// <summary>
        /// Pokazanie okna 
        /// </summary>
        public void Show()
        {
            ShowField hh = new ShowField(Brushes.White, "", _image, "");
            hh.ShowDialog();
        }

        public void ClearPawn(Pawn pawn)
        {
            PlaceForPawn.Children.Remove(pawn);
        }

        public bool PawnExists(Pawn pawn)
        {
            foreach (UIElement pp in PlaceForPawn.Children)
            {
                if ((pp as Pawn).Name == pawn.Name)
                    return true;
            }
            return false;
        }
    }
}