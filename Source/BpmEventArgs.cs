using System;

namespace RealtimeBeatDetector
{
    public class BpmEventArgs : EventArgs
    {
        public double Bpm { get; set; }
        public double AvgBpm { get; set; }

        public BpmEventArgs(double bpm, double avgBpm = 0)
        {
            Bpm = bpm;
            AvgBpm = avgBpm;
        }
    }
}
