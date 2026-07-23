namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class ComplexResponse
{
    public required string Name { get; set; }
    public required Guid Uuid { get; set; }
    public required Uri ResourceUri { get; set; }
    public required Version Ver { get; set; }
    public required int Foo { get; set; }
    public required long Bar { get; set; }
    public required decimal Baz { get; set; }
    public required BigInteger BigInt { get; set; }
    public required Dictionary<char, double> Dict { get; set; }
    public required ComplexResponseSubclass[] Subs { get; set; }

    public required DateTime ProcessedAt { get; set; }
    public required DateTimeOffset ProcessedAtOffset { get; set; }

    public class ComplexResponseSubclass
    {
        public required ushort[] Numbers { get; set; }
        public required bool[] Bits { get; set; }
        public required byte[] Bytes { get; set; }

        public required TimeSpan SpanOfATime { get; set; }
    }

    public static ComplexResponse Create(int seed)
    {
        var rng = new Random(seed);

        var nameChars = new char[rng.Next(2, 11)];
        for (int i = 0; i < nameChars.Length; i++)
            nameChars[i] = (char)rng.Next(33, 127);
        var name = new string(nameChars);

        var guidBytes = new byte[16];
        rng.NextBytes(guidBytes);
        var uuid = new Guid(guidBytes);

        var scheme = rng.Next(0, 2) == 0 ? "https" : "http";
        var resourceUri = new Uri($"{scheme}://example.com/api/{rng.Next()}/{rng.Next()}");

        var ver = new Version(
            rng.Next(0, 1000),
            rng.Next(0, 1000),
            rng.Next(0, 1000),
            rng.Next(0, 1000)
        );

        var intBytes = new byte[4];
        rng.NextBytes(intBytes);
        var foo = BitConverter.ToInt32(intBytes, 0);

        var longBytes = new byte[8];
        rng.NextBytes(longBytes);
        var bar = BitConverter.ToInt64(longBytes, 0);

        var decBytes = new byte[12];
        rng.NextBytes(decBytes);
        int lo = BitConverter.ToInt32(decBytes, 0);
        int mid = BitConverter.ToInt32(decBytes, 4);
        int hi = BitConverter.ToInt32(decBytes, 8);
        var isNegative = rng.Next(0, 2) == 1;
        var scale = (byte)rng.Next(0, 29);
        var baz = new decimal(lo, mid, hi, isNegative, scale);

        var bigIntBytes = new byte[rng.Next(4, 33)];
        rng.NextBytes(bigIntBytes);
        var bigInt = new BigInteger(bigIntBytes);

        var dictCount = rng.Next(2, 11);
        var dict = new Dictionary<char, double>(dictCount);
        while (dict.Count < dictCount)
        {
            var key = (char)rng.Next(33, 127);
            if (!dict.ContainsKey(key))
            {
                var rawBytes = new byte[8];
                rng.NextBytes(rawBytes);
                var mantissa53 = BitConverter.ToInt64(rawBytes, 0) >> 11;
                var exponent = rng.Next(-500, 500);
                var exactDouble = Math.ScaleB(mantissa53, exponent);

                dict[key] = exactDouble;
            }
        }

        var subCount = rng.Next(2, 11);
        var subs = new ComplexResponseSubclass[subCount];
        for (int i = 0; i < subCount; i++)
        {
            var numbers = new ushort[rng.Next(2, 11)];
            for (int j = 0; j < numbers.Length; j++)
                numbers[j] = (ushort)rng.Next(0, 65536);

            bool[] bits = new bool[rng.Next(2, 11)];
            for (int j = 0; j < bits.Length; j++)
                bits[j] = rng.Next(0, 2) == 1;

            var bytes = new byte[rng.Next(2, 11)];
            rng.NextBytes(bytes);

            var tsBytes = new byte[8];
            rng.NextBytes(tsBytes);
            var rawTicks = BitConverter.ToInt64(tsBytes, 0);
            var ticks = Math.Clamp(rawTicks, TimeSpan.MinValue.Ticks, TimeSpan.MaxValue.Ticks);
            var spanOfATime = TimeSpan.FromTicks(ticks);

            subs[i] = new ComplexResponseSubclass
            {
                Numbers = numbers,
                Bits = bits,
                Bytes = bytes,
                SpanOfATime = spanOfATime
            };
        }

        return new ComplexResponse
        {
            Name = name,
            Uuid = uuid,
            ResourceUri = resourceUri,
            Ver = ver,
            Foo = foo,
            Bar = bar,
            Baz = baz,
            BigInt = bigInt,
            Dict = dict,
            Subs = subs,

            ProcessedAt = DateTime.UtcNow,
            ProcessedAtOffset = DateTimeOffset.UtcNow
        };
    }

    public static bool Validate(int seed, ComplexResponse other)
    {
        if (other is null)
            return false;

        var expected = Create(seed);

        if (other.Name != expected.Name)
            return false;
        if (other.Uuid != expected.Uuid)
            return false;
        if (other.ResourceUri != expected.ResourceUri)
            return false;
        if (other.Ver != expected.Ver)
            return false;
        if (other.Foo != expected.Foo)
            return false;
        if (other.Bar != expected.Bar)
            return false;
        if (other.Baz != expected.Baz)
            return false;
        if (other.BigInt != expected.BigInt)
            return false;

        if (other.Dict is null || other.Dict.Count != expected.Dict.Count)
            return false;
        foreach (var kvp in expected.Dict)
        {
            if (!other.Dict.TryGetValue(kvp.Key, out double actualValue))
                return false;
            if (BitConverter.DoubleToInt64Bits(kvp.Value) != BitConverter.DoubleToInt64Bits(actualValue))
                return false;
        }

        if (other.Subs is null || other.Subs.Length != expected.Subs.Length)
            return false;
        for (int i = 0; i < expected.Subs.Length; i++)
        {
            var expSub = expected.Subs[i];
            var actSub = other.Subs[i];

            if (actSub is null)
                return false;
            if (expSub.SpanOfATime != actSub.SpanOfATime)
                return false;

            if (actSub.Numbers is null || !expSub.Numbers.SequenceEqual(actSub.Numbers))
                return false;
            if (actSub.Bits is null || !expSub.Bits.SequenceEqual(actSub.Bits))
                return false;
            if (actSub.Bytes is null || !expSub.Bytes.SequenceEqual(actSub.Bytes))
                return false;
        }

        return true;
    }
}
