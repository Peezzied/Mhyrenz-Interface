using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using MahApps.Metro.Controls;

namespace Mhyrenz_Interface.Features.Inventory.Controls
{
    public class TrackedPopup : Popup
    {
        private Window _hostWindow;
        private ScrollViewer _scrollViewer;
        private FrameworkElement _trackedAnchor;

        // Scroll tracking: was the popup open before the anchor scrolled out of view?
        private bool _wasOpen;

        // Virtualization tracking: was the popup open before the anchor was recycled?
        private bool _suspendedOpen;

        // Prevents re-applying the same topmost state unnecessarily.
        private bool? _appliedTopMost;

        // -------------------------------------------------------------------------
        // Win32 interop
        // -------------------------------------------------------------------------

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width { get { return Right - Left; } }
            public int Height { get { return Bottom - Top; } }
        }

        [Flags]
        private enum SetWindowPosFlags : uint
        {
            NoMove = 0x0002,
            NoSize = 0x0001,
            NoActivate = 0x0010,
            NoRedraw = 0x0008,
            NoSendChanging = 0x0400,
            NoOwnerZOrder = 0x0200,
        }

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        // -------------------------------------------------------------------------
        // Anchor dependency property
        // -------------------------------------------------------------------------

        /// <summary>Identifies the <see cref="Anchor"/> dependency property.</summary>
        public static readonly DependencyProperty AnchorProperty =
            DependencyProperty.Register(
                nameof(Anchor),
                typeof(FrameworkElement),
                typeof(TrackedPopup),
                new PropertyMetadata(null, OnAnchorChanged));

        /// <summary>
        /// Gets or sets the <see cref="FrameworkElement"/> used as the anchor for scroll
        /// visibility tracking. This can be the same as <see cref="Popup.PlacementTarget"/>
        /// or a different element (e.g. a named element inside the same visual tree).
        /// </summary>
        public FrameworkElement Anchor
        {
            get { return (FrameworkElement)GetValue(AnchorProperty); }
            set { SetValue(AnchorProperty, value); }
        }

        private static void OnAnchorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var popup = (TrackedPopup)d;

            // Detach from the old anchor.
            if (e.OldValue is FrameworkElement oldAnchor)
            {
                popup.DetachAnchorEvents(oldAnchor);
            }

