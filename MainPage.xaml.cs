using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;
using GeolocatorPlugin;
using GeolocatorPlugin.Abstractions;
using System;

namespace Metero;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        TurnOnGPS(SpeedLabel);
    }

    public async void TurnOnGPS(Label label)
    {
        if (await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(0.1), 1, true, new ListenerSettings
        {
            ActivityType = ActivityType.AutomotiveNavigation,
            AllowBackgroundUpdates = true,
            DeferLocationUpdates = false,
            ListenForSignificantChanges = false,
            PauseLocationUpdatesAutomatically = false,
        }))
        {
            CrossGeolocator.Current.PositionChanged += (s, e) => {
                label.Dispatcher.Dispatch(() =>
                {
                    double metersPerSecond = e.Position.Speed;
                    double kilometersPerHour = metersPerSecond * 3.6;
                    label.Text = kilometersPerHour.ToString("0.0");
                });
            };
        }
    }
}

