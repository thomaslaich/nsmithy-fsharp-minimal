open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open NSmithy.Server.AspNetCore
open NSmithy.Server.AspNetCore.Docs
open Example.Weather

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    builder.Services.AddSmithyServer() |> ignore
    builder.Services.AddWeatherServiceHandler<WeatherHandler>() |> ignore

    let app = builder.Build()
    app.MapSmithyOpenApi() |> ignore
    app.MapSmithyDocs() |> ignore
    app.MapWeatherService() |> ignore
    app.Run()
    0
