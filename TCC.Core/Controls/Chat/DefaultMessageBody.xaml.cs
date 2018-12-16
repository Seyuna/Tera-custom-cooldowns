﻿using Dragablz;
using System.Windows;
using System.Windows.Input;
using TCC.Data.Chat;
using TCC.ViewModels;

namespace TCC.Controls.Chat
{
    /// <summary>
    /// Interaction logic for DefaultMessageBody.xaml
    /// </summary>
    public partial class DefaultMessageBody
    {
        public DefaultMessageBody()
        {
            InitializeComponent();

        }

        private void WrapPanel_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var contextMenu = (sender as FrameworkElement)?.ContextMenu;
            if (contextMenu != null) contextMenu.IsOpen = true;
        }

        private void PinBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as ChatMessage;
            foreach (var w in ChatWindowManager.Instance.ChatWindows)
            {
                if (!w.IsMouseOver) continue;
                var currTabVm = w.TabControl.SelectedItem as HeaderedItemViewModel;
                var currTab = currTabVm?.Content as Tab;
                // ReSharper disable once PossibleNullReferenceException
                currTab.PinnedMessage = currTab.PinnedMessage == dc ? null : dc;
            }
        }

        private void CopyBtn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(((ChatMessage)DataContext).ToString());
            }
            catch
            {
                // ignored
            }
        }
    }
}
