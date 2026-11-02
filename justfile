server:
    dotnet run --project Weather.Server

client *args:
    dotnet run --project Weather.Client -- {{args}}

client-debug *args:
    dotnet run --project Weather.Client -- --debug {{args}}

fmt:
    dotnet fantomas Weather.Server Weather.Client
    dotnet csharpier format .

fmt-check:
    dotnet fantomas --check Weather.Server Weather.Client
    dotnet csharpier check .
