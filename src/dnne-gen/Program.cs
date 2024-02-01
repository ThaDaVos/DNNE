// Copyright 2020 Aaron R Robinson
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DNNE.Assembly;
using DNNE.Assembly.Old;
using DNNE.Exceptions;
using DNNE.Generators;

namespace DNNE
{
    internal partial class Program
    {
        internal const string SafeMacroRegEx = "[^a-zA-Z0-9_]";

        internal readonly static Dictionary<string, GeneratorMapping> POSSIBLE_GENERATORS = new Dictionary<string, GeneratorMapping>()
        {
            {
                "ClarionSource",
                new GeneratorMapping{
                    Factory = (i) => new ClarionCodeGenerator(i),
                    NeedsClassSupport = true
                }
            },
            {
                "ClarionInc",
                new GeneratorMapping{
                    Factory = (i) => new ClarionIncludeGenerator(i),
                    NeedsClassSupport = true
                }
            },
            {
                "ClarionLib",
                new GeneratorMapping{
                    Factory = (i) => new ClarionLibGenerator(i),
                    NeedsClassSupport = true
                }
            },
            {
                "XMLDefinition",
                new GeneratorMapping{
                    Factory = (i) => new XMLDefinitionGenerator(i),
                    NeedsClassSupport = false
                }
            },
            {
                "XSDTypeContracts",
                new GeneratorMapping{
                    Factory = (i) => new XMLTypeContractsGenerator(i),
                    NeedsClassSupport = false
                }
            },
        };

        internal static void Main(string[] args)
        {
            try
            {
                // No arguments means help.
                if (args.Length == 0)
                {
                    args = new[] { "-?" };
                }

                ParsedArguments arguments = Parse(args);

                Console.WriteLine($"Processing assembly from `{arguments.AssemblyPath}`");

                Parser parser = new Parser(arguments.AssemblyPath, arguments.XmlDocFile);

                AssemblyInformation assemblyInformation = parser.Parse();

                ExecuteGenerators(
                    arguments.OutputPath,
                    arguments.UseClasses,
                    arguments.AdditionalGenerators,
                    assemblyInformation
                );
            }
            catch (ParseException pe)
            {
                Console.WriteLine($"Argument: '{pe.Argument}':\n{pe.Message}");
            }
            catch (GeneratorException ge)
            {
                Console.WriteLine($"Generator: '{ge.AssemblyPath}':\n{ge.Message}");
            }
        }

