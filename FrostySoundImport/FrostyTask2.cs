using System.Windows.Controls;
using FrostyEditor;

namespace DanDev.FrostySoundImport
{
    // Decompiled with JetBrains decompiler
    // Type: FrostyEditor.FrostyTask
    // Assembly: FrostyEditor, Version=1.0.5.9, Culture=neutral, PublicKeyToken=null
    // MVID: F67C98FD-21DF-4D85-A71B-9DF78751463F
    // Assembly location: E:\frosty4\FrostyEditor.exe

    using FrostySdk.Interfaces;
    using System;
    using System.Windows;
    using System.Windows.Shell;

    namespace DanDev.FrostySoundImport
    {
        public class FrostyTask2
        {
            public static void Begin(string task, string initialStatus = "", Action<bool> cancelAction = null)
            {
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                var taskCancelButton = mainWindow.GetFieldValue<Button>("taskCancelButton");
                var taskTextBlock = mainWindow.GetFieldValue<TextBlock>("taskTextBlock");
                var taskWindow = mainWindow.GetFieldValue<Grid>("taskWindow");
                var statusTextBox = mainWindow.GetFieldValue<TextBlock>("statusTextBox");
                var taskProgressBar = mainWindow.GetFieldValue<ProgressBar>("taskProgressBar");


                taskTextBlock.Text = task;
                statusTextBox.Text = initialStatus;
                taskProgressBar.Value = 0.0;
                taskWindow.Visibility = Visibility.Visible;
                taskCancelButton.Visibility = Visibility.Collapsed;
                if (cancelAction == null)
                    return;
                taskCancelButton.Visibility = Visibility.Visible;
                taskCancelButton.Click += (RoutedEventHandler)((o, e) => cancelAction(true));
            }

            public static void End()
            {
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.GetFieldValue<Grid>("taskWindow").Visibility = Visibility.Collapsed;
                mainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
            }

            public static void Update(string status = null, double? progress = null)
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                    if (status != null)
                        mainWindow.GetFieldValue<TextBlock>("statusTextBox").Text = status;
                    if (!progress.HasValue)
                        return;
                    mainWindow.GetFieldValue<ProgressBar>("taskProgressBar").Value = progress.Value;
                    mainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                    mainWindow.TaskbarItemInfo.ProgressValue = progress.Value / 100.0;
                }));
            }

            public class Logger : ILogger
            {
                public void Log(string text, params object[] vars)
                {
                    if (text.StartsWith("progress:"))
                    {
                        text = text.Replace("progress:", "");
                        FrostyTask2.Update((string)null, new double?(double.Parse(text)));
                    }
                    else
                        FrostyTask2.Update(string.Format(text, vars), new double?());
                }

                public void LogWarning(string text, params object[] vars)
                {
                    throw new NotImplementedException();
                }

                public void LogError(string text, params object[] vars)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }

}