using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IScheduleService : IApplicationService
    {
        ScheduleSection GetSection(int sectionId);
        IEnumerable<ScheduleSection> GetSections(int unitId);
        IEnumerable<TempSection> GetTempSections(int unitId);
        bool CreateOrUpdateTempSections(int unitId, IEnumerable<TempSection> tempSections);
        void ClearTempSections(int unitId);
        void ApplyTempSections(int unitId);

        /// <summary>
        /// Create or update a set of sections for the specifying hemo unit. If any sections already existed, they get updated.
        /// Mark for delete can be used to remove existing one(s) from the set.
        /// <br></br>
        /// (note: minimum gap between each sections is 4 hours)
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="sections"></param>
        /// <param name="deletes"></param>
        /// <returns></returns>
        int CreateOrUpdateSections(int unitId, IEnumerable<ScheduleSection> sections, IEnumerable<ScheduleSection> deletes);
        /// <summary>
        /// Slot the patient into section slot. If the patient is already slotted in the same day, move to this new slot.
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="slot"></param>
        /// <param name="patientId"></param>
        /// <returns></returns>
        int SlotPatientSchedule(int sectionId, SectionSlots slot, string patientId);
        /// <summary>
        /// Swap two patient slot. If the second slot has no patient, move the first slot to second slot instead.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        bool SwapSlot(SectionSlotPatient first, SectionSlotPatient second);
        /// <summary>
        /// Delete the slotted patient.
        /// </summary>
        /// <param name="patientId"></param>
        /// <param name="sectionId"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        bool DeletePatientSlot(string patientId, int sectionId, SectionSlots slot);
        /// <summary>
        /// Use this for receiving schedule view for FE.
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        ScheduleResult GetSchedule(int unitId);
        /// <summary>
        /// Temporary override the specific section slot schedule(s). Can only reschedule the upcoming schedule, not the past.
        /// </summary>
        /// <param name="schedules"></param>
        /// <returns></returns>
        IEnumerable<Schedule> Reschedule(IEnumerable<Schedule> schedules);
        /// <summary>
        /// Get all schedules for a unit. (including old schedules and cross unit patients)
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        IEnumerable<Schedule> GetReschedules(int unitId, Expression<Func<Schedule, bool>> whereCondition = null);
        /// <summary>
        /// Get a list of current active scheduled patients for a unit. (including cross unit patients, but not old schedules).
        /// <br></br>
        /// This API automatically normalize the data for you and include only the upcoming schedule for each patients.
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        IEnumerable<Schedule> GetActiveSchedules(int unitId, Expression<Func<Schedule, bool>> whereCondition = null);
        /// <summary>
        /// Check whether the patient has active cross schedule or not.
        /// </summary>
        /// <param name="patientId"></param>
        /// <returns></returns>
        bool IsCrossScheduleExisted(string patientId);
        /// <summary>
        /// Check on the unit whether it has any schedule yet.
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        bool IsScheduleEmpty(int unitId);
        Schedule GetPatientScheduleById(Guid id);

        /// <summary>
        /// Check for invalid request
        /// </summary>
        /// <param name="schedule"></param>
        void VerifySchedule(Schedule schedule);
        bool DeleteSchedule(Guid scheduleId);
        // ================== For Today Patients ============================
        IEnumerable<Schedule> GetActiveSchedulesForToday(IEnumerable<int> unitList, Expression<Func<Schedule, bool>> whereCondition = null);
        IEnumerable<SectionSlotPatient> GetActiveSlotForToday(IEnumerable<int> unitList, Expression<Func<SectionSlotPatient, bool>> whereCondition = null);

        ScheduleCheck PatientCheckForToday(string patientId);
    }
}