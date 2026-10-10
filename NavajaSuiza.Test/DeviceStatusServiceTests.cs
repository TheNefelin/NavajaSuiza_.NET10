using Microsoft.Extensions.Logging.Abstractions;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Test;

public class DeviceStatusServiceTests
{
    [Theory]
    [InlineData(57, -1.0, 57)]
    [InlineData(0, -1.0, 0)]
    [InlineData(100, -1.0, 100)]
    [InlineData(-1, 0.5, 50)]
    [InlineData(101, 1.0, 100)]
    [InlineData(101, 0.0, 0)]
    [InlineData(-1, -1.0, -1)]
    [InlineData(-1, 1.5, -1)]
    public void GetBatteryCapacity_ResolvesCapacityAndFallbackLevel(int capacity, double level, int expected)
    {
        var source = new FakeSource { BatteryCapacity = capacity, BatteryChargeLevel = level };
        var service = CreateService(source);

        Assert.Equal(expected, service.GetBatteryCapacity());
    }

    [Fact]
    public void GetBatteryCapacity_WhenSourceThrows_ReturnsMinusOne()
    {
        var service = CreateService(new FakeSource { ThrowOnBattery = true });

        Assert.Equal(-1, service.GetBatteryCapacity());
    }

    [Fact]
    public void GetAvailableStorageBytes_ReturnsSourceValue()
    {
        var service = CreateService(new FakeSource { AvailableStorage = 12_345 });

        Assert.Equal(12_345, service.GetAvailableStorageBytes());
    }

    [Fact]
    public void GetTotalStorageBytes_ReturnsSourceValue()
    {
        var service = CreateService(new FakeSource { TotalStorage = 98_765 });

        Assert.Equal(98_765, service.GetTotalStorageBytes());
    }

    [Fact]
    public void GetAvailableStorageBytes_WhenSourceThrows_ReturnsMinusOne()
    {
        var service = CreateService(new FakeSource { ThrowOnStorage = true });

        Assert.Equal(-1, service.GetAvailableStorageBytes());
    }

    [Fact]
    public void GetTotalStorageBytes_WhenSourceThrows_ReturnsMinusOne()
    {
        var service = CreateService(new FakeSource { ThrowOnStorage = true });

        Assert.Equal(-1, service.GetTotalStorageBytes());
    }

    private static DeviceStatusService CreateService(IDeviceStatusSource source)
        => new(source, NullLogger<DeviceStatusService>.Instance);

    private sealed class FakeSource : IDeviceStatusSource
    {
        public int BatteryCapacity { get; set; } = -1;
        public double BatteryChargeLevel { get; set; } = -1;
        public long AvailableStorage { get; set; }
        public long TotalStorage { get; set; }
        public bool ThrowOnBattery { get; set; }
        public bool ThrowOnStorage { get; set; }

        public int GetBatteryCapacity()
        {
            if (ThrowOnBattery) throw new InvalidOperationException("battery unavailable");
            return BatteryCapacity;
        }

        public double GetBatteryChargeLevel()
        {
            if (ThrowOnBattery) throw new InvalidOperationException("battery unavailable");
            return BatteryChargeLevel;
        }

        public long GetAvailableStorageBytes()
        {
            if (ThrowOnStorage) throw new InvalidOperationException("storage unavailable");
            return AvailableStorage;
        }

        public long GetTotalStorageBytes()
        {
            if (ThrowOnStorage) throw new InvalidOperationException("storage unavailable");
            return TotalStorage;
        }
    }
}
