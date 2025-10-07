using ProjetoBanco.Services;

var builder = WebApplication.CreateBuilder(args);

// Registrar serviços e Swagger antes do Build
builder.Services.AddSingleton<BancoService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "API Banco Digital", Version = "v1" });
});

var app = builder.Build();

// Middleware depois do Build
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
