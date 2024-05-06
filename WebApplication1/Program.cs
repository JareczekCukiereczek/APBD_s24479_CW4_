
using WebApplication1.Repository;
using WebApplication1.Service;
using WebApplication1.Services;

class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args); //wzorzed buidler

        
        builder.Services.AddEndpointsApiExplorer(); //bibiloteki automat generuja dok.do.api
        builder.Services.AddSwaggerGen(); //wizualna dokumentacja openAPI - dok.apk.webowych
        builder.Services.AddControllers();
        builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>(); //nowy serwis rejestracja w konter IoC
        builder.Services.AddScoped<IWarehouseService, WarehouseService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) //co z zadaniem http jak dojdzie do naszego server - pipeline przetwaorzajacy  zapytanie do servera
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        app.UseHttpsRedirection();
        app.Run();
    }
}