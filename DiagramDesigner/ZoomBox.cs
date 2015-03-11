using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DiagramDesigner
{
    public class ZoomBox : Control
    {
        private Thumb zoomThumb;
        private Canvas zoomCanvas;
        private Slider zoomSlider;
        private ScaleTransform scaleTransform;
        private DesignerCanvas designerCanvas;

        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }

        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register("ScrollViewer", typeof(ScrollViewer), typeof(ZoomBox), new PropertyMetadata(OnScrollViewerChanged));

        private static void OnScrollViewerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = d as ZoomBox;
            ctl.findParts();
        }

        static ZoomBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomBox), new FrameworkPropertyMetadata(typeof(ZoomBox)));            
        }

        private bool templateApplied = false;
        private bool eventsAdded = false;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            templateApplied = true;

            findParts();
        }

        private void findParts()
        {
            if (eventsAdded)
            {
                this.designerCanvas.LayoutUpdated -= this.DesignerCanvas_LayoutUpdated;
                this.zoomThumb.DragDelta -= this.Thumb_DragDelta;
                this.zoomSlider.ValueChanged -= this.ZoomSlider_ValueChanged;
                eventsAdded = false;
            }

            if (this.ScrollViewer == null || !templateApplied)
                return;

            this.designerCanvas = this.ScrollViewer.Content as DesignerCanvas;
            if (this.designerCanvas == null)
                throw new Exception("DesignerCanvas must not be null!");

            this.zoomThumb = Template.FindName("PART_ZoomThumb", this) as Thumb;
            if (this.zoomThumb == null)
                throw new Exception("PART_ZoomThumb template is missing!");

            this.zoomCanvas = Template.FindName("PART_ZoomCanvas", this) as Canvas;
            if (this.zoomCanvas == null)
                throw new Exception("PART_ZoomCanvas template is missing!");

            this.zoomSlider = Template.FindName("PART_ZoomSlider", this) as Slider;
            if (this.zoomSlider == null)
                throw new Exception("PART_ZoomSlider template is missing!");

            this.designerCanvas.LayoutUpdated += this.DesignerCanvas_LayoutUpdated;
            this.zoomThumb.DragDelta += this.Thumb_DragDelta;
            this.zoomSlider.ValueChanged += this.ZoomSlider_ValueChanged;
            eventsAdded = true;

            this.scaleTransform = new ScaleTransform();
            this.designerCanvas.LayoutTransform = this.scaleTransform;
            this.designerCanvas.ScaleChanged += designerCanvas_ScaleChanged;
        }

        void designerCanvas_ScaleChanged(double scale)
        {
            this.zoomSlider.ValueChanged -= this.ZoomSlider_ValueChanged;
            zoomSlider.Value = scale;
            this.zoomSlider.ValueChanged += this.ZoomSlider_ValueChanged;
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.ScrollViewer != null)
            {
                double scale = e.NewValue/e.OldValue;

                double halfViewportHeight = this.ScrollViewer.ViewportHeight/2;
                double newVerticalOffset = ((this.ScrollViewer.VerticalOffset + halfViewportHeight)*scale -
                                            halfViewportHeight);

                double halfViewportWidth = this.ScrollViewer.ViewportWidth/2;
                double newHorizontalOffset = ((this.ScrollViewer.HorizontalOffset + halfViewportWidth)*scale -
                                              halfViewportWidth);

                this.scaleTransform.ScaleX *= scale;
                this.scaleTransform.ScaleY *= scale;

                this.ScrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
                this.ScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (this.ScrollViewer != null)
            {
                double scale, xOffset, yOffset;
                this.InvalidateScale(out scale, out xOffset, out yOffset);

                this.ScrollViewer.ScrollToHorizontalOffset(this.ScrollViewer.HorizontalOffset + e.HorizontalChange/scale);
                this.ScrollViewer.ScrollToVerticalOffset(this.ScrollViewer.VerticalOffset + e.VerticalChange/scale);
            }
        }

        private void DesignerCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            if (this.ScrollViewer != null)
            {
                double scale, xOffset, yOffset;
                this.InvalidateScale(out scale, out xOffset, out yOffset);

                this.zoomThumb.Width = this.ScrollViewer.ViewportWidth*scale;
                this.zoomThumb.Height = this.ScrollViewer.ViewportHeight*scale;

                Canvas.SetLeft(this.zoomThumb, xOffset + this.ScrollViewer.HorizontalOffset*scale);
                Canvas.SetTop(this.zoomThumb, yOffset + this.ScrollViewer.VerticalOffset*scale);
            }
        }

        private void InvalidateScale(out double scale, out double xOffset, out double yOffset)
        {
            // designer canvas size
            double w = this.designerCanvas.ActualWidth * this.scaleTransform.ScaleX;
            double h = this.designerCanvas.ActualHeight * this.scaleTransform.ScaleY;

            // zoom canvas size
            double x = this.zoomCanvas.ActualWidth;
            double y = this.zoomCanvas.ActualHeight;

            double scaleX = x / w;
            double scaleY = y / h;

            scale = (scaleX < scaleY) ? scaleX : scaleY;

            xOffset = (x - scale * w) / 2;
            yOffset = (y - scale * h) / 2;
        }
    }
}
