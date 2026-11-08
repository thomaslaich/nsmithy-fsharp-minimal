open System
open System.Threading.Tasks
open Example.Weather
open NSmithy.Client
open NSmithy.Core.Validation

/// Walks every page of ListCities. Paginators are C#-only for now, so follow `nextToken` by hand.
let rec printPages (client: WeatherClient) (page: int) (token: string option) : Task<unit> =
    task {
        let! result = client.ListCitiesAsync { NextToken = token; PageSize = Some 3 }
        printfn "  Page %d:" page

        for city in result.Items do
            printfn "    %s: %s" city.CityId city.Name

        match result.NextToken with
        | Some next -> return! printPages client (page + 1) (Some next)
        | None -> ()
    }

let run (endpoint: Uri) (debug: bool) =
    task {
        use client =
            if debug then
                let config = WeatherClientConfig()
                config.Interceptors.Add(DebugInterceptor())
                new WeatherClient(endpoint, config)
            else
                new WeatherClient(endpoint)

        let! time = client.GetCurrentTimeAsync()
        printfn "Current time: %s" (time.Time.ToString "u")

        printfn "All cities (paginated, page size 3):"
        do! printPages client 1 None

        match! client.GetCityAsync { CityId = "SEA" } with
        | Ok seattle -> printfn "Seattle: (%g, %g)" seattle.Coordinates.Latitude seattle.Coordinates.Longitude
        | Error(GetCityError.NoSuchResource error) -> printfn "No such %s: SEA" error.ResourceType

        let! forecast = client.GetForecastAsync { CityId = "SEA" }

        match forecast.ChanceOfRain with
        | Some chance -> printfn "Forecast for SEA: %.0f%% chance of rain" (chance * 100.0f)
        | None -> printfn "Forecast for SEA: unknown"

        // "Atlantis" satisfies the model, so it reaches the handler and comes back as the error the
        // operation declares: a modeled error, returned as the Error case of the operation's Result.
        match! client.GetCityAsync { CityId = "Atlantis" } with
        | Ok _ -> ()
        | Error(GetCityError.NoSuchResource error) -> printfn "No such %s: Atlantis" error.ResourceType

        // CityId is @pattern("^[A-Za-z0-9 ]+$"), and constraints are enforced by the server — the
        // client sends what it is handed. The rejection comes back as
        // smithy.framework#ValidationException, an implicit modeled error on every operation, so it
        // deserializes into a typed exception naming the member and the constraint it failed.
        try
            let! _ = client.GetCityAsync { CityId = "SEA!" }
            ()
        with :? ValidationException as error ->
            printfn "Rejected \"SEA!\": %s" error.Message

            for field in error.FieldList do
                printfn "  %s: %s" field.Path field.Message
    }

[<EntryPoint>]
let main args =
    let debug = args |> Array.contains "--debug"

    let endpoint =
        args
        |> Array.tryFind (fun a -> not (a.StartsWith "--"))
        |> Option.defaultValue "http://localhost:5000"

    (run (Uri endpoint) debug).GetAwaiter().GetResult()
    0
