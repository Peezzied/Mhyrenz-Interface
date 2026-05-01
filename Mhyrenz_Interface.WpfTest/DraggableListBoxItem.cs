
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mhyrenz_Interface.WpfTest
{
    public class DraggableListBoxItem : ListBoxItem
    {
        // Animation speed in milliseconds
        private const int AnimationSpeed = 150;

        // Minimum pixel movement before drag starts (prevents accidental drags)
        private const double WaitLength = 20;

        // Static flag to prevent multiple items from being dragged simultaneously
        private static bool ItemIsDragging;

        // Drag state tracking
        private bool _isDragging;
        private bool _isDragged;
        private bool _isWaiting;

        // Position tracking
        private Point _dragPoint;
        private Point _mouseDownPoint;
        private int _mouseDownIndex;
        private double _mouseDownOffsetY;

        // Movement boundaries
        private double _maxMoveUp;
        private double _maxMoveDown;

        // Current state
        private int _currentIndex;
        internal double ItemHeight { get; set; }
        internal double TargetOffsetY { get; set; }

        // Attached property to mark an element as a drag handle
        public static readonly DependencyProperty IsDragHandleProperty =
            DependencyProperty.RegisterAttached("IsDragHandle", typeof(bool),
                typeof(DraggableListBoxItem), new PropertyMetadata(false));

        public static void SetIsDragHandle(DependencyObject element, bool value)
            => element.SetValue(IsDragHandleProperty, value);

        public static bool GetIsDragHandle(DependencyObject element)
            => (bool)element.GetValue(IsDragHandleProperty);
        private DraggablePanel _itemPanel;

        // Lazy-load panel from parent if not set
        internal DraggablePanel ItemPanel
        {
            get
            {
                if (_itemPanel == null)
                {
                    var parent = ListBoxParent;
                    if (parent != null)
                    {
                        _itemPanel = parent.ItemPanel;
                    }
                }
                return _itemPanel;
            }
            set => _itemPanel = value;
        }

        // Current index with automatic position update when changed
        internal int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (_currentIndex == value || value < 0) return;
                var oldIndex = _currentIndex;
                _currentIndex = value;
                UpdateItemOffsetY(oldIndex);
            }
        }

        private DraggableListBox ListBoxParent =>
            ItemsControl.ItemsControlFromItemContainer(this) as DraggableListBox;

        /// <summary>
        /// Checks if the mouse is over an element marked as a drag handle
        /// </summary>
        private bool IsMouseOverDragHandle(MouseButtonEventArgs e)
        {
            var position = e.GetPosition(this);
            var result = VisualTreeHelper.HitTest(this, position);

            if (result == null) return false;

            // Walk up the visual tree to find a drag handle
            var element = result.VisualHit;
            while (element != null && element != this)
            {
                if (element is DependencyObject depObj && GetIsDragHandle(depObj))
                {
                    return true;
                }
                element = VisualTreeHelper.GetParent(element);
            }

            return false;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var parent = ListBoxParent;
            if (parent?.IsDraggable != true || ItemIsDragging || _isDragging)
                return;

            // Check if mouse is over a drag handle (if any handles exist)
            if (!IsMouseOverDragHandle(e))
                return;

            // Initialize drag state
            _mouseDownOffsetY = RenderTransform.Value.OffsetY;
            var my = TranslatePoint(new Point(), parent).Y;
            _mouseDownIndex = CalLocationIndex(my);

            // Calculate movement boundaries (can't drag outside list bounds)
            var subIndex = _mouseDownIndex;
            _maxMoveUp = -subIndex * ItemHeight;
            _maxMoveDown = parent.ActualHeight - ActualHeight + _maxMoveUp;

            // Set up drag tracking
            _isDragging = true;
            ItemIsDragging = true;
            _isWaiting = true;
            _dragPoint = e.GetPosition(parent);
            _mouseDownPoint = _dragPoint;
            CaptureMouse();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!ItemIsDragging || !_isDragging) return;

            var parent = ListBoxParent;
            if (parent == null) return;

            // Calculate current position and index
            var subY = TranslatePoint(new Point(), parent).Y;
            CurrentIndex = CalLocationIndex(subY);

            var p = e.GetPosition(parent);
            var subTop = p.Y - _dragPoint.Y;
            var totalTop = p.Y - _mouseDownPoint.Y;

            // Wait for minimum movement before starting visual drag
            if (Math.Abs(subTop) <= WaitLength && _isWaiting) return;

            _isWaiting = false;
            _isDragged = true;

            // Bring dragged item to front
            Panel.SetZIndex(this, 1000);

            // Calculate new position with boundary constraints
            var top = subTop + RenderTransform.Value.OffsetY;
            if (totalTop < _maxMoveUp)
                top = _maxMoveUp + _mouseDownOffsetY;
            else if (totalTop > _maxMoveDown)
                top = _maxMoveDown + _mouseDownOffsetY;

            // Apply visual transform
            RenderTransform = new TranslateTransform(0, top);
            _dragPoint = p;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            ReleaseMouseCapture();

            if (_isDragged)
            {
                var parent = ListBoxParent;
                if (parent != null)
                {
                    // Calculate final position
                    var subY = TranslatePoint(new Point(), parent).Y;
                    var index = CalLocationIndex(subY);
                    var top = index * ItemHeight;
                    var offsetY = RenderTransform.Value.OffsetY;

                    // Animate to final position and update collection
                    CreateAnimation(offsetY, offsetY - subY + top, index);
                }
            }

            // Reset z-index back to normal
            Panel.SetZIndex(this, 0);

            // Reset drag state
            _isDragging = false;
            ItemIsDragging = false;
            _isDragged = false;
        }

        /// <summary>
        /// Updates the position of the item that was displaced by the drag
        /// </summary>
        private void UpdateItemOffsetY(int oldIndex)
        {
            if (!_isDragging || ItemPanel == null || CurrentIndex >= ItemPanel.ItemDic.Count)
                return;

            // Get the item that needs to move out of the way
            var moveItem = ItemPanel.ItemDic[CurrentIndex];
            moveItem.CurrentIndex -= CurrentIndex - oldIndex;

            // Calculate animation values
            var offsetY = moveItem.TargetOffsetY;
            var resultY = offsetY + (oldIndex - CurrentIndex) * ItemHeight;

            // Update dictionary
            ItemPanel.ItemDic[CurrentIndex] = this;
            ItemPanel.ItemDic[moveItem.CurrentIndex] = moveItem;

            // Animate the displaced item
            moveItem.CreateAnimation(offsetY, resultY);
        }

        /// <summary>
        /// Creates smooth animation from current position to target position
        /// </summary>
        internal void CreateAnimation(double offsetY, double resultY, int index = -1)
        {
            var parent = ListBoxParent;
            if (parent == null) return;

            void AnimationCompleted()
            {
                RenderTransform = new TranslateTransform(0, resultY);

                if (index == -1 || ItemPanel == null) return;

                // Update the actual collection after animation completes
                var list = parent.GetActualList();
                if (list == null) return;

                var item = parent.ItemContainerGenerator.ItemFromContainer(this);
                if (item == null) return;

                ItemPanel.CanUpdate = false;
                parent.IsInternalAction = true;

                // Find current index of the item
                var currentIdx = list.IndexOf(item);
                if (currentIdx == -1 || currentIdx == index)
                {
                    ItemPanel.CanUpdate = true;
                    parent.IsInternalAction = false;
                    return;
                }

                // Try to use Move method if available (preserves container state)
                var moveMethod = list.GetType().GetMethod("Move",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new[] { typeof(int), typeof(int) },
                    null);

                if (moveMethod != null)
                {
                    // Move preserves the container and its state!
                    try
                    {
                        moveMethod.Invoke(list, new object[] { currentIdx, index });
                    }
                    catch
                    {
                        // Fallback if Move fails
                        list.Remove(item);
                        list.Insert(index, item);
                    }
                }
                else
                {
                    // Fallback for collections without Move (may reset UI state)
                    list.Remove(item);
                    parent.IsInternalAction = true;
                    list.Insert(index, item);
                }

                ItemPanel.CanUpdate = true;
                ItemPanel.ForceUpdate = true;
                ItemPanel.Measure(new Size(ItemPanel.DesiredSize.Width, ActualHeight));
                ItemPanel.ForceUpdate = false;

                Focus();
                IsSelected = true;

                if (!IsMouseCaptured)
                {
                    parent.SelectedIndex = _currentIndex;
                }
            }

            TargetOffsetY = resultY;

            // Create and run animation
            var animation = new DoubleAnimation
            {
                To = resultY,
                Duration = TimeSpan.FromMilliseconds(AnimationSpeed),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.Stop
            };

            animation.Completed += (s, e) => AnimationCompleted();

            var transform = new TranslateTransform(0, offsetY);
            RenderTransform = transform;
            transform.BeginAnimation(TranslateTransform.YProperty, animation);
        }

        /// <summary>
        /// Calculates which index position corresponds to a Y coordinate
        /// Uses 0.5 threshold for snapping behavior
        /// </summary>
        private int CalLocationIndex(double top)
        {
            if (_isWaiting)
                return CurrentIndex;

            var maxIndex = ListBoxParent.Items.Count - 1;
            var div = (int)(top / ItemHeight);
            var rest = top % ItemHeight;

            // Snap to nearest: if past halfway, use next index
            var result = rest / ItemHeight > 0.5 ? div + 1 : div;

            return result > maxIndex ? maxIndex : result;
        }
    }
}