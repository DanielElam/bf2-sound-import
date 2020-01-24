// Program.cs - FrostySoundImport
// Copyright (C) 2020 Daniel Elam <dan@dandev.uk>
// This file is subject to the terms and conditions defined in the 'LICENSE' file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CSCore;
using CSCore.MediaFoundation;
using DanDev.FrostySoundImport.DanDev.FrostySoundImport;
using Frosty.Controls;
using FrostyEditor;
using FrostyEditor.Controls;
using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Microsoft.Win32;
using MeshSet = FrostySdk.Resources.MeshSet;

namespace DanDev.FrostySoundImport
{
    struct SoundImport
    {
        public byte[] Chunk;
        public uint ChunkOffset;
        public uint ChunkSize;
        public float SegmentLength;
        public string SourceFile;
    }

    class Program
    {
        private static MainWindow _mainWindow;
        private static FrostySoundWaveEditor _currentEditor;
        private static FrostyTabControl _tabControl;
        private static App _app;
        private static string _version;

        [STAThread]
        static void Main(string[] args)
        {
            var domain = AppDomain.CreateDomain("FrostyEditor2");

            domain.Load(typeof(App).Assembly.GetName());

            domain.DoCallBack(() =>
            {
                Application.ResourceAssembly = typeof(App).Assembly;
                _app = new App();

                _app.Activated += OnAppActivated;
                _app.InitializeComponent();
                _app.Run();
            });
        }

        private static void OnAppActivated(object sender, EventArgs e)
        {
            if (_app.MainWindow is FrostyEditor.Windows.PrelaunchWindow prelaunchWindow)
            {
                prelaunchWindow.Closed += (object sender2, EventArgs e2) =>
                {
                    if (_app.MainWindow is FrostyEditor.Windows.SplashWindow splashWindow)
                    {
                        splashWindow.Closed += (object sender3, EventArgs e3) =>
                        {
                            if (_app.MainWindow is MainWindow mainWindow)
                            {
                                _mainWindow = mainWindow;
                                OnMainWindowLaunch(mainWindow);
                            }
                        };
                    }
                };

                _app.Activated -= OnAppActivated;
            }
        }

        private static void OnMainWindowLaunch(MainWindow mainWindow)
        {
            _version = typeof(Program).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            App.Logger.Log($"FrostySoundImport by DanDev - Version {_version}");

            // hack because Frosty calls GetEntryAssembly() which returns null normally
            var domainManager2 = new AppDomainManager();
            domainManager2.SetFieldValue("m_entryAssembly", Application.ResourceAssembly);
            AppDomain.CurrentDomain.SetFieldValue("_domainManager", domainManager2);

            _tabControl = mainWindow.GetFieldValue<FrostyTabControl>("tabControl");
            _tabControl.SelectionChanged += TabControlOnSelectionChanged;
        }

        private static void TabControlOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = _tabControl.SelectedItem as FrostyTabItem;
            if (!(selectedItem?.Content is FrostySoundWaveEditor content))
                return;
            _currentEditor = content;

            // replace the items in the toolbar with a new list that has our custom command inserted in
            var control = FindChild<ItemsControl>(_mainWindow, "editorToolbarItems");
            var items = content.RegisterToolbarItems();
            items.Add(new ToolbarItem("DanDev Import", "Import by DanDev", "Images/Import.png", new RelayCommand(_ => OnImportSoundCommand(false), _ => true)));
            items.Add(new ToolbarItem("DanDev Import (Force Mono)", "Import by DanDev", "Images/Import.png", new RelayCommand(_ => OnImportSoundCommand(true), _ => true)));
            control.ItemsSource = items;
        }

        public static string EncodeParameterArgument(string original)
        {
            if (string.IsNullOrEmpty(original))
                return original;
            string value = Regex.Replace(original, @"(\\*)" + "\"", @"$1\$0");
            value = Regex.Replace(value, @"^(.*\s.*?)(\\*)$", "\"$1$2$2\"");
            return value;
        }

