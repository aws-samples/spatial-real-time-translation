using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public static class AudioUtils
{
    public static byte[] To16BitPCM(AudioClip clip)
    {
        //Get array of float values representing audio.
        var samples = new float[clip.samples];
        clip.GetData(samples, 0);

        //bytesData array is twice the size of
        //dataSource array because a float converted in Int16 is 2 bytes.
        Int16[] intData = new Int16[samples.Length];
        Byte[] bytesData = new Byte[samples.Length * 2];

        //magic number to convert float to Int16. 
        //This works because float values for audio are between -1 and 1. 
        //The max an int16 can represent is 32767 so we can use this to remap.
        var rescaleFactor = System.Int16.MaxValue;

        for (int i = 0; i < samples.Length; i++)
        {
            //get our Int16 representation of a float value.
            intData[i] = (short)(samples[i] * rescaleFactor);
            Byte[] byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }
        return bytesData;
    }

    //https://docs.aws.amazon.com/transcribe/latest/dg/event-stream.html
    public static byte[] CreateAudioEvent(byte[] payload) //CRCs and prelude must be bigendian.
    {
        Encoding utf8 = Encoding.UTF8;
        //Build our headers
        //ContentType
        List<byte> contentTypeHeader = GetHeaders(":content-type", "application/octet-stream");
        List<byte> eventTypeHeader = GetHeaders(":event-type", "AudioEvent");
        List<byte> messageTypeHeader = GetHeaders(":message-type", "event");
        List<byte> headers = new List<byte>();
        headers.AddRange(contentTypeHeader);
        headers.AddRange(eventTypeHeader);
        headers.AddRange(messageTypeHeader);

        //Calculate total byte length and headers byte length
        byte[] totalByteLength = BitConverter.GetBytes(headers.Count + payload.Length + 16); //16 accounts for 8 byte prelude, 2x 4 byte crcs.
        if (BitConverter.IsLittleEndian)
            Array.Reverse(totalByteLength);

        byte[] headersByteLength = BitConverter.GetBytes(headers.Count);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(headersByteLength);

        //Build the prelude
        byte[] prelude = new byte[8];
        totalByteLength.CopyTo(prelude, 0);
        headersByteLength.CopyTo(prelude, 4);

        //calculate checksum for prelude (total + headers)
        var crc32 = new Crc32();
        byte[] preludeCRC = BitConverter.GetBytes(crc32.Get(prelude));
        if (BitConverter.IsLittleEndian)
            Array.Reverse(preludeCRC);

        //Construct the message
        List<byte> messageAsList = new List<byte>();
        messageAsList.AddRange(prelude);
        messageAsList.AddRange(preludeCRC);
        messageAsList.AddRange(headers);
        messageAsList.AddRange(payload);

        //Calculate checksum for message
        byte[] message = messageAsList.ToArray();
        byte[] messageCRC = BitConverter.GetBytes(crc32.Get(message));
        if (BitConverter.IsLittleEndian)
            Array.Reverse(messageCRC);

        //Add message checksum
        messageAsList.AddRange(messageCRC);
        message = messageAsList.ToArray();

        return message;
    }

    private static List<byte> GetHeaders(string headerName, string headerValue)
    {
        Encoding utf8 = Encoding.UTF8;
        byte[] name = utf8.GetBytes(headerName);
        byte[] nameByteLength = new byte[] { Convert.ToByte(name.Length) };
        byte[] valueType = new byte[] { Convert.ToByte(7) }; //7 represents a string
        byte[] value = utf8.GetBytes(headerValue);
        byte[] valueByteLength = new byte[2];
        //byte length array is always two bytes regardless of the int it represents.
        valueByteLength[0] = (byte)((value.Length & 0xFF00) >> 8);
        valueByteLength[1] = (byte)(value.Length & 0x00FF);

        //Construct the header
        List<byte> headerList = new List<byte>();
        headerList.AddRange(nameByteLength);
        headerList.AddRange(name);
        headerList.AddRange(valueType);
        headerList.AddRange(valueByteLength);
        headerList.AddRange(value);

        return headerList;
    }

    // public static AudioClip ConvertStreamToClip(Stream stream, int sampleRate = 16000)
    // {
    //     float[] data = ConvertByteToFloat(ConvertStreamToByte(stream));
    //     AudioClip clip = AudioClip.Create("pollyClip", data.Length, 1, sampleRate, false);

    //     if (clip.SetData(data, 0))
    //     {
    //         return clip;
    //     }
    //     return null;
    // }

    // private static byte[] ConvertStreamToByte(Stream stream)
    // {
    //     byte[] buffer = new byte[4];
    //     using (MemoryStream ms = new MemoryStream())
    //     {
    //         int read;
    //         while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
    //         {
    //             ms.Write(buffer, 0, read);
    //         }
    //         return ms.ToArray();
    //     }
    // }

    // private static float[] ConvertByteToFloat(byte[] bytes)
    // {
    //     float[] floatArr = new float[bytes.Length / 4];
    //     for (int i = 0; i < floatArr.Length; i++)
    //     {
    //         if (BitConverter.IsLittleEndian)
    //         {
    //             Array.Reverse(bytes, i * 4, 4);
    //         }
    //         floatArr[i] = BitConverter.ToSingle(bytes, i * 4) / int.MaxValue; //0x80000000
    //     }
    //     return floatArr;
    // }
}