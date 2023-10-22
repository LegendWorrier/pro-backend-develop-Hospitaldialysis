using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Test.Fixture;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.ViewModels.Base;
using Xunit;
using Xunit.Abstractions;
using static System.Text.Json.JsonSerializer;


namespace Wasenshi.HemoDialysisPro.Test
{
    public class HemoControllerTest : ControllerTestBase, IClassFixture<EnvironmentFixture>
    {
        public HemoControllerTest(EnvironmentFixture env, ITestOutputHelper output) : base(env, output)
        {
        }

        [Fact]
        public async Task Get_Prescription_ByPatient_Should_Success()
        {
            var prescription = await CreatePrescription();

            _client.WithBasicUserToken();
            var response = await _client.GetAsync($"/api/Hemodialysis/prescriptions/patient/{prescription.PatientId}");
            response.EnsureSuccessStatusCode();

            var responseTxt = await response.Content.ReadAsStringAsync();
            var token = JToken.Parse(responseTxt);
            token.Should().HaveCount(1);
        }

        [Fact]
        public async Task Get_Prescription_WithWrongPatientId_Should_Fail()
        {
            _client.WithBasicUserToken();
            var response = await _client.GetAsync($"/api/Hemodialysis/prescriptions/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_CreatePrescription_WithWrongPatientId_Should_Fail()
        {
            await CreatePrescription("Test").Invoking(async x => await x).Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task Get_Prescription_WithWrongUnit_Should_Fail()
        {
            var prescription = await CreatePrescription();

            _client.WithBasicUserToken(new { unit = 2 });
            var response = await _client.GetAsync($"/api/Hemodialysis/prescriptions/{prescription.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response = await _client.GetAsync($"/api/Hemodialysis/prescriptions/patient/{prescription.PatientId}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_EditPrescription_By_HeadNurse_Should_Success()
        {
            var prescription = await CreatePrescription();

            prescription.Anticoagulant = "something";
            prescription.DryWeight = 54.3f;
            prescription.Note = "haha";

            _client.WithHeadNurseToken();
            var response = await _client.PostAsync($"/api/Hemodialysis/prescriptions/{prescription.Id}", GetJsonContent(prescription));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response = await _client.GetAsync($"/api/Hemodialysis/prescriptions/{prescription.Id}");
            DialysisPrescriptionViewModel edited = await DeserializeAsync<DialysisPrescriptionViewModel>(response);

            edited.Should().BeEquivalentTo(prescription, c => c.Excluding(x => x.CreatedBy).Excluding(x => x.Updated).Excluding(x => x.UpdatedBy));
        }

        [Fact]
        public async Task Post_EditPrescription_PatientId_Should_Fail()
        {
            var prescription = await CreatePrescription();

            var otherPatient = await CreatePatientAsync();
            prescription.PatientId = otherPatient.Id;

            _client.WithHeadNurseToken();
            var response = await _client.PostAsync($"/api/Hemodialysis/prescriptions/{prescription.Id}", GetJsonContent(prescription));
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Get_HemoRecords_Should_Success()
        {
            await CreateHemoRecord();
            await CreateHemoRecord();

            _client.WithBasicUserToken();
            var response = await _client.GetAsync($"/api/Hemodialysis/records").OutputResponse(output);
            response.EnsureSuccessStatusCode();

            var responseTxt = await response.Content.ReadAsStringAsync();
            var token = JToken.Parse(responseTxt);
            token["data"].Count().Should().BeGreaterThan(1);
        }

        [Fact]
        public async Task Get_HemoRecords_Should_Filter_Unit_Correctly()
        {
            var patient = await CreatePatientAsync();
            await CreateHemoRecord(patient.Id);
            var patient2 = await CreatePatientAsync(unitId: 2);
            await CreateHemoRecord(patient2.Id);

            _client.WithBasicUserToken(new { unit = 2 });
            var response = await _client.GetAsync($"/api/Hemodialysis/records").OutputResponse(output);
            response.EnsureSuccessStatusCode();

            var responseTxt = await response.Content.ReadAsStringAsync();
            var token = JToken.Parse(responseTxt);
            token["data"].Count().Should().Be(1);
        }

        [Fact]
        public async Task Get_HemoRecords_ByPatient_Should_Success()
        {
            var record = await CreateHemoRecord();

            _client.WithBasicUserToken();
            var response = await _client.GetAsync($"/api/Hemodialysis/records/patient/{record.PatientId}");
            response.EnsureSuccessStatusCode();

            var result = await DeserializeAsync<PageView<HemodialysisRecordViewModel>>(response);
            result.Total.Should().Be(1);
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Get_HemoRecord_WithWrongPatientId_Should_Fail()
        {
            _client.WithBasicUserToken();
            var response = await _client.GetAsync($"/api/Hemodialysis/records/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_CreateHemoRecord_WithWrongPatientId_Should_Fail()
        {
            await CreateHemoRecord("Test").Invoking(async x => await x).Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task Get_HemoRecord_WithWrongUnit_Should_Fail()
        {
            var record = await CreateHemoRecord();

            _client.WithBasicUserToken(new { unit = 2 });
            var response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response = await _client.GetAsync($"/api/Hemodialysis/records/patient/{record.PatientId}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_CreateHemoRecord_WithPrescription_Should_AutoAssign_Correctly()
        {
            var prescription = await CreatePrescription();
            var record = await CreateHemoRecord(prescription.PatientId);

            var response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            HemodialysisRecordViewModel result = await DeserializeAsync<HemodialysisRecordViewModel>(response);
            result.DialysisPrescription.Should().NotBeNull("Should auto assign prescription");
        }

        [Fact]
        public async Task Post_CreateHemoRecord_MultilpleTimes_Should_Reuse_And_Assign_Correctly()
        {
            var prescription = await CreatePrescription();
            var record = await CreateHemoRecord(prescription.PatientId); // create dummy record with prescription

            //complete before create new one
            await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}/complete", new StringContent("{}", Encoding.UTF8, "application/json"));

            record = await CreateHemoRecord(prescription.PatientId); // create another record which suppose to has the same prescription

            var response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            HemodialysisRecordViewModel result = await DeserializeAsync<HemodialysisRecordViewModel>(response);
            result.DialysisPrescription.Should().NotBeNull("Should auto assign prescription");
        }

        [Fact]
        public async Task Post_CreateHemoRecord_MultilpleTimes_WithoutCompleted_Should_Fail()
        {
            var prescription = await CreatePrescription();
            var record = await CreateHemoRecord(prescription.PatientId); // create dummy record with prescription

            await prescription.PatientId.Invoking(async x => await CreateHemoRecord(x)).Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task Post_CreatePrescription_MultilpleTimes_Should_Assign_TheLatest_Always()
        {
            var prescription = await CreatePrescription();
            await CreatePrescription(prescription.PatientId, duration: TimeSpan.FromHours(5));
            var record = await CreateHemoRecord(prescription.PatientId); // create record which suppose to has the latest prescription

            var response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            HemodialysisRecordViewModel result = await DeserializeAsync<HemodialysisRecordViewModel>(response);
            result.DialysisPrescription.Should().NotBeNull("Should auto assign prescription");
            result.DialysisPrescription.Duration.Should().Be((int)TimeSpan.FromHours(5).TotalMinutes);
            result.DialysisPrescription.Mode.Should().Be(DialysisMode.HF.ToString());
        }

        [Fact]
        public async Task Post_TemporaryPrescription_ShouldNot_TakePrecedence_over_latest()
        {
            var prescription = await CreatePrescription(mode: DialysisMode.HDF, temp: true);
            var record = await CreateHemoRecord(prescription.PatientId); // create a record which suppose to has the temporary prescription

            var response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            HemodialysisRecordViewModel result = await DeserializeAsync<HemodialysisRecordViewModel>(response);
            result.DialysisPrescription.Should().NotBeNull("Should auto assign prescription");
            result.DialysisPrescription.Mode.Should().Be(DialysisMode.HDF.ToString());
            result.DialysisPrescription.Temporary.Should().BeTrue();

            //complete before create new one
            await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}/complete", new StringContent("{}", Encoding.UTF8, "application/json"));

            await CreatePrescription(prescription.PatientId);
            record = await CreateHemoRecord(prescription.PatientId); // create another record which suppose to has the latest prescription, which should not be temporary

            response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            result = await DeserializeAsync<HemodialysisRecordViewModel>(response);
            result.DialysisPrescription.Should().NotBeNull("Should auto assign prescription");
            result.DialysisPrescription.Temporary.Should().BeFalse();
        }

        [Fact]
        public async Task Post_TemporaryPrescription_ShouldNot_Be_Reused()
        {
            var prescription = await CreatePrescription(mode: DialysisMode.HDF, temp: true);
            var record = await CreateHemoRecord(prescription.PatientId); // create a record which suppose to has the temporary prescription

            var response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            HemodialysisRecordViewModel result = await DeserializeAsync<HemodialysisRecordViewModel>(response);
            result.DialysisPrescription.Should().NotBeNull("Should auto assign prescription");
            result.DialysisPrescription.Mode.Should().Be(DialysisMode.HDF.ToString());
            result.DialysisPrescription.Temporary.Should().BeTrue();

            //complete before create new one
            await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}/complete", new StringContent("{}", Encoding.UTF8, "application/json"));

            record = await CreateHemoRecord(prescription.PatientId); // create another record which suppose to has no prescription, or at least, not the previous temporary prescription.

            response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            result = await DeserializeAsync<HemodialysisRecordViewModel>(response);
            result.DialysisPrescription.Should().BeNull();
        }

        [Fact]
        public async Task Post_EditHemoRecord_By_Nurse_Should_Success()
        {
            var prescription = await CreatePrescription(mode: DialysisMode.HDF);
            var record = await CreateHemoRecord(prescription.PatientId);

            record.Ward = "Myward";
            record.Bed = "mybed";
            record.PreVitalsign = new[] { new VitalSignRecordViewModel { BPD = 100, BPS = 120 } };
            record.PostVitalsign = new[] { new VitalSignRecordViewModel { BPD = 90, BPS = 100 } };
            record.Dialyzer.UseNo = 3;
            record.Dehydration.PreTotalWeight = 56;
            record.BloodCollection.Pre = "1M";
            record.AvShunt.ANeedleCC = 22.5f;
            record.AvShunt.ShuntSite = "test";
            // Things that should not allow/should bypass
            var anticoagulant = record.DialysisPrescription.Anticoagulant;
            var created = record.Created;
            var completedTime = record.CompletedTime;
            record.DialysisPrescription.Anticoagulant = "use something, im hacking!";
            record.Created = DateTime.UtcNow;
            record.CompletedTime = DateTime.UtcNow;

            _client.WithBasicUserToken();
            var response = await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}", GetJsonContent(record));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            HemodialysisRecordViewModel edited = await DeserializeAsync<HemodialysisRecordViewModel>(response);

            record.DialysisPrescription.Anticoagulant = anticoagulant;
            record.Created = created;
            record.CompletedTime = completedTime;
            edited.Should().BeEquivalentTo(record, c =>
                c.Excluding(x => x.Updated).Excluding(x => x.UpdatedBy));
        }

        [Fact]
        public async Task Post_EditHemoRecord_VitalSign_Should_Add_Edit_Remove_Correctly()
        {
            var prescription = await CreatePrescription(mode: DialysisMode.HDF);
            var record = await CreateHemoRecord(prescription.PatientId);

            record.PreVitalsign = new[] { new VitalSignRecordViewModel { BPD = 100, BPS = 120 } };
            record.PostVitalsign = new[] { new VitalSignRecordViewModel { BPD = 90, BPS = 100 } };

            _client.WithBasicUserToken();
            var response = await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}", GetJsonContent(record));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            HemodialysisRecordViewModel edited = await DeserializeAsync<HemodialysisRecordViewModel>(response);

            edited.PostVitalsign.Should().BeEquivalentTo(record.PostVitalsign);
            edited.PreVitalsign.Should().BeEquivalentTo(record.PreVitalsign);

            // edit

            record.PreVitalsign = new[] { new VitalSignRecordViewModel { BPD = 55, BPS = 60, HR = 60 } };
            record.PostVitalsign = new[] { new VitalSignRecordViewModel { BPD = 60, BPS = 80, HR = 70 } };

            _client.WithBasicUserToken();
            response = await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}", GetJsonContent(record));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            edited = await DeserializeAsync<HemodialysisRecordViewModel>(response);

            edited.PostVitalsign.Should().BeEquivalentTo(record.PostVitalsign);
            edited.PreVitalsign.Should().BeEquivalentTo(record.PreVitalsign);

            // Add / Remove

            record.PreVitalsign = new[] { new VitalSignRecordViewModel { BPD = 55, BPS = 60, HR = 60 }, new VitalSignRecordViewModel { RR = 60, Temp = 45 } };
            record.PostVitalsign = new VitalSignRecordViewModel[0];

            _client.WithBasicUserToken();
            response = await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}", GetJsonContent(record));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            edited = await DeserializeAsync<HemodialysisRecordViewModel>(response);

            edited.PostVitalsign.Should().BeEquivalentTo(record.PostVitalsign);
            edited.PreVitalsign.Should().BeEquivalentTo(record.PreVitalsign);
        }

        [Fact]
        public async Task Post_CompleteHemoRecord_Should_Success()
        {
            var prescription = await CreatePrescription(mode: DialysisMode.HDF);
            var record = await CreateHemoRecord(prescription.PatientId);

            // edit some data
            record.CycleStartTime = DateTimeOffset.Now;
            record.Ward = _fixture.Create<string>();
            record.Bed = _fixture.Create<string>();
            record.Dialyzer.UseNo = 2;
            _client.WithBasicUserToken();
            var response = await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}", GetJsonContent(record));
            var checkResponse = await response.Content.ReadAsStringAsync();
            output.WriteLine("edit hemosheet response:");
            output.WriteLine(checkResponse);
            response.EnsureSuccessStatusCode();

            // check original
            response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            var ori = await DeserializeAsync<HemodialysisRecordViewModel>(response);
            ori.CompletedTime.Should().BeNull();

            // complete
            _client.WithBasicUserToken();

            response = await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}/complete", GetJsonContent(new { }));
            checkResponse = await response.Content.ReadAsStringAsync();
            output.WriteLine("complete hemosheet response:");
            output.WriteLine(checkResponse);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // check complete
            response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            var result = await DeserializeAsync<HemodialysisRecordViewModel>(response);
            result.CompletedTime.Should().NotBeNull();
            result.Should().BeEquivalentTo(ori, c =>
            {
                c.Excluding(x => x.CompletedTime)
                    .Excluding(x => x.Updated)
                    .Excluding(x => x.UpdatedBy)
                    .Using<DateTimeOffset?>(x => x.Expectation?.Should().BeCloseTo(x.Subject.GetValueOrDefault(), TimeSpan.FromSeconds(1)))
                    .WhenTypeIs<DateTimeOffset?>()
                    .Using<bool>(x => x.Subject.Should().BeTrue()).When(m =>
                    m.Path.Equals($"{nameof(result.DialysisPrescription)}.{nameof(result.DialysisPrescription.IsHistory)}"));
                return c;
            });
        }

        [Fact]
        public async Task Post_CompleteHemoRecord_Should_Not_Allow_Null_Prescription()
        {
            // Create a record without prescription
            var record = await CreateHemoRecord();

            _client.WithBasicUserToken();
            var response = await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}/complete", GetJsonContent(null));
            response.StatusCode.Should().NotBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_CompleteHemoRecord_WithEdit_Should_Success()
        {
            //create previous prescription
            var prescription = await CreatePrescription(mode: DialysisMode.HDF);
            var record = await CreateHemoRecord(prescription.PatientId);

            //create newer prescription (for the same patient)
            var prescription2 = await CreatePrescription(prescription.PatientId, mode: DialysisMode.IUF);

            _client.WithHeadNurseToken();
            EditHemodialysisRecordViewModel edit = new EditHemodialysisRecordViewModel
            {
                Dialyzer = new DialyzerRecordViewModel { UseNo = 3 },
                Bed = _fixture.Create<string>(),
                CycleStartTime = DateTimeOffset.UtcNow.TruncateMilli(),
                DialysisPrescriptionId = prescription2.Id
            };
            CompleteHemoViewModel request = new CompleteHemoViewModel
            {
                Update = edit
            };
            var response = await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}/complete", GetJsonContent(request));
            var checkResponse = await response.Content.ReadAsStringAsync();
            output.WriteLine("complete hemosheet response:");
            output.WriteLine(checkResponse);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            var result = await DeserializeAsync<HemodialysisRecordViewModel>(response);
            result.CompletedTime.Should().NotBeNull();
            result.Dialyzer.UseNo.Should().Be(edit.Dialyzer.UseNo);
            result.Bed.Should().Be(edit.Bed);
            result.CycleStartTime.Should().BeCloseTo(edit.CycleStartTime.Value, TimeSpan.FromSeconds(1));
            result.DialysisPrescription.Id.Should().Be(prescription2.Id);
        }

        [Fact]
        public async Task Post_EditHemoRecord_Add_Prescription_Should_Success()
        {
            // Create a record without prescription
            var record = await CreateHemoRecord();

            //create newer prescription (for the same patient)
            var prescription2 = await CreatePrescription(record.PatientId, mode: DialysisMode.IUF);

            EditHemodialysisRecordViewModel edit = new EditHemodialysisRecordViewModel
            {
                PatientId = record.PatientId,
                DialysisPrescriptionId = prescription2.Id
            };

            _client.WithHeadNurseToken();
            var response = await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}", GetJsonContent(edit)).OutputResponse(output, "Edit hemosheet");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            var result = await DeserializeAsync<HemodialysisRecordViewModel>(response);

            result.DialysisPrescription.Should().NotBeNull();
            result.DialysisPrescription.Id.Should().Be(prescription2.Id);
        }

        [Fact]
        public async Task Post_EditHemoRecord_Change_Prescription_Should_Success()
        {
            var prescription = await CreatePrescription(mode: DialysisMode.SLED);
            // Create a record with prescription
            var record = await CreateHemoRecord(prescription.PatientId);

            var response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            var result = await DeserializeAsync<HemodialysisRecordViewModel>(response);
            // check id before edit
            result.DialysisPrescription.Id.Should().Be(prescription.Id);

            //create newer prescription (for the same patient)
            var prescription2 = await CreatePrescription(record.PatientId, mode: DialysisMode.IUF);

            EditHemodialysisRecordViewModel edit = new EditHemodialysisRecordViewModel
            {
                PatientId = record.PatientId,
                DialysisPrescriptionId = prescription2.Id
            };

            var content = new StringContent(Serialize(edit), Encoding.UTF8, "application/json");

            _client.WithHeadNurseToken();
            response = await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}", GetJsonContent(edit)).OutputResponse(output, "Complete hemosheet");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            result = await DeserializeAsync<HemodialysisRecordViewModel>(response);
            // check id after edit
            result.DialysisPrescription.Id.Should().Be(prescription2.Id);
        }

        [Fact]
        public async Task Post_EditHemoRecord_Change_Prescription_Should_NotAllow_DeletedPrescription()
        {
            //create previous prescription (not use)
            var prescription = await CreatePrescription(mode: DialysisMode.HDF);
            //create newer prescription (for the same patient)
            var prescription2 = await CreatePrescription(prescription.PatientId, mode: DialysisMode.IUF);
            // Create a record with prescription 2
            var record = await CreateHemoRecord(prescription2.PatientId);

            // Delete previous prescription
            _client.WithHeadNurseToken();
            var response = await _client.DeleteAsync($"/api/Hemodialysis/prescriptions/{prescription.Id}");
            response.EnsureSuccessStatusCode();

            EditHemodialysisRecordViewModel edit = new EditHemodialysisRecordViewModel
            {
                PatientId = record.PatientId,
                DialysisPrescriptionId = prescription.Id
            };

            _client.WithHeadNurseToken();
            response = await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}", GetJsonContent(edit));
            response.StatusCode.Should().NotBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_EditHemoRecord_Change_Prescription_Should_NotAllow_ForCompletedRecord()
        {
            //create previous prescription (not use)
            var prescription = await CreatePrescription(mode: DialysisMode.HDF);
            //create newer prescription (for the same patient)
            var prescription2 = await CreatePrescription(prescription.PatientId, mode: DialysisMode.IUF);
            // Create a record with prescription 2
            var record = await CreateHemoRecord(prescription2.PatientId);

            // complete the record
            _client.WithBasicUserToken();
            var response = await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}/complete", GetJsonContent(new { }));
            response.StatusCode.Should().Be(HttpStatusCode.OK);


            EditHemodialysisRecordViewModel edit = new EditHemodialysisRecordViewModel
            {
                PatientId = record.PatientId,
                DialysisPrescriptionId = prescription.Id
            };

            _client.WithHeadNurseToken();
            response = await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}", GetJsonContent(edit));
            response.StatusCode.Should().NotBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_AssignCosign_WithPassword_Should_Work_Correctly()
        {
            var prescription = await CreatePrescription();
            var record = await CreateHemoRecord(prescription.PatientId);
            var id = await CreateUser(Roles.Nurse);
            CosignRequestViewModel request = new CosignRequestViewModel
            {
                UserId = Guid.Parse(id),
                Password = "TestNaja1234" //hard-coded from controller base (look inside 'CreateUser(..)')
            };
            var response = await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}/cosign", GetJsonContent(request)).OutputResponse(output, "Assign cosign");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // check hemosheet
            response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            var result = await DeserializeAsync<HemodialysisRecordViewModel>(response);

