namespace HarvestDirectoryEx.BuildTasks
{
    /// <summary>
    /// MSBuild task for extended folder harvesting
    /// </summary>
    /// <remarks>
    /// Ideally we could have extended WixToolset.BuildTasks.HeatDirectory,
    /// but it is sealed.
    /// </remarks>
    public sealed class HeatDirectoryEx : HeatDirectory
    {
        private string[] _include;
        private string[] _exclude;

        public string[] Include
        {
            get => _include;
            set
            {
                _include = value;
                // If it's just one string and it contains semicolons, let's
                // split it into separate items.
                if (_include.Length == 1)
                {
                    _include = _include[0].Split(new[] { ';' });
                }
            }
        }

        public string[] Exclude
        {
            get => _exclude;
            set
            {
                _exclude = value;
                // If it's just one string and it contains semicolons, let's
                // split it into separate items.
                if (_exclude.Length == 1)
                {
                    _exclude = _exclude[0].Split(new[] { ';' });
                }
            }
        }

        /// <summary>
        /// Full path to the assembly where the IExtensionFactory is implemented
        /// </summary>
        public string ExtensionFactoryAssembly { get; set; }

        protected sealed override string OperationName
        {
            get { return "direx"; }
        }

        protected override void BuildCommandLine(WixCommandLineBuilder commandLineBuilder)
        {
            // Append the new operation name
            commandLineBuilder.AppendSwitch(OperationName);
            // Dont forget the directory too we commented out. It HAS to come second.
            commandLineBuilder.AppendFileNameIfNotNull(Directory);

            commandLineBuilder.AppendArrayIfNotNull("-include ", Include);
            commandLineBuilder.AppendArrayIfNotNull("-exclude ", Exclude);

            // The magic line that tells Heat to pick up our extension so we don't have to do any
            // special proj exec task or .targets manipulating.
            commandLineBuilder.AppendSwitchIfNotNull("-ext ", ExtensionFactoryAssembly);

            base.BuildCommandLine(commandLineBuilder);
        }

    }
}
