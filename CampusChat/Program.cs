using CampusChat.Components;
using CampusChat.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<UserStateService>();

builder.Services.AddSingleton<CampusChatStore>();
builder.Services.AddSingleton<ChangeNotifier>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ThreadService>();
builder.Services.AddSingleton<PostService>();
builder.Services.AddSingleton<DirectChatService>();
builder.Services.AddSingleton<UnreadService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();