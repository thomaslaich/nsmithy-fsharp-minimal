open System
open System.Threading.Tasks
open Example.Weather
open FSharp.Control
open NSmithy.Client

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

        // The generated paginator requests page after page, following `nextToken`, and yields each item.
        printfn "All cities (page size 3):"

        do!
            client.ListCitiesItemsAsync { NextToken = None; PageSize = Some 3 }
            |> TaskSeq.iter (fun city -> printfn "  %s: %s" city.CityId city.Name)

        match! client.GetCityAsync { CityId = "SEA" } with
        | Ok seattle -> printfn "Seattle: (%g, %g)" seattle.Coordinates.Latitude seattle.Coordinates.Longitude
        | Error(GetCityError.NoSuchResource error) -> printfn "No such %s: SEA" error.ResourceType
        | Error(GetCityError.ValidationException error) -> printfn "Rejected \"SEA\": %s" error.Message

        match! client.GetForecastAsync { CityId = "SEA" } with
        | Ok { ChanceOfRain = Some chance } -> printfn "Forecast for SEA: %.0f%% chance of rain" (chance * 100.0f)
        | Ok _ -> printfn "Forecast for SEA: unknown"
        | Error(GetForecastError.ValidationException error) -> printfn "Rejected \"SEA\": %s" error.Message

        // "Atlantis" satisfies the model, so it reaches the handler and comes back as the error the
        // operation declares: a modeled error, returned as the Error case of the operation's Result.
        match! client.GetCityAsync { CityId = "Atlantis" } with
        | Ok _ -> ()
        | Error(GetCityError.NoSuchResource error) -> printfn "No such %s: Atlantis" error.ResourceType
        | Error(GetCityError.ValidationException error) -> printfn "Rejected \"Atlantis\": %s" error.Message

        // CityId is @pattern("^[A-Za-z0-9 ]+$"), and constraints are enforced by the server: the
        // client sends what it is handed. The rejection comes back as
        // smithy.framework#ValidationException, which the model declares on GetCity, so it is a
        // case of the operation's Result naming the member and the constraint it failed.
        match! client.GetCityAsync { CityId = "SEA!" } with
        | Ok _ -> ()
        | Error(GetCityError.NoSuchResource error) -> printfn "No such %s: SEA!" error.ResourceType
        | Error(GetCityError.ValidationException error) ->
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
