using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Ink;
using System.Collections.ObjectModel;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// StrokeComparisonWindow.xaml
    /// </summary>
    public partial class StrokeComparisonWindow : Window
    {
        private ObservableCollection<StrokeComparisonItemData> StrokeComparisonDataList = new ObservableCollection<StrokeComparisonItemData>();
        private AnswerSheetAnalyzer analyzer;

        /// <summary>
        /// Initialization
        /// </summary>
        public StrokeComparisonWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialization with analyzer
        /// </summary>
        /// <param name="analysis"></param>
        public StrokeComparisonWindow(AnswerSheetAnalyzer a)
            : this()
        {
            this.analyzer = a;

            List<StrokeComparisonResult> results = this.analyzer.GetVisualizedStrokeComparisonExamples();
            foreach (StrokeComparisonResult res in results)
            {
                StrokeComparisonItemData item = new StrokeComparisonItemData();

                // draw stroke
                double scale = 0.0;
                Rect s1bb = res.Stroke1.BoundingBox;
                Rect s2bb = res.Stroke2.BoundingBox;
                double maxSize = (new double[] { s1bb.Width, s1bb.Height, s2bb.Width, s2bb.Height }).Max();
                scale = Config.OutputStrokeCanvasSize / maxSize;
                Point s1offset = new Point((Config.OutputStrokeCanvasSize / scale - s1bb.Width) / 2.0, (Config.OutputStrokeCanvasSize / scale - s1bb.Height) / 2.0);
                Point s2offset = new Point((Config.OutputStrokeCanvasSize / scale - s2bb.Width) / 2.0, (Config.OutputStrokeCanvasSize / scale - s2bb.Height) / 2.0);

                item.Strokes1.Add(res.Stroke1.GetStrokeObject(scale: scale, offset: s1offset, isOrigin: true));
                item.Strokes2.Add(res.Stroke2.GetStrokeObject(scale: scale, offset: s2offset, isOrigin: true));
                
                // draw sampled coordinates
                item.ResultCanvasCollection = new ObservableCollection<UIElement>();
                Stroke ss1 = res.SampledStroke1.GetStrokeObject(scale: scale, offset: s1offset, isOrigin: true);
                Stroke ss2 = res.SampledStroke2.GetStrokeObject(scale: scale, offset: s2offset, isOrigin: true);
                foreach (StylusPoint p in ss1.StylusPoints)
                {
                    Ellipse e = new Ellipse();
                    e.StrokeThickness = 0.0;
                    e.Fill = Brushes.Blue;
                    e.Width = 5.0;
                    e.Height = 5.0;
                    Canvas.SetLeft(e, p.X - (e.Width / 2.0));
                    Canvas.SetTop(e, p.Y - (e.Height / 2.0));
                    item.ResultCanvasCollection.Add(e);
                }
                foreach (StylusPoint p in ss2.StylusPoints)
                {
                    Ellipse e = new Ellipse();
                    e.StrokeThickness = 0.0;
                    e.Fill = Brushes.Blue;
                    e.Width = 5.0;
                    e.Height = 5.0;
                    Canvas.SetLeft(e, p.X - (e.Width / 2.0) + Config.OutputStrokeCanvasSize);
                    Canvas.SetTop(e, p.Y - (e.Height / 2.0));
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
                        l.X1 = ss1.StylusPoints[matchings[i][0]].X;
                        l.Y1 = ss1.StylusPoints[matchings[i][0]].Y;
                        l.X2 = ss2.StylusPoints[matchings[i][1]].X + Config.OutputStrokeCanvasSize;
                        l.Y2 = ss2.StylusPoints[matchings[i][1]].Y;
                        l.HorizontalAlignment = HorizontalAlignment.Left;
                        l.VerticalAlignment = VerticalAlignment.Center;
                        item.ResultCanvasCollection.Add(l);
                    }
                }

                // similarity score
                item.DistanceScore = res.Results.Distance;
                ContentControl totalDistanceContent = new ContentControl();
                Canvas.SetLeft(totalDistanceContent, 0.0);
                Canvas.SetTop(totalDistanceContent, 0.0);
                TextBlock txtBlock = new TextBlock();
                txtBlock.Text = res.Results.Distance.ToString("F2");
                txtBlock.Foreground = Brushes.Red;
                txtBlock.FontSize = 16;
                totalDistanceContent.Content = txtBlock;
                item.ResultCanvasCollection.Add(totalDistanceContent);


                this.StrokeComparisonDataList.Add(item);
            }

            // sort by score
            this.StrokeComparisonDataList = new ObservableCollection<StrokeComparisonItemData>(this.StrokeComparisonDataList.OrderBy(n => n.DistanceScore));

            this.StrokeComparisonsControl.ItemsSource = this.StrokeComparisonDataList;
        }
    }

    /// <summary>
    /// View class for visualizing stroke matching result
    /// </summary>
    public class StrokeComparisonItemData : ViewModelBase
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
        /// ItemsControl.ItemsSource property which insert diagrams about visualization
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
        public StrokeComparisonItemData()
        {
            this.Strokes1 = new StrokeCollection();
            this.strokes2 = new StrokeCollection();
        }
    }
}
