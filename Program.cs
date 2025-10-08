using ProjetoBanco.Services;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURAR CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontLocal", policy =>
    {
        policy
            .WithOrigins("http://127.0.0.1:5500") // endereço do seu front
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Registrar serviços e Swagger antes do Build
builder.Services.AddSingleton<BancoService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "API Banco Digital", Version = "v1" });
});

var app = builder.Build();

// --- APLICAR CORS ---
app.UseCors("PermitirFrontLocal");

// Middleware depois do Build
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();


// http://localhost:5151/swagger/index.html
// http://localhost:5151/api/Banco/deposito