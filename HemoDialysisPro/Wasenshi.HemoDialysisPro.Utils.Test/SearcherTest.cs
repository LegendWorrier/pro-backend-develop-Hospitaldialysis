using AutoFixture;
using FluentAssertions;
using System;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Web.Api.Utils.ModelSearchers;
using Xunit;

namespace Wasenshi.HemoDialysisPro.Models.Test
{
    public class SearcherTest
    {
        IFixture fixture;
        public SearcherTest()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoFixture.AutoMoq.AutoMoqCustomization());
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            fixture.Register(() => DateOnly.FromDateTime(fixture.Create<DateTime>()));
            fixture.Register(() => TimeOnly.FromDateTime(fixture.Create<DateTime>()));
        }

        [Fact]
        public void PatientBaseSearcher()
        {
            var searcher = new PatientBaseSearcher<LabOverview>((LabOverview l) => l.Patient);

            var expr = searcher.GetWhereCondition("something");
            expr.Should().NotBeNull();

            var labOverview = fixture.Create<LabOverview>();
            labOverview.Patient.Name = "Something";

            expr.Compile()(labOverview).Should().BeTrue();

            // explicit search string should work also
            expr = searcher.GetWhereCondition("name = something");
            expr.Should().NotBeNull();

            expr.Compile()(labOverview).Should().BeTrue();
        }
    }
}
