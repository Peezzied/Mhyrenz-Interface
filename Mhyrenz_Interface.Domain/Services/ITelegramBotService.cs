using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ITelegramBotService
    {
        Task SendMessage(string message);
    }
}
