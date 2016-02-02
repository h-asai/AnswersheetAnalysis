using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Ink;
using System.Windows.Shapes;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Answer sheet visualization library
    /// </summary>
    public class AnswerSheetVisualizer
    {
        private AnswerSheetAnalyzer analyzer = null;

        public AnswerSheetVisualizer(AnswerSheetAnalyzer a)
        {
            this.analyzer = a;
        }

        /// <summary>
        /// Visualize answer sheet
        /// </summary>
        /// <param name="filePath">Path to answer data file</param>
        /// <param name="inkCanvas">Stroke drawing canvas</param>
        /// <param name="answerStepGraphCanvas">Answer step graph drawing canvas</param>
        /// <param name="showAnswerStepGraph">Draw answer step graph</param>
        /// <param name="colorAnswerSteps">Color strokes by each answer step</param>
        /// <param name="showAnswerStepBox">Draw bounding box of answer step</param>
        public void VisualizeAnswerSheet(string filePath, InkCanvas inkCanvas, Canvas answerStepGraphCanvas,
            bool showAnswerStepGraph = false,
            bool colorAnswerSteps = false,
            bool showAnswerStepBox = false)
        {
            List<AnalysisPenStroke> strokes = this.analyzer.LoadStrokesFromFile(filePath);

            inkCanvas.Strokes.Clear();
            answerStepGraphCanvas.Children.Clear();

            List<AnswerStep> ansGroupList = this.analyzer.GroupAnswerStep(strokes);
            if (colorAnswerSteps)
            {
                ColorAnswerGroupStrokes(ansGroupList, inkCanvas);
            }
            else
            {
                ColorAnswerGroupStrokes(ansGroupList, inkCanvas, true);
            }
            if (showAnswerStepBox)
            {
                VisualizeAnswerGroupBoundingBox(ansGroupList, answerStepGraphCanvas);
            }
            if (showAnswerStepGraph)
            {
                DrawAnswerGroupGraph(ansGroupList, answerStepGraphCanvas);
            }
        }

        /// <summary>
        /// Visualize answer sheet comparison
        /// </summary>
        /// <param name="filePath1">Path to answer 1 file</param>
        /// <param name="filePath2">Path to answer 2 file</param>
        /// <param name="inkCanvas1">Stroke drawing canvas of answer 1</param>
        /// <param name="inkCanvas2">Stroke drawing canvas of answer 2</param>
        /// <param name="graphCanvas">Answer step graph drawing canvas</param>
        /// <param name="colorAnswerSteps">Color strokes by each answer step</param>
        public void VisualizeAnswerSheetComparison(string filePath1, string filePath2, InkCanvas inkCanvas1, InkCanvas inkCanvas2, Canvas graphCanvas,
            bool colorAnswerSteps = false)
        {
            List<AnalysisPenStroke> strokes1 = this.analyzer.LoadStrokesFromFile(filePath1);
            List<AnalysisPenStroke> strokes2 = this.analyzer.LoadStrokesFromFile(filePath2);

            inkCanvas1.Strokes.Clear();
            inkCanvas2.Strokes.Clear();
            graphCanvas.Children.Clear();

            List<AnswerStep> ansGroupList1 = this.analyzer.GroupAnswerStep(strokes1);
            List<AnswerStep> ansGroupList2 = this.analyzer.GroupAnswerStep(strokes2);

            if (colorAnswerSteps)
            {
                ColorAnswerGroupStrokes(ansGroupList1, inkCanvas1);
                ColorAnswerGroupStrokes(ansGroupList2, inkCanvas2);
            }
            else
            {
                ColorAnswerGroupStrokes(ansGroupList1, inkCanvas1, true);
                ColorAnswerGroupStrokes(ansGroupList2, inkCanvas2, true);
            }

            VisualizeAnswerGroupBoundingBox(ansGroupList1, graphCanvas);
            VisualizeAnswerGroupBoundingBox(ansGroupList2, graphCanvas, new Point(Config.OutputCanvasWidth, 0));

            DPMatchingResult matchingResult = this.analyzer.CalcAnswerProcessSimilarity(ansGroupList1, ansGroupList2);

            VisualizeMatchingResult(ansGroupList1, ansGroupList2, matchingResult, graphCanvas, new Point(Config.OutputCanvasWidth, 0));
        }

        #region PrivateMethods

        /// <summary>
        /// Draw and color strokes by each answer step
        /// </summary>
        /// <param name="ansGroupList">Answer step group list</param>
        /// <param name="inkCanvas">Drawing inkcanvas</param>
        /// <param name="black">Using black color only</param>
        private void ColorAnswerGroupStrokes(List<AnswerStep> ansGroupList, InkCanvas inkCanvas, bool black = false)
        {
            inkCanvas.Strokes.Clear();
            Random rand = new Random();

            foreach (AnswerStep group in ansGroupList)
            {
                Color strokeColor = Colors.Black;
                if (!black)
                {
                    strokeColor = Color.FromRgb((byte)rand.Next(256), (byte)rand.Next(256), (byte)rand.Next(256));
                }
                foreach (AnalysisPenStroke dstroke in group.Strokes)
                {
                    Stroke stroke = dstroke.GetStrokeObject(Config.OutputCanvasWidth, Config.OutputCanvasHeight);
                    stroke.DrawingAttributes.Color = strokeColor;
                    inkCanvas.Strokes.Add(stroke);
                }
            }
        }

        /// <summary>
        /// Visualize bounding box of answer step
        /// </summary>
        /// <param name="ansGroupList"></param>
        /// <param name="graphCanvas"></param>
        /// <param name="origin">Offset point of origin</param>
        private void VisualizeAnswerGroupBoundingBox(List<AnswerStep> ansGroupList, Canvas graphCanvas, Point origin)
        {
            foreach (AnswerStep group in ansGroupList)
            {
                Rect groupRect = group.GetBounds(Config.OutputCanvasWidth, Config.OutputCanvasHeight);

                Rectangle rect = new Rectangle();
                rect.Stroke = Brushes.Blue;
                rect.StrokeThickness = 2;
                rect.SetValue(Canvas.LeftProperty, groupRect.Left + origin.X);
                rect.SetValue(Canvas.TopProperty, groupRect.Top + origin.Y);
                rect.Width = groupRect.Width;
                rect.Height = groupRect.Height;
                graphCanvas.Children.Add(rect);

                // group ID
                ContentControl txtContent = new ContentControl();
                Canvas.SetLeft(txtContent, groupRect.Left - 25.0 + origin.X);
                Canvas.SetTop(txtContent, groupRect.Top + origin.Y);
                TextBlock tb = new TextBlock();
                tb.Text = group.GroupID.ToString("D2");
                tb.Foreground = rect.Stroke;
                tb.FontSize = 16;
                txtContent.Content = tb;
                graphCanvas.Children.Add(txtContent);
            }
        }
        /// <summary>
        /// Visualize bounding box of answer step
        /// </summary>
        /// <param name="ansGroupList"></param>
        /// <param name="graphCanvas"></param>
        private void VisualizeAnswerGroupBoundingBox(List<AnswerStep> ansGroupList, Canvas graphCanvas)
        {
            VisualizeAnswerGroupBoundingBox(ansGroupList, graphCanvas, new Point(0, 0));
        }

        /// <summary>
        /// Visualize answer process matching result
        /// </summary>
        /// <param name="ansGroupList1"></param>
        /// <param name="ansGroupList2"></param>
        /// <param name="matchingResult"></param>
        /// <param name="graphCanvas"></param>
        /// <param name="g2Origin">Offset point of origin</param>
        private void VisualizeMatchingResult(List<AnswerStep> ansGroupList1, List<AnswerStep> ansGroupList2, DPMatchingResult matchingResult, Canvas graphCanvas, Point g2Origin)
        {
            for (int i = 0, ilen = matchingResult.MatchingList.Count; i < ilen; i++)
            {
                List<int[]> matchings = matchingResult.MatchingList;
                if (matchings[i][0] != -1 && matchings[i][1] != -1)
                {
                    // draw joined group
                    Rect rect1 = ansGroupList1[matchings[i][0]].GetBounds(Config.OutputCanvasWidth, Config.OutputCanvasHeight);
                    Rect rect2 = ansGroupList2[matchings[i][1]].GetBounds(Config.OutputCanvasWidth, Config.OutputCanvasHeight);
                    if (matchings[i][3] > 0)
                    {
                        // join
                        List<AnswerStep> joinSteps = new List<AnswerStep>();
                        Rect jr;
                        List<int> joinedId = new List<int>();
                        double canvasLeftOffset = 0.0;
                        double canvasTopOffset = 0.0;
                        if (matchings[i - 1][1] == -1)
                        {
                            // group1 joined
                            for (int k = 0; k <= matchings[i][3]; k++)
                            {
                                joinSteps.Add(ansGroupList1[matchings[i][0] - k]);
                                joinedId.Add(matchings[i][0] - k);
                            }
                            jr = AnswerStep.GetJoinedBounds(joinSteps, Config.OutputCanvasWidth, Config.OutputCanvasHeight);
                            rect1 = jr;
                        }
                        else
                        {
                            // group2 joined
                            for (int k = 0; k <= matchings[i][3]; k++)
                            {
                                joinSteps.Add(ansGroupList2[matchings[i][1] - k]);
                                joinedId.Add(matchings[i][1] - k);
                            }
                            canvasLeftOffset = g2Origin.X;
                            canvasTopOffset = g2Origin.Y;
                            jr = AnswerStep.GetJoinedBounds(joinSteps, Config.OutputCanvasWidth, Config.OutputCanvasHeight);
                            rect2 = jr;
                        }
                        joinedId.Sort();

                        Rectangle drawRect = new Rectangle();
                        drawRect.Stroke = Brushes.Red;
                        drawRect.StrokeThickness = 3;
                        drawRect.SetValue(Canvas.LeftProperty, jr.Left + canvasLeftOffset);
                        drawRect.SetValue(Canvas.TopProperty, jr.Top + canvasTopOffset);
                        drawRect.Width = jr.Width;
                        drawRect.Height = jr.Height;
                        graphCanvas.Children.Add(drawRect);

                        // draw joined IDs
                        StringBuilder idtxt = new StringBuilder("Joined: ");
                        foreach (int id in joinedId)
                        {
                            idtxt.Append(id.ToString("D2") + ", ");
                        }
                        idtxt.Length -= 2;
                        ContentControl idtxtContent = new ContentControl();
                        Canvas.SetLeft(idtxtContent, jr.Left + canvasLeftOffset);
                        Canvas.SetTop(idtxtContent, jr.Bottom + canvasTopOffset);
                        TextBlock idtxtBlock = new TextBlock();
                        idtxtBlock.Text = idtxt.ToString();
                        idtxtBlock.Foreground = Brushes.Red;
                        idtxtBlock.FontSize = 16;
                        idtxtContent.Content = idtxtBlock;
                        graphCanvas.Children.Add(idtxtContent);
                    }

                    // draw matching connector
                    Line l = new Line();
                    l.Stroke = Brushes.Red;
                    l.StrokeThickness = 2;
                    l.X1 = (rect1.TopRight.X + rect1.BottomRight.X) / 2.0;
                    l.Y1 = (rect1.TopRight.Y + rect1.BottomRight.Y) / 2.0;
                    l.X2 = (rect2.TopLeft.X + rect2.BottomLeft.X) / 2.0 + g2Origin.X;
                    l.Y2 = (rect2.TopLeft.Y + rect2.BottomLeft.Y) / 2.0 + g2Origin.Y;
                    l.HorizontalAlignment = HorizontalAlignment.Left;
                    l.VerticalAlignment = VerticalAlignment.Center;
                    graphCanvas.Children.Add(l);

                    // draw distance score
                    ContentControl txtContent = new ContentControl();
                    Canvas.SetLeft(txtContent, (l.X1 + l.X2) / 2.0);
                    Canvas.SetTop(txtContent, (l.Y1 + l.Y2) / 2.0);
                    TextBlock tb = new TextBlock();
                    tb.Text = matchings[i][2].ToString();
                    tb.Foreground = Brushes.Red;
                    tb.FontSize = 16;
                    txtContent.Content = tb;
                    graphCanvas.Children.Add(txtContent);
                }
            }

            // similarity score
            ContentControl totalDistanceContent = new ContentControl();
            Canvas.SetLeft(totalDistanceContent, 0.0);
            Canvas.SetTop(totalDistanceContent, 0.0);
            TextBlock txtBlock = new TextBlock();
            txtBlock.Text = ((int)(matchingResult.Distance * 1000.0)).ToString();
            txtBlock.Foreground = Brushes.Red;
            txtBlock.FontSize = 18;
            totalDistanceContent.Content = txtBlock;
            graphCanvas.Children.Add(totalDistanceContent);
        }

        /// <summary>
        /// Draw answer step graph
        /// </summary>
        /// <param name="ansGroupList"></param>
        /// <param name="graphCanvas"></param>
        private void DrawAnswerGroupGraph(List<AnswerStep> ansGroupList, Canvas graphCanvas)
        {
            // draw edge
            for (int i = 1, ilen = ansGroupList.Count; i < ilen; i++)
            {
                Line line = new Line();
                //line.Stroke = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
                line.Stroke = new SolidColorBrush(Color.FromArgb(180, 230, 70, 70));
                line.X1 = ansGroupList[i - 1].CenterPoint.X + (ansGroupList[i - 1].NodeSize / 2.0);
                line.Y1 = ansGroupList[i - 1].CenterPoint.Y + (ansGroupList[i - 1].NodeSize / 2.0);
                line.X2 = ansGroupList[i].CenterPoint.X + (ansGroupList[i].NodeSize / 2.0);
                line.Y2 = ansGroupList[i].CenterPoint.Y + (ansGroupList[i].NodeSize / 2.0);
                line.StrokeThickness = 2;
                graphCanvas.Children.Add(line);
            }

            // draw node
            foreach (AnswerStep group in ansGroupList)
            {
                double nodeSize = group.NodeSize;

                Ellipse ansNode = new Ellipse();
                ansNode.Width = nodeSize;
                ansNode.Height = nodeSize;
                ansNode.SetValue(Canvas.LeftProperty, group.CenterPoint.X);
                ansNode.SetValue(Canvas.TopProperty, group.CenterPoint.Y);

                ansNode.Stroke = Brushes.Black;
                ansNode.StrokeThickness = 2;
                ansNode.Fill = new SolidColorBrush(Color.FromArgb(180, 230, 70, 70));
                ansNode.Fill.Opacity = 1.0;

                graphCanvas.Children.Add(ansNode);

                // draw node number text
                TextBlock tb = new TextBlock();
                tb.Text = group.GroupID.ToString();
                //tb.Background = Brushes.White;
                tb.FontSize = 20;
                tb.FontWeight = FontWeights.Bold;
                tb.Foreground = Brushes.Black;
                tb.SetValue(Canvas.LeftProperty, group.CenterPoint.X + (nodeSize / 2.0) - 10.0);
                tb.SetValue(Canvas.TopProperty, group.CenterPoint.Y + (nodeSize / 2.0) - 10.0);
                graphCanvas.Children.Add(tb);
            }
        }

        #endregion
    }
}
