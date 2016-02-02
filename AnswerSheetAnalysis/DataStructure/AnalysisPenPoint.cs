using System.Windows;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Object of holding pen points
    /// </summary>
    public class AnalysisPenPoint
    {
        /// <summary>
        /// Time information
        /// </summary>
        public ulong Time { get; set; }
        /// <summary>
        /// X coordinates
        /// </summary>
        public double X { get; set; }
        /// <summary>
        /// Y coordinates
        /// </summary>
        public double Y { get; set; }
        /// <summary>
        /// Get "Point" object
        /// </summary>
        public Point PointObject
        {
            get
            {
                return new Point(this.X, this.Y);
            }
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="time"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public AnalysisPenPoint(ulong time, double x, double y)
        {
            this.Time = time;
            this.X = x;
            this.Y = y;
        }
    }
}
