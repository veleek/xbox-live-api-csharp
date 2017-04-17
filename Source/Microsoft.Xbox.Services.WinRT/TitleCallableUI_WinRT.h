#pragma once
#include "pch.h"

namespace Microsoft {
    namespace Xbox {
        namespace Services {
            namespace WinRT {

    public enum class GamingPrivilege sealed
    {
        /// <summary>The user can broadcast live gameplay.</summary>
        Broadcast = 190,

        /// <summary>The user can view other user's friends list if this privilege is present.</summary>
        ViewFriendsList = 197,

        /// <summary>The user can upload recorded in-game videos to the cloud if this privilege is present. Viewing GameDVRs is subject to privacy controls.</summary>
        GameDVR = 198,

        /// <summary>Kinect recorded content can be uploaded to the cloud for the user and made accessible to anyone if this privilege is present. Viewing other user's Kinect content is subject to a privacy setting.</summary>
        ShareKinectContent = 199,

        /// <summary>The user can join a party session if this privilege is present</summary>
        MultiplayerParties = 203,

        /// <summary>The user can participate in voice chat during parties and multiplayer game sessions if this privilege is present. Communicating with other users is subject to additional privacy permission checks</summary>
        CommunicationVoiceIngame = 205,

        /// <summary>The user can use voice communication with Skype on Xbox One if this privilege is present</summary>
        CommunicationVoiceSkype =206,

        /// <summary>The user can allocate a cloud compute cluster and manage a cloud compute cluster for a hosted game session if this privilege is present</summary>
        CloudGamingManageSession = 207,

        /// <summary>The user can join a cloud compute session if this privilege is present</summary>
        CloudGamingJoinSession = 208,

        /// <summary>The user can save games in cloud title storage if this privilege is present</summary>
        CloudSavedGames = 209,

        /// <summary>The user can share content with others if this privilege is present</summary>
        ShareContent = 211,

        /// <summary>The user can purchase, download and launch premium content available with the Xbox LIVE Gold subscription if this privilege is present</summary>
        PremiumContent = 214,

        /// <summary>The user can purchase and download premium subscription content and use premium subscription features when this privilege is present</summary>
        SubscriptionContent = 219,

        /// <summary>The user is allowed to share progress information on social networks when this privilege is present</summary>
        SocialNetworkSharing = 220,

        /// <summary>The user can access premium video services if this privilege is present</summary>
        PremiumVideo = 224,

        /// <summary>The user can use video communication with Skype or other providers when this privilege is present. Communicating with other users is subject to additional privacy permission checks</summary>
        VideoCommunications = 235,

        /// <summary>The user is authorized to purchase content when this privilege is present</summary>
        PurchaseContent = 245,

        /// <summary>The user is authorized to download and view online user created content when this privilege is present.</summary>
        UserCreatedContent = 247,

        /// <summary>The user is authorized to view other user's profiles when this privilege is present. Viewing other user's profiles is subject to additional privacy checks</summary>
        ProfileViewing = 249,

        /// <summary>The user can use asynchronous text messaging with anyone when this privilege is present. Extra privacy permissions checks are required to determine who the user is authorized to communicate with. Communicating with other users is subject to additional privacy permission checks</summary>
        Communications = 252,

        /// <summary>The user can join a multiplayer sessions for a game when this privilege is present.</summary>
        MultiplayerSessions = 254,

        /// <summary>The user can follow other Xbox LIVE users and add Xbox LIVE friends when this privilege is present.</summary>
        AddFriend = 255
    };
    
    public ref class TitleCallableUI sealed
    {
    public:
        
        static Windows::Foundation::IAsyncAction^ ShowProfileCardUIAsync(
            _In_ Platform::String^ targetXboxUserId,
            _In_ Windows::System::User^ user
            );

        static bool CheckPrivilegeSilently(
            _In_ GamingPrivilege privilege,
            _In_ Windows::System::User^ user,
            _In_ Platform::String^ scope,
            _In_ Platform::String^ policy
            );

        static Windows::Foundation::IAsyncOperation<bool>^ CheckPrivilegeWithUIAsync(
            _In_ GamingPrivilege privilege,
            _In_opt_ Platform::String^ friendlyMessage,
            _In_ Windows::System::User^ user,
            _In_ Platform::String^ scope,
            _In_ Platform::String^ policy
            );
    };

}}}}
