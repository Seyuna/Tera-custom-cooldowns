﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TCC.Data;

namespace TCC.Converters
{
    public class WarriorStanceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var col = parameter != null && parameter.ToString().IndexOf("color", StringComparison.Ordinal) != -1;
            var light = parameter != null && parameter.ToString().IndexOf("light", StringComparison.Ordinal) != -1;
            var fallback = parameter != null && parameter.ToString().IndexOf("fallback", StringComparison.Ordinal) != -1;
            var lightFback = Colors.DarkGray;
            lightFback.A = 170;
            var color = fallback ? light ?  Colors.White : lightFback : Colors.Transparent;

            if (value != null)
            {
                var s = (WarriorStance)value;
                switch (s)
                {
                    case WarriorStance.Assault:
                        color = (Color)Application.Current.FindResource($"AssaultStanceColor{(light ? "Light" : "")}");
                        break;
                    case WarriorStance.Defensive:
                        color = (Color)Application.Current.FindResource($"DefensiveStanceColor{(light ? "Light" : "")}");
                        break;
                }
            }

            if (col) return color;
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}