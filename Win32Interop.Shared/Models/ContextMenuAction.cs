using System;
using System.Collections.Generic;
using System.Text;

namespace Win32Interop.Shared.Models
{
    class ContextMenuAction
    {
        public enum ActionType
        {
            ShowMenu,
            InvokeCommand
        }

        public ActionType Type { get; set; }
        public string Path { get; set; }
        public int MenuId { get; set; }
    }
}
