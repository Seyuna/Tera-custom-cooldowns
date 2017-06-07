﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TCC.Data;
using TCC.ViewModels;

namespace TCC.Controls
{
    /// <summary>
    /// Logica di interazione per MysticAurasIndicator.xaml
    /// </summary>
    public partial class MysticAurasIndicator : UserControl
    {
        public MysticAurasIndicator()
        {
            InitializeComponent();
        }

        AurasTracker _context;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            _context = (AurasTracker)DataContext;
            _context.PropertyChanged += _context_PropertyChanged;
        }

        private void _context_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "AuraChanged")
            {
                if (_context.CritAura)
                {
                    crit.Visibility = Visibility.Visible;
                }
                else
                {
                    crit.Visibility = Visibility.Hidden;
                }

                if (_context.ManaAura)
                {
                    mp.Visibility = Visibility.Visible;
                }
                else
                {
                    mp.Visibility = Visibility.Hidden;
                }
                if (_context.CritResAura)
                {
                    critRes.Visibility = Visibility.Visible;
                }
                else
                {
                    critRes.Visibility = Visibility.Hidden;
                }
                if (_context.SwiftAura)
                {
                    swift.Visibility = Visibility.Visible;
                }
                else
                {
                    swift.Visibility = Visibility.Hidden;
                }
            }
        }
    }
}