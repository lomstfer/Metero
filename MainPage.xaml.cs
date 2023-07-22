using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;
using GeolocatorPlugin;
using GeolocatorPlugin.Abstractions;
using System;

namespace Metero;

public partial class MainPage : ContentPage
{
    float gpsSpeed = 0;
    float visualSpeed = 0;

    float topSpeed = 0;

    public MainPage()
    {
        InitializeComponent();
        TurnOnGPS();
        Loop();
    }

    float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat * (1 - by) + secondFloat * by;
    }

    void OnResetTopSpeed(object o, EventArgs e)
    {
        topSpeed = 0;
    }

    private async void Loop()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (true)
        {
            float deltaTime = stopwatch.ElapsedMilliseconds / 1000f;
            stopwatch.Restart();

            visualSpeed = Lerp(visualSpeed, gpsSpeed, 1f - (float)Math.Pow(0.05, deltaTime));

            SpeedLabel.Dispatcher.Dispatch(() =>
            {
                SpeedLabel.Text = visualSpeed.ToString("0");
            });

            if (gpsSpeed > topSpeed)
            {
                topSpeed = gpsSpeed;
            }

            TopSpeedLabel.Dispatcher.Dispatch(() =>
            {
                TopSpeedLabel.Text = topSpeed.ToString("0.0");
            });

            await Task.Delay(10);
        }
    }

    private async void TurnOnGPS()
    {
        PermissionStatus status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Denied)
        {
            return;
        }

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
                double metersPerSecond = e.Position.Speed;
                double kilometersPerHour = metersPerSecond * 3.6;
                //label.Text = kilometersPerHour.ToString("0.0");
                gpsSpeed = (float)kilometersPerHour;
            };
        }
    }
}

