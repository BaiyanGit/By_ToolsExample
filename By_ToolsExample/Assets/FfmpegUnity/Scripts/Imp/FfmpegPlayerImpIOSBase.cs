#if UNITY_IOS || ((UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX) && !FFMPEG_UNITY_USE_BINARY_MAC)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace FfmpegUnity
{
    public abstract class FfmpegPlayerImpIOSBase : FfmpegPlayerImpBase
    {
        TextReader reader_;

#if UNITY_IOS
        const string IMPORT_NAME = "__Internal";
#else
        const string IMPORT_NAME = "FfmpegUnityMacPlugin";
#endif

        [DllImport(IMPORT_NAME)]
        static extern IntPtr ffmpeg_ffprobeExecuteAsync(string command);
        [DllImport(IMPORT_NAME)]
        static extern bool ffmpeg_isRunnning(IntPtr session);
        [DllImport(IMPORT_NAME)]
        static extern int ffmpeg_getOutputLength(IntPtr session);
        [DllImport(IMPORT_NAME)]
        static extern void ffmpeg_getOutput(IntPtr session, int startIndex, IntPtr output, int outputLength);
        [DllImport(IMPORT_NAME)]
        static extern void ffmpeg_mkpipe(IntPtr output, int outputLength);
        [DllImport(IMPORT_NAME)]
        static extern void ffmpeg_closePipe(string pipeName);

        public FfmpegPlayerImpIOSBase(FfmpegPlayerCommand playerCommand) : base(playerCommand)
        {
        }

        public override IEnumerator OpenFfprobeReaderCoroutine(string inputPathAll)
        {
            IntPtr ffprobeSession = ffmpeg_ffprobeExecuteAsync("-i \"" + inputPathAll + "\" -show_streams");

            while (ffmpeg_isRunnning(ffprobeSession))
            {
                yield return null;
            }

            int allocSize = ffmpeg_getOutputLength(ffprobeSession) + 1;
            IntPtr hglobal = Marshal.AllocHGlobal(allocSize);
            ffmpeg_getOutput(ffprobeSession, 0, hglobal, allocSize);
            string outputStr = Marshal.PtrToStringAuto(hglobal);
            Marshal.FreeHGlobal(hglobal);

            reader_ = new StringReader(outputStr);
        }

        public override TextReader OpenFfprobeReader(string inputPathAll)
        {
            return reader_;
        }

        public override void CloseFfprobeReader()
        {
            if (reader_ != null)
            {
                //reader_.ReadToEnd();
                reader_.Dispose();
                reader_ = null;
            }
        }
    }
}

#endif