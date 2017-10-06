////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    using static System.Globalization.CultureInfo;

    internal static class Logger
    {
        internal static void Info(string format, params object[] args) => Log("Info ", format, args);

        internal static void Warn(string format, params object[] args) => Log("Warn ", format, args);

        internal static void Error(string format, params object[] args) => Log("Error", format, args);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const string LogTag = "SmartTrade";
        private static readonly TextWriter Writer = GetLogWriter();

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "There's no way to dispose this properly.")]
        private static TextWriter GetLogWriter() => new StreamWriter(GetLogFilePath(), true) { AutoFlush = true };

        private static string GetLogFilePath()
        {
            const int MaxFileCount = 10;
            const int MaxFileLength = 1 << 20;

            var folder = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.ToString(), "SmartTrade");
            Directory.CreateDirectory(folder);
            var files = new DirectoryInfo(folder).EnumerateFiles().OrderBy(i => i.Name).ToList();
            FileInfo lastFile;

            if ((files.Count > 0) && ((lastFile = files[files.Count - 1]).Length < MaxFileLength))
            {
                return lastFile.FullName;
            }
            else
            {
                while (files.Count >= MaxFileCount)
                {
                    files[0].Delete();
                    files.RemoveAt(0);
                }

                var dateString = DateTime.UtcNow.ToString("s", InvariantCulture);
                return Path.Combine(folder, dateString.Replace(":", string.Empty).Replace("-", string.Empty) + ".txt");
            }
        }

        private static void Log(string severity, string format, object[] args)
        {
            var line = DateTime.UtcNow.ToString("o", InvariantCulture) + " " + severity + " " +
                string.Format(InvariantCulture, format, args);
            Writer.WriteLine(line);
        }
    }
}