/*
 * Student Name: Sarah Shields
 * Student Number: 301264350
 * Submission Date: October 30th, 2024
 */

using _301264350_shields__Lab3.Data;
using Amazon.S3;
using Amazon.DynamoDBv2;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Amazon.DynamoDBv2.DataModel;

var builder = WebApplication.CreateBuilder(args);

// set up AWS options
builder.Services.AddAWSService<IAmazonSimpleSystemsManagement>();
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());

// fetch RDS password from AWS Parameter Store
var ssmClient = builder.Services.BuildServiceProvider().GetRequiredService<IAmazonSimpleSystemsManagement>();
var parameterResponse = await ssmClient.GetParameterAsync(new GetParameterRequest
{
    Name = "Assign3Passwords",
    WithDecryption = true
});

// extract password from response
var rdsPassword = parameterResponse.Parameter.Value;

// temporary: Log the password to the console
Console.WriteLine($"RDS Password: {rdsPassword}");

// set up connection string
var connectionString = $"Server=assign3userregistrationrds.cd4m242ueu41.us-east-1.rds.amazonaws.com;Database=assign3_user_registration;User Id=admin;Password={rdsPassword};TrustServerCertificate=True;";

// add DbContext with dynamic connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Enable cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
    });

// Register controllers, Razor Pages, S3 client, DynamoDB client/context
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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

app.MapRazorPages();

app.Run();