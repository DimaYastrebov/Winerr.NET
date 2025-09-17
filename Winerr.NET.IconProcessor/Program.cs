
using ImageMagick;
var processingFolder = @"G:\projects\Winerr.NET\Winerr.NET.Assets\Styles\win7_aero\Icons";

if (!Directory.Exists(processingFolder))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"ОШИБКА: Папка не найдена: {processingFolder}");
    Console.ResetColor();
    return;
}

var initialFiles = Directory.GetFiles(processingFolder);
var successfulTempFiles = new List<string>();

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"Начинаю обработку. Найдено файлов: {initialFiles.Length}");
Console.WriteLine("--------------------------------------------------");
Console.ResetColor();

foreach (var filePath in initialFiles)
{
    Console.Write($"Обрабатываю: {Path.GetFileName(filePath)}... ");
    bool success = false;
    var tempOutputPath = Path.Combine(processingFolder, $"__temp_{Guid.NewGuid()}.png");
    try
    {
        using var image = new MagickImage(filePath);
        image.Format = MagickFormat.Png;
        image.Write(tempOutputPath);
        success = true;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("УСПЕХ! (обработан как есть)");
    }
    catch
    {
    }
    if (!success)
    {
        var tempIcoPath = filePath + ".ico";
        try
        {
            File.Move(filePath, tempIcoPath);
            using var image = new MagickImage(tempIcoPath);
            image.Format = MagickFormat.Png;
            image.Write(tempOutputPath);
            success = true;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("УСПЕХ! (обработан как .ico)");
        }
        catch
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ПРОВАЛ! Файл битый.");
        }
        finally
        {
            if (File.Exists(tempIcoPath))
            {
                File.Move(tempIcoPath, filePath);
            }
        }
    }

    if (success)
    {
        successfulTempFiles.Add(tempOutputPath);
    }

    Console.ResetColor();
}

Console.WriteLine("--------------------------------------------------");
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("Фаза очистки: удаляю исходные файлы...");
foreach (var filePath in initialFiles)
{
    File.Delete(filePath);
}
Console.WriteLine("Очистка завершена.");
Console.ResetColor();

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("Фаза переименования: присваиваю порядковые номера...");
for (int i = 0; i < successfulTempFiles.Count; i++)
{
    var tempFile = successfulTempFiles[i];
    var finalPath = Path.Combine(processingFolder, $"{i}.png");
    File.Move(tempFile, finalPath);
}
Console.WriteLine("Переименование завершено.");
Console.ResetColor();


Console.WriteLine("--------------------------------------------------");
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("Обработка завершена.");
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Успешно обработано и сохранено: {successfulTempFiles.Count} файлов.");
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine($"Пропущено (битые): {initialFiles.Length - successfulTempFiles.Count} файлов.");
Console.ResetColor();
