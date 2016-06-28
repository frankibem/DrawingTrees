using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawingTrees
{
    /// <summary>
    /// SortedTree implementation that is able to draw itself.
    /// </summary>
    public class Tree
    {
        public TreeNode Root { get; private set; }

        /// <summary>
        /// The surface on which the tree will be rendered
        /// </summary>
        public Canvas Surface
        {
            get { return surface; }
            set
            {
                surface = value;
                Render();
            }
        }

        /// <summary>
        /// Center horizontal coordinate of root node
        /// </summary>
        public double RootX
        {
            get { return rootX; }
            set
            {
                rootX = value;
                Render();
            }
        }

        /// <summary>
        /// Center vertical coordinate of root node
        /// </summary>
        public double RootY
        {
            get { return rootY; }
            set
            {
                rootY = value;
                Render();
            }
        }

        public double Diameter
        {
            get { return diameter; }
            set
            {
                diameter = value;
                Render();
            }
        }

        public double LevelHeight
        {
            get { return levelHeight; }
            set
            {
                levelHeight = value;
                Render();
            }
        }

        public double ChildSeparation
        {
            get { return childSeparation; }
            set
            {
                childSeparation = value;
                Render();
            }
        }

        /// <summary>
        /// Creates a new drawable tree centered at the given coordinates
        /// </summary>
        /// <param name="surface">Canvas on which to draw the tree</param>
        /// <param name="x">Center horizontal coordinate of root node</param>
        /// <param name="y">Center vertical coordinate of root node</param>
        /// <remarks>Assumes canvas is exclusively used for drawing the tree</remarks>
        public Tree(Canvas surface, double x = 0, double y = 0, double diameter = 40, double levelHeight = 80, double childSeparation = 80)
        {
            Surface = surface;
            RootX = x;
            RootY = y;
            Diameter = diameter;
            LevelHeight = levelHeight;
            ChildSeparation = childSeparation;
        }

        /// <summary>
        /// Add a new node with the given data to the tree.
        /// Causes a redraw of the tree
        /// </summary>
        /// <param name="element"></param>
        public void Add(char element)
        {
            if(Root == null)
            {
                Root = new TreeNode(element);
            }
            else
            {
                Root.AddNode(element);
            }

            Render();
        }

        /// <summary>
        /// Adds a sequence of elements to the tree in order
        /// </summary>
        /// <param name="elements"></param>
        public void Add(IEnumerable<char> elements)
        {
            foreach(var element in elements)
                Add(element);
        }

        /// <summary>
        /// Draws the tree on the given canvas
        /// </summary>
        /// <remarks>Assumes an unlimited amount of space is available</remarks>
        void Render()
        {
            if(Surface != null && Root != null)
            {
                Surface.Children.Clear();
                Root.CalculateBounds(Diameter, LevelHeight, ChildSeparation);
                Root.DrawNode(Surface, RootX, RootY, Diameter, LevelHeight, ChildSeparation);
            }
        }

        private Canvas surface;
        private double
            rootX,
            rootY,
            diameter,
            levelHeight,
            childSeparation;
    }

    /// <summary>
    /// Represents a node in the tree
    /// </summary>
    [DebuggerDisplay("Node {Data}")]
    public class TreeNode
    {
        public const int TextZIndex = 3;
        public const int CircleZIndex = 2;
        public const int LineZIndex = 1;
        public static SolidColorBrush FillColor = new SolidColorBrush(Colors.SkyBlue);
        public static SolidColorBrush StrokeColor = new SolidColorBrush(Colors.Black);

        public char Data { get; set; }
        public TreeNode Left { get; set; }
        public TreeNode Right { get; set; }

        public double Width { get; private set; }
        public double Height { get; private set; }

        public TreeNode(char data)
        {
            Data = data;
        }

        public void AddNode(char element)
        {
            // Place in left subtree
            if(element < Data)
            {
                if(Left == null)
                    Left = new TreeNode(element);
                else
                    Left.AddNode(element);
            }
            else
            {
                if(Right == null)
                    Right = new TreeNode(element);
                else
                    Right.AddNode(element);
            }
        }

        /// <summary>
        /// Calculates the width and height of the tree rooted at this node
        /// </summary>
        /// <param name="diameter">The diameter of a node</param>
        /// <param name="levelHeight">The distance (between centers) of nodes in successive layers</param>
        /// <param name="childSeparation">The space between bounding rectangles of children</param>
        public void CalculateBounds(double diameter, double levelHeight, double childSeparation)
        {
            // Calculate size of children
            if(Left != null) Left.CalculateBounds(diameter, levelHeight, childSeparation);
            if(Right != null) Right.CalculateBounds(diameter, levelHeight, childSeparation);

            if(Left != null && Right != null)   // has both children
            {
                Width = Left.Width + Right.Width + childSeparation;
                Height = levelHeight + Math.Max(Left.Height, Right.Height);
            }
            else if(Left != null)   // single left-child
            {
                Width = Left.Width + (childSeparation + diameter) / 2f;
                Height = Left.Height + levelHeight;
            }
            else if(Right != null) // single right-child
            {
                Width = Right.Width + (childSeparation + diameter) / 2f;
                Height = Right.Height + levelHeight;
            }
            else // no child
            {
                Width = diameter;
                Height = diameter;
            }
        }

        /// <summary>
        /// Draws the sutree rooted at this node on the given surface with this node
        /// centered at (x,y)
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawNode(Canvas surface, double x, double y, double diameter, double levelHeight, double childSeparation)
        {
            // Draw this node
            var circle = getNode(x, y, diameter);
            var text = getText(x, y, diameter);

            var toDraw = new List<UIElement>();
            toDraw.Add(circle);
            toDraw.Add(text);

            double nextY = y + levelHeight;
            if(Left != null && Right != null)       // has both children, position those so that this node appears at center
            {
                double leftStart = x - Width / 2;   // start of left bounding rectangle
                double leftX;
                if(Left.Left != null && Left.Right != null)             // center
                    leftX = leftStart + Left.Width / 2;
                else if(Left.Left != null)                              // position at top-right of bounding rectangle
                    leftX = leftStart + Left.Width - diameter / 2;
                else                                                    // position at top-left of bounding rectangle
                    leftX = leftStart + diameter / 2;

                double rightEnd = leftStart + Width;    // end of right bounding rectangle
                double rightX;
                if(Right.Left != null && Right.Right != null)           // center
                    rightX = rightEnd - Right.Width / 2;
                else if(Right.Right != null)                            // position at top-left of bounding rectangle
                    rightX = rightEnd - Right.Width + diameter / 2;
                else                                                    // position at top-right of bounding rectangle
                    rightX = rightEnd - diameter / 2;

                toDraw.Add(getLine(x, y, leftX, nextY));
                toDraw.Add(getLine(x, y, rightX, nextY));

                Left.DrawNode(surface, leftX, nextY, diameter, levelHeight, childSeparation);
                Right.DrawNode(surface, rightX, nextY, diameter, levelHeight, childSeparation);
            }
            else if(Left != null)
            {
                double leftX = x - (childSeparation + diameter) / 2;
                toDraw.Add(getLine(x, y, leftX, nextY));
                Left.DrawNode(surface, leftX, nextY, diameter, levelHeight, childSeparation);
            }
            else if(Right != null)
            {
                double rightX = x + (childSeparation + diameter) / 2;
                toDraw.Add(getLine(x, y, rightX, nextY));
                Right.DrawNode(surface, rightX, nextY, diameter, levelHeight, childSeparation);
            }

            foreach(var child in toDraw)
            {
                surface.Children.Add(child);
            }
        }

        private Ellipse getNode(double x, double y, double diameter)
        {
            Ellipse circle = new Ellipse();
            circle.Width = circle.Height = diameter;

            // Position it
            double radius = diameter / 2;
            circle.SetValue(Canvas.LeftProperty, x - radius);
            circle.SetValue(Canvas.TopProperty, y - radius);

            // Color stuff
            circle.Stroke = StrokeColor;
            circle.StrokeThickness = 0.5;
            circle.Fill = FillColor;
            circle.SetValue(Panel.ZIndexProperty, CircleZIndex);

            return circle;
        }

        private Border getText(double x, double y, double size)
        {
            // Border is used to center text vertically
            Border border = new Border();
            border.Width = border.Height = size;
            double half = size / 2;
            border.SetValue(Canvas.LeftProperty, x - half);
            border.SetValue(Canvas.TopProperty, y - half);
            border.SetValue(Panel.ZIndexProperty, TextZIndex);

            TextBlock text = new TextBlock();
            text.Text = Data.ToString();
            border.Child = text;
            text.Foreground = StrokeColor;

            // Position it
            text.TextAlignment = TextAlignment.Center;
            text.VerticalAlignment = VerticalAlignment.Center;

            // Draw above lines
            text.SetValue(Panel.ZIndexProperty, TextZIndex);

            return border;
        }

        private Line getLine(double x1, double y1, double x2, double y2)
        {
            Line line = new Line();
            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;

            line.Stroke = new SolidColorBrush(Colors.Black);
            line.StrokeThickness = 1;
            line.SetValue(Panel.ZIndexProperty, LineZIndex);

            return line;
        }
    }
}