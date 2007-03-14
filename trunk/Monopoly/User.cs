using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using PlayMate;

namespace Monopoly
{
    public class User
    {
        private string _nick;
        private Pawn _pawn;
        private double _money;
        private int _position;
        private int _lastPosition;
        private Collection<int> _RealEstates = new Collection<int>();


        #region Accessors

        /// <summary>
        /// Nick gracza
        /// </summary>
        public string Nick
        {
            get { return _nick; }
            set { _nick = value; }
        }

        /// <summary>
        /// Pionek gracza
        /// </summary>
        public Pawn Pawn
        {
            get { return _pawn; }
            set { _pawn = value; }
        }

        /// <summary>
        /// Iloœc pieniêdzy gracza
        /// </summary>
        public double Money
        {
            get { return _money; }
            set { _money = value; }
        }

        /// <summary>
        /// NIeruchomoœci gracza
        /// </summary>
        public Collection<int> RealEstates
        {
            get { return _RealEstates; }
            set { _RealEstates = value; }
        }

        /// <summary>
        /// Numer pola na ktorym znajduje sie uzytkownik
        /// </summary>
        public int Positon
        {
            get { return _position; }
            set { _lastPosition = _position; _position = value; }
        }
        /// <summary>
        /// Numer pola na ktorym znajdowal sie uzytkownik w poprzedniej turze
        /// </summary>
        public int LastPositon
        {
            get { return _lastPosition; }
            set { _lastPosition = value; }
        }

        #endregion

        public User(string Nick, Pawn Pawn)
        {
            _nick = Nick;
            _pawn = Pawn;
            Pawn.Name = _nick;
        }
    }
}
