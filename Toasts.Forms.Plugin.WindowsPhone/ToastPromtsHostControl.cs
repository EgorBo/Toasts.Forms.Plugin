using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Plugin.Toasts
{
    public class ToastPromtsHostControl : Grid
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly Queue<NotificationItem> _notificationQueue = new Queue<NotificationItem>();
        private readonly List<ToastItem> _toastItems = new List<ToastItem>();
        private static ToastPromtsHostControl _lastUsedInstance = null;
        private readonly ItemsControl _activeItemsControl;
        
        private static int _maxToastCount = 3; //by default we can display up to 3 toasts in a row.
        
        public static int MaxToastCount
        {
            get { return _maxToastCount; }
            set { _maxToastCount = value; }
        }
        
        public ToastPromtsHostControl()
        {
            _activeItemsControl = new ItemsControl { Background = new SolidColorBrush(Colors.Transparent) };
            Children.Add(_activeItemsControl);
            
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_OnTick;
            _timer.Start();
            _lastUsedInstance = this; //instance is defined in xaml
        }

        public static void Clear()
        {
            if (_lastUsedInstance == null)
                return;
            _lastUsedInstance.ClearAll();
        }

        public static void EnqueueItem(UIElement content, Action<bool> submitAction, Brush bgBrush, bool tappable, TimeSpan timeout, bool showCloseButton)
        {
            if (_lastUsedInstance == null)
                return;

            _lastUsedInstance._notificationQueue.Enqueue(new NotificationItem(content, bgBrush, submitAction, tappable, timeout, showCloseButton));
            _lastUsedInstance.TryDequeue();
        }
        
        private void Timer_OnTick(object sender, object o)
        {
            foreach (ToastItem toastItem in _toastItems.ToArray())
            {
                if (!toastItem.IsFinalizing && toastItem.Started + toastItem.NotificationItem.ToastTimeout <= DateTime.Now)
                {
                    toastItem.NotificationItem.PerformAction(false);
                    RemoveToast(toastItem);
                }
            }
        }

        private void UpdateVisibility(bool forceVisible = false)
        {
            if (forceVisible)
            {
                Visibility = Visibility.Visible;
                return;
            }

            if (Visibility == Visibility.Visible)
            {
                if (!_toastItems.Any() && !_notificationQueue.Any())
                    Visibility = Visibility.Collapsed;
            }
            else
            {
                if (_toastItems.Any() || _notificationQueue.Any())
                    Visibility = Visibility.Visible;
            }
        }

        private void RemoveToast(ToastItem toastItem)
        {
            if (toastItem.IsFinalizing)
                return;

            toastItem.IsFinalizing = true;
            toastItem.FinalizingActions();

            var storyboard = new Storyboard();
            var projectionAnimation = new DoubleAnimation { Duration = new Duration(TimeSpan.FromSeconds(0.6)), To = 90 };
            storyboard.Children.Add(projectionAnimation);
            Storyboard.SetTargetProperty(projectionAnimation, new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationX)"));
            Storyboard.SetTarget(projectionAnimation, toastItem.Element);
            var item = toastItem;

            EventHandler completedHandler = null;
            completedHandler = (s, ea) =>
            {
                _toastItems.Remove(item);
                _activeItemsControl.Items.Remove(item.Element);
                UpdateVisibility();
                TryDequeue();
                storyboard.Completed -= completedHandler;
            };

            storyboard.Completed += completedHandler;
            storyboard.Begin();
        }

        private void TryDequeue()
        {
            if (_toastItems.Count < MaxToastCount && _notificationQueue.Any())
            {
                var item = _notificationQueue.Dequeue();
                UpdateVisibility(true);
                AppendToast(item);
                TryDequeue();
            }
        }

        private void ClearAll()
        {
            _notificationQueue.Clear();
            foreach (var toastItem in _toastItems)
            {
                RemoveToast(toastItem);
            }
        }

        private void AppendToast(NotificationItem notification)
        {
            //root layout
            var layoutGrid = new Grid();
            var toastItem = new ToastItem(layoutGrid, DateTime.Now, notification);
            layoutGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            layoutGrid.Height = 70;
            layoutGrid.Margin = new Thickness(0, 0, 0, 2); //2 - a margin between toasts
            layoutGrid.Background = notification.Brush;
            layoutGrid.Projection = new PlaneProjection();
            layoutGrid.RenderTransformOrigin = new Point(0.5, 0.5);
            layoutGrid.RenderTransform = new CompositeTransform { TranslateX = -800 };
            layoutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


            Button closeButton = null;
            if (notification.ShowCloseButton)
            {
                layoutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40.0) });
                //close button
                closeButton = new Button();
                SetColumn(closeButton, 1);
                closeButton.Tag = toastItem;
                closeButton.Width = 66;
                closeButton.Height = 90;
                closeButton.Content = "\u2716"; //X symbol
                closeButton.BorderThickness = new Thickness(0);
                closeButton.FontSize = 32;
                closeButton.Padding = new Thickness(0);
                closeButton.Opacity = 0.4;
                closeButton.Margin = new Thickness(-12, -15, -8, -8); //TODO: fix it
                closeButton.HorizontalAlignment = HorizontalAlignment.Right;
                closeButton.Tap += CloseButton_OnTap;
                layoutGrid.Children.Add(closeButton);
            }

            //toast content
            var contentGrid = new Grid();
            contentGrid.Tag = toastItem;
            contentGrid.Children.Add(notification.Content);
            layoutGrid.Children.Add(contentGrid);
            contentGrid.Tap += ContentGrid_OnTap;

            toastItem.FinalizingActions += () => contentGrid.Tap -= ContentGrid_OnTap;

            if (closeButton != null)
            {
                toastItem.FinalizingActions += () => closeButton.Tap -= CloseButton_OnTap;
            }

            //appear animation
            var animation = new DoubleAnimation { Duration = new Duration(TimeSpan.FromSeconds(0.2)), To = 0 };
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, layoutGrid);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            storyboard.Begin();

            _activeItemsControl.Items.Add(layoutGrid);
            _toastItems.Add(toastItem);
        }

        private void ContentGrid_OnTap(object sender, object e)
        {
            var toastItem = ((FrameworkElement)sender).Tag as ToastItem;
            if (toastItem == null)
                return;

            if (!toastItem.IsFinalizing && toastItem.NotificationItem.Tappable)
            {
                RemoveToast(toastItem);
                toastItem.NotificationItem.PerformAction(true);
            }
        }

        private void CloseButton_OnTap(object sender, object e)
        {
            var toastItem = ((FrameworkElement)sender).Tag as ToastItem;
            if (toastItem == null)
                return;

            toastItem.NotificationItem.PerformAction(false);

            RemoveToast(toastItem);
        }

        private class ToastItem
        {
            public FrameworkElement Element { get; set; }
            public DateTime Started { get; set; }
            public bool IsFinalizing { get; set; }
            public NotificationItem NotificationItem { get; set; }
            public Action FinalizingActions { get; set; }

            public ToastItem(FrameworkElement element, DateTime started, NotificationItem notification)
            {
                Element = element;
                Started = started;
                NotificationItem = notification;
            }
        }

        private class NotificationItem
        {
            public UIElement Content { get; set; }
            public Brush Brush { get; set; }
            public Action<bool> Action { get; set; }
            public bool Tappable { get; set; }
            public TimeSpan ToastTimeout { get; set; }
            public bool ShowCloseButton { get; set; }

            public NotificationItem(UIElement content, Brush brush, Action<bool> action, bool tappable, TimeSpan toastTimeout, bool showCloseButton)
            {
                Content = content;
                Brush = brush;
                Action = action;
                Tappable = tappable;
                ToastTimeout = toastTimeout;
                ShowCloseButton = showCloseButton;
            }

            public void PerformAction(bool result)
            {
                if (Action != null)
                {
                    Action(result);
                    Action = null;
                }
            }
        }
    }
}
