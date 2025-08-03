var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// allow webapp to hit API
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAnyOriginPolicy",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();
// allow webapp to hit API
app.UseCors("AllowAnyOriginPolicy");
app.MapControllers();
app.Run();
