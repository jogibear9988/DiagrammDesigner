using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace DiagramDesigner
{
    // Represents a selectable item in the Toolbox/>.
    public class ToolboxItem : ListBoxItem
    {
        public static Size GetDesiredSize(DependencyObject obj)
        {
            return (Size)obj.GetValue(DesiredSizeProperty);
        }

        public static void SetDesiredSize(DependencyObject obj, Size value)
        {
            obj.SetValue(DesiredSizeProperty, value);
        }
        
        public static readonly DependencyProperty DesiredSizeProperty =
            DependencyProperty.RegisterAttached("DesiredSize", typeof(Size), typeof(ToolboxItem), new PropertyMetadata(null));

        
        // caches the start point of the drag operation
        private Point? dragStartPoint = null;

        static ToolboxItem()
        {
            // set the key to reference the style for this control
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ToolboxItem), new FrameworkPropertyMetadata(typeof(ToolboxItem)));
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            this.dragStartPoint = new Point?(e.GetPosition(this));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton != MouseButtonState.Pressed)
                this.dragStartPoint = null;

            if (this.dragStartPoint.HasValue)
            {
                // XamlWriter.Save() has limitations in exactly what is serialized,
                // see SDK documentation; short term solution only;
                //string xamlString = XamlWriter.Save(this.Content);
                DragObject dataObject = new DragObject();
                dataObject.ObjectType = this.Content.GetType();

                var panel = VisualTreeHelper.GetParent(this) as Panel;
                if (panel != null)
                {
                    dataObject.DesiredSize = GetDesiredSize((DependencyObject)this.Content);

                    // desired size for DesignerCanvas is the stretched Toolbox item size
                    if (dataObject.DesiredSize == Size.Empty)
                    {
                        double scale = 1.3;
                        if (panel is WrapPanel)
                            dataObject.DesiredSize = new Size(((WrapPanel)panel).ItemWidth * scale, ((WrapPanel)panel).ItemHeight * scale);
                    }
                }

                DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);

                e.Handled = true;
            }
        }
    }

    // Wraps info of the dragged object into a class
    public class DragObject
    {
        // Xaml string that represents the serialized content
        public Type ObjectType { get; set; }

        // Defines width and height of the DesignerItem
        // when this DragObject is dropped on the DesignerCanvas
        public Size? DesiredSize { get; set; }
    }
}
