using System;
using System.Diagnostics;

namespace Gameplay.Telemetry;

public sealed class PerformanceMetrics
{
    public enum ScopeKind
    {
        Update,
        Draw,
        Probe,
    }

    private const double Alpha = 0.12;

    private long _lastDrawTicks;
    private int _frames;
    private double _fpsWindowSeconds;

    private int _lastGc0, _lastGc1, _lastGc2;
    private double _gcWindowSeconds;

    private double _probeFrameSumMs;
    private int _probeFrameCalls;
    private string _probeFrameName = "";

    public int Fps { get; private set; }
    public double UpdateMs { get; private set; }
    public double DrawMs { get; private set; }
    public int Gc0Delta { get; private set; }
    public int Gc1Delta { get; private set; }
    public int Gc2Delta { get; private set; }

    public string ProbeName { get; private set; } = "";
    public double ProbeMs { get; private set; }

    public int ProbeCalls { get; private set; }

    private void ResetProbeFrame()
    {
        _probeFrameSumMs = 0;
        _probeFrameCalls = 0;
        _probeFrameName = "";
    }

    private void PublishProbeFrame()
    {
        if (_probeFrameCalls == 0) return;

        if (!ReferenceEquals(ProbeName, _probeFrameName) && ProbeName != _probeFrameName)
            ProbeName = _probeFrameName;

        ProbeCalls = _probeFrameCalls;

        var totalMs = _probeFrameSumMs;
        ProbeMs = ProbeMs <= 0 ? totalMs : Lerp(ProbeMs, totalMs, Alpha);
    }


    public Scope MeasureUpdate()
    {
        ResetProbeFrame();
        return new Scope(this, ScopeKind.Update, "");
    }

    public Scope MeasureDraw()
    {
        ResetProbeFrame();
        return new Scope(this, ScopeKind.Draw, "");
    }

    public Scope MeasureProbe(string name) => new(this, ScopeKind.Probe, name);

    public void TickDrawFrame()
    {
        var now = Stopwatch.GetTimestamp();
        var dtSeconds = ComputeDrawDtSeconds(now);
        if (dtSeconds > 0)
            OnDrawFrame(dtSeconds);
    }

    private void RecordUpdate(double ms) => UpdateMs = UpdateMs <= 0 ? ms : Lerp(UpdateMs, ms, Alpha);
    private void RecordDraw(double ms) => DrawMs = DrawMs <= 0 ? ms : Lerp(DrawMs, ms, Alpha);

    private void RecordProbe(double ms, string name)
    {
        if (_probeFrameCalls == 0)
            _probeFrameName = name;

        _probeFrameSumMs += ms;
        _probeFrameCalls++;
    }


    private void OnDrawFrame(double dtSeconds)
    {
        _frames++;
        _fpsWindowSeconds += dtSeconds;
        if (_fpsWindowSeconds >= 1.0)
        {
            Fps = (int)Math.Round(_frames / _fpsWindowSeconds);
            _frames = 0;
            _fpsWindowSeconds = 0;
        }

        _gcWindowSeconds += dtSeconds;
        if (_gcWindowSeconds >= 1.0)
        {
            var gc0 = GC.CollectionCount(0);
            var gc1 = GC.CollectionCount(1);
            var gc2 = GC.CollectionCount(2);

            Gc0Delta = gc0 - _lastGc0;
            Gc1Delta = gc1 - _lastGc1;
            Gc2Delta = gc2 - _lastGc2;

            _lastGc0 = gc0;
            _lastGc1 = gc1;
            _lastGc2 = gc2;

            _gcWindowSeconds = 0;
        }
    }

    private double ComputeDrawDtSeconds(long nowTicks)
    {
        if (_lastDrawTicks == 0)
        {
            _lastDrawTicks = nowTicks;
            return 0;
        }

        var dt = (nowTicks - _lastDrawTicks) / (double)Stopwatch.Frequency;
        _lastDrawTicks = nowTicks;
        return dt;
    }

    private static double Lerp(double a, double b, double t) => a + (b - a) * t;

    public readonly struct Scope(PerformanceMetrics metrics, ScopeKind kind, string name) : IDisposable
    {
        private readonly long _startTicks = Stopwatch.GetTimestamp();

        public void Dispose()
        {
            var end = Stopwatch.GetTimestamp();
            var ms = (end - _startTicks) * 1000.0 / Stopwatch.Frequency;

            if (kind == ScopeKind.Update)
            {
                metrics.RecordUpdate(ms);
                metrics.PublishProbeFrame();
            }
            else if (kind == ScopeKind.Draw)
            {
                metrics.RecordDraw(ms);
                metrics.PublishProbeFrame();
            }
            else
            {
                metrics.RecordProbe(ms, name);
            }
        }
    }
}