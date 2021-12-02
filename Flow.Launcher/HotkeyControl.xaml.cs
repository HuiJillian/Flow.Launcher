﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Flow.Launcher.Core.Resource;
using Flow.Launcher.Helper;
using Flow.Launcher.Infrastructure.Hotkey;
using Flow.Launcher.Plugin;

namespace Flow.Launcher
{
    public partial class HotkeyControl : UserControl
    {
        public HotkeyModel CurrentHotkey { get; private set; }
        public bool CurrentHotkeyAvailable { get; private set; }

        public event EventHandler HotkeyChanged;

        protected virtual void OnHotkeyChanged() => HotkeyChanged?.Invoke(this, EventArgs.Empty);

        public HotkeyControl()
        {
            InitializeComponent();
        }

        void TbHotkey_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            tbMsg.Visibility = Visibility.Hidden;

            //when alt is pressed, the real key should be e.SystemKey
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            SpecialKeyState specialKeyState = GlobalHotkey.Instance.CheckModifiers();

            var hotkeyModel = new HotkeyModel(
                specialKeyState.AltPressed,
                specialKeyState.ShiftPressed,
                specialKeyState.WinPressed,
                specialKeyState.CtrlPressed,
                key);

            var hotkeyString = hotkeyModel.ToString();

            if (hotkeyString == tbHotkey.Text)
            {
                return;
            }

            Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(500);
                SetHotkey(hotkeyModel);
            });
        }

        public void SetHotkey(HotkeyModel keyModel, bool triggerValidate = true)
        {
            CurrentHotkey = keyModel;

            tbHotkey.Text = CurrentHotkey.ToString();
            tbHotkey.Select(tbHotkey.Text.Length, 0);

            if (triggerValidate)
            {
                CurrentHotkeyAvailable = CheckHotkeyAvailability();
                if (!CurrentHotkeyAvailable)
                {
                    tbMsg.Foreground = new SolidColorBrush(Colors.Red);
                    tbMsg.Text = InternationalizationManager.Instance.GetTranslation("hotkeyUnavailable");
                }
                else
                {
                    tbMsg.Foreground = new SolidColorBrush(Colors.Green);
                    tbMsg.Text = InternationalizationManager.Instance.GetTranslation("success");
                }
                tbMsg.Visibility = Visibility.Visible;
                OnHotkeyChanged();
            }
        }

        public void SetHotkey(string keyStr, bool triggerValidate = true)
        {
            SetHotkey(new HotkeyModel(keyStr), triggerValidate);
        }

        private bool CheckHotkeyAvailability() => HotKeyMapper.CheckAvailability(CurrentHotkey);

        public new bool IsFocused
        {
            get { return tbHotkey.IsFocused; }
        }
    }
}
