using Microsoft.EntityFrameworkCore;
using Entities;
using Services;
using ServiceContracts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IUsersService, UsersService>();

builder.Services.AddDbContext<UserDbContext>(options => { options.UseSqlServer(builder.Configuration
	.GetConnectionString("DefaultConnection"));
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddHttpClient();


var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapControllers();

app.Run();
