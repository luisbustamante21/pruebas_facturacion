
using ApiFacturacion.Interface;
using ApiFacturacion.Models;
using ApiFacturacion.Service;
using Microsoft.EntityFrameworkCore;
using System;

namespace ApiFacturacion
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddScoped<IFacturacionService, FacturacionService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddControllers().AddJsonOptions(x =>
            {
                x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });
            builder.Services.AddDbContext<FacturacionContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

            builder.Services.AddControllers();


            // Configuración de CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("PermitirTodo", policy =>
                {
                    policy.AllowAnyOrigin()   // Permite cualquier origen
                          .AllowAnyMethod()   // Permite cualquier método (GET, POST, PUT, DELETE, etc.)
                          .AllowAnyHeader();  // Permite cualquier encabezado
                });
            });


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            // Middleware de CORS
            app.UseCors("PermitirTodo");

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
