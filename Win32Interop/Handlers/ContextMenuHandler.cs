using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Win32Interop.Shared.Models;
using Win32Interop.Shell;

namespace Win32Interop.Handlers
{
    internal class ContextMenuHandler : AbsHandler
    {
        public override string Type { get; } = "ContextMenu";

        private uint GetTextLength(IntPtr hMenu, int i, bool byPosition)
        {
            var info = new MENUITEMINFO();
            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            info.fMask = MIIM.MIIM_TYPE;
            if (!User32.GetMenuItemInfo(
                hMenu,
                i,
                byPosition,
                ref info))
                throw new Win32Exception();
            return info.cch;
        }

        private List<MenuInfo> GenerateMenuInfo(IntPtr menu)
        {
            if (menu == IntPtr.Zero) return new List<MenuInfo>();
            var count = User32.GetMenuItemCount(menu);
            return Enumerable.Range(0, count).Select(it =>
            {
                var cch = GetTextLength(menu, it, true) + 1;
                var itemInfo = new MENUITEMINFO();
                itemInfo.cbSize = (uint) Marshal.SizeOf(itemInfo);
                itemInfo.fMask = MIIM.MIIM_STRING | MIIM.MIIM_SUBMENU | MIIM.MIIM_BITMAP | MIIM.MIIM_ID;
                itemInfo.cch = cch;
                itemInfo.dwTypeData = Marshal.AllocCoTaskMem((int) cch * sizeof(char));
                itemInfo.hSubMenu = IntPtr.Zero;
                User32.GetMenuItemInfo(menu, it, true, ref itemInfo);
                var title = Marshal.PtrToStringAuto(itemInfo.dwTypeData);
                Marshal.FreeCoTaskMem(itemInfo.dwTypeData);


                return new MenuInfo
                {
                    Title = title,
                    Id = Convert.ToInt64(itemInfo.wID),
                    SubMenu = GenerateMenuInfo(itemInfo.hSubMenu)
                };
            }).ToList();
        }

        public override async Task<string> Handle(string data)
        {
            var path = data;
            var shellItem = Shell32.SHCreateItemFromParsingName(path, IntPtr.Zero,
                typeof(IShellItem).GUID);
            var folder = Marshal.GetTypedObjectForIUnknown(shellItem.BindToHandler(IntPtr.Zero,
                    BHID.SFObject, typeof(IShellFolder).GUID),
                typeof(IShellFolder)) as IShellFolder;
            var resultPtr = shellItem.GetDisplayName(SIGDN.FILESYSPATH);
            var ptr = Marshal.PtrToStringUni(resultPtr);
            Marshal.FreeCoTaskMem(resultPtr);
            var pids = Shell32.ILFindLastID(Shell32.SHGetIDListFromObject(shellItem));
            folder.GetUIObjectOf(IntPtr.Zero, 1, new[] {pids}, typeof(IContextMenu).GUID, 0, out var result);
            var contextMenu = Marshal.GetTypedObjectForIUnknown(result, typeof(IContextMenu)) as IContextMenu;
            var menu = User32.CreatePopupMenu();
            contextMenu.QueryContextMenu(menu, 0, 1, int.MaxValue, CMF.NORMAL);
            var res = GenerateMenuInfo(menu);
            return JsonConvert.SerializeObject(res);
        }
    }

}