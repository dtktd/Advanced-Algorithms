﻿using Advanced.Algorithms.DataStructures.Graph.AdjacencyList;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advanced.Algorithms.GraphAlgorithms.Flow
{

    /// <summary>
    /// A Push-Relabel algorithm implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class PushRelabelMaxFlow<T, W> where W : IComparable
    {
        IFlowOperators<W> operators;
        public PushRelabelMaxFlow(IFlowOperators<W> operators)
        {
            this.operators = operators;
        }

        /// <summary>
        /// Computes Max Flow using Push-Relabel algorithm
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="source"></param>
        /// <param name="sink"></param>
        /// <returns></returns>
        public W ComputeMaxFlow(WeightedDiGraph<T, W> graph,
            T source, T sink)
        {
            //clone to create a residual graph
            var residualGraph = createResidualGraph(graph);

            //init vertex Height & Overflow object (ResidualGraphVertexStatus)
            var vertexStatusMap = new Dictionary<T, ResidualGraphVertexStatus>();
            foreach(var vertex in residualGraph.Vertices)
            {
                if (vertex.Value.Value.Equals(source))
                {
                    //for source vertex
                    //init source height to Maximum (equal to total vertex count)
                    vertexStatusMap.Add(vertex.Value.Value,
                      new ResidualGraphVertexStatus(residualGraph.Vertices.Count,
                      operators.defaultWeight));
                }
                else
                {              
                    vertexStatusMap.Add(vertex.Value.Value,
                      new ResidualGraphVertexStatus(0,
                      operators.defaultWeight));
                }

            }

            //init source neighbour overflow to capacity of source-neighbour edges
            foreach (var edge in residualGraph.Vertices[source].OutEdges.ToList())
            {
               //update edge vertex overflow
                vertexStatusMap[edge.Key.Value].Overflow = edge.Value;

                //increment reverse edge
                residualGraph.Vertices[edge.Key.Value]
                    .OutEdges[residualGraph.Vertices[source]] = edge.Value;

                //set to minimum
                residualGraph.Vertices[source].OutEdges[edge.Key] = operators.defaultWeight;

            }

            var overflowVertex = FindOverflowVertex(vertexStatusMap, source, sink);
            
            //until there is not more overflow vertices
            while (!overflowVertex.Equals(default(T)))
            {
                //if we can't push this vertex
                if (!Push(residualGraph.Vertices[overflowVertex], vertexStatusMap))
                {
                    //increase its height and try again
                    Relabel(residualGraph.Vertices[overflowVertex], vertexStatusMap);
                }

                overflowVertex = FindOverflowVertex(vertexStatusMap, source, sink);
            }

            //overflow of sink will be the net flow
            return vertexStatusMap[sink].Overflow;
        }

        /// <summary>
        /// Increases the height of a vertex by one greater than min height of neighbours
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="vertexStatusMap"></param>
        private void Relabel(WeightedDiGraphVertex<T, W> vertex, 
            Dictionary<T, ResidualGraphVertexStatus> vertexStatusMap)
        {
            var min = int.MaxValue;

            foreach(var edge in vertex.OutEdges)
            {
                //+ive out capacity  
                if(min.CompareTo(vertexStatusMap[edge.Key.Value].Height) > 0
                    && edge.Value.CompareTo(operators.defaultWeight) > 0)
                {
                    min = vertexStatusMap[edge.Key.Value].Height;
                   
                }
            }

            vertexStatusMap[vertex.Value].Height = min + 1;

        }

        /// <summary>
        /// Tries to Push the overflow in current vertex to neighbours if possible
        /// Push is possible if neighbour edge is not full
        /// and any of neighbour has height of current vertex
        /// otherwise returns false
        /// </summary>
        /// <param name="overflowVertex"></param>
        /// <param name="vertexStatusMap"></param>
        /// <returns></returns>
        private bool Push(WeightedDiGraphVertex<T, W> overflowVertex, 
            Dictionary<T, ResidualGraphVertexStatus> vertexStatusMap)
        {
            var overflow = vertexStatusMap[overflowVertex.Value].Overflow;

            foreach(var edge in overflowVertex.OutEdges)
            {
                //if out edge has +ive weight & neighbour height is less then flow is possible
                if(edge.Value.CompareTo(operators.defaultWeight) > 0
                    && vertexStatusMap[edge.Key.Value].Height 
                       < vertexStatusMap[overflowVertex.Value].Height)
                {
                    var possibleWeightToPush = operators.defaultWeight;

                    if(edge.Value.CompareTo(overflow) < 0)
                    {
                        possibleWeightToPush = edge.Value;
                    }
                    else
                    {
                        possibleWeightToPush = overflow;
                    }

                    //decrement overflow
                    vertexStatusMap[overflowVertex.Value].Overflow = 
                        operators.SubstractWeights(vertexStatusMap[overflowVertex.Value].Overflow,
                        possibleWeightToPush);

                    //increment flow of target vertex
                    vertexStatusMap[edge.Key.Value].Overflow =
                        operators.AddWeights(vertexStatusMap[edge.Key.Value].Overflow,
                         possibleWeightToPush);

                    //decrement edge weight
                    overflowVertex.OutEdges[edge.Key] = operators.SubstractWeights(edge.Value, possibleWeightToPush);
                    //increment reverse edge weight
                    edge.Key.OutEdges[overflowVertex] = operators.AddWeights(edge.Key.OutEdges[overflowVertex], possibleWeightToPush);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a vertex with an overflow
        /// </summary>
        /// <param name="vertexStatusMap"></param>
        /// <param name="source"></param>
        /// <param name="sink"></param>
        /// <returns></returns>
        private T FindOverflowVertex(Dictionary<T, ResidualGraphVertexStatus> vertexStatusMap,
            T source, T sink)
        {
            foreach(var vertexStatus in vertexStatusMap)
            {
                //ignore source and sink (which can have non-zero overflow)
                if(!vertexStatus.Key.Equals(source) && !vertexStatus.Key.Equals(sink) &&
                    vertexStatus.Value.Overflow.CompareTo(operators.defaultWeight) > 0)
                {
                    return vertexStatus.Key;
                }
            }

            return default(T);
        }

        /// <summary>
        /// Return all flow Paths
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="source"></param>
        /// <param name="sink"></param>
        /// <returns></returns>
        public List<List<T>> ComputeMaxFlowAndReturnFlowPath(WeightedDiGraph<T, W> graph,
            T source, T sink)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// clones this graph and creates a residual graph
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        private WeightedDiGraph<T, W> createResidualGraph(WeightedDiGraph<T, W> graph)
        {
            var newGraph = new WeightedDiGraph<T, W>();

            //clone graph vertices
            foreach (var vertex in graph.Vertices)
            {
                newGraph.AddVertex(vertex.Key);
            }

            //clone edges
            foreach (var vertex in graph.Vertices)
            {
                //Use either OutEdges or InEdges for cloning
                //here we use OutEdges
                foreach (var edge in vertex.Value.OutEdges)
                {
                    //original edge
                    newGraph.AddEdge(vertex.Key, edge.Key.Value, edge.Value);
                    //add a backward edge for residual graph with edge value as default(W)
                    newGraph.AddEdge(edge.Key.Value, vertex.Key, default(W));
                }
            }

            return newGraph;
        }

        /// <summary>
        /// An object to keep track of Vertex Overflow & Height
        /// </summary>
        internal class ResidualGraphVertexStatus
        {
            /// <summary>
            /// Current overflow in this vertex
            /// </summary>
            public W Overflow { get; set; }

            /// <summary>
            /// Current height of the vertex
            /// </summary>
            public int Height { get; set; }

            public ResidualGraphVertexStatus(int height, W overflow)
            {
                Height = height;
                Overflow = overflow;
            }

        }
    }
}
