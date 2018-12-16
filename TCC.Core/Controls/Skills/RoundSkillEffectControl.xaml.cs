﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using TCC.Properties;
using TCC.ViewModels;

namespace TCC.Controls.Skills
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>  
    /// Logica di interazione per LancerBuffCooldownControl.xaml
    /// </summary>
    public partial class RoundSkillEffectControl : INotifyPropertyChanged
    {
        public RoundSkillEffectControl()
        {
            InitializeComponent();
        }

        private DurationCooldownIndicator _context;
        private DoubleAnimation _anim;
        public string DurationLabel => _context == null? "": Utils.TimeFormatter(_context.Buff.Seconds);
        public bool ShowEffectSeconds => _context?.Buff != null && _context.Buff.Seconds > 0;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //externalArc.BeginAnimation(Arc.EndAngleProperty, new DoubleAnimation(359.9, 0, TimeSpan.FromMilliseconds(50000)));
            if (DesignerProperties.GetIsInDesignMode(this) || DataContext == null) return;
            _context = (DurationCooldownIndicator)DataContext;
            FixedSkillControl.DataContext = _context.Cooldown;
            _context.Buff.Started += OnBuffStarted;
            _context.Buff.SecondsUpdated += OnSecondsUpdated;
            _context.Buff.Ended += OnBuffEnded;
            _anim = new DoubleAnimation(359.9, 0, TimeSpan.FromMilliseconds(_context.Buff.Duration));
        }

        private void OnBuffEnded(Data.CooldownMode obj)
        {
            ExternalArc.BeginAnimation(Arc.EndAngleProperty, null);
            ExternalArc.EndAngle = 0;
        }

        private void OnSecondsUpdated()
        {
            NPC(nameof(DurationLabel));
            NPC(nameof(ShowEffectSeconds));
        }

        private void OnBuffStarted(Data.CooldownMode obj)
        {
            _anim.Duration = TimeSpan.FromMilliseconds(_context.Buff.Duration);
            ExternalArc.BeginAnimation(Arc.EndAngleProperty, _anim);

        }

        //private void Buff_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    Dispatcher.InvokeIfRequired(() =>
        //    {

        //        if (e.PropertyName == "Start")
        //        {
        //            _anim.Duration = TimeSpan.FromMilliseconds(_context.Buff.Cooldown);
        //            ExternalArc.BeginAnimation(Arc.EndAngleProperty, _anim);
        //            return;
        //        }
        //        if (e.PropertyName == "Refresh")
        //        {
        //            _anim.Duration = TimeSpan.FromMilliseconds(_context.Buff.Cooldown);
        //            ExternalArc.BeginAnimation(Arc.EndAngleProperty, _anim);
        //            return;
        //        }
        //        if (e.PropertyName == nameof(_context.Buff.Seconds))
        //        {
        //            NPC(nameof(DurationLabel));
        //        }

        //    }, System.Windows.Threading.DispatcherPriority.DataBind);

        //}

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void NPC([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
