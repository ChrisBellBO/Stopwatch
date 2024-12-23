using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Stopwatch
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      InitializeComponent();

      Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500, 150));
      Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize = new Size(500, 150);
      Windows.UI.ViewManagement.ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.PreferredLaunchViewSize;

      timer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 0, 0, 10)};
      timer.Tick += Timer_Tick;
    }

    private void Timer_Tick(object sender, object e)
    {
      currentDuration = DateTime.Now.Subtract(startTime);
      SetLabelText();
    }

    private void SetLabelText()
    {
      lcdLabel.Text = currentDuration.ToString(@"mm\:ss\:ff");
    }

    private readonly DispatcherTimer timer;
    private DateTime startTime;
    private TimeSpan currentDuration = new TimeSpan(0);
    private bool running;

    private void button_Click(object sender, RoutedEventArgs e)
    {
      if (running)
      {
        timer.Stop();
        button.Content = "Start";
      }
      else
      {
        startTime = DateTime.Now.Subtract(currentDuration);
        timer.Start();
        button.Content = "Stop";
      }
      running = !running;
    }

    private void resetButton_Click(object sender, RoutedEventArgs e)
    {
      currentDuration = new TimeSpan(0);
      startTime = DateTime.Now;
      SetLabelText();
    }

    private void lapButton_Click(object sender, RoutedEventArgs e)
    {
      if (timer.IsEnabled)
      {
        timer.Stop();
      }
      else
      {
        timer.Start();
      }
    }
  }
}
