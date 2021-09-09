using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

using WixToolset.Data;
using WixToolset.Harvesters;
using WixToolset.Harvesters.Data;
using WixToolset.Harvesters.Extensibility;

using Wix = WixToolset.Harvesters.Serialize;

namespace HarvestDirectoryEx.Core.Harvesters
{
    /// <summary>
    /// Harvest WiX authoring for a directory from the file system.
    /// </summary>
    internal sealed class DirectoryHarvesterEx : BaseHarvesterExtension
    {
        private readonly FileHarvester _fileHarvester;
        private readonly Matcher _matcher;
        private HashSet<string> _allowedFiles;
        private const string ComponentPrefix = "cmp";
        private const string DirectoryPrefix = "dir";
        private const string FilePrefix = "fil";

        /// <summary>
        /// Instantiate a new DirectoryHarvester.
        /// </summary>
        public DirectoryHarvesterEx()
        {
            _fileHarvester = new FileHarvester();
            _matcher = new Matcher();
            _allowedFiles = new HashSet<string>();
            SetUniqueIdentifiers = true;
        }

        /// <summary>
        /// Gets or sets what type of elements are to be generated.
        /// </summary>
        /// <value>The type of elements being generated.</value>
        public GenerateType GenerateType { get; set; }

        /// <summary>
        /// Gets or sets the option to keep empty directories.
        /// </summary>
        /// <value>The option to keep empty directories.</value>
        public bool KeepEmptyDirectories { get; set; }

        /// <summary>
        /// Gets or sets the rooted DirectoryRef Id if the user has supplied it.
        /// </summary>
        /// <value>The DirectoryRef Id to use as the root.</value>
        public string? RootedDirectoryRef { get; set; }

        /// <summary>
        /// Gets of sets the option to set unique identifiers.
        /// </summary>
        /// <value>The option to set unique identifiers.</value>
        public bool SetUniqueIdentifiers { get; set; }

        /// <summary>
        /// Gets or sets the option to suppress including the root directory as an element.
        /// </summary>
        /// <value>The option to suppress including the root directory as an element.</value>
        public bool SuppressRootDirectory { get; set; }

        /// <summary>
        /// Gets or set the glob patterns to specify what to include in harvesting.
        /// </summary>
        public string[]? Includes { get; set; }

        /// <summary>
        /// Gets or sets the glob patterns to specify what to exclude from include matches.
        /// </summary>
        public string[]? Excludes { get; set; }

        /// <summary>
        /// Harvest a directory.
        /// </summary>
        /// <param name="argument">The path of the directory.</param>
        /// <returns>The harvested directory.</returns>
        public override Wix.Fragment[] Harvest(string argument)
        {
            if (null == argument)
            {
                throw new ArgumentNullException("argument");
            }

            _matcher.AddIncludePatterns(Includes);
            _matcher.AddExcludePatterns(Excludes);

            _allowedFiles = _matcher.GetResultsInFullPath(argument).ToHashSet();

            Wix.IParentElement harvestParent = HarvestDirectory(argument, true, GenerateType);
            Wix.ISchemaElement harvestElement;

            if (GenerateType == GenerateType.PayloadGroup)
            {
                Wix.PayloadGroup payloadGroup = (Wix.PayloadGroup)harvestParent;
                payloadGroup.Id = RootedDirectoryRef;
                harvestElement = payloadGroup;
            }
            else
            {
                Wix.Directory directory = (Wix.Directory)harvestParent;

                Wix.DirectoryRef directoryRef = new Wix.DirectoryRef();
                directoryRef.Id = RootedDirectoryRef;

                if (SuppressRootDirectory)
                {
                    foreach (Wix.ISchemaElement element in directory.Children.OfType<Wix.ISchemaElement>())
                    {
                        directoryRef.AddChild(element);
                    }
                }
                else
                {
                    directoryRef.AddChild(directory);
                }
                harvestElement = directoryRef;
            }

            Wix.Fragment fragment = new Wix.Fragment();
            fragment.AddChild(harvestElement);

            return new Wix.Fragment[] { fragment };
        }

