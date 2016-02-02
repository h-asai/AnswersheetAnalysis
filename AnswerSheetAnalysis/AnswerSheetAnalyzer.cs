using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Windows.Ink;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Answer process analysis class
    /// </summary>
    public class AnswerSheetAnalyzer
    {

        #region Parameters

        // Answer step grouping (time and distance)
        /// <summary>
        /// Alpha parameter of answer step grouping (time and distance)
        /// </summary>
        private const double GROUP_TE_ALPHA = 1.0;
        /// <summary>
        /// Beta parameter of answer step grouping (time and distance)
        /// </summary>
        private const double GROUP_TE_BETA = 20.0;
        /// <summary>
        /// Threshold of answer step grouping (time and distance)
        /// </summary>
        private const double GROUP_TE_THRES = 2000;

        // Answer step grouping (manhattan distance)
        /// <summary>
        /// Y-axis weight of answer step grouping (manhattan distance)
        /// </summary>
        private const double GROUP_WM_YWEIGHT = 0.80;
        /// <summary>
        /// Threshold of answer step grouping (manhattan distance)
        /// </summary>
        private const double GROUP_WM_THRES = 200;
        /// <summary>
        /// Y-axis overlap threshold for reworking in answer step grouping (manhattan distance)
        /// </summary>
        private const double GROUP_WM_OVERLAP_Y = 0.6;

        // Answer sheet grouping (hierarchical clustering / group average method)
        /// <summary>
        /// Threshold for cluster division in answer sheet grouping (hierarchical clustering / group average method)
        /// </summary>
        private const double CLUSTER_GAM_DTHRES = 2.0;

        // Calculate stroke similarity
        /// <summary>
        /// Threshold of ramer sampling
        /// </summary>
        //private const double RamerSamplingDistanceThres = 4.0;
        private const double RamerSamplingDistanceThres = 5.0;

        // Calculate answer step similarity
        /// <summary>
        /// Normalized height of bounding box
        /// </summary>
        public const double NormarizingHeight = 100.0;

        // Stroke visualization
        /// <summary>
        /// Maximum value of stroke comparison visualization
        /// </summary>
        private const int MaxStrokeComparisonExample = 1000;

        /// <summary>
        /// Maximum value of step comparison visualization
        /// </summary>
        private const int MaxStepComparisonExample = 1000;

        #endregion

        #region Properties

        private string name;
        /// <summary>
        /// Answer sheet group name
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
        }
        /// <summary>
        /// Result value of hierarchical clustering
        /// </summary>
        private HierarchicalClusteringResult hierarchicalResult = null;
        /// <summary>
        /// Result of hierarchical clustering
        /// </summary>
        public HierarchicalClusteringResult HierarchicalResult
        {
            get
            {
                return this.hierarchicalResult;
            }
        }
        /// <summary>
        /// Result value of hierarchical clustering using answering time
        /// </summary>
        private HierarchicalClusteringResult hierarchicalResultAnswerTime = null;
        /// <summary>
        /// Result of hierarchical clustering using answering time
        /// </summary>
        public HierarchicalClusteringResult HierarchicalResultAnswerTime
        {
            get
            {
                return this.hierarchicalResultAnswerTime;
            }
        }

        #endregion

        /// <summary>
        /// Initialization
        /// </summary>
        public AnswerSheetAnalyzer()
        {
        }

        /// <summary>
        /// Enum of clustering methods
        /// </summary>
        public enum ClassificationFeature
        {
            /// <summary>
            /// Proposed method
            /// </summary>
            Proposed,
            /// <summary>
            /// Using answering time
            /// </summary>
            AnswerTime
        }

        #region PublicMethods

        /// <summary>
        /// Load writing data file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<AnalysisPenStroke> LoadStrokesFromFile(string path)
        {
            List<AnalysisPenStroke> strokes = new List<AnalysisPenStroke>();
            using (StreamReader sr = new StreamReader(path))
            {
                while (sr.Peek() >= 0)
                {
                    string jsonStr = sr.ReadLine();
                    AnalysisPenStroke dnpStroke = new AnalysisPenStroke(jsonStr);
                    Stroke stroke = dnpStroke.GetStrokeObject(Config.OutputCanvasWidth, Config.OutputCanvasHeight);
                    strokes.Add(dnpStroke);
                }
            }

            return strokes;
        }

        /// <summary>
        /// Get answer sheet object by loading writing data file
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns></returns>
        public AnswerSheet LoadAnswerSheetFromFile(string path, int id = 0)
        {
            AnswerSheet ans = new AnswerSheet(id);
            ans.FilePath = path;
            ans.Name = System.IO.Path.GetFileNameWithoutExtension(path);
            ans.Strokes = LoadStrokesFromFile(path);

            return ans;
        }

        /// <summary>
        /// Obtain answer sheet list by loading directory which is in writing data files
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<AnswerSheet> LoadAnswerSheetsFromDirectory(string path)
        {
            List<AnswerSheet> answerList = new List<AnswerSheet>();
            int idCount = 0;

            string[] ansPaths = Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly);
            for (int i = 0, ilen = ansPaths.Length; i < ilen; i++)
            {
                AnswerSheet ans = LoadAnswerSheetFromFile(ansPaths[i], idCount);
                if (ans != null)
                {
                    answerList.Add(ans);
                    idCount++;
                }
            }

            return answerList;
        }

        /// <summary>
        /// Group answer sheets
        /// </summary>
        /// <param name="dirPath">Directory path which includes answer sheet data files</param>
        public void GroupAnswerSheet(string dirPath)
        {
            List<AnswerSheet> answerList = LoadAnswerSheetsFromDirectory(dirPath);
            GroupAnswerSheet(answerList, System.IO.Path.GetFileNameWithoutExtension(dirPath));
        }
        /// <summary>
        /// Group answer sheets
        /// </summary>
        /// <param name="answers">List of answer sheets</param>
        /// <param name="name">Name of answer group</param>
        /// <returns></returns>
        public void GroupAnswerSheet(List<AnswerSheet> answers, string name)
        {
            // Calculate processing time
            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            this.name = name;
            this.hierarchicalResult = ClusteringAnswerSheets(answers, ClassificationFeature.Proposed);
            this.hierarchicalResultAnswerTime = ClusteringAnswerSheets(answers, ClassificationFeature.AnswerTime);

            //sw.Stop();
            //Console.WriteLine("Processing time: " + sw.Elapsed);
        }

        /// <summary>
        /// Get similarity ranking of model answer
        /// </summary>
        /// <param name="answers"></param>
        /// <param name="modelAnswer"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<AnswerSheet> ModelAnswerExtraction(List<AnswerSheet> answers, AnswerSheet modelAnswer, string name)
        {
            this.name = name;
            List<AnswerSheet> sortedAnswers = SortAnswerByModelAnswerSimilarity(answers, modelAnswer);

            return sortedAnswers;
        }

        /// <summary>
        /// Get sample of stroke comparison result
        /// </summary>
        /// <returns></returns>
        public List<StrokeComparisonResult> GetVisualizedStrokeComparisonExamples()
        {
            List<StrokeComparisonResult> res = null;

            List<AnswerSheetGroup> groups = this.hierarchicalResult.GetGroupedAnswerSheets(0);

            res = GetStrokeComparisons(groups[0], AnswerSheetAnalyzer.MaxStrokeComparisonExample);

            return res;
        }

        /// <summary>
        /// Get sample of step comparison result
        /// </summary>
        /// <returns></returns>
        public List<StepComparisonResult> GetVisualizedStepComparisonExamples()
        {
            List<StepComparisonResult> res = null;

            List<AnswerSheetGroup> groups = this.hierarchicalResult.GetGroupedAnswerSheets(0);

            res = GetStepComparisons(groups[0], AnswerSheetAnalyzer.MaxStepComparisonExample);

            return res;
        }

        /// <summary>
        /// Get cluster tree height
        /// </summary>
        /// <returns></returns>
        public int GetClusterTreeHeight(ClassificationFeature feature)
        {
            int height = 0;
            switch (feature)
            {
                case ClassificationFeature.Proposed:
                    height = this.hierarchicalResult.RootTree.GetHeight();
                    break;
                case ClassificationFeature.AnswerTime:
                    height = this.hierarchicalResultAnswerTime.RootTree.GetHeight();
                    break;
            }
            return height;
        }

        /// <summary>
        /// Group answer step. Result is set to this.answerGroupList.
        /// Weighted manhattan distance method
        /// </summary>
        /// <param name="strokes"></param>
        /// <returns>Result of grouping</returns>
        public List<AnswerStep> GroupAnswerStep(List<AnalysisPenStroke> strokes)
        {
            List<AnswerStep> ansGroupList = new List<AnswerStep>();

            // grouping by using weighted manhattan distance (WMD)
            // AnsGrp:0 - AnsGrp:N
            AnswerStep ansGroup = new AnswerStep(0);
            for (int i = 0, ilen = strokes.Count - 1; i < ilen; i++)
            {
                ansGroup.Strokes.Add(strokes[i]);
                double d = CommonFunction.WeightedManhattanDistance(strokes[i].CenterPoint, strokes[i + 1].CenterPoint, 1.0 - GROUP_WM_YWEIGHT, GROUP_WM_YWEIGHT);
                //Console.WriteLine("WMDistance: " + d);
                if (d > GROUP_WM_THRES)
                {
                    ansGroupList.Add(ansGroup);
                    ansGroup = new AnswerStep(ansGroupList.Count);
                }
            }
            ansGroup.Strokes.Add(strokes[strokes.Count - 1]);
            ansGroupList.Add(ansGroup);

            // for reworking
            bool combined = true;
            while (combined)
            {
                combined = false;
                for (int i = 0, ilen = ansGroupList.Count; i < ilen; i++)
                {
                    for (int k = i + 1; k < ilen; k++)
                    {
                        Rect ri = ansGroupList[i].GetBounds();
                        Rect rk = ansGroupList[k].GetBounds();
                        // Overlap condition
                        if (rk.Top < ri.Bottom && ri.Top < rk.Bottom)
                        {
                            // calculate y-axis overlap amount
                            double overlap = 0.0;
                            if (ri.Top <= rk.Top && ri.Bottom <= rk.Bottom)
                            {
                                overlap = (ri.Bottom - rk.Top) / ((ri.Height < rk.Height) ? ri.Height : rk.Height);
                            }
                            else if (rk.Top <= ri.Top && rk.Bottom <= ri.Bottom)
                            {
                                overlap = (rk.Bottom - ri.Top) / ((ri.Height < rk.Height) ? ri.Height : rk.Height);
                            }
                            else
                            {
                                overlap = 1.0;
                            }

                            if (GROUP_WM_OVERLAP_Y < overlap)
                            {
                                // calculate x-axis distance
                                double dx = GROUP_WM_THRES / (1.0 - GROUP_WM_YWEIGHT);
                                if (rk.Left < ri.Right + dx && ri.Left - dx < rk.Right)
                                {
                                    // combine answer step group
                                    ansGroupList[i].Strokes.AddRange(ansGroupList[k].Strokes);
                                    ansGroupList.RemoveAt(k);

                                    combined = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (combined) break;
                }
            }
            // reassign group ID
            for (int i = 0, ilen = ansGroupList.Count; i < ilen; i++)
            {
                ansGroupList[i].GroupID = i;
            }

            return ansGroupList;
        }

        /// <summary>
        /// Calculate answer process similarity
        /// </summary>
        /// <param name="group1"></param>
        /// <param name="group2"></param>
        /// <returns>Matching result</returns>
        public DPMatchingResult CalcAnswerProcessSimilarity(List<AnswerStep> group1, List<AnswerStep> group2)
        {
            // parameters----------------
            // dynamic programming matching
            double gapCost = 40.0;
            // weight of join cost. 1: without JOIN penalty, 0: with JOIN penalty same as gapCost
            double joinCostWeight = 1.0;
            // ---------------------------

            if (group1.Count == 0 && group2.Count == 0)
            {
                DPMatchingResult result = new DPMatchingResult(0.0, new List<int[]>());
                return result;
            }
            if (group1.Count == 0)
            {
                DPMatchingResult result = new DPMatchingResult(1.0 / ((double)group2.Count * gapCost), new List<int[]>());
                return result;
            }
            if (group2.Count == 0)
            {
                DPMatchingResult result = new DPMatchingResult(1.0 / ((double)group1.Count * gapCost), new List<int[]>());
                return result;
            }

            // DP matching
            double[,] m = new double[group1.Count + 1, group2.Count + 1];
            // from which direction?
            // 0-n->Match, -1->fromGRP1Skip, -2->fromGRP2Skip
            int[,] from = new int[group1.Count + 1, group2.Count + 1];
            double[,] dm = new double[group1.Count, group2.Count];

            m[0, 0] = 0.0;
            // implememtation of corner case
            // entry point for part matching
            for (int i = 1, ilen = group1.Count + 1; i < ilen; i++)
            {
                m[i, 0] = (double)i * gapCost;
                from[i, 0] = -1;
            }
            for (int i = 0, ilen = group2.Count + 1; i < ilen; i++)
            {
                m[0, i] = (double)i * gapCost;
                from[0, i] = -2;
            }
            from[0, 0] = 0;

            for (int i = 1, ilen = group1.Count + 1; i < ilen; i++)
            {
                for (int k = 1, klen = group2.Count + 1; k < klen; k++)
                {
                    double distance_match = double.MaxValue;

                    if (from[i - 1, k - 1] >= 0)
                    {
                        // cannot join
                        distance_match = CalcAnswerStepDistance(group1[i - 1], group2[k - 1]).Distance;
                        from[i, k] = 0;
                    }
                    else if (from[i - 1, k - 1] == -1)
                    {
                        // can join at group 1
                        for (int n = 0; from[i - 1 - n, k - 1] == -1; n++)
                        {
                            // n-> number of joins
                            AnswerStep join_group = new AnswerStep(i - 1, group1[i - 1].Strokes);
                            for (int o = 1; o <= n; o++)
                            {
                                join_group.JoinStrokes(group1[i - 1 - o]);
                            }
                            double distance_tmp = CalcAnswerStepDistance(join_group, group2[k - 1]).Distance;
                            if (distance_tmp < distance_match)
                            {
                                distance_match = distance_tmp;
                                from[i, k] = n;
                            }
                        }
                    }
                    else if (from[i - 1, k - 1] == -2)
                    {
                        // can join at group 2
                        for (int n = 0; from[i - 1, k - 1 - n] == -2; n++)
                        {
                            // n-> number of joins
                            AnswerStep join_group = new AnswerStep(k - 1, group2[k - 1].Strokes);
                            for (int o = 1; o <= n; o++)
                            {
                                join_group.JoinStrokes(group2[k - 1 - o]);
                            }
                            double distance_tmp = CalcAnswerStepDistance(group1[i - 1], join_group).Distance;
                            if (distance_tmp < distance_match)
                            {
                                distance_match = distance_tmp;
                                from[i, k] = n;
                            }
                        }
                    }
                    dm[i - 1, k - 1] = distance_match;
                    //Console.WriteLine("AnswerStep Distance: " + distance_match.ToString());

                    double from_grp1 = m[i - 1, k] + gapCost;
                    double from_grp2 = m[i, k - 1] + gapCost;
                    double from_match = m[i - 1, k - 1] + distance_match - ((double)from[i, k] * gapCost * joinCostWeight);
                    if (1 < i && 1 < k)
                    {
                        // can match, skip
                        ;
                    }
                    else if (i == 1 && k == 1)
                    {
                        // can only match
                        from_grp1 = double.MaxValue;
                        from_grp2 = double.MaxValue;
                    }
                    else if (k == 1)
                    {
                        // can match and group 1 skip
                        from_grp2 = double.MaxValue;
                    }
                    else if (i == 1)
                    {
                        // can match and group 2 skip
                        from_grp1 = double.MaxValue;
                    }

                    if (from_match <= from_grp1 && from_match <= from_grp2)
                    {
                        m[i, k] = from_match;
                    }
                    else if (from_grp1 <= from_grp2 && from_grp1 <= from_match)
                    {
                        from[i, k] = -1;
                        m[i, k] = from_grp1;
                    }
                    else if (from_grp2 <= from_grp1 && from_grp2 <= from_match)
                    {
                        from[i, k] = -2;
                        m[i, k] = from_grp2;
                    }
                }
            }

            // generate matching list
            // {group1Pos, group2Pos, distance, joinCount}
            List<int[]> matchingList = new List<int[]>();
            int g1pos = group1.Count - 1;
            int g2pos = group2.Count - 1;
            //int matchCnt = 0;
            for (int i = group1.Count, k = group2.Count; i != 0 || k != 0; )
            {
                int[] match = null;
                switch (from[i, k])
                {
                    case -1:
                        match = new int[4] { g1pos, -1, -1, 0 };
                        i--;
                        g1pos--;
                        break;
                    case -2:
                        match = new int[4] { -1, g2pos, -1, 0 };
                        k--;
                        g2pos--;
                        break;
                    default:
                        // match
                        match = new int[4] { g1pos, g2pos, (int)(dm[g1pos, g2pos] * 100.0), from[i,k] };
                        i--;
                        k--;
                        g1pos--;
                        g2pos--;
                        //matchCnt++;
                        break;
                }
                matchingList.Add(match);
            }
            matchingList.Reverse();

            //DPMatchingResult res = new DPMatchingResult(m[group1.Count, group2.Count] / (group1.Count + group2.Count + 1.0), matchingList);
            DPMatchingResult res = new DPMatchingResult(m[group1.Count, group2.Count] / (Math.Min(group1.Count, group2.Count) + 1.0), matchingList);
            return res;
        }

        #endregion

        #region PrivateMethods

        /// <summary>
        /// Get samples of stroke comparison
        /// </summary>
        /// <param name="group">Answer sheet group</param>
        /// <param name="max">Maximum number</param>
        /// <returns>List of comparison result</returns>
        private List<StrokeComparisonResult> GetStrokeComparisons(AnswerSheetGroup group, int max)
        {
            List<StrokeComparisonResult> results = new List<StrokeComparisonResult>();

            List<AnswerStep> steps1 = GroupAnswerStep(group.AnswerSheetList[0].Strokes);
            List<AnswerStep> steps2 = GroupAnswerStep(group.AnswerSheetList[1].Strokes);

            int cnt = 0;
            bool limit = false;
            foreach (AnswerStep step1 in steps1)
            {
                List<AnalysisPenStroke> strokes1 = step1.GetNormalizedStrokes(AnswerSheetAnalyzer.NormarizingHeight, false);
                foreach (AnswerStep step2 in steps2)
                {
                    List<AnalysisPenStroke> strokes2 = step2.GetNormalizedStrokes(AnswerSheetAnalyzer.NormarizingHeight, false);
                    for (int i = 0, ilen = strokes1.Count; i < ilen; i++)
                    {
                        for (int k = 0, klen = strokes2.Count; k < klen; k++)
                        {
                            DPMatchingResult matchRes = CalcStrokeDistance(strokes1[i], strokes2[k]);
                            Point[] p1 = strokes1[i].GetRamerSampledPoints_NonRecursive(AnswerSheetAnalyzer.RamerSamplingDistanceThres);
                            Point[] p2 = strokes2[k].GetRamerSampledPoints_NonRecursive(AnswerSheetAnalyzer.RamerSamplingDistanceThres);
                            AnalysisPenStroke sampledStroke1 = new AnalysisPenStroke();
                            AnalysisPenStroke sampledStroke2 = new AnalysisPenStroke();
                            for (int m = 0, mlen = p1.Length; m < mlen; m++)
                            {
                                sampledStroke1.Points.Add(new AnalysisPenPoint(0, p1[m].X, p1[m].Y));
                            }
                            for (int m = 0, mlen = p2.Length; m < mlen; m++)
                            {
                                sampledStroke2.Points.Add(new AnalysisPenPoint(0, p2[m].X, p2[m].Y));
                            }
                            StrokeComparisonResult compResult = new StrokeComparisonResult(strokes1[i], strokes2[k], sampledStroke1, sampledStroke2, matchRes);
                            results.Add(compResult);

                            cnt++;
                            if (max <= cnt)
                            {
                                limit = true;
                                break;
                            }
                        }
                        if (limit) break;
                    }
                    if (limit) break;
                }
                if (limit) break;
            }

            return results;
        }

        /// <summary>
        /// Get samples of step comparison
        /// </summary>
        /// <param name="group">Answer sheet group</param>
        /// <param name="max">Maximum number</param>
        /// <returns>List of comparison result</returns>
        private List<StepComparisonResult> GetStepComparisons(AnswerSheetGroup group, int max)
        {
            List<StepComparisonResult> results = new List<StepComparisonResult>();

            // enumration of all answer sheets combination
            int cnt = 0;
            bool limit = false;
            for (int i = 0, ilen = group.AnswerSheetList.Count; i < ilen; i++)
            {
                List<AnswerStep> steps1 = GroupAnswerStep(group.AnswerSheetList[i].Strokes);
                for (int k = i + 1, klen = group.AnswerSheetList.Count; k < klen; k++)
                {
                    List<AnswerStep> steps2 = GroupAnswerStep(group.AnswerSheetList[k].Strokes);
                    foreach (AnswerStep s1 in steps1)
                    {
                        foreach (AnswerStep s2 in steps2)
                        {
                            DPMatchingResult matchRes = CalcAnswerStepDistance(s1, s2);
                            StepComparisonResult compResult = new StepComparisonResult(s1, s2, matchRes);
                            results.Add(compResult);

                            cnt++;
                            if (max <= cnt)
                            {
                                limit = true;
                                break;
                            }
                        }
                        if (limit) break;
                    }
                    if (limit) break;
                }
                if (limit) break;
            }

            return results;
        }

        /// <summary>
        /// Calculate answer step distance.
        /// DP matching
        /// </summary>
        /// <param name="group1"></param>
        /// <param name="group2"></param>
        /// <returns></returns>
        private DPMatchingResult CalcAnswerStepDistance(AnswerStep group1, AnswerStep group2)
        {
            // parameters ---------------------
            // Gap cost for DP matching
            double gapCost = 100.0;
            // --------------------------------

            List<AnalysisPenStroke> strokes1 = group1.GetNormalizedStrokes(AnswerSheetAnalyzer.NormarizingHeight, true);
            List<AnalysisPenStroke> strokes2 = group2.GetNormalizedStrokes(AnswerSheetAnalyzer.NormarizingHeight, true);

            if (group1.Strokes.Count == 0 && group2.Strokes.Count == 0)
            {
                return new DPMatchingResult(0.0, new List<int[]>());
            }
            if (group1.Strokes.Count == 0)
            {
                return new DPMatchingResult(1.0 / ((double)strokes2.Count * gapCost), new List<int[]>());
            }
            if (group2.Strokes.Count == 0)
            {
                return new DPMatchingResult(1.0 / ((double)strokes1.Count * gapCost), new List<int[]>());
            }

            double[,] m = new double[strokes1.Count, strokes2.Count];
            // from which direction?
            // 0->match, -1->fromStroke1, -2->fromStroke2
            int[,] from = new int[strokes1.Count, strokes2.Count];
            for (int i = 0, ilen = strokes1.Count; i < ilen; i++)
            {
                m[i, 0] = (double)i * gapCost;
                from[i, 0] = -1;
            }
            for (int i = 0, ilen = strokes2.Count; i < ilen; i++)
            {
                m[0, i] = (double)i * gapCost;
                from[0, i] = -2;
            }
            from[0, 0] = 0;
            for (int i = 1, ilen = strokes1.Count; i < ilen; i++)
            {
                for (int k = 1, klen = strokes2.Count; k < klen; k++)
                {
                    double d = 0.0;
                    d = CalcStrokeDistance(strokes1[i], strokes2[k]).Distance;
                    //Console.WriteLine("Stroke Sequence Distance: " + d);
                    //double[] scores = new double[] { m[i - 1, k] + gapCost, m[i - 1, k - 1] + d, m[i, k - 1] + gapCost };
                    //Console.WriteLine("Stroke Sequence Distance Candidates: " + scores[0] + "," + scores[1] + "," + scores[2]);
                    //m[i, k] = scores.Min();

                    double from_grp1 = m[i - 1, k] + gapCost;
                    double from_grp2 = m[i, k - 1] + gapCost;
                    double from_match = m[i - 1, k - 1] + d;
                    if (from_match <= from_grp1 && from_match <= from_grp2)
                    {
                        m[i, k] = from_match;
                    }
                    else if (from_grp1 <= from_grp2 && from_grp1 <= from_match)
                    {
                        from[i, k] = -1;
                        m[i, k] = from_grp1;
                    }
                    else if (from_grp2 <= from_grp1 && from_grp2 <= from_match)
                    {
                        from[i, k] = -2;
                        m[i, k] = from_grp2;
                    }
                }
            }

            // generate matching list
            // {group1Pos, group2Pos, distance, joinCount}
            List<int[]> matchingList = new List<int[]>();
            int g1Pos = strokes1.Count - 1;
            int g2Pos = strokes2.Count - 1;
            for (int i = strokes1.Count - 1, k = strokes2.Count - 1; i != 0 || k != 0; )
            {
                int[] match = null;
                switch (from[i, k])
                {
                    case -1:
                        match = new int[4] { g1Pos, -1, -1, 0 };
                        i--;
                        g1Pos--;
                        break;
                    case -2:
                        match = new int[4] { -1, g2Pos, -1, 0 };
                        k--;
                        g2Pos--;
                        break;
                    default:
                        match = new int[4] { g1Pos, g2Pos, -1, from[i, k] };
                        i--;
                        k--;
                        g1Pos--;
                        g2Pos--;
                        break;
                }
                matchingList.Add(match);
            }
            matchingList.Reverse();
            DPMatchingResult res = new DPMatchingResult(m[strokes1.Count - 1, strokes2.Count - 1] / (Math.Min(strokes1.Count, strokes2.Count)), matchingList);

            return res;
            //return m[strokes1.Count - 1, strokes2.Count - 1] / (Math.Min(strokes1.Count, strokes2.Count));
        }

        /// <summary>
        /// Calculate distance betwee strokes.
        /// DP matching
        /// </summary>
        /// <param name="stroke1"></param>
        /// <param name="stroke2"></param>
        /// <returns></returns>
        private DPMatchingResult CalcStrokeDistance(AnalysisPenStroke stroke1, AnalysisPenStroke stroke2)
        {
            //double distance = 0.0;

            // parameters ----------------------------
            // gap cost for DP matching
            double gapCost = 40.0;
            // ---------------------------------------

            Point[] points1 = stroke1.GetRamerSampledPoints_NonRecursive(AnswerSheetAnalyzer.RamerSamplingDistanceThres);
            Point[] points2 = stroke2.GetRamerSampledPoints_NonRecursive(AnswerSheetAnalyzer.RamerSamplingDistanceThres);

            if (points1.Length == 0 && points2.Length == 0)
            {
                return new DPMatchingResult(0.0, new List<int[]>());
            }
            if (points1.Length == 0)
            {
                return new DPMatchingResult(1.0 / ((double)points2.Length * gapCost), new List<int[]>());
                //return points2.Length * gapCost;
            }
            if (points2.Length == 0)
            {
                return new DPMatchingResult(1.0 / ((double)points1.Length * gapCost), new List<int[]>());
                //return points1.Length * gapCost;
            }

            double[,] m = new double[points1.Length, points2.Length];
            // from which direction?
            // 0->match, -1->fromPoints1, -2->fromPoints2
            int[,] from = new int[points1.Length, points2.Length];
            for (int i = 0, ilen = points1.Length; i < ilen; i++)
            {
                m[i, 0] = (double)i * gapCost;
                from[i, 0] = -1;
            }
            for (int i = 0, ilen = points2.Length; i < ilen; i++)
            {
                m[0, i] = (double)i * gapCost;
                from[0, i] = -2;
            }
            from[0, 0] = 0;
            for (int i = 1, ilen = points1.Length; i < ilen; i++)
            {
                for (int k = 1, klen = points2.Length; k < klen; k++)
                {
                    double d = 0.0;
                    d = CommonFunction.Distance(points1[i], points2[k]);
                    //Console.WriteLine("Stroke distance: " + d);
                    //double[] scores = new double[] { m[i - 1, k] + gapCost, m[i - 1, k - 1] + d, m[i, k - 1] + gapCost };
                    //m[i, k] = scores.Min();

                    double from_grp1 = m[i - 1, k] + gapCost;
                    double from_grp2 = m[i, k - 1] + gapCost;
                    double from_match = m[i - 1, k - 1] + d;
                    if (from_match <= from_grp1 && from_match <= from_grp2)
                    {
                        m[i, k] = from_match;
                    }
                    else if (from_grp1 <= from_grp2 && from_grp1 <= from_match)
                    {
                        from[i, k] = -1;
                        m[i, k] = from_grp1;
                    }
                    else if (from_grp2 <= from_grp1 && from_grp2 <= from_match)
                    {
                        from[i, k] = -2;
                        m[i, k] = from_grp2;
                    }
                }
            }

            // generate matching list
            // {group1Pos, group2Pos, distance, joinCount}
            List<int[]> matchingList = new List<int[]>();
            int g1pos = points1.Length - 1;
            int g2pos = points2.Length - 1;
            //int matchCnt = 0;
            for (int i = points1.Length - 1, k = points2.Length - 1; i != 0 || k != 0; )
            {
                int[] match = null;
                switch (from[i, k])
                {
                    case -1:
                        match = new int[4] { g1pos, -1, -1, 0 };
                        i--;
                        g1pos--;
                        break;
                    case -2:
                        match = new int[4] { -1, g2pos, -1, 0 };
                        k--;
                        g2pos--;
                        break;
                    default:
                        match = new int[4] { g1pos, g2pos, -1, from[i, k] };
                        i--;
                        k--;
                        g1pos--;
                        g2pos--;
                        //matchCnt++;
                        break;
                }
                matchingList.Add(match);
            }
            matchingList.Reverse();

            DPMatchingResult res = new DPMatchingResult(m[points1.Length - 1, points2.Length - 1] / (Math.Min(points1.Length, points2.Length)), matchingList);

            //distance = m[points1.Length - 1, points2.Length - 1] / (Math.Min(points1.Length, points2.Length));

            return res;
        }

        /// <summary>
        /// Group answer sheets in each answer strategy.
        /// Hierarchical clustering / Group average method
        /// </summary>
        /// <param name="answers"></param>
        /// <param name="feature">Using feature</param>
        /// <returns></returns>
        private HierarchicalClusteringResult ClusteringAnswerSheets(List<AnswerSheet> answers, ClassificationFeature feature)
        {
            Stopwatch sw = new Stopwatch();
            Console.WriteLine("Start answer process grouping: " + feature.ToString());
            sw.Start();

            AnswerDendrogramTreeNode treeParent = null;

            // Cache matrix of answer step grouping results
            List<List<AnswerStep>> answerSteps = new List<List<AnswerStep>>();
            if (feature == ClassificationFeature.Proposed)
            {
                Console.WriteLine("Answer Step Grouping...");
                for (int i = 0, ilen = answers.Count; i < ilen; i++)
                {
                    List<AnswerStep> s = GroupAnswerStep(answers[i].Strokes);
                    answerSteps.Add(s);
                }
                Console.WriteLine("Finished: " + sw.Elapsed);
            }

            // Cache matrix of answer process distance results
            Console.WriteLine("Creating answer process similarity table...");
            double[,] answerDistances = new double[answers.Count, answers.Count];
            double[,] answerDistances_Time = new double[answers.Count, answers.Count];
            double adAvg = 0.0;
            double adStd = 0.0;
            double adTimeAvg = 0.0;
            double adTimeStd = 0.0;
            int adNum = 0;
            for (int i = 0, ilen = answers.Count; i < ilen; i++)
            {
                answerDistances[i, i] = 0.0;
                answerDistances_Time[i, i] = 0.0;
            }
            for (int i = 0, ilen = answers.Count; i < ilen; i++)
            {
                for (int k = i + 1; k < ilen; k++)
                {
                    // proposed method
                    if (feature == ClassificationFeature.Proposed)
                    {
                        answerDistances[i, k] = CalcAnswerProcessSimilarity(answerSteps[i], answerSteps[k]).Distance;
                        answerDistances[k, i] = answerDistances[i, k];
                        adAvg += answerDistances[i, k];
                        adStd += answerDistances[i, k] * answerDistances[i, k];
                    }

                    // answering time
                    if (feature == ClassificationFeature.AnswerTime)
                    {
                        // answering time
                        answerDistances_Time[i, k] = Math.Abs((long)(answers[i].AnswerTime - answers[k].AnswerTime));
                        // writing time
                        //answerDistances_Time[i, k] = Math.Abs((long)(answers[i].WritingTime - answers[k].WritingTime));
                        answerDistances_Time[k, i] = answerDistances_Time[i, k];
                        adTimeAvg += answerDistances_Time[i, k];
                        adTimeStd += answerDistances_Time[i, k] * answerDistances_Time[i, k];
                    }

                    adNum++;
                }
            }
            adAvg /= (double)adNum;
            adStd -= adAvg * adAvg;
            adStd = Math.Sqrt(adStd);
            adTimeAvg /= (double)adNum;
            adTimeStd -= adTimeAvg * adTimeAvg;
            adTimeStd = Math.Sqrt(adTimeStd);

            // standardization of answer process similarity distance matrix
            // subtract average and devide standard deviation: average 0 and variance 1
            double[,] stdDistanceTable = new double[answers.Count, answers.Count];
            for (int i = 0, ilen = answers.Count; i < ilen; i++)
            {
                for (int k = i + 1; k < ilen; k++)
                {
                    switch (feature)
                    {
                        case ClassificationFeature.Proposed:
                            stdDistanceTable[i, k] = (answerDistances[i, k] - adAvg) / adStd;
                            break;
                        case ClassificationFeature.AnswerTime:
                            stdDistanceTable[i, k] = (answerDistances_Time[i, k] - adTimeAvg) / adTimeStd;
                            break;
                    }
                    stdDistanceTable[k, i] = stdDistanceTable[i, k];
                }
            }

            Console.WriteLine("Finished: " + sw.Elapsed);

            // Initial condition -> 1 element in 1 cluster
            // bottom up hierarchical clustering
            Console.WriteLine("Clustering answersheets (Group Average Method)...");
            List<AnswerDendrogramTreeNode> treeElements = new List<AnswerDendrogramTreeNode>();
            foreach (AnswerSheet answer in answers)
            {
                AnswerDendrogramTreeNode node = new AnswerDendrogramTreeNode();
                node.AnswerData = answer;
                treeElements.Add(node);
            }
            while (treeElements.Count != 1)
            {
                // Join clusters that indicate minimum distance combination
                int minPair1 = 0;
                int minPair2 = 0;
                double minDistance = double.MaxValue;
                for (int i = 0, ilen = treeElements.Count; i < ilen; i++)
                {
                    for (int k = i + 1; k < ilen; k++)
                    {
                        // calculate group average
                        List<AnswerSheet> cluster1 = treeElements[i].GetClusterAnswerSheets();
                        List<AnswerSheet> cluster2 = treeElements[k].GetClusterAnswerSheets();
                        double d = CalcAnswersGroupAverage(cluster1, cluster2, stdDistanceTable);

                        if (d < minDistance)
                        {
                            minPair1 = i;
                            minPair2 = k;
                            minDistance = d;
                        }
                    }
                }

                // join nodes
                AnswerDendrogramTreeNode child1 = treeElements[minPair1];
                AnswerDendrogramTreeNode child2 = treeElements[minPair2];
                AnswerDendrogramTreeNode[] children = new AnswerDendrogramTreeNode[] { child1, child2 };
                AnswerDendrogramTreeNode node = new AnswerDendrogramTreeNode(children, minDistance, stdDistanceTable);
                treeElements.Add(node);
                treeElements.Remove(child1);
                treeElements.Remove(child2);
            }
            treeParent = treeElements[0];

            sw.Stop();
            Console.WriteLine("Finished: " + sw.Elapsed);

            HierarchicalClusteringResult result = new HierarchicalClusteringResult();
            result.RootTree = treeParent;
            result.AnswerSheetDistances = stdDistanceTable;
            return result;
        }

        /// <summary>
        /// Calculate group average between answer sheet groups
        /// </summary>
        /// <param name="group1"></param>
        /// <param name="group2"></param>
        /// <returns></returns>
        private double CalcAnswersGroupAverage(List<AnswerSheet> group1, List<AnswerSheet> group2)
        {
            return CalcAnswersGroupAverage(group1, group2, null);
        }
        /// <summary>
        /// Calculate group average between answer sheet groups. (Cached version)
        /// </summary>
        /// <param name="group1"></param>
        /// <param name="group2"></param>
        /// <param name="answerDistances">Cache of answer process similarity</param>
        /// <returns></returns>
        private double CalcAnswersGroupAverage(List<AnswerSheet> group1, List<AnswerSheet> group2, double[,] answerDistances)
        {
            double distance = 0.0;

            List<List<AnswerStep>> stepGroup1 = null;
            List<List<AnswerStep>> stepGroup2 = null;
            if (answerDistances == null)
            {
                stepGroup1 = new List<List<AnswerStep>>();
                stepGroup2 = new List<List<AnswerStep>>();
                for (int i = 0, ilen = group1.Count; i < ilen; i++)
                {
                    List<AnswerStep> s = GroupAnswerStep(group1[i].Strokes);
                    stepGroup1.Add(s);
                }
                for (int i = 0, ilen = group2.Count; i < ilen; i++)
                {
                    List<AnswerStep> s = GroupAnswerStep(group2[i].Strokes);
                    stepGroup2.Add(s);
                }
            }

            int cnt = 0;
            for (int i = 0, ilen = group1.Count; i < ilen; i++)
            {
                for (int k = 0, klen = group2.Count; k < klen; k++)
                {
                    double d = 0.0;
                    d = answerDistances[group1[i].ID, group2[k].ID];
                    /*
                    if (answerDistances == null)
                    {
                        AnswerStepMatchingResult matchRes = CalcAnswerProcessSimilarity(stepGroup1[i], stepGroup2[k]);
                        d = matchRes.Distance;
                    }
                    else
                    {
                        d = answerDistances[group1[i].ID, group2[k].ID];
                    }
                     */
                    cnt++;
                    distance += d;
                }
            }
            distance /= cnt;

            return distance;
        }

        /// <summary>
        /// Sort answer sheet by model answer similarity
        /// </summary>
        /// <param name="answers"></param>
        /// <param name="modelAnswer"></param>
        /// <returns></returns>
        private List<AnswerSheet> SortAnswerByModelAnswerSimilarity(List<AnswerSheet> answers, AnswerSheet modelAnswer)
        {
            // calc distance scores
            List<AnswerStep> modelAnsStepList = GroupAnswerStep(modelAnswer.Strokes);
            for (int i = 0, ilen = answers.Count; i < ilen; i++)
            {
                List<AnswerStep> ansStepList = GroupAnswerStep(answers[i].Strokes);
                DPMatchingResult result = CalcAnswerProcessSimilarity(modelAnsStepList, ansStepList);

                answers[i].ModelAnswerDistance = result.Distance;
            }

            // sort
            List<AnswerSheet> ranking = new List<AnswerSheet>(answers.OrderBy(s => s.ModelAnswerDistance));

            return ranking;
        }

        #endregion
    }
}