            // Re-attach to the new anchor if we already have a host window.
            if (popup._hostWindow != null && e.NewValue is FrameworkElement newAnchor)
            {
                popup.AttachAnchorEvents(newAnchor);
                popup.AttachScrollViewer();
            }
        }

        // -------------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------------

        public TrackedPopup()
        {
            Loaded += OnLoaded;
            Opened += OnOpened;
        }

        // -------------------------------------------------------------------------
        // Loaded / Unloaded
        // -------------------------------------------------------------------------

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var root = ResolveAnchor();
            if (root == null)
            {
                return;
            }

            _hostWindow = Window.GetWindow(root);
            if (_hostWindow == null)
            {
                return;
            }

            AttachAnchorEvents(root);
            AttachScrollViewer();
            AttachHostWindow();

            if (PlacementTarget is FrameworkElement placementTarget)
            {
                placementTarget.SizeChanged -= OnSizeOrLocationChanged;
                placementTarget.SizeChanged += OnSizeOrLocationChanged;
            }

            // If we are reloading after a virtualization suspension, restore the popup.
            if (_suspendedOpen)
            {
                _suspendedOpen = false;
                RefreshPosition();
                SetCurrentValue(IsOpenProperty, true);
            }
            else
            {
                RefreshPosition();
            }

            Unloaded -= OnUnloaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // Determine whether this unload is due to UI virtualization recycling
            // the anchor, or a genuine teardown (window closing, control removed).
            //
            // Heuristic: if the anchor exists but is no longer visible, the virtualizer
            // has removed it from the visual tree. The host window is still alive, so
            // we suspend instead of fully detaching — we keep _hostWindow subscriptions
            // so we can detect if the window itself goes away while suspended.
            var anchor = ResolveAnchor();
            var isVirtualized = anchor != null
                                && !anchor.IsVisible
                                && _hostWindow != null
                                && _hostWindow.IsLoaded;

            if (isVirtualized)
            {
                OnVirtualized();
            }
            else
            {
                OnTeardown();
            }

            Unloaded -= OnUnloaded;
        }

        // Called when the anchor is recycled by a virtualizing panel.
        private void OnVirtualized()
        {
            // Remember whether we were open so we can restore on re-realize.
            _suspendedOpen = IsOpen || _wasOpen;
            _wasOpen = false;

            SetCurrentValue(IsOpenProperty, false);

            // Detach per-anchor and per-scroll resources, but keep host window
            // subscriptions alive so we notice if the window closes while suspended.
            DetachAnchorEvents(ResolveAnchor());
            DetachScrollViewer();

            if (PlacementTarget is FrameworkElement placementTarget)
            {
                placementTarget.SizeChanged -= OnSizeOrLocationChanged;
            }

            // Reset topmost cache — the HWND will be gone after close.
            _appliedTopMost = null;
        }

        // Called on genuine teardown (window closing, control permanently removed).
        private void OnTeardown()
        {
            _suspendedOpen = false;
            _wasOpen = false;

            DetachAnchorEvents(ResolveAnchor());
            DetachScrollViewer();
            DetachHostWindow();

            if (PlacementTarget is FrameworkElement placementTarget)
            {
                placementTarget.SizeChanged -= OnSizeOrLocationChanged;
            }

            Opened -= OnOpened;

            _hostWindow = null;
            _appliedTopMost = null;
        }

        // -------------------------------------------------------------------------
        // Anchor events (virtualization detection)
        // -------------------------------------------------------------------------

        private void AttachAnchorEvents(FrameworkElement anchor)
        {
            if (anchor == null || ReferenceEquals(_trackedAnchor, anchor))
            {
                return;
            }

            DetachAnchorEvents(_trackedAnchor);

            _trackedAnchor = anchor;
            _trackedAnchor.IsVisibleChanged -= OnAnchorIsVisibleChanged;
            _trackedAnchor.IsVisibleChanged += OnAnchorIsVisibleChanged;
        }

        private void DetachAnchorEvents(FrameworkElement anchor)
        {
            if (anchor == null)
            {
                return;
            }

            anchor.IsVisibleChanged -= OnAnchorIsVisibleChanged;

            if (ReferenceEquals(_trackedAnchor, anchor))
            {
                _trackedAnchor = null;
            }
        }

        private void OnAnchorIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // When the anchor turns invisible while the popup is open, treat it as
            // an early virtualization signal and close immediately — before Unloaded
            // fires — to avoid the popup briefly hanging in the wrong position.
            if (!(bool)e.NewValue && IsOpen)
            {
                _suspendedOpen = true;
                _wasOpen = false;
                SetCurrentValue(IsOpenProperty, false);
                _appliedTopMost = null;
            }
        }

        // -------------------------------------------------------------------------
        // Host window tracking
        // -------------------------------------------------------------------------

        private void AttachHostWindow()
        {
            _hostWindow.LocationChanged -= OnSizeOrLocationChanged;
            _hostWindow.LocationChanged += OnSizeOrLocationChanged;
            _hostWindow.SizeChanged -= OnSizeOrLocationChanged;
            _hostWindow.SizeChanged += OnSizeOrLocationChanged;
            _hostWindow.StateChanged -= OnHostWindowStateChanged;
            _hostWindow.StateChanged += OnHostWindowStateChanged;
            _hostWindow.Activated -= OnHostWindowActivated;
            _hostWindow.Activated += OnHostWindowActivated;
            _hostWindow.Deactivated -= OnHostWindowDeactivated;
            _hostWindow.Deactivated += OnHostWindowDeactivated;
            _hostWindow.Closed -= OnHostWindowClosed;
            _hostWindow.Closed += OnHostWindowClosed;
        }

        private void DetachHostWindow()
        {
            if (_hostWindow == null)
            {
                return;
            }

            _hostWindow.LocationChanged -= OnSizeOrLocationChanged;
            _hostWindow.SizeChanged -= OnSizeOrLocationChanged;
            _hostWindow.StateChanged -= OnHostWindowStateChanged;
            _hostWindow.Activated -= OnHostWindowActivated;
            _hostWindow.Deactivated -= OnHostWindowDeactivated;
            _hostWindow.Closed -= OnHostWindowClosed;
        }

        private void OnHostWindowActivated(object sender, EventArgs e)
        {
            SetTopmostState(true);
        }

        private void OnHostWindowDeactivated(object sender, EventArgs e)
        {
            SetTopmostState(false);
        }

        private void OnHostWindowStateChanged(object sender, EventArgs e)
        {
            if (_hostWindow == null || _hostWindow.WindowState == WindowState.Minimized)
            {
                return;
            }

            var wasOpen = IsOpen;
            SetCurrentValue(IsOpenProperty, false);
            if (wasOpen)
            {
                OnHostWindowRestored();
            }
        }

        private void OnHostWindowClosed(object sender, EventArgs e)
        {
            // Window closed while we were suspended — discard the suspended state
            // so we don't try to restore a popup whose owner is gone.
            _suspendedOpen = false;
            _wasOpen = false;
            SetCurrentValue(IsOpenProperty, false);
            DetachHostWindow();
            _hostWindow = null;
        }

        /// <summary>
        /// Called when the host window is restored from a minimized state and the popup
        /// was open at the time. Override to reopen or refresh the popup as needed.
        /// </summary>
        protected virtual void OnHostWindowRestored()
        {
        }

        // -------------------------------------------------------------------------
        // ScrollViewer tracking
        // -------------------------------------------------------------------------

        private void AttachScrollViewer()
        {
            DetachScrollViewer();

            var anchor = ResolveAnchor();
            if (anchor == null)
            {
                return;
            }

            _scrollViewer = TreeHelper.TryFindParent<ScrollViewer>(anchor);
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged += OnScrollChanged;
            }
        }

        private void DetachScrollViewer()
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged -= OnScrollChanged;
                _scrollViewer = null;
            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange == 0 && e.HorizontalChange == 0)
            {
                return;
            }

            RefreshPosition();

            var anchor = ResolveAnchor();
            if (IsElementVisible(anchor, _scrollViewer))
            {
                // Anchor has scrolled back into view — restore if it was open before.
                if (_wasOpen && !IsOpen)
                {
                    SetCurrentValue(IsOpenProperty, true);
                }
            }
            else
            {
                // Anchor has scrolled out of view — remember state and hide.
                if (IsOpen)
                {
                    _wasOpen = true;
                    SetCurrentValue(IsOpenProperty, false);
                }
            }
        }

        private static bool IsElementVisible(FrameworkElement element, FrameworkElement container)
        {
            if (element == null || container == null || !element.IsVisible)
            {
                return false;
            }

            var bounds = element.TransformToAncestor(container)
                                .TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            var rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.IntersectsWith(bounds);
        }

        // -------------------------------------------------------------------------
        // Repositioning
        // -------------------------------------------------------------------------

        private void OnSizeOrLocationChanged(object sender, EventArgs e)
        {
            RefreshPosition();
        }

        private void RefreshPosition()
        {
            var offset = HorizontalOffset;
            // Bump the offset by 1 to force the Popup to recalculate its position,
            // then immediately restore it. This is the standard WPF trick for this.
            SetCurrentValue(HorizontalOffsetProperty, offset + 1);
            SetCurrentValue(HorizontalOffsetProperty, offset);
        }

        // -------------------------------------------------------------------------
        // Topmost state (Win32)
        // -------------------------------------------------------------------------

        private void OnOpened(object sender, EventArgs e)
        {
            // Reset _wasOpen whenever the popup is opened externally,
            // so scroll tracking starts fresh.
            _wasOpen = true;
            SetTopmostState(true);
        }

        private void SetTopmostState(bool isTop)
        {
            if (_appliedTopMost.HasValue && _appliedTopMost == isTop)
            {
                return;
            }

            if (Child == null)
            {
                return;
            }

            if (!(PresentationSource.FromVisual(Child) is HwndSource hwndSource))
            {
                return;
            }

            var handle = hwndSource.Handle;

            var rect = new RECT();
            if (!GetWindowRect(handle, ref rect))
            {
                return;
            }

            if (rect.Left == 0 && rect.Top == 0 && rect.Right == 0 && rect.Bottom == 0)
            {
                return;
            }

            var flags = (uint)(
                SetWindowPosFlags.NoActivate
                | SetWindowPosFlags.NoOwnerZOrder
                | SetWindowPosFlags.NoSize
                | SetWindowPosFlags.NoMove
                | SetWindowPosFlags.NoRedraw
                | SetWindowPosFlags.NoSendChanging);

            if (isTop)
            {
                SetWindowPos(handle, HWND_TOPMOST, rect.Left, rect.Top, rect.Width, rect.Height, flags);
            }
            else
            {
                // Must go BOTTOM → TOP → NOTOPMOST to correctly refresh the Z-order
                // when clicking non-titlebar areas of the external window.
                SetWindowPos(handle, HWND_BOTTOM, rect.Left, rect.Top, rect.Width, rect.Height, flags);
                SetWindowPos(handle, HWND_TOP, rect.Left, rect.Top, rect.Width, rect.Height, flags);
                SetWindowPos(handle, HWND_NOTOPMOST, rect.Left, rect.Top, rect.Width, rect.Height, flags);
            }

            _appliedTopMost = isTop;
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        private FrameworkElement ResolveAnchor()
        {
            return Anchor ?? (PlacementTarget as FrameworkElement);
        }
    }
}
