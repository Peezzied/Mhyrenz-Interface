using System;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace Mhyrenz_Interface.Commands
{

    public abstract class PropertyChangeCommand<R> : IUndoableCommand, IPropertyChangedCommand
    {
        private readonly TrackPropertyHelper.Setter _setter;
        private readonly Action _propertyChangeHandler;

        public Action<NavigationViewModel> SideEffect { get; set; }
        public Type CurrentViewIn { get; }

        public ChangedArgs PropertyChangedArgs { get; set; }

        public PropertyChangeCommand(ChangedArgs args, TrackPropertyHelper.Setter setter, Action propertyChangeHandler, Type currentViewIn)
        {
            PropertyChangedArgs = args;
            _setter = setter;
            _propertyChangeHandler = propertyChangeHandler;

            CurrentViewIn = currentViewIn;
        }

        public void Execute()
        {
            CommandHandler(PropertyChangedArgs.NewValue, ActionType.Normal);
        }

        public bool Undo()
        {
            _setter(PropertyChangedArgs.OldValue, PropertyChangeOrigin.UndoRedo);
            CommandHandler(PropertyChangedArgs.OldValue, ActionType.Undo);
            return true;
        }

        public bool Redo()
        {
            _setter(PropertyChangedArgs.NewValue, PropertyChangeOrigin.UndoRedo);
            CommandHandler(PropertyChangedArgs.NewValue, ActionType.Redo);
            return true;
        }

        private void CommandHandler(object parameter, ActionType intent)
        {
            Command(parameter, intent);
            _propertyChangeHandler();
        }

        public abstract bool Command(object parameter, ActionType intent);

        public class ChangedArgs
        {
            public object OldValue { get; set; }
            public object NewValue { get; set; }
            public R RowInfo { get; set; }
        }
    }

    public interface IPropertyChangedCommand
    {
    }
}
