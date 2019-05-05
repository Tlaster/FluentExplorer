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

        public static Guid IID_IContextMenu2 = new Guid("{000214f4-0000-0000-c000-000000000046}");
        public static Guid IID_IContextMenu3 = new Guid("{bcfce0a0-ec17-11d0-8d10-00a0c90f2719}");
        public static Guid IID_IShellExtInit = new Guid("{000214e8-0000-0000-c000-000000000046}");
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
                    command = GetCommandString(contextMenu, uid, true);
                if (itemInfo.hSubMenu != IntPtr.Zero)
                    Marshal.AllocCoTaskMem(sizeof(int)).UseCoTask(indexPointer =>
                    {
                        Marshal.WriteInt32(indexPointer, it);
                        contextMenu3.HandleMenuMsg2(KAN.WM_INITMENUPOPUP, itemInfo.hSubMenu, indexPointer,
                            out _);
                    });
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

        public override async Task<string> Handle(string data)
        {
            var item = JsonConvert.DeserializeObject<ContextMenuAction>(data);
            var path = item.Path;
            using var shellItem =
                new ComObjDisposable<IShellItem>(
                    Shell32.SHCreateItemFromParsingName(path, IntPtr.Zero, typeof(IShellItem).GUID));
            using var iContextMenuPtr =
                shellItem.Instance.BindToHandler(IntPtr.Zero, BHID.SFUIObject, typeof(IContextMenu).GUID);
            using var contextMenu = new ComObjDisposable<IContextMenu>(
                Marshal.GetTypedObjectForIUnknown(iContextMenuPtr.DangerousGetHandle(), typeof(IContextMenu)) as
                    IContextMenu);
            using var menu = User32.CreatePopupMenu();
            contextMenu.Instance.QueryContextMenu(menu.DangerousGetHandle(), 0, 0, int.MaxValue,
                CMF.NORMAL);
            Marshal.QueryInterface(iContextMenuPtr.DangerousGetHandle(), ref IID_IContextMenu2,
                out var iContextMenuPtr2);
            Marshal.QueryInterface(iContextMenuPtr.DangerousGetHandle(), ref IID_IContextMenu3,
                out var iContextMenuPtr3);
            using var contextMenuPtr2 = new IntPtrSafeHandle(iContextMenuPtr2, true);
            using var contextMenuPtr3 = new IntPtrSafeHandle(iContextMenuPtr3, true);
            using var contextMenu3 = new ComObjDisposable<IContextMenu3>(Marshal.GetTypedObjectForIUnknown(
                contextMenuPtr3.DangerousGetHandle(),
                typeof(IContextMenu3)) as IContextMenu3);
            using var contextMenu2 = new ComObjDisposable<IContextMenu2>(
                Marshal.GetTypedObjectForIUnknown(contextMenuPtr2.DangerousGetHandle(), typeof(IContextMenu2)) as
                    IContextMenu2);
            if (item.Type == ContextMenuAction.ActionType.ShowMenu)
            {
                var res = GenerateMenuInfo(menu.DangerousGetHandle(), contextMenu.Instance,
                    contextMenu3.Instance);
                return JsonConvert.SerializeObject(res);
            }
            else
            {
                var id = item.MenuId;
                const int SW_SHOWNORMAL = 1;
                var invoke = new CMINVOKECOMMANDINFO_ByIndex();
                invoke.cbSize = Marshal.SizeOf(invoke);
                invoke.iVerb = id;
                invoke.nShow = SW_SHOWNORMAL;
                contextMenu2.Instance.InvokeCommand(ref invoke);
                return string.Empty;
            }
        }
    }
}