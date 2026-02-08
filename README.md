# Pre-conditions

.csproj) includes the necessary NuGet references. Run these commands locally:

Add libraries:

```
dotnet add package Newtonsoft.Json

dotnet add package AutoCAD.NET --version 24.2.0  # Adjust version for your AutoCAD target

```

System.Management: In .NET Core/5+, you need to add the package: 

`dotnet add package System.Management`

## Expalain

net8.0-windows: Tells GitHub to use the Windows-specific .NET SDK that includes desktop libraries.

UseWindowsForms: Unlocks the System.Windows.Forms namespace so your using statement doesn't fail.

AutoCAD.NET Packages: By using these Official Autodesk NuGet Packages, GitHub Actions can download the necessary DLLs (like accoremgd.dll) during the dotnet restore step without you having to upload them manually.

## How to build

`dotnet build`

How to run

`dotnet run`

# How to run on AutoCAD

Để chạy thử file
.dll bạn vừa build từ C# (có nhúng Lisp bên trong) vào AutoCAD, bạn thực hiện theo các bước chuẩn sau đây:
Bước 1: Chuẩn bị file DLL
Sau khi bạn chạy lệnh dotnet build trên Linux (hoặc Windows), hãy tìm file đã biên dịch:

    Đường dẫn thường là: bin/Debug/net8.0-windows/MyApp.dll (hoặc net10.0-windows).
    Lưu ý: Nếu bạn build trên Linux, bạn cần copy file .dll này sang máy có cài AutoCAD (Windows) để chạy thử.

Bước 2: Nạp Tool vào AutoCAD (Lệnh NETLOAD)

    Mở AutoCAD lên.
    Tại dòng lệnh (Command Line), gõ: NETLOAD rồi nhấn Enter.
    Một bảng chọn file hiện ra, bạn tìm đến file MyApp.dll vừa build và nhấn Open.
    Nếu AutoCAD hiện cảnh báo "Security - Unsigned Executable", hãy chọn Always Load (hoặc Load Once).

Bước 3: Chạy Lệnh chính (Lệnh của C#)
Bây giờ, thay vì gõ lệnh Lisp, bạn phải gõ lệnh mà bạn đã định nghĩa trong C# (phần [CommandMethod("...")]):

    Gõ lệnh: MYTOOL và nhấn Enter.
    Lúc này, logic C# sẽ chạy:
        Nó sẽ kiểm tra bản quyền (Hàm IsLicenseValid).
        Nếu OK, nó sẽ tự động nạp nội dung Lisp từ trong "ruột" của nó ra.
        Sau đó nó tự thực thi lệnh HELLO_CONG (đã viết trong file .lsp).

Bước 4: Kiểm tra kết quả

    Bạn sẽ thấy dòng chữ: "[Hệ thống] Mã nguồn Lisp đã được nạp vào bộ nhớ AutoCAD." xuất hiện ở Command Line.
    AutoCAD sẽ yêu cầu: "Chọn tâm để vẽ hình tròn:".
    Bạn click chọn một điểm, một hình tròn bán kính 100 sẽ hiện ra.

Các lỗi thường gặp và cách xử lý (Troubleshooting)

    Gõ MYTOOL báo "Unknown Command":
        Do bạn chưa NETLOAD thành công.
        Hoặc tên lệnh trong code C# khác với lệnh bạn gõ (kiểm tra lại [CommandMethod("...")]).
    Lỗi "Could not load file or assembly...":
        Do phiên bản .NET bạn build (ví dụ .NET 8.0) cao hơn phiên bản AutoCAD hỗ trợ. (AutoCAD 2025 dùng .NET 8, AutoCAD 2021-2024 dùng .NET Core 3.1 hoặc .NET 6).
    Lisp không chạy sau khi gõ MYTOOL:
        Kiểm tra lại tên Resource trong code C#: ReadLispFromResource("MyApp.my_logic.lsp").
        Mẹo: Tên Resource phải phân biệt hoa thường và đúng cấu trúc TenProject.TenFile.lsp.

Cách gỡ Tool để Build lại
Khi bạn sửa code và muốn build lại, AutoCAD sẽ không cho bạn ghi đè file .dll đang dùng. Bạn phải:

    Đóng AutoCAD.
    Build lại file .dll.
    Mở lại AutoCAD và NETLOAD lại.