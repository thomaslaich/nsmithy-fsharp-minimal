module Example.Weather.WeatherHandler

open System
open System.Threading.Tasks

/// Hardcoded in-memory implementation of every operation of the Weather service.
let create () =
    { new IWeatherServiceHandler with
        member _.GetCurrentTimeAsync(_, _) =
            Task.FromResult { Time = DateTimeOffset.UtcNow }

        member _.GetCityAsync(input, _) =
            match CityData.cities |> List.tryFind (fun c -> c.CityId = input.CityId) with
            | Some city ->
                Task.FromResult(
                    Ok
                        {
                            Name = city.Name
                            Coordinates =
                                {
                                    Latitude = city.Latitude
                                    Longitude = city.Longitude
                                }
                        }
                )
            | None -> Task.FromResult(Error(GetCityError.NoSuchResource { ResourceType = "City" }))

        member _.ListCitiesAsync(input, _) =
            let pageSize = defaultArg input.PageSize 3

            let start =
                match input.NextToken with
                | Some token ->
                    match CityData.cities |> List.tryFindIndex (fun c -> c.CityId = token) with
                    | Some i -> i + 1
                    | None -> 0
                | None -> 0

            let page = CityData.cities |> List.skip start |> List.truncate pageSize

            let nextToken =
                if start + page.Length < CityData.cities.Length then
                    Some (List.last page).CityId
                else
                    None

            Task.FromResult
                {
                    Items = page |> List.map (fun c -> { CityId = c.CityId; Name = c.Name })
                    NextToken = nextToken
                }

        member _.GetForecastAsync(_, _) =
            Task.FromResult(Ok { ChanceOfRain = Some 0.4f })
    }
