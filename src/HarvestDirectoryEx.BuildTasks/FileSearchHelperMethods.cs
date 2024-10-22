﻿// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

using System.IO;

namespace HarvestDirectoryEx.BuildTasks
{
    /// <summary>
    /// Contains helper methods on searching for files
    /// </summary>
    public static class FileSearchHelperMethods
    {
        /// <summary>
        /// Searches for the existence of a file in multiple directories.
        /// Search is satisfied if default file path is valid and exists. If not,
        /// file name is extracted from default path and combined with each of the directories
        /// looking to see if it exists. If not found, input default path is returned.
        /// </summary>
        /// <param name="directories">Array of directories to look in, without filenames in them</param>
        /// <param name="defaultFullPath">Default path - to use if not found</param>
        /// <returns>File path if file found. Empty string if not found</returns>
        public static string SearchFilePaths(string[] directories, string defaultFullPath)
        {
            if (string.IsNullOrEmpty(defaultFullPath))
            {
                return string.Empty;
            }

            if (File.Exists(defaultFullPath))
            {
                return defaultFullPath;
            }

            if (directories == null)
            {
                return string.Empty;
            }

            string fileName = Path.GetFileName(defaultFullPath);
            foreach (string currentPath in directories)
            {
                if (string.IsNullOrEmpty(currentPath) || string.IsNullOrEmpty(currentPath.Trim()))
                {
                    continue;
                }

                if (File.Exists(Path.Combine(currentPath, fileName)))
                {
                    return Path.Combine(currentPath, fileName);
                }
            }

            return string.Empty;
        }
    }
}
