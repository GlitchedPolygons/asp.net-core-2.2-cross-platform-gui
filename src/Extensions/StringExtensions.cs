// Copyright (c) 2019, Raphael Beck. All rights reserved.
// Use of this source code is governed by the BSD 3-Clause license that can be found in the repository root directory's LICENSE file.

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CrossPlatformGUI.Extensions
{
    /// <summary>
    /// Extensions method for <c>string</c>s.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Opens the <c>string</c> URL in the browser.
        /// </summary>
        /// <param name="url">The URL <see cref="string"/> to open.</param>
        public static void OpenUrlInBrowser(this string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
    }
}
