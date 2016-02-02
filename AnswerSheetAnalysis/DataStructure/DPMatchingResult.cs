using System;
using System.Collections.Generic;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// DP matching result information class
    /// </summary>
    public class DPMatchingResult
    {
        /// <summary>
        /// Distance value
        /// </summary>
        public double Distance { get; set; }
        /// <summary>
        /// Get similarity value
        /// </summary>
        public double Similarity
        {
            get
            {
                double s = double.MaxValue;
                if (this.Distance != 0.0)
                {
                    s = 1.0 / Math.Log(1.0 + this.Distance);
                }
                return s;
            }
        }

        /// <summary>
        /// Matching result information
        /// ID pair is used to represent the relation. -1 represents gap.
        /// </summary>
        public List<int[]> MatchingList { get; set; }

        /// <summary>
        /// Initialization
        /// </summary>
        public DPMatchingResult()
        {
            this.Distance = 0.0;
            this.MatchingList = new List<int[]>();
        }

        /// <summary>
        /// Initialization with setting a matching result
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="matching"></param>
        public DPMatchingResult(double distance, List<int[]> matching)
        {
            this.Distance = distance;
            this.MatchingList = matching;
        }
    }
}
