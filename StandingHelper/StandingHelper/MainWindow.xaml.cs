using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace StandingHelper
{
    internal static class ProgramProperties
    {
        public static bool Running { get; set; }
        public static bool Standing = true;
        public static string ProgramText { get; set; }
        public static DateTime TimeRemaining { get; set; }
        public static int StandingTime { get; set; }
        public static int SittingTime { get; set; }
        public static bool BusinessRunning { get; set; }
        public static DateTime LastUpdated = DateTime.Now;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SynchronizationContext synchronizationContext;

        private async void Business()
        {
            ProgramProperties.BusinessRunning = true;
            while (ProgramProperties.Running)
            {
                await Task.Run(()=> StandingMethod());
                await Task.Run(() => SittingMethod());
            }

            ProgramProperties.BusinessRunning = false;
        }

        public void StandingMethod()
        {
            while (!ProgramProperties.Standing && ProgramProperties.Running)
            {
                ProgramProperties.ProgramText = "Click \"Stand\" Button";
            }
            ProgramProperties.Standing = true;
            ProgramProperties.TimeRemaining = DateTime.Now.AddMinutes(ProgramProperties.StandingTime);
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "StandingHelper.sounds.Geralt of Rivia - Let's get this over with_.wav";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(stream);
                player.Play();
                player.Dispose();
            }

            while (ProgramProperties.TimeRemaining > DateTime.Now && ProgramProperties.Running && ProgramProperties.Standing)
            {
                ProgramProperties.ProgramText = $"Standing for {(ProgramProperties.TimeRemaining - DateTime.Now).Minutes} minutes...";
                Thread.Sleep(1000);
            }
        }

        public void SittingMethod()
        {
            while (ProgramProperties.Standing && ProgramProperties.Running)
            {
                ProgramProperties.ProgramText = "Click \"Sit\" Button";
            }
            ProgramProperties.Standing = false;
            ProgramProperties.TimeRemaining = DateTime.Now.AddMinutes(ProgramProperties.SittingTime);
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "StandingHelper.sounds.Vilgefortz - Quite the menial task_.wav";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(stream);
                player.Play();
                player.Dispose();
            }

            while (ProgramProperties.TimeRemaining > DateTime.Now && ProgramProperties.Running && !ProgramProperties.Standing)
            {
                ProgramProperties.ProgramText = $"Sitting for {(ProgramProperties.TimeRemaining - DateTime.Now).Minutes} minutes...";
                Thread.Sleep(1000);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            synchronizationContext = SynchronizationContext.Current;
            ProgramProperties.Running = false;
            ToggleOptions();
        }
        
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ProgramProperties.Running = true;
            ProgramProperties.Standing = true;
            ProgramProperties.SittingTime = Convert.ToInt32(this.SittingTimeField.Text);
            ProgramProperties.StandingTime = Convert.ToInt32(this.StandingTimeField.Text);
            ToggleOptions();
            if (!ProgramProperties.BusinessRunning)
            {
                Business();
            }

            await Task.Run(() =>
            {
                while (ProgramProperties.Running)
                {
                    UpdateTextBox();
                }
            });
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            ProgramProperties.Running = false;
            ToggleOptions();
            this.UpdateText.Text = "Click Start";
        }

        private void StandButton_Click(object sender, RoutedEventArgs e)
        {
            ProgramProperties.Standing = true;
        }

        private void SitButton_Click(object sender, RoutedEventArgs e)
        {
            ProgramProperties.Standing = false;
        }

        private void UpdateTextBox()
        {
            var timeNow = DateTime.Now;

            if ((DateTime.Now - ProgramProperties.LastUpdated).Milliseconds <= 50) return;

            synchronizationContext.Post(new SendOrPostCallback(o =>
            {
                this.UpdateText.Text = ProgramProperties.ProgramText;
            }), 1);

            ProgramProperties.LastUpdated = timeNow;
        }

        private void ToggleOptions()
        {
            this.StartButton.IsEnabled = !ProgramProperties.Running;
            this.StopButton.IsEnabled = ProgramProperties.Running;
            this.SitButton.IsEnabled = ProgramProperties.Running;
            this.StandButton.IsEnabled = ProgramProperties.Running;
            this.StandingTimeField.IsEnabled = !ProgramProperties.Running;
            this.SittingTimeField.IsEnabled = !ProgramProperties.Running;
        }
    }
}