        /// <summary>
        /// Harvest a directory.
        /// </summary>
        /// <param name="path">The path of the directory.</param>
        /// <param name="harvestChildren">The option to harvest child directories and files.</param>
        /// <param name="generateType">The type to generate.</param>
        /// <returns>The harvested directory.</returns>
        private Wix.IParentElement HarvestDirectory(string path, bool harvestChildren, GenerateType generateType)
        {
            if (File.Exists(path))
            {
                throw new WixException(ErrorMessages.ExpectedDirectoryGotFile("dir", path));
            }

            if (null == RootedDirectoryRef)
            {
                RootedDirectoryRef = "TARGETDIR";
            }

            // use absolute paths
            path = Path.GetFullPath(path);

            // Remove any trailing separator to ensure Path.GetFileName() will return the directory name.
            path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            Wix.IParentElement harvestParent;
            if (generateType == GenerateType.PayloadGroup)
            {
                harvestParent = new Wix.PayloadGroup();
            }
            else
            {
                Wix.Directory directory = new Wix.Directory();
                directory.Name = Path.GetFileName(path);
                directory.FileSource = path;

                if (SetUniqueIdentifiers)
                {
                    if (SuppressRootDirectory)
                    {
                        directory.Id = Core.GenerateIdentifier(DirectoryPrefix, RootedDirectoryRef);
                    }
                    else
                    {
                        directory.Id = Core.GenerateIdentifier(DirectoryPrefix, RootedDirectoryRef, directory.Name);
                    }
                }
                harvestParent = directory;
            }

            if (harvestChildren)
            {
                try
                {
                    int fileCount = HarvestDirectory(path, "SourceDir\\", harvestParent, generateType);

                    if (generateType != GenerateType.PayloadGroup)
                    {
                        // its an error to not harvest anything with the option to keep empty directories off
                        if (0 == fileCount && !KeepEmptyDirectories)
                        {
                            throw new WixException(HarvesterErrors.EmptyDirectory(path));
                        }
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    throw new WixException(HarvesterErrors.DirectoryNotFound(path));
                }
            }

            return harvestParent;
        }

        /// <summary>
        /// Harvest a directory.
        /// </summary>
        /// <param name="path">The path of the directory.</param>
        /// <param name="relativePath">The relative path that will be used when harvesting.</param>
        /// <param name="harvestParent">The directory for this path.</param>
        /// <param name="generateType"></param>
        /// <returns>The number of files harvested.</returns>
        private int HarvestDirectory(string path, string relativePath, Wix.IParentElement harvestParent, GenerateType generateType)
        {
            int fileCount = 0;
            Wix.Directory directory = generateType != GenerateType.PayloadGroup ? (Wix.Directory)harvestParent : new();

            // harvest the child directories
            foreach (string childDirectoryPath in Directory.GetDirectories(path))
            {
                var childDirectoryName = Path.GetFileName(childDirectoryPath);
                Wix.IParentElement newParent;
                Wix.Directory? childDirectory = null;

                if (generateType == GenerateType.PayloadGroup)
                {
                    newParent = harvestParent;
                }
                else
                {
                    childDirectory = new Wix.Directory();
                    newParent = childDirectory;

                    childDirectory.Name = childDirectoryName;
                    childDirectory.FileSource = childDirectoryPath;

                    if (SetUniqueIdentifiers)
                    {
                        childDirectory.Id = Core.GenerateIdentifier(DirectoryPrefix, directory.Id, childDirectory.Name);
                    }
                }

                int childFileCount = HarvestDirectory(childDirectoryPath, string.Concat(relativePath, childDirectoryName, "\\"), newParent, generateType);

                if (generateType != GenerateType.PayloadGroup)
                {
                    // keep the directory if it contained any files (or empty directories are being kept)
                    if (0 < childFileCount || KeepEmptyDirectories)
                    {
                        directory.AddChild(childDirectory);
                    }
                }

                fileCount += childFileCount;
            }

            // harvest the files
            string[] files = Directory.GetFiles(path);
            int numFilesHarvested = 0;
            if (0 < files.Length)
            {
                foreach (string filePath in files)
                {
                    if (!_allowedFiles.Contains(filePath))
                    {
                        continue;
                    }

                    string fileName = Path.GetFileName(filePath);
                    string source = string.Concat(relativePath, fileName);

                    Wix.ISchemaElement newChild;
                    if (generateType == GenerateType.PayloadGroup)
                    {
                        Wix.Payload payload = new Wix.Payload();
                        newChild = payload;

                        payload.SourceFile = source;
                    }
                    else
                    {
                        Wix.Component component = new Wix.Component();
                        newChild = component;

                        Wix.File file = _fileHarvester.HarvestFile(filePath);
                        file.Source = source;

                        if (SetUniqueIdentifiers)
                        {
                            file.Id = Core.GenerateIdentifier(FilePrefix, directory.Id, fileName);
                            component.Id = Core.GenerateIdentifier(ComponentPrefix, directory.Id, file.Id);
                        }

                        numFilesHarvested++;
                        component.AddChild(file);
                    }

                    harvestParent.AddChild(newChild);
                }
            }
            else if (generateType != GenerateType.PayloadGroup && 0 == fileCount && KeepEmptyDirectories)
            {
                Wix.Component component = new Wix.Component();
                component.KeyPath = Wix.YesNoType.yes;

                if (SetUniqueIdentifiers)
                {
                    component.Id = Core.GenerateIdentifier(ComponentPrefix, directory.Id);
                }

                Wix.CreateFolder createFolder = new Wix.CreateFolder();
                component.AddChild(createFolder);

                directory.AddChild(component);
            }

            return fileCount + numFilesHarvested;
        }
    }

    /// <summary>
    /// Defines generated element types.
    /// </summary>
    internal enum GenerateType
    {
        /// <summary>Generate Components.</summary>
        Components,

        /// <summary>Generate a Container with Payloads.</summary>
        Container,

        /// <summary>Generate a Bundle PackageGroups.</summary>
        PackageGroup,

        /// <summary>Generate a PayloadGroup with Payloads.</summary>
        PayloadGroup,
    }
}
