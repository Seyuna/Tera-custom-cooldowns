﻿using System.Windows.Input;
using TCC.Data.Chat;
using TCC.ViewModels;

namespace TCC.Controls.Chat
{
    /// <summary>
    /// Interaction logic for BrokerOfferBody.xaml
    /// </summary>
    public partial class BrokerOfferBody
    {
        public BrokerOfferBody()
        {
            InitializeComponent();
        }

        private void Accept(object sender, MouseButtonEventArgs e)
        {
            var dc = (BrokerChatMessage) DataContext;
            Proxy.Proxy.AcceptBrokerOffer(dc.PlayerId, dc.ListingId);
            ChatWindowManager.Instance.SetPaused(false, dc);
            ChatWindowManager.Instance.ScrollToBottom();

        }

        private void Decline(object sender, MouseButtonEventArgs e)
        {
            var dc = (BrokerChatMessage) DataContext;
            if (dc.Handled) return;
            Proxy.Proxy.DeclineBrokerOffer(dc.PlayerId, dc.ListingId);
            OnHandled();
            ChatWindowManager.Instance.SetPaused(false, dc);
            ChatWindowManager.Instance.ScrollToBottom();

        }

        private void OnHandled()
        {
            var dc = (BrokerChatMessage) DataContext;
            dc.Handled = true;
            //declineButton.Visibility = Visibility.Collapsed;
            //acceptButton.Visibility = Visibility.Collapsed;
        }

        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            var dc = (BrokerChatMessage) DataContext;
            ChatWindowManager.Instance.SetPaused(true, dc);
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                var dc = (BrokerChatMessage) DataContext;
                ChatWindowManager.Instance.SetPaused(false, dc);
            }
            catch 
            {
                ChatWindowManager.Instance.SetPaused(false);
            }
            finally
            {
                ChatWindowManager.Instance.ScrollToBottom();
            }
        }
    }
}