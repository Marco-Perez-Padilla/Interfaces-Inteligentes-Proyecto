//  Derived from Darktable's script
//  https://gist.github.com/darktable/2317063
//
//  This version keeps the original style but returns a byte[] instead of writing to disk.

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public static class SavWavMemory
{
    const int HEADER_SIZE = 44;

    // ---------------------------
    // Public interface
    // ---------------------------
    public static byte[] FromAudioClip(AudioClip clip, bool trimSilence = false, float min = 0.01f)
    {
        AudioClip processedClip = clip;
        if (trimSilence)
        {
            processedClip = TrimSilence(clip, min);
        }

        return ConvertAndWriteToMemory(processedClip);
    }

    public static AudioClip TrimSilence(AudioClip clip, float min)
    {
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        return TrimSilence(new List<float>(samples), min, clip.channels, clip.frequency);
    }

    public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz)
    {
        return TrimSilence(samples, min, channels, hz, false);
    }

    public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz, bool stream)
    {
        int start = 0;
        int end = samples.Count - 1;

        for (; start < samples.Count; start++)
            if (Mathf.Abs(samples[start]) > min) break;

        for (; end >= 0; end--)
            if (Mathf.Abs(samples[end]) > min) break;

        if (end < start) end = start;

        var trimmedSamples = samples.GetRange(start, end - start + 1);

        var clip = AudioClip.Create("TempClip", trimmedSamples.Count / channels, channels, hz, stream);
        clip.SetData(trimmedSamples.ToArray(), 0);
        return clip;
    }

    // ---------------------------
    // Internal memory WAV writer
    // ---------------------------
    static byte[] ConvertAndWriteToMemory(AudioClip clip)
    {
        using (var stream = new MemoryStream())
        {
            int samplesCount = clip.samples * clip.channels;
            float[] samples = new float[samplesCount];
            clip.GetData(samples, 0);

            // Header placeholder
            WriteWavHeader(stream, clip.channels, clip.frequency, samplesCount);

            // Convert floats to PCM16
            const int rescaleFactor = 32767;
            byte[] bytesData = new byte[samplesCount * 2];
            for (int i = 0; i < samplesCount; i++)
            {
                short val = (short)(samples[i] * rescaleFactor);
                BitConverter.GetBytes(val).CopyTo(bytesData, i * 2);
            }

            stream.Write(bytesData, 0, bytesData.Length);
            return stream.ToArray();
        }
    }

    static void WriteWavHeader(Stream stream, int channels, int sampleRate, int samplesCount)
    {
        stream.Write(Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(36 + samplesCount * 2), 0, 4);
        stream.Write(Encoding.UTF8.GetBytes("WAVEfmt "), 0, 8);
        stream.Write(BitConverter.GetBytes(16), 0, 4); // Subchunk1Size
        stream.Write(BitConverter.GetBytes((short)1), 0, 2); // PCM
        stream.Write(BitConverter.GetBytes((short)channels), 0, 2);
        stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
        stream.Write(BitConverter.GetBytes(sampleRate * channels * 2), 0, 4); // byteRate
        stream.Write(BitConverter.GetBytes((short)(channels * 2)), 0, 2); // blockAlign
        stream.Write(BitConverter.GetBytes((short)16), 0, 2); // bitsPerSample
        stream.Write(Encoding.UTF8.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(samplesCount * 2), 0, 4);
    }
}
