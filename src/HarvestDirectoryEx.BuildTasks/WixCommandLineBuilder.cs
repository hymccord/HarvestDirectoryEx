﻿// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace HarvestDirectoryEx.BuildTasks
{
    /// <summary>
    /// Helper class for appending the command line arguments.
    /// </summary>
    public class WixCommandLineBuilder : CommandLineBuilder
    {
        internal const int Unspecified = -1;

        /// <summary>
        /// Append a switch to the command line if the value has been specified.
        /// </summary>
        /// <param name="switchName">Switch to append.</param>
        /// <param name="value">Value specified by the user.</param>
        public void AppendIfSpecified(string switchName, int value)
        {
            if (value != Unspecified)
            {
                AppendSwitchIfNotNull(switchName, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Append a switch to the command line if the condition is true.
        /// </summary>
        /// <param name="switchName">Switch to append.</param>
        /// <param name="condition">Condition specified by the user.</param>
        public void AppendIfTrue(string switchName, bool condition)
        {
            if (condition)
            {
                AppendSwitch(switchName);
            }
        }

        /// <summary>
        /// Append a switch to the command line if any values in the array have been specified.
        /// </summary>
        /// <param name="switchName">Switch to append.</param>
        /// <param name="values">Values specified by the user.</param>
        public void AppendArrayIfNotNull(string switchName, IEnumerable<ITaskItem> values)
        {
            if (values != null)
            {
                foreach (ITaskItem value in values)
                {
                    AppendSwitchIfNotNull(switchName, value);
                }
            }
        }

        /// <summary>
        /// Append a switch to the command line if any values in the array have been specified.
        /// </summary>
        /// <param name="switchName">Switch to append.</param>
        /// <param name="values">Values specified by the user.</param>
        public void AppendArrayIfNotNull(string switchName, IEnumerable<string> values)
        {
            if (values != null)
            {
                foreach (string value in values)
                {
                    AppendSwitchIfNotNull(switchName, value);
                }
            }
        }

        /// <summary>
        /// Build the extensions argument. Each extension is searched in the current folder, user defined search
        /// directories (ReferencePath), HintPath, and under Wix Extension Directory in that order.
        /// The order of precedence is based off of that described in Microsoft.Common.Targets's SearchPaths
        /// property for the ResolveAssemblyReferences task.
        /// </summary>
        /// <param name="extensions">The list of extensions to include.</param>
        /// <param name="wixExtensionDirectory">Evaluated default folder for Wix Extensions</param>
        /// <param name="referencePaths">User defined reference directories to search in</param>
        public void AppendExtensions(ITaskItem[] extensions, string wixExtensionDirectory, string[] referencePaths)
        {
            if (extensions == null)
            {
                return;
            }

            foreach (ITaskItem extension in extensions)
            {
                string className = extension.GetMetadata("Class");

                string fileName = Path.GetFileName(extension.ItemSpec);

                if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                {
                    fileName += ".dll";
                }

                // First try reference paths
                var resolvedPath = FileSearchHelperMethods.SearchFilePaths(referencePaths, fileName);

                if (string.IsNullOrEmpty(resolvedPath))
                {
                    // Now try HintPath
                    resolvedPath = extension.GetMetadata("HintPath");

                    if (!File.Exists(resolvedPath))
                    {
                        // Now try the item itself
                        resolvedPath = extension.ItemSpec;

                        if (string.IsNullOrEmpty(Path.GetExtension(resolvedPath)))
                        {
                            resolvedPath += ".dll";
                        }

                        if (!File.Exists(resolvedPath))
                        {
                            if (!string.IsNullOrEmpty(wixExtensionDirectory))
                            {
                                // Now try the extension directory
                                resolvedPath = Path.Combine(wixExtensionDirectory, Path.GetFileName(resolvedPath));
                            }

                            if (!File.Exists(resolvedPath))
                            {
                                // Extension wasn't found, just set it to the extension name passed in
                                resolvedPath = extension.ItemSpec;
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(className))
                {
                    AppendSwitchIfNotNull("-ext ", resolvedPath);
                }
                else
                {
                    AppendSwitchIfNotNull("-ext ", className + ", " + resolvedPath);
                }
            }
        }

        /// <summary>
        /// Append arbitrary text to the command-line if specified.
        /// </summary>
        /// <param name="textToAppend">Text to append.</param>
        public void AppendTextIfNotNull(string textToAppend)
        {
            if (!string.IsNullOrEmpty(textToAppend))
            {
                AppendSpaceIfNotEmpty();
                AppendTextUnquoted(textToAppend);
            }
        }

        /// <summary>
        /// Append arbitrary text to the command-line if specified.
        /// </summary>
        /// <param name="textToAppend">Text to append.</param>
        public void AppendTextIfNotWhitespace(string textToAppend)
        {
            if (!string.IsNullOrWhiteSpace(textToAppend))
            {
                AppendSpaceIfNotEmpty();
                AppendTextUnquoted(textToAppend);
            }
        }
    }
}
