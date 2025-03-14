using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntoslikBot.Interfaces
{
    internal interface IManualModeService
    {
        string? Readline { get; }
        Task Text();
        Task SeeUsersInVoiceChannels();
        Task SeeAllRoles();
        Task ChangeRoles();
    }
}
