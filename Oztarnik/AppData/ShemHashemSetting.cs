﻿using Microsoft.VisualBasic;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Oztarnik.AppData
{
    public static class ShemHashemSetting
    {
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        private static bool _replaceShemHashemMode =  bool.TryParse(Interaction.GetSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "ShemHashem", "false"), out var result) && result;
        public static bool ReplaceShemHashemMode
        {
            get => _replaceShemHashemMode;
            set
            {
                if (value != _replaceShemHashemMode)
                {
                    _replaceShemHashemMode = value;
                    Interaction.SaveSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "ShemHashem", value.ToString());
                    OnStaticPropertyChanged(nameof(ReplaceShemHashemMode));
                }
            }
        }

        private static readonly Regex ShemHashemRegex = new Regex(@"(י\p{Mn}*)ה(\p{Mn}*)(ו\p{Mn}*)ה(\p{Mn}*)", RegexOptions.Compiled);
        //private static readonly Regex ShemElokimRegex = new Regex(@"(א\p{Mn}*ל\p{Mn}*ו?\p{Mn}*)ה(\p{Mn}*ים)", RegexOptions.Compiled);

        private static readonly Regex ShemElokimRegex = new Regex(
            @"(?<!\S)(א[\p{Mn}]*ל[\p{Mn}]*ו?[\p{Mn}]*)(ה)([\p{Mn}]*ים?)?(?![\p{Mn}\s]*(אחרים|מצרים|הגוים|העמים))",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string ReplaceShemHashem(this string input)
        {
            if (!ReplaceShemHashemMode)
                return input;

            input = ShemHashemRegex.Replace(input, "$1ק$2$3ק$4");
            input = ShemElokimRegex.Replace(input, "$1ק$3");

            return input;
        }
    }
}
