using System.Windows;
using System.Windows.Input;

namespace DrawingTrees
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Tree tree;
        private const double diameter = 40;
        private const double levelHeight = 80;
        private const double childSeparation = 40;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;
            FocusManager.SetFocusedElement(this, input);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            initializeTree();
        }

        private void initializeTree()
        {
            double x = surface.ActualWidth / 2;
            double y = diameter + diameter / 2;
            tree = new Tree(surface, x, y, diameter, levelHeight, childSeparation);
        }

        /// <summary>
        /// Redraw the tree whenever the window size changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(tree != null)
                tree.RootX = surface.ActualWidth / 2;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddNode();
        }

        /// <summary>
        /// Adds a node to the tree using input from the textbox
        /// </summary>
        private void AddNode()
        {
            if(input.Text.Length > 0)
            {
                tree.Add(input.Text[0]);
                input.Text = string.Empty;
            }
        }

        private void input_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                AddNode();
            }

            e.Handled = true;
        }

        /// <summary>
        /// Clear the drawing canvas and create a new empty tree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void reset_Click(object sender, RoutedEventArgs e)
        {
            surface.Children.Clear();
            initializeTree();
        }
    }
}