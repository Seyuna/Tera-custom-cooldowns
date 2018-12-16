﻿using System;
using System.Windows;
using System.Windows.Media.Animation;
using TCC.ViewModels;

namespace TCC.Controls.Classes
{
    /// <summary>
    /// Logica di interazione per ValkyrieBar.xaml
    /// </summary>
    public partial class ValkyrieBar
    {
        private ValkyrieBarManager _dc;
        private DoubleAnimation _an;
        private DoubleAnimation _rag;

        public ValkyrieBar()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _dc = (ValkyrieBarManager)DataContext;
            _an = new DoubleAnimation(_dc.StaminaTracker.Factor * 359.99 + 40, TimeSpan.FromMilliseconds(150));
            _rag = new DoubleAnimation(318, 42, TimeSpan.FromMilliseconds(0));
            _dc.StaminaTracker.PropertyChanged += ST_PropertyChanged;
            _dc.Ragnarok.Buff.Started += OnRagnarokStarted;
        }

        private void OnRagnarokStarted(Data.CooldownMode obj)
        {
            _rag.Duration = TimeSpan.FromMilliseconds(_dc.Ragnarok.Buff.OriginalDuration);
            RagArc.BeginAnimation(Arc.EndAngleProperty, _rag);
        }

        private void ST_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(_dc.StaminaTracker.Factor)) return;
            if (!_dc.Ragnarok.Buff.IsAvailable) return;
            _an.To = _dc.StaminaTracker.Factor * (359.99 - MainReArc.StartAngle*2) + MainReArc.StartAngle;
            MainReArc.BeginAnimation(Arc.EndAngleProperty, _an);
        }
    }
}
