﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TCC.Data;
using MessageBoxImage = TCC.Data.MessageBoxImage;

namespace TCC.Windows
{
    /// <summary>
    /// Logica di interazione per TccMessageBox.xaml
    /// </summary>
    public partial class TccMessageBox
    {
        private TccMessageBox()
        {
            InitializeComponent();
            Closing += OnClosing;
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private static TccMessageBox _messageBox;
        private static MessageBoxResult _result = MessageBoxResult.No;

        private static MessageBoxResult Show (string caption, string msg, MessageBoxType type)
        {
            switch (type)
            {
                case MessageBoxType.ConfirmationWithYesNo:
                    return Show(caption, msg, MessageBoxButton.YesNo, MessageBoxImage.Question);
                case MessageBoxType.ConfirmationWithYesNoCancel:
                    return Show(caption, msg, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                case MessageBoxType.Information:
                    return Show(caption, msg, MessageBoxButton.OK, MessageBoxImage.Information);
                case MessageBoxType.Error:
                    return Show(caption, msg, MessageBoxButton.OK, MessageBoxImage.Error);
                case MessageBoxType.Warning:
                    return Show(caption, msg, MessageBoxButton.OK, MessageBoxImage.Warning);
                default:
                    return MessageBoxResult.No;
            }
        }
        public static MessageBoxResult Show(string msg, MessageBoxType type)
        {
            return Show(string.Empty, msg, type);
        }
        public static MessageBoxResult Show(string msg)
        {
            return Show(string.Empty, msg, MessageBoxButton.OK, MessageBoxImage.None);
        }
        public static MessageBoxResult Show (string caption, string text)
        {
            return Show(caption, text, MessageBoxButton.OK, MessageBoxImage.None);
        }
        public static MessageBoxResult Show
        (string caption, string text, MessageBoxButton button)
        {
            return Show(caption, text, button, MessageBoxImage.None);
        }
        public static MessageBoxResult Show (string caption, string text, MessageBoxButton button, MessageBoxImage image)
        {
            if (_messageBox == null) App.BaseDispatcher.Invoke(Create);

            _messageBox?.Dispatcher.Invoke(() =>
            {
                _messageBox.TxtMsg.Text = text;
                _messageBox.MessageTitle.Text = caption;
                SetVisibilityOfButtons(button);
                SetImageOfMessageBox(image);
               _messageBox.ShowDialog();
            });
            return _result;
        }
        private static void SetVisibilityOfButtons(MessageBoxButton button)
        {
            _messageBox.BtnCancel.Visibility = Visibility.Visible;
            _messageBox.BtnNo.Visibility = Visibility.Visible;
            _messageBox.BtnYes.Visibility = Visibility.Visible;
            _messageBox.BtnOk.Visibility = Visibility.Visible;

            switch (button)
            {
                case MessageBoxButton.OK:
                    _messageBox.BtnCancel.Visibility = Visibility.Collapsed;
                    _messageBox.BtnNo.Visibility = Visibility.Collapsed;
                    _messageBox.BtnYes.Visibility = Visibility.Collapsed;
                    _messageBox.BtnOk.Focus();
                    break;
                case MessageBoxButton.OKCancel:
                    _messageBox.BtnNo.Visibility = Visibility.Collapsed;
                    _messageBox.BtnYes.Visibility = Visibility.Collapsed;
                    _messageBox.BtnCancel.Focus();
                    break;
                case MessageBoxButton.YesNo:
                    _messageBox.BtnOk.Visibility = Visibility.Collapsed;
                    _messageBox.BtnCancel.Visibility = Visibility.Collapsed;
                    _messageBox.BtnNo.Focus();
                    break;
                case MessageBoxButton.YesNoCancel:
                    _messageBox.BtnOk.Visibility = Visibility.Collapsed;
                    _messageBox.BtnCancel.Focus();
                    break;
            }
        }
        private static void SetImageOfMessageBox(MessageBoxImage image)
        {
            switch (image)
            {
                case MessageBoxImage.Warning:
                    //_messageBox.SetImage("Warning.png");
                    break;
                case MessageBoxImage.Question:
                    //_messageBox.SetImage("Question.png");
                    break;
                case MessageBoxImage.Information:
                    _messageBox.Bg.Background = R.Brushes.MpBrush;//Application.Current.FindResource("MpBrush") as SolidColorBrush;
                    //_messageBox.SetImage("Information.png");
                    break;
                case MessageBoxImage.Error:
                    //_messageBox.SetImage("Error.png");
                    _messageBox.Bg.Background = R.Brushes.HpBrush;//Application.Current.FindResource("HpBrush") as SolidColorBrush;
                    break;
            }
        }
        [SuppressMessage("ReSharper", "PossibleUnintendedReferenceComparison")]
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == BtnOk)
                _result = MessageBoxResult.OK;
            else if (sender == BtnYes)
                _result = MessageBoxResult.Yes;
            else if (sender == BtnNo)
                _result = MessageBoxResult.No;
            else if (sender == BtnCancel)
                _result = MessageBoxResult.Cancel;
            else
                _result = MessageBoxResult.None;
            BeginAnimation(OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase() });
            RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1, .8, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() });
            RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1, .8, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() });
            Task.Delay(250).ContinueWith(t =>
            {
                Dispatcher.Invoke(() =>
                {
                    _messageBox.Hide();
                    //_messageBox = null;
                });
            });
        }
        // ReSharper disable once UnusedMember.Local
        private void SetImage(string imageName)
        {
            var uri = $"/Resources/images/{imageName}";
            // ReSharper disable once UnusedVariable
            var uriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
            //img.Source = new BitmapImage(uriSource);
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((bool)e.NewValue) != true) return;
            BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase() });
            RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(.5, 1, TimeSpan.FromMilliseconds(500)) { EasingFunction = new ElasticEase() { Oscillations = 1 } });
            RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(.5, 1, TimeSpan.FromMilliseconds(500)) { EasingFunction = new ElasticEase() { Oscillations = 1 } });
        }

        private void BG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public static void Create()
        {
            _messageBox = new TccMessageBox();
        }
    }
}