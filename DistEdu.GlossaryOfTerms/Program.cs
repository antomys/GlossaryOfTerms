using DistEdu.GlossaryOfTerms;

const string testFolder = "TestFiles";
Console.Write("Please choose job:\n1. Simple process(task1)\n2. Normalize for Invert index(task2)\n3. Build adjacency matrix\n0. Exit");

do
{
    Console.Write("\n>");
    int.TryParse(Console.ReadLine(), out var inputNumber);
    
    switch (inputNumber)
    {
        case 1:
        {
            await Reader.ProcessValues(testFolder);
            await Task.WhenAll(Reader.WriteCustomFileAsync(), Reader.WriteJsonFileAsync(), Reader.WriteMsgPackFileAsync());
           
            break;
        }
        case 2:
        {
            await Reader.ProcessCsvValues(testFolder); 

            break;
        }
        case 3:
        {
            await Reader.ProcessMatrixValues(testFolder);
            
            break;
        }
        case 0:
        {
            return 0;
        }
        default:
        {
            Console.Write("Invalid input. Try again");

            break;
        }
    }
}
while (true);