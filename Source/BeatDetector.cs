using System;

namespace RealtimeBeatDetector
{
    public class BeatDetector
    {
        public event EventHandler BeatDetected;
        public event EventHandler<BpmEventArgs> BpmDetected;

        public const int EnergyBuffLen = 100;
        public const int AvgWindowWidth = 40;
        public const int BeatMinSpacingMillis = 400;

        public BufferList<float> energyBuffer = new BufferList<float>(EnergyBuffLen);
        public BufferList<float> bpmBuffer = new BufferList<float>(4);
        public float[] localAvgBuffer = new float[EnergyBuffLen];
        public float[] localDiffBuffer = new float[EnergyBuffLen];

        private DateTime lastBeat = DateTime.Now;

        public float BeatThreshold { get; set; } = 300;

        public void ProcessBuffer(byte[] buffer, int length)
        {
            float[] samples = PCMUtils.PCM32ToSamples(buffer, length);
            float energy = 0;

            for (int i = 0; i < samples.Length; i++)
            {
                float s = samples[i];
                energy += s * s;
            }

            energyBuffer.Add(energy);

            if (energyBuffer.Count == EnergyBuffLen)
            {
                ComputeLocalDiff();
            }
        }

        private void ComputeLocalDiff()
        {
            float max = 0;

            for (int i = 0; i < EnergyBuffLen - AvgWindowWidth; i++)
            {
                float avg = GetAveragePastEnergy(i, AvgWindowWidth);
                float avg2 = GetAveragePastEnergy(i, 3);
                float diff = Math.Max(0, avg2 - avg);

                if (diff > max)
                {
                    max = diff;
                }

                localAvgBuffer[i] = avg;
                localDiffBuffer[i] = diff;
            }


            if (localDiffBuffer[0] > BeatThreshold)
            {
                DetectBeat();
            }

            max /= 3;

            BeatThreshold += (max - BeatThreshold) * (GetAveragePastDiff(0, (EnergyBuffLen - AvgWindowWidth) / 2) / 10000);
        }

        private float GetAveragePastEnergy(int index, int radius)
        {
            float avg = 0;

            for (int j = 0; j <= radius; j++)
            {
                avg += energyBuffer[index + j];
            }

            return avg / radius;
        }

        private float GetAveragePastDiff(int start, int end)
        {
            float avg = 0;

            for (int j = start; j < end; j++)
            {
                avg += localDiffBuffer[j];
            }

            return avg / (end - start);
        }

        private float GetPastDiff(int start)
        {
            float diff = 0;

            for (int j = start; j < EnergyBuffLen - AvgWindowWidth - 1; j++)
            {
                diff += Math.Abs(localDiffBuffer[j] - localDiffBuffer[j + 1]);
            }

            return diff;
        }

        private void DetectBeat()
        {
            DateTime now = DateTime.Now;
            double millis = now.Subtract(lastBeat).TotalMilliseconds;

            if (millis > BeatMinSpacingMillis)
            {
                BeatDetected?.Invoke(this, null);

                float bpm = (float)(60 / millis * 1000);

                bpmBuffer.Add(bpm);

                float avgBpm = 0;

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
}
