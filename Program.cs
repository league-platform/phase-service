using System;
using Microsoft.AspNetCore.Builder;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger(); 
app.UseSwaggerUI();

app.MapPost("/phases", async ([FromBody] Phase phase, IDynamoDBContext db) =>
{
    phase.CreatedAt = DateTime.UtcNow;
    await db.SaveAsync(phase);
    Console.WriteLine($"EVENT: phase.created -> {phase.Name}");
    return Results.Created($"/phases/{phase.Id}", phase);
});

app.MapGet("/phases", async (IDynamoDBContext db) =>
{
    var conditions = new List<ScanCondition>();
    var results = await db.ScanAsync<Phase>(conditions).GetRemainingAsync();
    return Results.Ok(results);
});

app.Run();

[DynamoDBTable("Phases")]
public class Phase
{
    [DynamoDBHashKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [DynamoDBProperty]
    public string Name { get; set; }

    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; }
}
