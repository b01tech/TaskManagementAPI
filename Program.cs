using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();

        builder.Services.AddDbContext<TaskContext>(option =>
            option.UseSqlite(builder.Configuration.GetConnectionString("TaskDbConnetion")));

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();    
        app.MapControllers();

        app.Run();
    }
}