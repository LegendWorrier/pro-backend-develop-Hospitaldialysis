using ServiceStack.Messaging;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Share.GlobalEvents;

namespace Wasenshi.HemoDialysisPro.Share
{
    public class MonitorPool
    {
        public static readonly string BED_LIST = "BedBoxList";
        public static readonly string BED_CONNECTION_MAPS = "BedConnectionMaps";

        public static readonly string ALERT_HASH = "AlertRecords";

        public static string GetAlertList(string macAddress) => $"{ALERT_HASH}-{macAddress}";

        public IRedisClient Redis { get; }

        public static readonly string UNIT_CACHE = "UnitsCache";

        public static MonitorPool Create(IRedisClient redis)
        {
            var pool = new MonitorPool(redis);
            return pool;
        }

        private MonitorPool(IRedisClient redis)
        {
            Redis = redis;
        }

        public IEnumerable<Unit> UnitListFromCache()
        {
            return Redis.As<Unit>().GetAll();
        }

        public static void UpdateUnitCache(IMessage<UnitUpdated> message, IRedisClient redis)
        {
            var data = message.GetBody().Data;
            var remove = message.GetBody().Remove;
            if (remove)
            {
                redis.As<Unit>().DeleteById(data.Id);
            }
            else
            {
                redis.As<Unit>().Store(data);
            }
        }

        private IRedisHash<string, BedBoxInfo> Beds => Redis.As<BedBoxInfo>().GetHash<string>(BED_LIST);
        private IRedisHash ConnectionMaps => Redis.Hashes[BED_CONNECTION_MAPS];

        public void AddOrUpdateBed(BedBoxInfo bed, string connectionId = null)
        {
            if (!string.IsNullOrEmpty(connectionId))
            {
                bed.ConnectionId = connectionId;
            }
            Redis.As<BedBoxInfo>().SetEntryInHash(Beds, bed.MacAddress, bed);
            if (!string.IsNullOrEmpty(connectionId))
            {
                Redis.SetEntryInHash(BED_CONNECTION_MAPS, connectionId, bed.MacAddress);
                // ConnectionMaps[connectionId] = bed.MacAddress;
            }
        }

        public void UpdateConnectionId(BedBoxInfo bed, string connectionId)
        {
            if (Redis.ContainsKey(BED_CONNECTION_MAPS))
            {
                if (!string.IsNullOrWhiteSpace(bed.ConnectionId) && ConnectionMaps.ContainsKey(bed.ConnectionId))
                {
                    ConnectionMaps.Remove(bed.ConnectionId);
                }
            }

            AddOrUpdateBed(bed, connectionId);
        }

        /// <summary>
        /// Should never need to use this.
        /// </summary>
        /// <param name="bed"></param>
        public void RemoveBed(BedBoxInfo bed)
        {
            ConnectionMaps.Remove(bed.ConnectionId);
            Redis.As<BedBoxInfo>().RemoveEntryFromHash(Beds, bed.MacAddress);
        }

        public void RemoveBed(string connectionId)
        {
            ConnectionMaps.Remove(connectionId, out string macAddress);
            Redis.As<BedBoxInfo>().RemoveEntryFromHash(Beds, macAddress);
        }

        public BedBoxInfo GetBedByConnectionId(string connectionId)
        {
            if (!ConnectionMaps.ContainsKey(connectionId))
            {
                return null;
            }
            return Beds[ConnectionMaps[connectionId]];
        }

        public BedBoxInfo GetBedByMacAddress(string macAddress)
        {
            return Beds[macAddress];
        }

        public BedBoxInfo GetBedByPatientId(string patientId)
        {
            return Redis.As<BedBoxInfo>().GetHashValues(Beds).FirstOrDefault(x => x.PatientId == patientId);
        }

        public string GetConnectionId(string macAddress)
        {
            return GetBedByMacAddress(macAddress)?.ConnectionId;
        }

        // ====================== Alert ==============================

        public void ClearAlertRecord()
        {
            var all = Redis.GetAllItemsFromSet(ALERT_HASH);
            foreach (var item in all)
            {
                Redis.Remove(item);
            }
            Redis.Remove(ALERT_HASH);
        }

        public void AddAlert(string macAddress, Alarm type)
        {
            var info = new AlertInfo
            {
                Timestamp = DateTime.UtcNow,
                Type = type
            };

            var key = GetAlertList(macAddress);
            Redis.AddItemToSet(ALERT_HASH, key);
            var list = Redis.As<AlertInfo>().Lists[key];
            Redis.As<AlertInfo>().PrependItemToList(list, info);
        }

        public IDictionary<string, ICollection<AlertInfo>> Alerts
        {
            get
            {
                var all = Redis.GetAllItemsFromSet(ALERT_HASH);
                return all.ToDictionary(k => k, key => (ICollection<AlertInfo>)Redis.As<AlertInfo>().GetAllItemsFromList(Redis.As<AlertInfo>().Lists[key]));
            }
        }

        public IEnumerable<BedBoxInfo> BedList
        {
            get
            {
                return Redis.As<BedBoxInfo>().GetHashValues(Beds);
            }
        }

        /// <summary>
        /// Use for the first start of server
        /// </summary>
        public void ClearConnectionList()
        {
            if (Redis.ContainsKey(BED_CONNECTION_MAPS))
            {
                ConnectionMaps.Clear();
            }
        }
    }

    public static class MonitorPoolRedisExtension
    {
        public static MonitorPool GetMonitorPool(this IRedisClient redis)
        {
            return MonitorPool.Create(redis);
        }
    }
}
