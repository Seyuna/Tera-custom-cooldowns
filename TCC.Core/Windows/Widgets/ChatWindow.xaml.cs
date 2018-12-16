﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Dragablz;
using GongSolutions.Wpf.DragDrop.Utilities;
using TCC.Settings;
using TCC.ViewModels;

namespace TCC.Windows.Widgets
{

    public partial class ChatWindow
    {
        private readonly DoubleAnimation _opacityUp;
        private readonly DoubleAnimation _opacityDown;
        private bool _bottom = true;
        public ChatViewModel VM => Dispatcher.Invoke(() => DataContext as ChatViewModel);
        public bool IsPaused => Dispatcher.Invoke(() => VM.Paused);

        private ChatWindow(ChatWindowSettings ws)
        {
            InitializeComponent();
            //ButtonsRef = buttons;
            MainContent = content;
            Init(ws, false, true, false);
            _opacityUp = new DoubleAnimation(1, TimeSpan.FromMilliseconds(120));
            _opacityDown = new DoubleAnimation(0.01, TimeSpan.FromMilliseconds(120));
            AddHandler(DragablzItem.IsDraggingChangedEvent, new RoutedPropertyChangedEventHandler<bool>(OnDragCompleted));
        }

        private void OnVisibilityChanged(bool obj)
        {
            if ((WindowSettings as ChatWindowSettings).FadeOut) AnimateChatVisibility(obj);
        }


        public ChatWindow(ChatWindowSettings ws, ChatViewModel vm) : this(ws)
        {
            DataContext = vm;
            VM.WindowSettings = ws;

            //UpdateSettings();
            (WindowSettings as ChatWindowSettings).FadeoutChanged += () => VM.RefreshHideTimer();
            //(WindowSettings as ChatWindowSettings).OpacityChanged += () => VM.NotifyOpacityChanged();
            VM.VisibilityChanged += OnVisibilityChanged;
            VM.RefreshHideTimer();
        }



        private void OnDragCompleted(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if (e.NewValue)
            {
                FocusManager.ForceFocused = true; //FocusTimer.Enabled = false;
                return;
            }

            var newOrder = TabControl.GetOrderedHeaders();
            var old = new HeaderedItemViewModel[VM.TabVMs.Count];
            VM.TabVMs.CopyTo(old, 0);
            var same = true;
            var items = newOrder.ToList();
            for (var i = 0; i < items.Count; i++)
            {
                if (old[i].Header != items.ToList()[i].Content)
                {
                    same = false;
                    break;
                }
            }
            if (same)
            {
                FocusManager.ForceFocused = false;
                return;
            }
            VM.TabVMs.Clear();
            foreach (var tab in items)
            {
                VM.TabVMs.Add(old.FirstOrDefault(x => x.Header == tab.Content));
            }
            FocusManager.ForceFocused = false;

        }

        public void UpdateSettings()
        {
            Dispatcher.Invoke(() =>
            {

                if (VM.TabVMs.Count == 0)
                {
                    foreach (var tab in TabControl.ItemsSource)
                    {
                        VM.TabVMs.Add(tab as HeaderedItemViewModel);
                    }
                }

                ((ChatWindowSettings)WindowSettings).Tabs.Clear();
                ((ChatWindowSettings)WindowSettings).Tabs.AddRange(VM.Tabs);
                //((ChatWindowSettings)WindowSettings).LfgOn = VM.LfgOn;
                //((ChatWindowSettings)WindowSettings).BackgroundOpacity = VM.BackgroundOpacity;
                ((ChatWindowSettings)WindowSettings).X = Left / SettingsHolder.ScreenW;
                ((ChatWindowSettings)WindowSettings).Y = Top / SettingsHolder.ScreenH;
                var v = SettingsHolder.ChatWindowsSettings;
                var s = v.FirstOrDefault(x => x == WindowSettings);
                if (s == null) v.Add((ChatWindowSettings)WindowSettings);

                if (!Equals(ChatTabClient.LastSource, this) && ChatTabClient.LastSource != null)
                {
                    ChatTabClient.LastSource.UpdateSettings();
                }
            });
        }

        private void AnimateChatVisibility(bool isChatVisible)
        {
            Dispatcher.Invoke(() => { Root.BeginAnimation(OpacityProperty, isChatVisible ? _opacityUp : _opacityDown); });
        }

        private void SwPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var s = (ScrollViewer)sender;
            var offset = e.Delta > 0 ? -2 : 2;
            s.ScrollToVerticalOffset(s.VerticalOffset - offset);
            e.Handled = true;
            if (s.VerticalOffset == 0)
            {
                _bottom = true;
                ChatWindowManager.Instance.AddFromQueue(2);
                if (ChatWindowManager.Instance.IsQueueEmpty) ChatWindowManager.Instance.SetPaused(false);

            }
            else
            {

                _bottom = false;
            }

