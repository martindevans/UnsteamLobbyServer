using UnsteamLobbyServer.Lobbies;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<LobbyServer>();

var app = builder.Build();

app.UseWebSockets();

var lobbies = app.Services.GetRequiredService<LobbyServer>();
app.MapGet("/connect", lobbies.Connect);

app.Run();
