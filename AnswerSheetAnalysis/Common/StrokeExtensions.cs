using System.Windows.Ink;
using System.Windows;
using System.Windows.Input;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Expand "Stroke" class
    /// </summary>
    public static class StrokeExtensions
    {
        /// <summary>
        /// Get center of gravity
        /// </summary>
        /// <param name="stroke"></param>
        /// <returns></returns>
        public static Point getCenter(this Stroke stroke)
        {
            double x = 0.0;
            double y = 0.0;
            foreach (StylusPoint p in stroke.StylusPoints)
            {
                x += p.X;
                y += p.Y;
            }
            x /= stroke.StylusPoints.Count;
            y /= stroke.StylusPoints.Count;

            return new Point(x, y);
        }
    }
}
