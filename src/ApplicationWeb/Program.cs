using ApplicationCore.Entities.ApplicationUser;
using ApplicationCore.Interfaces;
using ApplicationCore.Utilities;
using ApplicationWeb.Data;
using ApplicationWeb.HelperAndConstant;
using ApplicationWeb.Hubs;
using ApplicationWeb.MiddlewareExtensions;
using ApplicationWeb.SubscribeTableDependencies;
using AutoMapper;
using FluentValidation.AspNetCore;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Accounting;
using Infrastructure.Interfaces.Authentication;
using Infrastructure.Interfaces.Inventory;
using Infrastructure.Interfaces.Marketing;
using Infrastructure.Interfaces.Products;
using Infrastructure.Interfaces.Public;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Infrastructure.Interfaces.UI;
using Infrastructure.Services;
using Infrastructure.Services.Accounting;
using Infrastructure.Services.Inventory;
using Infrastructure.Services.Marketing;
using Infrastructure.Services.Products;
using Infrastructure.Services.Public;
using Infrastructure.Services.SetupAndConfigurations;
using Infrastructure.Services.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var host = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
        webBuilder.UseWebRoot("wwwroot");
    })
    .Build();

builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
}).AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders(); ;

builder.Services.AddAuthorization(options =>
{
    foreach (var item in RequiredPolicy.GetRequiredPolicy())
    {
        options.AddPolicy(item.PolicyName, policy => policy.RequireAuthenticatedUser().RequireClaim(item.ClaimName, item.Value));
    }
});

builder.Services.AddRazorPages();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    //options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    // enables immediate logout, after updating the user's stat.
    options.ValidationInterval = TimeSpan.Zero;
});

builder.Services.Configure<IdentityOptions>(opts =>
{
    opts.Password.RequiredLength = 3;
    opts.Password.RequireLowercase = false;
    opts.Password.RequireUppercase = false;
    opts.Password.RequireDigit = false;
    opts.Password.RequireNonAlphanumeric = false;
});

builder.Services.Configure<DataProtectionTokenProviderOptions>(opts => opts.TokenLifespan = TimeSpan.FromHours(10));

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.AddHttpClient();
builder.Services.AddControllers().AddFluentValidation(v =>
{
    v.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
});


builder.Services.Configure<Setting>(options =>
{
    options.BasePath = builder.Configuration.GetSection("AppSettings:FileBasePath")?.Value;
    options.CategoryFilePath = builder.Configuration.GetSection("AppSettings:CategoryPath")?.Value;
    options.ConnectionString = builder.Configuration.GetConnectionString("ConnectionString");
    options.FileDomain = builder.Configuration.GetSection("AppSettings:Domain")?.Value;
    options.QuestionFilePath = builder.Configuration.GetSection("AppSettings:QuestionPath")?.Value;
    options.WordsFilePath = builder.Configuration.GetSection("AppSettings:WordsPath")?.Value;
    options.TemplateFilePath = builder.Configuration.GetSection("AppSettings:TemplatePath")?.Value;
    options.MaterialFilePath = builder.Configuration.GetSection("AppSettings:MaterialPath")?.Value;
    options.BlogPath = builder.Configuration.GetSection("AppSettings:BlogPath")?.Value;

    options.ContainerAccount = builder.Configuration.GetSection("AppSettings:ContainerAccount")?.Value;
    options.ContainerAccessKey = builder.Configuration.GetSection("AppSettings:ContainerAccessKey")?.Value;

    options.MessageSecretId = builder.Configuration.GetSection("MessageConfiguration:SecretId")?.Value;
    options.MessagteApiKey = builder.Configuration.GetSection("MessageConfiguration:ApiKey")?.Value;
    options.MessageType = builder.Configuration.GetSection("MessageConfiguration:Type")?.Value;
    options.MessageSenderId = builder.Configuration.GetSection("MessageConfiguration:SenderId")?.Value;
    options.MessageTemplateId = builder.Configuration.GetSection("MessageConfiguration:TemplateId")?.Value;

    options.MailHost = builder.Configuration.GetSection("MailConfiguration:Host")?.Value;
    options.MailPort = builder.Configuration.GetSection("MailConfiguration:Port")?.Value;
    options.MailUserId = builder.Configuration.GetSection("MailConfiguration:UserId")?.Value;
    options.MailPassword = builder.Configuration.GetSection("MailConfiguration:Password")?.Value;
    options.MailSSL = Convert.ToBoolean(builder.Configuration.GetSection("MailConfiguration:SSL")?.Value);
});

#region Authentication

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
    };
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // Set the expiration time for the cookie
    options.SlidingExpiration = true;
});

#endregion Authentication

#region Services

