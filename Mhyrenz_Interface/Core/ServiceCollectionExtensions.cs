using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mhyrenz_Interface.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddViewModelFactory<TViewModel, TDTO>(
            this IServiceCollection services)
            where TViewModel : class
            where TDTO : class
        {
            services.AddSingleton<CreateViewModel<TViewModel>>(s =>
            {
                return parameters =>
                {
                    if (parameters.Length == 1 && parameters[0] is TDTO dto)
                        return ActivatorUtilities.CreateInstance<TViewModel>(s, dto);

                    throw new ArgumentException(
                        $"Expected a single {typeof(TDTO).Name} parameter " +
                        $"for {typeof(TViewModel).Name} creation.");
                };
            });

            return services;
        }

        public static IServiceCollection AddViewModelFactory<TViewModel>(
            this IServiceCollection services)
            where TViewModel : class
        {
            services.AddSingleton<CreateViewModel<TViewModel>>(s =>
            {
                return parameters =>
                    ActivatorUtilities.CreateInstance<TViewModel>(
                        s, parameters ?? Array.Empty<object>());
            });

            return services;
        }

        public static IServiceCollection AddViewModelFactory<TViewModel>(
            this IServiceCollection services,
            bool resolveFromContainer)
            where TViewModel : class
        {
            services.AddSingleton<CreateViewModel<TViewModel>>(s =>
                _ => ActivatorUtilities.CreateInstance<TViewModel>(s));

            return services;
        }
    }
}
