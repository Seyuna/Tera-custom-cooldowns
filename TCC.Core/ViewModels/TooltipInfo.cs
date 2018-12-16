﻿using System.Linq;
using System.Windows.Threading;
using TCC.Data;

namespace TCC.ViewModels
{
    public class TooltipInfo : TSPropertyChanged
    {
        private string _name;
        public string Name
        {
            get => _name; set
            {
                if (_name == value) return;
                _name = value;
                N(nameof(Name));
                N(nameof(BlockLabelText));
                N(nameof(ShowAddFriend));
                N(nameof(ShowWhisper));
            }
        }
        private string _info;
        public string Info
        {
            get => _info; set
            {
                if (_info == value) return; _info = value;
                N(nameof(Info));
            }
        }
        private int _level;
        public int Level
        {
            get => _level; set
            {
                if (_level == value) return; _level = value; N(nameof(Level));
            }
        }
        private Class _charClass;
        public Class Class
        {
            get => _charClass;
            set
            {
                if (_charClass == value) return;
                _charClass = value;
                N(nameof(Class));
            }
        }
        private bool _showPartyInvite;
        public bool ShowPartyInvite
        {
            get => _showPartyInvite;
            set
            {
                if (_showPartyInvite == value) return;
                _showPartyInvite = value;
                N(nameof(ShowPartyInvite));
            }
        }
        private bool _showGuildInvite;
        public bool ShowGuildInvite
        {
            get => _showGuildInvite;
            set
            {
                if (_showGuildInvite == value) return;
                _showGuildInvite = value;
                N(nameof(ShowGuildInvite));
            }
        }

        public bool ShowAddFriend => !IsBlocked;
        public bool ShowWhisper => !IsBlocked;
        public string BlockLabelText => IsBlocked ? "Unblock" : "Block";
        public string FriendLabelText => IsFriend ? "Remove friend" : "Add friend";
        public string PowersLabelText => !GroupWindowViewModel.Instance.HasPowers(Name) ? "Grant invite power" : "Revoke invite power";

        public bool ShowGrantPowers => GroupWindowViewModel.Instance.AmILeader && GroupWindowViewModel.Instance.Raid && GroupWindowViewModel.Instance.Exists(Name) && Name != SessionManager.CurrentPlayer.Name;
        public bool ShowKick => GroupWindowViewModel.Instance.Exists(Name) && Name != SessionManager.CurrentPlayer.Name;
        public bool ShowDelegateLeader => GroupWindowViewModel.Instance.AmILeader && GroupWindowViewModel.Instance.Exists(Name) && Name != SessionManager.CurrentPlayer.Name;
        public bool IsBlocked => ChatWindowManager.Instance.BlockedUsers.Contains(_name);
        public bool IsFriend
        {
            get
            {
                var f = ChatWindowManager.Instance.Friends.FirstOrDefault(x => x.Name == _name);
                return f != null;
            }
        }
        public bool ShowFpsUtils => Proxy.Proxy.IsConnected && Proxy.Proxy.IsFpsUtilsAvailable;
        public TooltipInfo(string n, string i, int l)
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
            Name = n;
            Info = i;
            Level = l;
        }
        public void Refresh()
        {
            N(nameof(ShowPartyInvite));
            N(nameof(ShowGuildInvite));
            N(nameof(ShowAddFriend));
            N(nameof(ShowKick));
            N(nameof(ShowWhisper));
            N(nameof(BlockLabelText));
            N(nameof(FriendLabelText));
            N(nameof(IsBlocked));
            N(nameof(IsFriend));
            N(nameof(PowersLabelText));
            N(nameof(ShowDelegateLeader));
            N(nameof(ShowGrantPowers));
            N(nameof(ShowFpsUtils));
        }
        public void SetInfo(uint model)
        {
            var c = (model % 100) - 1;
            Class = (Class)c;
        }
    }
}
