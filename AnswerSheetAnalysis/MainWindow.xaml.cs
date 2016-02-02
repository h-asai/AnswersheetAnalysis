using System;
using System.Windows;
using System.IO;
using System.Windows.Forms;
using CommonDialog = Microsoft.Win32.CommonDialog;
using FolderBrowserDialogForms = System.Windows.Forms.FolderBrowserDialog;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Answer sheet classification demo
    /// </summary>
    public partial class MainWindow : Window
    {
        private AnswerSheetAnalyzer analyzer = null;
        private AnswerSheetVisualizer visualizer = null;

        public MainWindow()
        {
            InitializeComponent();

            this.analyzer = new AnswerSheetAnalyzer();
            this.visualizer = new AnswerSheetVisualizer(analyzer);
        }

        private void Window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            // Show analysis result when saved files are dropped
            string[] paths = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];

            if (paths.Length == 1)
            {
                // visualize single answer sheet
                string filepath = paths[0];
                if (System.IO.Path.GetExtension(filepath) == ".json")
                {
                    // Visualize answer sheet when 1 json file is dropped
                    PreviewWindow previewWindow = new PreviewWindow();
                    previewWindow.Show();
                    this.visualizer.VisualizeAnswerSheet(filepath, previewWindow.PreviewCanvas, previewWindow.StepGraphCanvas, true, false, true);
                }
                else if (Directory.Exists(filepath))
                {
                    // Group answer sheets when directory is dropped.
                    this.analyzer.GroupAnswerSheet(filepath);
                    AnswerGroupWindow groupWindow = new AnswerGroupWindow(this.analyzer, this.visualizer);
                    groupWindow.Show();
                }
            }
            else if (paths.Length == 2)
            {
                string filepath1 = paths[0];
                string filepath2 = paths[1];
                if (System.IO.Path.GetExtension(filepath1) == ".json"
                    && System.IO.Path.GetExtension(filepath2) == ".json")
                {
                    // visualize answer sheets comparison when 2 json files are dropped
                    ComparisonWindow comparisonWindow = new ComparisonWindow();
                    comparisonWindow.Show();
                    this.visualizer.VisualizeAnswerSheetComparison(filepath1, filepath2, comparisonWindow.PreviewCanvas1, comparisonWindow.PreviewCanvas2, comparisonWindow.GraphCanvas);
                }
            }
        }

        private void BtnOpenAnswerSheet_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.FileName = "";
            ofd.DefaultExt = "*.json";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == true)
            {
                PreviewWindow w = new PreviewWindow();
                w.Show();
                this.visualizer.VisualizeAnswerSheet(ofd.FileName, w.PreviewCanvas, w.StepGraphCanvas, true, false, true);
            }
        }

        private void BtnCompareAnswerSheets_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.FileName = "";
            ofd.DefaultExt = "*.json";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                if (ofd.FileNames.Length == 2)
                {
                    ComparisonWindow w = new ComparisonWindow();
                    w.Show();
                    this.visualizer.VisualizeAnswerSheetComparison(ofd.FileNames[0], ofd.FileNames[1], w.PreviewCanvas1, w.PreviewCanvas2, w.GraphCanvas);
                }
            }
        }

        private void BtnGroupAnswerSheets_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select directory which have answer sheet data (*.json)";
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            if (fbd.ShowDialog(this) == true)
            {
                this.analyzer.GroupAnswerSheet(fbd.SelectedPath);
                AnswerGroupWindow w = new AnswerGroupWindow(this.analyzer, this.visualizer);
                w.Show();
            }
        }
    }

    public class FolderBrowserDialog : CommonDialog
    {
        public string Description { get; set; }
        public Environment.SpecialFolder RootFolder { get; set; }
        public string SelectedPath { get; set; }
        public bool ShowNewFolderButton { get; set; }

        public FolderBrowserDialog()
        {
            Reset();
        }

        public override void Reset()
        {
            Description = String.Empty;
            RootFolder = Environment.SpecialFolder.Desktop;
            SelectedPath = String.Empty;
            ShowNewFolderButton = true;
        }

        private class Win32Window : IWin32Window
        {
            private IntPtr _handle;

            public Win32Window(IntPtr handle)
            {
                _handle = handle;
            }

            public IntPtr Handle
            {
                get
                {
                    return _handle;
                }
            }
        }

        protected override bool RunDialog(IntPtr hwndOwner)
        {
            using (var fbd = new FolderBrowserDialogForms())
            {
                fbd.Description = Description;
                fbd.RootFolder = RootFolder;
                fbd.SelectedPath = SelectedPath;
                fbd.ShowNewFolderButton = ShowNewFolderButton;

                if (fbd.ShowDialog(new Win32Window(hwndOwner)) != DialogResult.OK)
                {
                    return false;
                }

                SelectedPath = fbd.SelectedPath;
                return true;
            }
        }
    }
}
