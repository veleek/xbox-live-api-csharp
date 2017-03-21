// Copyright (c) Microsoft Corporation
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Xbox.Services.Stats.Manager
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Text;
    using global::System.Threading.Tasks;

    using Newtonsoft.Json;

    public class StatsService
    {
        private readonly XboxLiveContextSettings settings;
        private readonly XboxLiveContext context;
        private readonly XboxLiveAppConfiguration config;

        private readonly string statsReadEndpoint;
        private readonly string statsWriteEndpoint;

        internal StatsService(XboxLiveContext context)
        {
            this.config = context.AppConfig;
            this.context = context;
            this.settings = context.Settings;

            this.statsReadEndpoint = context.AppConfig.GetEndpointForService("statsread");
            this.statsWriteEndpoint = context.AppConfig.GetEndpointForService("statswrite");
        }

        public Task UpdateStatsValueDocument(StatsValueDocument statValuePostDocument)
        {
            string pathAndQuery = PathAndQueryStatSubpath(
                this.context.User.XboxUserId,
                this.config.ServiceConfigurationId,
                false
            );

            XboxLiveHttpRequest req = XboxLiveHttpRequest.Create(this.settings, "POST", this.statsWriteEndpoint, pathAndQuery);
            var svdModel = new Models.StatsValueDocumentModel()
            {
                Revision = statValuePostDocument.Revision,
                Timestamp = DateTime.Now,
                Stats = new Models.Stats()
                {
                    Title = new Dictionary<string, Models.Stat>()
                }
            };

            svdModel.Stats.Title = statValuePostDocument.Stats.ToDictionary(
                stat => stat.Key,
                stat => new Models.Stat()
                {
                    Value = stat.Value.Value
                });

            req.RequestBody = JsonConvert.SerializeObject(svdModel, new JsonSerializerSettings
            {
            });

            return req.GetResponseWithAuth(this.context.User, HttpCallResponseBodyType.JsonBody).ContinueWith(task =>
            {
                XboxLiveHttpResponse response = task.Result;
                if (response.ErrorCode == 0)
                {
                    ++statValuePostDocument.Revision;
                }
            });
        }

        public Task<StatsValueDocument> GetStatsValueDocument()
        {
            string pathAndQuery = PathAndQueryStatSubpath(
                this.context.User.XboxUserId,
                this.config.ServiceConfigurationId,
                false
            );

            XboxLiveHttpRequest req = XboxLiveHttpRequest.Create(this.settings, "GET", this.statsReadEndpoint, pathAndQuery);
            return req.GetResponseWithAuth(this.context.User, HttpCallResponseBodyType.JsonBody).ContinueWith(task =>
            {
                XboxLiveHttpResponse response = task.Result;
                var svdModel = JsonConvert.DeserializeObject<Models.StatsValueDocumentModel>(response.ResponseBodyJson);
                var svd = new StatsValueDocument(svdModel.Stats.Title)
                {
                    Revision = svdModel.Revision + 1
                };
                return svd;
            });
        }

        private static string PathAndQueryStatSubpath(string xuid, string scid, bool userXuidTag)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/stats/users/");
            if (userXuidTag)
            {
                sb.AppendFormat("xuid({0})", xuid);
            }
            else
            {
                sb.Append(xuid);
            }

            sb.AppendFormat("/scids/{0}", scid);

            return sb.ToString();
        }
    }
}