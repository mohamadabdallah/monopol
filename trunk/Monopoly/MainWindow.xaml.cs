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
using System.Threading;

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
            _UsersList.Add("kk",new User("kk",new Pawn()));
            _UsersList["kk"].Positon = 0;
            UserRun("kk", 1);
        }

        /// <summary>
        /// Dodanie uzytkownika do listy
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="pawn"></param>
        public void AddUser(string nick, Pawn pawn)
        {
            //trzeba dorobic sprawdzanie czy na serwerze nie ma juz takiej nazwy nicka
            _UsersList.Add(nick,new User(nick,pawn));
        }

        //test rzutu kostka
        public void Run_Clicked(object sender, RoutedEventArgs e)
        {
            Random ff = new Random();
            UserRun("kk",ff.Next(1,6));
            plansza.ShowFieldInfo(_UsersList["kk"].Positon);
        }

        //tes kupowania
        public void Buy_Clicked(object sender, RoutedEventArgs e)
        {
            Random ff = new Random();
            UserRun("kk", ff.Next(1, 6));
            plansza.BuyProperty(_UsersList["kk"].Positon);
        }

        /// <summary>
        /// Metoda przemieszczajaca uzytkownika na pole wyrzucone kostka
        /// </summary>
        /// <param name="nick">nick uzytkownika</param>
        /// <param name="number">liczba oczek na kostce</param>
        public void UserRun(string nick,int number)
        {
            int _Pos = _UsersList[nick].Positon + number;
            if (_Pos >= 40)
            {
                _Pos = _Pos - 40;
            }
            _UsersList[nick].LastPositon = _UsersList[nick].Positon;
            _UsersList[nick].Positon = _Pos;
            plansza.RunPawn(_Pos,_UsersList[nick].LastPositon,_UsersList[nick].Pawn);
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
        //do szans i comunity chest uzyj komunikatu monopolycommunicate
        //serwer i tym podobne rzeczy podepnij sie pod przycisk  w menu opcje gry i stworz tam jakies okno z opcjami do ustawienia, typu ilosc uzytkownikowi takie tam bzdety
    }
}