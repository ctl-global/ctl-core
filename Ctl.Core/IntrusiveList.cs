using System;
using System.Collections.Generic;
using System.Text;

namespace Ctl
{
    interface IIntrusiveList<T> where T : class, IIntrusiveNode<T>
    {
        T First { get; set; }
        T Last { get; set; }
    }

    interface IIntrusiveNode<T> where T : class, IIntrusiveNode<T>
    {
        T Prev { get; set; }
        T Next { get; set; }
        bool IsInList { get; set; }
    }

    static class IntrusiveList
    {
        public static bool IsEmpty<T>(IIntrusiveList<T> list) where T : class, IIntrusiveNode<T>
        {
            return list.First == null;
        }

        public static T Front<T>(IIntrusiveList<T> list) where T : class, IIntrusiveNode<T>
        {
            return list.First;
        }

        public static bool IsInList<T>(T node) where T : class, IIntrusiveNode<T>
        {
            return node.IsInList;
        }

        public static void AddToEnd<T>(IIntrusiveList<T> list, T node) where T : class, IIntrusiveNode<T>
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (node == null) throw new ArgumentNullException(nameof(node));

            node.Next = null;
            node.IsInList = true;

            if (list.Last == null)
            {
                node.Prev = null;

                list.First = node;
                list.Last = node;
            }
            else
            {
                node.Prev = list.Last;
                list.Last.Next = node;
                list.Last = node;
            }
        }

        public static bool Remove<T>(IIntrusiveList<T> list, T node) where T : class, IIntrusiveNode<T>
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (node == null) throw new ArgumentNullException(nameof(node));

            if (!node.IsInList)
            {
                return false;
            }

            if (node.Prev != null)
            {
                node.Prev.Next = node.Next;
            }
            else
            {
                list.First = node.Next;
            }

            if (node.Next != null)
            {
                node.Next.Prev = node.Prev;
            }
            else
            {
                list.Last = node.Prev;
            }

            node.Prev = null;
            node.Next = null;
            node.IsInList = false;

            return true;
        }
    }
}
