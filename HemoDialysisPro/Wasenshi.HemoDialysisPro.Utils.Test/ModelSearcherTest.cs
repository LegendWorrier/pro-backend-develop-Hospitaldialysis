using FluentAssertions;
using System;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;
using Xunit;

namespace Wasenshi.HemoDialysisPro.Models.Test
{
    public class ModelSearcherTest
    {
        TestModel model;

        public ModelSearcherTest()
        {
            model = new TestModel
            {
                A = true,
                B = false,
                C = true,
                Name = "Test"
            };
        }

        [Theory]
        [InlineData("a & b", false)]
        [InlineData("a | b", true)]
        [InlineData("Test", true)]
        [InlineData("hahaa", false)]
        public void ModelSearcherShouldParseBasicConditionsCorrectly(string whereCondition, bool expectedResult)
        {
            var searcher = new TestSearch();
            var expr = searcher.GetWhereCondition(whereCondition);

            expr.Should().NotBeNull();
            var function = expr.Compile();

            function(model).Should().Be(expectedResult);
        }

        [Fact]
        public void Parenthesis_Must_Have_Correct_Effect()
        {
            model.C = false;
            var searcher = new TestSearch();

            var first = "A | B & C";
            var expr = searcher.GetWhereCondition(first);
            expr.Should().NotBeNull();
            expr.Compile()(model).Should().Be(false);

            var second = "A | (B&C)"; // space should not have any effect
            expr = searcher.GetWhereCondition(second);
            expr.Compile()(model).Should().Be(true);

            var third = "(A | B) & C";
            expr = searcher.GetWhereCondition(third);
            expr.Should().NotBeNull();
            expr.Compile()(model).Should().Be(false);
        }

        [Fact]
        public void Parenthesis_Should_Work_Complexly()
        {
            var searcher = new TestSearch();

            var first = "a|c&b&(a|b)";
            var expr = searcher.GetWhereCondition(first);
            expr.Should().NotBeNull();
            expr.Compile()(model).Should().Be(false);

            var second = "a| (c&b) &(a|b)"; // space should not have any effect
            expr = searcher.GetWhereCondition(second);
            expr.Compile()(model).Should().Be(true);
        }

        [Fact]
        public void Parenthesis_Should_Not_Modify_order()
        {
            var searcher = new TestSearch();

            var first = "a|b&b&(a|b)";
            var expr = searcher.GetWhereCondition(first);
            expr.Should().NotBeNull();
            expr.Compile()(model).Should().Be(false);

            var second = "(a | b & b )&(a | b )"; // space should not have any effect
            expr = searcher.GetWhereCondition(second);
            expr.Should().NotBeNull();
            expr.Compile()(model).Should().Be(false);

            var third = "(a | b & b ) | c & (a | b )"; // space should not have any effect
            expr = searcher.GetWhereCondition(third);
            expr.Should().NotBeNull();
            expr.Compile()(model).Should().Be(true);
        }

        [Fact]
        public void Parenthesis_Should_Work_Recursively()
        {
            var searcher = new TestSearch();

            var first = "a & (a | (B & C) & B ) | C";
            var expr = searcher.GetWhereCondition(first);
            expr.Should().NotBeNull();
            expr.Compile()(model).Should().Be(true);

            var second = "a & (a | (B & C) ) | (C & B)";
            expr = searcher.GetWhereCondition(second);
            expr.Should().NotBeNull();
            expr.Compile()(model).Should().Be(true);
        }

        [Fact]
        public void Space_ShouldNot_Effect()
        {
            var searcher = new TestSearch();

            var second = "( a  |  b  &  b  )  &  (  a  |  b  )  "; // space should not have any effect
            var expr = searcher.GetWhereCondition(second);
            expr.Should().NotBeNull();
            expr.Compile()(model).Should().Be(false);
        }

        [Fact]
        public void Special_Operator_Should_Work()
        {
            var searcher = new TestSearch();

            var first = "( a  OR  (b  AND  b)  )  or  (  a  AND b )"; // space should not have any effect
            var expr = searcher.GetWhereCondition(first);
            expr.Should().NotBeNull();
            expr.Compile()(model).Should().Be(true);
        }


        class TestModel
        {
            public string Name { get; set; }
            public bool A { get; set; }
            public bool B { get; set; }
            public bool C { get; set; }
        }

        class TestSearch : ModelSearcher<TestModel>
        {
            protected override Expression<Func<TestModel, bool>> ParseConditionBlock(string whereString)
            {
                // assume code has safe-guard for case-insensitive
                if (whereString == "a")
                {
                    return (m) => m.A;
                }
                if (whereString == "b")
                {
                    return (m) => m.B;
                }
                if (whereString == "c")
                {
                    return (m) => m.C;
                }

                return null;
            }

            protected override Expression<Func<TestModel, bool>> ParseDefault(string whereString)
            {
                return (TestModel m) => m.Name.ToLower() == whereString.ToLower();
            }
        }
    }
}
