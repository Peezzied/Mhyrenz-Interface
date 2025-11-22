using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mhyrenz_Interface.Commands
{
    public abstract class PropertyChangeCommand<T> : IUndoableCommand
    {
        private readonly T _target;
        private readonly string _propertyName;
        private readonly object _oldValue;
        private readonly object _newValue;
        private readonly Action _propertyChangeHandler;

        public Action<NavigationViewModel> SideEffect { get; set; }
        public Type CurrentViewIn { get; }

        public PropertyChangeCommand(T target, string propertyName, object oldValue, object newValue, Action propertyChangeHandler, Type currentViewIn)
        {
            _target = target;
            _propertyName = propertyName;
            _oldValue = oldValue;
            _newValue = newValue;
            _propertyChangeHandler = propertyChangeHandler;

            CurrentViewIn = currentViewIn;
            SideEffect = SideEffectAction;
        }

        private void SideEffectAction(NavigationViewModel vm)
        {
            var view = vm as InventoryViewModel;

            view.RowIntoView(App.ServiceProvider.GetRequiredService<IInventoryStore>().LastProductChanged.Products);
        }

        public void Execute()
        {
            //SetProperty(_newValue);
            CommandHandler(_newValue, ActionType.Normal);
        }

        public bool Undo()
        {
            SetProperty(_oldValue);
            CommandHandler(_oldValue, ActionType.Undo);
            return true;
        }

        public bool Redo()
        {
            SetProperty(_newValue);
            CommandHandler(_newValue, ActionType.Redo);
            return true;
        }

        private void CommandHandler(object parameter, ActionType intent)
        {
            Command(parameter, intent);
            _propertyChangeHandler();
        }

        public abstract bool Command(object parameter, ActionType intent);

        private void SetProperty(object value)
        {
            var prop = typeof(T).GetProperty(_propertyName);
            if (prop != null && prop.CanWrite)
            {
                PropertyChangeTracker.Suppress = true;
                prop.SetValue(_target, value);
                PropertyChangeTracker.Suppress = false;
            }
        }

    }

}
