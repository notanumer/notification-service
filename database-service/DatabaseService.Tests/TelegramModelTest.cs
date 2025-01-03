using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Models;
using Xunit;

namespace DatabaseService.Tests
{
    public class PayloadTests
    {
        [Fact]
        public void Test_Payload_Initialization()
        {
            // Arrange
            long expectedUserId = 123456;
            string expectedText = "Hello, World!";

            // Act
            var payload = new Payload
            {
                UserId = expectedUserId,
                Text = expectedText
            };

            // Assert
            Xunit.Assert.Equal(expectedUserId, payload.UserId);
            Xunit.Assert.Equal(expectedText, payload.Text);
        }
    }
    }
