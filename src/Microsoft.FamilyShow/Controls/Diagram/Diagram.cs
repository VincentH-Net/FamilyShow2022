/*
 * Arranges nodes based on relationships. Contains a series or rows (DiagramRow), 
 * each row contains a series of groups (DiagramGroup), and each group contains a 
 * series of nodes (DiagramNode).
 * 
 * Contains a list of connections. Each connection describes where the node
 * is located in the diagram and type of connection. The lines are draw
 * during OnRender.
 * 
 * Diagram is responsible for managing the rows. The logic that populates the rows
 * and understand all of the relationships is contained in DiagramLogic.
 *
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow.Controls.Diagram;

/// <summary>
///     Diagram that lays out and displays the nodes.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Diagram : FrameworkElement
{
    public Diagram()
    {
        // Init the diagram logic, which handles all of the layout logic.
        logic = new DiagramLogic
        {
            NodeClickHandler = OnNodeClick
        };

        // Can have an empty People collection when in design tools such as Blend.
        if (logic.Family != null)
        {
            logic.Family.ContentChanged += OnFamilyContentChanged;
            logic.Family.CurrentChanged += OnFamilyCurrentChanged;
        }
    }

    /// <summary>
    ///     Draw the connector lines at a lower level (OnRender) instead
    ///     of creating visual tree objects.
    /// </summary>
    protected override void OnRender(DrawingContext drawingContext)
    {
#if DEBUG
        if (displayBorder)
            // Draws borders around the rows and groups.
            foreach (var row in rows)
            {
                // Display row border.
                var bounds = new Rect(row.Location, row.DesiredSize);
                drawingContext.DrawRectangle(null, new Pen(Brushes.DarkKhaki, 1), bounds);

                foreach (var group in row.Groups)
                {
                    // Display group border.
                    bounds = new Rect(@group.Location, @group.DesiredSize);
                    bounds.Offset(row.Location.X, row.Location.Y);
                    bounds.Inflate(-1, -1);
                    drawingContext.DrawRectangle(null, new Pen(Brushes.Gray, 1), bounds);
                }
            }
#endif

        // Draw child connectors first, so marriage information appears on top.
        foreach (var connector in logic.Connections)
            if (connector.IsChildConnector)
                connector.Draw(drawingContext);

        // Draw all other non-child connectors.
        foreach (var connector in logic.Connections)
            if (!connector.IsChildConnector)
                connector.Draw(drawingContext);
    }

    /// <summary>
    ///     Animate the new person that was added to the diagram.
    /// </summary>
    private void AnimateNewPerson()
    {
        // The new person is optional, can be null.
        if (newPerson == null) return;

        // Get the UI element to animate.                
        var node = logic.GetDiagramNode(newPerson);
        if (node != null)
        {
            // Create the new person animation.
            var anim = new DoubleAnimation(0, 1, App.GetAnimationDuration(Const.NewPersonAnimationDuration));

            // Animate the node.
            var transform = new ScaleTransform();
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
            node.RenderTransform = transform;
        }

        newPerson = null;
    }

    /// <summary>
    ///     Called when data changed in the main People collection. This can be
    ///     a new node added to the collection, updated Person details, and
    ///     updated relationship data.
    /// </summary>
    private void OnFamilyContentChanged(object sender, ContentChangedEventArgs e)
    {
        // Ignore if currently repopulating the diagram.
        if (populating) return;

        // Save the person that is being added to the diagram.
        // This is optional and can be null.
        newPerson = e.NewPerson;

        // Redraw the diagram.
        UpdateDiagram();
        InvalidateMeasure();
        InvalidateArrange();
        InvalidateVisual();
    }

    /// <summary>
    ///     Called when the current person in the main People collection changes.
    ///     This means the diagram should be updated based on the new selected person.
    /// </summary>
    private void OnFamilyCurrentChanged(object sender, EventArgs e)
    {
        // Save the bounds for the current primary person, this 
        // is required later when animating the diagram.
        SelectedNodeBounds = logic.GetNodeBounds(logic.Family.Current);

        // Repopulate the diagram.
        Populate();
    }

    /// <summary>
    ///     A node was clicked, make that node the primary node.
    /// </summary>
    private void OnNodeClick(object sender, RoutedEventArgs e)
    {
        // Get the node that was clicked.
        if (sender is DiagramNode node)
            // Make it the primary node. This raises the CurrentChanged
            // event, which repopulates the diagram.
            logic.Family.Current = node.Person;
    }


#if DEBUG
    private void OnToggleBorderClick(object sender, RoutedEventArgs e)
    {
        // Display or hide the row and group borders.
        displayBorder = !displayBorder;

        // Update check on menu.
        var menuItem = ContextMenu.Items[0] as MenuItem;
        menuItem.IsChecked = displayBorder;

        InvalidateVisual();
    }
#endif

    #region fields

    private static class Const
    {
        // Duration to pause before displaying new nodes.
        public static readonly double AnimationPauseDuration = 600;

        // Duration for nodes to fade in when the diagram is repopulated.
        public static readonly double NodeFadeInDuration = 500;

        // Duration for the new person animation.
        public static readonly double NewPersonAnimationDuration = 250;

        // Stop adding new rows when the number of nodes exceeds the max node limit.
        public static readonly int MaximumNodes = 50;

        // Group space.
        public static readonly double PrimaryRowGroupSpace = 20;
        public static readonly double ChildRowGroupSpace = 20;
        public static readonly double ParentRowGroupSpace = 40;

        // Amount of space between each row.
        public static readonly double RowSpace = 40;

        // Scale multiplier for spouse and siblings.
        public static readonly double RelatedMultiplier = 0.8;

        // Scale multiplier for each future generation row.
        public static readonly double GenerationMultiplier = 0.9;
    }

    // List of rows in the diagram. Each row contains groups, and each group contains nodes.
    private readonly List<DiagramRow> rows = new();

    // Populates the rows with nodes.
    private readonly DiagramLogic logic;

    // Size of the diagram. Used to layout all of the nodes before the
    // control gets an actual size.
    private Size totalSize = new(0, 0);

    // Zoom level of the diagram.
    private double scale = 1.0;

    // Bounding area of the selected node, the selected node is the 
    // non-primary node that is selected, and will become the primary node.

    // Flag if currently populating or not. Necessary since diagram populate 
    // contains several parts and animations, request to update the diagram
    // are ignored when this flag is set.
    private bool populating;

    // The person that has been added to the diagram.
    private Person newPerson;

    // Timer used with the repopulating animation.
    private readonly DispatcherTimer animationTimer = new();

#if DEBUG
    // Flag if the row and group borders should be drawn.
    private bool displayBorder;
#endif

    #endregion

    #region events

    public event EventHandler DiagramUpdated;

    private void OnDiagramUpdated()
    {
        if (DiagramUpdated != null) DiagramUpdated(this, EventArgs.Empty);
    }

    public event EventHandler DiagramPopulated;

    private void OnDiagramPopulated()
    {
        if (DiagramPopulated != null) DiagramPopulated(this, EventArgs.Empty);
    }

    #endregion

    #region properties

    /// <summary>
    ///     Gets or sets the zoom level of the diagram.
    /// </summary>
    public double Scale
    {
        get => scale;
        set
        {
            if (scale != value)
            {
                scale = value;
                LayoutTransform = new ScaleTransform(scale, scale);
            }
        }
    }

    /// <summary>
    ///     Sets the display year filter.
    /// </summary>
    public double DisplayYear
    {
        set
        {
            // Filter nodes and connections based on the year.
            logic.DisplayYear = value;
            InvalidateVisual();
        }
    }

    /// <summary>
    ///     Gets the minimum year specified in the nodes and connections.
    /// </summary>
    public double MinimumYear => logic.MinimumYear;

    /// <summary>
    ///     Gets the bounding area (relative to the diagram) of the primary node.
    /// </summary>
    public Rect PrimaryNodeBounds => logic.GetNodeBounds(logic.Family.Current);

    /// <summary>
    ///     Gets the bounding area (relative to the diagram) of the selected node.
    ///     The selected node is the non-primary node that was previously selected
    ///     to be the primary node.
    /// </summary>
    public Rect SelectedNodeBounds { get; private set; } = Rect.Empty;

    /// <summary>
    ///     Gets the number of nodes in the diagram.
    /// </summary>
    public int NodeCount => logic.PersonLookup.Count;

    #endregion

    #region layout

    protected override void OnInitialized(EventArgs e)
    {
#if DEBUG
        // Context menu so can display row and group borders.
        ContextMenu = new ContextMenu();
        var item = new MenuItem();
        ContextMenu.Items.Add(item);
        item.Header = "Show Diagram Outline";
        item.Click += OnToggleBorderClick;
        item.Foreground = SystemColors.MenuTextBrush;
        item.Background = SystemColors.MenuBrush;
#endif

        UpdateDiagram();
        base.OnInitialized(e);
    }

    protected override int VisualChildrenCount =>
        // Return the number of rows.
        rows.Count;

    protected override Visual GetVisualChild(int index)
    {
        // Return the requested row.
        return rows[index];
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // Let each row determine how large they want to be.
        var size = new Size(double.PositiveInfinity, double.PositiveInfinity);
        foreach (var row in rows) row.Measure(size);

        // Return the total size of the diagram.
        return ArrangeRows(false);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // Arrange the rows in the diagram, return the total size.
        return ArrangeRows(true);
    }

    /// <summary>
    ///     Arrange the rows in the diagram, return the total size.
    /// </summary>
    private Size ArrangeRows(bool arrange)
    {
        // Location of the row.
        double pos = 0;

        // Bounding area of the row.
        var bounds = new Rect();

        // Total size of the diagram.
        var size = new Size(0, 0);

        foreach (var row in rows)
        {
            // Row location, center the row horizontaly.
            bounds.Y = pos;
            bounds.X = totalSize.Width == 0 ? 0 : bounds.X = (totalSize.Width - row.DesiredSize.Width) / 2;

            // Row Size.
            bounds.Width = row.DesiredSize.Width;
            bounds.Height = row.DesiredSize.Height;

            // Arrange the row, save the location.
            if (arrange)
            {
                row.Arrange(bounds);
                row.Location = bounds.TopLeft;
            }

            // Update the size of the diagram.
            size.Width = Math.Max(size.Width, bounds.Width);
            size.Height = pos + row.DesiredSize.Height;

            pos += bounds.Height;
        }

        // Store the size, this is necessary so the diagram
        // can be laid out without a valid Width property.
        totalSize = size;
        return size;
    }

    #endregion

    #region diagram updates

    /// <summary>
    ///     Reset all of the data associated with the diagram.
    /// </summary>
    public void Clear()
    {
        foreach (var row in rows)
        {
            row.Clear();
            RemoveVisualChild(row);
        }

        rows.Clear();
        logic.Clear();
    }

    /// <summary>
    ///     Populate the diagram. Update the diagram and hide all non-primary nodes.
    ///     Then pause, and finish the populate by fading in the new nodes.
    /// </summary>
    private void Populate()
    {
        // Set flag to ignore future updates until complete.
        populating = true;

        // Update the nodes in the diagram.
        UpdateDiagram();

        // First hide all of the nodes except the primary node.
        foreach (var connector in logic.PersonLookup.Values)
            if (connector.Node.Person != logic.Family.Current)
                connector.Node.Visibility = Visibility.Hidden;

        // Required to update (hide) the connector lines.            
        InvalidateVisual();
        InvalidateArrange();
        InvalidateMeasure();

        // Pause before displaying the new nodes.
        animationTimer.Interval = App.GetAnimationDuration(Const.AnimationPauseDuration);
        animationTimer.Tick += OnAnimationTimer;
        animationTimer.IsEnabled = true;

        // Let other controls know the diagram has been repopulated.
        OnDiagramPopulated();
    }

    /// <summary>
    ///     The animation pause timer is complete, finish populating the diagram.
    /// </summary>
    private void OnAnimationTimer(object sender, EventArgs e)
    {
        // Turn off the timer.
        animationTimer.IsEnabled = false;

        // Fade each node into view.
        foreach (var connector in logic.PersonLookup.Values)
            if (connector.Node.Visibility != Visibility.Visible)
            {
                connector.Node.Visibility = Visibility.Visible;
                connector.Node.BeginAnimation(OpacityProperty,
                    new DoubleAnimation(0, 1,
                        App.GetAnimationDuration(Const.NodeFadeInDuration)));
            }

        // Redraw connector lines.
        InvalidateVisual();

        populating = false;
    }

    /// <summary>
    ///     Reset the diagram with the nodes. This is accomplished by creating a series of rows.
    ///     Each row contains a series of groups, and each group contains the nodes. The elements
    ///     are not laid out at this time. Also creates the connections between the nodes.
    /// </summary>
    private void UpdateDiagram()
    {
        // Necessary for Blend.
        if (logic.Family == null)
            return;

        // First reset everything.
        Clear();

        // Nothing to draw if there is not a primary person.
        if (logic.Family.Current == null)
            return;

        // Primary row.
        var primaryPerson = logic.Family.Current;
        var primaryRow = logic.CreatePrimaryRow(primaryPerson, 1.0, Const.RelatedMultiplier);
        primaryRow.GroupSpace = Const.PrimaryRowGroupSpace;
        AddRow(primaryRow);

        // Create as many rows as possible until exceed the max node limit.
        // Switch between child and parent rows to prevent only creating
        // child or parents rows (want to create as many of each as possible).
        var nodeCount = NodeCount;

        // The scale values of future generations, this makes the nodes
        // in each row slightly smaller.
        var nodeScale = 1.0;

        var childRow = primaryRow;
        var parentRow = primaryRow;

        while (nodeCount < Const.MaximumNodes && (childRow != null || parentRow != null))
        {
            // Child Row.
            if (childRow != null) childRow = AddChildRow(childRow);

            // Parent row.
            if (parentRow != null)
            {
                nodeScale *= Const.GenerationMultiplier;
                parentRow = AddParentRow(parentRow, nodeScale);
            }

            // See if reached node limit yet.                                       
            nodeCount = NodeCount;
        }

        // Raise event so others know the diagram was updated.
        OnDiagramUpdated();

        // Animate the new person (optional, might not be any new people).
        AnimateNewPerson();
    }

    /// <summary>
    ///     Add a child row to the diagram.
    /// </summary>
    private DiagramRow AddChildRow(DiagramRow row)
    {
        // Get list of children for the current row.
        var children = DiagramLogic.GetChildren(row);
        if (children.Count == 0) return null;

        // Add bottom space to existing row.
        row.Margin = new Thickness(0, 0, 0, Const.RowSpace);

        // Add another row.
        var childRow = logic.CreateChildrenRow(children, 1.0, Const.RelatedMultiplier);
        childRow.GroupSpace = Const.ChildRowGroupSpace;
        AddRow(childRow);
        return childRow;
    }

    /// <summary>
    ///     Add a parent row to the diagram.
    /// </summary>
    private DiagramRow AddParentRow(DiagramRow row, double nodeScale)
    {
        // Get list of parents for the current row.
        var parents = DiagramLogic.GetParents(row);
        if (parents.Count == 0) return null;

        // Add another row.
        var parentRow = logic.CreateParentRow(parents, nodeScale, nodeScale * Const.RelatedMultiplier);
        parentRow.Margin = new Thickness(0, 0, 0, Const.RowSpace);
        parentRow.GroupSpace = Const.ParentRowGroupSpace;
        InsertRow(parentRow);
        return parentRow;
    }

    /// <summary>
    ///     Add a row to the visual tree.
    /// </summary>
    private void AddRow(DiagramRow row)
    {
        if (row != null && row.NodeCount > 0)
        {
            AddVisualChild(row);
            rows.Add(row);
        }
    }

    /// <summary>
    ///     Insert a row in the visual tree.
    /// </summary>
    private void InsertRow(DiagramRow row)
    {
        if (row != null && row.NodeCount > 0)
        {
            AddVisualChild(row);
            rows.Insert(0, row);
        }
    }

    #endregion
}