﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TCC.Data;

namespace TCC.Controls.Classes.Elements
{
    /// <summary>
    /// Logica di interazione per TraverseCutControl.xaml
    /// </summary>
    public partial class TraverseCutControl : UserControl
    {
        private DoubleAnimation _toZeroAnimation;
        private DoubleAnimation _anim;
        private StatTracker _dc;
        private bool _isAnimating;
        private DispatcherTimer _delay;
        private uint _lastDuration;
        public string IconName { get; } = "icon_skills.dualrapidpiercing_tex";

        public TraverseCutControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (!(_dc is StatTracker st)) return;
            _dc.ToZero -= OnToZero;
            _dc.PropertyChanged -= OnPropertyChanged;
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _dc = (StatTracker)DataContext;

            _anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(100));
            _anim.Completed += (_, __) => _isAnimating = false;

            _toZeroAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(0));

            _delay = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(_anim.Duration.TimeSpan.Milliseconds + 10)
            };
            _delay.Tick += (_, __) =>
            {
                _delay.Stop();
                OnToZero(_lastDuration);
            };

            _dc.ToZero += OnToZero;
            _dc.PropertyChanged += OnPropertyChanged;
        }


        private void OnToZero(uint duration)
        {
            if (_isAnimating)
            {
                _lastDuration = duration;
                _delay.Start();
                return;
            }
            Dispatcher.Invoke(() =>
            {
                //var delay = _anim.Duration.TimeSpan.Milliseconds + 10;
                //_toZeroAnimation.BeginTime = TimeSpan.FromMilliseconds(_isAnimating ? delay : 0);
                _toZeroAnimation.Duration = TimeSpan.FromMilliseconds(duration);
                _toZeroAnimation.From = _dc.Factor * 359.9;
                ExternalArc.BeginAnimation(Arc.EndAngleProperty, _toZeroAnimation);
            });
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StatTracker.Factor))
            {
                _anim.To = _dc.Factor * 359.9;
                ExternalArc.BeginAnimation(Arc.EndAngleProperty, _anim);
                _isAnimating = true;
            }
        }
    }
}
