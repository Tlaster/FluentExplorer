using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Win32Interop.Shared.Models;
using Win32Interop.Shell;

namespace Win32Interop.Handlers
{
    public class ContextMenuHandler : AbsHandler
    {
        private const int MAX_PATH = 256;
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

        private List<MenuInfo> GenerateMenuInfo(IntPtr menu, IContextMenu contextMenu, IContextMenu3 contextMenu3)
        {
            if (menu == IntPtr.Zero) return new List<MenuInfo>();
            var count = User32.GetMenuItemCount(menu);
            return Enumerable.Range(0, count).Select(it =>
            {
                var cch = GetTextLength(menu, it, true) + 1;
                var itemInfo = new MENUITEMINFO();
                itemInfo.cbSize = (uint) Marshal.SizeOf(itemInfo);
                itemInfo.fMask = MIIM.MIIM_STRING | MIIM.MIIM_SUBMENU | MIIM.MIIM_ID;
                itemInfo.cch = cch;
                itemInfo.dwTypeData = Marshal.AllocCoTaskMem((int) cch * sizeof(char));
                itemInfo.hSubMenu = IntPtr.Zero;
                User32.GetMenuItemInfo(menu, it, true, ref itemInfo);
                var title = Marshal.PtrToStringAuto(itemInfo.dwTypeData);
                Marshal.FreeCoTaskMem(itemInfo.dwTypeData);
                var id = Convert.ToInt64(itemInfo.wID);
                var command = string.Empty;
                if (!string.IsNullOrEmpty(title) && id > 0 && uint.TryParse(id + "", out var uid))
                {
                    command = GetCommandString(contextMenu, uid, true);
                }
                if (itemInfo.hSubMenu != IntPtr.Zero)
                {
                    var indexPointer = Marshal.AllocHGlobal(sizeof(int));
                    Marshal.WriteInt32(indexPointer, it);
                    contextMenu3.HandleMenuMsg2(KAN.WM_INITMENUPOPUP, itemInfo.hSubMenu, indexPointer,
                        out var result);
                }
                return new MenuInfo
                {
                    CommandName = command,
                    Title = title,
                    Id = id,
                    SubMenu = GenerateMenuInfo(itemInfo.hSubMenu, contextMenu, contextMenu3)
                };
            }).ToList();
        }


        private string GetCommandString(IContextMenu iContextMenu, uint idcmd, bool executeString)
        {
            var command = GetCommandStringW(iContextMenu, idcmd, executeString);

            if (string.IsNullOrEmpty(command))
                command = GetCommandStringA(iContextMenu, idcmd, executeString);

            return command;
        }

        private string GetCommandStringA(IContextMenu iContextMenu, uint idcmd, bool executeString)
        {
            var sb = new StringBuilder(MAX_PATH);
            iContextMenu.GetCommandString(
                idcmd,
                executeString ? GCS.VERBA : GCS.HELPTEXTA,
                0,
                sb,
                MAX_PATH);
            return sb.ToString();
        }

        private string GetCommandStringW(IContextMenu iContextMenu, uint idcmd, bool executeString)
        {
            var sb = new StringBuilder(MAX_PATH);
            iContextMenu.GetCommandString(
                idcmd,
                executeString ? GCS.VERBW : GCS.HELPTEXTW,
                0,
                sb,
                MAX_PATH);

            return sb.ToString();
        }

        public static Guid IID_IContextMenu2 = new Guid("{000214f4-0000-0000-c000-000000000046}");
        public static Guid IID_IContextMenu3 = new Guid("{bcfce0a0-ec17-11d0-8d10-00a0c90f2719}");
        public static Guid IID_IShellExtInit = new Guid("{000214e8-0000-0000-c000-000000000046}");

        public override async Task<string> Handle(string data)
        {
            var path = data;
            var shellItem = Shell32.SHCreateItemFromParsingName(path, IntPtr.Zero,
                typeof(IShellItem).GUID);
            var iContextMenuPtr = shellItem.BindToHandler(IntPtr.Zero, BHID.SFUIObject, typeof(IContextMenu).GUID);
            var contextMenu = Marshal.GetTypedObjectForIUnknown(iContextMenuPtr, typeof(IContextMenu)) as IContextMenu;
            
            //Marshal.QueryInterface(
            //    iContextMenuPtr,
            //    ref IID_IShellExtInit,
            //    out var iShellExtInitPtr);
            //var iShellExtInit = Marshal.GetTypedObjectForIUnknown(
            //    iShellExtInitPtr, typeof(IShellExtInit)) as IShellExtInit;
            //iShellExtInit.Initialize(IntPtr.Zero, IntPtr.Zero, 0);

            var menu = User32.CreatePopupMenu();

            contextMenu.QueryContextMenu(menu, 0, 0, int.MaxValue, CMF.EXPLORE | CMF.CANRENAME);


            Marshal.QueryInterface(iContextMenuPtr, ref IID_IContextMenu2, out var iContextMenuPtr2);
            Marshal.QueryInterface(iContextMenuPtr, ref IID_IContextMenu3, out var iContextMenuPtr3);

            var contextMenu2 =
                Marshal.GetTypedObjectForIUnknown(iContextMenuPtr2, typeof(IContextMenu2)) as IContextMenu2;

            var contextMenu3 =
                Marshal.GetTypedObjectForIUnknown(iContextMenuPtr3, typeof(IContextMenu3)) as IContextMenu3;
            var res = GenerateMenuInfo(menu, contextMenu, contextMenu3);
            return JsonConvert.SerializeObject(res);
        }
    }
}