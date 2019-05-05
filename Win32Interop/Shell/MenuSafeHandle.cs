using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace Win32Interop.Shell
{
    public class AutoDisposable<T> : IDisposable
    {
        public Action<T> Finalizer { get; }
        public T Instance { get; }

        public AutoDisposable(T instance, Action<T> finalizer)
        {
            Instance = instance;
            Finalizer = finalizer;
        }

        public void Dispose()
        {
            Finalizer.Invoke(Instance);
        }
    }

    public class ComObjDisposable<T> : AutoDisposable<T>
    {
        public ComObjDisposable(T instance) : base(instance, it => Marshal.ReleaseComObject(it))
        {
        }
    }

    public class MenuSafeHandle : SafeHandle
    {
        public MenuSafeHandle() : base(IntPtr.Zero, true)
        {
            
        }

        public MenuSafeHandle(IntPtr invalidHandleValue, bool ownsHandle) : base(invalidHandleValue, ownsHandle)
        {
        }

        protected override bool ReleaseHandle()
        {
            return User32.DestroyMenu(handle);
        }

        public override bool IsInvalid => handle == IntPtr.Zero;
    }

    public class IntPtrSafeHandle : SafeHandle
    {
        public IntPtrSafeHandle () : base(IntPtr.Zero, true)
        {
            
        }

        public IntPtrSafeHandle (IntPtr invalidHandleValue, bool ownsHandle) : base(invalidHandleValue, ownsHandle)
        {
        }
        
        protected override bool ReleaseHandle()
        {
            Marshal.Release(handle);
            return true;
        }

        public override bool IsInvalid => handle == IntPtr.Zero;
    }
}
