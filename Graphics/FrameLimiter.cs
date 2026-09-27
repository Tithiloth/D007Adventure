using System;
using System.Diagnostics;
using System.Threading;

namespace D007Adventure.Graphics
{
    // Die aktuell verfügbaren FPS-Limits.
    // Der Zahlenwert entspricht direkt der gewünschten Bildrate.
    public enum FpsLimit
    {
        Fps30 = 30,
        Fps60 = 60,
        Fps90 = 90
    }

    public class FrameLimiter
    {
        // Misst die Zeit zwischen zwei Frames.
        private readonly Stopwatch stopwatch;

        // Aktuell verwendetes FPS-Limit.
        private FpsLimit limit;

        public FpsLimit Limit
        {
            get
            {
                return limit;
            }

            set
            {
                if (limit == value)
                {
                    return;
                }

                limit = value;

                Logger.Info(
                    $"FPS-Limit geändert: {(int)limit} FPS"
                );
            }
        }

        public FrameLimiter(
            FpsLimit limit = FpsLimit.Fps60)
        {
            this.limit = limit;

            stopwatch = new Stopwatch();
            stopwatch.Start();

            Logger.Info(
                $"FrameLimiter initialisiert: {(int)limit} FPS"
            );
        }

        public void Wait()
        {
            // Wie lange darf ein Frame maximal dauern?
            double targetMilliseconds =
                1000.0 / (int)limit;

            // Wie viel Zeit ist seit dem letzten Frame vergangen?
            double elapsedMilliseconds =
                stopwatch.Elapsed.TotalMilliseconds;

            // Noch verbleibende Zeit bis zum nächsten Frame.
            double remainingMilliseconds =
                targetMilliseconds - elapsedMilliseconds;

            if (remainingMilliseconds > 1)
            {
                // Sleep gibt dem Betriebssystem die CPU-Zeit zurück.
                // Das ist deutlich effizienter als dauerhaft zu warten.
                Thread.Sleep(
                    (int)remainingMilliseconds
                );
            }

            // Sleep kann etwas ungenau sein.
            // Deshalb warten wir am Ende noch kurz aktiv,
            // bis die gewünschte Frame-Zeit wirklich erreicht ist.
            while (stopwatch.Elapsed.TotalMilliseconds
                < targetMilliseconds)
            {
                Thread.SpinWait(100);
            }

            // Messung für den nächsten Frame zurücksetzen.
            stopwatch.Restart();
        }
    }
}