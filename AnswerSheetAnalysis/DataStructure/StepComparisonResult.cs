
namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Answer step omparison result information class
    /// </summary>
    public class StepComparisonResult
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

        private AnswerStep step1;
        /// <summary>
        /// Compairing answer step information 1
        /// </summary>
        public AnswerStep Step1
        {
            get
            {
                return this.step1;
            }
        }

        private AnswerStep step2;
        /// <summary>
        /// Comparing answer step information 2
        /// </summary>
        public AnswerStep Step2
        {
            get
            {
                return this.step2;
            }
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="res"></param>
        public StepComparisonResult(AnswerStep s1, AnswerStep s2, DPMatchingResult res)
        {
            this.step1 = s1;
            this.step2 = s2;
            this.results = res;
        }
    }
}
