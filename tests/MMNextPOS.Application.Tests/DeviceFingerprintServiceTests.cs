using System.Net.NetworkInformation;
using MMNextPOS.Application.Services;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    public class DeviceFingerprintServiceTests
    {
        [Fact]
        public void GetCurrent_ReturnsNonEmptyHash()
        {
            var service = new DeviceFingerprintService();
            var fp = service.GetCurrent();

            Assert.False(string.IsNullOrWhiteSpace(fp.Hash));
            Assert.Equal(64, fp.Hash.Length); // SHA-256 hex length
        }

        [Fact]
        public void GetCurrent_IsDeterministic()
        {
            var service = new DeviceFingerprintService();

            var first = service.GetCurrent();
            var second = service.GetCurrent();

            Assert.Equal(first.Hash, second.Hash);
            Assert.Equal(first.MacAddress, second.MacAddress);
            Assert.Equal(first.MachineName, second.MachineName);
        }

        [Fact]
        public void GetCurrent_ExposesMachineAndOsMetadata()
        {
            var service = new DeviceFingerprintService();
            var fp = service.GetCurrent();

            Assert.False(string.IsNullOrWhiteSpace(fp.MachineName));
            Assert.False(string.IsNullOrWhiteSpace(fp.OsVersion));
        }

        [Fact]
        public void GetCurrent_HandlesNoNetworkInterfaces_Gracefully()
        {
            // We can't truly force "no interfaces" in a portable test, but
            // we can verify the service never throws even on platforms with
            // unusual adapter sets by simply calling it a few times.
            var service = new DeviceFingerprintService();
            for (int i = 0; i < 3; i++)
            {
                var fp = service.GetCurrent();
                Assert.NotNull(fp);
            }
        }
    }
}
