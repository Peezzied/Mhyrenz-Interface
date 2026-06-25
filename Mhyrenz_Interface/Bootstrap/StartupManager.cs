using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HandyControl.Controls;

namespace Mhyrenz_Interface.Bootstrap
{
    public class StartupManager
    {
        public struct Action
        {
            public string EventName { get; set; }
            public string Output { get; set; }
            public Func<IServiceProvider, Task<string>> Method { get; set; }
            public Action(string name, string output, Func<IServiceProvider, Task<string>> action)
            {
                EventName = name;
                Method = action;
                Output = output;
            }

            public Action(string name, string output, Func<IServiceProvider, Task> action)
            {
                EventName = name;
                // FIXME: this undermines the Task of the action
                Method = async (p) =>
                {
                    await action(p);
                    return await Task.FromResult(string.Empty);
                };
                Output = output;
            }
        }

        private static readonly Queue<Action> _actions = new Queue<Action>();

        public static void Register(Action startupAction)
        {
            _actions.Enqueue(startupAction);
        }

        public static async Task<IServiceProvider> Init(IServiceProvider provider, SplashWindow splashWindow)
        {
            while (_actions.Count > 0)
            {
                var item = _actions.Dequeue();
                splashWindow.AddMessage($"{item.EventName}: {item.Output}...");
                var outputEffect = await item.Method(provider);
                if (outputEffect != string.Empty)
                    splashWindow.AddMessage($"{item.EventName}: {outputEffect}...");
            }
            splashWindow.AddMessage($"Done!");

            return provider;
        }
    }
}