            result.Should().BeEquivalentTo(record, c =>
            {
                c.IncludingNestedObjects();
                c.Using<Guid?>(x => x.Subject.Should().NotBeNull().And.Be(request.UserId))
                    .When(x => x.Path == nameof(result.ProofReader));
                c.Excluding(h => h.Updated)
                    .Excluding(h => h.UpdatedBy);

                return c;
            });
        }

        [Fact]
        public async Task Post_AssignCosign_To_Yourself_Should_Fail()
        {
            // the owner
            var user = await CreateUser();
            Guid userId = Guid.Parse(user);
            var prescription = await CreatePrescription(userId: userId);
            var record = await CreateHemoRecord(prescription.PatientId, userId);

            CosignRequestViewModel request = new CosignRequestViewModel
            {
                UserId = userId,
                Password = "TestNaja1234" //hard-coded from controller base (look inside 'CreateUser(..)')
            };
            var content = new StringContent(Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"/api/Hemodialysis/records/{record.Id}/cosign", GetJsonContent(request)).OutputResponse(output, "Assign cosign");
            response.IsSuccessStatusCode.Should().BeFalse();

            // check hemosheet
            response = await _client.GetAsync($"/api/Hemodialysis/records/{record.Id}");
            var result = await DeserializeAsync<HemodialysisRecordViewModel>(response);

            result.Should().BeEquivalentTo(record, c =>
            {
                c.IncludingNestedObjects();
                c.Excluding(h => h.Updated)
                    .Excluding(h => h.UpdatedBy);
                return c;
            });
        }

