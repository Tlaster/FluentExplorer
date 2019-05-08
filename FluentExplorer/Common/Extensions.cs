using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace FluentExplorer.Common
{
    internal static class Extensions
    {
        
        public static void AddAll(this IList list, int startFrom, IEnumerable? secondList)
        {
            if (secondList == null)
            {
                return;
            }
            foreach (var item in secondList)
            {
                list.Insert(startFrom, item);
                startFrom++;
            }
        }

        public static void RemoveAll(this IList list, int startFrom, IEnumerable? secondList)
        {
            if (secondList == null)
            {
                return;
            }
            foreach (var item in secondList)
            {
                list.RemoveAt(startFrom);
                startFrom++;
            }
        }

        
        public static void AddAll(this IList list, IEnumerable? secondList)
        {
            if (secondList == null)
            {
                return;
            }
            foreach (var item in secondList)
            {
                list.Add(item);
            }
        }

        public static void RemoveAll(this IList list, IEnumerable? secondList)
        {
            if (secondList == null)
            {
                return;
            }
            foreach (var item in secondList)
            {
                list.Remove(item);
            }
        }

        public static void AddAll<T>(this IList<T> list, IEnumerable<T>? secondList)
        {
            if (secondList == null)
            {
                return;
            }
            foreach (var item in secondList)
            {
                list.Add(item);
            }
        }

        public static void RemoveAll<T>(this IList<T> list, IEnumerable<T>? secondList)
        {
            if (secondList == null)
            {
                return;
            }
            foreach (var item in secondList)
            {
                list.Remove(item);
            }
        }

        public static bool IsVisible(this UIElement element)
        {
            return element.Visibility == Visibility.Visible;
        }

        public static void FireAndForget<T>(this IAsyncOperation<T> task)
        {

        }

        public static void FireAndForget(this Task task)
        {

        }

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

        // Kotlin: fun <T, R> T.let(block: (T) -> R): R
        public static R Let<T, R>(this T self, Func<T, R> block)
        {
            return block(self);
        }

        // Kotlin: fun <T> T.also(block: (T) -> Unit): T
        public static T Also<T>(this T self, Action<T> block)
        {
            block(self);
            return self;
        }
    }
}
