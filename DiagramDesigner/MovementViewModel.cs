using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DiagramDesigner
{
    public class MovementViewModel
    {
        public void HandleKeyDown(KeyEventArgs e, UIElement element)
        {
            if (element == null) return;

            double left = Canvas.GetLeft(element);
            if (Double.IsNaN(left)) left = 0;
            double top = Canvas.GetTop(element);
            if (Double.IsNaN(top)) top = 0;

            var src = new Point(left, top);

            double odx = 0, ody = 0;
            switch (e.Key)
            {
                case Key.Left:
                    odx = Keyboard.IsKeyDown(Key.LeftShift) ? -10 : -1;
                    break;
                case Key.Up:
                    ody = Keyboard.IsKeyDown(Key.LeftShift) ? -10 : -1;
                    break;
                case Key.Right:
                    odx = Keyboard.IsKeyDown(Key.LeftShift) ? 10 : 1;
                    break;
                case Key.Down:
                    ody = Keyboard.IsKeyDown(Key.LeftShift) ? 10 : 1;
                    break;
            }

            src.Offset(odx, ody);

            var dest = src;
            left = dest.X;
            top = dest.Y;

            if (left < 0) left = 0;
            if (top < 0) top = 0;

            Canvas.SetLeft(element, left);
            Canvas.SetTop(element, top);
            e.Handled = true;
        }
    }
}
