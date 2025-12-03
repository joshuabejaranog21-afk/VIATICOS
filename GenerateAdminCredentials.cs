using System;
using System.Security.Cryptography;
using System.Text;

// Script para generar credenciales del admin
// Ejecuta este archivo con: dotnet-script GenerateAdminCredentials.cs
// O cópialo en un proyecto de consola temporal

var password = "Admin123!";

using var hmac = new HMACSHA512();
var salt = Convert.ToBase64String(hmac.Key);
var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

Console.WriteLine("=== Credenciales Admin ===");
Console.WriteLine($"Password: {password}");
Console.WriteLine();
Console.WriteLine("=== Para actualizar en la base de datos ===");
Console.WriteLine($"PasswordSalt: {salt}");
Console.WriteLine();
Console.WriteLine($"PasswordHash: {hash}");
Console.WriteLine();
Console.WriteLine("=== Script SQL ===");
Console.WriteLine($@"
UPDATE Users
SET PasswordHash = '{hash}',
    PasswordSalt = '{salt}',
    Email = 'admin@onecard.com',
    IsActive = 1
WHERE Username = 'admin';
");
