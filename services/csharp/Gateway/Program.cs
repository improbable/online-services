﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Mono.Unix;
using Mono.Unix.Native;
using Google.LongRunning;
using Improbable.OnlineServices.Base.Server;
using Improbable.OnlineServices.Common.Interceptors;
using Improbable.OnlineServices.Proto.Gateway;
using Improbable.SpatialOS.Platform.Common;
using Improbable.SpatialOS.PlayerAuth.V2Alpha1;
using MemoryStore.Redis;
using Serilog;
using Serilog.Formatting.Compact;

namespace Gateway
{
    class GatewayArgs : CommandLineArgs
    {
        [Option("redis_connection_string", HelpText = "Redis connection string.", Default = "localhost:6379")]
        public string RedisConnectionString { get; set; }
    }

    class Program
    {
        private const string RedisEnvironmentVariable = "REDIS_CONNECTION_STRING";
        private const string SpatialRefreshTokenEnvironmentVariable = "SPATIAL_REFRESH_TOKEN";

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .Enrich.FromLogContext()
                .CreateLogger();

            // Required to have enough I/O threads to handle Redis+gRPC traffic
            // See https://support.microsoft.com/en-gb/help/821268/contention-poor-performance-and-deadlocks-when-you-make-calls-to-web-s
            ThreadPool.SetMaxThreads(100, 100);
            ThreadPool.SetMinThreads(50, 50);


            Parser.Default.ParseArguments<GatewayArgs>(args)
                .WithParsed(parsedArgs =>
                {
                    //TODO(dom): remove in favour of just passing in via the cmd
                    var memoryStoreClientManager =
                        new RedisClientManager(
                            Environment.GetEnvironmentVariable(RedisEnvironmentVariable) ??
                            parsedArgs.RedisConnectionString);

                    //TODO(dom): remove in favour of just passing in via the cmd
                    var spatialRefreshToken =
                        Environment.GetEnvironmentVariable(SpatialRefreshTokenEnvironmentVariable) ??
                        throw new Exception("SPATIAL_REFRESH_TOKEN environment variable must be set.");
                    var playerAuthClient =
                        PlayerAuthServiceClient.Create(
                            credentials: new PlatformRefreshTokenCredential(spatialRefreshToken));

                    var server = GrpcBaseServer.Build(parsedArgs);
                    server.AddInterceptor(new PlayerIdentityTokenValidatingInterceptor(
                        playerAuthClient,
                        memoryStoreClientManager.GetRawClient(Database.CACHE)));
                    server.AddService(
                        GatewayService.BindService(new GatewayServiceImpl(memoryStoreClientManager)));
                    server.AddService(
                        Operations.BindService(new OperationsServiceImpl(memoryStoreClientManager, playerAuthClient)));

                    var serverTask = Task.Run(() => server.Start());
                    var signalTask = Task.Run(() => UnixSignal.WaitAny(new[] { new UnixSignal(Signum.SIGINT), new UnixSignal(Signum.SIGTERM) }));
                    Task.WaitAny(serverTask, signalTask);

                    if (signalTask.IsCompleted)
                    {
                        Log.Information($"Received UNIX signal {signalTask.Result}");
                        Log.Information("Server shutting down...");
                        server.Shutdown();
                        serverTask.Wait();
                        Log.Information("Server stopped cleanly");
                    }
                    else
                    {
                        /* The server task has completed; we can just exit. */
                        Log.Information("The Gateway server has stopped itself or encountered an unhandled exception.");
                    }
                });
        }
    }
}