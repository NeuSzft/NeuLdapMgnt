using System.Collections.Generic;
using System.Text;

namespace NeuLdapMgnt.Models;

public partial class UserPassword {
    private const string ShadowChars = "./0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    private static readonly Dictionary<char, int> ShadowCharsLookup = new();

    static UserPassword() {
        for (int i = 0; i < ShadowChars.Length; i++)
            ShadowCharsLookup.Add(ShadowChars[i], i);
    }

    private static string EncodeBytes(byte[] bytes) {
        int remaining = bytes.Length - (bytes.Length / 3 * 3);

        StringBuilder result = new(bytes.Length * 2);

        for (int i = 0; i < bytes.Length - remaining; i += 3) {
            int value = (bytes[i] << 16) + (bytes[i + 1] << 8) + bytes[i + 2];
            result.Append(ShadowChars[value >> 18]);
            result.Append(ShadowChars[(value >> 12) & 0b00111111]);
            result.Append(ShadowChars[(value >> 6)  & 0b00111111]);
            result.Append(ShadowChars[value         & 0b00111111]);
        }

        switch (remaining) {
            case 1:
                int value1 = bytes[^1] << 16;
                result.Append(ShadowChars[value1 >> 18]);
                result.Append(ShadowChars[(value1 >> 12) & 0b00111111]);
                break;

            case 2:
                int value2 = (bytes[^2] << 16) + (bytes[^1] << 8);
                result.Append(ShadowChars[value2 >> 18]);
                result.Append(ShadowChars[(value2 >> 12) & 0b00111111]);
                result.Append(ShadowChars[(value2 >> 6)  & 0b00111111]);
                break;
        }

        return result.ToString();
    }

    private static byte[] DecodeBytes(string encoded) {
        int remaining = encoded.Length - (encoded.Length / 4 * 4);

        int size = encoded.Length / 4 * 3;
        if (remaining > 0)
            size += remaining - 1;

        byte[] result = new byte[size];
        int    index  = 0;

        for (int i = 0; i < encoded.Length - remaining; i += 4) {
            int v1 = ShadowCharsLookup[encoded[i]]     << 18;
            int v2 = ShadowCharsLookup[encoded[i + 1]] << 12;
            int v3 = ShadowCharsLookup[encoded[i + 2]] << 6;
            int v4 = ShadowCharsLookup[encoded[i + 3]];

            int sum = v1 + v2 + v3 + v4;

            result[index++] = (byte)((sum >> 16) & 0xff);
            result[index++] = (byte)((sum >> 8)  & 0xff);
            result[index++] = (byte)(sum         & 0xff);
        }

        switch (remaining) {
            case 2:
                int v21 = ShadowCharsLookup[encoded[^2]] << 18;
                int v22 = ShadowCharsLookup[encoded[^1]] << 12;

                int sum2 = v21 + v22;

                result[index] = (byte)((sum2 >> 16) & 0xff);
                break;

            case 3:
                int v31 = ShadowCharsLookup[encoded[^3]] << 18;
                int v32 = ShadowCharsLookup[encoded[^2]] << 12;
                int v33 = ShadowCharsLookup[encoded[^1]] << 6;

                int sum3 = v31 + v32 + v33;

                result[index]     = (byte)((sum3 >> 16) & 0xff);
                result[index + 1] = (byte)((sum3 >> 8)  & 0xff);
                break;
        }

        return result;
    }
}
