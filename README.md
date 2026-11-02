# NSmithy Minimal F# Example

A minimal F# example of [NSmithy](https://github.com/thomaslaich/smithy-dotnet) — a .NET implementation of the [Smithy](https://smithy.io) IDL. It is the F# twin of [nsmithy-minimal](../nsmithy-minimal), which does the same in C#.

NSmithy notices that a project is an `.fsproj` and generates F#: records for structures, discriminated unions for unions and enums, `option` for members that may be absent, and exception types for `@error` shapes. See the [F# guide](https://thomaslaich.github.io/smithy-dotnet/guides/fsharp/).

The service is the Weather example from the [Smithy quickstart](https://smithy.io/2.0/quickstart.html): a REST API that exposes city resources with paginated listing and per-city weather forecasts.

## Projects

- **Weather.Contracts** — Smithy model (`model/weather.smithy`). Builds the contract and publishes the model to the server and client, which each generate their own F# types from it.
- **Weather.Server** — ASP.NET Core server; `WeatherHandler.fs` implements the generated `IWeatherServiceHandler` against hardcoded in-memory data.
- **Weather.Client** — Console app that exercises the API end-to-end. Paginators are C#-only for now, so it follows `nextToken` with a small recursive function.

## Prerequisites

Requires the [.NET SDK](https://dotnet.microsoft.com/download) (10.0+), the [Smithy CLI](https://smithy.io/2.0/guides/smithy-cli/cli_installation.html) (1.74.1+, on `PATH`) for code generation, and Python (for Smithy doc generation).

Alternatively, use [pixi](https://pixi.sh) to drop into a dev shell that provides all three (on macOS, Linux and Windows):

```sh
pixi shell
```

If you'd rather not install the Smithy CLI, reference the [`NSmithy.SmithyCli`](https://www.nuget.org/packages/NSmithy.SmithyCli) package instead, which bundles it with its Java runtime — add this to `Directory.Packages.props`:

```xml
<ItemGroup>
  <GlobalPackageReference Include="NSmithy.SmithyCli" Version="1.74.1" />
</ItemGroup>
```

## Running

[just](https://github.com/casey/just) is available in the pixi shell:

```sh
just server         # start the server
just client         # run the client against http://localhost:5000
just client-debug   # same, with wire-level request/response logging
```

Or directly with the .NET CLI:

```sh
dotnet run --project Weather.Server
dotnet run --project Weather.Client
```

The client connects to `http://localhost:5000` by default. On macOS, port 5000 is often taken by the AirPlay Receiver; either turn it off or run the server elsewhere, e.g. `ASPNETCORE_URLS=http://localhost:5080 just server` and `just client http://localhost:5080`. Pass a different URL as an argument if needed:

```sh
just client http://localhost:8080
# or
dotnet run --project Weather.Client -- http://localhost:8080
```

Pass `--debug` to log requests, responses, and wire bytes (this is what `just client-debug` does):

```sh
dotnet run --project Weather.Client -- --debug
```

## Rejecting a bad request

The server enforces the model at the boundary, so a request that violates it is answered with a structured 4xx rather than reaching the handler. Nothing in the handlers implements these checks — they are generated from the model.

`CityId` is declared `@pattern("^[A-Za-z0-9 ]+$")`, so a city ID carrying a character the pattern excludes is rejected with `ValidationException`, which names the member and the constraint it failed:

```sh
curl -i 'http://localhost:5000/cities/SEA%21'   # "SEA!"
```

```
HTTP/1.1 400 Bad Request
X-Amzn-Errortype: ValidationException

{"message":"1 validation error detected. Value at '/cityId' failed to satisfy
constraint: Member must satisfy regular expression pattern: ^[A-Za-z0-9 ]+$",
"fieldList":[{"path":"/cityId","message":"..."}]}
```

Input that cannot become modeled input at all fails earlier, during deserialization, and comes back as `SerializationException` — here `pageSize` is modeled as `Integer`:

```sh
curl -i 'http://localhost:5000/cities?pageSize=abc'
```

```
HTTP/1.1 400 Bad Request
X-Amzn-Errortype: SerializationException

{"message":"Value 'abc' is not a valid integer."}
```

An `Accept` header that excludes the response's media type is answered with 406:

```sh
curl -i -H 'Accept: application/xml' http://localhost:5000/cities/SEA
```

```
HTTP/1.1 406 Not Acceptable
X-Amzn-Errortype: NotAcceptableException

{"message":"Response is 'application/json', which Accept 'application/xml' excludes."}
```

A request that satisfies the model but names something absent still reaches the handler and returns the error the operation models — a space is inside `CityId`'s pattern, so this is a `NoSuchResource`, not a validation failure:

```sh
curl -i 'http://localhost:5000/cities/%20'
```

```
HTTP/1.1 400 Bad Request
X-Amzn-Errortype: NoSuchResource

{"resourceType":"City"}
```

The client exercises both of these — a modeled `NoSuchResource` and an implicit `ValidationException` — as typed exceptions (`with :? NoSuchResource as error -> ...`) rather than bare status codes.

Once the server is running, the following endpoints are also available:

- **Scalar API explorer** — `http://localhost:5000/openapi`
- **Smithy docs** — `http://localhost:5000/docs`

## Formatting

F# code is formatted with [Fantomas](https://fsprojects.github.io/fantomas/) and the MSBuild/XML files (`.fsproj`, `.props`, `.slnx`) with [CSharpier](https://csharpier.com), both pinned in `dotnet-tools.json`. `.csharpierrc.yaml` maps `*.fsproj` to CSharpier's XML formatter, which it does not pick up by default:

```sh
dotnet tool restore
just fmt         # format
just fmt-check   # fail if anything is unformatted
```

## A note on FSharp.Core

The repo uses central package management, so `Directory.Build.props` turns off the SDK's implicit `FSharp.Core` reference and references it explicitly, with the version pinned in `Directory.Packages.props`. Without this, `FSharp.Core.dll` is not copied to the output and the apps fail at startup.
