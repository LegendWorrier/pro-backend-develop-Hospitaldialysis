using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;

namespace Wasenshi.HemoDialysisPro.Services.BusinessLogic
{
    public class ShiftProcessor : IShiftProcessor
    {
        private readonly IConfiguration config;

        public ShiftProcessor(IConfiguration config)
        {
            this.config = config;
        }

        public ShiftResult ProcessSlotData(IEnumerable<UserShift> users, IEnumerable<ShiftSlot> slots)
        {
            var userResult = new List<UserShiftResult>();
            foreach (var user in users)
            {
                userResult.Add(new UserShiftResult
                {
                    UserShift = user,
                    Slots = slots.Where(x => x.UserId == user.UserId).OrderBy(x => x.Date)
                });
            }

            return new ShiftResult
            {
                Users = userResult
            };
        }
    }
}
