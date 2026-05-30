using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Core
{
    public class TrackPropertyHelper
    {
        public delegate void Setter(object value, PropertyChangeOrigin origin = PropertyChangeOrigin.Programmatic);
        public delegate object Getter();
        public delegate void Handler(Setter setter, Getter getter, string propertyName);

        private readonly Type _type;
        private readonly TrackedViewModel _obj;
        private readonly string _propertyName;

        public TrackPropertyHelper(Type type, TrackedViewModel obj, string propertyName)
        {
            _type = type;
            _obj = obj;
            _propertyName = propertyName;
        }

        public TrackPropertyHelper Track(string propertyName, Handler handler)
        {
            if (_propertyName != propertyName)
                return this;

            void setter(object val, PropertyChangeOrigin origin)
            {
                _obj.TrackingOrigin = origin;
                _type.GetProperty(propertyName).SetValue(_obj, val);
                _obj.TrackingOrigin = default;
            }

            object getter()
            {
                return _type.GetProperty(propertyName).GetValue(_obj);
            }

            handler(setter, getter, propertyName);
            return this;
        }

        public static TrackPropertyHelper Build(TrackedViewModel obj, string propertyName)
        {
            return new TrackPropertyHelper(obj.GetType(), obj, propertyName);
        }
    }
}
