var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true)));

// TODO: Add JwtBearer auth + EF Core DbContext here

var app = builder.Build();
app.UseSwagger(); app.UseSwaggerUI();
app.UseCors();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/test/detect", async (IConfiguration cfg) =>
{
    var channel = Grpc.Net.Client.GrpcChannel.ForAddress(cfg["AI_GRPC_URL"] ?? "http://localhost:50051");
    var client = new Inference.InferenceClient(channel);
    var resp = await client.DetectObjectsAsync(new DetectRequest { ImageUrl = "http://example.com/x.jpg" });
    return Results.Ok(resp.Detections);
});

app.MapHub<AlertsHub>("/hub/alerts"); // SignalR
app.Run();

public class AlertsHub : Microsoft.AspNetCore.SignalR.Hub { }
