<%(TOC)%>

<p align="center"><img src="{{assetRoot}}img/docs-header-diagram.png" /></p>

SpatialOS Online Services provide complementary infrastructure around your game's [SpatialOS](https://docs.improbable.io) game server software and hosting; services such as authentication and matchmaking. Online Services work with SpatialOS game projects created using Unreal Engine with the [GDK for Unreal](https://docs.improbable.io/unreal), or Unity with the [GDK for Unity](https://docs.improbable.io/unity), or [your own engine](https://docs.improbable.io/reference/latest/shared/byoe/introduction).

The Online Services repository provides a suite of example gRPC (with additional HTTP support) services, packages and images. It gives you everything you need to start building online services to support your SpatialOS game. The Services are as unopinionated and generic as possible because you know best what your game requires. The primary language is C#, but we provide our [protocol buffer files](https://developers.google.com/protocol-buffers/) too so you can re-implement the services in whichever language you choose, meaning that you can customize them for your game if you need to. The services support [gRPC](https://grpc.io/) and HTTP.

## Services

**Matchmaking Service - the Gateway**

For matchmaking, you can use the Gateway Service.

To find out about the Gateway, see:

* the [Gateway overview]({{urlRoot}}/content/services-packages/gateway/overview) documentation
* the [services & packages overview]({{urlRoot}}/content/services-packages/overview)

You can also check out the Improbable blogpost on [Matchmaking with SpatialOS](https://improbable.io/blog/matchmaking-with-spatialos); it describes how you can use the Gateway as a matchmaking service.

**Authentication Service - PlayFab Auth**

For authentication, you can use the PlayFab Auth Service.

To find out about PlayFab Auth, see:

* the Gateway [deploy]({{urlRoot}}/content/services-packages/gateway/deploy) & [usage]({{urlRoot}}/content/services-packages/gateway/usage) documentation
* the [services & packages overview]({{urlRoot}}/content/services-packages/overview)

**Deployment Pool**

Maintains game deployments in a ready-to-go state. It is useful if you want players to be able to jump into a game or between levels with minimal wait times.

To find out about the Deployment Pool, see:

* the [overview]({{urlRoot}}/content/services-packages/deployment-pool/overview) documentation
* the [usage overview]({{urlRoot}}/content/services-packages/deployment-pool/deploy)

**Analytics Pipeline**

A REST endpoint to capture analytics events stemming from either Online Services or a game, see:

* the [Analytics Pipeline]({{urlRoot}}/content/services-packages/analytics-pipeline/overview) documentation

## The Online Services repository

The Online Services, packages and configuration examples are all on GitHub.

Repository on GitHub: [github.com/spatialos/online-services](https://github.com/spatialos/online-services)

We recommend you create a fork of the repository so that you can make whatever customizations you want; use this as a base, rather than a comprehensive one-size-fits-all solution.

## Where to start

* Find out what's included in the Online Services repository:

    - services & packages - see documentation [overview]({{urlRoot}}/content/services-packages/overview)
    - configuration examples - see [`SampleClient`](https://github.com/spatialos/online-services/tree/master/services/csharp/SampleClient) and [`SampleMatcher`](https://github.com/spatialos/online-services/tree/master/services/csharp/SampleMatcher).
<br><br>
* Find out more about the Gateway.

    Read the [Gateway guide]({{urlRoot}}/content/services-packages/gateway/overview). This describes how the Gateway system works, and includes best practices for using it with your game.

<%(Nav hide="next")%>
<%(Nav hide="prev")%>

<br/>------------<br/>
_2019-07-16 Page added with editorial review_
[//]: # (TODO: https://improbableio.atlassian.net/browse/DOC-1135)
