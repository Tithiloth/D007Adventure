using System;
using System.Diagnostics;

namespace D007Adventure.Graphics
{
    // Misst die aktuellen, tatsächlichen FPS
    // Der Framelimiter begrenzt die FPS, der FpsCounter
    // misst sie und gibt sie aus
    public class FpsCounter
    {
        // Misst, wie viel Zeit seit dem letzten FPS-Update
        // vergangen ist
        private readonly Stopwatch stopwatch;

        // Zählt die Frames innerhalb des aktuellen Messintervals
        private int frameCount;

        // Zuletzt berechnete Frames
        public int CurrentFps
        {
            get;
            private set;
        }

        public FpsCounter()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();

            frameCount = 0;
            CurrentFps = 0;

            Logger.Info(
                "FpsCounter initialisiert."
            );
        }

        public void Update()
        {
            // Ein Frame wurde vollständig verarbeitet.
            frameCount++;

            // Nach einer Sekunde übernehmen wir
            // die Anzahl der gezählten Frames als FPS-Wert.
            if (stopwatch.Elapsed.TotalSeconds >= 1.0)
            {
                CurrentFps = frameCount;

                // Messung für das nächste Intervall zurüclsetzten.
                frameCount = 0;
                stopwatch.Restart();
            }
        }
    }
}