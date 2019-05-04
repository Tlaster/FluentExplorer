using System;
using System.Runtime.InteropServices;

namespace Win32Interop
{
    static class Extensions
    {
        public static void UseCoTask(this IntPtr intPtr, Action<IntPtr> block)
        {
            block(intPtr);
            Marshal.FreeCoTaskMem(intPtr);
        }

        public static T UseCoTask<T>(this IntPtr intPtr, Func<IntPtr, T> block)
        {
            var result = block(intPtr);
            Marshal.FreeCoTaskMem(intPtr);
            return result;
        }

        public static void UseComObject<T>(this T obj, Action<T> block)
        {
            block(obj);
            Marshal.ReleaseComObject(obj);
        }

        public static K UseComObject<T, K>(this T obj, Func<T, K> block)
        {
            var result = block(obj);
            Marshal.ReleaseComObject(obj);
            return result;
        }

        public static void UsePtr(this IntPtr intPtr, Action<IntPtr> block)
        {
            block(intPtr);
            Marshal.Release(intPtr);
        }

        public static T UsePtr<T>(this IntPtr intPtr, Func<IntPtr, T> block)
        {
            var result = block(intPtr);
            Marshal.Release(intPtr);
            return result;
        }
    }
}