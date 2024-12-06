using ApiGateway.Services;
using ApiGateway.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ApiGateway.Middleware;
using Elastic.Clients.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình ElasticsearchClient
builder.Services.AddSingleton<ElasticsearchClient>(sp =>
{
    var settings = new ElasticsearchClientSettings(new Uri("http://8.210.61.240:9200/"))
        .DefaultIndex("application-logs-emails-api-development-*"); // Thay đổi theo tên index của bạn

    var client = new ElasticsearchClient(settings);
    return client;
});

// Đọc cài đặt MongoDB từ appsettings.json
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

// Đăng ký MongoClient vào DI Container
builder.Services.AddSingleton<IMongoClient>(s =>
{
    var settings = s.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});


// ...
builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<ApiKeyService>();

builder.Services.AddScoped<ReportService>();

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Add controllers with views
builder.Services.AddControllersWithViews();

// Đăng ký các dịch vụ (UserService và DatabaseInitializer)
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<DatabaseInitializer>();

var app = builder.Build();

// Khởi chạy tác vụ nền để khởi tạo cơ sở dữ liệu sau khi ứng dụng bắt đầu chạy
Task.Run(async () =>
{
    using (var scope = app.Services.CreateScope())
    {
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeDatabaseAsync();
    }
});

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

// Add the API Key Authentication Middleware
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

// Cấu hình middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Trang lỗi cho Exception
    app.UseHsts();
}

// Thêm middleware xử lý mã trạng thái HTTP
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();