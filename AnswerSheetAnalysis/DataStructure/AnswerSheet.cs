using System.Collections.Generic;
using System.Linq;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Answer sheet data class
    /// </summary>
    public class AnswerSheet
    {
        /// <summary>
        /// Answer sheet ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Answer sheet name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Stroke data
        /// </summary>
        public List<AnalysisPenStroke> Strokes { get; set; }
        /// <summary>
        /// File path of saved data
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// Answering time
        /// </summary>
        public ulong AnswerTime
        {
            get
            {
                ulong start = this.Strokes[0].Points[0].Time;
                ulong end = this.Strokes.Last().Points.Last().Time;
                return end - start;
            }
        }
        /// <summary>
        /// Similarity with model answer
        /// </summary>
        public double ModelAnswerDistance { get; set; }

        /// <summary>
        /// Ratio of writing time
        /// </summary>
        public double WritingRatio
        {
            get
            {
                return (double)WritingTime / (double)AnswerTime;
            }
        }

        /// <summary>
        /// Writing time
        /// </summary>
        public ulong WritingTime
        {
            get
            {
                ulong writingTime = 0;
                foreach (AnalysisPenStroke s in this.Strokes)
                {
                    writingTime += s.Points.Last().Time - s.Points.First().Time;
                }
                return writingTime;
            }
        }

        /// <summary>
        /// Average of writing speed
        /// </summary>
        public double WritingSpeedAvg
        {
            get
            {
                double speed = 0.0;
                int cnt = 0;
                foreach (AnalysisPenStroke s in this.Strokes)
                {
                    if (s.Points.Count > 10)
                    {
                        double time = (double)(s.Points.Last().Time - s.Points.First().Time);
                        if (time > 0.0)
                        {
                            speed += s.Length / time;
                            cnt++;
                        }
                    }
                }
                speed /= (double)cnt;

                return speed;
            }
        }

        /// <summary>
        /// Variance of writing speed
        /// </summary>
        public double WritingSpeedVar
        {
            get
            {
                double variance = 0.0;
                double avg = 0.0;
                int cnt = 0;
                foreach (AnalysisPenStroke s in this.Strokes)
                {
                    if (s.Points.Count > 10)
                    {
                        double time = (double)(s.Points.Last().Time - s.Points.First().Time);
                        if (time > 0.0)
                        {
                            double speed = s.Length / time;
                            variance += speed * speed;
                            avg += speed;
                            cnt++;
                        }
                    }
                }
                avg /= (double)cnt;
                variance = variance / (double)cnt - avg * avg;

                return variance;
            }
        }

        /// <summary>
        /// Initialization
        /// </summary>
        public AnswerSheet(int id)
        {
            this.ID = id;
            this.Name = null;
            this.Strokes = null;
            this.FilePath = null;
        }
    }
}
