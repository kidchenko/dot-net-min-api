using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDbContext>(options => options.UseInMemoryDatabase("TodoItems"));
await using var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapGet("/", (Func<string>)(() => "Hello World!"));

app.MapGet("/todos", async (http) =>
{
    var dbContext = http.RequestServices.GetRequiredService<TodoDbContext>();
    var todos = await dbContext.TodoItems.ToListAsync();

    await http.Response.WriteAsJsonAsync(todos);
});

app.MapGet("/todos/{id}", async (http) =>
{
    if (!http.Request.RouteValues.TryGetValue("id", out var id))
    {
        http.Response.StatusCode = 400;
        return;
    }

    var dbContext = http.RequestServices.GetRequiredService<TodoDbContext>();
    var todo = await dbContext.TodoItems.FindAsync(int.Parse(id.ToString()));
    if (todo == null)
    {
        http.Response.StatusCode = 404;
        return;
    }

    await http.Response.WriteAsJsonAsync(todo);
});

app.MapPost("/todos", async (http) =>
{
    var todo = await http.Request.ReadFromJsonAsync<TodoItem>();
    var dbContext = http.RequestServices.GetRequiredService<TodoDbContext>();

    dbContext.TodoItems.Add(todo);

    await dbContext.SaveChangesAsync();

    http.Response.StatusCode = 201;
});

app.MapPut("/todos/{id}", async (http) =>
{
    if (!http.Request.RouteValues.TryGetValue("id", out var id))
    {
        http.Response.StatusCode = 400;
        return;
    }

    var dbContext = http.RequestServices.GetRequiredService<TodoDbContext>();
    var todo = await dbContext.TodoItems.FindAsync(int.Parse(id.ToString()));
    if (todo == null)
    {
        http.Response.StatusCode = 404;
        return;
    }

    var todoRequest = await http.Request.ReadFromJsonAsync<TodoItem>();
    todo.IsCompleted = todoRequest.IsCompleted;
    await dbContext.SaveChangesAsync();
    http.Response.StatusCode = 204;
});

app.MapDelete("/todos/{id}", async (http) =>
{
    if (!http.Request.RouteValues.TryGetValue("id", out var id))
    {
        http.Response.StatusCode = 400;
        return;
    }

    var dbContext = http.RequestServices.GetRequiredService<TodoDbContext>();
    var todo = await dbContext.TodoItems.FindAsync(int.Parse(id.ToString()));
    if (todo == null)
    {
        http.Response.StatusCode = 404;
        return;
    }

    dbContext.TodoItems.Remove(todo);
    await dbContext.SaveChangesAsync();

    http.Response.StatusCode = 204;
});

await app.RunAsync();
