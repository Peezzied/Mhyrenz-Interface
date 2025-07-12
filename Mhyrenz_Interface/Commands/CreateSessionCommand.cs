using HandyControl.Controls;
using HandyControl.Data;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SessionService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Commands
{
    public class CreateSessionCommand : BaseAsyncCommand
    {
        private readonly ISessionStore _sessionStore;
        private ISessionService _sessionService;
        private readonly SessionBoxContext _viewModel;

        public CreateSessionCommand(SessionBoxContext vm, ISessionService sessionService, ISessionStore sessionStore)
        {
            _sessionStore = sessionStore;
            _sessionService = sessionService;
            _viewModel = vm;
        }

        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter)
                && Validator.TryValidateObject(_viewModel, new ValidationContext(_viewModel), null, validateAllProperties: true);
        }
        public override async Task ExecuteAsync(object parameter)
        {
            await App.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                if (!Validator.TryValidateObject(_viewModel, new ValidationContext(_viewModel), null, validateAllProperties: true))
                    return;
                _sessionStore.CurrentSession = await _sessionService.GenerateSession(new Session { Period = DateTime.Now });

                Growl.Success(new GrowlInfo
                {
                    Message = $"Session \"{_sessionStore.CurrentSession.Period:D}\" has been created successfully!",
                    ShowDateTime = false,
                });

                _viewModel.IsSessionBox = false;
                _viewModel.RaiseSessionCreate();
            }), System.Windows.Threading.DispatcherPriority.Input);

        }
    }
}
