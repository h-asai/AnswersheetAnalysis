using System.Collections.Generic;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Answer sheet group information class
    /// </summary>
    public class AnswerSheetGroup
    {
        /// <summary>
        /// Group ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Answer sheet list that belongs to this group
        /// </summary>
        public List<AnswerSheet> AnswerSheetList { get; set; }

        /// <summary>
        /// Group name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initialization
        /// </summary>
        public AnswerSheetGroup(int id)
        {
            this.ID = id;
            this.Name = "Group " + (id + 1).ToString();
        }

        /// <summary>
        /// Calculate average answer time in this group
        /// </summary>
        /// <returns></returns>
        public double GetAverageAnswerTime()
        {
            double average = 0.0;
            foreach (AnswerSheet ans in this.AnswerSheetList)
            {
                average += ans.AnswerTime;
            }
            average /= this.AnswerSheetList.Count;

            return average;
        }
    }
}
