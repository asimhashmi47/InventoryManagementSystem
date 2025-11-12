using IMS_API;
using IMS_API.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()    // or .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddScoped<DatabaseContext>();// Register DatabaseContext as a Scoped service

// Register IDatabaseConnectionProvider
builder.Services.AddScoped<IDatabaseConnectionProvider, IMS_API.Repositories.DatabaseConnectionProvider>();

// Register IUserRepository
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Register IInventoryRepository
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
// Register ISupplierRepository and SupplierRepository
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
// Register IRoleRepository and IRoleRepository
builder.Services.AddScoped<IRoleRepository, RoleRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseCors("AllowAll");

app.Run();
