using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;

namespace Mhyrenz_Interface.Core
{
    public class TargetChangedEventArgs : EventArgs
    {
        public object Target { get; }
        public string PropertyOf { get; set; }

        public TargetChangedEventArgs(object target, string propertyOf)
        {
            Target = target;
            PropertyOf = propertyOf;
        }
    }

    public interface IBasePropertyChangeTrackerArgs<out VM> where VM : BaseViewModel
    {
        VM Owner { get; }
        object Value { get; }
        string Navigator { get; }
    }

    public abstract class BasePropertyChangeTrackerArgs<VM> : IBasePropertyChangeTrackerArgs<VM> where VM : BaseViewModel
    {
        public VM Owner { get; }
        public object Value { get; }
        public string Navigator { get; }

        protected BasePropertyChangeTrackerArgs(VM owner, object value, string navigator)
        {
            Owner = owner;
            Value = value;
            Navigator = navigator;
        }
    }

    public class PropertyChangeTracker
    {
        public static bool Suppress { get; set; }

        public class PropertyChangeTrackerGenericArgs<VM> : BasePropertyChangeTrackerArgs<VM> where VM : BaseViewModel
        {
            public PropertyChangeTrackerGenericArgs(VM owner, object value, string navigator) : base(owner, value, navigator) { }
        }
    }

    public class PropertyChangeTracker<T> where T : BaseViewModel
    {

        public class PropertyChangeTrackerArgs : BasePropertyChangeTrackerArgs<T>
        {
            public PropertyChangeTrackerArgs(T owner, object value, string navigator) : base(owner, value, navigator) { }
        }
        
        public Dictionary<string, IBasePropertyChangeTrackerArgs<BaseViewModel>> PreviousValues { get; } = new Dictionary<string, IBasePropertyChangeTrackerArgs<BaseViewModel>>();
        public T Target { get; private set; }
        public Dictionary<string, Action<PropertyChangeTracker<T>, TargetChangedEventArgs, object, object>> Methods { get; } = new Dictionary<string, Action<PropertyChangeTracker<T>, TargetChangedEventArgs, object, object>>();

        public PropertyChangeTracker(T target)
        {
            Target = target;
            WeakEventManager<T, PropertyChangedEventArgs>.AddHandler(Target, nameof(BaseViewModel.PropertyChanged), HandlePropertyChanged);
        }

        public PropertyChangeTracker<T> Track(string propertyName, object value, Action<PropertyChangeTracker<T>, TargetChangedEventArgs, object, object> onPropertyChanged)
        {
            PreviousValues[propertyName] = new PropertyChangeTrackerArgs(Target, value, propertyName);
            Methods[propertyName] = onPropertyChanged;
            return this;
        }


        public PropertyChangeTracker<T> Track<VM>(string propertyName, string valueProperty, VM owner, object value, Action<PropertyChangeTracker<T>, TargetChangedEventArgs, object, object> onPropertyChanged) where VM : BaseViewModel
        {
            PreviousValues[propertyName] = new PropertyChangeTracker.PropertyChangeTrackerGenericArgs<VM>(owner, value, valueProperty);
            Methods[propertyName] = onPropertyChanged;
            return this;
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Core.PropertyChangeTracker.Suppress) return;
            if (!(sender is T target)) throw new ArithmeticException("sender");

            if (e.PropertyName is null) return;
            if (PreviousValues.TryGetValue(e.PropertyName, out var val))
            {
                if (val is PropertyChangeTrackerArgs @this)
                {
                    var propertyName = @this.Navigator;
                    //if (val is Propert)

                    var property = @this.Owner.GetType().GetProperty(propertyName) ?? throw new NoNullAllowedException("property");
                    var newValue = property.GetValue(target);
                    Methods[propertyName]?.Invoke(this, new TargetChangedEventArgs(
                        sender,
                        propertyName), @this.Value, newValue);
                    PreviousValues[propertyName] = new PropertyChangeTrackerArgs(Target, newValue, propertyName);
                }
                else if (val is IBasePropertyChangeTrackerArgs<BaseViewModel> arg)
                {
                    var propertyName = arg.Navigator;
                    var property = arg.Owner.GetType().GetProperty(propertyName) ?? throw new NoNullAllowedException("property");
                    var newValue = property.GetValue(arg.Owner);
                    Methods[e.PropertyName]?.Invoke(this, new TargetChangedEventArgs(
                        arg,
                        e.PropertyName), arg.Value, newValue);
                    PreviousValues[e.PropertyName] = new PropertyChangeTracker.PropertyChangeTrackerGenericArgs<BaseViewModel>(arg.Owner, newValue, propertyName);
                }
            }


            // Update the last known value

        }
    }

}
