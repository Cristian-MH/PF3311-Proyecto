using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        if (clip == null)
            throw new ArgumentNullException(nameof(clip));

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        return ConvertToWav(samples, clip.channels, clip.frequency);
    }

    private static byte[] ConvertToWav(float[] samples, int channels, int sampleRate)
    {
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream);

        int sampleCount = samples.Length;
        int dataSize = sampleCount * 2;
        int byteRate = sampleRate * channels * 2;

        writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
        writer.Write(36 + dataSize);
        writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));

        writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((short)(channels * 2));
        writer.Write((short)16);

        writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
        writer.Write(dataSize);

        foreach (float sample in samples)
        {
            short value = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
            writer.Write(value);
        }

        return stream.ToArray();
    }
}