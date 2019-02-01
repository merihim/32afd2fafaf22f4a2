﻿using System;
using System.IO;
using System.Media;
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
                await Task.Run(() => StandingMethod());
                await Task.Run(() => SittingMethod());
            }

            ProgramProperties.BusinessRunning = false;
        }

        public void StandingMethod()
        {
            ProgramProperties.Standing = true;
            ProgramProperties.TimeRemaining = DateTime.Now.AddMinutes(ProgramProperties.StandingTime);
            PlayStandSound();
            while (ProgramProperties.TimeRemaining > DateTime.Now && ProgramProperties.Running && ProgramProperties.Standing)
            {
                ProgramProperties.ProgramText = $"Standing for {(ProgramProperties.TimeRemaining - DateTime.Now).Minutes} minutes...";
                Thread.Sleep(1000);
            }
            PlaySitSound();
        }

        public void SittingMethod()
        {
            ProgramProperties.Standing = false;
            ProgramProperties.TimeRemaining = DateTime.Now.AddMinutes(ProgramProperties.SittingTime);
            PlaySitSound();
            while (ProgramProperties.TimeRemaining > DateTime.Now && ProgramProperties.Running && !ProgramProperties.Standing)
            {
                ProgramProperties.ProgramText = $"Sitting for {(ProgramProperties.TimeRemaining - DateTime.Now).Minutes} minutes...";
                Thread.Sleep(1000);
            }
            PlayStandSound();
        }

        public void PlayStandSound()
        {
            SystemSounds.Asterisk.Play();
            System.Threading.Thread.Sleep(2000);
            SystemSounds.Asterisk.Play();
            System.Threading.Thread.Sleep(2000);
            SystemSounds.Asterisk.Play();
        }

        public void PlaySitSound()
        {
            var resourceName = "StandingHelper.sounds.Vilgefortz - Quite the menial task_.wav";
            PlaySound(resourceName);
        }

        public void PlaySound(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(stream);
                player.Play();
                player.Dispose();
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
