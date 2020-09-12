﻿using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Quamotion.GitVersioning.Git
{
    public static class FileHelpers
    {
        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, CharSet = CharSet.Unicode, BestFitMapping = false, ExactSpelling = true)]
        public static unsafe extern SafeFileHandle CreateFile(
            string filename,
            FileAccess access,
            FileShare share,
            IntPtr securityAttributes,
            FileMode creationDisposition,
            FileAttributes flagsAndAttributes,
            IntPtr templateFile);

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, CharSet = CharSet.Unicode, BestFitMapping = false, ExactSpelling = true)]
        public static unsafe extern SafeFileHandle CreateFile(
            byte* filename,
            FileAccess access,
            FileShare share,
            IntPtr securityAttributes,
            FileMode creationDisposition,
            FileAttributes flagsAndAttributes,
            IntPtr templateFile);

        private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool TryOpen(string path, out Stream stream)
        {
            if (IsWindows)
            {
                var handle = CreateFile(path, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

                if (!handle.IsInvalid)
                {
                    stream = new FileStream(handle, FileAccess.Read);
                    return true;
                }
                else
                {
                    stream = null;
                    return false;
                }
            }
            else
            {
                if (!File.Exists(path))
                {
                    stream = null;
                    return false;
                }

                stream = File.OpenRead(path);
                return true;
            }
        }

        public static unsafe bool TryOpen(Span<byte> path, out Stream stream)
        {
            if (IsWindows)
            {
                SafeFileHandle handle;

                fixed (byte* pathPtr = path)
                {
                    handle = CreateFile(pathPtr, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
                }

                if (!handle.IsInvalid)
                {
                    stream = new FileStream(handle, FileAccess.Read);
                    return true;
                }
                else
                {
                    stream = null;
                    return false;
                }
            }
            else
            {
                var fullPath = Encoding.Unicode.GetString(path);

                if (!File.Exists(fullPath))
                {
                    stream = null;
                    return false;
                }

                stream = File.OpenRead(fullPath);
                return true;
            }
        }
    }
}
