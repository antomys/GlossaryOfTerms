// See https://aka.ms/new-console-template for more information

using DistEdu.GlossaryOfTerms;

// For csv.
await Reader.ProcessCsvValues("TestFiles");
// await Task.WhenAll(Reader.WriteCustomFileAsync(), Reader.WriteJsonFileAsync(), Reader.WriteMsgPackFileAsync());
// For csv.