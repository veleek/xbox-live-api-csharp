// -----------------------------------------------------------------------
//  <copyright file="StatEventType.cs" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      Internal use only.
//  </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Xbox.Services.Stats.Manager
{
    public enum StatEventType
    {
        LocalUserAdded,
        LocalUserRemoved,
        StatUpdateComplete,
        GetLeaderboardComplete,
    }
}