builder.Services.AddSignalR();
builder.Services.AddSingleton<DashboardHub>();
builder.Services.AddSingleton<SubscribeProductTableDependency>();
builder.Services.AddSingleton<ProductsAndCustomerTableDependency>();
builder.Services.AddSingleton<ProductNotificationTableDependency>();

builder.Services.AddTransient(typeof(IDapperBaseService<>), typeof(DapperBaseService<>));
builder.Services.AddTransient(typeof(IDapperService<>), typeof(DapperService<>));
builder.Services.AddTransient<IImageProcessing, ImageProcessing>();
builder.Services.AddTransient<IClaimInformationService, ClaimInformationService>();
builder.Services.AddTransient<IMenuMasterService, MenuMasterService>();
builder.Services.AddTransient<MenuMasterService>();
builder.Services.AddTransient<IProductReportsService, ProductReportsService>();
builder.Services.AddTransient<IOperationalUserService, OperationalUserService>();
builder.Services.AddTransient<IItemService, ItemService>();
builder.Services.AddTransient<IPurchaseService, PurchaseService>();
builder.Services.AddTransient<IPurchaseReturnService, PurchaseReturnService>();
builder.Services.AddTransient<ISalesReturnService, SalesReturnService>();
builder.Services.AddTransient<ISalseService, SalseService>();
builder.Services.AddTransient<ReportService>();

builder.Services.AddTransient<IAccountLedgerService, AccountLedgerService>();
builder.Services.AddTransient<IPaymentVoucherService, PaymentVoucherService>();
builder.Services.AddTransient<IReceiveVoucherService, ReceiveVoucherService>();
builder.Services.AddTransient<IJournalVoucherService, JournalVoucherService>();

builder.Services.AddTransient<ICollectionFinalizedService, CollectionFinalizedService>();
builder.Services.AddScoped<Infrastructure.Interfaces.Inventory.IPaymentService, Infrastructure.Services.Inventory.PaymentService>();
builder.Services.AddTransient<IAdjustmentService, AdjustmentService>();
builder.Services.AddScoped<IPaymentFinalizedService, PaymentFinalizedService>();
//builder.Services.AddTransient<ICollectionReportService, CollectionReportService>();
builder.Services.AddScoped<CommonVoucherService>();


builder.Services.AddTransient<ICollectionService, CollectionService>();
builder.Services.AddTransient<ICollectionDepositService, CollectionDepositService>();

#region Product

builder.Services.AddTransient<IColorService, ColorService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddTransient<IUnitService, UnitService>();
builder.Services.AddTransient<IBrandService, BrandService>();
builder.Services.AddTransient<IAttributeService, AttributeService>();

builder.Services.AddTransient<IProductStockService, ProductStockService>();
builder.Services.AddTransient<IProductTaxService, ProductTaxService>();
builder.Services.AddTransient<IFlashDealService, FlashDealService>();
builder.Services.AddTransient<ICouponService, Infrastructure.Services.Marketing.CouponService>();
builder.Services.AddTransient<IReviewsService, ReviewsService>();
builder.Services.AddTransient<IProductTypeService, ProductTypeService>();
builder.Services.AddTransient<IProductService, Infrastructure.Services.Products.ProductService>();
builder.Services.AddTransient<IUploadService, UploadService>();

builder.Services.AddTransient<IOrderListService, OrderListService>();
builder.Services.AddTransient<IBulkActionService, BulkActionService>();

#endregion Product

#region Setup & Configuration

builder.Services.AddTransient<IGeneralSettings, GeneralSettingsService>();
builder.Services.AddTransient<ICurrencyService, CurrencyService>();
builder.Services.AddTransient<IVatAndTaxService, VatAndTaxService>();
builder.Services.AddTransient<IPickupPointsService, PickupPointsService>();
builder.Services.AddTransient<IBusinessSettingService, BusinessSettingService>();
builder.Services.AddTransient<IShippingCountryService, ShippingCountryService>();
builder.Services.AddTransient<IShippingStateService, ShippingStateService>();
builder.Services.AddTransient<IShippingCityService, ShippingCityService>();
builder.Services.AddTransient<IStaffService, StaffService>();

#endregion Setup & Configuration

#region UI

builder.Services.AddScoped<IGeneralService, GeneralService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, Infrastructure.Services.CustomerService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<Infrastructure.Interfaces.Authentication.IPaymentService, Infrastructure.Services.PaymentService>();

#endregion UI

#endregion Services

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseDeveloperExceptionPage();

app.MapHub<DashboardHub>("/dashboardHub");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area=Admin}/{controller=Administration}/{action=login}/{id?}");

    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.UseSqlTableDependency<SubscribeProductTableDependency>(connectionString);
app.UseSqlTableDependency<ProductsAndCustomerTableDependency>(connectionString);
app.UseSqlTableDependency<ProductNotificationTableDependency>(connectionString);
app.Run();