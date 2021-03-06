﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Utility
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Used to centrally handle invalid user arguments
    /// </summary>
    public class TaskArgumentException : TaskException
    {
        public TaskArgumentException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }

        public TaskArgumentException(string format, params object[] args)
            : base(String.Format(CultureInfo.CurrentUICulture, format, args))
        {
        }

        public static TaskArgumentException MissingArgument(string argument)
        {
            return new TaskArgumentException(XTaskStrings.ErrorArgumentMustHaveValue, argument);
        }

        public override ExitCode ExitCode
        {
            get { return ExitCode.InvalidArgument; }
        }
    }
}