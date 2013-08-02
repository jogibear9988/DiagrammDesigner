using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using DiagramDesigner.PathFinder;

namespace DiagramDesigner
{
    public class Connection : Control, IZIndex, ISelectable, INotifyPropertyChanged
    {
        private Adorner connectionAdorner;

        #region Properties

        public Guid ID { get; set; }

        // source connector
        private Connector source;
        public Connector Source
        {
            get
            {
                return source;
            }
            set
            {
                if (source != value)
                {
                    if (source != null)
                    {
                        source.PropertyChanged -= new PropertyChangedEventHandler(OnConnectorPositionChanged);
                        source.Connections.Remove(this);
                    }

                    source = value;

                    if (source != null)
                    {
                        source.Connections.Add(this);
                        source.PropertyChanged += new PropertyChangedEventHandler(OnConnectorPositionChanged);
                    }

                    UpdatePathGeometry();
                }
            }
        }

        // sink connector
        private Connector sink;
        public Connector Sink
        {
            get { return sink; }
            set
            {
                if (sink != value)
                {
                    if (sink != null)
                    {
                        sink.PropertyChanged -= new PropertyChangedEventHandler(OnConnectorPositionChanged);
                        sink.Connections.Remove(this);
                    }

                    sink = value;

                    if (sink != null)
                    {
                        sink.Connections.Add(this);
                        sink.PropertyChanged += new PropertyChangedEventHandler(OnConnectorPositionChanged);
                    }
                    UpdatePathGeometry();
                }
            }
        }

        // connection path geometry
        private PathGeometry pathGeometry;
        public PathGeometry PathGeometry
        {
            get { return pathGeometry; }
            set
            {
                if (pathGeometry != value)
                {
                    pathGeometry = value;
                    UpdateAnchorPosition();
                    OnPropertyChanged("PathGeometry");
                }
            }
        }

        // connection path geometry
        private List<Point> points;
        public List<Point> Points
        {
            get { return points; }
            set
            {
                if (points != value)
                {
                    points = value;
                    UpdateAnchorPosition();
                    OnPropertyChanged("Points");
                }
            }
        }

        public PathFinderTypes pathFinder;
        public PathFinderTypes PathFinder
        {
            get
            {                
                return pathFinder;
            }
            set
            {
                if (pathFinder != value)
                {
                    pathFinder = value;
                    UpdateAnchorPosition();
                    OnPropertyChanged("PathFinder");
                }
            }
        }
        // between source connector position and the beginning 
        // of the path geometry we leave some space for visual reasons; 
        // so the anchor position source really marks the beginning 
        // of the path geometry on the source side
        private Point anchorPositionSource;
        public Point AnchorPositionSource
        {
            get { return anchorPositionSource; }
            set
            {
                if (anchorPositionSource != value)
                {
                    anchorPositionSource = value;
                    OnPropertyChanged("AnchorPositionSource");
                    OnPropertyChanged("AnchorPositionMiddle");
                }
            }
        }

        // slope of the path at the anchor position
        // needed for the rotation angle of the arrow
        private double anchorAngleSource = 0;
        public double AnchorAngleSource
        {
            get { return anchorAngleSource; }
            set
            {
                if (anchorAngleSource != value)
                {
                    anchorAngleSource = value;
                    OnPropertyChanged("AnchorAngleSource");
                }
            }
        }

        // analogue to source side
        private Point anchorPositionSink;
        public Point AnchorPositionSink
        {
            get { return anchorPositionSink; }
            set
            {
                if (anchorPositionSink != value)
                {
                    anchorPositionSink = value;
                    OnPropertyChanged("AnchorPositionSink");
                    OnPropertyChanged("AnchorPositionMiddle");
                }
            }
        }
        // analogue to source side
        private double anchorAngleSink = 0;
        public double AnchorAngleSink
        {
            get { return anchorAngleSink; }
            set
            {
                if (anchorAngleSink != value)
                {
                    anchorAngleSink = value;
                    OnPropertyChanged("AnchorAngleSink");                    
                }
            }
        }

