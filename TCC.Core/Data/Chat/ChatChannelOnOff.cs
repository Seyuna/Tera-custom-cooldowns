﻿using System.Windows.Threading;

namespace TCC.Data.Chat
{
    public class ChatChannelOnOff : TSPropertyChanged
    {
        private bool _enabled;
        private ChatChannel _channel;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value) return;
                _enabled = value;
                N(nameof(Enabled));
            }
        }
        public ChatChannel Channel
        {
            get => _channel;
            set
            {
                if (_channel == value) return;
                _channel = value;
                N(nameof(Channel));
            }
        }

        public ChatChannelOnOff()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
        }
        public ChatChannelOnOff(ChatChannel ch, bool en = true) : this()
        {
            Channel = ch;
            Enabled = en;
        }
    }
}
