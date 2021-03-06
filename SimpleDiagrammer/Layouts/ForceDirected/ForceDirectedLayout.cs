﻿//source of inspiration: http://www.brad-smith.info/blog/archives/129
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SimpleDiagrammer.Support;

namespace SimpleDiagrammer.Layouts.ForceDirected
{
    class ForceDirectedLayout : LayoutAlgorithm
    {
        private static readonly Random random = new Random();

        internal Configuration Configuration { get; private set; }

        private const double ATTRACTION_CONSTANT = 0.1;		// spring constant
        private const double REPULSION_CONSTANT = 5000;	    // charge constant
        private const double DEFAULT_DAMPING = 0.5;

        private Dictionary<Node, NodeLayoutInfo> nodeToLayoutInfo = new Dictionary<Node, NodeLayoutInfo>();

        public override void UpdateLayout(IEnumerable<Node> nodes)
        {
            if ((!Configuration.ApplyAttractionForce) && (!Configuration.ApplyRepulsionForce)) {
                return;
            }

            #region Separate overlapping nodes
            if (Configuration.ApplyRepulsionForce) {
                foreach (Node node1 in nodes) {
                    foreach (Node node2 in nodes) {
                        if ((node1 != node2) && (CalcDistance(node1.Location, node2.Location) < 1.0d)) {
                            node1.Location += new Vector(random.NextDouble() * 100d - 50d, random.NextDouble() * 100d - 50d);
                        }
                    }
                }
            }
            #endregion

            #region Update nodeToLayoutInfo
            foreach (Node node in nodeToLayoutInfo.Keys.ToArray()) {
                if (!nodes.Contains(node)) {
                    nodeToLayoutInfo.Remove(node);
                }
            }
            foreach (Node node in nodes) {
                if (!nodeToLayoutInfo.ContainsKey(node)) {
                    NodeLayoutInfo layoutInfo = new NodeLayoutInfo(node, new Vector(), node.Location);
                    nodeToLayoutInfo.Add(node, layoutInfo);
                }
            }
            #endregion

            foreach (NodeLayoutInfo current in nodeToLayoutInfo.Values) {
                Point currentPosition = current.Node.Location;
                Vector netForce = new Vector(0, 0);

                // determine repulsion between nodes
                if (Configuration.ApplyRepulsionForce) {
                    foreach (Node other in nodes) {
                        if (other != current.Node) {
                            netForce += CalcRepulsionForce(current.Node, other);
                        }
                    }
                }

                // determine attraction caused by connections
                if (Configuration.ApplyAttractionForce) {
                    foreach (Node child in current.Node.GetAdjacentNodes()) {
                        netForce += CalcAttractionForce(current.Node, child, Configuration.AttractionSpringLength);
                    }
                    foreach (Node parent in nodes) {
                        if (parent.GetAdjacentNodes().Contains(current.Node)) {
                            netForce += CalcAttractionForce(current.Node, parent, Configuration.AttractionSpringLength);
                        }
                    }
                }

                // apply net force to node velocity
                current.Velocity = (current.Velocity + netForce) * DEFAULT_DAMPING;

                if (current.Velocity.Length >= 1) {
                    // apply velocity to node position
                    current.NextPosition = currentPosition + current.Velocity;
                } else {
                    current.NextPosition = currentPosition;
                }
            }

            // move nodes to resultant positions
            foreach (NodeLayoutInfo current in nodeToLayoutInfo.Values) {
                current.Node.Location = current.NextPosition;
            }
        }

        /// <summary>
        /// Calculates the attraction force between two connected nodes, using the specified spring length.
        /// </summary>
        /// <param name="x">The node that the force is acting on.</param>
        /// <param name="y">The node creating the force.</param>
        /// <param name="springLength">The length of the spring, in pixels.</param>
        /// <returns>A Vector representing the attraction force.</returns>
        private Vector CalcAttractionForce(Node x, Node y, double springLength)
        {
            //double proximity = Math.Max(GetNodeDistance(x, y), 1);
            double proximity = Math.Max(CalcDistance(x.Location, y.Location), 1);

            // Hooke's Law: F = -kx
            double force = ATTRACTION_CONSTANT * Math.Max(proximity - springLength, 0);
            double angle = GetBearingAngle(x.Location, y.Location);

            return CreateVectorFromLengthAndDirection(force, angle);
        }

