// See https://aka.ms/new-console-template for more information

using DistEdu.GlossaryOfTerms;

await Reader.ProcessValues("TestFiles");
await Task.WhenAll(Reader.WriteCustomFileAsync(), Reader.WriteJsonFileAsync(), Reader.WriteMsgPackFileAsync());
