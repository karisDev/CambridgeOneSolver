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
using CambridgeOneSolver.Infrastructure;
using CambridgeOneSolver.Models;
using CambridgeOneSolver.Views.CustomControls;

namespace CambridgeOneSolver.Views.UserControls
{
    /// <summary>
    /// Так как это окно работает отдельно от всей программы, то MVVM можно не добавлять
    /// </summary>
    public partial class DonatorcUC : UserControl
    {
        public DonatorcUC()
        {
            InitializeComponent();
            InitializeDonators();
        }
        async void InitializeDonators()
        {
            try
            {
                ServerRequests serverRequests = await ServerRequests.Donators(AppConstants.Email, AppConstants.Version);
                string[] messageText = serverRequests.DontatorsMessageBody;
                string[] messageDate = serverRequests.DontatorsMessageDate;
                for (int i = 0; i < messageDate.Length; i++)
                {
                    AddNewCard(messageText[i], messageDate[i]);
                }
            }
            catch (Exception ex)
            {
                AddNewCard(ex.Message, "Или интернет не работает");
            }
            //AddNewCard("Если вы это видите, значит у нас упал сервер :c", "Или интернет не работает");

        }
        void AddNewCard(string MessageText, string DateText)
        {
            DonatorCard donatorCard = new DonatorCard
            {
                MessageBody = MessageText,
                DateMark = DateText
            };
            DonatorsWrap.Children.Add(donatorCard);
        }
    }
}
