using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mhyrenz_Interface.WpfTest
{
    public class DraggablePanel : Panel
    {
        // Dictionary to quickly look up items by their index position
        internal Dictionary<int, DraggableListBoxItem> ItemDic = new Dictionary<int, DraggableListBoxItem>();

        // Controls whether the panel should respond to measure requests
        internal bool CanUpdate = true;
        internal bool ForceUpdate = false;

        private int _itemCount;
        private Size _oldSize;
        private bool _isLoaded;

        // Item height for uniform sizing
        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(nameof(ItemHeight), typeof(double),
                typeof(DraggablePanel), new PropertyMetadata(40.0));

        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public DraggablePanel()
        {
            Loaded += (s, e) =>
            {
                if (_isLoaded) return;
                UpdateMeasure();
                _isLoaded = true;
            };
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // Skip measure if nothing changed and we're not forcing an update
            if ((_itemCount == InternalChildren.Count || !CanUpdate) && !ForceUpdate)
                return _oldSize;

            _itemCount = InternalChildren.Count;
            ItemDic.Clear();

            if (_itemCount == 0)
            {
                _oldSize = new Size();
                return _oldSize;
            }

            var size = new Size();
            var itemHeight = ItemHeight;

            // Measure and position each item
            for (var index = 0; index < _itemCount; index++)
            {
                if (InternalChildren[index] is DraggableListBoxItem item)
                {
                    // Each item gets a TranslateTransform for animation
                    item.RenderTransform = new TranslateTransform();

                    // Arrange item in its position
                    var rect = new Rect(0, size.Height, constraint.Width, itemHeight);
                    item.Arrange(rect);

                    // Track item properties
                    item.ItemHeight = itemHeight;
                    item.CurrentIndex = index;
                    item.TargetOffsetY = 0;
                    ItemDic[index] = item;

                    size.Height += itemHeight;
                }
            }

            size.Width = constraint.Width;
            _oldSize = size;
            return _oldSize;
        }

        public void UpdateMeasure()
        {
            ForceUpdate = true;
            Measure(new Size(DesiredSize.Width, ActualHeight));
            ForceUpdate = false;

            // Ensure all items have reference to this panel
            foreach (var item in ItemDic.Values)
            {
                item.ItemPanel = this;
            }
        }
    }
}