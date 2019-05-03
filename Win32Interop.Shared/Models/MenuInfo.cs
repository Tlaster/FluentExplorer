using System;
using System.Collections.Generic;
using System.Text;

namespace Win32Interop.Shared.Models
{
    class MenuInfo
    {
        public string CommandName { get; set; }
        public long Id { get; set; }
        public string Title { get; set; }
        public List<MenuInfo> SubMenu { get; set; }
    }
}
