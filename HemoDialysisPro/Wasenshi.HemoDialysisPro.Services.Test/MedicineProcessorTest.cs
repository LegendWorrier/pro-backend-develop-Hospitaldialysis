using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Xunit;

namespace Wasenshi.HemoDialysisPro.Services.Test
{
    public class MedicineProcessorTest
    {
        private IFixture _fixture;
        private Mock<IExecutionRecordRepository> _executionRecordRepo;

        private Guid prescriptionId = Guid.NewGuid();

        private List<MedicineRecord> medicineRecords;

        public MedicineProcessorTest()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

            medicineRecords = new List<MedicineRecord>
            {
                new MedicineRecord
                {
                    Id = Guid.NewGuid(),
                    PrescriptionId = prescriptionId,
                    Timestamp = DateTime.UtcNow,
                    IsExecuted = true,
                    Created = DateTime.UtcNow.AddMonths(1)
                },
                new MedicineRecord
                {
                    Id = Guid.NewGuid(),
                    PrescriptionId = prescriptionId,
                    Timestamp = DateTime.UtcNow.AddDays(-2), // day before yesterday
                    IsExecuted = true
                },
                new MedicineRecord
                {
                    Id = Guid.NewGuid(),
                    PrescriptionId = prescriptionId,
                    Timestamp = DateTime.UtcNow.AddDays(-7), // definitely last week
                    IsExecuted = true
                },
                new MedicineRecord
                {
                    Id = Guid.NewGuid(),
                    PrescriptionId = prescriptionId,
                    Timestamp = DateTime.UtcNow.AddDays(-14), // definitely last 2 week
                    IsExecuted = true
                }
            };

            _executionRecordRepo = _fixture.Freeze<Mock<IExecutionRecordRepository>>();
            _executionRecordRepo.Setup(x => x.GetMedicineRecords(false))
                .Returns(medicineRecords.AsQueryable());
        }

        [Fact]
        public void Processor_Should_Verify_Successfully()
        {
            MedicinePrescription prescription = new MedicinePrescription
            {
                Id = prescriptionId,
                Frequency = Frequency.BID
            };

            var sub = _fixture.Create<MedicineRecordProcessor>();
            sub.CheckAvailablity(prescription, out string reason)
                .Should().BeTrue();
        }

        [Fact]
        public void Processor_Should_Fail_Validation_Successfully()
        {
            MedicinePrescription prescription = new MedicinePrescription
            {
                Id = prescriptionId,
                Frequency = Frequency.QD
            };

            var sub = _fixture.Create<MedicineRecordProcessor>();
            sub.CheckAvailablity(prescription, out string reason)
                .Should().BeFalse();

            reason.Should().ContainEquivalentOf("once a day");
        }

        [Fact]
        public void Prescription_Should_Expired()
        {
            MedicinePrescription prescription = new MedicinePrescription
            {
                Id = prescriptionId,
                Frequency = Frequency.BID,
                AdministerDate = DateTime.Today.AddDays(-5),
                Duration = 2
            };

            var sub = _fixture.Create<MedicineRecordProcessor>();
            sub.CheckAvailablity(prescription, out string reason)
                .Should().BeFalse();

            reason.Should().ContainEquivalentOf("expire");
        }

        [Fact]
        public void Prescription_Should_Not_Expired()
        {
            MedicinePrescription prescription = new MedicinePrescription
            {
                Id = prescriptionId,
                Frequency = Frequency.BID,
                AdministerDate = DateTime.Today.AddDays(-5),
                Duration = 6
            };

            var sub = _fixture.Create<MedicineRecordProcessor>();
            sub.CheckAvailablity(prescription, out string reason)
                .Should().BeTrue();

            prescription.AdministerDate = DateTime.Today;
            prescription.Duration = 1;
            sub.CheckAvailablity(prescription, out reason)
                .Should().BeTrue();
        }

        [Theory]
        [InlineData(Frequency.QD, false, "once a day")]
        [InlineData(Frequency.QN, false, "once every night")]
        [InlineData(Frequency.BID, false, "two times a day")]
        [InlineData(Frequency.TID, true, null)]
        [InlineData(Frequency.QID, true, null)]
        public void Processor_Should_Verify_Correctly_WhenHas_NotExecuted(Frequency freq, bool expect, string expectReason)
        {
            medicineRecords.AddRange(new[]
            {
                new MedicineRecord
                {
                    PrescriptionId = prescriptionId,
                    Created = DateTime.UtcNow
                }
            });
            MedicinePrescription prescription = new MedicinePrescription
            {
                Id = prescriptionId,
                Frequency = freq,
            };

            var sub = _fixture.Create<MedicineRecordProcessor>();
            sub.CheckAvailablity(prescription, out string reason)
                .Should().Be(expect);
            if (expectReason != null)
            {
                reason.Should().ContainEquivalentOf(expectReason);
            }
        }

