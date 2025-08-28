using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Core.Application.Interfaces;
using RpgRooms.Core.Application.Services;
using RpgRooms.Core.Security;
using RpgRooms.Infrastructure.Data;
using RpgRooms.Web.Authorization;
using RpgRooms.Web.Endpoints;
using RpgRooms.Web.Hubs;
using RpgRooms.Web.Data;

var builder = WebApplication.CreateBuilder(args);
var cs = builder.Configuration.GetConnectionString("Default") ?? "Data Source=rpgrooms.db";

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(cs));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
#if DEBUG
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;
#endif
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultUI();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();

builder.Services.AddScoped<ICampaignService, CampaignService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.IsGmOfCampaign, policy => policy.Requirements.Add(new IsGmOfCampaignRequirement()));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAuthorizationHandler, IsGmOfCampaignHandler>();

// HttpClient com BaseAddress do request atual (para usar URLs relativas em componentes)
builder.Services.AddScoped(sp =>
{
    var accessor = sp.GetRequiredService<IHttpContextAccessor>();
    var req = accessor.HttpContext?.Request;
    var baseUri = req != null ? $"{req.Scheme}://{req.Host}{req.PathBase}/" : "http://localhost/";
    return new HttpClient { BaseAddress = new Uri(baseUri) };
});

builder.Services.AddScoped<IdentitySeeder>();

var app = builder.Build();

// Criar DB (dev) rapidamente
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapHub<CampaignChatHub>("/hubs/campaign-chat");

// Minimal APIs
app.MapCampaignEndpoints();

app.MapFallbackToPage("/_Host");

app.Run();
