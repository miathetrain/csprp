using System;
using DiscordRPC;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DiscordPresence
{
    class Program
    {
        public DiscordRpcClient client;
        public CancellationTokenSource tokenSource;
        private static Program instance = new Program();

        static void Main(string[] args)
        {
            var art = @" ________  ________  ________  ________  ________   
|\   ____\|\   ____\|\   __  \|\   __  \|\   __  \  
\ \  \___|\ \  \___|\ \  \|\  \ \  \|\  \ \  \|\  \ 
 \ \  \    \ \_____  \ \   ____\ \   _  _\ \   ____\
  \ \  \____\|____|\  \ \  \___|\ \  \\  \\ \  \___|
   \ \_______\____\_\  \ \__\    \ \__\\ _\\ \__\   
    \|_______|\_________\|__|     \|__|\|__|\|__|   
             \|_________|                           ";
            Console.WriteLine(art);
            Console.WriteLine("This is a tool for running a CSP Discord Presence!");
            Console.WriteLine("Run this instead of CLIP STUDIO PAINT.");
            Process.Start("C:\\Program Files\\CELSYS\\CLIP STUDIO 1.5\\CLIP STUDIO PAINT\\CLIPStudioPaint.exe");


            Instance.Initialize();
            Instance.startThread();
              Console.ReadLine();
            Process.GetCurrentProcess().WaitForExit();
        }

        public void startThread()
        {
            tokenSource = new CancellationTokenSource();
            var task = Repeat.Interval(
                    TimeSpan.FromSeconds(10),
                    () => checkProcess(), tokenSource.Token);
        }

        public void Initialize()
        {
            client = new DiscordRpcClient("928158606313000961");

            client.OnReady += (sender, e) =>
            {
                Console.WriteLine("Received Ready from user {0}", e.User.Username);
            };

            client.OnPresenceUpdate += (sender, e) =>
            {
                Console.WriteLine("Received Update! {0}", e.Presence);
            };

            //Connect to the RPC
            client.Initialize();

            //Set the rich presence
            //Call this as many times as you want and anywhere in your code.
            client.SetPresence(new RichPresence()
            {
                Details = "Drawing",
                State = "",
                Timestamps = Timestamps.Now,
                Buttons = new Button[]
                        {
                    new Button() { Label = "CSP", Url = "https://www.clipstudio.net/en/" }
                        },

                Assets = new Assets()
                {
                    LargeImageKey = "paint-new",
                    LargeImageText = "Created by Mia!",
                    SmallImageKey = ""
                }
            });
        }

        public static void checkProcess()
        {
            Process[] pname = Process.GetProcessesByName("CLIPStudioPaint");
            if (pname.Length == 0)
            {
                Console.WriteLine("CLIPStudioPaint.exe process cannot be found.");
                disable();
            }
            else
            {
                Console.WriteLine("CLIPStudioPaint.exe process is still running.");
            }
        }

        public static Program Instance
        {
            get { return instance; }
        }


        public static void disable()
        {
            Console.WriteLine("Stopped CLIPStudioPaint.exe process checker.");
            Instance.tokenSource.Cancel();
            Console.WriteLine("Stopped DiscordPresence.");
            Instance.client.Dispose();
            Environment.Exit(0);
        }
    }

}
internal static class Repeat
{
    public static Task Interval(
        TimeSpan pollInterval,
        Action action,
        CancellationToken token)
    {
        // We don't use Observable.Interval:
        // If we block, the values start bunching up behind each other.
        return Task.Factory.StartNew(
            () =>
            {
                for (; ; )
                {
                    if (token.WaitCancellationRequested(pollInterval))
                        break;

                    action();
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }
}

static class CancellationTokenExtensions
{
    public static bool WaitCancellationRequested(
        this CancellationToken token,
        TimeSpan timeout)
    {
        return token.WaitHandle.WaitOne(timeout);
    }
}
