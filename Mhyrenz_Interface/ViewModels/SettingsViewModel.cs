﻿using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.ViewModels.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.ViewModels
{
    public class SettingsViewModel : NavigationViewModel
    {
        public SettingsViewModel(INavigationServiceEx navigationServiceEx) : base(navigationServiceEx)
        {
        }
    }
}
