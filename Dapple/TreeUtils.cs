using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace Dapple
{
   class TreeUtils
   {
      private TreeUtils()
      {
      }
      
      /// <summary>
      /// Determines whether parent/child relationship exist
      /// </summary>
      /// <param name="parentNode"></param>
      /// <param name="childNode"></param>
      /// <returns></returns>
      public static bool isParent(TreeNode parentNode, TreeNode childNode)
      {
         if (parentNode == childNode)
            return true;

         TreeNode n = childNode;
         bool bFound = false;
         while (!bFound && n != null)
         {
            n = n.Parent;
            bFound = (n == parentNode);
         }
         return bFound;
      }

      /// <summary>
      /// Breadth first search for first treenode tag of type
      /// </summary>
      /// <param name="type"></param>
      /// <param name="collection"></param>
      /// <returns></returns>
      public static TreeNode FindNodeOfTypeBFS(Type type, TreeNodeCollection collection)
      {
         System.Collections.Generic.Queue<TreeNode> queue = new Queue<TreeNode>();
         foreach (TreeNode treeNode in collection)
         {
            queue.Enqueue(treeNode);
         }

         while (queue.Count > 0)
         {
            TreeNode treeNode = queue.Dequeue();
            if (treeNode.Tag != null && treeNode.Tag.GetType() == type)
               return treeNode;
            else
            {
               foreach (TreeNode tNode in treeNode.Nodes)
               {
                  queue.Enqueue(tNode);
               }
            }
         }

         return null;
      }

      /// <summary>
      /// Breadth first search for treenode with the same tag reference
      /// </summary>
      /// <param name="tag"></param>
      /// <param name="collection"></param>
      /// <returns></returns>
      public static TreeNode FindNodeBFS(object tag, TreeNodeCollection collection)
      {
         System.Collections.Generic.Queue<TreeNode> queue = new Queue<TreeNode>();
         foreach (TreeNode treeNode in collection)
         {
            queue.Enqueue(treeNode);
         }

         while (queue.Count > 0)
         {
            TreeNode treeNode = queue.Dequeue();
            if (treeNode.Tag == tag)
               return treeNode;
            else
            {
               foreach (TreeNode tNode in treeNode.Nodes)
               {
                  queue.Enqueue(tNode);
               }
            }
         }

         return null;
      }

      /// <summary>
      /// Depth First search for a node by its tag
      /// </summary>
      /// <param name="col"></param>
      /// <param name="tag"></param>
      /// <returns></returns>
      public static TreeNode FindNodeDFS(TreeNodeCollection col, object tag)
      {
         foreach (TreeNode treeNode in col)
         {
            if (treeNode.Tag == tag)
            {
               return treeNode;
            }
            else
            {
               TreeNode temp = FindNodeDFS(treeNode.Nodes, tag);
               if (temp != null) return temp;
            }
         }
         return null;
      }
   }
}
