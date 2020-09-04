using System;

namespace BeatSlayerServer.Enums.Game
{
    [Flags]
    public enum ModEnum
    {
        None =          0,
        NoFail =        1,
        Easy =          2,
        Hard =          4,
        DoubleTime =    8,
        OneTry =        16,
        NightCore =     32,
        NoBombs =       64,
        NoArrows =      128
    }
}
