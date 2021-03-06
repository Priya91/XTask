﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XFile
{
    using System.Diagnostics;
    using XFile.Tasks;
    using XTask.Tasks;

    /// <summary>
    /// Example task service
    /// </summary>
    public class XFileTaskService : TaskService
    {
        private XFileTaskService()
            : base (XFileStrings.HelpGeneral)
        {
        }

        public static ITaskService Create()
        {
            XFileTaskService taskService = new XFileTaskService();
            taskService.Initialize();
            return taskService;
        }

        public override void Initialize()
        {
            SimpleTaskRegistry registry = this.GetTaskRegistry();

            // Debugger.Launch();

            registry.RegisterTask(() => new FullPathTask(), "fullpath", "fp");
            registry.RegisterTask(() => new LongPathTask(), "longpath", "lp");
            registry.RegisterTask(() => new ShortPathTask(), "shortpath", "sp");
            registry.RegisterTask(() => new FinalPathTask(), "finalpath", "final");
            registry.RegisterTask(() => new GetVolumeInformationTask(), "getvolumeinformation", "getvolumeinfo", "gvi");
            registry.RegisterTask(() => new MakeDirectoryTask(), "makedirectory", "makedir", "md");
            registry.RegisterTask(() => new QueryDosDeviceTask(), "querydosdevice", "qdd");
            registry.RegisterTask(() => new GetLogicalDriveStringsTask(), "getlogicaldrivestrings", "glds");
            registry.RegisterTask(() => new GetVolumeNameTask(), "getvolumename", "gvn");
            registry.RegisterTask(() => new GetVolumePathNameTask(), "getvolumepathname", "gvpn");
            registry.RegisterTask(() => new GetVolumePathNamesTask(), "getvolumepathnames", "gvpns");
            registry.RegisterTask(() => new PrintCurrentDirectoryTask(), "printcurrentdirectory", "pwd", "pcd");
            registry.RegisterTask(() => new DirectoryTask(), "directory", "dir", "ls");
            registry.RegisterTask(() => new ChangeDirectoryTask(), "changedirectory", "chdir", "cd");
            registry.RegisterTask(() => new TypeTask(), "type");
            registry.RegisterTask(() => new EchoTask(), "echo");
            registry.RegisterTask(() => new CopyTask(), "copy", "cp");
            registry.RegisterTask(() => new ListStreamsTask(), "liststreams", "streams");

            base.Initialize();
        }
    }
}
