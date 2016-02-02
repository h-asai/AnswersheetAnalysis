
namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Stroke comparison result information class
    /// </summary>
    public class StrokeComparisonResult
    {
        private DPMatchingResult results;
        /// <summary>
        /// Result of DP matching
        /// </summary>
        public DPMatchingResult Results
        {
            get
            {
                return this.results;
            }
        }

        private AnalysisPenStroke stroke1;
        /// <summary>
        /// Comparing stroke information 1
        /// </summary>
        public AnalysisPenStroke Stroke1
        {
            get
            {
                return this.stroke1;
            }
        }

        private AnalysisPenStroke stroke2;
        /// <summary>
        /// Comparing stroke information 2
        /// </summary>
        public AnalysisPenStroke Stroke2
        {
            get
            {
                return this.stroke2;
            }
        }

        private AnalysisPenStroke sampledStroke1;
        /// <summary>
        /// Stroke information 1 after sampling
        /// </summary>
        public AnalysisPenStroke SampledStroke1
        {
            get
            {
                return this.sampledStroke1;
            }
        }

        private AnalysisPenStroke sampledStroke2;
        /// <summary>
        /// Stroke information 2 after sampling
        /// </summary>
        public AnalysisPenStroke SampledStroke2
        {
            get
            {
                return this.sampledStroke2;
            }
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="s1">Comparison stroke 1</param>
        /// <param name="s2">Comparison stroke 2</param>
        /// <param name="sampled_s1">Comparison stroke 1 after sampling</param>
        /// <param name="sampled_s2">Comparison stroke 2 after sampling</param>
        /// <param name="res">Matching result</param>
        public StrokeComparisonResult(AnalysisPenStroke s1, AnalysisPenStroke s2, AnalysisPenStroke sampled_s1, AnalysisPenStroke sampled_s2, DPMatchingResult res)
        {
            this.stroke1 = new AnalysisPenStroke();
            foreach (AnalysisPenPoint p in s1.Points)
            {
                AnalysisPenPoint point = new AnalysisPenPoint(p.Time, p.X, p.Y);
                this.stroke1.Points.Add(point);
            }
            this.stroke2 = new AnalysisPenStroke();
            foreach (AnalysisPenPoint p in s2.Points)
            {
                AnalysisPenPoint point = new AnalysisPenPoint(p.Time, p.X, p.Y);
                this.stroke2.Points.Add(point);
            }
            this.sampledStroke1 = sampled_s1;
            this.sampledStroke2 = sampled_s2;
            this.results = res;
        }
    }
}
