﻿using System;
using System.Collections.Generic;

namespace Advanced.Algorithms.DataStructures.Heap.Max
{
    //TODO implement IEnumerable & make sure duplicates are handled correctly if its not already
    public class FibornacciMaxHeap<T> where T : IComparable
    {
        internal FibornacciHeapNode<T> heapForestHead;

        //holds the maximum node at any given time
        private FibornacciHeapNode<T> maxNode = null;

        public int Count { get; private set; }

        /// <summary>
        /// O(1) complexity amortized
        /// </summary>
        /// <param name="newItem"></param>
        public FibornacciHeapNode<T> Insert(T newItem)
        {
            var newNode = new FibornacciHeapNode<T>(newItem);

            //return pointer to new Node
            MergeForests(newNode);

            if (maxNode == null)
            {
                maxNode = newNode;
            }
            else
            {
                if (maxNode.Value.CompareTo(newNode.Value) < 0)
                {
                    maxNode = newNode;
                }
            }

            Count++;

            return newNode;
        }

        /// <summary>
        /// Merge roots with same degrees in Forest 
        /// </summary>
        private void Meld()
        {

            if (heapForestHead == null)
            {
                maxNode = null;
                return;
            }

            //degree - node dictionary
            var mergeDictionary = new Dictionary<int, FibornacciHeapNode<T>>();

            var current = heapForestHead;
            maxNode = current;
            while (current != null)
            {
                current.Parent = null;
                var next = current.Next;
                //no same degree already in merge dictionary
                //add to hash table
                if (!mergeDictionary.ContainsKey(current.Degree))
                {
                  

                    mergeDictionary.Add(current.Degree, current);

                    if (maxNode == current)
                    {
                        maxNode = null;
                    }

                    DeleteNode(ref heapForestHead, current);

                    current = next;
                    continue;
                }
                //insert back to forest by merging current tree 
                //with existing tree in merge dictionary
                else
                {
                    var currentDegree = current.Degree;
                    var existing = mergeDictionary[currentDegree];

                    if (existing.Value.CompareTo(current.Value) > 0)
                    {
                        current.Parent = existing;

                        DeleteNode(ref heapForestHead, current);

                        var childHead = existing.ChildrenHead;
                        InsertNode(ref childHead, current);
                        existing.ChildrenHead = childHead;

                        existing.Degree++;

                        InsertNode(ref heapForestHead, existing);
                        current = existing;
                        current.Next = next;

                    }
                    else
                    {
                        existing.Parent = current;

                        var childHead = current.ChildrenHead;
                        InsertNode(ref childHead, existing);
                        current.ChildrenHead = childHead;

                        current.Degree++;
                    }


                    if (maxNode == null
                        || maxNode.Value.CompareTo(current.Value) < 0)
                    {
                        maxNode = current;
                    }

                    mergeDictionary.Remove(currentDegree);

                }

            }

            //insert back trees with unique degrees to forest
            if (mergeDictionary.Count > 0)
            {
                foreach (var node in mergeDictionary)
                {
                    InsertNode(ref heapForestHead, node.Value);

                    if (maxNode == null
                        || maxNode.Value.CompareTo(node.Value.Value) < 0)
                    {
                        maxNode = node.Value;
                    }
                }

                mergeDictionary.Clear();
            }

        }


        /// <summary>
        /// O(log(n)) complexity
        /// </summary>
        /// <returns></returns>
        public T ExtractMax()
        {
            if (heapForestHead == null)
                throw new Exception("Empty heap");

            var maxValue = maxNode.Value;

            //remove tree root
            DeleteNode(ref heapForestHead, maxNode);

            MergeForests(maxNode.ChildrenHead);
            Meld();

            Count--;

            return maxValue;
        }


        /// <summary>
        /// Update the Heap with new value for this node pointer
        /// O(1) complexity amortized
        /// </summary>
        /// <param name="key"></param>
        public void IncrementKey(FibornacciHeapNode<T> node)
        {

            if (node.Parent == null
                && maxNode.Value.CompareTo(node.Value) < 0)
            {
                maxNode = node;
            }

            var current = node;

            if (current.Parent != null
                && current.Value.CompareTo(current.Parent.Value) > 0)
            {

                var parent = current.Parent;

                //if parent already lost one child
                //then cut current and parent
                if (parent.LostChild)
                {
                    parent.LostChild = false;

                    var grandParent = parent.Parent;

                    //mark grand parent
                    if (grandParent != null)
                    {
                        Cut(parent);
                        Cut(current);
                    }
                }
                else
                {
                    Cut(current);
                }
            }

        }
        /// <summary>
        /// Delete this node from Heap Tree and adds it to forest as a new tree 
        /// </summary>
        /// <param name="node"></param>
        private void Cut(FibornacciHeapNode<T> node)
        {
            var parent = node.Parent;

            //cut child and attach to heap Forest
            //and mark parent for lost child
            var childHead = node.Parent.ChildrenHead;
            DeleteNode(ref childHead, node);
            node.Parent.ChildrenHead = childHead;

            node.Parent.Degree--;
            if (parent.Parent != null)
            {
                parent.LostChild = true;
            }
            node.LostChild = false;
            node.Parent = null;

            InsertNode(ref heapForestHead, node);

            //update max
            if (maxNode.Value.CompareTo(node.Value) < 0)
            {
                maxNode = node;
            }

        }

        /// <summary>
        /// Unions this heap with another
        /// O(k) complexity where K is the FibornacciHeap Forest Length 
        /// </summary>
        /// <param name="FibornacciHeap"></param>
        public void Union(FibornacciMaxHeap<T> FibornacciHeap)
        {
            MergeForests(FibornacciHeap.heapForestHead);
            Count = Count + FibornacciHeap.Count;
        }

        /// <summary>
        /// Merges the given fibornacci node list to current Forest 
        /// </summary>
        /// <param name="headPointer"></param>
        private void MergeForests(FibornacciHeapNode<T> headPointer)
        {
            var current = headPointer;
            while (current != null)
            {
                var next = current.Next;
                InsertNode(ref heapForestHead, current);
                current = next;
            }

        }

        private void InsertNode(ref FibornacciHeapNode<T> head, FibornacciHeapNode<T> newNode)
        {
            newNode.Next = newNode.Previous = null;

            if (head == null)
            {
                head = newNode;
                return;
            }

            head.Previous = newNode;
            newNode.Next = head;

            head = newNode;
        }

        private void DeleteNode(ref FibornacciHeapNode<T> heapForestHead, FibornacciHeapNode<T> deletionNode)
        {
            if (deletionNode == heapForestHead)
            {
                if (deletionNode.Next != null)
                {
                    deletionNode.Next.Previous = null;
                }

                heapForestHead = deletionNode.Next;
                deletionNode.Next = null;
                deletionNode.Previous = null;
                return;
            }

            deletionNode.Previous.Next = deletionNode.Next;

            if (deletionNode.Next != null)
            {
                deletionNode.Next.Previous = deletionNode.Previous;
            }

            deletionNode.Next = null;
            deletionNode.Previous = null;
        }

        /// <summary>
        ///  O(1) complexity 
        /// <returns></returns>
        public T PeekMax()
        {
            if (heapForestHead == null)
                throw new Exception("Empty heap");

            return maxNode.Value;
        }
    }


}
