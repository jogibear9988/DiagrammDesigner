using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DiagramDesigner.PathFinder
{
    internal static class PathFinderHelper
    {
        private static StraightPathFinder straightPathFinder = new StraightPathFinder();
        private static OrthogonalPathFinder orthogonalPathFinder = new OrthogonalPathFinder();

        public static IPathFinder GetPathFinder(PathFinderTypes pathFinder)
        {
            switch (pathFinder)
            {
                case PathFinderTypes.OrthogonalPathFinder:
                    return orthogonalPathFinder;
                default:
                    return straightPathFinder;
            }
        }


        internal static Rect GetRectWithMargin(ConnectorInfo connectorThumb, double margin)
        {
            Rect rect = new Rect(connectorThumb.DesignerItemLeft,
                                 connectorThumb.DesignerItemTop,
                                 connectorThumb.DesignerItemSize.Width,
                                 connectorThumb.DesignerItemSize.Height);

            rect.Inflate(margin, margin);

            return rect;
        }

        internal static double Distance(Point p1, Point p2)
        {
            return Point.Subtract(p1, p2).Length;
        }

        internal static Point GetOffsetPoint(ConnectorInfo connector, Rect rect)
        {
            Point offsetPoint = new Point();

            switch (connector.Orientation)
            {
                case ConnectorOrientation.Left:
                    offsetPoint = new Point(rect.Left, connector.Position.Y);
                    break;
                case ConnectorOrientation.Top:
                    offsetPoint = new Point(connector.Position.X, rect.Top);
                    break;
                case ConnectorOrientation.Right:
                    offsetPoint = new Point(rect.Right, connector.Position.Y);
                    break;
                case ConnectorOrientation.Bottom:
                    offsetPoint = new Point(connector.Position.X, rect.Bottom);
                    break;
                default:
                    break;
            }

            return offsetPoint;
        }

    }
}