        [Fact]
        public void Prescription_SingleTime_Should_Verify_Successfully()
        {
            MedicinePrescription prescription = new MedicinePrescription
            {
                Id = prescriptionId,
                Frequency = Frequency.ST
            };

            var sub = _fixture.Create<MedicineRecordProcessor>();
            sub.CheckAvailablity(prescription, out string reason)
                .Should().BeFalse();

            reason.Should().ContainEquivalentOf("only once");
        }

        [Theory]
        [InlineData(Frequency.QN, "once every night")]
        [InlineData(Frequency.QD, "once a day")]
        [InlineData(Frequency.BID, "two times a day")]
        [InlineData(Frequency.TID, "three times a day")]
        [InlineData(Frequency.QID, "four times a day")]
        public void Freq_For_Day_Interval_Should_Work_Correctly(Frequency freq, string expectedReason)
        {
            int repeat = Math.Max((int)freq, 1);
            medicineRecords.Clear();
            for (int i = 0; i < repeat; i++)
            {
                medicineRecords.Add(new MedicineRecord
                {
                    PrescriptionId = prescriptionId,
                    Timestamp = DateTime.UtcNow.AddDays(-1), // definitely yesterday
                    IsExecuted = true
                });
            }

            MedicinePrescription prescription = new MedicinePrescription
            {
                Id = prescriptionId,
                Frequency = freq
            };

            var sub = _fixture.Create<MedicineRecordProcessor>();
            sub.CheckAvailablity(prescription, out string reason)
                .Should().BeTrue();

            // move next day
            medicineRecords.ConvertAll(input =>
            {
                input.Timestamp = input.Timestamp.AddDays(1);
                return input;
            });

            sub.CheckAvailablity(prescription, out reason)
                .Should().BeFalse();

            reason.Should().ContainEquivalentOf(expectedReason);

            // take out by 1 record
            medicineRecords.RemoveAt(0);
            sub.CheckAvailablity(prescription, out reason)
                .Should().BeTrue();
        }

        [Fact]
        public void Freq_QOD_Should_Work_Correctly()
        {
            MedicinePrescription prescription = new MedicinePrescription
            {
                Id = prescriptionId,
                Frequency = Frequency.QOD
            };

            var sub = _fixture.Create<MedicineRecordProcessor>();
            sub.CheckAvailablity(prescription, out string reason)
                .Should().BeFalse();

            reason.Should().ContainEquivalentOf("once every other day");
            // move next day
            medicineRecords.ConvertAll(input =>
            {
                input.Timestamp = input.Timestamp.AddDays(-1);
                return input;
            });

            sub.CheckAvailablity(prescription, out reason)
                .Should().BeFalse();

            reason.Should().ContainEquivalentOf("once every other day");
            // move next day again
            medicineRecords.ConvertAll(input =>
            {
                input.Timestamp = input.Timestamp.AddDays(-1);
                return input;
            });

            sub.CheckAvailablity(prescription, out reason)
                .Should().BeTrue();
        }


        [Theory]
        [InlineData(Frequency.QW, "once a week")]
        [InlineData(Frequency.BIW, "two times a week")]
        [InlineData(Frequency.TIW, "three times a week")]
        [InlineData(Frequency.QIW, "four times a week")]
        public void Freq_For_Week_Interval_Should_Work_Correctly(Frequency freq, string expectedReason)
        {
            int repeat = (int)freq + 8;
            medicineRecords.Clear();
            for (int i = 0; i < repeat; i++)
            {
                medicineRecords.Add(new MedicineRecord
                {
                    PrescriptionId = prescriptionId,
                    Timestamp = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.Date.DayOfWeek - 1), // definitely last week ( starting from last saturday, sunday considered new week.)
                    IsExecuted = true
                });
            }

            MedicinePrescription prescription = new MedicinePrescription
            {
                Id = prescriptionId,
                Frequency = freq
            };

            var sub = _fixture.Create<MedicineRecordProcessor>();
            sub.CheckAvailablity(prescription, out string reason)
                .Should().BeTrue("All the meds should be considered last week : {0}",
                    medicineRecords.Select(x => x.Timestamp).Aggregate("", (s, time) => $"{s}, {time}").Trim(','));

            // move to this week
            medicineRecords.ForEach(input =>
            {
                input.Timestamp = input.Timestamp.AddDays(1);
            });

            sub.CheckAvailablity(prescription, out reason)
                .Should().BeFalse("The med in this week should exceed the limit : {0} ({1})", expectedReason, medicineRecords.Count);

            reason.Should().ContainEquivalentOf(expectedReason);

