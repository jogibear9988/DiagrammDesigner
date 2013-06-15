using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DiagramDesigner.PathFinder
{    
    internal class StraightPathFinder : IPathFinder
    {
        private const int margin = 0;

        public List<Point> GetConnectionLine(ConnectorInfo source, ConnectorInfo sink, bool showLastLine)
        {
            List<Point> linePoints = new List<Point>();

            Rect rectSource = PathFinderHelper.GetRectWithMargin(source, margin);
            Rect rectSink = PathFinderHelper.GetRectWithMargin(sink, margin);

            Point startPoint = PathFinderHelper.GetOffsetPoint(source, rectSource);
            Point endPoint = PathFinderHelper.GetOffsetPoint(sink, rectSink);

            linePoints.Add(startPoint);
            linePoints.Add(endPoint);
            
            return linePoints;
        }        

        public List<Point> GetConnectionLine(ConnectorInfo source, Point sinkPoint, ConnectorOrientation preferredOrientation)
        {
            List<Point> linePoints = new List<Point>();
            Rect rectSource = PathFinderHelper.GetRectWithMargin(source, 10);
            Point startPoint = PathFinderHelper.GetOffsetPoint(source, rectSource);
            Point endPoint = sinkPoint;

            linePoints.Add(startPoint);
            linePoints.Add(sinkPoint);
            
            return linePoints;
        }

    }
}
