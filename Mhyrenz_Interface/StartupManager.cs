﻿using HandyControl.Controls;
using Mhyrenz_Interface.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Mhyrenz_Interface
{
    public struct StartupAction
    {
        public string EventName { get; set; }
        public string Output { get; set; }
        public Func<IServiceProvider, Task> Action { get; set; }
        public StartupAction(string name, string output, Func<IServiceProvider, Task> action)
        {
            EventName = name;
            Action = action;
            Output = output;
        }
    }

    public class StartupManager
    {
        private static readonly Queue<StartupAction> _actions = new Queue<StartupAction>();

        public static void Register(StartupAction startupAction)
        {
            _actions.Enqueue(startupAction);
        }

        public static async Task<IServiceProvider> Init(IServiceProvider provider, SplashWindow splashWindow)
        {
            while (_actions.Count > 0)
            {
                var item = _actions.Dequeue();
                splashWindow.AddMessage($"{item.EventName}: {item.Output}...");
                await item.Action(provider);
            }
            splashWindow.AddMessage($"Done!");

            return provider;
        }
    }
}