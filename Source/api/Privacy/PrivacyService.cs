// Copyright (c) Microsoft Corporation
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// 

namespace Microsoft.Xbox.Services.Privacy
{
    using global::System.Text;
    using global::System.Threading.Tasks;
    using global::System.Collections.Generic;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class PrivacyService : IPrivacyService
    {
        private readonly string privacyEndpoint;

        protected XboxLiveSettings settings;

        internal PrivacyService()
        {
            this.privacyEndpoint = XboxLiveAppConfiguration.Instance.GetEndpointForService("privacy");
        }

        public Task<PermissionCheckResult> CheckPermissionWithTargetUserAsync(XboxLiveUser user, string permissionId, string targetXboxUserId)
        {
            XboxLiveHttpRequest req = XboxLiveHttpRequest.Create(
                HttpMethod.Get, 
                this.privacyEndpoint,
                string.Format("/users/xuid({0})/permission/validate?setting={1}&target=xuid({2})", user.XboxUserId, permissionId, targetXboxUserId));

            return req.GetResponseWithAuth(user)
                .ContinueWith(responseTask =>
                {
                    var response = responseTask.Result;
                    JObject responseBody = JObject.Parse(response.ResponseBodyString);
                    PermissionCheckResult result = responseBody.ToObject<PermissionCheckResult>();
                    return result;
                });
        }

        public Task<List<MultiplePermissionsCheckResult>> CheckMultiplePermissionsWithMultipleTargetUsersAsync(XboxLiveUser user, IList<string> permissionIds, IList<string> targetXboxUserIds)
        {
            XboxLiveHttpRequest req = XboxLiveHttpRequest.Create(
                HttpMethod.Post,
                this.privacyEndpoint,
                string.Format("/users/xuid({0})/permission/validate", user.XboxUserId));

            Models.PrivacySettingsRequest reqBodyObject = new Models.PrivacySettingsRequest(permissionIds, targetXboxUserIds);
            req.RequestBody = JsonSerialization.ToJson(reqBodyObject);
            return req.GetResponseWithAuth(user)
                .ContinueWith(responseTask =>
                {
                    var response = responseTask.Result;
                    JObject responseBody = JObject.Parse(response.ResponseBodyString);
                    List<MultiplePermissionsCheckResult> results = responseBody["responses"].ToObject<List<MultiplePermissionsCheckResult>>();
                    return results;
                });
        }
    }
}
