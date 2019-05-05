using System;
using System.Runtime.InteropServices;

namespace Win32Interop.Shell
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
    public interface IShellItem
    {
        IntPtrSafeHandle BindToHandler(IntPtr pbc,
            [MarshalAs(UnmanagedType.LPStruct)]Guid bhid,
            [MarshalAs(UnmanagedType.LPStruct)]Guid riid);

        [PreserveSig]
        HResult GetParent(out IShellItem ppsi);

        IntPtr GetDisplayName(SIGDN sigdnName);

        SFGAO GetAttributes(SFGAO sfgaoMask);

        int Compare(IShellItem psi, SICHINT hint);
    };
    public class BHID
    {
        public static Guid SFObject { get; } = new Guid("3981e224-f559-11d3-8e3a-00c04f6837d5");

        public static Guid SFUIObject { get; } = new Guid("3981e225-f559-11d3-8e3a-00c04f6837d5");
    }
}