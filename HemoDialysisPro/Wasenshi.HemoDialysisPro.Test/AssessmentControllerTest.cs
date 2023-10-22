using FluentAssertions;
using FluentAssertions.Equivalency;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Test.Fixture;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Wasenshi.HemoDialysisPro.Test
{
    public class AssessmentControllerTest : ControllerTestBase, IClassFixture<EnvironmentFixture>
    {
        public AssessmentControllerTest(EnvironmentFixture env, ITestOutputHelper output) : base(env, output)
        {
        }

        [Fact]
        public async Task Get_Assessments_Should_Success()
        {
            _client.WithBasicUserToken();
            var response = await _client.GetAsync($"/api/Assessments").OutputResponse(output);
            response.EnsureSuccessStatusCode();

            var responseTxt = await response.Content.ReadAsStringAsync();
            var token = JToken.Parse(responseTxt);
            token.Should().HaveCountGreaterThan(1);
        }

        [Theory]
        [InlineData(AssessmentTypes.Pre, OptionTypes.Checkbox)]
        [InlineData(AssessmentTypes.Post, OptionTypes.Radio)]
        public async Task Post_Create_New_Assessments_Should_Success(AssessmentTypes type, OptionTypes option)
        {
            AssessmentViewModel assessment = new AssessmentViewModel
            {
                Name = "test",
                DisplayName = "Test",
                Multi = true,
                HasOther = true,
                Type = type,
                OptionType = option,
                OptionsList = new List<AssessmentOptionViewModel>
                {
                    new AssessmentOptionViewModel
                    {
                        Name = "option1",
                        DisplayName = "Option1"
                    },
                    new AssessmentOptionViewModel
                    {
                        Name = "option2",
                        DisplayName = "Option2"
                    }
                }
            };
            // create
            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/Assessments", GetJsonContent(assessment)).OutputResponse(output);
            response.EnsureSuccessStatusCode();

            // check result
            _client.WithBasicUserToken();
            response = await _client.GetAsync($"/api/Assessments");
            response.EnsureSuccessStatusCode();

            var responseTxt = await response.Content.ReadAsStringAsync();
            var result = Deserialize<IEnumerable<AssessmentViewModel>>(responseTxt);

            var target = result.FirstOrDefault(x => x.Name == "test");
            target.Should().NotBeNull();
            target.Should().BeEquivalentTo(assessment, c =>
            {
                c.IncludingNestedObjects();
                c.Excluding((IMemberInfo info) => info.Name == nameof(target.Id))
                    .Excluding((IMemberInfo info) => info.Name == nameof(target.Created))
                    .Excluding((IMemberInfo info) => info.Name == nameof(target.CreatedBy))
                    .Excluding((IMemberInfo info) => info.Name == nameof(target.Updated))
                    .Excluding((IMemberInfo info) => info.Name == nameof(target.UpdatedBy));

                c.Using<AssessmentOptionViewModel>(x =>
                    {
                        x.Subject.Should().BeEquivalentTo(x.Expectation, cc =>
                            cc.Excluding(xx => xx.Id)
                                .Excluding(xx => xx.AssessmentId)
                                .Excluding(xx => xx.Created)
                                .Excluding(xx => xx.CreatedBy)
                                .Excluding(xx => xx.Updated)
                                .Excluding(xx => xx.UpdatedBy));

                        x.Subject.AssessmentId.Should().NotBe(0L);
                    })
                    .WhenTypeIs<AssessmentOptionViewModel>();

                return c;
            });
        }

        [Fact]
        public async Task Post_Reorder_Assessments_Should_Success()
        {
            _client.WithBasicUserToken();
            var response = await _client.GetAsync($"/api/Assessments");
            response.EnsureSuccessStatusCode();

            var responseTxt = await response.Content.ReadAsStringAsync();
            var allAssessments = Deserialize<List<AssessmentViewModel>>(responseTxt);
            allAssessments.Should().HaveCountGreaterThan(1, "Should have data seed. If not, check the codes.");

            var take = allAssessments.TakeLast(2).ToList();
            var firstId = take[0].Id;
            var secondId = take[1].Id;

            _client.WithPowerAdminToken();
            response = await _client.PostAsync($"/api/Assessments/reorder/{firstId}/{secondId}", GetJsonContent(null));
            response.EnsureSuccessStatusCode();

            // check
            _client.WithBasicUserToken();
            response = await _client.GetAsync($"/api/Assessments");
            response.EnsureSuccessStatusCode();

            responseTxt = await response.Content.ReadAsStringAsync();
            allAssessments = Deserialize<List<AssessmentViewModel>>(responseTxt);
            allAssessments.Should().HaveCountGreaterThan(1);

            responseTxt = await response.Content.ReadAsStringAsync();
            allAssessments = Deserialize<List<AssessmentViewModel>>(responseTxt);
            allAssessments.Should().HaveCountGreaterThan(1);

            take = allAssessments.TakeLast(2).ToList();
            take[0].Id.Should().Be(secondId);
            take[1].Id.Should().Be(firstId);
        }

        [Fact]
        public async Task Post_Add_Assessment_Items_Should_Success()
        {
            var hemo = await CreateHemoRecord();

            // check
            _client.WithBasicUserToken();
            var response = await _client.GetAsync($"/api/Assessments/hemosheet/{hemo.Id}/items").OutputResponse(output, "Get Hemo");
            response.EnsureSuccessStatusCode();
            var defaultAssessments = JToken.Parse(await response.Content.ReadAsStringAsync()).Count();

            List<AssessmentItemViewModel> items = new List<AssessmentItemViewModel>
            {
                new AssessmentItemViewModel
                {
                    AssessmentId = -1,
                    Checked = true
                },
                new AssessmentItemViewModel
                {
                    AssessmentId = -2,
                    Checked = true
                }
            };

            _client.WithBasicUserToken();
            response = await _client.PostAsync($"/api/Assessments/hemosheet/{hemo.Id}/items", GetJsonContent(items)).OutputResponse(output, "add/update items");
            response.EnsureSuccessStatusCode();

            // check
            _client.WithBasicUserToken();
            response = await _client.GetAsync($"/api/Assessments/hemosheet/{hemo.Id}/items").OutputResponse(output, "Get Hemo");
            response.EnsureSuccessStatusCode();

            var result = Deserialize<List<AssessmentItem>>(await response.Content.ReadAsStringAsync());
            result.Should().HaveCount(defaultAssessments + 2);
        }

        // ================== util =================
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
            HemodialysisRecordViewModel result =
                Deserialize<HemodialysisRecordViewModel>(await response.Content.ReadAsStringAsync());

            return result;
        }
    }
}
