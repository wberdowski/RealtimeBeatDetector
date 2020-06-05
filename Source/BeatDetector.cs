using System;
using System.Collections.Generic;

namespace RealtimeBeatDetector
{
    public class BeatDetector
    {
        public int EnergyBufferLength { get; set; } = 120; // 30
        public int BpmBufferLength { get; set; } = 4; // 4
        public int BeatMinSpacingMillis { get; set; } = 300; // 300
        public double MinBeatThreshold { get; set; } = 10; // 40
        public double Peak { get; set; }
        public readonly List<double> energyBuffer = new List<double>();
        public readonly List<double> bpmBuffer = new List<double>();
        public event EventHandler BeatDetected;
        public event EventHandler<BpmEventArgs> BpmDetected;
        private DateTime lastBeat = DateTime.Now;

        public BeatDetector()
        {

        }

        public BeatDetector(int energyBufferLength = 120, int bpmBufferLength = 4, int beatMinSpacingMillis = 300, double minBeatThreshold = 10)
        {
            EnergyBufferLength = energyBufferLength;
            BpmBufferLength = bpmBufferLength;
            BeatMinSpacingMillis = beatMinSpacingMillis;
            MinBeatThreshold = minBeatThreshold;
        }

        public void ProcessBuffer(byte[] buffer, int length)
        {
            double energy = 0;
            float[] samples = new float[length / sizeof(float)];
            Buffer.BlockCopy(buffer, 0, samples, 0, length);

            for (int i = 0; i < samples.Length; i += 2)
            {
                double s = MergeSamples(samples, i, 2);

                if (s > Peak)
                {
                    Peak = s;
                }

                energy += Math.Pow(s, 2);
            }

            DetectBeat(energy);
        }

        private void DetectBeat(double energy)
        {
            double avgBufferEnergy = 0;

            bool isMax = true;
            int count = 0;
            Peak = 0;

            for (int i = 0; i < energyBuffer.Count; i++)
            {
                if (i < EnergyBufferLength / 4)
                {
                    if (energyBuffer[i] > energy)
                    {
                        isMax = false;
                    }

                    avgBufferEnergy += energyBuffer[i];
                    count++;
                }

                if (energyBuffer[i] > Peak)
                {
                    Peak = energyBuffer[i];
                }
            }

            avgBufferEnergy /= count;

            double BeatThreshold = Math.Max(MinBeatThreshold, Peak * 0.40f);/*Math.Max(MinBeatThreshold, avgBufferEnergy * 1.75f);*/

            if (isMax)
            {
                if (energy > BeatThreshold)
                {
                    DateTime now = DateTime.Now;
                    double millis = now.Subtract(lastBeat).TotalMilliseconds;
                    if (millis > BeatMinSpacingMillis)
                    {
                        BeatDetected?.Invoke(this, null);

                        double bpm = 60 / millis * 1000;
                        double avgBpm = 0;

                        bpmBuffer.Insert(0, bpm);
                        if (bpmBuffer.Count > BpmBufferLength)
                        {
                            bpmBuffer.RemoveAt(bpmBuffer.Count - 1);
                        }

                        for (int i = 0; i < bpmBuffer.Count; i++)
                        {
                            avgBpm += bpmBuffer[i];
                        }

                        avgBpm /= bpmBuffer.Count;

                        BpmDetected?.Invoke(this, new BpmEventArgs(bpm, avgBpm));

                        lastBeat = now;
                    }
                }
            }

            energyBuffer.Insert(0, energy);
            if (energyBuffer.Count > EnergyBufferLength)
            {
                energyBuffer.RemoveAt(energyBuffer.Count - 1);
            }
        }

        private float MergeSamples(float[] samples, int index, int channelCount)
        {
            float z = 0f;
            for (int i = 0; i < channelCount; i++)
            {
                z += samples[index + i];
            }
            return z / channelCount;
        }
    }
}
