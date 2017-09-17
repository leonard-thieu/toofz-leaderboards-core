﻿using System;
using System.Threading.Tasks;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using toofz.NecroDancer.Leaderboards.Steam.ClientApi;

namespace toofz.NecroDancer.Leaderboards.Tests.Steam.ClientApi
{
    class SteamClientApiTransientErrorDetectionStrategyTests
    {
        [TestClass]
        public class CreateRetryPolicyMethod
        {
            [TestMethod]
            public void ReturnsRetryPolicy()
            {
                // Arrange
                var retryStrategy = RetryStrategy.NoRetry;
                var log = Mock.Of<ILog>();

                // Act
                var retryPolicy = SteamClientApiTransientErrorDetectionStrategy.CreateRetryPolicy(retryStrategy, log);

                // Assert
                Assert.IsInstanceOfType(retryPolicy, typeof(RetryPolicy<SteamClientApiTransientErrorDetectionStrategy>));
            }

            [TestMethod]
            public void OnRetrying_LogsDebugMessage()
            {
                // Arrange
                var retryStrategy = new FixedInterval(1, TimeSpan.Zero);
                var mockLog = new Mock<ILog>();
                var log = mockLog.Object;
                var retryPolicy = SteamClientApiTransientErrorDetectionStrategy.CreateRetryPolicy(retryStrategy, log);
                var count = 0;

                // Act
                retryPolicy.ExecuteAction(() =>
                {
                    if (count == 0)
                    {
                        count++;
                        throw new SteamClientApiException("", new TaskCanceledException());
                    }
                });

                // Assert
                mockLog.Verify(l => l.Debug(It.IsAny<string>()), Times.Once);
            }
        }

        [TestClass]
        public class IsTransientMethod
        {
            [TestMethod]
            public void ExIsSteamClientApiExceptionWithTaskCanceledExceptionAsInnerException_ReturnsTrue()
            {
                // Arrange
                var strategy = new SteamClientApiTransientErrorDetectionStrategy();
                var ex = new SteamClientApiException("myMessage", new TaskCanceledException());

                // Act
                var isTransient = strategy.IsTransient(ex);

                // Assert
                Assert.IsTrue(isTransient);
            }

            [TestMethod]
            public void ExIsSteamClientApiExceptionWithoutTaskCanceledExceptionAsInnerException_ReturnsFalse()
            {
                // Arrange
                var strategy = new SteamClientApiTransientErrorDetectionStrategy();
                var ex = new SteamClientApiException("myMessage");

                // Act
                var isTransient = strategy.IsTransient(ex);

                // Assert
                Assert.IsFalse(isTransient);
            }

            [TestMethod]
            public void ExIsNotTaskCanceledException_ReturnsFalse()
            {
                // Arrange
                var strategy = new SteamClientApiTransientErrorDetectionStrategy();
                var ex = new Exception();

                // Act
                var isTransient = strategy.IsTransient(ex);

                // Assert
                Assert.IsFalse(isTransient);
            }
        }
    }
}
