using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debitos
{
    public static class DatabaseConfig
    {
        // Al ser estática, esta variable vive en toda la aplicación.
        // Si el servidor cambia, SOLO modificás esta línea y toda la app se actualiza.
        public static string ConnectionString { get; } = "Host=172.16.13.219;Port=5432;Username=postgres;Password=postgres;Database=Debitos;";
    }
}
