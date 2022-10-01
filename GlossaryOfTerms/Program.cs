// See https://aka.ms/new-console-template for more information

using GlossaryOfTerms;

await Reader.ProcessValues("TestFiles");
await Task.WhenAll(Reader.WriteFileV1Async(), Reader.WriteFileV2Async(), Reader.WriteFileV3Async());