        private static ParsedArguments Parse(string[] args)
        {
            var parsed = new ParsedArguments()
            {
                OutputPath = string.Empty
            };

            for (int i = 0; i < args.Length; ++i)
            {
                var arg = args[i];
                if (arg.Length == 0)
                {
                    continue;
                }

                // Set the assembly
                if (arg[0] != '-')
                {
                    if (parsed.AssemblyPath != null)
                    {
                        throw new ParseException(arg, "Assembly already supplied.");
                    }

                    if (!File.Exists(arg))
                    {
                        throw new ParseException(arg, "Assembly not found.");
                    }

                    parsed.AssemblyPath = Path.GetFullPath(arg);
                    continue;
                }

                // Handle flags
                var flag = arg.Substring(1).ToLowerInvariant();
                switch (flag)
                {
                    case "c":
                        {
                            parsed.UseClasses = true;
                            break;
                        }
                    case "g":
                        {
                            if ((i + 1) == args.Length)
                            {
                                throw new ParseException(flag, "Missing list of additional generators");
                            }

                            arg = args[++i];
                            parsed.AdditionalGenerators = arg;
                            break;
                        }
                    case "o":
                        {
                            if ((i + 1) == args.Length)
                            {
                                throw new ParseException(flag, "Missing output file");
                            }
                            arg = args[++i];
                            parsed.OutputPath = arg;
                            break;
                        }
                    case "d":
                        {
                            if ((i + 1) == args.Length)
                            {
                                throw new ParseException(flag, "Missing documentation file");
                            }
                            arg = args[++i];
                            if (!File.Exists(arg))
                            {
                                throw new ParseException(arg, "Documentation file not found.");
                            }
                            parsed.XmlDocFile = arg;
                            break;
                        }
                    case "?":
                    case "help":
                        {
                            throw new ParseException(flag,
    @"Syntax: dnne-gen [-o <filepath> | -?]+ <path_to_assembly>
    -o <filepath>   : The output file for the generated source.
                        The last value is used. If file exists,
                        it will be overwritten.
                        If not supplied the generated source is
                        written to stdout.
    -d <xmldocfile>   : The location to the XML documentation file.
                        This can be activated project properties.
                        If supplied the comments from the functions
                        are added to the output header file.
    -?              : This message.
");
                        }
                    default:
                        throw new ParseException(flag, "Unknown flag");
                }
            }

            return parsed;
        }

        private static AssemblyInformation Read(string assemblyPath, string xmlDocFile)
        {
            using AssemblyReader reader = new AssemblyReader(assemblyPath, xmlDocFile);

            return reader.Read();
        }

        private static void ExecuteGenerators(string outputPath, bool useClasses, string additionalGenerators, AssemblyInformation assemblyInformation)
        {
            IEnumerable<GeneratorMapping> additionalGeneratorMappings = ExtractAdditionalGenerators(additionalGenerators);

            DoC99Generation(
                outputPath,
                useClasses || additionalGeneratorMappings.Any((mapping) => mapping.NeedsClassSupport),
                assemblyInformation
            );

            IEnumerable<IGenerator> generators = additionalGeneratorMappings.Select((mapping) => mapping.Factory(assemblyInformation));

            DoAdditionalGeneratorsGeneration(outputPath, generators);
        }

        private static IEnumerable<GeneratorMapping> ExtractAdditionalGenerators(string additionalGenerators)
        {
            string[] givenAdditionalGenerators = additionalGenerators?.Split(';', '|', ',') ?? Array.Empty<string>();

            bool includeAllGenerators = givenAdditionalGenerators.Contains("*");

            IEnumerable<GeneratorMapping> neededGeneratorMappings = POSSIBLE_GENERATORS
                .Where(
                    predicate: (KeyValuePair<string, GeneratorMapping> entry) =>
                        includeAllGenerators == true
                        || givenAdditionalGenerators.Contains(entry.Key)
                        || givenAdditionalGenerators.Any((generator) => entry.Key.StartsWith(generator.TrimEnd('*')))
            ).Select(selector: (KeyValuePair<string, GeneratorMapping> entry) => entry.Value);
            return neededGeneratorMappings;
        }

        private static void DoC99Generation(string outputPath, bool useClasses, AssemblyInformation assemblyInformation)
        {
            IGenerator c99Generator = useClasses
                ? new C99ClassedGenerator(assemblyInformation)
                : new C99Generator(assemblyInformation);

            if (string.IsNullOrWhiteSpace(value: outputPath))
            {
                using MemoryStream stream = new MemoryStream();

                c99Generator.Emit(outputStream: stream);
                Console.Out.Write(value: stream);
            }
            else
            {
                c99Generator.Emit(outputFile: outputPath);
                Console.WriteLine(value: $"Generated export for 'C99' written to '{outputPath}'.");
            }
        }

        private static void DoAdditionalGeneratorsGeneration(string outputPath, IEnumerable<IGenerator> generators)
        {
            foreach (IGenerator generator in generators)
            {
                try
                {
                    generator.Emit(outputFile: outputPath);
                    Console.WriteLine(
                    $"Generated export for '{generator.GetType().Name.Replace("Generator", "")}' written to '{generator.ParseOutPutFileName(outputPath)}'."
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                    $"Failed to generated export for '{generator.GetType().Name.Replace("Generator", "")}' error: {e}"
                    );
                }
            }
        }
    }
}
