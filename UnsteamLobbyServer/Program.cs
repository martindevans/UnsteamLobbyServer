using UnsteamLobbyServer.Lobbies;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<LobbyServer>();

var app = builder.Build();

app.UseWebSockets();

var lobbies = app.Services.GetRequiredService<LobbyServer>();
app.MapGet("/connect", lobbies.Connect);
app.MapGet("/list", async () => await lobbies.List());
app.MapPost("/create", lobbies.Create);
app.MapPost("/join", lobbies.Join);
app.MapGet("/status", () => "ok");

app.Run();
