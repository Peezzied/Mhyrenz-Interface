using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Mhyrenz_Interface.WpfTest
{
    public class DraggableListBox : ListBox
    {
        internal DraggablePanel ItemPanel { get; private set; }

        // Prevents infinite loops during programmatic collection changes
        internal bool IsInternalAction;

        public DraggableListBox()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Ensure panel is set up after template is applied
            ItemPanel?.UpdateMeasure();
        }

        // Enable/disable drag functionality
        public static readonly DependencyProperty IsDraggableProperty =
            DependencyProperty.Register(nameof(IsDraggable), typeof(bool),
                typeof(DraggableListBox), new PropertyMetadata(true));

        public bool IsDraggable
        {
            get => (bool)GetValue(IsDraggableProperty);
            set => SetValue(IsDraggableProperty, value);
        }

        // Item height for uniform sizing
        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(nameof(ItemHeight), typeof(double),
                typeof(DraggableListBox), new PropertyMetadata(40.0));

        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ItemPanel = GetTemplateChild("PART_ItemPanel") as DraggablePanel;
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            // Skip if this is an internal action (to prevent recursion)
            if (IsInternalAction)
            {
                IsInternalAction = false;
                return;
            }

            if (ItemPanel == null)
            {
                IsInternalAction = false;
                return;
            }

            // Set up new items with reference to panel
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                for (var i = 0; i < Items.Count; i++)
                {
                    if (ItemContainerGenerator.ContainerFromIndex(i) is DraggableListBoxItem item)
                    {
                        item.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        item.ItemPanel = ItemPanel;
                    }
                }
            }

            // Force panel to remeasure with new items
            ItemPanel.ForceUpdate = true;
            ItemPanel.InvalidateMeasure();

            IsInternalAction = false;
        }

        /// <summary>
        /// Gets the actual list (either ItemsSource or Items collection)
        /// </summary>
        internal IList GetActualList()
        {
            return ItemsSource as IList ?? Items;
        }

        protected override bool IsItemItsOwnContainerOverride(object item) =>
            item is DraggableListBoxItem;

        protected override DependencyObject GetContainerForItemOverride() =>
            new DraggableListBoxItem();
    }
}