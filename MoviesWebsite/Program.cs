using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MoviesWebsite.Data;
using MoviesWebsite.Models;
using MoviesWebsite.Models.Repository;
using MoviesWebsite.Models.Repository.Interface;
using MoviesWebsite.Services;
using MVC.ExtendNethods;
using MVC.Menu;
using MVC.Services;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

builder.Services.AddOptions();

// Excel
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// DBcontext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DBContext") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    options.UseSqlServer(connectionString);
});
builder.Services.AddCloudinary(builder.Configuration);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Repository
builder.Services.AddScoped<IActorRepository, ActorRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IEpisodeRepository, EpisodeRepository>();
builder.Services.AddScoped<IEvaluateRepository, EvaluateRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();

// Add send mail services
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddSingleton<IEmailSender, SendMailService>();

// Indentity
builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Sidebar
builder.Services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddTransient<AdminSidebarService>();


// IdentityOptions
builder.Services.Configure<IdentityOptions>(options =>
{
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 6; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lầ thì khóa
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;  // Email là duy nhất


    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
    options.SignIn.RequireConfirmedAccount = true;

});

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    // Thay đổi thời gian kiểm tra lại mã bảo mật
    // Mặc định là 30 p
    options.ValidationInterval = TimeSpan.Zero;
});


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login/";
    options.LogoutPath = "/logout/";
    options.AccessDeniedPath = "/AccessDenied/";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ViewManageMenu", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser();
        policyBuilder.RequireRole(RoleName.Administrator, RoleName.Editor);
    });

    options.AddPolicy("Edit", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser();
        policyBuilder.RequireRole(RoleName.Administrator,RoleName.Editor);
    });

    options.AddPolicy("Vip", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser();
        policyBuilder.RequireRole(RoleName.Administrator, RoleName.Editor, RoleName.Premium);
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline .
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// create folder upload
if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Uploads")))
{
    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Uploads"));
}

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Uploads")),
    RequestPath = "/contents"
});

// Custom response error 400 -500
app.AddStatusCodePages();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
      name: "admin",
      areaName: "Admin",
      pattern: "/{areas=admin}/{controller=dashboard}/{action=index}/{id?}");


app.MapRazorPages();

app.Run();
