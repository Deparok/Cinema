using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net;
using WebApplicationCinema.Models;
using WebApplicationCinema.Models.DBInstances;

namespace WebApplicationCinema
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            IConfigurationSection sql = builder.Configuration.GetSection("SqlConnection");
            var sqlUser = sql.GetSection("User");
            var sqlAdmin = sql.GetSection("Admin");

            //MSSQLConnector user = new MSSQLConnector(sql.GetValue<string>("Ip"), sql.GetValue<string>("DataBase"), sqlUser.GetValue<string>("Login"), sqlUser.GetValue<string>("Password"));
            MSSQLConnector admin = new MSSQLConnector(sql.GetValue<string>("Ip"), sql.GetValue<string>("DataBase"), sqlAdmin.GetValue<string>("Login"), sqlAdmin.GetValue<string>("Password"));
            
            //user.AutoReconnect();
            admin.AutoReconnect();

            //if (!user.Connect())
            //{
            //    Console.WriteLine("Невозможно открыть подключение (от имени пользователя) к базе данных!");
            //    return;
            //}

            //if (!admin.Connect())
            //{
            //    Console.WriteLine("Невозможно открыть подключение (от имени администратора) к базе данных!");
            //    return;
            //}

            

            IConfigurationSection mailBox = builder.Configuration.GetSection("MailBox");

            MailBox box = new MailBox(mailBox.GetValue<string>("SmtpAddress"), mailBox.GetValue<int>("Port"), mailBox.GetValue<string>("Sender"), mailBox.GetValue<string>("Password"), mailBox.GetValue<bool>("EnableSSL"));

            PasswordHasher<string> hasher = new PasswordHasher<string>();

            PdfFormer pdf = new PdfFormer("Кинотеатр Эверест");

            builder.Services.AddKeyedSingleton<IAsyncDBWorker, MSSQLConnector>("user", (provider, key) => admin);
            builder.Services.AddKeyedSingleton<IAsyncDBWorker, MSSQLConnector>("admin", (provider, key) => admin);

            builder.Services.AddSingleton<IMailBox, MailBox>(provider => box);

            builder.Services.AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>(provider => hasher);

            builder.Services.AddSingleton<IPdfFormer, PdfFormer>(provider => pdf);

            ////LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Program>();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/denied";
                options.LogoutPath = "/logout";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = true;
                //options.Validate();
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                Console.WriteLine("Development");
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapControllers();

            app.Run();

            //user.Disconnect();
            admin.Disconnect();
        }
    }
}
