using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Mhyrenz_Interface.Shared.Controls
{
    public class HotButton : Button
    {
        public static readonly DependencyProperty KeyBindProperty =
            DependencyProperty.Register(
                "KeyBind",
                typeof(Key),
                typeof(HotButton),
                new PropertyMetadata(Key.None));

        public Key KeyBind
        {
            get => (Key)GetValue(KeyBindProperty);
            set => SetValue(KeyBindProperty, value);
        }

        public static readonly DependencyProperty KeyBindModifierProperty =
            DependencyProperty.Register(
                "KeyBindModifier",
                typeof(ModifierKeys),
                typeof(HotButton),
                new PropertyMetadata(ModifierKeys.None));

        public ModifierKeys KeyBindModifier
        {
            get => (ModifierKeys)GetValue(KeyBindModifierProperty);
            set => SetValue(KeyBindModifierProperty, value);
        }

        // Display string e.g. "F5" or "Ctrl+S"
        public string KeyBindDisplay => KeyBindModifier != ModifierKeys.None
            ? $"{KeyBindModifier}+{KeyBind}"
            : KeyBind.ToString();

        public static readonly DependencyProperty KeyBindForegroundProperty =
            DependencyProperty.Register(
                "KeyBindForeground",
                typeof(Brush),
                typeof(HotButton),
                new PropertyMetadata(null));

        public Brush KeyBindForeground
        {
            get => (Brush)GetValue(KeyBindForegroundProperty);
            set => SetValue(KeyBindForegroundProperty, value);
        }

        static HotButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HotButton),
                new FrameworkPropertyMetadata(typeof(HotButton)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var window = Window.GetWindow(this);
            if (window == null) return;

            // Clean hook — won't double subscribe
            window.KeyDown -= OnWindowKeyDown;
            window.KeyDown += OnWindowKeyDown;

            Unloaded += (s, e) => window.KeyDown -= OnWindowKeyDown;
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (KeyBind == Key.None) return;
            if (!IsEnabled) return;
            if (e.Key != KeyBind) return;
            if (Keyboard.Modifiers != KeyBindModifier) return;

            // Fire the button's command
            if (Command != null && Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);

            // Also raise Click event for click handlers
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));

            e.Handled = true;
        }
    }
}