        /// <summary>
        /// Calculates the repulsion force between any two nodes in the diagram space.
        /// </summary>
        /// <param name="x">The node that the force is acting on.</param>
        /// <param name="y">The node creating the force.</param>
        /// <returns>A Vector representing the repulsion force.</returns>
        private Vector CalcRepulsionForce(Node x, Node y)
        {
            double proximity = Math.Max(GetNodeDistance(x, y), 20);

            // Coulomb's Law: F = k(Qq/r^2)
            double force = -(REPULSION_CONSTANT / Math.Pow(proximity, 2));
            double angle = GetBearingAngle(x.Location, y.Location);

            Rect rect1 = new Rect(x.Location, x.Size);
            Rect rect2 = new Rect(y.Location, y.Size);

            return CreateVectorFromLengthAndDirection(force, angle);
        }

        private double GetNodeDistance(Node node1, Node node2)
        {
            return MathUtils.GetLinkLengthBetweenRectangles(new Rect(node1.Location, node1.Size), new Rect(node2.Location, node2.Size));
        }

        /// <summary>
        /// Calculates the bearing angle from one point to another.
        /// </summary>
        /// <param name="start">The node that the angle is measured from.</param>
        /// <param name="end">The node that creates the angle.</param>
        /// <returns>The bearing angle, in degrees.</returns>
        private double GetBearingAngle(Point start, Point end)
        {
            Point half = new Point(start.X + ((end.X - start.X) / 2), start.Y + ((end.Y - start.Y) / 2));

            double diffX = half.X - start.X;
            double diffY = half.Y - start.Y;

            if (diffX == 0) diffX = 0.001;
            if (diffY == 0) diffY = 0.001;

            double angle;
            if (Math.Abs(diffX) > Math.Abs(diffY)) {
                angle = Math.Tanh(diffY / diffX) * (180.0 / Math.PI);
                if (((diffX < 0) && (diffY > 0)) || ((diffX < 0) && (diffY < 0))) angle += 180;
            } else {
                angle = Math.Tanh(diffX / diffY) * (180.0 / Math.PI);
                if (((diffY < 0) && (diffX > 0)) || ((diffY < 0) && (diffX < 0))) angle += 180;
                angle = (180 - (angle + 90));
            }

            return angle;
        }

        /// <summary>
        /// Calculates the distance between two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The pixel distance between the two points.</returns>
        public double CalcDistance(Point a, Point b)
        {
            double xDist = (a.X - b.X);
            double yDist = (a.Y - b.Y);
            return Math.Sqrt(Math.Pow(xDist, 2) + Math.Pow(yDist, 2));
        }

        private Vector CreateVectorFromLengthAndDirection(double length, double direction)
        {
            double x = length * Math.Cos((Math.PI / 180.0) * direction);
            double y = length * Math.Sin((Math.PI / 180.0) * direction);
            return new Vector(x, y);
        }

        public override object CreateDefaultConfiguration()
        {
            return new Configuration();
        }

        public override void ApplyConfiguration(object configuration)
        {
            Configuration = (Configuration)configuration;
        }

        /// <summary>
        /// Private inner class used to track the node's position and velocity during simulation.
        /// </summary>
        private class NodeLayoutInfo
        {

            public Node Node;			// reference to the node in the simulation
            public Vector Velocity;		// the node's current velocity, expressed in vector form
            public Point NextPosition;	// the node's position after the next iteration

            /// <summary>
            /// Initialises a new instance of the Diagram.NodeLayoutInfo class, using the specified parameters.
            /// </summary>
            /// <param name="node"></param>
            /// <param name="velocity"></param>
            /// <param name="nextPosition"></param>
            public NodeLayoutInfo(Node node, Vector velocity, Point nextPosition)
            {
                Node = node;
                Velocity = velocity;
                NextPosition = nextPosition;
            }
        }
    }
}
