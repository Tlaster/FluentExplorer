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

        private List<MenuInfo> GenerateMenuInfo(IntPtr menu, IContextMenu contextMenu)
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
                var id = Convert.ToInt64(itemInfo.wID);
                var command = string.Empty;
                if (!string.IsNullOrEmpty(title) && id > 0 && uint.TryParse(id + "", out var uid))
                {
                    command = GetCommandString(contextMenu, uid - 1, true);
                }

                return new MenuInfo
                {
                    CommandName = command,
                    Title = title,
                    Id = id,
                    SubMenu = GenerateMenuInfo(itemInfo.hSubMenu, contextMenu)
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
            var path = data;
            var shellItem = Shell32.SHCreateItemFromParsingName(path, IntPtr.Zero,
                typeof(IShellItem).GUID);
            var contextMenu = Marshal.GetTypedObjectForIUnknown(
                shellItem.BindToHandler(IntPtr.Zero, BHID.SFUIObject, typeof(IContextMenu).GUID),
                typeof(IContextMenu)) as IContextMenu;
            var menu = User32.CreatePopupMenu();
            contextMenu.QueryContextMenu(menu, 0, 1, int.MaxValue, CMF.NORMAL);
            var res = GenerateMenuInfo(menu, contextMenu);
            return JsonConvert.SerializeObject(res);
        }
    }
}