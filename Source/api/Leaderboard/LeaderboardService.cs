// Copyright (c) Microsoft Corporation
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Xbox.Services.Leaderboard
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Text;
    using global::System.Threading.Tasks;

    public class LeaderboardService : ILeaderboardService
    {
        private const string leaderboardApiContract = "4";

        private static readonly Uri leaderboardsBaseUri = new Uri("https://leaderboards.xboxlive.com");

        private readonly XboxLiveUser userContext;
        private readonly XboxLiveContextSettings xboxLiveContextSettings;
        private readonly XboxLiveAppConfiguration appConfig;

        internal LeaderboardService(XboxLiveUser userContext, XboxLiveContextSettings xboxLiveContextSettings, XboxLiveAppConfiguration appConfig)
        {
            this.userContext = userContext;
            this.xboxLiveContextSettings = xboxLiveContextSettings;
            this.appConfig = appConfig;
        }

        public Task<LeaderboardResult> GetLeaderboardAsync(string statName, LeaderboardQuery query)
        {
            return this.GetLeaderboardInternal(this.userContext.XboxUserId, this.appConfig.ServiceConfigurationId, statName, null, query, LeaderboardRequestType.Global);
        }

        public Task<LeaderboardResult> GetSocialLeaderboardAsync(string leaderboardName, string socialGroup, LeaderboardQuery query)
        {
            if (string.IsNullOrEmpty(socialGroup))
            {
                throw new XboxException("socialGroup parameter is required for getting Leaderboard for social group.");
            }

            if (string.Equals(socialGroup, "people", StringComparison.CurrentCultureIgnoreCase))
            {
                socialGroup = "all";
            }
            return this.GetLeaderboardInternal(this.userContext.XboxUserId, this.appConfig.ServiceConfigurationId, leaderboardName, socialGroup, query, LeaderboardRequestType.Social);
        }

        internal Task<LeaderboardResult> GetLeaderboardInternal(string xuid, string serviceConfigurationId, string leaderboardName, string socialGroup, LeaderboardQuery query, LeaderboardRequestType leaderboardType)
        {
            string skipToXboxUserId = null;
            if (query.SkipResultToMe)
            {
                skipToXboxUserId = this.userContext.XboxUserId;
            }

            string requestPath;
            if (leaderboardType == LeaderboardRequestType.Social)
            {
                requestPath = CreateSocialLeaderboardUrlPath(serviceConfigurationId, leaderboardName, xuid, query.MaxItems, skipToXboxUserId, query.SkipResultsToRank, query.ContinuationToken, socialGroup);
            }
            else
            {
                requestPath = CreateLeaderboardUrlPath(serviceConfigurationId, leaderboardName, query.MaxItems, skipToXboxUserId, query.SkipResultsToRank, query.ContinuationToken, socialGroup);
            }

            XboxLiveHttpRequest request = XboxLiveHttpRequest.Create(this.xboxLiveContextSettings, HttpMethod.Get, leaderboardsBaseUri.ToString(), requestPath);
            request.ContractVersion = leaderboardApiContract;
            return request.GetResponseWithAuth(this.userContext, HttpCallResponseBodyType.JsonBody)
                .ContinueWith(
                    responseTask =>
                    {
                        var leaderboardRequestType = LeaderboardRequestType.Global;
                        if (socialGroup != null)
                        {
                            leaderboardRequestType = LeaderboardRequestType.Social;
                        }
                        LeaderboardRequest leaderboardRequest = new LeaderboardRequest(leaderboardRequestType, leaderboardName);
                        return this.HandleLeaderboardResponse(leaderboardRequest, responseTask, query);
                    });
        }

        internal LeaderboardResult HandleLeaderboardResponse(LeaderboardRequest request, Task<XboxLiveHttpResponse> responseTask, LeaderboardQuery query)
        {
            XboxLiveHttpResponse response = responseTask.Result;

            LeaderboardResponse lbResponse = JsonSerialization.FromJson<LeaderboardResponse>(response.ResponseBodyString);

            IList<LeaderboardColumn> columns = new List<LeaderboardColumn> { lbResponse.LeaderboardInfo.Column };

            IList<LeaderboardRow> rows = new List<LeaderboardRow>();
            foreach (LeaderboardRowResponse row in lbResponse.Rows)
            {
                LeaderboardRow newRow = new LeaderboardRow
                {
                    Gamertag = row.Gamertag,
                    Percentile = row.Percentile,
                    Rank = row.Rank,
                    XboxUserId = row.XboxUserId,
                    Values = row.Value != null ? new List<string> { row.Value } : row.Values,
                };
                rows.Add(newRow);
            }

            if (lbResponse.PagingInfo != null)
            {
                query = new LeaderboardQuery(query, lbResponse.PagingInfo.ContinuationToken);
            }

            LeaderboardResult result = new LeaderboardResult(lbResponse.LeaderboardInfo.TotalCount, columns, rows, query);
            return result;
        }

        private static string CreateLeaderboardUrlPath(string serviceConfigurationId, string leaderboardName, uint maxItems, string skipToXboxUserId, uint skipToRank, string continuationToken, string socialGroup)
        {
            StringBuilder requestPath = new StringBuilder();
            requestPath.AppendFormat("scids/{0}/leaderboards/stat({1})?", serviceConfigurationId, leaderboardName);
            AppendQueryParameters(requestPath, maxItems, skipToXboxUserId, skipToRank, continuationToken, socialGroup);

            return requestPath.ToString();
        }

        private static string CreateSocialLeaderboardUrlPath(string serviceConfigurationId, string leaderboardName, string xuid, uint maxItems, string skipToXboxUserId, uint skipToRank, string continuationToken, string socialGroup)
        {
            StringBuilder requestPath = new StringBuilder();
            requestPath.AppendFormat("users/xuid({0})/scids/{1}/stats/{2}/people/{3}?", xuid, serviceConfigurationId, leaderboardName, socialGroup);
            AppendQueryParameters(requestPath, maxItems, skipToXboxUserId, skipToRank, continuationToken, null);

            return requestPath.ToString();
        }

        private static void AppendQueryParameters(StringBuilder queryString, uint maxItems, string skipToXboxUserId, uint skipToRank, string continuationToken, string socialGroup)
        {
            if (maxItems > 0)
            {
                AppendQueryParameter(queryString, "maxItems", maxItems);
            }

            if (!string.IsNullOrEmpty(skipToXboxUserId) && skipToRank > 0)
            {
                throw new ArgumentException("Cannot provide both user and rank to skip to.");
            }

            if (continuationToken != null)
            {
                AppendQueryParameter(queryString, "continuationToken", continuationToken);
            }
            else if (!string.IsNullOrEmpty(skipToXboxUserId))
            {
                AppendQueryParameter(queryString, "skipToUser", skipToXboxUserId);
            }
            else if (skipToRank > 0)
            {
                AppendQueryParameter(queryString, "skipToRank", skipToRank);
            }

            if (socialGroup != null)
            {
                AppendQueryParameter(queryString, "view", "People");
                AppendQueryParameter(queryString, "viewTarget", socialGroup);
            }

            // Remove the trailing query string bit
            queryString.Remove(queryString.Length - 1, 1);
        }

        private static void AppendQueryParameter(StringBuilder builder, string parameterName, object parameterValue)
        {
            builder.Append(parameterName);
            builder.Append("=");
            builder.Append(parameterValue);
            builder.Append("&");
        }
    }
}