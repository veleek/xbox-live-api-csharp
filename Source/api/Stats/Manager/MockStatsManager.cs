﻿// Copyright (c) Microsoft Corporation
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Xbox.Services.Stats.Manager
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;

    using Microsoft.Xbox.Services.Leaderboard;

    class MockStatsManager : IStatsManager
    {
        private StatsValueDocument statValueDocument;
        private List<StatEvent> statEventList;
        private MockLeaderboardService leaderboardService;

        internal MockStatsManager()
        {
            this.LocalUsers = new List<XboxLiveUser>();
            Dictionary<string, Models.Stat> statMap = new Dictionary<string, Models.Stat>
            {
                {
                    "DefaultNum", new Models.Stat()
                    {
                        Value = 1.5f
                    }
                },
                {
                    "DefaultString", new Models.Stat()
                    {
                        Value = "stringVal"
                    }
                },
                {
                    "Default", new Models.Stat()
                    {
                        Value = 1
                    }
                }
            };

            this.statValueDocument = new StatsValueDocument(statMap);

            this.statEventList = new List<StatEvent>();
        }

        public IList<XboxLiveUser> LocalUsers { get; private set; }

        public void AddLocalUser(XboxLiveUser user)
        {
            this.LocalUsers.Add(user);
            this.leaderboardService = new MockLeaderboardService();
            this.statEventList.Add(new StatEvent(StatEventType.LocalUserAdded, user, null, new StatEventArgs()));
        }

        public void RemoveLocalUser(XboxLiveUser user)
        {
            this.LocalUsers.Remove(user);
            this.statEventList.Add(new StatEvent(StatEventType.LocalUserRemoved, user, null, new StatEventArgs()));
        }

        public StatValue GetStat(XboxLiveUser user, string statName)
        {
            return this.statValueDocument.GetStat(statName);
        }

        public List<string> GetStatNames(XboxLiveUser user)
        {
            return this.statValueDocument.GetStatNames();
        }

        public void SetStatAsNumber(XboxLiveUser user, string statName, double value)
        {
            this.statValueDocument.SetStat(statName, value);
            this.RequestFlushToService(user);
        }

        public void SetStatAsInteger(XboxLiveUser user, string statName, Int64 value)
        {
            this.statValueDocument.SetStat(statName, (double)value);
            this.RequestFlushToService(user);
        }

        public void SetStatAsString(XboxLiveUser user, string statName, string value)
        {
            this.statValueDocument.SetStat(statName, value);
            this.RequestFlushToService(user);
        }

        public void RequestFlushToService(XboxLiveUser user, bool isHighPriority = false)
        {
            this.statValueDocument.DoWork();
        }

        public List<StatEvent> DoWork()
        {
            var copyList = this.statEventList.ToList();

            this.statValueDocument.DoWork();
            this.statEventList.Clear();
            return copyList;
        }

        public void GetLeaderboard(XboxLiveUser user, LeaderboardQuery query)
        {
            if (!this.LocalUsers.Contains(user))
            {
                throw new ArgumentException("Local User needs to be added.");
            }

            this.leaderboardService.GetLeaderboardAsync(user, query).ContinueWith(responseTask =>
            {
                this.statEventList.Add(
                    new StatEvent(StatEventType.GetLeaderboardComplete,
                        user,
                        responseTask.Exception,
                        new LeaderboardResultEventArgs(responseTask.Result)
                    ));
            });
        }
    }
}