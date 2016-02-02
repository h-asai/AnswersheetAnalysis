using System.Collections.Generic;

namespace AnswerSheetAnalysis
{
    /// <summary>
    /// Generic class of tree structure
    /// </summary>
    public abstract class TreeNodeBase<T> : ITreeNode<TreeNodeBase<T>> where T : class
    {
        /// <summary>
        /// Parent node value
        /// </summary>
        protected TreeNodeBase<T> parent = null;
        /// <summary>
        /// Parent node
        /// </summary>
        public virtual TreeNodeBase<T> Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }

        /// <summary>
        /// Child nodes value
        /// </summary>
        protected IList<TreeNodeBase<T>> children = null;
        /// <summary>
        /// Child nodes
        /// </summary>
        public virtual IList<TreeNodeBase<T>> Children
        {
            get
            {
                if (children == null)
                {
                    children = new List<TreeNodeBase<T>>();
                }
                return children;
            }
            set
            {
                children = value;
            }
        }

        /// <summary>
        /// Have child node or not
        /// </summary>
        public virtual bool HaveChildren
        {
            get
            {
                if (children == null || children.Count == 0)
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
        /// Have parent node or not
        /// </summary>
        public virtual bool HaveParent
        {
            get
            {
                if (parent == null)
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
        /// Add child node
        /// </summary>
        /// <param name="child"></param>
        public virtual TreeNodeBase<T> AddChild(TreeNodeBase<T> child)
        {
            this.Children.Add(child);
            child.Parent = this;
            return this;
        }

        /// <summary>
        /// Get tree height
        /// </summary>
        /// <returns></returns>
        public virtual int GetHeight()
        {
            return GetHeightRecursive(this);
        }

        private int GetHeightRecursive(TreeNodeBase<T> node)
        {
            if (node.HaveChildren)
            {
                int height = 0;
                foreach (TreeNodeBase<T> n in node.Children)
                {
                    int h = GetHeightRecursive(n);
                    if (height < h)
                    {
                        height = h;
                    }
                }
                return height + 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
