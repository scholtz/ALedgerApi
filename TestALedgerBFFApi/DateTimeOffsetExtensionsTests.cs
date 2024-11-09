using System;
using ALedgerBFFApi.Extensions;
using NUnit.Framework;

namespace DateTimeOffsetExtensions.Tests
{
    [TestFixture]
    public class DateTimeOffsetExtensionsTests
    {
        [Test]
        public void ToUtcPreservingTime_ShouldConvertToUtcKeepingLocalTime()
        {
            // Arrange
            var originalDateTimeOffset = new DateTimeOffset(2000, 1, 1, 13, 0, 0, TimeSpan.FromHours(1));

            // Act
            var result = originalDateTimeOffset.ToUtcPreservingTime();

            // Assert
            Assert.That(result.Year, Is.EqualTo(2000));
            Assert.That(result.Month, Is.EqualTo(1));
            Assert.That(result.Day, Is.EqualTo(1));
            Assert.That(result.Hour, Is.EqualTo(13));
            Assert.That(result.Minute, Is.EqualTo(0));
            Assert.That(result.Second, Is.EqualTo(0));
            Assert.That(result.Offset, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void ToUtcPreservingTime_ShouldConvertNegativeOffsetToUtcKeepingLocalTime()
        {
            // Arrange
            var originalDateTimeOffset = new DateTimeOffset(2000, 1, 1, 13, 0, 0, TimeSpan.FromHours(-5));

            // Act
            var result = originalDateTimeOffset.ToUtcPreservingTime();

            // Assert
            Assert.That(result.Year, Is.EqualTo(2000));
            Assert.That(result.Month, Is.EqualTo(1));
            Assert.That(result.Day, Is.EqualTo(1));
            Assert.That(result.Hour, Is.EqualTo(13));
            Assert.That(result.Minute, Is.EqualTo(0));
            Assert.That(result.Second, Is.EqualTo(0));
            Assert.That(result.Offset, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void ToUtcPreservingTime_ShouldHandleAlreadyUtcOffset()
        {
            // Arrange
            var originalDateTimeOffset = new DateTimeOffset(2000, 1, 1, 13, 0, 0, TimeSpan.Zero);

            // Act
            var result = originalDateTimeOffset.ToUtcPreservingTime();

            // Assert
            Assert.That(result, Is.EqualTo(originalDateTimeOffset)); // Should be equal to the original value
        }

        [Test]
        public void ToUtcPreservingTime_ShouldHandleDifferentDateTimes()
        {
            // Arrange
            var originalDateTimeOffset = new DateTimeOffset(2024, 11, 9, 15, 45, 30, TimeSpan.FromHours(2));

            // Act
            var result = originalDateTimeOffset.ToUtcPreservingTime();

            // Assert
            Assert.That(result.Year, Is.EqualTo(2024));
            Assert.That(result.Month, Is.EqualTo(11));
            Assert.That(result.Day, Is.EqualTo(9));
            Assert.That(result.Hour, Is.EqualTo(15));
            Assert.That(result.Minute, Is.EqualTo(45));
            Assert.That(result.Second, Is.EqualTo(30));
            Assert.That(result.Offset, Is.EqualTo(TimeSpan.Zero));
        }
        [Test]
        public void ToUtcPreservingTime_ShouldConvertParsedDateTimeOffsetKeepingLocalTime()
        {
            // Arrange
            var originalDateTimeOffset = DateTimeOffset.Parse("2000-01-01T13:00:00+01:00");

            // Act
            var result = originalDateTimeOffset.ToUtcPreservingTime();

            // Assert
            Assert.That(result.ToString("o"), Is.EqualTo("2000-01-01T13:00:00.0000000+00:00"));
        }
    }
}
