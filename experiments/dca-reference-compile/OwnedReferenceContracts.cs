// Compile-only owned signature fixture, never loaded or executed. No SDK services.
using Knx.Ets.Sdk.AddIns.AddInViews;
using SdkProject = Knx.Ets.Sdk.Project.Project;
using SdkDevice = Knx.Ets.Sdk.Project.Device;

namespace Knx.Editor.Task005
{
    public interface ICompileOnlyReferenceTypes
    {
        IInitializationContext InitializationContext { get; }
        IDeviceConfigurationAddIn DeviceConfigurationAddIn { get; }
        IEts4AddIn AddIn { get; }
        IEts4AddInV2 AddInV2 { get; }
        SdkProject Project { get; }
        SdkDevice Device { get; }
    }
}
