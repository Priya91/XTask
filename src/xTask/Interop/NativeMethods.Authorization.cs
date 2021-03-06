﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;
    using System.Text;


    internal static partial class NativeMethods
    {
        [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
        internal static class Authorization
        {
            // In winnt.h
            private const uint PRIVILEGE_SET_ALL_NECESSARY = 1;
            internal const uint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
            internal const uint SE_PRIVILEGE_ENABLED = 0x00000002;
            internal const uint SE_PRIVILEGE_REMOVED = 0x00000004;
            internal const uint SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;

            private enum TOKEN_INFORMATION_CLASS
            {
                TokenUser = 1,
                TokenGroups,
                TokenPrivileges,
                TokenOwner,
                TokenPrimaryGroup,
                TokenDefaultDacl,
                TokenSource,
                TokenType,
                TokenImpersonationLevel,
                TokenStatistics,
                TokenRestrictedSids,
                TokenSessionId,
                TokenGroupsAndPrivileges,
                TokenSessionReference,
                TokenSandBoxInert,
                TokenAuditPolicy,
                TokenOrigin,
                TokenElevationType,
                TokenLinkedToken,
                TokenElevation,
                TokenHasRestrictions,
                TokenAccessInformation,
                TokenVirtualizationAllowed,
                TokenVirtualizationEnabled,
                TokenIntegrityLevel,
                TokenUIAccess,
                TokenMandatoryPolicy,
                TokenLogonSid,
                TokenIsAppContainer,
                TokenCapabilities,
                TokenAppContainerSid,
                TokenAppContainerNumber,
                TokenUserClaimAttributes,
                TokenDeviceClaimAttributes,
                TokenRestrictedUserClaimAttributes,
                TokenRestrictedDeviceClaimAttributes,
                TokenDeviceGroups,
                TokenRestrictedDeviceGroups,
                TokenSecurityAttributes,
                TokenIsRestricted,
                MaxTokenInfoClass
            }

            // https://msdn.microsoft.com/en-us/library/aa379261.aspx
            [StructLayout(LayoutKind.Sequential)]
            internal struct LUID
            {
                public uint LowPart;
                public uint HighPart;
            }

            // https://msdn.microsoft.com/en-us/library/aa379263.aspx
            [StructLayout(LayoutKind.Sequential)]
            internal struct LUID_AND_ATTRIBUTES
            {
                public LUID Luid;
                public uint Attributes;
            }

            // https://msdn.microsoft.com/en-us/library/aa379307.aspx
            [StructLayout(LayoutKind.Sequential)]
            private struct PRIVILEGE_SET
            {
                public uint PrivilegeCount;
                public uint Control;

                [MarshalAs(UnmanagedType.ByValArray)]
                public LUID_AND_ATTRIBUTES[] Privilege;
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379630.aspx
            [StructLayout(LayoutKind.Sequential)]
            private struct TOKEN_PRIVILEGES
            {
                public uint PrivilegeCount;

                [MarshalAs(UnmanagedType.ByValArray)]
                public LUID_AND_ATTRIBUTES[] Privileges;
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379572.aspx
            private enum SECURITY_IMPERSONATION_LEVEL
            {
                SecurityAnonymous = 0,
                SecurityIdentification = 1,
                SecurityImpersonation = 2,
                SecurityDelegation = 3
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379633.aspx
            private enum TOKEN_TYPE
            {
                TokenPrimary = 1,
                TokenImpersonation
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa375202.aspx
            [DllImport("advapi32.dll", EntryPoint = "AdjustTokenPrivileges", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool AdjustTokenPrivilegesPrivate(
                IntPtr TokenHandle,
                [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges,
                TOKEN_PRIVILEGES NewState,
                uint BufferLength,
                out TOKEN_PRIVILEGES PreviousState,
                out uint ReturnLength);


            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa446671.aspx
            [DllImport("advapi32.dll", EntryPoint = "GetTokenInformation", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetTokenInformationPrivate(
                IntPtr TokenHandle,
                TOKEN_INFORMATION_CLASS TokenInformationClass,
                IntPtr TokenInformation,
                uint TokenInformationLength,
                out uint ReturnLength);

            internal static IEnumerable<PrivilegeSetting> GetTokenPrivileges(SafeCloseHandle token)
            {
                // Get the buffer size we need
                uint bytesNeeded;
                if (GetTokenInformationPrivate(
                    token.DangerousGetHandle(),
                    TOKEN_INFORMATION_CLASS.TokenPrivileges,
                    IntPtr.Zero,
                    0,
                    out bytesNeeded))
                {
                    // Didn't need any space for output, let's assume there are no privileges
                    return Enumerable.Empty<PrivilegeSetting>();
                }

                int error = Marshal.GetLastWin32Error();
                if (error != WinError.ERROR_INSUFFICIENT_BUFFER)
                    throw GetIoExceptionForError(error);

                // Initialize the buffer and get the data
                NativeBuffer buffer = new NativeBuffer(bytesNeeded);
                if (!GetTokenInformationPrivate(
                    token.DangerousGetHandle(),
                    TOKEN_INFORMATION_CLASS.TokenPrivileges,
                    buffer,
                    buffer.Size,
                    out bytesNeeded))
                {
                    error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error);
                }


                // Loop through and get our privileges
                BinaryReader reader = new BinaryReader(buffer.GetStream(), Encoding.Unicode, leaveOpen: true);
                uint count = reader.ReadUInt32();

                var privileges = new List<PrivilegeSetting>();
                StringBuilder sb = new StringBuilder(256);

                for (int i = 0; i < count; i++)
                {
                    LUID luid = new LUID
                    {
                        LowPart = reader.ReadUInt32(),
                        HighPart = reader.ReadUInt32(),
                    };

                    uint length = (uint)sb.Capacity;

                    if (!LookupPrivilegeNamePrivate(IntPtr.Zero, ref luid, sb, ref length))
                    {
                        error = Marshal.GetLastWin32Error();
                        throw GetIoExceptionForError(error);
                    }

                    PrivilegeState state = (PrivilegeState)reader.ReadUInt32();
                    privileges.Add(new PrivilegeSetting(sb.ToString(), state));
                    sb.Clear();
                }

                return privileges;
            }

            /// <summary>
            /// Returns true if the given token has the specified privilege. The privilege may or may not be enabled.
            /// </summary>
            internal static bool HasPrivilege(SafeCloseHandle token, Privileges privilege)
            {
                return GetTokenPrivileges(token).Any(t => t.Privilege == privilege);
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379176.aspx
            [DllImport("advapi32.dll", EntryPoint = "LookupPrivilegeName", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool LookupPrivilegeNamePrivate(
                IntPtr lpSystemName,
                ref LUID lpLuid,
                StringBuilder lpName,
                ref uint cchName
            );

            // https://msdn.microsoft.com/en-us/library/aa379180.aspx
            [DllImport("advapi32.dll", EntryPoint = "LookupPrivilegeValueW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool LookupPrivilegeValuePrivate(
                string lpSystemName,
                string lpName,
                ref LUID lpLuid);

            internal static LUID LookupPrivilegeValue(string name)
            {
                LUID luid = new LUID();
                if (!LookupPrivilegeValuePrivate(null, name, ref luid))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, name);
                }

                return luid;
            }

            // https://msdn.microsoft.com/en-us/library/aa379304.aspx
            [DllImport("advapi32.dll", EntryPoint = "PrivilegeCheck", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool PrivilegeCheckPrivate(
                SafeCloseHandle ClientToken,
                ref PRIVILEGE_SET RequiredPrivileges,
                [MarshalAs(UnmanagedType.Bool)] out bool pfResult);

            /// <summary>
            /// Checks if the given privilege is enabled. This does not tell you whether or not it
            /// is possible to get a privilege- most held privileges are not enabled by default.
            /// </summary>
            internal static bool IsPrivilegeEnabled(SafeCloseHandle token, Privileges privilege)
            {
                LUID luid = LookupPrivilegeValue(privilege.ToString());

                var luidAttributes = new LUID_AND_ATTRIBUTES
                {
                    Luid = luid,
                    Attributes = SE_PRIVILEGE_ENABLED
                };

                var set = new PRIVILEGE_SET
                {
                    Control = PRIVILEGE_SET_ALL_NECESSARY,
                    PrivilegeCount = 1,
                    Privilege = new[] { luidAttributes }
                };


                bool result;
                if (!PrivilegeCheckPrivate(token, ref set, out result))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, privilege.ToString());
                }

                return result;
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379295.aspx
            [DllImport("advapi32.dll", EntryPoint = "OpenProcessToken", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool OpenProcessTokenPrivate(
                IntPtr ProcessHandle,
                TokenAccessLevels DesiredAccesss,
                out SafeCloseHandle TokenHandle);

            internal static SafeCloseHandle OpenProcessToken(TokenAccessLevels desiredAccess)
            {
                SafeCloseHandle processToken;
                if (!OpenProcessTokenPrivate(Process.GetCurrentProcess().Handle, desiredAccess, out processToken))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw GetIoExceptionForError(error, desiredAccess.ToString());
                }

                return processToken;
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379296.aspx
            [DllImport("advapi32.dll", EntryPoint = "OpenThreadToken", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool OpenThreadTokenPrivate(
                IntPtr ThreadHandle,
                TokenAccessLevels DesiredAccess,
                [MarshalAs(UnmanagedType.Bool)] bool OpenAsSelf,
                out SafeCloseHandle TokenHandle
            );

            internal static SafeCloseHandle OpenThreadToken(TokenAccessLevels desiredAccess, bool openAsSelf)
            {
                SafeCloseHandle threadToken;
                if (!OpenThreadTokenPrivate(GetCurrentThread(), desiredAccess, openAsSelf, out threadToken))
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != WinError.ERROR_NO_TOKEN)
                        throw GetIoExceptionForError(error, desiredAccess.ToString());

                    SafeCloseHandle processToken = OpenProcessToken(TokenAccessLevels.Duplicate);
                    if (!DuplicateTokenExPrivate(
                        processToken,
                        TokenAccessLevels.Impersonate | TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges,
                        IntPtr.Zero,
                        SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                        TOKEN_TYPE.TokenImpersonation,
                        ref threadToken))
                    {
                        error = Marshal.GetLastWin32Error();
                        throw GetIoExceptionForError(error, desiredAccess.ToString());
                    }
                }

                return threadToken;
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379590.aspx
            [DllImport("advapi32.dll", EntryPoint = "SetThreadToken", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool SetThreadTokenPrivate(
                IntPtr Thread,
                SafeCloseHandle Token);

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa379317.aspx
            [DllImport("advapi32.dll", EntryPoint = "RevertToSelf", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool RevertToSelf();

            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa446617.aspx
            [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool DuplicateTokenExPrivate(
                SafeCloseHandle hExistingToken,
                TokenAccessLevels dwDesiredAccess,
                IntPtr lpTokenAttributes,
                SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
                TOKEN_TYPE TokenType,
                ref SafeCloseHandle phNewToken);
        }
    }
}
