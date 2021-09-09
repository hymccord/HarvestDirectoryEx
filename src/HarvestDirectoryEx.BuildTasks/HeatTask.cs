// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

using Microsoft.Build.Framework;

namespace HarvestDirectoryEx.BuildTasks
{
    /// <summary>
    /// A base MSBuild task to run the WiX harvester.
    /// Specific harvester tasks should extend this class.
    /// </summary>
    public abstract partial class HeatTask : ToolsetTask
    {
        private bool autogenerageGuids;
        private bool generateGuidsNow;
        private ITaskItem outputFile;
        private bool suppressFragments;
        private bool suppressUniqueIds;
        private string[] transforms;

        public HeatTask()
        {
            RunAsSeparateProcess = true;
        }

        public bool AutogenerateGuids
        {
            get { return autogenerageGuids; }
            set { autogenerageGuids = value; }
        }

        public bool GenerateGuidsNow
        {
            get { return generateGuidsNow; }
            set { generateGuidsNow = value; }
        }

        [Required]
        [Output]
        public ITaskItem OutputFile
        {
            get { return outputFile; }
            set { outputFile = value; }
        }

        public bool SuppressFragments
        {
            get { return suppressFragments; }
            set { suppressFragments = value; }
        }

        public bool SuppressUniqueIds
        {
            get { return suppressUniqueIds; }
            set { suppressUniqueIds = value; }
        }

        public string[] Transforms
        {
            get { return transforms; }
            set { transforms = value; }
        }

        protected sealed override string ToolName => "heat.exe";

        /// <summary>
        /// Gets the name of the heat operation performed by the task.
        /// </summary>
        /// <remarks>This is the first parameter passed on the heat.exe command-line.</remarks>
        /// <value>The name of the heat operation performed by the task.</value>
        protected abstract string OperationName
        {
            get;
        }

        /// <summary>
        /// Builds a command line from options in this task.
        /// </summary>
        protected override void BuildCommandLine(WixCommandLineBuilder commandLineBuilder)
        {
            base.BuildCommandLine(commandLineBuilder);

            commandLineBuilder.AppendIfTrue("-ag", AutogenerateGuids);
            commandLineBuilder.AppendIfTrue("-gg", GenerateGuidsNow);
            commandLineBuilder.AppendIfTrue("-sfrag", SuppressFragments);
            commandLineBuilder.AppendIfTrue("-suid", SuppressUniqueIds);
            commandLineBuilder.AppendArrayIfNotNull("-t ", Transforms);
            commandLineBuilder.AppendTextIfNotNull(AdditionalOptions);
            commandLineBuilder.AppendSwitchIfNotNull("-out ", OutputFile);
        }
    }
}
