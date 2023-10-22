using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Web.Api.WebSocket
{
    internal static class HemoBoxHubHelpers
    {
        public struct RequiredService
        {
            public MonitorPool MonitorPool { get; set; }
            public IMasterDataService Master { get; set; }
        }
        public static RequiredService GetServiceArg(MonitorPool monitor, IMasterDataService master)
        {
            return new RequiredService { MonitorPool = monitor, Master = master };
        }
        public static BedBoxInfo UpdateBedWithNewConnectionId(RegisterBoxReq info, string connectionId, RequiredService services)
        {
            bool invalidUnit = info.UnitId.HasValue && !services.MonitorPool.UnitListFromCache().Any(x => x.Id == info.UnitId);
            BedBoxInfo bed = services.MonitorPool.GetBedByMacAddress(info.MacAddress);
            if (bed == null)
            {
                bed = new BedBoxInfo
                {
                    MacAddress = info.MacAddress,
                    Name = info.Name,
                    PatientId = info.PatientId,
                    Patient = info.Patient,
                    UnitId = invalidUnit ? null : info.UnitId,
                    IsRegistered = false // TODO: all registered beds must be loaded up by system init before accepting any connection from hemo box
                };
                if (bed.Patient != null)
                {
                    bed.Patient.UnitId = bed.UnitId ?? 0;
                }
            }
            else
            {
                bed.Name = info.Name;
                bed.PatientId = info.PatientId;
                bed.Patient = info.Patient;
                bed.UnitId = invalidUnit ? null : info.UnitId;
                if (bed.Patient != null)
                {
                    bed.Patient.UnitId = bed.UnitId ?? 0;
                }
            }
            bed.Online = true;
            bed.Sending = info.Sending;

            services.MonitorPool.UpdateConnectionId(bed, connectionId);

            return bed;
        }

        public static async Task DispatchHemoBoxUpdateEvent(BedBoxInfo bed, IHubContext<UserHub, IUserClient> users, IMapper mapper, Func<IUserClient, Task> targetEvent = null)
        {
            IUserClient targets = GetDispatchTarget(bed.UnitId, users);
            if (targetEvent != null)
            {
                await targetEvent(targets);
            }
            else
            {
                await targets.BedUpdate(mapper.Map<BedViewModel>(bed));
            }
        }

        public static async Task DispatchHemoBoxUpdateEvent(BedBoxInfo bed, IHubCallerClients<IUserClient> clients, IMapper mapper, Func<IUserClient, Task> targetEvent = null)
        {
            IUserClient targets = GetDispatchTarget(bed.UnitId, clients);
            IUserClient rootAdmins = clients.Group(UserHub.ROOT_ADMIN); // root admin needs separate channel and call (because technically, he doesn't belong to any units)
            if (targetEvent != null)
            {
                await targetEvent(targets);
                if (bed.UnitId.HasValue)
                {
                    await targetEvent(rootAdmins);
                }
            }
            else
            {
                await targets.BedUpdate(mapper.Map<BedViewModel>(bed));
                if (bed.UnitId.HasValue)
                {
                    await rootAdmins.BedUpdate(mapper.Map<BedViewModel>(bed));
                }
            }
        }

        public static IUserClient GetDispatchTarget(int? unitId, IHubContext<UserHub, IUserClient> users)
        {
            IUserClient targets = unitId.HasValue ?
                users.Clients.Groups(UserHub.GetUnitChannelName(unitId.Value), UserHub.ROOT_ADMIN) :
                users.Clients.Group(UserHub.NO_UNIT_CHANNEL); // root admin already in no unit channel
            return targets;
        }

        public static IUserClient GetDispatchTarget(int? unitId, IHubCallerClients<IUserClient> clients)
        {
            IUserClient targets = unitId.HasValue ?
                clients.OthersInGroup(UserHub.GetUnitChannelName(unitId.Value)) :
                clients.OthersInGroup(UserHub.NO_UNIT_CHANNEL);
            return targets;
        }
    }
}