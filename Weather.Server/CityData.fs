module Example.Weather.CityData

type City =
    {
        CityId: string
        Name: string
        Latitude: float32
        Longitude: float32
    }

let cities =
    [
        {
            CityId = "SEA"
            Name = "Seattle"
            Latitude = 47.6f
            Longitude = -122.3f
        }
        {
            CityId = "NYC"
            Name = "New York"
            Latitude = 40.7f
            Longitude = -74.0f
        }
        {
            CityId = "LAX"
            Name = "Los Angeles"
            Latitude = 34.1f
            Longitude = -118.2f
        }
        {
            CityId = "CHI"
            Name = "Chicago"
            Latitude = 41.9f
            Longitude = -87.6f
        }
        {
            CityId = "HOU"
            Name = "Houston"
            Latitude = 29.8f
            Longitude = -95.4f
        }
        {
            CityId = "PHX"
            Name = "Phoenix"
            Latitude = 33.4f
            Longitude = -112.1f
        }
        {
            CityId = "PHL"
            Name = "Philadelphia"
            Latitude = 39.9f
            Longitude = -75.2f
        }
        {
            CityId = "SAN"
            Name = "San Antonio"
            Latitude = 29.4f
            Longitude = -98.5f
        }
        {
            CityId = "SDA"
            Name = "San Diego"
            Latitude = 32.7f
            Longitude = -117.2f
        }
        {
            CityId = "DAL"
            Name = "Dallas"
            Latitude = 32.8f
            Longitude = -96.8f
        }
    ]
