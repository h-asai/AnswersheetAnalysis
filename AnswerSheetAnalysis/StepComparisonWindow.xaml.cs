using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Ink;
using System.Collections.ObjectModel;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// StepComparisonWindow.xam
    /// </summary>
    public partial class StepComparisonWindow : Window
    {
        private ObservableCollection<StepComparisonitemData> StepComparisonDataList = new ObservableCollection<StepComparisonitemData>();
        private AnswerSheetAnalyzer analyzer;

        /// <summary>
        /// Initialization
        /// </summary>
        public StepComparisonWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialization with analyer
        /// </summary>
        /// <param name="analysis"></param>
        public StepComparisonWindow(AnswerSheetAnalyzer a)
            : this()
        {
            this.analyzer = a;

            List<StepComparisonResult> results = this.analyzer.GetVisualizedStepComparisonExamples();
            foreach (StepComparisonResult res in results)
            {
                StepComparisonitemData item = new StepComparisonitemData();

                // draw stroke
                double scale1 = Config.OutputStepCanvasHeight / res.Step1.GetBounds().Height;
                double scale2 = Config.OutputStepCanvasHeight / res.Step2.GetBounds().Height;
                List<Stroke> strokes1 = res.Step1.GetStrokeObjects(scale: scale1, isOrigin: true, sort: true);
                List<Stroke> strokes2 = res.Step2.GetStrokeObjects(scale: scale2, isOrigin: true, sort: true);
                item.Strokes1.Add(new StrokeCollection(strokes1));
                item.Strokes2.Add(new StrokeCollection(strokes2));

                // draw coordinates
                foreach (Stroke s in strokes1)
                {
                    Ellipse e = new Ellipse();
                    e.StrokeThickness = 0.0;
                    e.Fill = Brushes.Blue;
                    e.Width = 5.0;
                    e.Height = 5.0;
                    Canvas.SetLeft(e, s.StylusPoints[0].X - (e.Width / 2.0));
                    Canvas.SetTop(e, s.StylusPoints[0].Y - (e.Height / 2.0));
                    item.ResultCanvasCollection.Add(e);
                }
                foreach (Stroke s in strokes2)
                {
                    Ellipse e = new Ellipse();
                    e.StrokeThickness = 0.0;
                    e.Fill = Brushes.Blue;
                    e.Width = 5.0;
                    e.Height = 5.0;
                    Canvas.SetLeft(e, s.StylusPoints[0].X - (e.Width / 2.0));
                    Canvas.SetTop(e, s.StylusPoints[0].Y + Config.OutputStepCanvasHeight - (e.Height / 2.0));
                    item.ResultCanvasCollection.Add(e);
                }

                // visualize matching results
                for (int i = 0, ilen = res.Results.MatchingList.Count; i < ilen; i++)
                {
                    List<int[]> matchings = res.Results.MatchingList;
                    if (matchings[i][0] != -1 && matchings[i][1] != -1)
                    {
                        // draw matching connector
                        Line l = new Line();
                        l.Stroke = Brushes.Red;
                        l.StrokeThickness = 1;
                        l.X1 = strokes1[matchings[i][0]].StylusPoints[0].X;
                        l.Y1 = strokes1[matchings[i][0]].StylusPoints[0].Y;
                        l.X2 = strokes2[matchings[i][1]].StylusPoints[0].X;
                        l.Y2 = strokes2[matchings[i][1]].StylusPoints[0].Y + Config.OutputStepCanvasHeight;
                        l.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        l.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        item.ResultCanvasCollection.Add(l);
                    }
                }

                // similarity score
                item.DistanceScore = res.Results.Distance;
                ContentControl totalDistanceContent = new ContentControl();
                Canvas.SetLeft(totalDistanceContent, 800.0);
                Canvas.SetTop(totalDistanceContent, 0.0);
                TextBlock txtBlock = new TextBlock();
                txtBlock.Text = res.Results.Distance.ToString("F2");
                txtBlock.Foreground = Brushes.Red;
                txtBlock.FontSize = 18;
                totalDistanceContent.Content = txtBlock;
                item.ResultCanvasCollection.Add(totalDistanceContent);
                
                this.StepComparisonDataList.Add(item);
            }

            // sort by score
            this.StepComparisonDataList = new ObservableCollection<StepComparisonitemData>(this.StepComparisonDataList.OrderBy(n => n.DistanceScore));

            this.StepComparisonControl.ItemsSource = this.StepComparisonDataList;
        }
    }

    /// <summary>
    /// View class for visualizing answer step matching result
    /// </summary>
    public class StepComparisonitemData : ViewModelBase
    {
        private StrokeCollection strokes1;
        /// <summary>
        /// Stroke data 1
        /// </summary>
        public StrokeCollection Strokes1
        {
            get
            {
                return this.strokes1;
            }
            set
            {
                this.strokes1 = value;
                OnPropertyChanged(this, "Strokes1");
            }
        }

        private StrokeCollection strokes2;
        /// <summary>
        /// Stroke data 2
        /// </summary>
        public StrokeCollection Strokes2
        {
            get
            {
                return this.strokes2;
            }
            set
            {
                this.strokes2 = value;
                OnPropertyChanged(this, "Strokes2");
            }
        }

        private ObservableCollection<UIElement> resultCanvasCollection;
        /// <summary>
        /// ItemsControl.ItemsSource property which insert diagram about visualization
        /// </summary>
        public ObservableCollection<UIElement> ResultCanvasCollection
        {
            get
            {
                return this.resultCanvasCollection;
            }
            set
            {
                this.resultCanvasCollection = value;
                OnPropertyChanged(this, "ResultCanvasCollection");
            }
        }

        /// <summary>
        /// Distance score
        /// </summary>
        public double DistanceScore { get; set; }

        /// <summary>
        /// Initialization
        /// </summary>
        public StepComparisonitemData()
        {
            this.strokes1 = new StrokeCollection();
            this.strokes2 = new StrokeCollection();
            this.resultCanvasCollection = new ObservableCollection<UIElement>();
        }
    }
}
