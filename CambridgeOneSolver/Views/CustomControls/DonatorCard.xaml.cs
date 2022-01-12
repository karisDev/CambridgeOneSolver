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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CambridgeOneSolver.Views.CustomControls
{
    /// <summary>
    /// Interaction logic for DonatorCard.xaml
    /// </summary>
    public partial class DonatorCard : UserControl
    {
        public DonatorCard()
        {
            InitializeComponent();
        }
        public string MessageBody
        {
            get => (string)GetValue(MessageBodyProppety);
            set => SetValue(MessageBodyProppety, value);
        }
        public static readonly DependencyProperty MessageBodyProppety =
            DependencyProperty.Register("MessageBody", typeof(string), typeof(DonatorCard));
        //public string DateMark
        //{

        //}
        public static readonly DependencyProperty DateMarkProperty =
            DependencyProperty.Register("MessageBody", typeof(string), typeof(DonatorCard));

    }
}
