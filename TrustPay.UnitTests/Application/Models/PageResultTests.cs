using System.Collections.Generic;
using FluentAssertions;
using TrustPay.Application.Common.Models;
using Xunit;

namespace TrustPay.UnitTests.Application.Common.Models
{
    public class PageResultTests
    {
        [Theory]
        [InlineData(10, 3, 4)]
        [InlineData(10, 5, 2)]
        [InlineData(0, 10, 0)]
        [InlineData(5, 0, 0)]
        public void TotalPages_ShouldCalculateCorrectly(int totalCount, int pageSize, int expectedTotalPages)
        {
            var result = new PageResult<string>(new List<string>(), 1, pageSize, totalCount);

            result.TotalPages.Should().Be(expectedTotalPages);
        }

        [Theory]
        [InlineData(1, 3, true)]
        [InlineData(2, 3, true)]
        [InlineData(3, 3, false)]
        [InlineData(1, 0, false)]
        public void HasNextPage_ShouldReturnExpectedResult(int pageNumber, int totalPages, bool expectedHasNext)
        {
            int pageSize = 10;
            int totalCount = totalPages * pageSize;
            var result = new PageResult<string>(new List<string>(), pageNumber, pageSize, totalCount);

            result.HasNextPage.Should().Be(expectedHasNext);
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(2, true)]
        [InlineData(3, true)]
        public void HasPreviousPage_ShouldReturnExpectedResult(int pageNumber, bool expectedHasPrevious)
        {
            var result = new PageResult<string>(new List<string>(), pageNumber, 10, 30);

            result.HasPreviousPage.Should().Be(expectedHasPrevious);
        }
    }
}