        [Fact]
        public async Task Post_WhenClaimHemosheet_ShouldNot_Affect_OtherData()
        {
            var patient = await CreatePatientAsync();
            await CreatePrescription(patient.Id);
            var hemosheet = await CreateHemoRecord(patient.Id);

            // create dummy user
            var user = await CreateUser();
            // claim hemosheet by updating created by
            hemosheet.CreatedBy = Guid.Parse(user);
            // then update hemosheet with edit api
            var response = await _client.PostAsync($"/api/Hemodialysis/records/{hemosheet.Id}", GetJsonContent(hemosheet)).OutputResponse(output);
            response.EnsureSuccessStatusCode();


            // IMPORTANT Part: check result
            response = await _client.GetAsync($"/api/Hemodialysis/records/{hemosheet.Id}").OutputResponse(output, "Get Hemo response:");
            response.EnsureSuccessStatusCode();
            var result = await DeserializeAsync<HemodialysisRecordViewModel>(response);

            result.Should().BeEquivalentTo(hemosheet, c => c.Excluding(x => x.Updated).Excluding(x => x.UpdatedBy));
        }

        // ===================== Util ==================================

        private async Task<DialysisPrescriptionViewModel> CreatePrescription(string patientId = null, TimeSpan duration = new TimeSpan(), DialysisMode mode = DialysisMode.HF, bool temp = false, Guid? userId = null)
        {
            if (patientId == null)
            {
                var patient = await CreatePatientAsync();
                patientId = patient.Id;
            }

            DialysisPrescriptionViewModel request = new DialysisPrescriptionViewModel
            {
                PatientId = patientId,
                Duration = (int)(duration == TimeSpan.Zero ? new TimeSpan(1, 40, 0).TotalMinutes : duration.TotalMinutes),
                Mode = mode.ToString(),
                Frequency = 7,
                Temporary = temp
            };

            _client.WithPowerAdminToken(userId);
            var response = await _client.PostAsync($"/api/Hemodialysis/prescriptions", GetJsonContent(request));
            response.EnsureSuccessStatusCode();

            var id = Guid.Parse(response.Headers.Location.OriginalString);

            request.Id = id;
            return request;
        }

        private async Task<HemodialysisRecordViewModel> CreateHemoRecord(string patientId = null, Guid? userId = null)
        {
            if (patientId == null)
            {
                var patient = await CreatePatientAsync();
                patientId = patient.Id;
            }

            HemodialysisRecordViewModel request = new HemodialysisRecordViewModel
            {
                PatientId = patientId,
                Dehydration = new DehydrationRecordViewModel
                {
                    FoodDrinkWeight = 55
                }
            };

            _client.WithPowerAdminToken(userId);
            var response = await _client.PostAsync($"/api/Hemodialysis/records", GetJsonContent(request));
            response.EnsureSuccessStatusCode();

            var id = Guid.Parse(response.Headers.Location.OriginalString);
            response = await _client.GetAsync($"/api/Hemodialysis/records/{id}");
            response.EnsureSuccessStatusCode();
            HemodialysisRecordViewModel result = await DeserializeAsync<HemodialysisRecordViewModel>(response);

            return result;
        }
    }
}
