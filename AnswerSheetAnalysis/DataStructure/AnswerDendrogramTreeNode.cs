using System.Collections.Generic;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Implementation of dendrogram tree node for answer sheet classification
    /// </summary>
    public class AnswerDendrogramTreeNode : TreeNodeBase<AnswerDendrogramTreeNode>
    {
        /// <summary>
        /// Weight value of average intra cluster distance when optimal cluster number is decided.
        /// The more the value increase, the more cluster number increases.
        /// </summary>
        private const double OPTIMAL_DEPTH_DISTANCE_WEIGHT = 0.5;

        /// <summary>
        /// Answer sheet data
        /// </summary>
        public AnswerSheet AnswerData { get; set; }
        /// <summary>
        /// Inter cluster distance of children nodes
        /// </summary>
        public double InterDistance { get; set; }
        /// <summary>
        /// average intra cluster distance
        /// </summary>
        public double IntraDistance { get; set; }

        /// <summary>
        /// Initialization
        /// </summary>
        public AnswerDendrogramTreeNode()
            : base()
        {
            this.AnswerData = null;
            this.InterDistance = 0.0;
            this.IntraDistance = 0.0;
        }
        /// <summary>
        /// Initialization with intra cluster distance calculation
        /// </summary>
        /// <param name="children"></param>
        /// <param name="interDistance"></param>
        /// <param name="answerDistances"></param>
        public AnswerDendrogramTreeNode(AnswerDendrogramTreeNode[] children, double interDistance, double[,] answerDistances)
            : this()
        {
            foreach (AnswerDendrogramTreeNode n in children)
            {
                this.Children.Add(n);
            }
            this.InterDistance = interDistance;
            this.IntraDistance = GetClusterIntraDistance(answerDistances);
        }

        /// <summary>
        /// Get all belonging answer sheets
        /// </summary>
        /// <returns></returns>
        public List<AnswerSheet> GetClusterAnswerSheets()
        {
            List<AnswerSheet> answers = new List<AnswerSheet>();

            if (this.HaveChildren)
            {
                foreach (AnswerDendrogramTreeNode node in this.Children)
                {
                    answers.AddRange(node.GetClusterAnswerSheets());
                }
            }
            else
            {
                answers.Add(this.AnswerData);
            }

            return answers;
        }

        /// <summary>
        /// Get average intra cluster distance
        /// </summary>
        /// <param name="answerDistances"></param>
        /// <returns></returns>
        private double GetClusterIntraDistance(double[,] answerDistances)
        {
            double distance = 0.0;
            List<AnswerSheet> answers = GetClusterAnswerSheets();

            if (answers.Count == 1)
            {
                return 0.0;
            }

            int cnum = 0;
            for (int i = 0, ilen = answers.Count; i < ilen; i++)
            {
                for (int k = i + 1; k < ilen; k++)
                {
                    distance += answerDistances[answers[i].ID, answers[k].ID];
                    cnum++;
                }
            }
            distance /= cnum;

            return distance;
        }

        /// <summary>
        /// Get grouped answer sheet list using intra cluster distance
        /// </summary>
        /// <param name="distance_thres"></param>
        /// <returns></returns>
        public List<AnswerSheetGroup> GetGroupedAnswerSheets(double distance_thres)
        {
            // Grouping answer sheets based on dendrogram tree.
            // When the intra cluster distance is over the distance threshold, the answer sheet grouping is fixed
            List<AnswerSheetGroup> asGroup = new List<AnswerSheetGroup>();

            List<AnswerDendrogramTreeNode> clusterNodeList = new List<AnswerDendrogramTreeNode>();
            AnswerDendrogramTreeNode currentNode = this;
            while (currentNode.HaveChildren && currentNode.InterDistance >= distance_thres)
            {
                AnswerDendrogramTreeNode child1 = (AnswerDendrogramTreeNode)currentNode.Children[0];
                AnswerDendrogramTreeNode child2 = (AnswerDendrogramTreeNode)currentNode.Children[1];
                if ((child1.HaveChildren && child2.HaveChildren && child1.InterDistance <= child2.InterDistance)
                || !child2.HaveChildren)
                {
                    currentNode = child1;
                    clusterNodeList.Add(child2);
                }
                else if ((child1.HaveChildren && child2.HaveChildren && child2.InterDistance < child1.InterDistance)
                    || !child1.HaveChildren)
                {
                    currentNode = child2;
                    clusterNodeList.Add(child1);
                }
            }
            clusterNodeList.Add(currentNode);
            for (int i = 0, ilen = clusterNodeList.Count; i < ilen; i++)
            {
                AnswerSheetGroup g = new AnswerSheetGroup(i);
                g.AnswerSheetList = clusterNodeList[i].GetClusterAnswerSheets();
                asGroup.Add(g);
            }

            return asGroup;
        }

        /// <summary>
        /// Get grouped answer sheet list using tree height
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public List<AnswerSheetGroup> GetGroupedAnswerSheets(int depth)
        {
            List<AnswerSheetGroup> asGroup = new List<AnswerSheetGroup>();

            List<AnswerDendrogramTreeNode> clusterNodeList = new List<AnswerDendrogramTreeNode>();
            clusterNodeList.Add(this);
            for (int i = 0; i < depth; i++)
            {
                List<AnswerDendrogramTreeNode> tmpNodeList = new List<AnswerDendrogramTreeNode>();
                for (int k = 0, klen = clusterNodeList.Count; k < klen; k++)
                {
                    if (clusterNodeList[k].HaveChildren)
                    {
                        for (int m = 0, mlen = clusterNodeList[k].Children.Count; m < mlen; m++)
                        {
                            tmpNodeList.Add((AnswerDendrogramTreeNode)(clusterNodeList[k].Children[m]));
                        }
                    }
                    else
                    {
                        tmpNodeList.Add(clusterNodeList[k]);
                    }
                }
                clusterNodeList = tmpNodeList;
            }
            for (int i = 0, ilen = clusterNodeList.Count; i < ilen; i++)
            {
                AnswerSheetGroup g = new AnswerSheetGroup(i);
                g.AnswerSheetList = clusterNodeList[i].GetClusterAnswerSheets();
                asGroup.Add(g);
            }

            return asGroup;
        }

        /// <summary>
        /// Get optimal tree depth using average intra cluster distance
        /// </summary>
        /// <returns></returns>
        public int GetOptimalTreeDepth()
        {
            int optimalDepth = 0;

            List<AnswerDendrogramTreeNode> clusterNodeList = new List<AnswerDendrogramTreeNode>();
            clusterNodeList.Add(this);
            double parentScore = double.MaxValue;
            for (int i = 0, ilen = this.GetHeight(); i <= ilen; i++)
            {
                double currentScore = 0.0;
                foreach (AnswerDendrogramTreeNode n in clusterNodeList)
                {
                    currentScore += n.IntraDistance;
                }
                currentScore /= clusterNodeList.Count;
                currentScore = OPTIMAL_DEPTH_DISTANCE_WEIGHT * currentScore + (1 - OPTIMAL_DEPTH_DISTANCE_WEIGHT) * clusterNodeList.Count;
                //Console.WriteLine("Current Cluster Fit Score: " + currentScore + ", Depth: " + i);

                if (parentScore < currentScore)
                {
                    break;
                }
                parentScore = currentScore;
                optimalDepth = i;

                List<AnswerDendrogramTreeNode> tmpNodeList = new List<AnswerDendrogramTreeNode>();
                foreach (AnswerDendrogramTreeNode n in clusterNodeList)
                {
                    if (n.HaveChildren)
                    {
                        foreach (AnswerDendrogramTreeNode cn in n.children)
                        {
                            tmpNodeList.Add(cn);
                        }
                    }
                    else
                    {
                        tmpNodeList.Add(n);
                    }
                }
                clusterNodeList = tmpNodeList;
            }

            return optimalDepth;
        }
    }
}
