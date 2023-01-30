﻿using Microsoft.Extensions.Hosting;
using Mindscape.Raygun4Net;

using Raygun4Maui.MattJohnsonPint.Maui;
using Raygun4Maui.RaygunLogger;

namespace Mindscape.Raygun4Maui
{
    // All the code in this file is included in all platforms.
    public class RaygunMauiClient : RaygunClient
    {
        public RaygunMauiClient(string apiKey) : base(apiKey)
        {
            this.RaygunMauiClientInit(apiKey);
        }

        public RaygunMauiClient(RaygunSettingsBase settings) : base(settings)
        {
            this.RaygunMauiClientInit(settings.ApiKey);
        }

        private void RaygunMauiClientInit(string apiKey)
        {
            AttachMauiExceptionHandler();
        }

        private void AttachMauiExceptionHandler()
        {
            MauiExceptions.UnhandledException += (sender, args) =>
            {
                Exception e = (Exception)args.ExceptionObject;
                this.Send(e, new List<string>() { "UnhandledException" }, null);
            };
        }
    }
}