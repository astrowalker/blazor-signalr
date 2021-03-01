using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultiSignalBlazorApp50.Server
{
    public sealed class FeedbackHub : Hub
    {
        private readonly Timer timer;
        private HashSet<string> connections;

        public FeedbackHub()
        {
            this.connections = new HashSet<string>();
            Console.WriteLine($"{this} created");
            this.timer = new Timer(TimerCallback1);
            this.timer.Change(TimeSpan.FromSeconds(3), System.Threading.Timeout.InfiniteTimeSpan);
        }

        public override Task OnConnectedAsync()
        {
            lock (this.connections)
            {
                if (!connections.Add(Context.ConnectionId))
                    Console.WriteLine("Connected with same connection");
                else
                    Console.WriteLine("New hub connection");
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            lock (this.connections)
            {
                this.connections.Remove(Context.ConnectionId);
                Console.WriteLine("Disconnected");
            }
            return base.OnDisconnectedAsync(exception);
        }
        public async void TimerCallback1(object state)
        {
            if (DateTimeOffset.UtcNow.Second % 2 == 0)
                await SendInfoAsync("channelA", 1).ConfigureAwait(false);
            else
                await SendInfoAsync("channelB", 777).ConfigureAwait(false);

            this.timer.Change(TimeSpan.FromSeconds(3), System.Threading.Timeout.InfiniteTimeSpan);
        }

        public async Task SendInfoAsync(string channel, int value)
        {
            IHubCallerClients clients = this.Clients;
            if (clients == null)
                Console.WriteLine("no clients yet");
            else
            {
                string[] conn;
                lock (this.connections)
                    conn = connections.ToArray();

                if (conn != null)
                {
                    await clients.Clients(conn).SendAsync(channel, value);
                    Console.WriteLine($"Sent {value} via {channel} to {conn.Length}");
                }
                else
                {
                    Console.WriteLine("Skipping sending, connection null");
                }
            }
        }

    }
}
