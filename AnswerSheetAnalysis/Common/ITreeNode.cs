using System.Collections.Generic;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Interface that represents tree structure
    /// </summary>
    public interface ITreeNode<T>
    {
        /// <summary>
        /// Parent node
        /// </summary>
        T Parent { get; set; }
        /// <summary>
        /// Set of child nodes
        /// </summary>
        IList<T> Children { get; set; }
        /// <summary>
        /// Have children or not
        /// </summary>
        bool HaveChildren { get; }
        /// <summary>
        /// Have parent or not
        /// </summary>
        bool HaveParent { get; }

        /// <summary>
        /// Add child node
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        T AddChild(T child);

        /// <summary>
        /// Get tree height
        /// </summary>
        /// <returns></returns>
        int GetHeight();
    }
}
