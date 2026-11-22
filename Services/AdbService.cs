
using System.Diagnostics;
using System.IO;
using MobileScreenMirror.Models;

namespace MobileScreenMirror.Services;

public class AdbService
{
    private readonly string _adbPath;

    public AdbService()
    {
        string localAdbPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "platform-tools",
            "adb.exe");

        _adbPath = File.Exists(localAdbPath)
            ? localAdbPath
            : "adb";
    }

    private async Task<string> ExecuteAdbAsync(
        params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _adbPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        Task<string> outputTask =
            process.StandardOutput.ReadToEndAsync();

        Task<string> errorTask =
            process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        string output = await outputTask;
        string error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(error)
                    ? output
                    : error);
        }

        return output.Trim();
    }

    public async Task<string> PairAsync(
        string ipAddress,
        string pairingPort,
        string pairingCode)
    {
        string endpoint = $"{ipAddress}:{pairingPort}";

        return await ExecuteAdbAsync(
            "pair",
            endpoint,
            pairingCode);
    }

    public async Task<string> ConnectAsync(
        string ipAddress,
        string port)
    {
        string endpoint = $"{ipAddress}:{port}";

        return await ExecuteAdbAsync(
            "connect",
            endpoint);
    }

    public async Task<string> DisconnectAsync(
        string ipAddress,
        string port)
    {
        string endpoint = $"{ipAddress}:{port}";

        return await ExecuteAdbAsync(
            "disconnect",
            endpoint);
    }

    public async Task<List<AndroidDevice>>
        GetDevicesAsync()
    {
        string output = await ExecuteAdbAsync("devices");

        var devices = new List<AndroidDevice>();

        string[] lines = output.Split(
            new[] { '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines.Skip(1))
        {
            string[] parts = line.Split(
                '\t',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                continue;

            var device = new AndroidDevice
            {
                DeviceId = parts[0].Trim(),
                Status = parts[1].Trim()
            };

            if (device.IsConnected)
            {
                try
                {
                    device.Model = await ExecuteAdbAsync(
                        "-s",
                        device.DeviceId,
                        "shell",
                        "getprop",
                        "ro.product.model");
                }
                catch
                {
                    device.Model = "Android Device";
                }
            }
            else
            {
                device.Model = "Unknown Device";
            }

            devices.Add(device);
        }

        return devices;
    }
}
