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
    /// Interaction logic for Field.xaml
    /// </summary>

    public partial class Field : System.Windows.Controls.UserControl, FieldInterface
    {
        private string _name;
        private double _price;
        private Image _image;
        private Brush _headerColor;
        private bool _isFree;
        private bool _readyToBuy;

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

        /// <summary>
        /// Czy wolne
        /// </summary>
        public bool IsFree
        {
            get { return this._isFree; }
            set { this._isFree = value; }
        }

        /// <summary>
        /// Gotowy w tej chwili do kupna
        /// </summary>
        public bool ReadyToBuy
        {
            set { this._readyToBuy = value; }
        }

        public Pawn Pawn
        {
            set { PlaceForPawn.Children.Add(value); }
        }

        #endregion

        /// <summary>
        /// Konstruktor pola
        /// </summary>
        /// <param name="_Name">Nazwa miasta</param>
        /// <param name="_Price">Cena miasta</param>
        /// <param name="_Image">Zdjêcie</param>
        /// <param name="_HeaderColor">Kolor nag³ówka</param>
        public Field(string _Name,double _Price,Image _Image,Brush _HeaderColor)
        {
            InitializeComponent();
            CityName = _Name;
            Price = _Price;
            Image = _Image;
            HeaderColor = _HeaderColor;
            IsFree = true;
            _readyToBuy = false;
            Load();
           
        }

        /// <summary>
        /// Pokazanie okna z informacjami o mieœcie
        /// </summary>
        public void Show()
        {
            ShowField hh = new ShowField(HeaderColor,CityName,Image,Price.ToString());
            if (_readyToBuy == true)
            {
                hh.ShowDialogButtons();
            }
            hh.ShowDialog();

            if (hh.DialogResult == true)
                _isFree = false;
        }

        /// <summary>
        /// Prze³adowanie skórki
        /// </summary>
        /// <param name="_Name">Nazwa miasta</param>
        /// <param name="_Price">Cena miasta</param>
        /// <param name="_Image">Zdjêcie</param>
        /// <param name="_HeaderColor">Kolor nag³ówka</param>
        public void Reload(string _Name, double _Price, Image _Image, Brush _HeaderColor)
        {
            CityName = _Name;
            Price = _Price;
            Image.Source = _Image.Source;
            HeaderColor = _HeaderColor;
            Load();
        }

        /// <summary>
        /// Zastosowanie zmian
        /// </summary>
        public void Load()
        {
            _CityName.Content = CityName;
            _Header.Fill = HeaderColor;
            _Price.Content = Price.ToString();
            _Image.Source = Image.Source;
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