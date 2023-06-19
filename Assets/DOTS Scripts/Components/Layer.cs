using System;

[Flags]
public enum Layer : uint
{
    Nothing = 0,
    Default = 1 << 0,
    TransparentFX = 1 << 1,
    IgnoreRayCast = 1 << 2,
    World = 1 << 3,
    Water = 1 << 4,
    UI = 1 << 5,
    Tank = 1 << 6,

    All = ~0u

}
