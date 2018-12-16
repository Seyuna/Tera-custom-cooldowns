﻿using System.Windows.Input;
using TCC.Data.Chat;

namespace TCC.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChannelSetting.xaml
    /// </summary>
    public partial class ChannelSetting
    {
        public ChannelSetting()
        {
            InitializeComponent();
        }

        private void Ellipse_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dc = (ChatChannelOnOff)DataContext;
            dc.Enabled = !dc.Enabled;
        }
    }
}
