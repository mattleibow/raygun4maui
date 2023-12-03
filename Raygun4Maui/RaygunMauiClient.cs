using System.Reflection;
using Mindscape.Raygun4Net;
using System.Globalization;
using System.Collections;
using Raygun4Maui.DeviceIdProvider;
using Raygun4Maui.MauiRUM;
using Raygun4Maui.MauiRUM.EventTypes;

namespace Raygun4Maui
{
    public class RaygunMauiClient : RaygunClient
    {
        private RaygunRum _rum;

        public override RaygunIdentifierMessage UserInfo
        {
            get => _userInfo;
            set
            {
                _userInfo = value;
                _rum?.UpdateUser(value);
            }
        }

        private RaygunIdentifierMessage _userInfo;

        private IDeviceIdProvider _deviceId;

        private static RaygunMauiClient _instance;
        public static RaygunMauiClient Current => _instance;

        private static readonly string Name = Assembly.GetExecutingAssembly().GetName().Name;
        private static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private static readonly string
            ClientUrl =
                "https://github.com/MindscapeHQ/raygun4maui"; //It does not seem like this can be obtained automatically

        public static readonly RaygunClientMessage ClientMessage = new()
        {
            Name = Name,
            Version = Version,
            ClientUrl = ClientUrl
        };

        internal static void Attach(RaygunMauiClient client)
        {
            if (_instance != null)
            {
                throw new Exception("You should only call 'AddRaygun4maui' once in your app.");
            }

            _instance = client;
        }

        public RaygunMauiClient(string apiKey) : base(apiKey)
        {
            // TODO: Create rum?
            // _rum = new RaygunRUM();
        }

        public RaygunMauiClient(Raygun4MauiSettings settings) : base(settings)
        {
            _rum = new RaygunRum();
        }

        public void EnableRealUserMonitoring(IDeviceIdProvider deviceId)
        {
            // TODO: Find a better way to inject deviceId
            _deviceId = deviceId;
            
            _userInfo = new RaygunIdentifierMessage(_deviceId.GetDeviceId()) {IsAnonymous = true};
            
            _rum.Enable(_settings as Raygun4MauiSettings, _userInfo);
        }
        
        public void SendTimingEvent(RaygunRumEventTimingType type, string name, long milliseconds)
        {
            if (_rum.Enabled)
            {
                _rum.SendCustomTimingEvent(type, name, milliseconds);
            }
        }

        protected override async Task<RaygunMessage> BuildMessage(Exception exception, IList<string> tags,
            IDictionary userCustomData, RaygunIdentifierMessage userInfo)
        {
            DateTime now = DateTime.Now;
            var environment = new RaygunMauiEnvironmentMessage // Most likely should be static
            {
                UtcOffset = TimeZoneInfo.Local.GetUtcOffset(now).TotalHours,
                Locale = CultureInfo.CurrentCulture.DisplayName,
                OSVersion = DeviceInfo.Current.VersionString,
                Architecture = NativeDeviceInfo.Architecture(),
                WindowBoundsWidth = DeviceDisplay.MainDisplayInfo.Width,
                WindowBoundsHeight = DeviceDisplay.MainDisplayInfo.Height,
                DeviceManufacturer = DeviceInfo.Current.Manufacturer,
                Platform = NativeDeviceInfo.Platform(),
                Model = DeviceInfo.Current.Model,
                ProcessorCount = Environment.ProcessorCount,
                ResolutionScale = DeviceDisplay.MainDisplayInfo.Density,
                TotalPhysicalMemory = NativeDeviceInfo.TotalPhysicalMemory(),
                AvailablePhysicalMemory = NativeDeviceInfo.AvailablePhysicalMemory(),
                CurrentOrientation = DeviceDisplay.MainDisplayInfo.Orientation.ToString(),
            };

            var details = new RaygunMessageDetails
            {
                MachineName = DeviceInfo.Current.Name,
                Client = ClientMessage,
                Error = RaygunErrorMessageBuilder.Build(exception),
                UserCustomData = userCustomData,
                Tags = tags,
                Version = ApplicationVersion,
                User = userInfo ?? UserInfo ?? (!String.IsNullOrEmpty(User) ? new RaygunIdentifierMessage(User) : null),
                Environment = environment
            };

            var message = new RaygunMessage
            {
                OccurredOn = DateTime.UtcNow,
                Details = details
            };

            var customGroupingKey = await OnCustomGroupingKey(exception, message).ConfigureAwait(false);

            if (string.IsNullOrEmpty(customGroupingKey) == false)
            {
                message.Details.GroupingKey = customGroupingKey;
            }

            return message;
        }
    }
}