/*
    Copyright (c) 2014, CTL Global, Inc.
    Copyright (c) 2012, iD Commerce + Logistics
    All rights reserved.

    Redistribution and use in source and binary forms, with or without modification, are permitted
    provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions
    and the following disclaimer. Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in the documentation and/or other
    materials provided with the distribution.
 
    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
    IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
    FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
    CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
    CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
    SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
    THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
    OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
    POSSIBILITY OF SUCH DAMAGE.
*/

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
