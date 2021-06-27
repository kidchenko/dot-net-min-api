using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
await using var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapGet("/", (Func<string>)(() => "Hello World!"));

app.MapGet("/ping", async (http) =>
{
    await http.Response.WriteAsync("pong");
});

app.MapGet("/json", async (http) =>
{
    await http.Response.WriteAsJsonAsync("pong");
});

await app.RunAsync();
