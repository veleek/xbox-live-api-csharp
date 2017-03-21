// -----------------------------------------------------------------------
//  <copyright file="StatEvent.cs" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      Internal use only.
//  </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Xbox.Services.Stats.Manager
{
    using global::System;

    public class StatEvent
    {
        public StatEventType EventType { get; private set; }
        public StatEventArgs EventArgs { get; private set; }
        public XboxLiveUser LocalUser { get; private set; }
        public Exception ErrorInfo { get; private set; }

        public StatEvent(StatEventType eventType, XboxLiveUser user, Exception errorInfo, StatEventArgs args)
        {
            this.EventType = eventType;
            this.LocalUser = user;
            this.ErrorInfo = errorInfo;
            this.EventArgs = args;
        }
    }
}