# Pre-conditions

.csproj) includes the necessary NuGet references. Run these commands locally:

Add libraries:

```
dotnet add package Newtonsoft.Json

dotnet add package AutoCAD.NET --version 24.2.0  # Adjust version for your AutoCAD target

```

System.Management: In .NET Core/5+, you need to add the package: 

`dotnet add package System.Management`

# Expalain

net8.0-windows: Tells GitHub to use the Windows-specific .NET SDK that includes desktop libraries.

UseWindowsForms: Unlocks the System.Windows.Forms namespace so your using statement doesn't fail.

AutoCAD.NET Packages: By using these Official Autodesk NuGet Packages, GitHub Actions can download the necessary DLLs (like accoremgd.dll) during the dotnet restore step without you having to upload them manually.

# How to build

`dotnet build`

# How to run

`dotnet run`

