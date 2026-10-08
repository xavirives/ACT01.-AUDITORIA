using System;
using AppInsegura.Datos;
using AppInsegura.Modelos;
using AppInsegura.Servicios;

namespace AppInsegura
{
    public class Program
    {
        private static readonly BaseDatosUsuarios baseDatos = new BaseDatosUsuarios();
        private static readonly AuthService auth = new AuthService(baseDatos);
        private static Usuario? usuarioActual = null;

        public static void Main(string[] args)
        {
            CargarUsuariosDeEjemplo();

            Console.WriteLine("=== Gestor de Usuarios y Partidas ===");
            Console.WriteLine("(usuarios de prueba: admin/admin1234, ana/ana2024)");
            Console.WriteLine();

            bool salir = false;
            while (!salir)
            {
                MostrarMenu();
                string opcion = Console.ReadLine() ?? "";

                try
                {
                    switch (opcion)
                    {
                        case "1":
                            Registrar();
                            break;
                        case "2":
                            IniciarSesion();
                            break;
                        case "3":
                            BuscarUsuario();
                            break;
                        case "4":
                            VerPerfil();
                            break;
                        case "5":
                            PanelAdministracion();
                            break;
                        case "6":
                            SincronizarConServidor();
                            break;
                        case "0":
                            salir = true;
                            break;
                        default:
                            Console.WriteLine("Opción no válida.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ha ocurrido un error inesperado:");
                    Console.WriteLine(ex.ToString());
                }

                Console.WriteLine();
            }

            Console.WriteLine("Hasta luego.");
        }

        private static void CargarUsuariosDeEjemplo()
        {
            auth.Registrar("admin", "admin1234", "admin");
            auth.Registrar("ana", "ana2024", "jugador");
        }

        private static void MostrarMenu()
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine($"Usuario actual: {(usuarioActual != null ? usuarioActual.Nombre : "ninguno")}");
            Console.WriteLine("1. Registrar usuario");
            Console.WriteLine("2. Iniciar sesión");
            Console.WriteLine("3. Buscar usuario por nombre");
            Console.WriteLine("4. Ver mi perfil");
            if (usuarioActual != null && usuarioActual.Rol == "admin")
            {
                Console.WriteLine("5. Panel de administración");
            }
            Console.WriteLine("6. Sincronizar partida con el servidor");
            Console.WriteLine("0. Salir");
            Console.Write("Elige una opción: ");
        }

        private static void Registrar()
        {
            Console.Write("Nombre de usuario: ");
            string nombre = Console.ReadLine() ?? "";

            Console.Write("Correo electrónico: ");
            string correo = Console.ReadLine() ?? "";

            if (!correo.Contains("@") || !correo.Contains("."))
            {
                Console.WriteLine("El correo electrónico no es válido.");
                return;
            }

            string contrasena = "";
            bool contrasenaValida = false;
            int intentos = 0;

            while (!contrasenaValida && intentos < 3)
            {
                Console.Write("Contraseña (entre 8 y 20 caracteres y una mayúscula): ");
                contrasena = LeerContrasenaOculta();

                bool tieneMayuscula = false;

                for (int i = 0; i < contrasena.Length; i++)
                {
                    if (char.IsUpper(contrasena[i]))
                    {
                        tieneMayuscula = true;
                    }
                }

                if (contrasena.Length >= 8 &&
                    contrasena.Length <= 20 &&
                    tieneMayuscula)
                {
                    contrasenaValida = true;
                }
                else
                {
                    intentos++;
                    Console.WriteLine("Contraseña no válida. Debe tener entre 8 y 20 caracteres y una mayúscula.");
                    Console.WriteLine("Intentos restantes: " + (3 - intentos));
                }
            }

            if (!contrasenaValida)
            {
                Console.WriteLine("Has agotado los 3 intentos. Registro cancelado.");
                return;
            }

            Usuario nuevo = auth.Registrar(nombre, contrasena);
            Console.WriteLine($"Usuario '{nuevo.Nombre}' registrado con rol '{nuevo.Rol}'.");
        }

        private static void IniciarSesion()
        {
            Console.Write("Nombre de usuario: ");
            string nombre = Console.ReadLine() ?? "";
            Console.Write("Contraseña: ");
            string contrasena = LeerContrasenaOculta();

            Usuario? usuario = auth.IniciarSesion(nombre, contrasena);
            if (usuario == null)
            {
                Console.WriteLine("Usuario o contraseña incorrectos.");
                return;
            }

            usuarioActual = usuario;
            Console.WriteLine($"Bienvenido, {usuario.Nombre}.");
        }

        private static void BuscarUsuario()
        {
            Console.Write("Nombre a buscar: ");
            string nombre = Console.ReadLine() ?? "";

            Usuario? encontrado = baseDatos.BuscarPorNombre(nombre);
            Console.WriteLine(encontrado != null
                ? $"Encontrado: {encontrado.Nombre} (rol: {encontrado.Rol})"
                : "No se ha encontrado ningún usuario con ese nombre.");
        }

        private static void VerPerfil()
        {
            if (usuarioActual == null)
            {
                Console.WriteLine("Primero debes iniciar sesión.");
                return;
            }

            Console.WriteLine($"Nombre: {usuarioActual.Nombre}");
            Console.WriteLine($"Rol: {usuarioActual.Rol}");
            Console.WriteLine($"Token de sesión: {usuarioActual.TokenSesion}");
        }

        private static void PanelAdministracion()
        {
            // Verificación de autorización en la lógica de negocio
            if (usuarioActual == null || usuarioActual.Rol != "admin")
            {
                Console.WriteLine("Acceso denegado. No tienes permisos de administrador.");
                return;
            }

            Console.WriteLine("=== PANEL DE ADMINISTRACIÓN ===");
            Console.WriteLine("Lista de usuarios registrados:");
            foreach (Usuario u in baseDatos.ListarTodos())
            {
                Console.WriteLine($" - {u.Nombre} ({u.Rol})");
            }
        }

        private static void SincronizarConServidor()
        {
            if (usuarioActual == null)
            {
                Console.WriteLine("Primero debes iniciar sesión.");
                return;
            }

            var red = new RedService();
            red.EnviarPuntuacion(usuarioActual.Nombre, 1000);
        }

        private static string LeerContrasenaOculta()
        {
            string pass = "";
            ConsoleKeyInfo key;

            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass[..^1];
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
            }

            Console.WriteLine();
            return pass;
        }
    }
}
