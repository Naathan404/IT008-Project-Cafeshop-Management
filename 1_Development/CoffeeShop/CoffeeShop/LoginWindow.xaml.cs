using CoffeeShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private List<(string Quote, string Author)> quotes = new List<(string, string)>
        {
            ("Coffee is a language in itself.", "Jackie Chan"),
            ("I orchestrate my mornings to the tune of coffee.", "Terri Guillemets"),
            ("Even bad coffee is better than no coffee at all.", "David Lynch"),
            ("Coffee is the balm of the heart and spirit.", "Cellini Caffe"),
            ("There’s nothing sweeter than a cup of bitter coffee.", "Rian Aditia"),
            ("Coffee is a kind of magic you can drink.", "Catherynne M. Valente"),
            ("Life’s too short for bad coffee.", "Nescafe Australia"),
            ("Everyone starts somewhere!", "Anonymous"),
        };

        private List<String> imgBannerSources = new List<string>()
        { 
            "/Assets/Images/imgBanner_1.png",
            "/Assets/Images/imgBanner_2.png",
            "/Assets/Images/imgBanner_3.png",
            "/Assets/Images/imgBanner_4.png",
            "/Assets/Images/imgBanner_5.jpg",
        };


        public LoginWindow()
        {
            InitializeComponent();
            GenerateRandomLoginUI();
        }
        private void GenerateRandomLoginUI()
        {
            var quote = quotes[new Random().Next(0, quotes.Count)];
            txblQuote.Text = "\"" + quote.Quote + "\"";
            txblAuthor.Text = "- " + quote.Author + " -";
            imgBanner.Source = new BitmapImage(new Uri(imgBannerSources[new Random().Next(0, imgBannerSources.Count)], UriKind.Relative));
        }
    }
}
