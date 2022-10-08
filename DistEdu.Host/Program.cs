// See https://aka.ms/new-console-template for more information

using DistEdu.Host;

Console.CancelKeyPress += CancelHandler;
const string folder = "TestFiles";

var mainMenu = new MainMenu(folder);

await mainMenu.PrintAsync(folder);

return 0;


static void CancelHandler(object? sender, ConsoleCancelEventArgs args)
{
    Console.WriteLine("Exit application");

    Environment.Exit(0);
}