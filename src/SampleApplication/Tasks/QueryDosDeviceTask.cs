﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile.Tasks
{
    using System;
    using System.Linq;
    using XTask.Logging;
    using XTask.Utility;

    public class QueryDosDeviceTask : FileTask
    {
        protected override ExitCode ExecuteFileTask()
        {
            string target = this.Arguments.Target;
            target = String.IsNullOrWhiteSpace(target) ? null : target;

            var targetPaths =
                from path in ExtendedFileService.QueryDosDeviceNames(target)
                orderby path
                select path;

            int count = 0;
            foreach (string path in targetPaths)
            {
                count++;
                this.Loggers[LoggerType.Result].WriteLine(path);
            }

            this.Loggers[LoggerType.Status].WriteLine("\nFound {0} paths", count);

            return ExitCode.Success;
        }
    }
}
