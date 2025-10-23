using ETICARET.Business.Abstract;
using ETICARET.Business.Concrete;
using ETICARET.DataAccess.Abstract;
using ETICARET.DataAccess.Concrete;
using ETICARET.Entities;
using ETICARET.WebUI.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ETICARET.WebUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ApplicationIdentityDbContext>(opt =>
                opt.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
                .AddDefaultTokenProviders();
            
            // Seed Identity
            var userManager = builder.Services.BuildServiceProvider().GetService<UserManager<ApplicationUser>>();
            var roleManager = builder.Services.BuildServiceProvider().GetService<RoleManager<IdentityRole>>();

            builder.Services.Configure<IdentityOptions>(

                options =>
                {
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequiredLength = 0;

                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                    options.SignIn.RequireConfirmedEmail = true;
                });

            builder.Services.AddScoped<IProductDal, EfCoreProductDal>();
            builder.Services.AddScoped<IProductService, ProductManager>();
            builder.Services.AddScoped<ICategoryDal, EfCoreCategoryDal>();
            builder.Services.AddScoped<ICategoryService, CategoryManager>();
            builder.Services.AddScoped<ICommentDal, EfCoreCommentDal>();
            builder.Services.AddScoped<ICommentService, CommentManager>();
            builder.Services.AddScoped<ICartDal, EfCoreCartDal>();
            builder.Services.AddScoped<ICartService, CartManager>();
            builder.Services.AddScoped<IOrderDal, EfCoreOrdelDal>();
            builder.Services.AddScoped<IOrderService, OrderManager>();

            var app = builder.Build();

            // Seed Database 
            SeedDatabase.Seed();
            
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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Admin}/{action=CreateProduct}/{id?}");

            SeedIdentity.Seed(userManager, roleManager, app.Configuration).Wait();

            app.Run();
        }
    }
}
