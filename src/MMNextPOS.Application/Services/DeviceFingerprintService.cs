using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Cross-platform device fingerprint. The hash is the SHA-256 of
    /// <c>MAC|MachineName|OS</c>. CPU/HDD identifiers remain <c>null</c> on
    /// non-Windows targets; richer identifiers are out of scope for this layer
    /// (would require WMI, which is Windows-only).
    /// </summary>
    public sealed class DeviceFingerprintService : IDeviceFingerprintService
    {
        public DeviceFingerprint GetCurrent()
        {
            var machineName = SafeMachineName();
            var mac = PrimaryMacAddress();
            var os = RuntimeInformation.OSDescription;

            var canonical = string.Join("|", mac, machineName, os);
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();

            return new DeviceFingerprint(
                Hash: hash,
                MachineName: machineName,
                MacAddress: mac,
                CpuId: null,
                HardDiskSerial: null,
                OsVersion: os);
        }

        private static string SafeMachineName()
        {
            try
            {
                return Environment.MachineName;
            }
            catch
            {
                return "UNKNOWN";
            }
        }

        private static string PrimaryMacAddress()
        {
            try
            {
                var nic = NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up
                                && n.NetworkInterfaceType != NetworkInterfaceType.Loopback
                                && n.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                                && !n.Description.Contains("Hyper-V", StringComparison.OrdinalIgnoreCase)
                                && !n.Description.Contains("Virtual", StringComparison.OrdinalIgnoreCase)
                                && !n.Description.Contains("VPN", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(n => n.Speed)
                    .FirstOrDefault();

                if (nic == null)
                {
                    return "00:00:00:00:00:00";
                }

                var bytes = nic.GetPhysicalAddress()?.GetAddressBytes();
                if (bytes == null || bytes.Length == 0)
                {
                    return "00:00:00:00:00:00";
                }

                return string.Join(":", bytes.Select(b => b.ToString("X2")));
            }
            catch
            {
                return "00:00:00:00:00:00";
            }
        }
    }
}
