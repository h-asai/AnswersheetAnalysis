using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Codeplex.Data;
using System.Windows.Ink;
using System.Windows.Input;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Object of holding handwritten strokes
    /// </summary>
    public class AnalysisPenStroke
    {
        private Point[] ramerSampledPoints = null;

        /// <summary>
        /// Coordinates of stroke
        /// </summary>
        public List<AnalysisPenPoint> Points { get; set; }
        /// <summary>
        /// Gravity point of stroke
        /// </summary>
        public Point CenterPoint
        {
            get
            {
                double gx = 0.0;
                double gy = 0.0;
                foreach (AnalysisPenPoint p in this.Points)
                {
                    gx += p.X;
                    gy += p.Y;
                }
                gx /= this.Points.Count;
                gy /= this.Points.Count;

                return new Point(gx, gy);
            }
        }
        /// <summary>
        /// Bounding box of stroke
        /// </summary>
        public Rect BoundingBox
        {
            get
            {
                double left = double.MaxValue;
                double top = double.MaxValue;
                double right = 0.0;
                double bottom = 0.0;
                foreach (AnalysisPenPoint p in this.Points)
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
                return new Rect(left, top, right - left, bottom - top);
            }
        }
        /// <summary>
        /// Length of stroke
        /// </summary>
        public double Length
        {
            get
            {
                double l = 0.0;
                for (int i = 0, ilen = this.Points.Count - 1; i < ilen; i++)
                {
                    l += CommonFunction.Distance(this.Points[i].PointObject, this.Points[i + 1].PointObject);
                }
                return l;
            }
        }
        /// <summary>
        /// Density of stroke: Length / Area of bounding box
        /// </summary>
        public double Density
        {
            get
            {
                Rect bb = this.BoundingBox;
                return this.Length / (bb.Width * bb.Height + 1.0);
            }
        }
        /// <summary>
        /// Curveture of stroke: average of cosine values between points of stroke
        /// </summary>
        public double Curveture
        {
            get
            {
                double cos = 0.0;
                if (this.Points.Count < 3)
                {
                    return 0.0;
                }
                for (int i = 1, ilen = this.Points.Count - 1; i < ilen; i++)
                {
                    AnalysisPenPoint p1 = this.Points[i - 1];
                    AnalysisPenPoint p2 = this.Points[i];
                    AnalysisPenPoint p3 = this.Points[i + 1];
                    Vector v1 = new Vector(p1.X - p2.X, p1.Y - p2.Y);
                    Vector v2 = new Vector(p3.X - p2.X, p3.Y - p2.Y);
                    cos += (v1.X * v2.X + v1.Y * v2.Y) / ((v1.Length + 1.0) * (v2.Length + 1.0));
                }
                cos /= this.Points.Count - 2;
                return cos;
            }
        }

        /// <summary>
        /// Sort stroke array in order of x coordinates in bounding box gravity point
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static List<AnalysisPenStroke> SortByCenterX(List<AnalysisPenStroke> s)
        {
            List<AnalysisPenStroke> res = new List<AnalysisPenStroke>(s);
            res.Sort(delegate(AnalysisPenStroke s1, AnalysisPenStroke s2)
            {
                int ret = -1;
                double diff = s1.CenterPoint.X - s2.CenterPoint.X;
                if (diff >= 0.0)
                {
                    ret = 1;
                }
                return ret;
            });

            return res;
        }

        /// <summary>
        /// Initialization
        /// </summary>
        public AnalysisPenStroke()
        {
            this.Points = new List<AnalysisPenPoint>();
        }

        /// <summary>
        /// Generate stroke object from serialized json string
        /// </summary>
        /// <param name="jsonStr"></param>
        public AnalysisPenStroke(String jsonStr)
        {
            var json = DynamicJson.Parse(jsonStr);

            this.Points = new List<AnalysisPenPoint>();
            foreach (var p in json.stroke)
            {
                AnalysisPenPoint ppoint = new AnalysisPenPoint((ulong)p.time, (int)p.x, (int)p.y);
                this.Points.Add(ppoint);
            }
        }

        /// <summary>
        /// Get JSON string of stroke object
        /// </summary>
        /// <returns>JSON strings representing stroke object</returns>
        public String GetJsonString()
        {
            String ret = null;

            List<object> pointArr = new List<object>();
            for (int i = 0, ilen = this.Points.Count; i < ilen; i++)
            {
                var elem = new
                {
                    time = this.Points[i].Time,
                    x = this.Points[i].X,
                    y = this.Points[i].Y,
                };
                pointArr.Add(elem);
            }

            dynamic json = new DynamicJson();
            json.stroke = pointArr;

            ret = json.ToString();
            return ret;
        }

        /// <summary>
        /// Get "Stroke" object that is used for InkCanvas
        /// </summary>
        /// <param name="width">Width of output canvas</param>
        /// <param name="height">Height of output canvas</param>
        /// <param name="scale">Scale for resizing</param>
        /// <param name="offset">Moving offset value</param>
        /// <param name="isOrigin">Move the stroke object to the origin point</param>
        /// <returns></returns>
        public Stroke GetStrokeObject(double width = Config.OriginalA4PaperWidth, double height = Config.OriginalA4PaperHeight, double scale = 1.0, Point offset = default(Point), bool isOrigin = false)
        {
            Stroke stroke = null;

            Point orgOffset = new Point();
            if (isOrigin)
            {
                Rect bounds = this.BoundingBox;
                orgOffset.X = -1.0 * bounds.Left;
                orgOffset.Y = -1.0 * bounds.Top;
            }

            StylusPointCollection points = new StylusPointCollection();
            foreach (AnalysisPenPoint p in this.Points)
            {
                StylusPoint sp = new StylusPoint((p.X + offset.X + orgOffset.X) * width / Config.OriginalA4PaperWidth * scale, (p.Y + offset.Y + orgOffset.Y) * height / Config.OriginalA4PaperHeight * scale);
                points.Add(sp);
            }
            stroke = new Stroke(points);

            return stroke;
        }

        /// <summary>
        /// Get ramer-sampled points. Call "ClearCache" function for recalculation
        /// </summary>
        /// <param name="dthres">Distance threshold value</param>
        /// <returns></returns>
        public Point[] GetRamerSampledPoints(double dthres)
        {
            if (this.ramerSampledPoints == null)
            {
                List<Point> points = new List<Point>();
                foreach (AnalysisPenPoint p in this.Points)
                {
                    points.Add(p.PointObject);
                }
                List<Point> res = RamerSamplingRecursive(points, dthres);
                this.ramerSampledPoints = res.ToArray();
            }

            return this.ramerSampledPoints;
        }
        /// <summary>
        /// Recursion function for ramer sampling
        /// </summary>
        /// <param name="points"></param>
        /// <param name="dthres"></param>
        /// <returns></returns>
        private List<Point> RamerSamplingRecursive(List<Point> points, double dthres)
        {
            List<Point> results = null;

            // find the maximum distance point
            int end = points.Count - 1;
            double dmax = 0.0;
            int index = 0;
            for (int i = 1, ilen = points.Count - 1; i < ilen; i++)
            {
                double d = CommonFunction.PerpendicularDistance(points[i], points[0], points[end]);
                if (dmax < d)
                {
                    dmax = d;
                    index = i;
                }
            }

            if (dthres < dmax)
            {
                List<Point> resPoints1 = RamerSamplingRecursive(points.GetRange(0, index + 1), dthres);
                List<Point> resPoints2 = RamerSamplingRecursive(points.GetRange(index, points.Count - index), dthres);
                results = new List<Point>(resPoints1.GetRange(0, resPoints1.Count - 1));
                results.AddRange(resPoints2);
            }
            else
            {
                results = new List<Point>();
                results.Add(points[0]);
                results.Add(points[end]);
            }

            return results;
        }

        /// <summary>
        /// Get ramer-sampled points without using recursive function. Call "ClearCache" function for recalculation
        /// </summary>
        /// <param name="dthres"></param>
        /// <returns></returns>
        public Point[] GetRamerSampledPoints_NonRecursive(double dthres)
        {
            if (this.ramerSampledPoints == null)
            {
                Point[] points = new Point[this.Points.Count];
                for (int i = 0, ilen = this.Points.Count; i < ilen; i++)
                {
                    points[i] = this.Points[i].PointObject;
                }
                List<Point> sampledPoints = new List<Point>();
                sampledPoints.Add(points[0]);
                sampledPoints.Add(points.Last());
                Queue<int[]> q = new Queue<int[]>();
                q.Enqueue(new int[] {0, points.Length - 1});

                while (q.Count != 0)
                {
                    double dmax = 0.0;
                    int max_index = 0;
                    int[] range = q.Dequeue();
                    for (int i = range[0] + 1; i < range[1]; i++)
                    {
                        double d = CommonFunction.PerpendicularDistance(points[i], points[range[0]], points[range[1]]);
                        if (dmax < d)
                        {
                            dmax = d;
                            max_index = i;
                        }
                    }

                    if (dthres < dmax)
                    {
                        sampledPoints.Add(points[max_index]);
                        q.Enqueue(new int[] { range[0], max_index });
                        q.Enqueue(new int[] { max_index, range[1] });
                    }
                }

                this.ramerSampledPoints = sampledPoints.ToArray();
            }

            return this.ramerSampledPoints;
        }

        /// <summary>
        /// Clear cache
        /// </summary>
        public void ClearCache()
        {
            this.ramerSampledPoints = null;
        }
    }
}
