using System;
using System.Collections.Generic;
using System.Windows.Ink;
using System.Windows;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Answer step information class
    /// </summary>
    public class AnswerStep
    {
        private Point centerPoint = new Point(-1.0, -1.0);
        private List<AnalysisPenStroke> normalizedStrokes = null;

        /// <summary>
        /// Answer step managing ID
        /// </summary>
        public int GroupID { get; set; }
        /// <summary>
        /// Strokes in this answer step
        /// </summary>
        public List<AnalysisPenStroke> Strokes { get; set; }
        /// <summary>
        /// Get gravity point of this answer step group
        /// </summary>
        public Point CenterPoint
        {
            get
            {
                if (centerPoint.X < 0.0)
                {
                    double x = 0.0;
                    double y = 0.0;
                    foreach (AnalysisPenStroke s in this.Strokes)
                    {
                        Stroke stroke = s.GetStrokeObject(Config.OutputCanvasWidth, Config.OutputCanvasHeight);
                        Point center = stroke.getCenter();
                        x += center.X;
                        y += center.Y;
                    }
                    x /= this.Strokes.Count;
                    y /= this.Strokes.Count;

                    this.centerPoint = new Point(x, y);
                }

                return this.centerPoint;
            }
        }
        /// <summary>
        /// Calculate size of answer step graph node visualization
        /// </summary>
        public double NodeSize
        {
            get
            {
                double timeSpan = this.Strokes[this.Strokes.Count - 1].Points[0].Time - this.Strokes[0].Points[0].Time;
                return Math.Log(timeSpan + 10000) * 5.0;
            }
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="id"></param>
        public AnswerStep(int id)
        {
            this.GroupID = id;
            this.Strokes = new List<AnalysisPenStroke>();
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="id"></param>
        /// <param name="s"></param>
        public AnswerStep(int id, List<AnalysisPenStroke> s)
        {
            this.GroupID = id;
            this.Strokes = new List<AnalysisPenStroke>(s);
        }

        /// <summary>
        /// Get normalized strokes. Call "ClearCache" function for recalculating
        /// </summary>
        /// <param name="height">Height of bounding box after the normalization</param>
        /// <param name="sortPos">Sort strokes in order of x coordinate of gravity point</param>
        /// <returns></returns>
        public List<AnalysisPenStroke> GetNormalizedStrokes(double height, bool sortPos)
        {
            if (this.normalizedStrokes == null)
            {
                List<AnalysisPenStroke> strokes = new List<AnalysisPenStroke>(this.Strokes);
                Rect gr = GetBounds();
                double scale = height / gr.Height;

                // position sort
                if (sortPos)
                {
                    strokes = AnalysisPenStroke.SortByCenterX(strokes);
                }

                // normalization
                List<AnalysisPenStroke> nstrokes = new List<AnalysisPenStroke>();
                foreach (AnalysisPenStroke s in strokes)
                {
                    AnalysisPenStroke ns = new AnalysisPenStroke();
                    Rect sr = s.BoundingBox;
                    foreach (AnalysisPenPoint p in s.Points)
                    {
                        AnalysisPenPoint np = new AnalysisPenPoint(p.Time, (p.X - sr.Left) * scale, (p.Y - sr.Top) * scale);
                        ns.Points.Add(np);
                        //Console.Write("(" + np.X + "," + np.Y + "),");
                    }
                    nstrokes.Add(ns);
                    //Console.WriteLine();
                }
                this.normalizedStrokes = nstrokes;
            }

            return this.normalizedStrokes;
        }

        /// <summary>
        /// Get "Stroke" objects in the answer step
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="scale"></param>
        /// <param name="offset"></param>
        /// <param name="isOrigin"></param>
        /// <param name="sort">Sort stroke objects in order of x coordinates of gravity point</param>
        /// <returns></returns>
        public List<Stroke> GetStrokeObjects(double width = Config.OriginalA4PaperWidth, double height = Config.OriginalA4PaperHeight, double scale = 1.0, Point offset = default(Point), bool isOrigin = false, bool sort = false)
        {
            List<Stroke> strokes = new List<Stroke>();

            Point orgOffset = new Point();
            if (isOrigin)
            {
                Rect bounds = this.GetBounds();
                orgOffset.X = -1.0 * bounds.Left;
                orgOffset.Y = -1.0 * bounds.Top;
            }

            List<AnalysisPenStroke> nstrokes = this.Strokes;
            if (sort)
            {
                nstrokes = AnalysisPenStroke.SortByCenterX(this.Strokes);
            }

            foreach (AnalysisPenStroke ds in nstrokes)
            {
                Stroke s = ds.GetStrokeObject(width, height, scale, new Point(offset.X + orgOffset.X, offset.Y + orgOffset.Y));
                strokes.Add(s);
            }

            return strokes;
        }

        /// <summary>
        /// Get bounding box of the answer step
        /// </summary>
        /// <param name="width">Width of drawing canvas</param>
        /// <param name="height">Height of drawing canvas</param>
        /// <returns></returns>
        public Rect GetBounds(double width, double height)
        {
            double left = double.MaxValue;
            double top = double.MaxValue;
            double right = 0.0;
            double bottom = 0.0;

            foreach (AnalysisPenStroke s in this.Strokes)
            {
                Stroke stroke = s.GetStrokeObject(width, height);
                Rect rect = stroke.GetBounds();

                if (left > rect.Left)
                {
                    left = rect.Left;
                }
                if (top > rect.Top)
                {
                    top = rect.Top;
                }
                if (right < rect.Right)
                {
                    right = rect.Right;
                }
                if (bottom < rect.Bottom)
                {
                    bottom = rect.Bottom;
                }
            }

            return new Rect(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// Get bounding box of the answer step
        /// </summary>
        /// <returns></returns>
        public Rect GetBounds()
        {
            double left = double.MaxValue;
            double top = double.MaxValue;
            double right = 0.0;
            double bottom = 0.0;

            foreach (AnalysisPenStroke s in this.Strokes)
            {
                foreach (AnalysisPenPoint p in s.Points)
                {
                    if (left > p.X)
                    {
                        left = p.X;
                    }
                    if (top > p.Y)
                    {
                        top = p.Y;
                    }
                    if (right < p.X)
                    {
                        right = p.X;
                    }
                    if (bottom < p.Y)
                    {
                        bottom = p.Y;
                    }
                }
            }

            return new Rect(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// Clear cache
        /// </summary>
        public void ClearCache()
        {
            this.normalizedStrokes = null;
        }

        /// <summary>
        /// Join an answer step after the answer step
        /// </summary>
        /// <param name="step"></param>
        public void JoinStrokes(AnswerStep step)
        {
            // Calculate offset value for coordinate transformation
            Rect orgBounds = this.GetBounds();
            Rect addBounds = step.GetBounds();
            Point endPoint = new Point(orgBounds.Left, (orgBounds.Top + orgBounds.Bottom) / 2.0);
            Point startPoint = new Point(addBounds.Left, (addBounds.Top + addBounds.Bottom) / 2.0);
            Point offsetPoint = new Point(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);

            // transform coordinates to join anter the origin strokes
            foreach (AnalysisPenStroke addStroke in step.Strokes)
            {
                AnalysisPenStroke s = new AnalysisPenStroke();
                foreach (AnalysisPenPoint addPoint in addStroke.Points)
                {
                    AnalysisPenPoint p = new AnalysisPenPoint(addPoint.Time, addPoint.X + offsetPoint.X, addPoint.Y + offsetPoint.Y);
                    s.Points.Add(p);
                }
                this.Strokes.Add(s);
            }
        }

        /// <summary>
        /// Get bounding box which is joined several answer steps
        /// </summary>
        /// <param name="steps"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Rect GetJoinedBounds(List<AnswerStep> steps, double width, double height)
        {
            double left = double.MaxValue;
            double top = double.MaxValue;
            double right = 0.0;
            double bottom = 0.0;

            foreach (AnswerStep step in steps)
            {
                Rect r = step.GetBounds(width, height);
                if (left > r.Left)
                {
                    left = r.Left;
                }
                if (top > r.Top)
                {
                    top = r.Top;
                }
                if (right < r.Right)
                {
                    right = r.Right;
                }
                if (bottom < r.Bottom)
                {
                    bottom = r.Bottom;
                }
            }

            return new Rect(left, top, right - left, bottom - top);
        }
    }
}
