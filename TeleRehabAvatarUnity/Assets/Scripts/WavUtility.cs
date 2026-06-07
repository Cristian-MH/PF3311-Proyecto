using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    private const int TargetSampleRate = 16000;
    private const int TargetChannels = 1;
    private const short BitsPerSample = 16;

    public static byte[] FromAudioClipTo16KhzMono(AudioClip clip)
    {
        if (clip == null)
            throw new ArgumentNullException(nameof(clip));

        float[] sourceSamples = new float[clip.samples * clip.channels];
        clip.GetData(sourceSamples, 0);

        float[] monoSamples = ConvertToMono(sourceSamples, clip.channels);
        float[] resampledSamples = Resample(monoSamples, clip.frequency, TargetSampleRate);

        return ConvertToWavPcm16Mono(resampledSamples, TargetSampleRate);
    }

    private static float[] ConvertToMono(float[] sourceSamples, int sourceChannels)
    {
        if (sourceChannels == 1)
            return sourceSamples;

        int monoSampleCount = sourceSamples.Length / sourceChannels;
        float[] monoSamples = new float[monoSampleCount];

        for (int i = 0; i < monoSampleCount; i++)
        {
            float sum = 0f;

            for (int channel = 0; channel < sourceChannels; channel++)
            {
                sum += sourceSamples[i * sourceChannels + channel];
            }

            monoSamples[i] = sum / sourceChannels;
        }

        return monoSamples;
    }

    private static float[] Resample(float[] sourceSamples, int sourceSampleRate, int targetSampleRate)
    {
        if (sourceSampleRate == targetSampleRate)
            return sourceSamples;

        double resampleRatio = (double)targetSampleRate / sourceSampleRate;
        int targetSampleCount = Mathf.CeilToInt((float)(sourceSamples.Length * resampleRatio));

        float[] resampledSamples = new float[targetSampleCount];

        for (int i = 0; i < targetSampleCount; i++)
        {
            double sourceIndex = i / resampleRatio;
            int indexFloor = Mathf.FloorToInt((float)sourceIndex);
            int indexCeil = Mathf.Min(indexFloor + 1, sourceSamples.Length - 1);
            float t = (float)(sourceIndex - indexFloor);

            resampledSamples[i] = Mathf.Lerp(sourceSamples[indexFloor], sourceSamples[indexCeil], t);
        }

        return resampledSamples;
    }

    private static byte[] ConvertToWavPcm16Mono(float[] samples, int sampleRate)
    {
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream);

        int channels = TargetChannels;
        int bytesPerSample = BitsPerSample / 8;
        int dataSize = samples.Length * bytesPerSample;
        int byteRate = sampleRate * channels * bytesPerSample;
        short blockAlign = (short)(channels * bytesPerSample);

        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataSize);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1); // PCM
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write(BitsPerSample);

        writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        writer.Write(dataSize);

        foreach (float sample in samples)
        {
            short value = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
            writer.Write(value);
        }

        return stream.ToArray();
    }
}