using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// Graphing algorithms.
    /// </summary>
    public static class Graph
    {
        /// <summary>
        /// Calculates a minimum path, using A*.
        /// </summary>
        /// <typeparam name="TState">The type of state to use.</typeparam>
        /// <param name="maxStates">The maximum amount of internal state to track. Higher numbers will trade path performance for path efficiency.</param>
        /// <param name="initialState">The starting state of the path.</param>
        /// <param name="getNextStates">Retrieves the possible next nodes in a path.</param>
        /// <param name="isComplete">A function returning true if a state represents a completed path.</param>
        /// <param name="completeComparer">A comparer sorting completed states, from most efficient to least.</param>
        /// <param name="partialComparer">A comparer sorting states by completion, then efficiency.</param>
        /// <param name="finalPath">An output parameter retrieving the final path.</param>
        /// <returns>If a completed path could be found, true. Otherwise, false.</returns>
        public static bool MinimumPath<TState>(int maxStates, TState initialState, Func<TState, IEnumerable<TState>> getNextStates, Func<TState, bool> isComplete, IComparer<TState> completeComparer, IComparer<TState> partialComparer, out TState finalPath)
        {
            SortedSet<TState> completedStates = new SortedSet<TState>(completeComparer);
            SortedSet<TState> partialStates = new SortedSet<TState>(partialComparer);
            HashSet<TState> openStates = new HashSet<TState>();

            partialStates.Add(initialState);
            openStates.Add(initialState);

            while (openStates.Count != 0)
            {
                TState s = openStates.First();

                openStates.Remove(s);
                partialStates.Remove(s);

                foreach (var nextState in getNextStates(s))
                {
                    if (partialStates.Add(nextState))
                    {
                        if (!isComplete(nextState))
                        {
                            openStates.Add(nextState);
                        }
                        else
                        {
                            completedStates.Add(nextState);
                        }
                    }
                }

                while (partialStates.Count > maxStates)
                {
                    TState r = partialStates.Max;

                    partialStates.Remove(r);
                    completedStates.Remove(r);
                    openStates.Remove(r);
                }
            }

            bool ret = completedStates.Count != 0;

            finalPath = ret ? completedStates.Min : default(TState);
            return ret;
        }
    }
}
