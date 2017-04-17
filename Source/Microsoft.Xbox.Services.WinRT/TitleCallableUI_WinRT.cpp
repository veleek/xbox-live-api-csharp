#include "pch.h"
#include <gamingtcui.h>
#include <windows.system.h>
#include <concrt.h>

#include "TitleCallableUI_WinRT.h"

//using namespace Microsoft_Xbox_Services_System; // Microsoft::Xbox::Services::System;
using namespace Platform;
using namespace concurrency;

namespace Microsoft {
    namespace Xbox {
        namespace Services {
            namespace WinRT {

class tcui_context
{
public:
    tcui_context() : m_returnCode(S_OK)
    {
    }

    HRESULT wait()
    {
        m_event.wait();
        return m_returnCode;
    }

    void set( HRESULT hr )
    {
        m_returnCode = hr;
        m_event.set();
    }

    void set(HRESULT hr, const std::vector<std::wstring>& payload)
    {
        m_payload = payload;
        m_returnCode = hr;
        m_event.set();
    }

    std::vector<std::wstring> payload()
    {
        return m_payload;
    }

private:
    HRESULT m_returnCode;
    std::vector<std::wstring> m_payload;

    concurrency::event m_event;
};

// Detect if app is MUA and API is supported
bool IsMultiUserAPISupported()
{
    // Save the result in memory, as we only need to check once;
    static int isMultiUserSupported = -1;

    // Only RS1 sdk will have this check.
#ifdef NTDDI_WIN10_RS1
    if (isMultiUserSupported == -1)
    {
        try
        {
            // all RS1 based TCUI calls are based around multi-user
            bool apiExist = Windows::Foundation::Metadata::ApiInformation::IsMethodPresent("Windows.System.UserPicker", "IsSupported");
            isMultiUserSupported = (apiExist && Windows::System::UserPicker::IsSupported()) ? 1 : 0;
        }
        catch (...)
        {
            isMultiUserSupported = 0;
        }
    }
#endif
    return isMultiUserSupported == 1;
}

void WINAPI UICompletionRoutine(
    _In_ HRESULT returnCode,
    _In_ void* context
    )
{
    tcui_context* uiContext = (tcui_context*)context;
    if (uiContext != nullptr)
    {
        try
        {
            uiContext->set(returnCode);
        }
        catch (Platform::Exception^ exception)
        {
            HRESULT hr = (HRESULT)exception->HResult;
            uiContext->set(hr);
        }
        catch (...) // everything else
        {
            HRESULT hr = E_FAIL; 
            uiContext->set(hr);
        }
    }
}

Windows::Foundation::IAsyncAction^
TitleCallableUI::ShowProfileCardUIAsync(
    _In_ Platform::String^ targetXboxUserId,
    _In_ Windows::System::User^ user
    )
{
    return concurrency::create_async([targetXboxUserId, user]()
    {
        tcui_context context;

        HRESULT hr = S_OK;
        if (user != nullptr && IsMultiUserAPISupported())
        {
            ABI::Windows::System::IUser* userAbi = reinterpret_cast<ABI::Windows::System::IUser*>(user);
            hr = ShowProfileCardUIForUser(
                userAbi,
                reinterpret_cast<HSTRING>(targetXboxUserId),
                UICompletionRoutine,
                static_cast<void*>(&context)
                );
        }
        else
        {
            hr = ShowProfileCardUI(
                reinterpret_cast<HSTRING>(targetXboxUserId),
                UICompletionRoutine,
                static_cast<void*>(&context)
                );
        }

        if (SUCCEEDED(hr) || hr == E_PENDING)
        {
            hr = ProcessPendingGameUI(true);
            if (SUCCEEDED(hr))
            {
                hr = context.wait();
            }
        }

        if (FAILED(hr)) 
        {
            throw ref new Platform::Exception(hr);
        }
    });
}

bool
TitleCallableUI::CheckPrivilegeSilently(
    _In_ GamingPrivilege privilege,
    _In_ Windows::System::User^ user,
    _In_ Platform::String^ scope,
    _In_ Platform::String^ policy
    )
{
    BOOL hasPrivilege = FALSE;

    HRESULT hr = S_OK;
    if (user != nullptr && IsMultiUserAPISupported())
    {
        ABI::Windows::System::IUser* userAbi = reinterpret_cast<ABI::Windows::System::IUser*>(user);
        hr = CheckGamingPrivilegeSilentlyForUser(
            userAbi,
            static_cast<UINT32>(privilege),
            reinterpret_cast<HSTRING>(scope),
            reinterpret_cast<HSTRING>(policy),
            &hasPrivilege
        );
    }
    else
    {
        hr = CheckGamingPrivilegeSilently(
            static_cast<UINT32>(privilege),
            reinterpret_cast<HSTRING>(scope),
            reinterpret_cast<HSTRING>(policy),
            &hasPrivilege
        );
    }

    return hasPrivilege == TRUE;
}

Windows::Foundation::IAsyncOperation<bool>^
TitleCallableUI::CheckPrivilegeWithUIAsync(
    _In_ GamingPrivilege privilege,
    _In_opt_ Platform::String^ friendlyMessage,
    _In_ Windows::System::User^ user,
    _In_ Platform::String^ scope,
    _In_ Platform::String^ policy
)
{
    return concurrency::create_async([privilege, friendlyMessage, user, scope, policy]()
    {
        tcui_context context;
        BOOL hasPrivilege = FALSE;
        HRESULT hr = S_OK;

        if (user != nullptr && IsMultiUserAPISupported())
        {
            ABI::Windows::System::IUser* userAbi = reinterpret_cast<ABI::Windows::System::IUser*>(user);
            hr = CheckGamingPrivilegeWithUIForUser(
                userAbi,
                static_cast<UINT32>(privilege),
                reinterpret_cast<HSTRING>(scope),
                reinterpret_cast<HSTRING>(policy),
                reinterpret_cast<HSTRING>(friendlyMessage),
                UICompletionRoutine,
                static_cast<void*>(&context)
                );
        }
        else
        {
            hr = CheckGamingPrivilegeWithUI(
                static_cast<UINT32>(privilege),
                reinterpret_cast<HSTRING>(scope),
                reinterpret_cast<HSTRING>(policy),
                reinterpret_cast<HSTRING>(friendlyMessage),
                UICompletionRoutine,
                static_cast<void*>(&context)
                );
        }
        if (SUCCEEDED(hr) || hr == E_PENDING)
        {
            hr = ProcessPendingGameUI(true);
            if (SUCCEEDED(hr))
            {
                hr = context.wait();
                if (SUCCEEDED(hr))
                {
                    hr = CheckGamingPrivilegeSilently(
                        static_cast<UINT32>(privilege),
                        reinterpret_cast<HSTRING>(scope),
                        reinterpret_cast<HSTRING>(policy),
                        &hasPrivilege
                    );
                }
            }
        }

        return hasPrivilege == TRUE;
    });
}

}}}}