        private static async void OnImportSoundCommand(bool forceMono)
        {
            var asset = _currentEditor.Asset;
            var soundWave = (SoundWaveAsset)asset.RootObject;

            byte chunkIndex = 0;

            if (soundWave.Localization != null && soundWave.Localization.Count > 0)
            {
                var english = soundWave.Localization[0];
                chunkIndex = soundWave.RuntimeVariations[english.FirstVariationIndex].ChunkIndex;
            }

            var chunk = soundWave.Chunks[chunkIndex];

            var chunkEntry = App.AssetManager.GetChunkEntry(chunk.ChunkId);

            var ofd = new OpenFileDialog();
            ofd.Filter = "All Media Files|*.wav;*.mp3";
            ofd.Title = "Open Audio";
            ofd.Multiselect = true;

            if (ofd.ShowDialog(_mainWindow) != true)
                return;

            FrostyTask2.Begin("Importing Chunk", "");
            await Task.Run(() =>
            {
                var imports = new List<SoundImport>();
                var totalChunkSize = 0u;

                foreach (var fileName in ofd.FileNames)
                {
                    var tempFile = Path.GetTempFileName();
#if DEBUG
                    var tempFile2 = Path.GetTempFileName();

                    var decoder = new CSCore.Ffmpeg.FfmpegDecoder(fileName);
                    var source = decoder.ChangeSampleRate(48000);
                    if (forceMono)
                        decoder.ToMono();

                    using (var encoder = MediaFoundationEncoder.CreateMP3Encoder(source.WaveFormat,
                        tempFile2))
                    {
                        var buffer = new byte[source.WaveFormat.BytesPerSecond];
                        int read;
                        while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            encoder.Write(buffer, 0, read);
                        }
                    }
#else
                    var tempFile2 = fileName;
#endif

                    try
                    {
                        var fileNameEncoded = EncodeParameterArgument(tempFile2);
                        App.Logger.Log($"- \"{fileNameEncoded}\"");
                        var result = Primrose.Utility.ExternalTool.Run(@"dandev-el3.exe", $"{fileNameEncoded} -o {tempFile}",
                            out var stdout, out var stderr);

                        if (!string.IsNullOrEmpty(stderr))
                        {
                            App.Logger.LogError(stderr);
                        }

                        if (result == 0)
                        {
                            using (var nativeReader =
                                new NativeReader(
                                    new FileStream(tempFile, FileMode.Open, FileAccess.Read)))
                            {
                                var end = nativeReader.ReadToEnd();

                                if (end.Length > 0 && end[0] == 0x48)
                                {
                                    var lines = stdout.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                                    foreach (var line in lines)
                                    {
                                        App.Logger.Log("> " + line);
                                    }

                                    var chunkSize =
                                        uint.Parse(lines.First(l => l.StartsWith("ChunkSize:")).Substring(10));
                                    var segmentLength =
                                        float.Parse(lines.First(l => l.StartsWith("SegmentLength:")).Substring(15));

                                    imports.Add(new SoundImport
                                    {
                                        SourceFile = fileName,
                                        Chunk = end,
                                        ChunkOffset = totalChunkSize,
                                        ChunkSize = chunkSize,
                                        SegmentLength = segmentLength
                                    });
                                    totalChunkSize += chunkSize;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        App.Logger.LogError(e.Message);
                        App.Logger.LogError(e.StackTrace);
                    }
                    finally
                    {
                        try { File.Delete(tempFile); } catch (Exception) { /* ignored */ }
                        try { File.Delete(tempFile2); } catch (Exception) { /* ignored */ }
                    }
                }

                if (soundWave.Localization.Count > 0)
                {
                    var english = soundWave.Localization[0];

                    if (soundWave.Localization.Count > 1)
                        soundWave.Localization.RemoveRange(1, soundWave.Localization.Count - 1);

                    english.FirstVariationIndex = 0;
                    english.VariationCount = (ushort)imports.Count;
                }

                soundWave.Segments.Clear();
                soundWave.RuntimeVariations.Clear();

                var combinedChunk = new byte[totalChunkSize];

                for (var i = 0; i < imports.Count; i++)
                {
                    var import = imports[i];

                    App.Logger.Log($"Import[{i}] - SampleOffset: {import.ChunkOffset} - SegmentLength: {import.SegmentLength} - SourceFile: {import.SourceFile}");

                    soundWave.Segments.Add(new SoundWaveVariationSegment
                    {
                        SamplesOffset = import.ChunkOffset,
                        SeekTableOffset = uint.MaxValue,
                        SegmentLength = import.SegmentLength,
                    });

                    soundWave.RuntimeVariations.Add(new SoundWaveRuntimeVariation
                    {
                        ChunkIndex = chunkIndex,
                        FirstLoopSegmentIndex = 0,
                        FirstSegmentIndex = (ushort)i,
                        LastLoopSegmentIndex = 0,
                        PersistentDataSize = 0,
                        SegmentCount = 1,
                        Weight = 100,
                    });

                    Debug.Assert(import.ChunkSize == import.Chunk.Length);

                    Array.Copy(import.Chunk, 0, combinedChunk, (int)import.ChunkOffset, import.ChunkSize);
                }

                App.AssetManager.ModifyChunk(chunkEntry.Id, combinedChunk);

                chunk.ChunkSize = totalChunkSize;
                asset.Update();
                _app.Dispatcher.InvokeAsync(() =>
                {
                    App.AssetManager.ModifyEbx(_currentEditor.AssetEntry.Name, asset);
                });

            });
            FrostyTask2.End();
        }


        /// <summary>
        /// Finds a child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var childType = child as T;
                if (childType == null)
                {
                    foundChild = FindChild<T>(child, childName);

                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
    }

}
