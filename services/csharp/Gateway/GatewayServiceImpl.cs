﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Google.LongRunning;
using Grpc.Core;
using Improbable.OnlineServices.Common;
using Improbable.OnlineServices.DataModel;
using Improbable.OnlineServices.DataModel.Gateway;
using Improbable.OnlineServices.DataModel.Party;
using Improbable.OnlineServices.Proto.Gateway;
using MemoryStore;
using Serilog;
using JoinRequestProto = Improbable.OnlineServices.Proto.Gateway.JoinRequest;
using PartyDataModel = Improbable.OnlineServices.DataModel.Party.Party;

namespace Gateway
{
    public class GatewayServiceImpl : GatewayService.GatewayServiceBase
    {
        private readonly IMemoryStoreClientManager<IMemoryStoreClient> _memoryStoreClientManager;

        public GatewayServiceImpl(IMemoryStoreClientManager<IMemoryStoreClient> memoryStoreClientManager)
        {
            _memoryStoreClientManager = memoryStoreClientManager;
        }

        public override async Task<Operation> Join(JoinRequestProto request, ServerCallContext context)
        {
            // TODO(dom): refactor this later.
            var playerIdentifier = AuthHeaders.ExtractPlayerId(context);
            using (var memClient = _memoryStoreClientManager.GetClient())
            {
                try
                {
                    var party = await GetPartyForMember(memClient, playerIdentifier) ??
                                throw new EntryNotFoundException(playerIdentifier);

                    if (party.LeaderPlayerId != playerIdentifier)
                    {
                        throw new RpcException(new Status(StatusCode.PermissionDenied,
                            "Only the leader can start matchmaking"));
                    }

                    if (!party.SufficientMembers())
                    {
                        throw new RpcException(new Status(StatusCode.FailedPrecondition,
                            "There are not enough members in the party to start matchmaking"));
                    }

                    party.CurrentPhase = PartyDataModel.Phase.Matchmaking;

                    var matchmakingMetadata = new Dictionary<string, string>(request.Metadata);
                    var partyJoinRequest = new PartyJoinRequest(party, request.MatchmakingType, matchmakingMetadata);
                    var entriesToCreate = new List<Entry> { partyJoinRequest };
                    entriesToCreate.AddRange(CreateJoinRequestsForEachMember(party, request.MatchmakingType,
                        matchmakingMetadata));

                    using (var tx = memClient.CreateTransaction())
                    {
                        tx.UpdateAll(party.Yield());
                        tx.CreateAll(entriesToCreate);
                        tx.EnqueueAll(partyJoinRequest.Yield());
                    }
                }
                catch (EntryNotFoundException)
                {
                    Log.Information("Player is not a member of any party");
                    throw new RpcException(new Status(StatusCode.NotFound, "player is not a member of any party"));
                }
                catch (EntryAlreadyExistsException e)
                {
                    Reporter.JoinRequestQueuedInc();
                    Log.Information($"Party already queued: {e.Id}.");
                    throw new RpcException(new Status(StatusCode.AlreadyExists,
                        "party is already queued for matchmaking"));
                }
                catch (TransactionAbortedException)
                {
                    Reporter.TransactionAbortedInc("Join");
                    Log.Warning("Transaction for join was aborted");
                    throw new RpcException(new Status(StatusCode.Unavailable,
                        "join aborted due to concurrent modification; safe to retry"));
                }
            }

            Reporter.JoinRequestInc();
            Log.Information($"Created party join request for the party associated to player ${playerIdentifier}.");
            return new Operation
            {
                Name = playerIdentifier
            };
        }

        private static async Task<PartyDataModel> GetPartyForMember(IMemoryStoreClient memClient, string playerId)
        {
            var member = await memClient.GetAsync<Member>(playerId);
            if (member == null)
            {
                return null;
            }

            return await memClient.GetAsync<PartyDataModel>(member.PartyId);
        }

        private static IEnumerable<PlayerJoinRequest> CreateJoinRequestsForEachMember(PartyDataModel party,
            string matchmakingType, Dictionary<string, string> matchmakingMetadata)
        {
            var entriesToCreate = new List<PlayerJoinRequest>();
            foreach (var (memberId, memberPit) in party.MemberIdToPit)
            {
                entriesToCreate.Add(new PlayerJoinRequest(memberId, memberPit, matchmakingType, matchmakingMetadata));
            }

            return entriesToCreate;
        }
    }
}
