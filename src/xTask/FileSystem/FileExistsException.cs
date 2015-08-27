﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.FileSystem
{
    using System;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Thrown when a file or directory exists and invalidates the current operation.
    /// </summary>
    public class FileExistsException : IOException
    {
        public FileExistsException(string format, params object[] args)
            : base (String.Format(CultureInfo.CurrentCulture, format, args))
        {
        }
    }
}
