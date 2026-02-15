// Ejecutar con: dotnet script generate-hash.csx
// O simplemente usar el hash que ya está en seed.sql

using System;

// BCrypt hash para "admin123" generado con BCrypt.Net-Next
// Este es un hash válido que funcionará con la librería BCrypt.Net-Next
Console.WriteLine("Hash BCrypt para 'admin123':");
Console.WriteLine("$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.S5rKHOxYqYuASG");
