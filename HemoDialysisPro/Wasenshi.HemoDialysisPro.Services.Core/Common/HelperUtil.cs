global using Wasenshi.HemoDialysisPro.Constants;
using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using System.Collections.Generic;


namespace Wasenshi.HemoDialysisPro.Services
{
    public static class HelperUtil
    {
        public static Guid[] GetNurseInShift(this HemodialysisRecord hemosheet, IShiftUnitOfWork shiftUnit, IUserInfoService userService, IPatientRepository patientRepo, TimeZoneInfo tz, bool forceUpdate = false)
        {
            var patient = patientRepo.Get(hemosheet.PatientId);
            return GetNurseInShift(hemosheet, shiftUnit, userService, patient, tz, forceUpdate);
        }

        /// <summary>
        /// (No patient check. Passing wrong patient might result in an invalid data.)
        /// </summary>
        /// <param name="hemosheet"></param>
        /// <param name="shiftUnit"></param>
        /// <param name="patient"></param>
        /// <param name="tz"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static Guid[] GetNurseInShift(this HemodialysisRecord hemosheet, IShiftUnitOfWork shiftUnit, IUserInfoService userService, Patient patient, TimeZoneInfo tz, bool forceUpdate = false)
        {
            if (!forceUpdate && (hemosheet.NursesInShift?.Length > 0 || hemosheet.CompletedTime.HasValue))
            {
                return hemosheet.NursesInShift;
            }

            var section = hemosheet.ShiftSectionId == 0 ? GetUpdatedShiftSection(hemosheet, shiftUnit, patient, tz) :
                            shiftUnit.Section.GetAll(false).FirstOrDefault(x => x.Id == hemosheet.ShiftSectionId) ??
                            GetUpdatedShiftSection(hemosheet, shiftUnit, patient, tz);
            if (section != null)
            {
                var sections = shiftUnit.Section.GetAll(false)
                    .Where(x => x.UnitId == section.UnitId)
                    .OrderBy(x => x.StartTime)
                    .ToList();
                int sectionIndex = sections.IndexOf(sections.First(x => x.Id == section.Id));
                ShiftData shift;
                switch (sectionIndex)
                {
                    case 0:
                        shift = ShiftData.Section1;
                        break;
                    case 1:
                        shift = ShiftData.Section2;
                        break;
                    case 2:
                        shift = ShiftData.Section3;
                        break;
                    case 3:
                        shift = ShiftData.Section4;
                        break;
                    case 4:
                        shift = ShiftData.Section5;
                        break;
                    case 5:
                        shift = ShiftData.Section6;
                        break;
                    default:
                        throw new InvalidOperationException("Cannot find matching section!");
                }

                DateOnly target = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(new DateTimeOffset(hemosheet.CycleStartTime ?? DateTime.UtcNow, TimeSpan.Zero), tz).DateTime);

                var shiftSlots = shiftUnit.ShiftSlot.GetAll().Where(x =>
                    x.Date == target &&
                    x.ShiftMeta != null &&
                    x.ShiftMeta.ScheduleMeta.UnitId == section.UnitId)
                    .ToList();
                shiftSlots = shiftSlots.Where(x => x.Data.HasFlag(shift)).ToList();

                // filter out suspended user
                var userShifts = shiftUnit.UserShift.GetAll().Where(x =>
                    x.Month.Year == target.Year &&
                    x.Month.Month == target.Month)
                    .ToList();
                shiftSlots = shiftSlots.Where(x => !userShifts.FirstOrDefault(u => u.UserId == x.UserId)?.Suspended ?? true).ToList();
                var nurseIds = shiftSlots.Select(s => s.UserId).ToArray();
                // Default ordering by role (HeadNurse, then Nurse, then PN)
                var users = userService.GetAllUsers(x => nurseIds.Contains(x.Id)).ToList();
                users.Sort(new UserRolesComparer());

                return users.Select(x => x.User.Id).ToArray();
            }
            else
            {
                return Array.Empty<Guid>();
            }
        }

        /// <summary>
        /// Get corresponding shift section id from current CycleStartTime value.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static ScheduleSection GetUpdatedShiftSection(this HemodialysisRecord record, IShiftUnitOfWork shiftUnit, IPatientRepository patientRepo, TimeZoneInfo tz)
        {
            var patient = patientRepo.Get(record.PatientId);
            return GetUpdatedShiftSection(record, shiftUnit, patient, tz);
        }

        /// <summary>
        /// Get corresponding shift section id from current CycleStartTime value. (No patient check. Passing wrong patient might result in an invalid data.)
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static ScheduleSection GetUpdatedShiftSection(this HemodialysisRecord record, IShiftUnitOfWork shiftUnit, Patient patient, TimeZoneInfo tz)
        {
            var sections = shiftUnit.Section.GetAll(false).Where(x => x.UnitId == patient.UnitId).OrderBy(x => x.StartTime).ToList();

            if (record.CycleStartTime == null)
            {
                return null;
            }

            var tzTime = TimeZoneInfo.ConvertTime(new DateTimeOffset(record.CycleStartTime.Value), tz);
            var startTime = TimeOnly.FromTimeSpan(tzTime.TimeOfDay);
            foreach (var section in sections)
            {
                var sectionEndTime = section.StartTime.AddHours(4);
                if (startTime.IsBetween(section.StartTime, sectionEndTime))
                {
                    return section;
                }
            }

            return sections.LastOrDefault();
        }
    }

    internal sealed class UserRolesComparer : IComparer<UserResult>
    {
        public int Compare(UserResult x, UserResult y)
        {
            int roleCompare = GetRoleOrder(x.Roles).CompareTo(GetRoleOrder(y.Roles));
            if (roleCompare != 0)
            {
                return roleCompare;
            }
            // if same level, then compare with name
            return x.User.FirstName.CompareTo(y.User.FirstName);
        }

        private int GetRoleOrder(IList<string> roles)
        {
            if (roles.Contains(Roles.HeadNurse))
            {
                return 0;
            }
            if (roles.Contains(Roles.Nurse))
            {
                return 1;
            }
            if (roles.Contains(Roles.PN))
            {
                return 2;
            }
            // in case user is not nurse, he will be listed last. (but should not occur)
            return 3;
        }
    }
}
