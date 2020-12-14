using System;

namespace Server.Animation
{
    [Flags]
    public enum AnimationFlags
    {
        Normal = 0,
        Loop = 1,
        StopOnLastFrame = 2,
        OnlyAnimateUpperBody = 16,
        AllowPlayerControl = 32,
        Cancellable = 120
    }
}