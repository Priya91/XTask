﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using FileSystem;
    using FluentAssertions;
    using System;
    using System.IO;
    using XTask.Systems.File;
    using XTask.Systems.File.Concrete.Flex;
    using XTask.Interop;
    using Xunit;

    public class FileManagementTests
    {
        [Theory
            InlineData("")]
        public void FullPathErrorCases(string value)
        {
            Action action = () => NativeMethods.FileManagement.GetFullPathName(value);
            action.ShouldThrow<IOException>();
        }

        [Theory
            Trait("Environment", "CurrentDirectory")
            InlineData(@"C:", @"C:\Users")
            InlineData(@"C", @"D:\Temp\C")
            ]
        public void ValidateKnownRelativeBehaviors(string value, string expected)
        {
            // Set the current directory to D: and the hidden env for C:'s last current directory
            NativeMethods.SetEnvironmentVariable(@"=C:", @"C:\Users");
            using (new TempCurrentDirectory(@"D:\Temp"))
            {
                NativeMethods.FileManagement.GetFullPathName(value).Should().Be(expected);
            }
        }

        private class TempCurrentDirectory : IDisposable
        {
            private string priorDirectory;

            public TempCurrentDirectory(string directory)
            {
                priorDirectory = Environment.CurrentDirectory;
                Environment.CurrentDirectory = directory;
            }

            public void Dispose()
            {
                Environment.CurrentDirectory = priorDirectory;
            }
        }

        [Theory
            // Basic dot space handling
            InlineData(@"C:\", @"C:\")
            InlineData(@"C:\ ", @"C:\")
            InlineData(@"C:\.", @"C:\")
            InlineData(@"C:\..", @"C:\")
            InlineData(@"C:\...", @"C:\")
            InlineData(@"C:\ .", @"C:\")
            InlineData(@"C:\ ..", @"C:\")
            InlineData(@"C:\ ...", @"C:\")
            InlineData(@"C:\. ", @"C:\")
            InlineData(@"C:\.. ", @"C:\")
            InlineData(@"C:\... ", @"C:\")
            InlineData(@"C:\.\", @"C:\")
            InlineData(@"C:\..\", @"C:\")
            InlineData(@"C:\...\", @"C:\...\")
            InlineData(@"C:\ \", @"C:\ \")
            InlineData(@"C:\ .\", @"C:\ \")
            InlineData(@"C:\ ..\", @"C:\ ..\")
            InlineData(@"C:\ ...\", @"C:\ ...\")
            InlineData(@"C:\. \", @"C:\. \")
            InlineData(@"C:\.. \", @"C:\.. \")
            InlineData(@"C:\... \", @"C:\... \")
            InlineData(@"C:\A \", @"C:\A \")
            InlineData(@"C:\A \B", @"C:\A \B")

            // Same as above with prefix
            InlineData(@"\\?\C:\", @"\\?\C:\")
            InlineData(@"\\?\C:\ ", @"\\?\C:\")
            InlineData(@"\\?\C:\.", @"\\?\C:")           // Changes behavior, without \\?\, returns C:\
            InlineData(@"\\?\C:\..", @"\\?\")            // Changes behavior, without \\?\, returns C:\
            InlineData(@"\\?\C:\...", @"\\?\C:\")
            InlineData(@"\\?\C:\ .", @"\\?\C:\")
            InlineData(@"\\?\C:\ ..", @"\\?\C:\")
            InlineData(@"\\?\C:\ ...", @"\\?\C:\")
            InlineData(@"\\?\C:\. ", @"\\?\C:\")
            InlineData(@"\\?\C:\.. ", @"\\?\C:\")
            InlineData(@"\\?\C:\... ", @"\\?\C:\")
            InlineData(@"\\?\C:\.\", @"\\?\C:\")
            InlineData(@"\\?\C:\..\", @"\\?\")           // Changes behavior, without \\?\, returns C:\
            InlineData(@"\\?\C:\...\", @"\\?\C:\...\")

            // How deep can we go with prefix
            InlineData(@"\\?\C:\..\..", @"\\?\")
            InlineData(@"\\?\C:\..\..\..", @"\\?\")

            // Basic dot space handling with UNCs
            InlineData(@"\\Server\Share\", @"\\Server\Share\")
            InlineData(@"\\Server\Share\ ", @"\\Server\Share\")
            InlineData(@"\\Server\Share\.", @"\\Server\Share")          // UNCs can eat trailing separator
            InlineData(@"\\Server\Share\..", @"\\Server\Share")         // UNCs can eat trailing separator
            InlineData(@"\\Server\Share\...", @"\\Server\Share\")
            InlineData(@"\\Server\Share\ .", @"\\Server\Share\")
            InlineData(@"\\Server\Share\ ..", @"\\Server\Share\")
            InlineData(@"\\Server\Share\ ...", @"\\Server\Share\")
            InlineData(@"\\Server\Share\. ", @"\\Server\Share\")
            InlineData(@"\\Server\Share\.. ", @"\\Server\Share\")
            InlineData(@"\\Server\Share\... ", @"\\Server\Share\")
            InlineData(@"\\Server\Share\.\", @"\\Server\Share\")
            InlineData(@"\\Server\Share\..\", @"\\Server\Share\")
            InlineData(@"\\Server\Share\...\", @"\\Server\Share\...\")

            // Same as above with prefix
            InlineData(@"\\?\UNC\Server\Share\", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\ ", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\.", @"\\?\UNC\Server\Share")
            InlineData(@"\\?\UNC\Server\Share\..", @"\\?\UNC\Server")               // Extended UNCs can eat into Server\Share
            InlineData(@"\\?\UNC\Server\Share\...", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\ .", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\ ..", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\ ...", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\. ", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\.. ", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\... ", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\.\", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\..\", @"\\?\UNC\Server\")             // Extended UNCs can eat into Server\Share
            InlineData(@"\\?\UNC\Server\Share\...\", @"\\?\UNC\Server\Share\...\")

            // How deep can we go with prefix
            InlineData(@"\\?\UNC\Server\Share\..\..", @"\\?\UNC")
            InlineData(@"\\?\UNC\Server\Share\..\..\..", @"\\?\")
            InlineData(@"\\?\UNC\Server\Share\..\..\..\..", @"\\?\")

            // Root slash behavior
            InlineData(@"C:/", @"C:\")
            InlineData(@"C:/..", @"C:\")
            InlineData(@"//Server/Share", @"\\Server\Share")
            InlineData(@"//Server/Share/..", @"\\Server\Share")
            InlineData(@"//?/", @"")

            // Device behavior
            InlineData(@"CON", @"\\.\CON")
            InlineData(@"CON:Alt", @"\\.\CON")
            InlineData(@"LPT9", @"\\.\LPT9")
            ]
        public void ValidateKnownFixedBehaviors(string value, string expected)
        {
            NativeMethods.FileManagement.GetFullPathName(value).Should().Be(expected, $"source was {value}");
        }

        [Theory
            // Basic dot space handling
            InlineData(@"C:\", @"C:\")
            InlineData(@"C:\ ", @"C:\")
            InlineData(@"C:\.", @"C:\")
            InlineData(@"C:\..", @"C:\")
            InlineData(@"C:\...", @"C:\")
            // InlineData(@"C:\ .", @"C:\")                              // THROWS
            // InlineData(@"C:\ ..", @"C:\")                             // THROWS
            // InlineData(@"C:\ ...", @"C:\")                            // THROWS
            InlineData(@"C:\. ", @"C:\")
            InlineData(@"C:\.. ", @"C:\")
            InlineData(@"C:\... ", @"C:\")
            InlineData(@"C:\.\", @"C:\")
            InlineData(@"C:\..\", @"C:\")
            InlineData(@"C:\...\", @"C:\")                              // DIFFERS- Native is identical
            InlineData(@"C:\ \", @"C:\")                                // DIFFERS- Native is identical
            // InlineData(@"C:\ .\", @"C:\ \")                          // THROWS
            // InlineData(@"C:\ ..\", @"C:\ ..\")                       // THROWS
            // InlineData(@"C:\ ...\", @"C:\ ...\")                     // THROWS
            InlineData(@"C:\. \", @"C:\")                               // DIFFERS- Native is identical
            InlineData(@"C:\.. \", @"C:\")                              // DIFFERS- Native is identical
            InlineData(@"C:\... \", @"C:\")                             // DIFFERS- Native is identical
            InlineData(@"C:\A \", @"C:\A\")                             // DIFFERS- Native is identical
            InlineData(@"C:\A \B", @"C:\A\B")                           // DIFFERS- Native is identical

            // Basic dot space handling with UNCs
            InlineData(@"\\Server\Share\", @"\\Server\Share\")
            InlineData(@"\\Server\Share\ ", @"\\Server\Share\")
            // InlineData(@"\\Server\Share\.", @"\\Server\Share")       // UNCs can eat trailing separator THROWS ArgumentException
            InlineData(@"\\Server\Share\..", @"\\Server\Share")         // UNCs can eat trailing separator
            InlineData(@"\\Server\Share\...", @"\\Server\Share")        // DIFFERS- Native has a trailing slash
            // InlineData(@"\\Server\Share\ .", @"\\Server\Share\")     // THROWS
            // InlineData(@"\\Server\Share\ ..", @"\\Server\Share\")    // THROWS
            // InlineData(@"\\Server\Share\ ...", @"\\Server\Share\")   // THROWS
            InlineData(@"\\Server\Share\. ", @"\\Server\Share")         // DIFFERS- Native has a trailing slash
            InlineData(@"\\Server\Share\.. ", @"\\Server\Share")        // DIFFERS- Native has a trailing slash
            InlineData(@"\\Server\Share\... ", @"\\Server\Share")       // DIFFERS- Native has a trailing slash
            InlineData(@"\\Server\Share\.\", @"\\Server\Share\")
            InlineData(@"\\Server\Share\..\", @"\\Server\Share\")
            InlineData(@"\\Server\Share\...\", @"\\Server\Share\")      // DIFFERS- Native is identical

            // InlineData(@"C:\Foo:Bar", @"C:\Foo:Bar")                 // NotSupportedException
            ]
        public void CompareDotNetBehaviors(string value, string expected)
        {
            Path.GetFullPath(value).Should().Be(expected, $"source was {value}");
        }

        [Theory
            // InlineData(@" "),  // 5 Access is denied (UnauthorizedAccess)
            // InlineData(@"...") // 5 
            InlineData(@"A ")
            InlineData(@"A.")
            ]
        public void CreateFileTests(string fileName)
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.Combine(cleaner.TempFolder, fileName);
                using (var handle = NativeMethods.FileManagement.CreateFile(filePath, FileAccess.ReadWrite, FileShare.ReadWrite, FileMode.Create, 0))
                {
                    handle.IsInvalid.Should().BeFalse();
                    NativeMethods.FileManagement.FileExists(filePath).Should().BeTrue();
                }
            }
        }

        [Theory
            InlineData(@" "),
            InlineData(@"...")
            InlineData(@"A ")
            InlineData(@"A.")
            ]
        public void CreateFileExtendedTests(string fileName)
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.AddExtendedPathPrefix(Paths.Combine(cleaner.TempFolder, fileName), addIfUnderLegacyMaxPath: true);
                using (var handle = NativeMethods.FileManagement.CreateFile(filePath, FileAccess.ReadWrite, FileShare.ReadWrite, FileMode.Create, 0))
                {
                    handle.IsInvalid.Should().BeFalse();
                    NativeMethods.FileManagement.FileExists(filePath).Should().BeTrue();
                }
            }
        }

        [Fact]
        public void FileExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                File.WriteAllText(filePath, "FileExists");
                NativeMethods.FileManagement.FileExists(filePath).Should().BeTrue();
                NativeMethods.FileManagement.PathExists(filePath).Should().BeTrue();
                NativeMethods.FileManagement.DirectoryExists(filePath).Should().BeFalse();
            }
        }

        [Fact]
        public void FileNotExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                NativeMethods.FileManagement.FileExists(filePath).Should().BeFalse();
                NativeMethods.FileManagement.PathExists(filePath).Should().BeFalse();
                NativeMethods.FileManagement.DirectoryExists(filePath).Should().BeFalse();
            }
        }

        [Fact]
        public void LongPathFileExistsTests()
        {
            using (var cleaner = new TestFileCleaner(false))
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                string filePath = Paths.Combine(longPath, Path.GetRandomFileName());
                IFileService system = new FileService();
                system.CreateDirectory(longPath);
                system.WriteAllText(filePath, "FileExists");
                NativeMethods.FileManagement.FileExists(filePath).Should().BeTrue();
                NativeMethods.FileManagement.PathExists(filePath).Should().BeTrue();
                NativeMethods.FileManagement.DirectoryExists(filePath).Should().BeFalse();
            }
        }

        [Fact]
        public void LongPathFileNotExistsTests()
        {
            using (var cleaner = new TestFileCleaner(false))
            {
                string filePath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                NativeMethods.FileManagement.FileExists(filePath).Should().BeFalse();
                NativeMethods.FileManagement.PathExists(filePath).Should().BeFalse();
                NativeMethods.FileManagement.DirectoryExists(filePath).Should().BeFalse();
            }
        }

        [Fact]
        public void DirectoryExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directoryPath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                Directory.CreateDirectory(directoryPath);

                NativeMethods.FileManagement.FileExists(directoryPath).Should().BeFalse();
                NativeMethods.FileManagement.PathExists(directoryPath).Should().BeTrue();
                NativeMethods.FileManagement.DirectoryExists(directoryPath).Should().BeTrue();
            }
        }

        [Fact]
        public void DirectoryNotExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directoryPath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());

                NativeMethods.FileManagement.FileExists(directoryPath).Should().BeFalse();
                NativeMethods.FileManagement.PathExists(directoryPath).Should().BeFalse();
                NativeMethods.FileManagement.DirectoryExists(directoryPath).Should().BeFalse();
            }
        }

        [Fact]
        public void AttributesForNonExistantLongPath()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                FileAttributes attributes;
                NativeMethods.FileManagement.TryGetFileAttributes(longPath, out attributes).Should().BeFalse();
                attributes.Should().Be((FileAttributes)0);
            }
        }

        [Fact]
        public void FinalPathNameLongPathPrefixRoundTripBehavior()
        {
            using (var cleaner = new TestFileCleaner(false))
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                string filePath = Paths.Combine(longPath, Path.GetRandomFileName());
                IFileService system = new FileService();
                system.CreateDirectory(longPath);
                system.WriteAllText(filePath, "FinalPathNameLongPathPrefixRoundTripBehavior");

                // The wrapper for create file will add \\?\, we shouldn't get it back
                using (var handle = NativeMethods.FileManagement.CreateFile(filePath, FileAccess.Read, FileShare.ReadWrite, FileMode.Open, 0))
                {
                    handle.IsInvalid.Should().BeFalse();
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_NORMALIZED)
                        .Should().Be(filePath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_OPENED)
                        .Should().Be(filePath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_DOS)
                        .Should().Be(filePath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_GUID)
                        .Should().StartWith(@"Volume");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NT)
                        .Should().StartWith(@"\Device\");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NONE)
                        .Should().Be(filePath.Substring(2));
                }
            }
        }

        [Fact]
        public void FinalPathNameBehavior()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                IFileService system = new FileService();
                system.WriteAllText(filePath, "FinalPathNameBehavior");

                using (var handle = NativeMethods.FileManagement.CreateFile(filePath.ToLower(), FileAccess.Read, FileShare.ReadWrite, FileMode.Open, 0))
                {
                    handle.IsInvalid.Should().BeFalse();
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_NORMALIZED)
                        .Should().Be(filePath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_OPENED)
                        .Should().Be(filePath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_DOS)
                        .Should().Be(filePath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_GUID)
                        .Should().StartWith(@"Volume");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NT)
                        .Should().StartWith(@"\Device\");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NONE)
                        .Should().Be(filePath.Substring(2));
                }
            }
        }


        [Fact]
        public void FinalPathNameLinkBehavior()
        {
            IExtendedFileService fileService = new FileService();
            if (!fileService.CanCreateSymbolicLinks()) return;

            // GetFinalPathName always points to the linked file
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                fileService.WriteAllText(filePath, "CreateSymbolicLinkToFile");

                string symbolicLink = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                NativeMethods.FileManagement.CreateSymbolicLink(symbolicLink, filePath);
                NativeMethods.FileManagement.FileExists(symbolicLink).Should().BeTrue("symbolic link should exist");

                using (var handle = NativeMethods.FileManagement.CreateFile(symbolicLink, FileAccess.Read, FileShare.ReadWrite, FileMode.Open, 0))
                {
                    handle.IsInvalid.Should().BeFalse();
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_NORMALIZED)
                        .Should().Be(filePath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_OPENED)
                        .Should().Be(filePath);
                }
            }
        }

        [Fact]
        public void CreateSymbolicLinkToFile()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                IExtendedFileService fileService = new FileService();
                fileService.WriteAllText(filePath, "CreateSymbolicLinkToFile");

                string symbolicLink = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                Action action = () => NativeMethods.FileManagement.CreateSymbolicLink(symbolicLink, filePath);

                if (fileService.CanCreateSymbolicLinks())
                {
                    action();
                    var attributes = NativeMethods.FileManagement.GetFileAttributes(symbolicLink);
                    attributes.Should().HaveFlag(FileAttributes.ReparsePoint);
                }
                else
                {
                    // Can't create links unless you have admin rights SE_CREATE_SYMBOLIC_LINK_NAME SeCreateSymbolicLinkPrivilege
                    action.ShouldThrow<IOException>().And.HResult.Should().Be(NativeErrorHelper.GetHResultForWindowsError(NativeMethods.WinError.ERROR_PRIVILEGE_NOT_HELD));
                }
            }
        }

        [Fact]
        public void CreateSymbolicLinkToLongPathFile()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                IExtendedFileService fileService = new FileService();
                fileService.WriteAllText(filePath, "CreateSymbolicLinkToFile");

                string symbolicLink = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                Action action = () => NativeMethods.FileManagement.CreateSymbolicLink(symbolicLink, filePath);

                if (fileService.CanCreateSymbolicLinks())
                {
                    action();
                    var attributes = NativeMethods.FileManagement.GetFileAttributes(symbolicLink);
                    attributes.Should().HaveFlag(FileAttributes.ReparsePoint);
                }
                else
                {
                    action.ShouldThrow<IOException>().And.HResult.Should().Be(NativeErrorHelper.GetHResultForWindowsError(NativeMethods.WinError.ERROR_PRIVILEGE_NOT_HELD));
                }
            }
        }
    }
}
