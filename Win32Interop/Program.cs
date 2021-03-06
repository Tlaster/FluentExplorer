﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Win32Interop.Handlers;

//AppService

namespace Win32Interop
{
    internal class Program
    {
        private static readonly List<AbsHandler> _handlers = new List<AbsHandler>
        {
            new ContextMenuHandler()
        };

        private static AppServiceConnection _connection;
        private static readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        private static async Task Main(string[] args)
        {
            new Thread(ThreadProc).Start();
            Console.Title = "FluentExplorer Win32Interop";
            Console.WriteLine(
                "This process runs at the full privileges of the user and has access to the entire public desktop API surface");
            _manualResetEvent.WaitOne();
        }

        private static async void ThreadProc()
        {
            Console.WriteLine(DateTime.Now.TimeOfDay + "\tThreadProc Start!!");

            _connection = new AppServiceConnection
            {
                AppServiceName = "CommunicationService", PackageFamilyName = Package.Current.Id.FamilyName
            };
            _connection.RequestReceived += Connection_RequestReceived;

            var status = await _connection.OpenAsync();
            switch (status)
            {
                case AppServiceConnectionStatus.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Connection established - waiting for requests");
                    Console.WriteLine();
                    break;
                case AppServiceConnectionStatus.AppNotInstalled:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The app AppServicesProvider is not installed.");
                    return;
                case AppServiceConnectionStatus.AppUnavailable:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The app AppServicesProvider is not available.");
                    return;
                case AppServiceConnectionStatus.AppServiceUnavailable:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(
                        $"The app AppServicesProvider is installed but it does not provide the app service {_connection.AppServiceName}.");
                    return;
                case AppServiceConnectionStatus.Unknown:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(
                        "An unknown error occurred while we were trying to open an AppServiceConnection.");
                    return;
            }
        }

        private static async void Connection_RequestReceived(AppServiceConnection sender,
            AppServiceRequestReceivedEventArgs args)
        {
            Console.WriteLine(DateTime.Now.TimeOfDay + "\tGet message from UWP!");
            var type = args.Request.Message["type"] as string;
            if (type == "Close")
            {
                _manualResetEvent.Set();
                return;
            }

            var data = args.Request.Message["data"] as string;
            var def = args.GetDeferral();
            var handler = _handlers.FirstOrDefault(it => it.Type == type);
            var result = string.Empty;
            if (handler != null) result = await _handlers.FirstOrDefault(it => it.Type == type).Handle(data);
            def.Complete();
            var valueSet = new ValueSet {{"type", type}, {"data", result}};
            //Send back message to UWP
            args.Request.SendResponseAsync(valueSet).Completed += delegate { };
            Console.WriteLine(DateTime.Now.TimeOfDay + "\tMessage to UWP has been sent!!");
        }
    }
}