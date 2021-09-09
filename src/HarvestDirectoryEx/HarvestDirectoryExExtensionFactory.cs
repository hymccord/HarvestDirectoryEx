using System;

using HarvestDirectoryEx.Core;

using WixToolset.Extensibility;
using WixToolset.Harvesters.Extensibility;

namespace HarvestDirectoryEx
{
    public class HarvestDirectoryExExtensionFactory : IExtensionFactory
    {
        public bool TryCreateExtension(Type extensionType, out object? extension)
        {
            extension = null;

            if (typeof(IHeatExtension) == extensionType)
            {
                extension = new HarvestDirectoryExHeatExtension();
            }

            return extension != null;
        }
    }
}
