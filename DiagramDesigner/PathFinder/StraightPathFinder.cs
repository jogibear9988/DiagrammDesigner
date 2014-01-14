using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DiagramDesigner.PathFinder
{    
    internal class StraightPathFinder : IPathFinder
    {
        private const int margin = 0;

        public Point GetTextPosition(Point anchorSource, Point anchorSink, List<Point> points)
        {
            if (anchorSource != null && anchorSink != null)
            {
                var x = (anchorSource.X + anchorSink.X) / 2;
                var y = (anchorSource.Y + anchorSink.Y) / 2;
                return new Point(Math.Abs(x), Math.Abs(y));
            }

            return new Point(0, 0);
        }

        public List<Point> GetConnectionLine(ConnectorInfo source, ConnectorInfo sink, bool showLastLine)
        {
            List<Point> linePoints = new List<Point>();

            //Rect rectSource = PathFinderHelper.GetRectWithMargin(source, margin);
            //Rect rectSink = PathFinderHelper.GetRectWithMargin(sink, margin);

            //Point startPoint = PathFinderHelper.GetOffsetPoint(source, rectSource);
            //Point endPoint = PathFinderHelper.GetOffsetPoint(sink, rectSink);

            linePoints.Add(source.Position);
            linePoints.Add(sink.Position);
            
            return linePoints;
        }        

        public List<Point> GetConnectionLine(ConnectorInfo source, Point sinkPoint, ConnectorOrientation preferredOrientation)
        {
            List<Point> linePoints = new List<Point>();
            //Rect rectSource = PathFinderHelper.GetRectWithMargin(source, 0);
            //Point startPoint = PathFinderHelper.GetOffsetPoint(source, rectSource);
            //Point endPoint = sinkPoint;

            linePoints.Add(source.Position);
            linePoints.Add(sinkPoint);
            
            return linePoints;
        }

    }
}
