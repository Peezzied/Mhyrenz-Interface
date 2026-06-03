using System;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace Mhyrenz_Interface.Commands
{

    public abstract class PropertyChangeCommand<RowInfo> : IUndoableCommand, IPropertyChangedCommand
    {
        private readonly TrackPropertyHelper.Setter _setter;
        private readonly Action _propertyChangeHandler;

        public Action<NavigationViewModel> SideEffect { get; set; }
        public Type CurrentViewIn { get; }

        public ChangedArgs PropertyChangedArgs { get; set; }

        public PropertyChangeCommand(DTO dto)
        {
            PropertyChangedArgs = dto.ChangedArgs;
            _setter = dto.Setter;
            _propertyChangeHandler = dto.PropertyChangeHandler;

            CurrentViewIn = dto.CurrentViewIn;
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

        public abstract void Command(object parameter, ActionType intent);

        public class ChangedArgs
        {
            public object OldValue { get; set; }
            public object NewValue { get; set; }
            public RowInfo RowInfo { get; set; }
        }

        public class DTO
        {
            public ChangedArgs ChangedArgs { get; set; }
            public TrackPropertyHelper.Setter Setter { get; set; }
            public Action PropertyChangeHandler { get; set; }
            public Type CurrentViewIn { get; set; }
        }
    }

    public interface IPropertyChangedCommand
    {
    }
}
