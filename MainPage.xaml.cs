using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;
using GeolocatorPlugin;
using GeolocatorPlugin.Abstractions;
using System;

namespace Metero;

public partial class MainPage : ContentPage
{
    float gpsSpeed = 133;
    float visualSpeed = 0;

    float topSpeed = 0;

    bool isKPH = true;

    public MainPage()
    {
        InitializeComponent();
        Start();
    }

    static float NextFloat(float min, float max) {
        System.Random random = new System.Random();
        double val = (random.NextDouble() * (max - min) + min);
        return (float)val;
    }


    private float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat * (1 - by) + secondFloat * by;
    }

    private void OnResetTopSpeed(object o, EventArgs e)
    {
        gpsSpeed -= NextFloat(0, 10);
        topSpeed = 0;
    }

    private async void Start()
    {
        string oauthToken = await SecureStorage.Default.GetAsync("isKPH");
        if (oauthToken != null) {
            isKPH = oauthToken == "1";
        }
        UnitsLabelButton.Dispatcher.Dispatch(() =>
        {
            UnitsLabelButton.Text = isKPH ? "kph" : "mph";
        });

        //await TurnOnGPS();

        Loop();
    }
    private async Task TurnOnGPS()
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
                gpsSpeed = (float)kilometersPerHour;
            };
        }
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
                SpeedLabel.Text = (isKPH ? visualSpeed : visualSpeed * 0.621371192).ToString("0");
            });

            if (gpsSpeed > topSpeed)
            {
                topSpeed = gpsSpeed;
            }

            TopSpeedLabel.Dispatcher.Dispatch(() =>
            {
                TopSpeedLabel.Text = (isKPH ? topSpeed : topSpeed * 0.621371192).ToString("0.0");
            });

            await Task.Delay(10);
        }
    }

    private void OnChangeUnits(object o, EventArgs e) {
        isKPH = !isKPH;
        UnitsLabelButton.Dispatcher.Dispatch(() =>
        {
            UnitsLabelButton.Text = isKPH ? "kph" : "mph";
        });
        SecureStorage.Default.SetAsync("isKPH", isKPH ? "1" : "0");
    }
}

