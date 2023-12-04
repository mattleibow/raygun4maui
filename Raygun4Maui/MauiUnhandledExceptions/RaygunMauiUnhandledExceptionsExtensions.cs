using Raygun4Maui.MauiUnhandledExceptions.MattJohnsonPint.Maui;
using Raygun4Maui.Raygun4Net.BuildPlatforms;

namespace Raygun4Maui.MauiUnhandledExceptions
{
    internal static class RaygunMauiUnhandledExceptionsExtensions
    {
        internal static MauiAppBuilder AddRaygunUnhandledExceptionsListener(
            this MauiAppBuilder mauiAppBuilder,
            Raygun4MauiSettings raygunMauiSettings
            )
        {
            AttachMauiExceptionHandler(raygunMauiSettings);

            return mauiAppBuilder;
        }

        private static void AttachMauiExceptionHandler(Raygun4MauiSettings raygunMauiSettings)
        {
            MauiExceptions.UnhandledException += (sender, args) =>
            {
                Exception e = (Exception)args.ExceptionObject;
                List<string> tags = new List<string>() { "UnhandledException" };

                if (raygunMauiSettings.RaygunSettings.SendDefaultTags)
                {
                    tags.Add(Raygun4NetBuildPlatforms.GetBuildPlatform());
                }
                RaygunMauiClient.Current.Send(e, tags, null);
            };
        }
    }
}
