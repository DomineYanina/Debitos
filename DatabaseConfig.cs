using Microsoft.Extensions.Configuration;
using System.IO;

namespace Debitos
{
    public static class DatabaseConfig
    {
        // Propiedad de solo lectura que el resto del sistema consumirá
        public static string ConnectionString { get; }

        // El constructor estático se ejecuta una sola vez al iniciar la aplicación
        static DatabaseConfig()
        {
            // Construimos la configuración apuntando al archivo appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();

            // Leemos la cadena de conexión específica
            ConnectionString = config.GetConnectionString("DefaultConnection");
        }
    }
}