using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Test.Fixture;
using Wasenshi.HemoDialysisPro.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Wasenshi.HemoDialysisPro.Test
{
    public class PatientControllerTest : ControllerTestBase, IClassFixture<EnvironmentFixture>
    {
        public PatientControllerTest(EnvironmentFixture env, ITestOutputHelper output) : base(env, output)
        {
        }

        [Fact]
        public async Task Get_PatientList_Should_Success()
        {
            _client.WithBasicUserToken();
            var response = await _client.GetAsync($"/api/Patients");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var resTxt = await response.Content.ReadAsStringAsync();
            JToken result = JToken.Parse(resTxt);
            result.Value<int>("count").Should().BeGreaterThan(-1);
        }

        [Theory]
        [InlineData(Roles.Admin)]
        [InlineData(Roles.Doctor)]
        [InlineData(Roles.HeadNurse)]
        [InlineData(Roles.Nurse)]
        public async Task Post_AnyUserShould_BeAbleTo_CreateNewPatient(string role)
        {
            CreatePatientViewModel request = new CreatePatientViewModel { Name = _fixture.Create<string>(), BirthDate = new DateTime(1992, 11, 5), Id = _fixture.Create<string>(), HospitalNumber = "HN", IdentityNo = "123456789", UnitId = -1 };

            WithRoleToken(role);
            var response = await _client.PostAsync($"/api/Patients", GetJsonContent(request));
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Post_CreateNewPatient_WithRelatedData_Should_Success()
        {
            var dialysisInfo = new DialysisInfoViewModel
            {
                FirstTime = DateTimeOffset.Now.AddDays(-5).TruncateMilli(),
                FirstTimeAtHere = DateTimeOffset.Now.AddDays(-3).TruncateMilli(),
                Status = "Fine",
                CauseOfDeath = "canSave",
                KidneyTransplant = _fixture.Create<DateTimeOffset>().TruncateMilli(),
                TransferTo = "somewhere",
                TimeOfDeath = _fixture.Create<DateTimeOffset>().TruncateMilli()
            };
            var emergencyContact = new EmergencyContactViewModel { Name = _fixture.Create<string>(), PhoneNumber = _fixture.Create<string>() };
            var tags = new[] { new TagViewModel { Text = _fixture.Create<string>(), Color = _fixture.Create<string>() } };
            var allergy = new[] { -1, -2 };

            var patient = await CreatePatientAsync(dialysis: dialysisInfo, emergency: emergencyContact, allergy: allergy, tags: tags);
            var id = patient.Id;

            // check
            var response = await _client.GetAsync($"/api/Patients/{id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await DeserializeAsync<CreatePatientViewModel>(response);

            result.Should().BeEquivalentTo(patient, option => option
                .Using<DateTimeOffset>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromSeconds(1)))
                .WhenTypeIs<DateTimeOffset>()
                .Excluding(x => x.Tags)
                .Excluding(x => x.Allergy));
        }

        [Fact]
        public async Task Post_EditPatient_Should_Success()
        {
            // create
            var patient = await CreatePatientAsync();
            var id = patient.Id;

            patient.Name = _fixture.Create<string>();
            patient.Admission = AdmissionType.Hospitalized.ToString();
            patient.CoverageScheme = CoverageSchemeType.NationalHealthSecurity.ToString();
            patient.Telephone = "something";
            patient.Address = "something someting";

            var response = await _client.PostAsync($"/api/Patients/{id}", GetJsonContent(patient));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // check
            response = await _client.GetAsync($"/api/Patients/{id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await DeserializeAsync<CreatePatientViewModel>(response);

            result.Should().BeEquivalentTo(patient, option => option
                .Excluding(x => x.Tags)
                .Excluding(x => x.Allergy));
        }

        [Fact]
        public async Task Post_EditPatient_AllergyList_And_MedicineHistoryList_Should_Success()
        {
            var tags = new[] { new TagViewModel { Text = _fixture.Create<string>(), Color = _fixture.Create<string>() } };
            var allergy = new[] { -1, -2 };
            // create
            var patient = await CreatePatientAsync(allergy: allergy, tags: tags);
            var id = patient.Id;

            // edit : add and remove
            patient.Allergy = new[] { -2, -3, -4 };

            var response = await _client.PostAsync($"/api/Patients/{id}", GetJsonContent(patient)).OutputResponse(output, "Edit patient");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            // check
            response = await _client.GetAsync($"/api/Patients/{id}").OutputResponse(output, "Get patient");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await DeserializeAsync<CreatePatientViewModel>(response);

            result.Should().BeEquivalentTo(patient, options => options
                .Using<TagViewModel>(ctx => ctx.Subject.Id.Should().NotBeEmpty())
                .WhenTypeIs<TagViewModel>());
            result.Allergy.Should().HaveCount(3);
            result.Allergy.Should().Contain(patient.Allergy);
        }

        [Fact]
        public async Task Post_EditPatient_TagList_Should_Success()
        {
            var tags = new[] { new TagViewModel { Text = _fixture.Create<string>(), Color = _fixture.Create<string>() } };
            // create
            var patient = await CreatePatientAsync(tags: tags);
            var id = patient.Id;

            // edit : add
            patient.Tags = new[] { new TagViewModel { Text = _fixture.Create<string>(), Color = _fixture.Create<string>() }, new TagViewModel { Text = "test", Color = "test", Bold = false, Italic = true } };

            var response = await _client.PostAsync($"/api/Patients/{id}", GetJsonContent(patient)).OutputResponse(output, "Edit Patient");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            // check
            response = await _client.GetAsync($"/api/Patients/{id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await DeserializeAsync<CreatePatientViewModel>(response);

            result.Tags.Should().HaveCount(2);
            result.Should().BeEquivalentTo(patient, options => options
                .Using<TagViewModel>(ctx => ctx.Subject.Id.Should().NotBeEmpty())
                .WhenTypeIs<TagViewModel>()
                .Excluding(x => x.Allergy));

            // edit : remove
            patient.Tags = new[] { new TagViewModel { Text = "test2", Color = "test2", Bold = false, Italic = true } };

            response = await _client.PostAsync($"/api/Patients/{id}", GetJsonContent(patient)).OutputResponse(output, "Edit Patient");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // check
            response = await _client.GetAsync($"/api/Patients/{id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            result = await DeserializeAsync<CreatePatientViewModel>(response);

            result.Tags.Should().HaveCount(1);
            result.Should().BeEquivalentTo(patient, options => options
                .Using<TagViewModel>(ctx => ctx.Subject.Id.Should().NotBeEmpty())
                .WhenTypeIs<TagViewModel>()
                .Excluding(x => x.Allergy));
        }

        [Fact]
        public async Task Post_Doctor_EditPatient_WithDifferentDoctorId_Should_Fail()
        {
            // create doctor
            var doctorId = await CreateUser(Roles.Doctor);

            // create
            var patient = await CreatePatientAsync(doctorId);
            var id = patient.Id;

            // edit
            Guid anotherDoctor = Guid.NewGuid();
            patient.DoctorId = anotherDoctor;

            _client.WithDoctorToken(anotherDoctor);
            var response = await _client.PostAsync($"/api/Patients/{id}", GetJsonContent(patient));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_Doctor_EditPatient_WithSameDoctorId_Should_Success()
        {
            // create doctor
            var doctorId = await CreateUser(Roles.Doctor);

            // create
            var patient = await CreatePatientAsync(doctorId);
            var id = patient.Id;

            // edit
            patient.DialysisInfo = new DialysisInfoViewModel { CauseOfDeath = "ByMe TT" };

            var response = await _client.PostAsync($"/api/Patients/{id}", GetJsonContent(patient));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // check
            response = await _client.GetAsync($"/api/Patients/{id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await DeserializeAsync<CreatePatientViewModel>(response);

            result.Should().BeEquivalentTo(patient, options => options
                .Excluding(x => x.Tags)
                .Excluding(x => x.Allergy));
        }

        [Fact]
        public async Task Post_Nurse_EditPatient_WithDoctorId_Should_Success()
        {
            // create doctor
            var doctorId = await CreateUser(Roles.Doctor);

            // create
            var patient = await CreatePatientAsync(doctorId);
            var id = patient.Id;

            // edit
            patient.DialysisInfo = new DialysisInfoViewModel { CauseOfDeath = "NotMyFault" };

            _client.WithBasicUserToken();
            var response = await _client.PostAsync($"/api/Patients/{id}", GetJsonContent(patient)).OutputResponse(output, "Edit patient");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // check
            response = await _client.GetAsync($"/api/Patients/{id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await DeserializeAsync<CreatePatientViewModel>(response);

            result.Should().BeEquivalentTo(patient, options => options
                .Excluding(x => x.Tags)
                .Excluding(x => x.Allergy));
        }

        [Theory]
        [InlineData(Roles.PowerAdmin, true)]
        [InlineData(Roles.Admin, false)]
        [InlineData(Roles.Doctor, false)]
        [InlineData(Roles.HeadNurse, false)]
        [InlineData(Roles.Nurse, false)]
        public async Task Delete_OnlyPowerAdmin_Should_BeAbleTo_Delete_Patient(string role, bool canDel)
        {
            // create
            var patient = await CreatePatientAsync();
            var id = patient.Id;

            // remove
            WithRoleToken(role);
            var response = await _client.DeleteAsync($"/api/Patients/{id}");
            if (canDel)
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
            else
            {
                response.StatusCode.Should().NotBe(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task Post_CreatePatient_WithWrongUnit_Should_Fail()
        {
            // mock user to be in unit id -2
            _client.WithAdminToken(claim: new { unit = -2 });

            // create
            CreatePatientViewModel request = new CreatePatientViewModel { Name = _fixture.Create<string>(), BirthDate = new DateTime(1992, 11, 5), Id = _fixture.Create<string>(), HospitalNumber = "HN", IdentityNo = "123456789", UnitId = -1 };

            var response = await _client.PostAsync("/api/Patients", GetJsonContent(request));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_CreatePatient_WithoutUnit_Should_Fail()
        {
            // mock user to be in unit id -2
            _client.WithAdminToken();

            // create
            CreatePatientViewModel request = new CreatePatientViewModel { Name = _fixture.Create<string>(), BirthDate = new DateTime(1992, 11, 5), Id = _fixture.Create<string>(), HospitalNumber = "HN", IdentityNo = "123456789" };

            var response = await _client.PostAsync("/api/Patients", GetJsonContent(request));
            response.StatusCode.Should()
                .NotBe(HttpStatusCode.Created)
                .And.NotBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_EditPatient_WithWrongUnit_Should_Fail()
        {
            // create
            var patient = await CreatePatientAsync();
            var id = patient.Id;

            // edit
            patient.UnitId = -2;

            _client.WithAdminToken();
            var response = await _client.PostAsync("/api/Patients", GetJsonContent(patient));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            _client.WithAdminToken(claim: new { unit = -2 });
            patient.UnitId = -1;

            response = await _client.PostAsync("/api/Patients", GetJsonContent(patient));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Get_PatientList_Should_filter_with_Unit_correctly()
        {
            _client.WithHeadNurseToken();
            var response = await _client.GetAsync("/api/Patients");
            var responseTxt = await response.Content.ReadAsStringAsync();
            JToken token = JToken.Parse(responseTxt);
            int originalCount = token["total"].ToObject<int>();
            token["data"].ToList().Count.Should().Be(originalCount);

            await CreatePatientsForUnitsAsync();
            //check unit -1 only
            _client.WithHeadNurseToken();
            response = await _client.GetAsync("/api/Patients");
            response.EnsureSuccessStatusCode();
            responseTxt = await response.Content.ReadAsStringAsync();
            token = JToken.Parse(responseTxt);
            token.Value<int>("total").Should().Be(originalCount + 1);
            token["data"].ToList().Count.Should().Be(originalCount + 1);
            //check unit 1 only
            _client.WithHeadNurseToken(claim: new { unit = 1 });
            response = await _client.GetAsync("/api/Patients");
            response.EnsureSuccessStatusCode();
            responseTxt = await response.Content.ReadAsStringAsync();
            token = JToken.Parse(responseTxt);
            token.Value<int>("total").Should().Be(1);
            token["data"].ToList().Count.Should().Be(1);
            
            //check power admin must see all
            _client.WithPowerAdminToken();
            response = await _client.GetAsync("/api/Patients");
            response.EnsureSuccessStatusCode();
            responseTxt = await response.Content.ReadAsStringAsync();
            token = JToken.Parse(responseTxt);
            token.Value<int>("total").Should().Be(originalCount + 3);
            token["data"].ToList().Count.Should().Be(originalCount + 3);
        }

        [Fact]
        public async Task Get_DoctorPatientList_Should_filter_with_Unit_correctly()
        {
            List<int> unitIds = await CreateUnits();
            var doctorId = await CreateUser(unitIds.ToArray(), Roles.Doctor);

            await CreatePatientsForUnitsAsync(doctorId, unitIds.ToArray());

            _client.WithDoctorToken(doctorId);
            var response = await _client.GetAsync($"/api/Patients?doctorId={doctorId}").OutputResponse(output, "Get doctor patient");
            var responseTxt = await response.Content.ReadAsStringAsync();
            JToken token = JToken.Parse(responseTxt);
            token["total"].ToObject<int>().Should().Be(3);
            token["data"].ToList().Count.Should().Be(3);
        }

        [Fact]
        public async Task Get_DoctorPatientList_Should_filter_OnlyOwnPatient_Correctly()
        {
            List<int> unitIds = await CreateUnits();
            var doctorId = await CreateUser(unitIds.ToArray(), Roles.Doctor);

            await CreatePatientsForUnitsAsync(null, unitIds.ToArray());
            await CreatePatientAsync(doctorId, unitIds.First());

            //check multiple units user
            _client.WithDoctorToken(doctorId, claim: new { unit = unitIds.ToArray() });
            var response = await _client.GetAsync("/api/Patients").OutputResponse(output, "Get doctor patient");
            response.EnsureSuccessStatusCode();
            var responseTxt = await response.Content.ReadAsStringAsync();
            JToken token = JToken.Parse(responseTxt);
            token.Value<int>("total").Should().Be(1);
            token["data"].ToList().Count.Should().Be(1);
        }

        // ===================== Util ==================================

        private async Task CreatePatientsForUnitsAsync(string doctorId = null, params int[] unitIds)
        {
            if (!unitIds?.Any() ?? false)
            {
                unitIds = new[] { -1, 1, 2 };
            }
            foreach (var id in unitIds)
            {
                await CreatePatientAsync(doctorId, unitId: id);
            }
        }

        private async Task<List<int>> CreateUnits(int count = 3)
        {
            List<int> ids = new List<int>();
            for (int i = 0; i < count; i++)
            {
                var request = new UnitViewModel { Name = _fixture.Create<string>() };
                _client.WithPowerAdminToken();
                var response = await _client.PostAsync("/api/masterdata/unit", GetJsonContent(request));
                var id = int.Parse(response.Headers.Location.OriginalString);
                ids.Add(id);
            }
            return ids;
        }
    }
}
