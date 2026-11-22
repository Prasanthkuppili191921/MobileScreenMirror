
using System;
using System.Windows;
using MobileScreenMirror.Services;

namespace MobileScreenMirror;

public partial class MainWindow : Window
{
    private readonly AdbService _adbService = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private void SetStatus(string message)
    {
        StatusTextBlock.Text = message;
    }

    private bool ValidateEndpoint(
        string ipAddress,
        string port)
    {
        if (string.IsNullOrWhiteSpace(ipAddress) ||
            string.IsNullOrWhiteSpace(port))
        {
            MessageBox.Show(
                "IP Address and Port are required.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return false;
        }

        if (!int.TryParse(port, out int portNumber) ||
            portNumber < 1 ||
            portNumber > 65535)
        {
            MessageBox.Show(
                "Please enter a valid port number.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return false;
        }

        return true;
    }

    private async void PairDevice_Click(
        object sender,
        RoutedEventArgs e)
    {
        string ip = PairIpTextBox.Text.Trim();
        string port = PairPortTextBox.Text.Trim();
        string code = PairCodeBox.Password.Trim();

        if (!ValidateEndpoint(ip, port))
            return;

        if (string.IsNullOrWhiteSpace(code))
        {
            MessageBox.Show("Enter the pairing code.");
            return;
        }

        try
        {
            SetStatus("Pairing device...");

            string result = await _adbService.PairAsync(
                ip,
                port,
                code);

            SetStatus(result);

            MessageBox.Show(
                result,
                "Pairing Result",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            SetStatus("Pairing failed.");

            MessageBox.Show(
                ex.Message,
                "Pairing Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void ConnectDevice_Click(
        object sender,
        RoutedEventArgs e)
    {
        string ip = ConnectIpTextBox.Text.Trim();
        string port = ConnectPortTextBox.Text.Trim();

        if (!ValidateEndpoint(ip, port))
            return;

        try
        {
            SetStatus("Connecting to device...");

            string result = await _adbService.ConnectAsync(
                ip,
                port);

            SetStatus(result);

            await RefreshDevicesAsync();
        }
        catch (Exception ex)
        {
            SetStatus("Connection failed.");

            MessageBox.Show(
                ex.Message,
                "Connection Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void DisconnectDevice_Click(
        object sender,
        RoutedEventArgs e)
    {
        string ip = ConnectIpTextBox.Text.Trim();
        string port = ConnectPortTextBox.Text.Trim();

        if (!ValidateEndpoint(ip, port))
            return;

        try
        {
            SetStatus("Disconnecting...");

            string result =
                await _adbService.DisconnectAsync(ip, port);

            SetStatus(result);

            await RefreshDevicesAsync();
        }
        catch (Exception ex)
        {
            SetStatus("Disconnect failed.");

            MessageBox.Show(
                ex.Message,
                "Disconnect Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void RefreshDevices_Click(
        object sender,
        RoutedEventArgs e)
    {
        await RefreshDevicesAsync();
    }

    private async Task RefreshDevicesAsync()
    {
        try
        {
            SetStatus("Detecting devices...");

            var devices = await _adbService.GetDevicesAsync();

            DevicesListView.ItemsSource = devices;

            SetStatus(
                $"Devices detected: {devices.Count}");
        }
        catch (Exception ex)
        {
            SetStatus("Device detection failed.");

            MessageBox.Show(
                ex.Message,
                "ADB Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
