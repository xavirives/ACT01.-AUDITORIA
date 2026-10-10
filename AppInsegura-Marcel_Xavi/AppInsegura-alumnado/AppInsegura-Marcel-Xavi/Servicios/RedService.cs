using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AppInsegura.Servicios
{
    public class RedService
    {
        private const string UrlServidor = "https://api.miapp-insegura.local/puntuaciones";

        public void EnviarPuntuacion(string nombreUsuario, int puntuacion)
        {
            try
            {
                EnviarPuntuacionAsync(nombreUsuario, puntuacion).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine("No se ha podido conectar con el servidor (es normal si no tienes conexión real):");
                Console.WriteLine(ex.Message);
            }
        }

private async Task EnviarPuntuacionAsync(string nombreUsuario, int puntuacion)
{
    using var cliente = new HttpClient();

    var datos = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("usuario", nombreUsuario),
        new KeyValuePair<string, string>("puntos", puntuacion.ToString())
    });

    Console.WriteLine($"Enviando puntuación a: {UrlServidor}");
    HttpResponseMessage respuesta = await cliente.PostAsync(UrlServidor, datos);
    Console.WriteLine($"Respuesta del servidor: {(int)respuesta.StatusCode}");
}
    }
}
