using IMS_API.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddScoped<IMS_API.DatabaseContext>();  // Scoped (or Transient if required)
builder.Services.AddScoped<IUserRepository, UserRepository>(); // Register IUserRepository

// Register IDatabaseConnectionProvider (Scoped to match UserRepository's lifetime)
builder.Services.AddScoped<IDatabaseConnectionProvider, DatabaseConnectionProvider>();

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

app.Run();
