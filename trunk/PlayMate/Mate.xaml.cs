using System;
using System.Collections.Generic;
using System.IO;
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
        private List<System.Windows.Controls.UserControl> Fields;
        private string _skinName;

        public Mate(string path)
        {
            Fields = new List<System.Windows.Controls.UserControl>();
            InitializeComponent();
            ConstructMate(path,Fields);
            ReloadMate();

            #region logo
            Image img = new Image();
            string _path = System.IO.Path.Combine(Environment.CurrentDirectory, "monopoly.png");
            string _pa = _path.Replace("/", "\\");
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(_pa);
            myBitmapImage.EndInit();
            img.Source = myBitmapImage;
            image1.Source = img.Source;
            #endregion
        }

        /// <summary>
        /// Sk³adanie planszy
        /// </summary>
        /// <param name="path">miejsce ze zdjêciami pól</param>
        public void ConstructMate(string path, List<System.Windows.Controls.UserControl> fields)
        {
            BuildList(path);
        }

        private void BuildList(string path)
        {
            StreamReader file = new StreamReader(path);
            string line = "";
            bool end = false;
            try
            {
                line = file.ReadLine();
                if (String.IsNullOrEmpty(line))
                    throw new IOException("Wrong format");
                string[] tab = line.Split(':');
                if (tab[0] != "Name")
                {
                    throw new IOException("Wrong format");
                }
                _skinName = tab[1];

                line = file.ReadLine();

                if (line != "-start-")
                {
                    throw new IOException("Wrong format");
                }

                line = file.ReadLine();

                while (line != null)
                {
                    tab = line.Split(':');
                    switch(tab[0])
                    {
                        case "Start":
                            Image img1 = new Image();
                            img1.Width = 40;
                            img1.Height = 40;
                            string _path1 = System.IO.Path.Combine(Environment.CurrentDirectory, tab[2]);
                            string _pa1 = _path1.Replace("/", "\\");
                            BitmapImage myBitmapImage1 = new BitmapImage();
                            myBitmapImage1.BeginInit();
                            myBitmapImage1.UriSource = new Uri(_pa1);
                            myBitmapImage1.EndInit();
                            img1.Source = myBitmapImage1;
                            Fields.Add(new Fields.Start(img1));
                            break;
                        case "Jail":
                            img1 = new Image();
                            img1.Width = 40;
                            img1.Height = 40;
                             _path1 = System.IO.Path.Combine(Environment.CurrentDirectory, tab[2]);
                            _pa1 = _path1.Replace("/", "\\");
                             myBitmapImage1 = new BitmapImage();
                            myBitmapImage1.BeginInit();
                            myBitmapImage1.UriSource = new Uri(_pa1);
                            myBitmapImage1.EndInit();
                            img1.Source = myBitmapImage1;
                            Fields.Add(new Fields.Jail(img1));
                            break;
                        case "FreeParking":
                            img1 = new Image();
                            img1.Width = 40;
                            img1.Height = 40;
                            _path1 = System.IO.Path.Combine(Environment.CurrentDirectory, tab[2]);
                            _pa1 = _path1.Replace("/", "\\");
                            myBitmapImage1 = new BitmapImage();
                            myBitmapImage1.BeginInit();
                            myBitmapImage1.UriSource = new Uri(_pa1);
                            myBitmapImage1.EndInit();
                            img1.Source = myBitmapImage1;
                            Fields.Add(new Fields.FreeParking(img1));
                            break;
                        case "GoToJail":
                            img1 = new Image();
                            img1.Width = 40;
                            img1.Height = 40;
                            _path1 = System.IO.Path.Combine(Environment.CurrentDirectory, tab[2]);
                            _pa1 = _path1.Replace("/", "\\");
                            myBitmapImage1 = new BitmapImage();
                            myBitmapImage1.BeginInit();
                            myBitmapImage1.UriSource = new Uri(_pa1);
                            myBitmapImage1.EndInit();
                            img1.Source = myBitmapImage1;
                            Fields.Add(new Fields.GoToJail(img1));
                            break;
                        case "ComunityChest":
                            img1 = new Image();
                            img1.Width = 40;
                            img1.Height = 40;
                            _path1 = System.IO.Path.Combine(Environment.CurrentDirectory, tab[1]);
                            _pa1 = _path1.Replace("/", "\\");
                            myBitmapImage1 = new BitmapImage();
                            myBitmapImage1.BeginInit();
                            myBitmapImage1.UriSource = new Uri(_pa1);
                            myBitmapImage1.EndInit();
                            img1.Source = myBitmapImage1;
                            Fields.Add(new Fields.ComunityChest(img1));
                            break;
                        case "Chance":
                            img1 = new Image();
                            img1.Width = 40;
                            img1.Height = 40;
                            _path1 = System.IO.Path.Combine(Environment.CurrentDirectory, tab[1]);
                            _pa1 = _path1.Replace("/", "\\");
                            myBitmapImage1 = new BitmapImage();
                            myBitmapImage1.BeginInit();
                            myBitmapImage1.UriSource = new Uri(_pa1);
                            myBitmapImage1.EndInit();
                            img1.Source = myBitmapImage1;
                            Fields.Add(new Fields.Chance(img1));
                            break;
                        case "f":
                            double pric=Double.Parse(tab[5]);
                            Image img = new Image();
                            img.Width = 40;
                            img.Height = 40;
                            string _path = System.IO.Path.Combine(Environment.CurrentDirectory, tab[4]);
                            string _pa = _path.Replace("/","\\");
                            BitmapImage myBitmapImage = new BitmapImage();
                            myBitmapImage.BeginInit();
                            myBitmapImage.UriSource = new Uri(_pa);
                            myBitmapImage.EndInit();
                            Brush bru = Brushes.Black;
                            switch (tab[2])
                            {
                                case "Blue":
                                    bru = Brushes.Blue;
                                    break;
                                case "LightBlue":
                                    bru = Brushes.LightBlue;
                                    break;
                                case "Purple":
                                    bru = Brushes.Purple;
                                    break;
                                case "Orange":
                                    bru = Brushes.Orange;
                                    break;
                                case "Red":
                                    bru = Brushes.Red;
                                    break;
                                case "Yellow":
                                    bru = Brushes.Yellow;
                                    break;
                                case "Green":
                                    bru = Brushes.Green;
                                    break;
                                case "MediumBlue":
                                    bru = Brushes.MediumBlue;
                                    break;
                                case "Gold":
                                    bru = Brushes.Gold;
                                    break;
                                default:
                                    break;
                            }
                            img.Source = myBitmapImage;
                            
                            Fields.Field ftemp = new Fields.Field(tab[3], pric, img, bru);
                            //ftemp.Show();
                            Fields.Add(ftemp);
                            break;
                        case "-end-":
                            end = true;
                            break;
                        default:
                            throw new IOException("Wrong format");
                            
                    }
                    if (end == false)
                    {
                        line = file.ReadLine();
                    }
                    else
                    {
                        break;
                    }
                }


            }
            catch (IOException exc)
            {
                System.Windows.MessageBox.Show(exc.Message);
            }

        }

        private void ReloadMate()
        {
            
            this.Start.Child = Fields[0];
            this.f1.Child = Fields[1];
            this.f2.Child = Fields[2];
            this.f3.Child = Fields[3];
            this.f4.Child = Fields[4];
            this.f5.Child = Fields[5];
            this.f6.Child = Fields[6];
            this.f7.Child = Fields[7];
            this.f8.Child = Fields[8];
            this.f9.Child = Fields[9];
            this.Jail.Child = Fields[10];
            this.f10.Child = Fields[11];
            this.f11.Child = Fields[12];
            this.f12.Child = Fields[13];
            this.f13.Child = Fields[14];
            this.f14.Child = Fields[15];
            this.f15.Child = Fields[16];
            this.f16.Child = Fields[17];
            this.f17.Child = Fields[18];
            this.f18.Child = Fields[19];
            this.FreeParking.Child = Fields[20];
            this.f19.Child = Fields[21];
            this.f20.Child = Fields[22];
            this.f21.Child = Fields[23];
            this.f22.Child = Fields[24];
            this.f23.Child = Fields[25];
            this.f24.Child = Fields[26];
            this.f25.Child = Fields[27];
            this.f26.Child = Fields[28];
            this.f27.Child = Fields[29];
            this.GoToJail.Child = Fields[30];
            this.f28.Child = Fields[31];
            this.f29.Child = Fields[32];
            this.f30.Child = Fields[33];
            this.f31.Child = Fields[34];
            this.f32.Child = Fields[35];
            this.f33.Child = Fields[36];
            this.f34.Child = Fields[37];
            this.f35.Child = Fields[38];
            this.f36.Child = Fields[39];
        }


        #region komunikacja


        public void RunPawn(int index,int before,Pawn pawn)
        {

            #region porzednia pozycja
            if (before == 0)
            {
                if ((Fields[before] as Fields.Start).PawnExists(pawn) == true)
                (Fields[before] as Fields.Start).ClearPawn(pawn);
            }
            if (before == 10)
            {
                if ((Fields[before] as Fields.Jail).PawnExists(pawn) == true)
                (Fields[before] as Fields.Jail).ClearPawn(pawn);
            }
            if (before == 20)
            {
                if ((Fields[before] as Fields.FreeParking).PawnExists(pawn) == true)
                (Fields[before] as Fields.FreeParking).ClearPawn(pawn);
            }
            if (before == 30)
            {
                if ((Fields[before] as Fields.GoToJail).PawnExists(pawn) == true)
                (Fields[before] as Fields.GoToJail).ClearPawn(pawn);
            }
            if ((before == 2) | (before == 17) | (before == 33))
            {
                if ((Fields[before] as Fields.ComunityChest).PawnExists(pawn) == true)
                (Fields[before] as Fields.ComunityChest).ClearPawn(pawn);
            }
            if ((before == 7) | (before == 22) | (before == 36))
            {
                if ((Fields[before] as Fields.Chance).PawnExists(pawn) == true)
                (Fields[before] as Fields.Chance).ClearPawn(pawn);
            }
            if (before != 2) if (before != 17) if (before != 33) if (before != 7) if (before != 22) if (before != 36)
                                    if (before != 0) if (before != 10) if (before != 20) if (before != 30)
            {
                if ((Fields[before] as Fields.Field).PawnExists(pawn) == true)
                (Fields[before] as Fields.Field).ClearPawn(pawn);
            }
            #endregion

            #region nowa pozycja
            if (index == 0)
            {
                (Fields[index] as Fields.Start).Pawn = pawn;
            }
            if (index == 10)
            {
                (Fields[index] as Fields.Jail).Pawn = pawn;
            }
            if (index == 20)
            {
                (Fields[index] as Fields.FreeParking).Pawn = pawn;
            }
            if (index == 30)
            {
                (Fields[index] as Fields.GoToJail).Pawn = pawn;
            }
            if ((index == 2) | (index == 17) | (index == 33))
            {
                (Fields[index] as Fields.ComunityChest).Pawn = pawn;
            }
            if ((index == 7) | (index == 22) | (index == 36))
            {
                (Fields[index] as Fields.Chance).Pawn = pawn;
            }
            if (index != 2)if(index != 17)if(index != 33)if(index != 7)if(index != 22)if(index != 36)
                if(index != 0)if(index != 10)if(index != 20)if(index != 30)
            {
                (Fields[index] as Fields.Field).Pawn = pawn;
            }
            #endregion

        }

        /// <summary>
        /// Kupienie w³asnoœci
        /// </summary>
        public bool BuyProperty(int index)
        {
            #region Pominiêcie indeksów
            if (index != 0) if (index != 10) if (index != 20) if (index != 30)
            if (index != 2) if (index != 17) if(index != 33) if (index != 7)  
            if (index != 22) if (index != 36)  if (index != 2) if (index != 17) 
            if (index != 33) if (index != 7) if (index != 22) if (index != 36)
            if (index != 0) if (index != 10) if (index != 20) if (index != 30)
            #endregion
            if ((Fields[index] as Fields.Field).IsFree == true)
            {
                (Fields[index] as Fields.Field).ReadyToBuy = true;
                ShowFieldInfo(index);
                if ((Fields[index] as Fields.Field).IsFree == false)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Cena nieruchomosci
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double Price(int index)
        {
            return (Fields[index] as Fields.Field).Price;
        }

        /// <summary>
        /// Wyswietla informacje o polu
        /// </summary>
        /// <param name="index"></param>
        public void ShowFieldInfo(int index)
        {

            if (index == 0)
            {
                (Fields[index] as Fields.Start).Show();
            }
            if (index == 10)
            {
                (Fields[index] as Fields.Jail).Show();
            }
            if (index == 20)
            {
                (Fields[index] as Fields.FreeParking).Show();
            }
            if (index == 30)
            {
                (Fields[index] as Fields.GoToJail).Show();
            }
            if ((index == 2) | (index == 17) | (index == 33))
            {
                (Fields[index] as Fields.ComunityChest).Show();
            }
            if ((index == 7) | (index == 22) | (index == 36))
            {
                (Fields[index] as Fields.Chance).Show();
            }
            
            if (index != 2) if (index != 17) if (index != 33) if (index != 7) if (index != 22) if (index != 36)
                                    if (index != 0) if (index != 10) if (index != 20) if (index != 30)
                                                {
                                                    (Fields[index] as Fields.Field).Show();
                                                }
            
        }
        #endregion
    }
}