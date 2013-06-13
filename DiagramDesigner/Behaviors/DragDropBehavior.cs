/**************************************************************
 * Copyright (c) 2009 Charlie Robbins
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
**************************************************************/

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace ControlPanel.SharedClasses.Behaviors
{
    public class DragDropBehavior : Behavior<FrameworkElement>
    {
        #region IsHost Property

        public static readonly DependencyProperty IsHostProperty = DependencyProperty.RegisterAttached(
            "IsHost",
            typeof(bool),
            typeof(DragDropBehavior),
            new PropertyMetadata(false));

        public delegate void MouseClickDelegate(object sender, EventArgs e);
        public event MouseClickDelegate MouseClick;


        public static bool GetIsHost(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsHostProperty);
        }

        public static void SetIsHost(DependencyObject obj, bool value)
        {
            obj.SetValue(IsHostProperty, value);
        }

        #endregion

        #region X Property

        private double x = double.NaN;
        public double X
        {
            get
            {
                return this.x;
            }

            set
            {
                this.x = value;
            }
        }

        #endregion

        #region Y Property

        private double y = double.NaN;
        public double Y
        {
            get
            {
                return this.y;
            }

            set
            {
                this.y = value;
            }
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            //this.AssociatedObject.MouseLeftButtonDown += DragStart;
            this.AssociatedObject.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.DragStart), false);

        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.RemoveHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.DragStart));
            //this.AssociatedObject.MouseLeftButtonDown -= DragStart;
        }

        private bool ignoreMouseUp = false;

        private void DragStart(object sender, MouseButtonEventArgs args)
        {
            this.ignoreMouseUp = false;

            UIElement dragSource = sender as UIElement;

            // Create the TranslateTransform that will perform our drag operation
            TranslateTransform dragTransform = new TranslateTransform();
            dragTransform.X = 0;
            dragTransform.Y = 0;

            // Set the TranslateTransform if it is the first time DragDrop is being used
            dragSource.RenderTransform = (dragSource.RenderTransform is TranslateTransform) ? dragSource.RenderTransform : dragTransform;

            // Attach the event handlers for MouseMove and MouseLeftButtonUp for dragging and dropping respectively
            dragSource.MouseMove += this.DragDelta;
            //dragSource.MouseLeftButtonUp += DragComplete;
            this.AssociatedObject.AddHandler(FrameworkElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.DragComplete), true);

            //Capture the Mouse
            dragSource.CaptureMouse();
        }

        private void DragDelta(object sender, MouseEventArgs args)
        {
            FrameworkElement dragSource = sender as FrameworkElement;

            // Calculate the offset of the dragSource and update its TranslateTransform
            FrameworkElement dragDropHost = FindDragDropHost(dragSource);
            Point relativeLocationInHost = args.GetPosition(dragDropHost);
            Point relativeLocationInSource = args.GetPosition(dragSource);

            // Calculate the delta from the previous position
            double xChange = relativeLocationInHost.X - this.X;
            double yChange = relativeLocationInHost.Y - this.Y;

            // Update the position if this is not the first mouse move
            if (!double.IsNaN(this.X))
            {
                ((TranslateTransform)dragSource.RenderTransform).X += xChange;
                this.ignoreMouseUp = true;
            }

            if (!double.IsNaN(this.Y))
            {
                ((TranslateTransform)dragSource.RenderTransform).Y += yChange;
                this.ignoreMouseUp = true;
            }

            // Update our private attached properties for tracking drag location
            this.X = relativeLocationInHost.X;
            this.Y = relativeLocationInHost.Y;
        }

        private void DragComplete(object sender, MouseButtonEventArgs args)
        {
            UIElement dragSource = sender as UIElement;

            dragSource.MouseMove -= this.DragDelta;
            //dragSource.MouseLeftButtonUp -= DragComplete;
            this.AssociatedObject.RemoveHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.DragComplete));

            // Set the X & Y Values so that they can be reset next MouseDown
            this.X = double.NaN;
            this.Y = double.NaN;

            // Release Mouse Capture
            dragSource.ReleaseMouseCapture();

            if (!this.ignoreMouseUp && this.MouseClick != null)
            {
                this.ignoreMouseUp = true;
                this.MouseClick(this, new EventArgs());
            }
        }

        public static FrameworkElement FindDragDropHost(UIElement element)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(element);
            while (parent != null && !GetIsHost(parent))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as FrameworkElement;
        }

    }
}
