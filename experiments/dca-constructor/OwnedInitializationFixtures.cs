// Owned types only: no SDK identity, service implementation or configuration.
using System;

namespace Owned.Task008
{
    public sealed class ProjectFixture { }
    public interface IResourcesFixture { }
    public sealed class ConfigurationFixture { }
    public interface IHostNotificationFixture { }
    public interface IDialogServiceFixture { }
    public interface IInitializationContextFixture
    {
        ProjectFixture Project { get; }
        IResourcesFixture Resources { get; }
        ConfigurationFixture Configuration { get; }
        ConfigurationFixture UserConfiguration { get; }
        IHostNotificationFixture HostNotification { get; }
        IDialogServiceFixture DialogService { get; }
    }
    public sealed class InitializationFixture
    {
        public void Initialize(IInitializationContextFixture context) { GC.KeepAlive(context.Project); }
        public void InitializeCatching(IInitializationContextFixture context)
        {
            try { Initialize(context); }
            catch (NotSupportedException) { }
        }
        public static bool AssertDirectFailure(object context)
        {
            try { GC.KeepAlive(((IInitializationContextFixture)context).Project); }
            catch (NotSupportedException error) {
                return error.Message == "DCA_UNSUPPORTED_SERVICE:Owned.Task008.IInitializationContextFixture.get_Project";
            }
            return false;
        }
    }
}
