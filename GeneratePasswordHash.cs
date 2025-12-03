using System;
using System.Security.Cryptography;
using System.Text;

// Programa simple para generar hash de contraseñas compatible con OneCardExpenseValidator
// Compilar con: csc GeneratePasswordHash.cs
// Ejecutar con: GeneratePasswordHash.exe "tu_contraseña"

class Program
{
    static void Main(string[] args)
    {
        string password;

        if (args.Length > 0)
        {
            password = args[0];
        }
        else
        {
            Console.Write("Ingresa la contraseña: ");
            password = Console.ReadLine() ?? "";
        }

        var (hash, salt) = CreatePasswordHash(password);

        Console.WriteLine("\n=== Hash de Contraseña Generado ===");
        Console.WriteLine($"Contraseña: {password}");
        Console.WriteLine($"\nPasswordHash: {hash}");
        Console.WriteLine($"\nPasswordSalt: {salt}");
        Console.WriteLine("\nCopia estos valores al script SQL");
    }

    static (string hash, string salt) CreatePasswordHash(string password)
    {
        using var hmac = new HMACSHA512();
        var salt = Convert.ToBase64String(hmac.Key);
        var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        return (hash, salt);
    }
}
