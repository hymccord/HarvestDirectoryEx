// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

using Microsoft.Build.Framework;

namespace HarvestDirectoryEx.BuildTasks
{
    /// <summary>
    /// Reimplement HeatDirectory from Wix4.
    /// </summary>
    /// <remarks>
    /// All this class does is unseal
    /// </remarks>
    public abstract class HeatDirectory : HeatTask
    {
        public string ComponentGroupName { get; set; }

        [Required]
        public string Directory { get; set; }

        public string DirectoryRefId { get; set; }

        public bool KeepEmptyDirectories { get; set; }

        public string PreprocessorVariable { get; set; }

        public bool SuppressCom { get; set; }

        public bool SuppressRootDirectory { get; set; }

        public bool SuppressRegistry { get; set; }

        public string Template { get; set; }

        protected override void BuildCommandLine(WixCommandLineBuilder commandLineBuilder)
        {
            // These are commented out on purpose to show what I changed.
            // We're doing to append our own operation name
            //commandLineBuilder.AppendSwitch(this.OperationName);
            //commandLineBuilder.AppendFileNameIfNotNull(this.Directory);

            commandLineBuilder.AppendSwitchIfNotNull("-cg ", ComponentGroupName);
            commandLineBuilder.AppendSwitchIfNotNull("-dr ", DirectoryRefId);
            commandLineBuilder.AppendIfTrue("-ke", KeepEmptyDirectories);
            commandLineBuilder.AppendIfTrue("-scom", SuppressCom);
            commandLineBuilder.AppendIfTrue("-sreg", SuppressRegistry);
            commandLineBuilder.AppendIfTrue("-srd", SuppressRootDirectory);
            commandLineBuilder.AppendSwitchIfNotNull("-template ", Template);
            commandLineBuilder.AppendSwitchIfNotNull("-var ", PreprocessorVariable);

            base.BuildCommandLine(commandLineBuilder);
        }
    }
}
