using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TrueSleep.Interop;
using TrueSleep.Services;

namespace TrueSleep;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        UpdateStatus();
    }

    private void OnSourceInitialized(object? sender, EventArgs e) => WindowBackdrop.Apply(this);

    private async void OnDeepSleepClick(object sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        await RunPowerActionAsync(
            "در حال ورود به خواب عمیق…",
            PowerService.DeepSleepAsync);
    }

    private async void OnSleepClick(object sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        await RunPowerActionAsync(
            "در حال اجرای Force Sleep…",
            PowerService.ForceSleepAsync);
    }

    private async void OnConfigureLidClick(object sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        SetBusy(true);
        StatusText.Text = "در حال تنظیم بستن درِ لپ‌تاپ…";
        StatusDetailText.Text = "پیام دسترسی Administrator را تأیید کنید.";

        try
        {
            bool configured = await PowerService.ConfigureLidForDeepSleepAsync();
            StatusText.Text = configured ? "تنظیم انجام شد" : "تنظیم انجام نشد";
            StatusDetailText.Text = configured
                ? "از این پس با بستن درِ لپ‌تاپ، دستگاه وارد خواب عمیق می‌شود."
                : "دسترسی Administrator تأیید نشد یا powercfg خطا داد.";
        }
        catch (Win32Exception exception) when (exception.NativeErrorCode == 1223)
        {
            StatusText.Text = "عملیات لغو شد";
            StatusDetailText.Text = "دسترسی Administrator توسط کاربر لغو شد.";
        }
        catch (InvalidOperationException exception)
        {
            ShowError(exception.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task RunPowerActionAsync(string status, Func<Task> action)
    {
        SetBusy(true);
        StatusText.Text = status;
        StatusDetailText.Text = "فایل‌های باز خود را قبل از ادامه ذخیره کنید.";

        try
        {
            await action();
            UpdateStatus();
        }
        catch (Win32Exception exception) when (exception.NativeErrorCode == 1223)
        {
            StatusText.Text = "عملیات لغو شد";
            StatusDetailText.Text = "دسترسی Administrator توسط کاربر لغو شد.";
        }
        catch (Exception exception) when (exception is InvalidOperationException or Win32Exception)
        {
            ShowError(exception.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void UpdateStatus()
    {
        if (PowerService.CanHibernate)
        {
            StatusText.Text = "خواب عمیق آماده است";
            StatusDetailText.Text = "برای خاموش‌شدن قطعی فن و CPU از دکمهٔ آبی استفاده کنید.";
        }
        else
        {
            StatusText.Text = "Hibernate هنوز فعال نیست";
            StatusDetailText.Text = "در اولین اجرا، برنامه با اجازهٔ شما آن را فعال می‌کند.";
        }

        SleepButton.IsEnabled = PowerService.CanSleep;
    }

    private void SetBusy(bool isBusy)
    {
        DeepSleepButton.IsEnabled = !isBusy;
        SleepButton.IsEnabled = !isBusy && PowerService.CanSleep;
        LidButton.IsEnabled = !isBusy;
    }

    private void ShowError(string message)
    {
        StatusText.Text = "خطا در تغییر وضعیت برق";
        StatusDetailText.Text = message;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        Close();
    }

    private void OnWindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _ = sender;
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        try
        {
            DragMove();
        }
        catch (InvalidOperationException)
        {
            // The mouse can be released before DragMove starts.
        }
    }
}
