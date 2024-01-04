using Img2GD;
#if DEBUG
using System.Text.Json;
#endif

// TODO:
//  - Write a good readme
//  - Create a roadmap of features

// TODO NOW:
//  - Port over other features
//  - Implement ideas
//  - resize image before importing
//  - reserved alpha channels

// TODO LATER: [research]
//  - optimization


var opt = new Cli(args).Options;

var importer = new Importer(opt);

#if DEBUG
Logma.Debug("Hello, World!");
Logma.Debug("Parsed arguments:");
Logma.Debug(JsonSerializer.Serialize(opt, new JsonSerializerOptions{WriteIndented = true, IncludeFields = true}));
#endif