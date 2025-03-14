using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntoslikBot.Interfaces
{
    internal interface IAutoModeService
    {
        Task getHelp(SocketMessage msg);
        Task MarkFittingMessages(SocketMessage msg);
        Task TotalMute(SocketMessage msg);
    }
}
