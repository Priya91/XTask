﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using XTask.FileSystem;
    using XTask.Utility;

    public class FileContentComparer : IEqualityComparer<IFileInformation>
    {
        public bool Equals(IFileInformation x, IFileInformation y)
        {
            if (x == null || y == null) { return x == y; }

            if (x.Length == y.Length && Arrays.AreEquivalent(x.MD5Hash, y.MD5Hash))
            {
                return true;
            }

            return false;
        }

        public int GetHashCode(IFileInformation obj)
        {
            return obj.Path.GetHashCode();
        }
    }
}
