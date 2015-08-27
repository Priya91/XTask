﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.FileSystem.Concrete
{
    using Interop;
    using System;
    using System.Collections.Generic;

    public abstract class ExtendedFileService
    {
        public string GetFinalPath(string path)
        {
            return NativeMethods.FileManagement.GetFinalPathName(path);
        }

        public string GetLongPath(string path)
        {
            return NativeMethods.FileManagement.GetLongPathName(path);
        }

        public string GetShortPath(string path)
        {
            return NativeMethods.FileManagement.GetShortPathName(path);
        }

        public string GetVolumeName(string volumeMountPoint)
        {
            if (String.IsNullOrWhiteSpace(volumeMountPoint)) throw new ArgumentNullException(nameof(volumeMountPoint));

            return NativeMethods.VolumeManagement.GetVolumeNameForVolumeMountPoint(volumeMountPoint);
        }

        public string GetVolumePathName(string path)
        {
            if (String.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            return NativeMethods.VolumeManagement.GetVolumePathName(path);
        }

        public IEnumerable<string> GetVolumePathNames(string volumeName)
        {
            if (String.IsNullOrWhiteSpace(volumeName)) throw new ArgumentNullException(nameof(volumeName));

            return NativeMethods.VolumeManagement.GetVolumePathNamesForVolumeName(volumeName);
        }

        public IEnumerable<string> QueryDosDeviceNames(string dosAlias)
        {
            return NativeMethods.VolumeManagement.QueryDosDevice(dosAlias);
        }

        public IEnumerable<string> GetLogicalDriveStrings()
        {
            return NativeMethods.VolumeManagement.GetLogicalDriveStrings();
        }

        public VolumeInformation GetVolumeInformation(string rootPath)
        {
            return NativeMethods.VolumeManagement.GetVolumeInformation(rootPath);
        }

        public IEnumerable<AlternateStreamInformation> GetAlternateStreams(string path)
        {
            return NativeMethods.GetAlternateStreams(path);
        }
    }
}
