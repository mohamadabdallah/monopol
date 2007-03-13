using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace PlayMate.Fields
{
    public interface FieldInterface
    {
        /// <summary>
        /// Nazwa miasta
        /// </summary>
        string CityName
        {
            get;
            set;
        }

        /// <summary>
        /// Cena miasta
        /// </summary>
        double Price
        {
            get;
            set;
        }

        /// <summary>
        /// Zdjecie miasta
        /// </summary>
        Image Image
        {
            get;
            set;
        }

        /// <summary>
        /// Kolor nag³ówka
        /// </summary>
        Brush HeaderColor
        {
            get;
            set;
        }
    }
}
