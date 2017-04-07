// Copyright (c) Microsoft Corporation
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Xbox.Services
{
    using global::System.Threading.Tasks;
    using Microsoft.Xbox.Services.System;

    public partial class XboxLiveUser
    {
        public XboxLiveUser()
        {
            this.userImpl = new UserImpl(null, this);
        }

        public XboxLiveUser(Windows.System.User systemUser)
        {
            var user = new UserImpl(systemUser, this);
            user.SignInCompleted += (sender, args) =>
            {
                OnSignInCompleted(this);
            };
            user.SignOutCompleted += (sender, args) =>
            {
                OnSignOutCompleted(this);
            };

            this.userImpl = user;
        }

        public Task RefreshToken()
        {
            return this.userImpl.InternalGetTokenAndSignatureAsync("GET", this.userImpl.AuthConfig.XboxLiveEndpoint, null, null, false, true);
        }

        public Windows.System.User SystemUser
        {
            get
            {
                return this.userImpl.CreationContext;
            }
        }

    }
}