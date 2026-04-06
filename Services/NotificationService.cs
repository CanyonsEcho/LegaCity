using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;

namespace LegaCity.Services
{
    public class NotificationService
    {
        private readonly StackPanel _container;
        private readonly Queue<Border> _notifications = new();

        public NotificationService(StackPanel container)
        {
            _container = container;
            _container.Spacing = 8;
        }

        public async void ShowError(string message)
        {
            var notification = CreateNotification(message, "#FF4444");
            _container.Children.Insert(0, notification);
            _notifications.Enqueue(notification);

            await Task.Delay(5000);
            if (_container.Children.Contains(notification))
            {
                _container.Children.Remove(notification);
            }
            _notifications.Dequeue();
        }

        private Border CreateNotification(string message, string backgroundColor)
        {
            var notification = new Border
            {
                Background = new SolidColorBrush(Color.Parse(backgroundColor)),
                CornerRadius = new Avalonia.CornerRadius(4),
                Padding = new Avalonia.Thickness(16, 12),
                MaxWidth = 350
            };

            var textBlock = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 13,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };

            notification.Child = textBlock;

            return notification;
        }
    }
}