            ChatWindowManager.Instance.SetPaused(!_bottom);
        }

        private void TabClicked(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is FrameworkElement s) || !(s.DataContext is HeaderedItemViewModel t)) return;
            ((Tab)t.Content).Attention = false;
            if (((ChatViewModel)DataContext).CurrentTab != (Tab)t.Content)
            {
                ((ChatViewModel)DataContext).CurrentTab = (Tab)t.Content;
            }
            else
            {
                TabControl.GetVisualDescendents<ItemsControl>().ToList().ForEach(x =>
                {
                    var sw = Utils.GetChild<ScrollViewer>(x);
                    sw?.ScrollToVerticalOffset(0);
                });
                _bottom = true;
                ChatWindowManager.Instance.AddFromQueue(2);
                if (ChatWindowManager.Instance.IsQueueEmpty) ChatWindowManager.Instance.SetPaused(false);
                ChatWindowManager.Instance.SetPaused(!_bottom);


            }
            var w = s.ActualWidth;
            var left = s.TransformToAncestor(this).Transform(new Point()).X;
            if (left - 3 > 0)
            {
                LeftLine.Width = left - 3;
                RightLine.Margin = new Thickness(left + w - 3, 0, 0, 0);
            }
        }

        private void TabLoaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement s)) return;
            var p = Utils.FindVisualParent<DragablzItemsControl>(s);
            if (p.ItemsSource.TryGetList().IndexOf(s.DataContext) != 0) return;
            var w = s.ActualWidth;
            var left = s.TransformToAncestor(this).Transform(new Point()).X;
            if (left - 3 >= 0) LeftLine.Width = left - 3;
            if (left + w - 3 >= 0) RightLine.Margin = new Thickness(left + w - 3, 0, 0, 0);

        }


        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //if (!_bottom) return;
            /*
                        _currentContent = tabControl.SelectedContent as ItemsControl;
                        _currentPanel = GetInnerStackPanel(_currentContent);
                        var sw = (ScrollViewer)sender;
                        Rect svBounds = LayoutInformation.GetLayoutSlot(sw);
                        var testRect = new Rect(svBounds.Top - 5, svBounds.Left, svBounds.Width, svBounds.Height + 10);
                        List<FrameworkElement> visibleItems = GetVisibleItems(testRect);

                        foreach (var item in visibleItems)
                        {
                            var i = visibleItems.IndexOf(item);
                            if (i > 4)
                            {
                                continue;
                            }
                            else
                            {

                            }

                            var dc = ((ChatMessage)item.DataContext);
                            if (dc.Channel == ChatChannel.Bargain) return;
                            if (GetMessageRows(item.ActualHeight) > 1)
                            {
                                var lastItemRows = GetMessageRows(visibleItems.Last().ActualHeight);
                                //Debug.WriteLine("Rows = {0} - LastItemRows = {1}", dc.Rows, lastItemRows);
                                if (dc.Rows - lastItemRows >= 1)
                                {
                                    dc.Rows = dc.Rows - lastItemRows;
                                    dc.IsContracted = true;
                                }
                            }
                            else
                            {

                            }
                        }
            */
        }

        private void TccWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //var res = TccMessageBox.Show("TCC", $"There was an error while reading events-EU.xml. Manually correct the error and and press Ok to try again, else press Cancel to build a default config file.", MessageBoxButton.OKCancel);
            //Proxy.ChatTest("test");
            //ChatWindowManager.Instance.AddChatMessage(new ChatMessage(ChatChannel.Global, "Moonfury", "<font size=\"50\" color=\"#e87d7d\"> M</font>" + "<font size=\"70\" color=\"#e8b07d\">E</font>" + "<font size=\"70\" color=\"#e8d77d\">M</font>" + "<font size=\"70\" color=\"#c6e87d\">E</font>" + "<font size=\"50\" color=\"#92e87d\">S</font>" + "<font size=\"50\" color=\"#7de89b\">L</font>" + "<font size=\"50\" color=\"#7de8ce\">A</font>" + "<font size=\"50\" color=\"#7dcee8\">S</font>" + "<font size=\"50\" color=\"#7d8ee8\">H</font>"));
            //ChatWindowManager.Instance.AddChatMessage(new ChatMessage(ChatChannel.Global, "PODEM CONFIAR", "<font size=\"25\" color=\"#ff0000\"><a href=\"asfunction:chatLinkAction\">I am a retard that runs random code from the internet, so my character has been sent to the DOOMZONE.</a></font>"));
            //ChatWindowManager.Instance.AddChatMessage(new ChatMessage(ChatChannel.Global, "PODEM CONFIAR", "<font size=\"32\" color=\"#ff0000\"><a href=\"asfunction:chatLinkAction\">Goodbye everyone!</a></font>"));

            //GroupWindowViewModel.Instance.ClearAll();
            //for (int i = 0; i < 23; i++)
            //{
            //    var name = "D" + i;
            //    GroupWindowViewModel.Instance.AddOrUpdateMember(new User(GroupWindowViewModel.Instance.GetDispatcher()) { Name = name, UserClass = Class.Warrior, ServerId = (uint)i + 100 });
            //}
            //for (int i = 0; i < 2; i++)
            //{
            //    var name = "T" + i;

            //    GroupWindowViewModel.Instance.AddOrUpdateMember(new User(GroupWindowViewModel.Instance.GetDispatcher()) { Name = name, UserClass = Class.Lancer, ServerId = (uint)i + 200 });
            //}
            //for (int i = 0; i < 5; i++)
            //{
            //    var name = "H" + i;

            //    GroupWindowViewModel.Instance.AddOrUpdateMember(new User(GroupWindowViewModel.Instance.GetDispatcher()) { Name = name, UserClass = Class.Mystic, ServerId = (uint)i + 300 });
            //}
        }

        private void TccWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            VM.RefreshHideTimer();
            Settings.BeginAnimation(OpacityProperty, new DoubleAnimation(.3, TimeSpan.FromMilliseconds(300)));
            if (e.LeftButton == MouseButtonState.Pressed) UpdateSettings();
        }

        private void TccWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            VM.StopHideTimer();
            Settings.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(300)));
        }

        private void OpenTabSettings(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is FrameworkElement s)) return;
            if (!(s.DataContext is HeaderedItemViewModel dc)) return;
            var t = (Tab)dc.Content;
            var sw = new ChatSettingsWindow(t);
            sw.Show();
            sw.Activate();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsPopup.DataContext = DataContext;
            SettingsPopup.IsOpen = !SettingsPopup.IsOpen;
        }

        private void SettingsPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            if (SettingsPopup.IsMouseCaptured) return;
            SettingsPopup.IsOpen = false;
        }

        private void SettingsPopup_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (sender as FrameworkElement)?.CaptureMouse();
        }

        private void SettingsPopup_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            (sender as FrameworkElement)?.ReleaseMouseCapture();

        }

        private void ChatWindow_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var ws = Application.Current.Windows;

            foreach (Window w in ws)
            {
                if (w is ChatWindow cw)
                {
                    if (cw.VM.TabVMs.Count == 0)
                    {
                        ChatWindowManager.Instance.ChatWindows.Remove(cw);
                        cw.Close();
                    }
                }
            }
            UpdateSettings();
            if (FocusManager.ForceFocused) FocusManager.ForceFocused = false; //FocusTimer.Enabled = true;
        }


        private new void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender, e);
            TabControl.SelectedIndex = 0;
        }

        //private new void Drag(object sender, MouseButtonEventArgs e)
        //{
        //    //needed to keep windowchrome, no idea why
        //    return;
        //}
        private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void UnpinMessage(object sender, RoutedEventArgs e)
        {
            var currTabVm = TabControl.SelectedItem as HeaderedItemViewModel;
            //var msg = (sender as FrameworkElement).DataContext as ChatMessage;
            //var tabVm = VM.TabVMs.FirstOrDefault(x =>
            //    ((Tab)x.Content).Messages.Contains(msg) && x == currTabVm);
            if (currTabVm?.Content != null) ((Tab)currTabVm.Content).PinnedMessage = null;

            //var tab = VM.Tabs.FirstOrDefault(x => 
            //x.PinnedMessage == (((sender as FrameworkElement)?.DataContext as HeaderedItemViewModel)?.Content as Tab)?.PinnedMessage
            //);
            //if (tab != null) tab.PinnedMessage = null;
        }

        private void MakeGlobal(object sender, RoutedEventArgs e)
        {
            WindowSettings.MakePositionsGlobal();
        }

        private void ItemsControl_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var lb = sender as ItemsControl;
            var sw = Utils.GetChild<ScrollViewer>(lb);
            var lines = SettingsHolder.ChatScrollAmount;
            if (sw.VerticalOffset + e.Delta >= sw.ScrollableHeight)
            {
                lines = 1;
            }
            sw.ScrollToVerticalOffset(sw.VerticalOffset + (e.Delta > 0 ? lines : -lines));

            e.Handled = true;
            if (sw.VerticalOffset == 0)
            {
                _bottom = true;
                ChatWindowManager.Instance.AddFromQueue(0);
                if (ChatWindowManager.Instance.IsQueueEmpty) ChatWindowManager.Instance.SetPaused(false);
            }
            else
            {

                _bottom = false;
            }
            ChatWindowManager.Instance.SetPaused(!_bottom);

        }
    }

}