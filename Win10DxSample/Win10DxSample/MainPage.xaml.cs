using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Microsoft.Band;
using Microsoft.Band.Sensors;

namespace Win10DxSample
{
    public sealed partial class MainPage
    {
        private bool _continue;

        private const double Pi = Math.PI;
        private const double PiHalf = Pi * 0.5;

        private double _roll;
        private double _pitch;

        public MainPage()
        {
            InitializeComponent();

            Loaded += async (s, e) =>
            {
                await CheckOptions();
                DxPanel.Scale = 0.8;
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DxPanel.StartRenderLoop();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DxPanel.StopRenderLoop();
            _continue = false;
        }

        private void RootManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            DxPanel.Pitch = NormalizeAngle(
                DxPanel.Pitch + e.Delta.Translation.Y * 0.005);

            DxPanel.Yaw = NormalizeAngle(
                DxPanel.Yaw + e.Delta.Translation.X * 0.005);

            DxPanel.Scale *= e.Delta.Scale;
        }

        private async Task<IBandInfo> GetBandInfo()
        {
            // Get band
            var bands = await BandClientManager.Instance.GetBandsAsync();

            if (bands.Length == 0)
            {
                StatusText.Text = "No band found!";
                return null;
            }

            return bands[0];
        }

        private async Task InitializeBand()
        {
            StatusText.Text = "Connecting to the band";

            var info = await GetBandInfo();
            if (info == null)
            {
                return;
            }

            try
            {
                StatusText.Text = "Connecting to band " + info.Name;

                using (var bandClient = await BandClientManager.Instance.ConnectAsync(info))
                {
                    if (!bandClient.SensorManager.Accelerometer.IsSupported)
                    {
                        StatusText.Text = "Accelerometer is not supported";
                        return;
                    }

                    StatusText.Text = "Connected, starting reading";

                    bandClient.SensorManager.Accelerometer.ReadingChanged
                        += AccelerometerReadingChanged;

                    await bandClient.SensorManager.Accelerometer.StartReadingsAsync();

                    StatusText.Text = "Reading...";

                    _continue = true;

                    while (_continue)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1)); // Client needs to be kept alive during reading
                    }

                    try
                    {
                        await bandClient.SensorManager.Accelerometer.StopReadingsAsync();

                        bandClient.SensorManager.Accelerometer.ReadingChanged
                            -= AccelerometerReadingChanged;
                    }
                    catch
                    {
                        // Ignore errors here
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = ex.Message;
            }
        }

        private async void AccelerometerReadingChanged(
            object sender,
            BandSensorReadingEventArgs<IBandAccelerometerReading> e)
        {
            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    var reading = e.SensorReading;

                    var roll = Math.Atan2(reading.AccelerationX, reading.AccelerationY);
                    var pitch = Math.Atan2(reading.AccelerationZ, reading.AccelerationX);

                    DxPanel.Roll = NormalizeRoll(roll);
                    DxPanel.Pitch = NormalizePitch(pitch);

                }).AsTask();
        }

        private double NormalizeRoll(double roll)
        {
            if ((_pitch < Pi && _pitch >= PiHalf - 0.2)
                || (_pitch > Pi && _pitch <= 3 * PiHalf + 0.2))
            {
                return _roll;
            }

            _roll = roll + PiHalf;

            // Constrain value to avoid side effects
            if (_roll < -PiHalf + 0.2
                || _roll > Pi)
            {
                _roll = -PiHalf + 0.2;
            }
            else
            {
                if (_roll > PiHalf - 0.2)
                {
                    _roll = PiHalf - 0.2;
                }
            }

            return _roll;
        }

        private double NormalizePitch(double pitch)
        {
            if (_roll >= PiHalf - 0.2
                || _roll <= -PiHalf + 0.2)
            {
                return _pitch;
            }

            _pitch = pitch + Pi;

            // Constrain value to avoid side effects
            if (_pitch > Pi
                && _pitch < 3 * PiHalf + 0.2)
            {
                _pitch = 3 * PiHalf + 0.2;
            }
            else
            {
                if (_pitch > PiHalf - 0.2
                    && _pitch < Pi)
                {
                    _pitch = PiHalf - 0.2;
                }
            }

            return _pitch;
        }

        private double NormalizeAngle(double originalAngle)
        {
            // Constrain the pitch between -90 and 90 degrees
            // (90 degrees == Pi / 2 radians)

            return Math.Max(
                -PiHalf,
                Math.Min(
                    PiHalf,
                    originalAngle));
        }

        #region Options

        private async void OptionChanged(object sender, RoutedEventArgs e)
        {
            await CheckOptions();
        }

        private async Task CheckOptions()
        {
            if (Root == null)
            {
                return;
            }

            if (BandRadioButton.IsChecked != null
                && BandRadioButton.IsChecked.Value)
            {
                Root.ManipulationDelta -= RootManipulationDelta;
                Root.ManipulationMode = ManipulationModes.None;

                await InitializeBand();
            }
            else
            {
                if (TouchRadioButton.IsChecked != null
                    && TouchRadioButton.IsChecked.Value)
                {
                    _continue = false; // Stop listening to Band
                    Root.ManipulationMode = ManipulationModes.All;
                    Root.ManipulationDelta += RootManipulationDelta;
                    StatusText.Text = "Tap & hold to rotate. Pinch to zoom.";
                }
            }
        }

        #endregion
    }
}
