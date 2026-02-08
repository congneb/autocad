// Program.cs
using System.Runtime.InteropServices;

Console.WriteLine("--- Linux C# Info Tool ---");
Console.WriteLine($"OS: {RuntimeInformation.OSDescription}");
Console.WriteLine($"Architecture: {RuntimeInformation.OSArchitecture}");
Console.WriteLine($"Current Time: {DateTime.Now}");
Console.WriteLine("--------------------------");
