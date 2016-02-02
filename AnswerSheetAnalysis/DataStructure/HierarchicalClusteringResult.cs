using System.Collections.Generic;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Hierarchical clustering result information class
    /// </summary>
    public class HierarchicalClusteringResult
    {
        /// <summary>
        /// Root node of dendrogram tree
        /// </summary>
        public AnswerDendrogramTreeNode RootTree { get; set; }

        /// <summary>
        /// Distance matrix among answer sheets
        /// </summary>
        public double[,] AnswerSheetDistances { get; set; }

        /// <summary>
        /// Obtain optimal tree depth
        /// </summary>
        /// <returns></returns>
        public int GetOptimalTreeDepth()
        {
            return this.RootTree.GetOptimalTreeDepth();
        }

        /// <summary>
        /// Get answer sheet groups using depth value
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public List<AnswerSheetGroup> GetGroupedAnswerSheets(int depth)
        {
            return this.RootTree.GetGroupedAnswerSheets(depth);
        }
    }
}
