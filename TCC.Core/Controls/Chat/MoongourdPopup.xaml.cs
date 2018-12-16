﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using TCC.Annotations;
using TCC.Moongourd;

namespace TCC.Controls.Chat
{
    /// <summary>
    /// Logica di interazione per MoongourdPopup.xaml
    /// </summary>
    public partial class MoongourdPopup : INotifyPropertyChanged
    {
        private string _playerName;

        public MoongourdPopup()
        {
            InitializeComponent();
            Loaded += MoongourdPopup_Loaded;
            MouseLeave += (s, ev) => WindowManager.FloatingButton.ClosePlayerMenu();
        }

        public string PlayerName
        {
            get => _playerName;
            set
            {
                if (_playerName == value) return;
                _playerName = value;
                NPC();
            }
        }

        private void MoongourdPopup_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public void SetInfo(string name, string region)
        {
            Dispatcher.Invoke(() =>
            {
                PlayerName = name;
                return List.ItemsSource = null;
            });
            if (region.StartsWith("EU")) region = "EU";
            var mg = new MoongourdManager();
            mg.Started += () => Dispatcher.Invoke(() => { EmptyInfo.Text = "Loading..."; });
            mg.Done += (list) => Dispatcher.Invoke(() =>
            {
                EmptyInfo.Text = "No entries";
                List.ItemsSource = list;
            });
            mg.GetEncounters(name, region);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void NPC([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
