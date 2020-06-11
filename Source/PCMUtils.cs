using System;

namespace RealtimeBeatDetector
{
    public abstract class PCMUtils
    {
        public static float[] PCM32ToSamples(byte[] buffer, int length)
        {
            float[] samples = new float[length / sizeof(float)];
            Buffer.BlockCopy(buffer, 0, samples, 0, length);

            float[] result = new float[length / sizeof(float) / 2];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = MergeSamples(samples, i * 2, 2);
            }

            return result;
        }

        public static float MergeSamples(float[] samples, int index, int channelCount)
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
