// Copyright © 2021-2024 Matan Brightbert
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.




using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Scrupdate.Classes.Objects;


namespace Scrupdate.Classes.Utilities
{
    public static class WindowUtilities
    {
        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public const double BASE_DISPLAY_WIDTH_FOR_WINDOWS_SCALING = 2560.0D;
        public const double BASE_DISPLAY_HEIGHT_FOR_WINDOWS_SCALING = 1440.0D;
        public const double BASE_WINDOW_WIDTH_FOR_WINDOWS_SCALING = 1000.0D;
        public const double BASE_WINDOW_HEIGHT_FOR_WINDOWS_SCALING = 800.0D;
        public const double WINDOWS_MARGIN = 25.0D;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static double GetWindowsRenderingScale(SettingsHandler settingsHandler)
        {
            return GetWindowsRenderingScale(settingsHandler, false, out _);
        }
        public static double GetWindowsRenderingScale(SettingsHandler settingsHandler,
                                                      bool changeWindowsScalingFactorIfInvalid,
                                                      out bool windowsScalingFactorHasBeenChangedDueToBeingInvalid)
        {
            windowsScalingFactorHasBeenChangedDueToBeingInvalid = false;
            if (settingsHandler == null)
                throw new ArgumentNullException(nameof(settingsHandler));
            try
            {
                Rect displayWorkArea = SystemParameters.WorkArea;
                if (BASE_WINDOW_WIDTH_FOR_WINDOWS_SCALING + WINDOWS_MARGIN * 2.0D > displayWorkArea.Width ||
                    BASE_WINDOW_HEIGHT_FOR_WINDOWS_SCALING + WINDOWS_MARGIN * 2.0D > displayWorkArea.Height)
                {
                    if (changeWindowsScalingFactorIfInvalid)
                    {
                        if (settingsHandler.SettingsInMemory.Appearance.WindowsScalingFactor > 1.0D)
                        {
                            settingsHandler.SettingsInMemory.Appearance.WindowsScalingFactor = 1.0D;
                            settingsHandler.SaveSettingsFromMemoryToSettingsFile();
                            windowsScalingFactorHasBeenChangedDueToBeingInvalid = true;
                        }
                    }
                    if (BASE_WINDOW_WIDTH_FOR_WINDOWS_SCALING - displayWorkArea.Width >= BASE_WINDOW_HEIGHT_FOR_WINDOWS_SCALING - displayWorkArea.Height)
                        return (displayWorkArea.Width / (BASE_WINDOW_WIDTH_FOR_WINDOWS_SCALING + WINDOWS_MARGIN * 2.0D));
                    return (displayWorkArea.Height / (BASE_WINDOW_HEIGHT_FOR_WINDOWS_SCALING + WINDOWS_MARGIN * 2.0D));
                }
                else
                {
                    if (settingsHandler.SettingsInMemory.Appearance.WindowsScalingFactor == 0.0D)
                    {
                        double ratioBetweenCurrentDisplayResolutionAndBaseDisplayResolution = 1.0D;
                        if (displayWorkArea.Width - BASE_DISPLAY_WIDTH_FOR_WINDOWS_SCALING >= displayWorkArea.Height - BASE_DISPLAY_HEIGHT_FOR_WINDOWS_SCALING)
                            ratioBetweenCurrentDisplayResolutionAndBaseDisplayResolution = displayWorkArea.Height / BASE_DISPLAY_HEIGHT_FOR_WINDOWS_SCALING;
                        else
                            ratioBetweenCurrentDisplayResolutionAndBaseDisplayResolution = displayWorkArea.Width / BASE_DISPLAY_WIDTH_FOR_WINDOWS_SCALING;
                        if (ratioBetweenCurrentDisplayResolutionAndBaseDisplayResolution > 1.0D)
                            return (Math.Round(ratioBetweenCurrentDisplayResolutionAndBaseDisplayResolution * 4.0D, MidpointRounding.AwayFromZero) / 4.0D);
                        return 1.0D;
                    }
                    else
                    {
                        if (BASE_WINDOW_WIDTH_FOR_WINDOWS_SCALING * settingsHandler.SettingsInMemory.Appearance.WindowsScalingFactor + WINDOWS_MARGIN * 2.0D > displayWorkArea.Width ||
                            BASE_WINDOW_HEIGHT_FOR_WINDOWS_SCALING * settingsHandler.SettingsInMemory.Appearance.WindowsScalingFactor + WINDOWS_MARGIN * 2.0D > displayWorkArea.Height)
                        {
                            double renderingScaleOfWindow = 1.0D;
                            for (double i = 1.25D; true; i += 0.25D)
                            {
                                if (BASE_WINDOW_WIDTH_FOR_WINDOWS_SCALING * i + WINDOWS_MARGIN * 2.0D > displayWorkArea.Width ||
                                    BASE_WINDOW_HEIGHT_FOR_WINDOWS_SCALING * i + WINDOWS_MARGIN * 2.0D > displayWorkArea.Height)
                                {
                                    break;
                                }
                                renderingScaleOfWindow = i;
                            }
                            if (changeWindowsScalingFactorIfInvalid)
                            {
                                settingsHandler.SettingsInMemory.Appearance.WindowsScalingFactor = renderingScaleOfWindow;
                                settingsHandler.SaveSettingsFromMemoryToSettingsFile();
                                windowsScalingFactorHasBeenChangedDueToBeingInvalid = true;
                            }
                            return renderingScaleOfWindow;
                        }
                        return settingsHandler.SettingsInMemory.Appearance.WindowsScalingFactor;
                    }
                }
            }
            catch
            {
                return 1.0D;
            }
        }
        public static bool ChangeWindowRenderingScaleAndMoveWindowIntoScreenBoundaries(Window window,
                                                                                       Size baseSizeOfWindow,
                                                                                       double windowRenderingScale)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));
            try
            {
                Rect displayWorkArea = SystemParameters.WorkArea;
                Size newWindowSize = new Size(
                    baseSizeOfWindow.Width * windowRenderingScale,
                    baseSizeOfWindow.Height * windowRenderingScale
                );
                window.MinWidth = newWindowSize.Width;
                window.Width = newWindowSize.Width;
                window.MinHeight = newWindowSize.Height;
                window.Height = newWindowSize.Height;
                ((Panel)window.Content).LayoutTransform = new ScaleTransform(
                    windowRenderingScale,
                    windowRenderingScale
                );
                MoveWindowIntoScreenBoundaries(window, true);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool MoveWindowIntoScreenBoundaries(Window window,
                                                          bool resizeWindowIfItDoesNotFitScreenBoundaries)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));
            try
            {
                Rect displayWorkArea = SystemParameters.WorkArea;
                if (resizeWindowIfItDoesNotFitScreenBoundaries)
                {
                    if (window.Width + WINDOWS_MARGIN * 2.0D > displayWorkArea.Width)
                        window.Width = displayWorkArea.Width - WINDOWS_MARGIN * 2.0D;
                    if (window.Height + WINDOWS_MARGIN * 2.0D > displayWorkArea.Height)
                        window.Height = displayWorkArea.Height - WINDOWS_MARGIN * 2.0D;
                }
                else if (window.Width + WINDOWS_MARGIN * 2.0D > displayWorkArea.Width ||
                         window.Height + WINDOWS_MARGIN * 2.0D > displayWorkArea.Height)
                {
                    return false;
                }
                if (window.Left - WINDOWS_MARGIN < 0)
                    window.Left = WINDOWS_MARGIN;
                if (window.Top - WINDOWS_MARGIN < 0)
                    window.Top = WINDOWS_MARGIN;
                if (window.Left + (window.Width + WINDOWS_MARGIN) > displayWorkArea.Width)
                    window.Left = displayWorkArea.Width - (window.Width + WINDOWS_MARGIN);
                if (window.Top + (window.Height + WINDOWS_MARGIN) > displayWorkArea.Height)
                    window.Top = displayWorkArea.Height - (window.Height + WINDOWS_MARGIN);
                return true;
            }
            catch
            {
                return false;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
