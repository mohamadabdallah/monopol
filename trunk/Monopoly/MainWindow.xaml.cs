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
using System.Collections.ObjectModel;

namespace Monopoly
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

    public partial class MainWindow : System.Windows.Window
    {
        //plansza:)
        private Mate plansza;
        //uzytkownicy
        public Dictionary<string, User> _UsersList = new Dictionary<string, User>();

        //dostep do uzytkownikow
        public Dictionary<string, User> Users
        {
            get { return _UsersList; }
            set { _UsersList = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
            plansza = new Mate(System.IO.Path.Combine(Environment.CurrentDirectory ,@"Skins\Default.txt"));
            _matePanel.Children.Add(plansza);

            Scroll.ScrollToHorizontalOffset(4640.0);
            Scroll.ScrollToVerticalOffset(4820.0);
        }

        /// <summary>
        /// Dodanie uzytkownika do listy
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="pawn"></param>
        public void AddUser(string nick, string pawn)
        {
            //trzeba dorobic sprawdzanie czy na serwerze nie ma juz takiej nazwy nicka
            _UsersList.Add(nick,new User(nick,pawn));
        }

        /// <summary>
        /// Kupowanie nieruchomosci
        /// </summary>
        /// <param name="nick"></param>
        public void UserBuyProperty(string nick)
        {
            if (_UsersList[nick].Money >= plansza.Price(_UsersList[nick].Positon))
            {
               bool xx= plansza.BuyProperty(_UsersList[nick].Positon);
               if (xx == true)
                   _UsersList[nick].Money -= plansza.Price(_UsersList[nick].Positon);
                //rozeslac do wszystkich aktualne dane o uzytkowniku
            }
            else
            {
                MonopolyCommunicate.Communicate gg = new MonopolyCommunicate.Communicate("Nie mo¿esz kupic, bo masz \n za ma³o pieniêdzy.");
                gg.ShowDialog();
            }

        }

        //reszte zachowan musisz sobie dopisac bo sam nie wiem jakie jeszcze beda, przyklad masz u gory
        //dostep do uzytkownika jest poprzez liste a do pol poprzez plansze 
    }
}