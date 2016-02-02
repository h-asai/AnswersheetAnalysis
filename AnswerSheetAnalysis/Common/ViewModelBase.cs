using System.ComponentModel;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Base class that notify the changes of property to UI
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Events of changing property
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Changed properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
