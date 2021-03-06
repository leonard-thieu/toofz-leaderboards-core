﻿using Newtonsoft.Json;
using toofz.Steam.WebApi.ISteamRemoteStorage;
using toofz.Steam.Tests.Properties;
using Xunit;

namespace toofz.Steam.Tests.WebApi.ISteamRemoteStorage
{
    public class UgcFileDetailsTests
    {
        public class Serialization
        {
            [DisplayFact(nameof(UgcFileDetails.FileName))]
            public void WithoutFileName_DoesNotDeserialize()
            {
                // Arrange
                var json = Resources.UgcFileDetailsWithoutFileName;

                // Act -> Assert
                Assert.Throws<JsonSerializationException>(() =>
                {
                    JsonConvert.DeserializeObject<UgcFileDetailsEnvelope>(json);
                });
            }

            [DisplayFact(nameof(UgcFileDetails.Url))]
            public void WithoutUrl_DoesNotDeserialize()
            {
                // Arrange
                var json = Resources.UgcFileDetailsWithoutUrl;

                // Act -> Assert
                Assert.Throws<JsonSerializationException>(() =>
                {
                    JsonConvert.DeserializeObject<UgcFileDetailsEnvelope>(json);
                });
            }

            [DisplayFact(nameof(UgcFileDetails.Size))]
            public void WithoutSize_DoesNotDeserialize()
            {
                // Arrange
                var json = Resources.UgcFileDetailsWithoutSize;

                // Act -> Assert
                Assert.Throws<JsonSerializationException>(() =>
                {
                    JsonConvert.DeserializeObject<UgcFileDetailsEnvelope>(json);
                });
            }

            [DisplayFact]
            public void Deserializes()
            {
                // Arrange
                var json = Resources.UgcFileDetails;

                // Act
                var ugcFileDetails = JsonConvert.DeserializeObject<UgcFileDetails>(json);

                // Assert
                Assert.IsAssignableFrom<UgcFileDetails>(ugcFileDetails);
                Assert.Equal("2/9/2014_score191_zone1_level2", ugcFileDetails.FileName);
                Assert.Equal("http://cloud-3.steamusercontent.com/ugc/22837952671856412/756063F4E07B686916257652BBEB972C3C9E6F8D/", ugcFileDetails.Url);
                Assert.Equal(1558, ugcFileDetails.Size);
            }
        }
    }
}
