// Copyright (c) Microsoft Corporation
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Xbox.Services.Leaderboard
{
    using global::System.Threading.Tasks;

    internal interface ILeaderboardService
    {
        /// <summary>
        /// Get a leaderboard for a single leaderboard given a stat name and query parameters.
        /// </summary>
        /// <param name="statName">Stat name of the leaderboard</param>
        /// <param name="query">An object that contains query information</param>
        /// <returns>
        /// A LeaderboardResult object containing a collection of the leaderboard columns and rows
        /// </returns>
        /// <remarks>
        /// This stat needs to be configured on DevCenter for your title
        /// </remarks>
        Task<LeaderboardResult> GetLeaderboardAsync(string statName, LeaderboardQuery query);

        Task<LeaderboardResult> GetSocialLeaderboardAsync(string statName, string socialGroup, LeaderboardQuery query);
    }
}