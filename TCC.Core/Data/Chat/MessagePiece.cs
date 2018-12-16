﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using TCC.Annotations;
using TCC.Converters;
using TCC.Data.Map;
using TCC.ViewModels;

namespace TCC.Data.Chat
{
    public class MessagePiece : TSPropertyChanged, IDisposable
    {
        private ChatMessage _container;

        public ChatMessage Container
        {
            private get => _container;
            set
            {
                if (_container == value) return;
                _container = value;
                if (Color == null)
                {
                    var conv = new ChatChannelToColorConverter();
                    var col = ((SolidColorBrush)conv.Convert(Container.Channel, null, null, null));
                    Color = col;
                }
            }
        }

        public long ItemUid { get; set; }
        public uint ItemId { get; set; }
        public Location Location { [UsedImplicitly] get; set; }
        public string RawLink { get; set; }
        public Money Money { get; set; }

        public BoundType BoundType { get; set; }

        public Thickness Spaces { get; set; }

        public string OwnerName { get; set; }

        public MessagePieceType Type { get; set; }

        public string Text { get; set; }

        public string PlainText => Text.StartsWith("<") ? Text.Substring(1, Text.Length - 2) : Text;

        public SolidColorBrush Color { get; set; }

        private int _size = 18;
        private bool _customSize;
        private bool _isVisible;

        public int Size
        {
            get => _customSize ? _size : Settings.SettingsHolder.FontSize;
            set
            {
                if (_size == value) return;
                _size = value;
                _customSize = value != Settings.SettingsHolder.FontSize;
                N(nameof(Size));
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (value) SettingsWindowViewModel.FontSizeChanged += OnFontSizeChanged;
                else SettingsWindowViewModel.FontSizeChanged -= OnFontSizeChanged;

                if (_isVisible == value) return;
                _isVisible = value;
                N();
            }
        }

        private bool _isHovered;
        public bool IsHovered
        {
            get => _isHovered;
            set
            {
                if (_isHovered == value) return;
                _isHovered = value;
                if (Container != null)
                { 
                    var sameType = Container.Pieces.Where(x => x.Type == Type && x.RawLink == RawLink);
                    sameType.ToList().ForEach(x => x.IsHovered = IsHovered);
                }
                N();
            }
        }

        private Thickness SetThickness(string text)
        {
            double left = 0;
            double right = 0;
            if (text.StartsWith(" "))
            {
                left = 0;
                right = -1;
            }
            if (text.EndsWith(" "))
            {
                right = 4;
                left = -1;
            }

            return new Thickness(left, 0, right, 0);

        }
        public void SetColor(string color)
        {
            if (color == "") return;
            Dispatcher.Invoke(() =>
            {
                //if (color == "")
                //{
                //    var conv = new ChatChannelToColorConverter();
                //    var col = ((SolidColorBrush)conv.Convert(Container.Channel, null, null, null));
                //    Color = col;
                //}
                //else
                //{
                //try
                //{
                Color = new SolidColorBrush(Utils.ParseColor(color));
                //}
                //catch
                //{
                //    var conv = new ChatChannelToColorConverter();
                //    var col = ((SolidColorBrush)conv.Convert(Container.Channel, null, null, null));
                //    Color = col;
                //}
                //}
            });
        }
        public MessagePiece(string text, MessagePieceType type, int size, bool customSize, string col = "") : this(text)
        {
            if (col != "") SetColor(col);
            Type = type;

            _size = size;
            _customSize = customSize;

        }
        public MessagePiece(string text)
        {
            Dispatcher = ChatWindowManager.Instance.GetDispatcher();
            Text = text;
            Spaces = SetThickness(text);
            _customSize = false;

        }

        private void OnFontSizeChanged()
        {
            N(nameof(Size));
        }

        public MessagePiece(Money money) : this(text: "")
        {
            Type = MessagePieceType.Money;
            Money = money;
            _customSize = false;
        }

        public void Dispose()
        {
            Color = null;
            _container = null;
        }
    }
}
