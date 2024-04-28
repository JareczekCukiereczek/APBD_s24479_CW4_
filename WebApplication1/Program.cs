
using WebApplication1.Repository;

class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args); //wzorzed buidler

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer(); //bibiloteki automat generuja dok.do.api
        builder.Services.AddSwaggerGen(); //wizualna dokumentacja openAPI - dok.apk.webowych
        builder.Services.AddControllers();
        builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();//nowy serwis rejestracja w konter IoC

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