        public Point AnchorPositionMiddle
        {
            get
            {
                if (AnchorPositionSource != null && AnchorPositionSink != null)
                {
                    var x = (anchorPositionSource.X + anchorPositionSink.X)/2;
                    var y = (anchorPositionSource.Y + anchorPositionSink.Y)/2;
                    return new Point(Math.Abs(x), Math.Abs(y));
                }

                return new Point(0,0);
            }            
        }

        private string text = "";
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (text != value)
                {
                    text = value;
                    OnPropertyChanged("Text");
                }
            }
        }

        private ArrowSymbol sourceArrowSymbol = ArrowSymbol.None;
        public ArrowSymbol SourceArrowSymbol
        {
            get { return sourceArrowSymbol; }
            set
            {
                if (sourceArrowSymbol != value)
                {
                    sourceArrowSymbol = value;
                    OnPropertyChanged("SourceArrowSymbol");
                }
            }
        }

        public ArrowSymbol sinkArrowSymbol = ArrowSymbol.Arrow;
        public ArrowSymbol SinkArrowSymbol
        {
            get { return sinkArrowSymbol; }
            set
            {
                if (sinkArrowSymbol != value)
                {
                    sinkArrowSymbol = value;
                    OnPropertyChanged("SinkArrowSymbol");
                }
            }
        }

        // specifies a point at half path length
        private Point labelPosition;
        public Point LabelPosition
        {
            get { return labelPosition; }
            set
            {
                if (labelPosition != value)
                {
                    labelPosition = value;
                    OnPropertyChanged("LabelPosition");
                }
            }
        }

        // pattern of dashes and gaps that is used to outline the connection path
        private DoubleCollection strokeDashArray;
        public DoubleCollection StrokeDashArray
        {
            get
            {
                return strokeDashArray;
            }
            set
            {
                if (strokeDashArray != value)
                {
                    strokeDashArray = value;
                    OnPropertyChanged("StrokeDashArray");
                }
            }
        }
        // if connected, the ConnectionAdorner becomes visible
        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged("IsSelected");
                    if (isSelected)
                        ShowAdorner();
                    else
                        HideAdorner();
                }
            }
        }

        #endregion

        #region Dependency Properties

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(Connection), new PropertyMetadata(null));
        
        public bool ShowShadow
        {
            get { return (bool)GetValue(ShowShadowProperty); }
            set { SetValue(ShowShadowProperty, value); }
        }

        public static readonly DependencyProperty ShowShadowProperty =
            DependencyProperty.Register("ShowShadow", typeof(bool), typeof(Connection), new PropertyMetadata(false));
        
        #endregion



        public int ZIndex
        {
            get { return (int)GetValue(ZIndexProperty); }
            set { SetValue(ZIndexProperty, value); }
        }

        public static readonly DependencyProperty ZIndexProperty =
            DependencyProperty.Register("ZIndex", typeof(int), typeof(Connection), new PropertyMetadata(0));

        
        static Connection()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Connection), new FrameworkPropertyMetadata(typeof(Connection)));            
        }

        public Connection(Connector source, Connector sink, PathFinderTypes pathFinder)
        {
            this.ID = Guid.NewGuid();
            this.Source = source;
            this.pathFinder = pathFinder;
            this.Sink = sink;

            this.MouseDown += Connection_MouseDown;
            
            //base.Unloaded += new RoutedEventHandler(Connection_Unloaded);
        }

        void Connection_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Stops Event Bubbbling, Connections stays in Focus!
            e.Handled = true;
        }
       
        //void Connection_KeyUp(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Delete)
        //    {
        //        DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;
        //        designer.Children.Remove(this);
        //    }
        //}


        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            // usual selection business
            DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;
            if (designer != null)
            {
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                    if (this.IsSelected)
                    {
                        designer.SelectionService.RemoveFromSelection(this);
                    }
                    else
                    {
                        designer.SelectionService.AddToSelection(this);
                    }
                else if (!this.IsSelected)
                {
                    designer.SelectionService.SelectItem(this);
                }

                Focus();
            }
            e.Handled = false;
        }

        void OnConnectorPositionChanged(object sender, PropertyChangedEventArgs e)
        {
            // whenever the 'Position' property of the source or sink Connector 
            // changes we must update the connection path geometry
            if (e.PropertyName.Equals("Position"))
            {
                UpdatePathGeometry();
            }
        }

        private void UpdatePathGeometry()
        {
            if (Source != null && Sink != null)
            {
                PathGeometry geometry = new PathGeometry();
                var pointsList = PathFinderHelper.GetPathFinder(this.pathFinder).GetConnectionLine(Source.GetInfo(), Sink.GetInfo(), true);
                if (pointsList.Count > 0)
                {
                    PathFigure figure = new PathFigure();
                    figure.IsClosed = false;
                    figure.StartPoint = pointsList[0];
                    pointsList.Remove(pointsList[0]);
                    figure.Segments.Add(new PolyLineSegment(pointsList, true));
                    geometry.Figures.Add(figure);

                    this.PathGeometry = geometry;
                    this.Points = pointsList;
                }
            }
        }

        private bool templateApplied = false;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            templateApplied = true;
        } 
        private void UpdateAnchorPosition()
        {
            if (templateApplied)
            {
                try
                {
                    Point pathStartPoint, pathTangentAtStartPoint;
                    Point pathEndPoint, pathTangentAtEndPoint;
                    Point pathMidPoint, pathTangentAtMidPoint;

                    // the PathGeometry.GetPointAtFractionLength method gets the point and a tangent vector 
                    // on PathGeometry at the specified fraction of its length
                    this.PathGeometry.GetPointAtFractionLength(0, out pathStartPoint, out pathTangentAtStartPoint);
                    this.PathGeometry.GetPointAtFractionLength(1, out pathEndPoint, out pathTangentAtEndPoint);
                    this.PathGeometry.GetPointAtFractionLength(0.5, out pathMidPoint, out pathTangentAtMidPoint);

                    // get angle from tangent vector
                    this.AnchorAngleSource = Math.Atan2(-pathTangentAtStartPoint.Y, -pathTangentAtStartPoint.X)*
                                             (180/Math.PI);
                    this.AnchorAngleSink = Math.Atan2(pathTangentAtEndPoint.Y, pathTangentAtEndPoint.X)*(180/Math.PI);

                    // add some margin on source and sink side for visual reasons only
                    //pathStartPoint.Offset(-pathTangentAtStartPoint.X * 5, -pathTangentAtStartPoint.Y * 5);
                    //pathEndPoint.Offset(pathTangentAtEndPoint.X * 5, pathTangentAtEndPoint.Y * 5);

                    this.AnchorPositionSource = pathStartPoint;
                    this.AnchorPositionSink = pathEndPoint;
                    this.LabelPosition = pathMidPoint;
                }
                catch (Exception ex)
                { }
            }
        }

        private void ShowAdorner()
        {
            // the ConnectionAdorner is created once for each Connection
            if (this.connectionAdorner == null)
            {
                DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    this.connectionAdorner = new ConnectionAdorner(designer, this);
                    adornerLayer.Add(this.connectionAdorner);
                }
            }
            this.connectionAdorner.Visibility = Visibility.Visible;
        }

        internal void HideAdorner()
        {
            if (this.connectionAdorner != null)
                this.connectionAdorner.Visibility = Visibility.Collapsed;
        }

        //void Connection_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    // do some housekeeping when Connection is unloaded

        //    // remove event handler
        //    this.Source = null;
        //    this.Sink = null;

        //    // remove adorner
        //    if (this.connectionAdorner != null)
        //    {
        //        DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;

        //        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
        //        if (adornerLayer != null)
        //        {
        //            adornerLayer.Remove(this.connectionAdorner);
        //            this.connectionAdorner = null;
        //        }
        //    }
        //}

        #region INotifyPropertyChanged Members

        // we could use DependencyProperties as well to inform others of property changes
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }

    public enum ArrowSymbol
    {
        None,
        Arrow,
        Diamond
    }
}
