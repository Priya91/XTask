﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.FileSystem
{
    using System;
    using System.IO;

    /// <summary>
    /// Base interface for file/directory information
    /// </summary>
    public interface IFileSystemInformation
    {
        /// <summary>
        /// The name of the file/directory
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The full path to the file/directory
        /// </summary>
        /// <remarks>
        /// FullName in System.IO.
        /// </remarks>
        string Path { get; }

        /// <summary>
        /// True if the file/directory exists
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Creation time for the file/directory
        /// </summary>
        DateTime CreationTime { get; }

        /// <summary>
        /// Last access time for the file/directory
        /// </summary>
        DateTime LastAccessTime { get; }

        /// <summary>
        /// Last write time for the file/directory
        /// </summary>
        DateTime LastWriteTime { get; }

        /// <summary>
        /// Attributes for the file/directory
        /// </summary>
        FileAttributes Attributes { get; }

        /// <summary>
        /// Get the latest file information.
        /// </summary>
        void Refresh();
    }
}
