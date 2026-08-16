using System.Security.Claims;
using Kanawanagasaki.BlazorContracts.Demo.Components;
using Kanawanagasaki.BlazorContracts.Demo.Stores;
using Kanawanagasaki.BlazorContracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSingleton<AppStore>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth";
        options.LogoutPath = "/auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("MySuperPolicy", policy => policy.RequireRole("Admin"));

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddContracts();

var app = builder.Build();

app.Services.GetRequiredService<AppStore>().Seed();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Kanawanagasaki.BlazorContracts.Demo.Client._Imports).Assembly);

app.MapContracts();

app.MapPost("/api/auth/login", async (HttpContext ctx) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();
    var returnTo = SanitizeReturnTo(form["returnTo"].ToString());

    var role = (username, password) switch
    {
        ("admin", "adminpass") => "Admin",
        ("user",  "userpass")  => "User",
        _ => null,
    };
    if (role is null)
        return Results.Redirect($"/auth?error={Uri.EscapeDataString("Invalid username or password.")}");

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, username),
        new(ClaimTypes.Role, role),
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    return Results.Redirect(returnTo);
}).DisableAntiforgery();

app.MapPost("/api/auth/logout", async (HttpContext ctx) =>
{
    var returnTo = "/auth";
    if (ctx.Request.HasFormContentType)
    {
        var form = await ctx.Request.ReadFormAsync();
        returnTo = SanitizeReturnTo(form["returnTo"].ToString());
    }

    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect(returnTo);
}).DisableAntiforgery();

static string SanitizeReturnTo(string? returnTo)
{
    if (string.IsNullOrEmpty(returnTo))
        return "/auth";
    if (Uri.IsWellFormedUriString(returnTo, UriKind.Absolute))
        return "/auth";
    if (!returnTo.StartsWith('/'))
        return "/" + returnTo;
    return returnTo;
}

app.Run();