            // take out by 1 record
            medicineRecords.RemoveAt(0);

            if (DateTime.UtcNow.Date.DayOfWeek == DayOfWeek.Sunday)
            {
                if (freq == Frequency.QW) // no more test needed for once a week case
                {
                    return;
                }
                sub.CheckAvailablity(prescription, out reason).Should().BeFalse("Med should not allow more than once a day for week feq");
            }
            else
            {
                sub.CheckAvailablity(prescription, out reason)
                .Should().BeTrue("The med in this week should 'not' exceed the limit : {0} ({1})", expectedReason, medicineRecords.Count);
            }
        }

        [Fact]
        public void Freq_For_2Weeks_Interval_Should_Work_Correctly()
        {
            medicineRecords.Clear();
            medicineRecords.Add(new MedicineRecord
            {
                Id = Guid.NewGuid(),
                PrescriptionId = prescriptionId,
                Timestamp = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.Date.DayOfWeek - 8), // definitely last 2 week ( starting from last saturday, sunday considered new week.)
                IsExecuted = true
            });

            MedicinePrescription prescription = new MedicinePrescription
            {
                Id = prescriptionId,
                Frequency = Frequency.Q2W
            };

            var sub = _fixture.Create<MedicineRecordProcessor>();
            sub.CheckAvailablity(prescription, out string reason)
                .Should().BeTrue();

            // move next day
            medicineRecords.ForEach(input =>
            {
                input.Timestamp = input.Timestamp.AddDays(1);
            });

            sub.CheckAvailablity(prescription, out reason)
                .Should().BeFalse();

            reason.Should().ContainEquivalentOf("once every other week");
        }

        [Fact]
        public void Freq_For_3Weeks_Interval_Should_Work_Correctly()
        {
            medicineRecords.Clear();
            medicineRecords.Add(new MedicineRecord
            {
                Id = Guid.NewGuid(),
                PrescriptionId = prescriptionId,
                Timestamp = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.Date.DayOfWeek - 15), // definitely last 3 week ( starting from last saturday, sunday considered new week.)
                IsExecuted = true
            });

            MedicinePrescription prescription = new MedicinePrescription
            {
                Id = prescriptionId,
                Frequency = Frequency.Q3W
            };

            var sub = _fixture.Create<MedicineRecordProcessor>();
            sub.CheckAvailablity(prescription, out string reason)
                .Should().BeTrue();

            // move next day
            medicineRecords.ForEach(input =>
            {
                input.Timestamp = input.Timestamp.AddDays(1);
            });

            sub.CheckAvailablity(prescription, out reason)
                .Should().BeFalse();

            reason.Should().ContainEquivalentOf("once every 3 weeks");
        }

        [Fact]
        public void Freq_For_4Weeks_Interval_Should_Work_Correctly()
        {
            medicineRecords.Clear();
            medicineRecords.Add(new MedicineRecord
            {
                Id = Guid.NewGuid(),
                PrescriptionId = prescriptionId,
                Timestamp = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.Date.DayOfWeek - 22), // definitely last 4 week ( starting from last saturday, sunday considered new week.)
                IsExecuted = true
            });

            MedicinePrescription prescription = new MedicinePrescription
            {
                Id = prescriptionId,
                Frequency = Frequency.Q4W
            };

            var sub = _fixture.Create<MedicineRecordProcessor>();
            sub.CheckAvailablity(prescription, out string reason)
                .Should().BeTrue();

            // move next day
            medicineRecords.ForEach(input =>
            {
                input.Timestamp = input.Timestamp.AddDays(1);
            });

            sub.CheckAvailablity(prescription, out reason)
                .Should().BeFalse();

            reason.Should().ContainEquivalentOf("once every 4 weeks");
        }

        [Fact]
        public void Timezone_Should_Work_Correctly()
        {
            MedicinePrescription prescription = new MedicinePrescription
            {
                Id = prescriptionId,
                Frequency = Frequency.BID
            };

            var tzList = TimeZoneInfo.GetSystemTimeZones();

            TimeZoneInfo tz = tzList.FirstOrDefault(x => x.BaseUtcOffset == TimeSpan.FromHours(7));

            var sub = _fixture.Create<MedicineRecordProcessor>();
            sub.CheckAvailablity(prescription, out string reason, tz)
                .Should().BeTrue();

            tz = tzList.FirstOrDefault(x => x.BaseUtcOffset == TimeSpan.FromHours(-8));
            sub.CheckAvailablity(prescription, out reason, tz)
                .Should().BeTrue();

            tz = tzList.FirstOrDefault(x => x.BaseUtcOffset == TimeSpan.FromHours(12));
            sub.CheckAvailablity(prescription, out reason, tz)
                .Should().BeTrue();
        }
    }
}
