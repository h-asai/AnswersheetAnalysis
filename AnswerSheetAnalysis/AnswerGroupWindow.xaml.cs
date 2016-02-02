using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Ink;
using System.Collections.ObjectModel;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// AnswerGroupWindow.xaml
    /// </summary>
    public partial class AnswerGroupWindow : Window
    {
        /// <summary>
        /// Maximum threshold when heatmap color of answer time is calculated. For example, if we set the value to 20, then heatmap color will change between deviation value 30 to 70.
        /// </summary>
        private const double TIME_HEATMAP_STD_THRES = 15;

        private ObservableCollection<AnswerGroupItemData> answerGroupDataList = new ObservableCollection<AnswerGroupItemData>();
        private AnswerSheetAnalyzer.ClassificationFeature currentMethod;
        private AnswerSheetAnalyzer analyzer;
        private AnswerSheetVisualizer visualizer;

        private AnswerSheetItemData selectedSheet = null;
        /// <summary>
        /// Is there any selected answersheet
        /// </summary>
        public bool AnswerSheetSelected
        {
            get
            {
                if (this.selectedSheet == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Initialization
        /// </summary>
        public AnswerGroupWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialization with the result
        /// </summary>
        /// <param name="analysis"></param>
        /// <param name="c"></param>
        public AnswerGroupWindow(AnswerSheetAnalyzer a, AnswerSheetVisualizer v)
            : this()
        {
            this.analyzer = a;
            this.visualizer = v;
            this.currentMethod = AnswerSheetAnalyzer.ClassificationFeature.Proposed;
            ChangeMethod(this.currentMethod);
        }

        private void cbDepth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Change depth if the depth value is selected.
            ComboBox cb = (ComboBox)sender;
            ChangeDepth(cb.SelectedIndex, this.currentMethod);
        }

        /// <summary>
        /// Change tree depth
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="method"></param>
        private void ChangeDepth(int depth, AnswerSheetAnalyzer.ClassificationFeature method)
        {
            this.answerGroupDataList.Clear();
            List<AnswerSheetGroup> ansGroupList = null;
            switch (method)
            {
                case AnswerSheetAnalyzer.ClassificationFeature.Proposed:
                    ansGroupList = this.analyzer.HierarchicalResult.GetGroupedAnswerSheets(depth);
                    break;
                case AnswerSheetAnalyzer.ClassificationFeature.AnswerTime:
                    ansGroupList = this.analyzer.HierarchicalResultAnswerTime.GetGroupedAnswerSheets(depth);
                    break;
            }
            foreach (AnswerSheetGroup g in ansGroupList)
            {
                this.answerGroupDataList.Add(new AnswerGroupItemData(g, this.visualizer, this));
            }

            // sort group by average answer time
            this.answerGroupDataList = new ObservableCollection<AnswerGroupItemData>(this.answerGroupDataList.OrderBy(n => n.AnswerGroupData.GetAverageAnswerTime()));

            this.GroupedAnswerSheetControl.ItemsSource = this.answerGroupDataList;
            this.cbDepth.SelectedIndex = depth;

            // show answer time heatmap
            SetAnswerTimeHeatmap();
        }

        /// <summary>
        /// Change clustering method
        /// </summary>
        /// <param name="method"></param>
        private void ChangeMethod(AnswerSheetAnalyzer.ClassificationFeature method)
        {
            if (this.cbDepth == null)
            {
                return;
            }

            this.cbDepth.Items.Clear();
            for (int i = 0, ilen = this.analyzer.GetClusterTreeHeight(method); i <= ilen; i++)
            {
                this.cbDepth.Items.Add(i.ToString());
            }

            switch (method)
            {
                case AnswerSheetAnalyzer.ClassificationFeature.Proposed:
                    ChangeDepth(this.analyzer.HierarchicalResult.GetOptimalTreeDepth(), this.currentMethod);
                    break;
                case AnswerSheetAnalyzer.ClassificationFeature.AnswerTime:
                    ChangeDepth(this.analyzer.HierarchicalResultAnswerTime.GetOptimalTreeDepth(), this.currentMethod);
                    break;
            }
        }

        /// <summary>
        /// Set heatmap of answer time
        /// </summary>
        private void SetAnswerTimeHeatmap()
        {
            int ansNum = 0;
            // average
            double average = 0.0;
            foreach (AnswerGroupItemData group in this.answerGroupDataList)
            {
                foreach (AnswerSheetItemData sheet in group.AnswerSheetData)
                {
                    if (sheet.AnswerData.AnswerTime != 0)
                    {
                        average += 1.0 / Math.Log(sheet.AnswerData.AnswerTime);
                        ansNum++;
                    }
                }
            }
            average /= (double)ansNum;

            // standard deviation
            double stdDev = 0.0;
            foreach (AnswerGroupItemData group in this.answerGroupDataList)
            {
                foreach (AnswerSheetItemData sheet in group.AnswerSheetData)
                {
                    if (sheet.AnswerData.AnswerTime != 0)
                    {
                        stdDev += Math.Pow(1.0 / Math.Log(sheet.AnswerData.AnswerTime) - average, 2);
                    }
                }
            }
            stdDev = Math.Sqrt(stdDev / ansNum);

            // set heatmap color
            double minVal = average - TIME_HEATMAP_STD_THRES * stdDev / 10.0;
            double maxVal = average + TIME_HEATMAP_STD_THRES * stdDev / 10.0;
            foreach (AnswerGroupItemData group in this.answerGroupDataList)
            {
                foreach (AnswerSheetItemData sheet in group.AnswerSheetData)
                {
                    sheet.BorderBrush = new SolidColorBrush(CommonFunction.GetHeatmapColor(1.0 / Math.Log(sheet.AnswerData.AnswerTime), minVal, maxVal, 100));
                }
            }
        }

        /// <summary>
        /// Select answer sheet
        /// </summary>
        /// <param name="sheet"></param>
        public void SelectAnswerSheet(AnswerSheetItemData sheet)
        {
            if (this.selectedSheet != null)
            {
                UnselectAnswerSheet();
            }

            this.selectedSheet = sheet;
        }

        /// <summary>
        /// Unselect answer sheet
        /// </summary>
        /// <returns></returns>
        public AnswerSheetItemData UnselectAnswerSheet()
        {
            AnswerSheetItemData selected = this.selectedSheet;

            this.selectedSheet = null;

            foreach (AnswerGroupItemData g in this.answerGroupDataList)
            {
                foreach (AnswerSheetItemData sheet in g.AnswerSheetData)
                {
                    sheet.SelectCanvasVisibility = System.Windows.Visibility.Collapsed;
                }
            }

            return selected;
        }

        private void rbMethodProposed_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            switch (radio.Name)
            {
                case "rbMethodProposed":
                    this.currentMethod = AnswerSheetAnalyzer.ClassificationFeature.Proposed;
                    break;
                case "rbMethodTime":
                    this.currentMethod = AnswerSheetAnalyzer.ClassificationFeature.AnswerTime;
                    break;
            }
            ChangeMethod(this.currentMethod);
        }

        private void Btn_StrokeComparison_Click(object sender, RoutedEventArgs e)
        {
            StrokeComparisonWindow strokeWindow = new StrokeComparisonWindow(this.analyzer);
            strokeWindow.Show();
        }

        private void Btn_StepComparison_Click(object sender, RoutedEventArgs e)
        {
            StepComparisonWindow stepComparison = new StepComparisonWindow(this.analyzer);
            stepComparison.Show();
        }
    }

    /// <summary>
    /// View class for controlling answer group
    /// </summary>
    public class AnswerGroupItemData : ViewModelBase
    {
        #region Properties

        private AnswerSheetGroup answerGroupData;
        /// <summary>
        /// Answer group information
        /// </summary>
        public AnswerSheetGroup AnswerGroupData
        {
            get
            {
                return this.answerGroupData;
            }
        }

        private ObservableCollection<AnswerSheetItemData> answerSheetDataList;
        /// <summary>
        /// Answer sheets that belong to the group
        /// </summary>
        public ObservableCollection<AnswerSheetItemData> AnswerSheetData
        {
            get
            {
                return this.answerSheetDataList;
            }
            set
            {
                this.answerSheetDataList = value;
                OnPropertyChanged(this, "AnswerSheetItems");
            }
        }

        private string groupNameLabel;
        /// <summary>
        /// Gorup name
        /// </summary>
        public string GroupNameLabel
        {
            get
            {
                return this.groupNameLabel;
            }
            set
            {
                this.groupNameLabel = value;
                OnPropertyChanged(this, "GroupNameLabel");
            }
        }

        private string timeLabel;
        /// <summary>
        /// Average answer time
        /// </summary>
        public string TimeLabel
        {
            get
            {
                return this.timeLabel;
            }
            set
            {
                this.timeLabel = value;
                OnPropertyChanged(this, "TimeLabel");
            }
        }

        #endregion

        /// <summary>
        /// Initialization with group information
        /// </summary>
        /// <param name="group"></param>
        /// <param name="controller"></param>
        /// <param name="w"></param>
        public AnswerGroupItemData(AnswerSheetGroup group, AnswerSheetVisualizer visualizer, AnswerGroupWindow w)
        {
            this.answerGroupData = group;
            this.groupNameLabel = group.Name;
            this.timeLabel = "Average Time: " + (group.GetAverageAnswerTime() / 1000.0).ToString("f3") + "(sec)";

            this.AnswerSheetData = new ObservableCollection<AnswerSheetItemData>();
            foreach (AnswerSheet ans in group.AnswerSheetList)
            {
                this.AnswerSheetData.Add(new AnswerSheetItemData(ans, visualizer, w));
            }

            // sort item by answer time
            this.AnswerSheetData = new ObservableCollection<AnswerSheetItemData>(this.AnswerSheetData.OrderBy(n => n.AnswerData.AnswerTime));
        }
    }

    /// <summary>
    /// View class for visualizing answer sheets
    /// </summary>
    public class AnswerSheetItemData : ViewModelBase
    {
        /// <summary>
        /// Maximum value of writing time indicator
        /// </summary>
        private const double MaxWritingTime = 204.0;

        private AnswerSheetVisualizer visualizer = null;

        #region Properties

        RelayCommand openAnswerSheetCommand;
        /// <summary>
        /// ICommand for preview answer sheet
        /// </summary>
        public ICommand OpenAnswerSheetCommand
        {
            get
            {
                if (this.openAnswerSheetCommand == null)
                {
                    this.openAnswerSheetCommand = new RelayCommand(param => this.OpenAnswerSheet(), param => true);
                }
                return openAnswerSheetCommand;
            }
        }

        RelayCommand compareAnswerSheetCommand;
        /// <summary>
        /// ICommand for comparing answer sheets
        /// </summary>
        public ICommand CompareAnswerSheetCommand
        {
            get
            {
                if (this.compareAnswerSheetCommand == null)
                {
                    this.compareAnswerSheetCommand = new RelayCommand(param => this.CompareAnswerSheet(), param => true);
                }
                return compareAnswerSheetCommand;
            }
        }

        private AnswerGroupWindow parentWindow;

        private AnswerSheet answerData;
        /// <summary>
        /// Answer sheet data
        /// </summary>
        public AnswerSheet AnswerData
        {
            get
            {
                return this.answerData;
            }
        }

        private StrokeCollection strokes;
        /// <summary>
        /// Strokes data
        /// </summary>
        public StrokeCollection Strokes
        {
            get
            {
                return this.strokes;
            }
            set
            {
                this.strokes = value;
                OnPropertyChanged(this, "Strokes");
            }
        }

        private string nameLabel;
        /// <summary>
        /// Name of answer sheet
        /// </summary>
        public string NameLabel
        {
            get
            {
                return this.nameLabel;
            }
            set
            {
                this.nameLabel = value;
                OnPropertyChanged(this, "NameLabel");
            }
        }

        private string timeLabel;
        /// <summary>
        /// Answering time
        /// </summary>
        public string TimeLabel
        {
            get
            {
                return this.timeLabel;
            }
            set
            {
                this.timeLabel = value;
                OnPropertyChanged(this, "TimeLabel");
            }
        }

        private Brush borderBrush;
        /// <summary>
        /// Border color
        /// </summary>
        public Brush BorderBrush
        {
            get
            {
                return this.borderBrush;
            }
            set
            {
                this.borderBrush = value;
                OnPropertyChanged(this, "BorderBrush");
            }
        }

        private Visibility selectCanvasVisibility = Visibility.Collapsed;
        /// <summary>
        /// Canvas Visibility of selecting indicator
        /// </summary>
        public Visibility SelectCanvasVisibility
        {
            get
            {
                return this.selectCanvasVisibility;
            }
            set
            {
                this.selectCanvasVisibility = value;
                OnPropertyChanged(this, "SelectCanvasVisibility");
            }
        }

        private GridLength writingTime;
        /// <summary>
        /// Writing time ratio indicator
        /// </summary>
        public GridLength WritingTime
        {
            get
            {
                return this.writingTime;
            }
            set
            {
                this.writingTime = value;
                OnPropertyChanged(this, "WritingTime");
            }
        }

        private string writingTimeLabel;
        /// <summary>
        /// Writing time ratio label
        /// </summary>
        public string WritingTimeLabel
        {
            get
            {
                return this.writingTimeLabel;
            }
            set
            {
                this.writingTimeLabel = value;
                OnPropertyChanged(this, "WritingTimeLabel");
            }
        }

        private GridLength writingSpeedAvg;
        /// <summary>
        /// Average writing time indicator
        /// </summary>
        public GridLength WritingSpeedAvg
        {
            get
            {
                return this.writingSpeedAvg;
            }
            set
            {
                this.writingSpeedAvg = value;
                OnPropertyChanged(this, "WritingSpeedAvg");
            }
        }

        private string writingSpeedAvgLabel;
        /// <summary>
        /// Average writing speed label
        /// </summary>
        public string WritingSpeedAvgLabel
        {
            get
            {
                return this.writingSpeedAvgLabel;
            }
            set
            {
                this.writingSpeedAvgLabel = value;
                OnPropertyChanged(this, "WritingSpeedAvgLabel");
            }
        }

        private GridLength writingSpeedVar;
        /// <summary>
        /// Variance of writing speed indicator
        /// </summary>
        public GridLength WritingSpeedVar
        {
            get
            {
                return this.writingSpeedVar;
            }
            set
            {
                this.writingSpeedVar = value;
                OnPropertyChanged(this, "WritingSpeedVar");
            }
        }

        private string writingSpeedVarLabel;
        /// <summary>
        /// Variance of writing speed label
        /// </summary>
        public string WritingSpeedVarLabel
        {
            get
            {
                return this.writingSpeedVarLabel;
            }
            set
            {
                this.writingSpeedVarLabel = value;
                OnPropertyChanged(this, "WritingSpeedVarLabel");
            }
        }

        #endregion

        /// <summary>
        /// Initialization with answer information
        /// </summary>
        /// <param name="ans"></param>
        /// <param name="controller"></param>
        /// <param name="w"></param>
        public AnswerSheetItemData(AnswerSheet ans, AnswerSheetVisualizer v, AnswerGroupWindow w)
        {
            this.parentWindow = w;
            this.answerData = ans;
            this.visualizer = v;
            this.strokes = new StrokeCollection();
            foreach (AnalysisPenStroke s in ans.Strokes)
            {
                this.strokes.Add(s.GetStrokeObject(Config.OutputThumbnailCanvasWidth, Config.OutputThumbnailCanvasHeight));
            }
            this.NameLabel = ans.Name;
            this.timeLabel = "Time: " + ((double)ans.AnswerTime / 1000.0).ToString("f3") + "(sec)";

            double writingRatio = ans.WritingRatio;
            this.writingTime = new GridLength(MaxWritingTime * writingRatio * 2.5);
            this.writingTimeLabel = (writingRatio * 100.0).ToString("F2") + "%";

            double speedAvg = ans.WritingSpeedAvg;
            this.writingSpeedAvg = new GridLength(MaxWritingTime * (speedAvg - 0.5));
            this.writingSpeedAvgLabel = speedAvg.ToString("F4");

            double speedVar = ans.WritingSpeedVar;
            this.writingSpeedVar = new GridLength(MaxWritingTime * speedVar);
            this.writingSpeedVarLabel = speedVar.ToString("F4");
        }

        /// <summary>
        /// Preview answer sheet
        /// </summary>
        public void OpenAnswerSheet()
        {
            this.parentWindow.UnselectAnswerSheet();
            PreviewWindow previewWindow = new PreviewWindow();
            previewWindow.Show();
            this.visualizer.VisualizeAnswerSheet(this.answerData.FilePath, previewWindow.PreviewCanvas, previewWindow.StepGraphCanvas, false);
        }

        /// <summary>
        /// Visualize answe sheet comparison
        /// </summary>
        public void CompareAnswerSheet()
        {
            //Console.WriteLine("CTRL+LeftClick");

            if (this.parentWindow.AnswerSheetSelected)
            {
                AnswerSheetItemData selectedSheet = this.parentWindow.UnselectAnswerSheet();
                ComparisonWindow comparisonWindow = new ComparisonWindow();
                comparisonWindow.Show();
                this.visualizer.VisualizeAnswerSheetComparison(selectedSheet.AnswerData.FilePath, this.AnswerData.FilePath, comparisonWindow.PreviewCanvas1, comparisonWindow.PreviewCanvas2, comparisonWindow.GraphCanvas);
            }
            else
            {
                this.SelectCanvasVisibility = Visibility.Visible;
                this.parentWindow.SelectAnswerSheet(this);
            }
        }
    }
}

