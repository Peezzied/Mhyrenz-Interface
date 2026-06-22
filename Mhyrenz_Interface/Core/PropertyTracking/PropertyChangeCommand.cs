using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Core.PropertyTracking
{

    public abstract class PropertyChangeCommand<RowInfo> : UndoRedoBoundCommand
    {
        private readonly TrackPropertyHelper.Setter _setter;
        private readonly Action _propertyChangeHandler;

        public ChangedArgs PropertyChangedArgs { get; set; }

        public PropertyChangeCommand(DTO dto, Type context): base(context)
        {
            PropertyChangedArgs = dto.ChangedArgs;
            _setter = dto.Setter;
            _propertyChangeHandler = dto.PropertyChangeHandler;
        }

        public override async Task Command()
        {
            switch (Intent)
            {
                case ActionType.Undo:
                    _setter(PropertyChangedArgs.NewValue, PropertyChangeOrigin.UndoRedo);
                    break;
                case ActionType.Redo:
                    _setter(PropertyChangedArgs.OldValue, PropertyChangeOrigin.UndoRedo);
                    break;
            }
            _propertyChangeHandler?.Invoke();
        }

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
        }
    }
}
