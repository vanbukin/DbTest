﻿using Dodo.Primitives;

namespace DbTest;

public delegate Guid IdGenerator(DateTime createdAt, string value);

public static class IdGenerators
{
    public static readonly IdGenerator GuidNewGuid = GenerateGuidNewGuid;
    public static readonly IdGenerator UuidDbOptimized = GenerateUuidDbOptimized;


    private static Guid GenerateGuidNewGuid(DateTime createdAt, string value)
    {
        return Guid.NewGuid();
    }

    private static Guid GenerateUuidDbOptimized(DateTime createdAt, string value)
    {
        return NewDbOptimized(createdAt).ToGuidStringLayout();
    }

    private static unsafe Uuid NewDbOptimized(DateTime date)
    {
        const long christianCalendarGregorianReformTicksDate = 499_163_040_000_000_000L;
        const byte resetVersionMask = 0b0000_1111;
        const byte version1Flag = 0b0001_0000;

        const byte resetReservedMask = 0b0011_1111;
        const byte reservedFlag = 0b1000_0000;
        byte* resultPtr = stackalloc byte[16];
        var resultAsGuidPtr = (Guid*)resultPtr;
        var guid = Guid.NewGuid();
        resultAsGuidPtr[0] = guid;
        long currentTicks = date.Ticks - christianCalendarGregorianReformTicksDate;
        var ticksPtr = (byte*)&currentTicks;
        resultPtr[0] = (byte)((ticksPtr[7] & resetVersionMask) | version1Flag);
        resultPtr[1] = ticksPtr[6];
        resultPtr[2] = ticksPtr[5];
        resultPtr[3] = ticksPtr[4];
        resultPtr[4] = ticksPtr[3];
        resultPtr[5] = ticksPtr[2];
        resultPtr[6] = ticksPtr[1];
        resultPtr[7] = ticksPtr[0];
        resultPtr[8] = (byte)((resultPtr[8] & resetReservedMask) | reservedFlag);
        return new Uuid(resultPtr);
    }
}