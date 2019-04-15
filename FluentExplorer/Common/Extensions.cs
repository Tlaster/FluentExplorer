﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentExplorer.Common
{
    internal static class Extensions
    {
        public static string TrimStart(this string target, string trimString)
        {
            if (string.IsNullOrEmpty(trimString)) return target;

            string result = target;
            while (result.StartsWith(trimString))
            {
                result = result.Substring(trimString.Length);
            }

            return result;
        }
    }
}
