using SpotifyPlaylistGeneratorV1.Authentication;
using SpotifyPlaylistGeneratorV1.Services;
using SpotifyPlaylistGeneratorV1.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using SpotifyPlaylistGeneratorV1.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Logging.AddSimpleConsole(opt => opt.TimestampFormat = "[yyyy/MM/dd HH:mm:ss] ");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("default")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt => {
    opt.User.RequireUniqueEmail = true;
    opt.SignIn.RequireConfirmedEmail = false;
    opt.SignIn.RequireConfirmedAccount = false;
    opt.SignIn.RequireConfirmedPhoneNumber = false;
}).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton<IStringEncryptionService, StringEncryptionService>();
builder.Services.AddScoped<ISpotifyUserRepository, SpotifyUserRepository>();
builder.Services.AddScoped<ISpotify, SpotifyV1>();
builder.Services.AddScoped<IYoutube, YoutubeV1>();
builder.Services.AddSingleton<IPlaylistCreateQueue>(ctx => {
    if (!int.TryParse(builder.Configuration["ServiceQueueCapacity"], out int capacity))
    {
        capacity = 100;     //If no capacity set -> limit to 100
    }
    return new PlaylistCreateQueue(capacity);
});

builder.Services.AddHostedService<CreatePlaylistService>();

//Force auth on all controllers
builder.Services.AddMvc(options =>
{
    var policy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

options.Filters.Add(new AuthorizeFilter(policy));
});


//Configure redirect if not authed
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.Cookie.HttpOnly = true;
    opt.LoginPath = "/Auth/Login";

});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
