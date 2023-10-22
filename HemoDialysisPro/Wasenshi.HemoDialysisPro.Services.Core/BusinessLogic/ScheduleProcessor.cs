using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Services.Interfaces;

namespace Wasenshi.HemoDialysisPro.Services.BusinessLogic
{
    public class ScheduleProcessor : IScheduleProcessor
    {
        private readonly IConfiguration config;

        public ScheduleProcessor(IConfiguration config)
        {
            this.config = config;
        }

        public ScheduleResult ProcessSlotData(IEnumerable<ScheduleSection> sections, IEnumerable<SectionSlotPatient> slots)
        {
            var sectionResults = new List<SectionResult>();
            foreach (var section in sections)
            {
                sectionResults.Add(new SectionResult
                {
                    Section = section,
                    Slots = Enumerable.Range(0, 7).Select(i => new ScheduleSlot
                    {
                        Slot = (SectionSlots)i,
                        Section = section,
                        SectionId = section.Id
                    }).ToArray()
                });
            }

            var groups = slots.GroupBy(x => x.SectionId).ToList();

            if (groups.Count == 0)
            {
                foreach (var item in sectionResults)
                {
                    foreach (var slot in item.Slots)
                    {
                        slot.PatientList = Enumerable.Empty<SectionSlotPatient>();
                    }
                }
                return new ScheduleResult
                {
                    Sections = sectionResults
                };
            }

            foreach (var section in sectionResults)
            {
                var slotList = groups.FirstOrDefault(x => x.Key == section.Section.Id);
                if (slotList == null)
                {
                    foreach (var slot in section.Slots)
                    {
                        slot.PatientList = Enumerable.Empty<SectionSlotPatient>();
                    }

                    continue;
                }

                foreach (var slot in section.Slots)
                {
                    slot.PatientList = slotList
                        .Where(x => x.Slot == slot.Slot)
                        .OrderBy(x => x.Patient.Name);
                }
            }

            return new ScheduleResult
            {
                Sections = sectionResults,
                UnitId = sections.First().UnitId
            };
        }
    }
}
