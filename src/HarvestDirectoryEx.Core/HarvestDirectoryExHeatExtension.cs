using System;
using System.Collections.Generic;
using System.IO;

using HarvestDirectoryEx.Core.Harvesters;

using WixToolset.Data;
using WixToolset.Harvesters;
using WixToolset.Harvesters.Data;
using WixToolset.Harvesters.Extensibility;

namespace HarvestDirectoryEx.Core
{
    public class HarvestDirectoryExHeatExtension : BaseHeatExtension
    {
        public override HeatCommandLineOption[] CommandLineTypes
        {
            get
            {
                return new HeatCommandLineOption[]
                {
                    new HeatCommandLineOption("direx", "harvest a directory with extended options"),
                    new HeatCommandLineOption("-include", "only keep certain files for harvesting"),
                    new HeatCommandLineOption("-exclude", "reject files from being harvested"),
                };
            }
        }

        public override void ParseOptions(string type, string[] args)
        {
            DirectoryHarvesterEx harvesterExtension = new();
            List<string> includes = new();
            List<string> excludes = new();
            bool suppressHarvestingRegistryValues = false;
            UtilFinalizeHarvesterMutator utilFinalizeHarvesterMutator = new();
            GenerateType generateType = GenerateType.Components;


            // parse the options
            for (int i = 0; i < args.Length; i++)
            {
                string commandSwitch = args[i];

                if (null == commandSwitch || 0 == commandSwitch.Length) // skip blank arguments
                {
                    continue;
                }

                if ('-' == commandSwitch[0] || '/' == commandSwitch[0])
                {
                    string truncatedCommandSwitch = commandSwitch.Substring(1);

                    if ("dr" == truncatedCommandSwitch)
                    {
                        string? dr = GetArgumentParameter(args, i);

                        if (Core.Messaging.EncounteredError)
                        {
                            return;
                        }

                        harvesterExtension.RootedDirectoryRef = dr;
                    }
                    else if ("ke" == truncatedCommandSwitch)
                    {
                        harvesterExtension.KeepEmptyDirectories = true;
                    }
                    else if ("scom" == truncatedCommandSwitch)
                    {
                        utilFinalizeHarvesterMutator.SuppressCOMElements = true;
                    }
                    else if ("svb6" == truncatedCommandSwitch)
                    {
                        utilFinalizeHarvesterMutator.SuppressVB6COMElements = true;
                    }
                    else if ("srd" == truncatedCommandSwitch)
                    {
                        harvesterExtension.SuppressRootDirectory = true;
                    }
                    else if ("sreg" == truncatedCommandSwitch)
                    {
                        suppressHarvestingRegistryValues = true;
                    }
                    else if ("suid" == truncatedCommandSwitch)
                    {
                        harvesterExtension.SetUniqueIdentifiers = false;
                    }
                    else if ("var" == truncatedCommandSwitch)
                    {
                        utilFinalizeHarvesterMutator.PreprocessorVariable = GetArgumentParameter(args, i);

                        if (Core.Messaging.EncounteredError)
                        {
                            return;
                        }
                    }
                    else if ("generate" == truncatedCommandSwitch)
                    {
                        if (harvesterExtension is DirectoryHarvesterEx)
                        {
                            string? genType = GetArgumentParameter(args, i)?.ToUpperInvariant();
                            switch (genType)
                            {
                                case "COMPONENTS":
                                    generateType = GenerateType.Components;
                                    break;
                                case "PAYLOADGROUP":
                                    generateType = GenerateType.PayloadGroup;
                                    break;
                                default:
                                    throw new WixException(HarvesterErrors.InvalidDirectoryOutputType(genType));
                            }
                        }
                        else
                        {
                            // TODO: error message - not applicable
                        }
                    }
                    // These are the new options
                    else if ("include" == truncatedCommandSwitch)
                    {
                        var include = GetArgumentParameter(args, i);
                        if (include is not null)
                        {
                            includes.Add(include);
                        }
                    }
                    else if ("exclude" == truncatedCommandSwitch)
                    {
                        var exclude = GetArgumentParameter(args, i);
                        if (exclude is not null)
                        {
                            excludes.Add(exclude);
                        }
                    }
                }
            }

            harvesterExtension.Includes = includes.ToArray();
            harvesterExtension.Excludes = excludes.ToArray();

            // set the appropriate harvester extension
            Core.Harvester.Extension = harvesterExtension;

            if (!suppressHarvestingRegistryValues)
            {
                Core.Mutator.AddExtension(new UtilHarvesterMutator());
            }

            Core.Mutator.AddExtension(utilFinalizeHarvesterMutator);

            harvesterExtension.GenerateType = generateType;
            Core.Harvester.Core.RootDirectory = Core.Harvester.Core.ExtensionArgument;

            /*
               We dont actually need these. The options are parsed and handled by the
               default UtilHeatExtension that is bundled in Heat.dll
               // set the mutator
               Core.Mutator.AddExtension(utilMutator);

               //add the transforms
               foreach (UtilTransformMutator transformMutator in transformMutators)
               {
                   Core.Mutator.AddExtension(transformMutator);
               }
            */
        }

        private string? GetArgumentParameter(string[] args, int index)
        {
            return GetArgumentParameter(args, index, false);
        }

        private string? GetArgumentParameter(string[] args, int index, bool allowSpaces)
        {
            string truncatedCommandSwitch = args[index];
            string commandSwitchValue = args[index + 1];

            //increment the index to the switch value
            index++;

            if (IsValidArg(args, index) && !string.IsNullOrEmpty(commandSwitchValue.Trim()))
            {
                if (!allowSpaces && commandSwitchValue.Contains(" "))
                {
                    Core.Messaging.Write(HarvesterErrors.SpacesNotAllowedInArgumentValue(truncatedCommandSwitch, commandSwitchValue));
                }
                else
                {
                    return commandSwitchValue;
                }
            }
            else
            {
                Core.Messaging.Write(HarvesterErrors.ArgumentRequiresValue(truncatedCommandSwitch));
            }

            return null;
        }
    }
}
