using GuideViewer.Core.Utilities;

Console.WriteLine("GuideViewer Product Key Generator");
Console.WriteLine("==================================");
Console.WriteLine();

// Generate 5 admin and 5 technician keys
ProductKeyGenerator.PrintKeys(adminCount: 5, techCount: 5);

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
