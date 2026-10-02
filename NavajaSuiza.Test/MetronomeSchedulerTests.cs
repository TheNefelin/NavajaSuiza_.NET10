using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Test;

public class MetronomeSchedulerTests
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(500);

    [Fact]
    public void GetTick_WhenAheadOfSchedule_DelaysRemainingTime()
    {
        var tick = MetronomeScheduler.GetTick(
            TimeSpan.FromMilliseconds(1000),
            TimeSpan.FromMilliseconds(100),
            Interval);

        Assert.Equal(TimeSpan.FromMilliseconds(900), tick.Delay);
        Assert.Equal(TimeSpan.FromMilliseconds(1500), tick.NextTick);
    }

    [Fact]
    public void GetTick_WhenSlightlyBehind_DoesNotDelay()
    {
        var tick = MetronomeScheduler.GetTick(
            TimeSpan.FromMilliseconds(1000),
            TimeSpan.FromMilliseconds(1200),
            Interval);

        Assert.Equal(TimeSpan.Zero, tick.Delay);
        Assert.Equal(TimeSpan.FromMilliseconds(1500), tick.NextTick);
    }

    [Fact]
    public void GetTick_WhenExactlyOneIntervalBehind_ResyncsInsteadOfBursting()
    {
        var tick = MetronomeScheduler.GetTick(
            TimeSpan.FromMilliseconds(1000),
            TimeSpan.FromMilliseconds(1500),
            Interval);

        Assert.Equal(TimeSpan.Zero, tick.Delay);
        Assert.Equal(TimeSpan.FromMilliseconds(2000), tick.NextTick);
    }

    [Fact]
    public void GetTick_WhenManyIntervalsBehind_DoesNotCatchUpLostPulses()
    {
        var tick = MetronomeScheduler.GetTick(
            TimeSpan.FromMilliseconds(1000),
            TimeSpan.FromMilliseconds(6000),
            Interval);

        Assert.Equal(TimeSpan.Zero, tick.Delay);
        Assert.Equal(TimeSpan.FromMilliseconds(6500), tick.NextTick);
    }

    [Fact]
    public void GetTick_AfterLongPause_KeepsSteadyIntervalFromNow()
    {
        var first = MetronomeScheduler.GetTick(
            TimeSpan.FromMilliseconds(1000),
            TimeSpan.FromMilliseconds(30000),
            Interval);
        var resumedAt = TimeSpan.FromMilliseconds(30000) + first.Delay;
        var second = MetronomeScheduler.GetTick(first.NextTick, resumedAt, Interval);

        Assert.Equal(TimeSpan.Zero, first.Delay);
        Assert.Equal(Interval, second.Delay);
        Assert.Equal(first.NextTick + Interval, second.NextTick);
    }

    [Fact]
    public void GetTick_ConsistentBpm_DelaysSumToElapsedTime()
    {
        var target = TimeSpan.Zero;
        var delaySum = TimeSpan.Zero;
        var elapsed = TimeSpan.Zero;

        var tick = MetronomeScheduler.GetTick(target, elapsed, Interval);
        Assert.Equal(TimeSpan.Zero, tick.Delay);
        delaySum += tick.Delay;
        elapsed += tick.Delay;
        target = tick.NextTick;

        for (var i = 1; i < 10; i++)
        {
            tick = MetronomeScheduler.GetTick(target, elapsed, Interval);
            Assert.Equal(Interval, tick.Delay);
            delaySum += tick.Delay;
            elapsed += tick.Delay;
            target = tick.NextTick;
        }

        Assert.Equal(Interval * 9, delaySum);
        Assert.Equal(Interval * 10, target);
    }

    [Fact]
    public void GetTick_WhenIntervalChanged_RebasesToNewInterval()
    {
        var slow = TimeSpan.FromMilliseconds(1000);
        var fast = TimeSpan.FromMilliseconds(500);

        var beforeChange = MetronomeScheduler.GetTick(slow, TimeSpan.Zero, slow);
        Assert.Equal(slow, beforeChange.Delay);

        var afterChange = MetronomeScheduler.GetTick(
            beforeChange.NextTick,
            beforeChange.Delay,
            fast,
            slow);

        Assert.Equal(fast, afterChange.Delay);
        Assert.Equal(beforeChange.Delay + fast + fast, afterChange.NextTick);
    }

    [Fact]
    public void GetTick_WhenJitterExceedsIntervalAtFastTempo_DoesNotSkipPulses()
    {
        var fastInterval = TimeSpan.FromMilliseconds(171);
        var overshoot = TimeSpan.FromMilliseconds(250);

        var elapsed = TimeSpan.Zero;
        var nextTick = fastInterval;
        var previousInterval = (TimeSpan?)null;
        TimeSpan? lastPulse = null;
        var gaps = new List<TimeSpan>();

        for (var i = 0; i < 20; i++)
        {
            var tick = MetronomeScheduler.GetTick(nextTick, elapsed, fastInterval, previousInterval);
            previousInterval = fastInterval;
            nextTick = tick.NextTick;

            elapsed += tick.Delay + overshoot;

            if (lastPulse is { } previousPulse)
                gaps.Add(elapsed - previousPulse);

            lastPulse = elapsed;
        }

        Assert.Equal(19, gaps.Count);
        Assert.All(gaps, gap => Assert.True(gap < fastInterval + overshoot, $"Se salto un pulso: hueco de {gap}"));
    }
}