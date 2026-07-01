using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mhyrenz_Interface.Shared.Commands
{
    public class ControlCommands
    {
        public static ToggleActivateMainWindow ToggleActivateMainWindow { get; } = new ToggleActivateMainWindow();
    }
}
