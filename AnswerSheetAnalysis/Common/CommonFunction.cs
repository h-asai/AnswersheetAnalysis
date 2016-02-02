using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Common functions
    /// </summary>
    public class CommonFunction
    {
        /// <summary>
        /// Calculate euclidean distance between two points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double Distance(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        /// <summary>
        /// Calculate manhattan distance between two points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double ManhattanDistance(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }

        /// <summary>
        /// Calculate weighted-euclidean disntance between two points
        /// </summary>
        /// <param name="p1">Start point</param>
        /// <param name="p2">End point</param>
        /// <param name="wx">Weight of x-axis</param>
        /// <param name="wy">Weight of y-axis</param>
        /// <returns></returns>
        public static double WeightedManhattanDistance(Point p1, Point p2, double wx, double wy)
        {
            return Math.Abs(p1.X - p2.X) * wx + Math.Abs(p1.Y - p2.Y) * wy;
        }

        /// <summary>
        /// Calculate levenshtein distance between two strings
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int LevenshteinDistance(string a, string b)
        {
            int ret = 0;

            if (a.Length == 0 && b.Length == 0)
            {
                return 0;
            }
            if (a.Length == 0)
            {
                return b.Length;
            }
            if (b.Length == 0)
            {
                return a.Length;
            }

            int[,] m = new int[a.Length + 1, b.Length + 1];

            for (int i = 0, ilen = a.Length + 1; i < ilen; i++)
            {
                m[i, 0] = i;
            }

            for (int i = 0, ilen = b.Length + 1; i < ilen; i++)
            {
                m[0, i] = i;
            }

            for (int i = 1, ilen = a.Length + 1; i < ilen; i++)
            {
                for (int k = 1, klen = b.Length + 1; k < klen; k++)
                {
                    int x = 0;
                    if (a[i - 1] == b[k - 1])
                    {
                        x = 0;
                    }
                    else
                    {
                        x = 1;
                    }
                    int[] scores = new int[] { m[i - 1, k] + 1, m[i, k - 1] + 1, m[i - 1, k - 1] + x };
                    m[i, k] = scores.Min();
                }
            }

            ret = m[a.Length - 1, b.Length - 1];
            return ret;
        }

        /// <summary>
        /// Calculate cosine similarity between two point set
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double CosineSimilarity(double[] a, double[] b)
        {
            double norma = 0.0;
            double normb = 0.0;
            double sumproduct = 0.0;

            for (int i = 0, ilen = a.Length; i < ilen; i++)
            {
                norma += a[i] * a[i];
                normb += b[i] * b[i];
                sumproduct += a[i] * b[i];
            }

            if (norma == 0 || normb == 0)
            {
                return 0.0;
            }
            return sumproduct / (Math.Sqrt(norma) * Math.Sqrt(normb));
        }


        /// <summary>
        /// Calculate distance between point and line
        /// </summary>
        /// <param name="p">Coordinates of a point</param>
        /// <param name="a">Start point of a line</param>
        /// <param name="b">End point of a line</param>
        /// <returns></returns>
        public static double PerpendicularDistance(Point p, Point a, Point b)
        {
            Vector vp = new Vector(p.X - a.X, p.Y - a.Y);
            Vector vl = new Vector(b.X - a.X, b.Y - a.Y);
            return Math.Abs(Vector.CrossProduct(vl, vp)) / vl.Length;
        }

        /// <summary>
        /// Calculate heatmap color from numetric value
        /// </summary>
        /// <param name="val">Input value</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="alpha">Alpha value</param>
        /// <returns>Color value of heatmap</returns>
        public static Color GetHeatmapColor(double val, double min, double max, double alpha=255)
        {
            byte r, g, b, a;
            Color color;

            // normalization (0-255)
            int step = Math.Min(Math.Max((int)((val - min) / (max - min) * 255), 0), 255);

            // set red
            if (step < 128)
            {
                r = 0;
            }
            else if (step < 193)
            {
                r = (byte)((step - 127) * 4);
            }
            else
            {
                r = 255;
            }

            // set green
            if (step < 65)
            {
                g = (byte)(step * 4);
            }
            else if (step < 191)
            {
                g = 255;
            }
            else
            {
                g = (byte)(256 - (step - 191) * 4);
            }

            // set blue
            if (step < 65)
            {
                b = 255;
            }
            else if (step < 128)
            {
                b = (byte)(255 - (step - 64) * 4);
            }
            else
            {
                b = 0;
            }

            // validation
            if (val > 0.0000001)
            {
                r = (byte)(Math.Min(Math.Max((int)r, 0), 255));
                g = (byte)(Math.Min(Math.Max((int)g, 0), 255));
                b = (byte)(Math.Min(Math.Max((int)b, 0), 255));
                a = (byte)alpha;
            }
            else
            {
                r = (byte)0;
                g = (byte)0;
                b = (byte)0;
                a = (byte)0;
            }

            color = Color.FromArgb(a, r, g, b);
            return color;
        }
    }
}
