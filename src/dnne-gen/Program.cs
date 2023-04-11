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
using System.IO;
using DNNE.Generators;

namespace DNNE
{
    partial class Program
    {
        public const string SafeMacroRegEx = "[^a-zA-Z0-9_]";

        static void Main(string[] args)
        {
            try
            {
                // No arguments means help.
                if (args.Length == 0)
                {
                    args = new[] { "-?" };
                }

                var parsed = Parse(args);

                using (var reader = new AssemblyReader(parsed.AssemblyPath, parsed.XmlDocFile))
                {
                    var info = reader.Read();

                    Console.WriteLine($"Generating exports using classes?: " + (parsed.UseClasses ? "Yes" : "No"));

                    var generators = parsed.UseClasses
                        ? new IGenerator[] {
                            new C99ClassedGenerator(info),
                            new ClarionCodeGenerator(info),
                            new ClarionIncludeGenerator(info),
                            new ClarionLibGenerator(info),
                        }
                        : new IGenerator[] {
                            new C99Generator(info),
                        };

                    foreach (var generator in generators)
                    {
                        // if (string.IsNullOrWhiteSpace(parsed.OutputPath))
                        // {
                        //     generator.Emit(Console);
                        // }
                        // else
                        // {
                            generator.Emit(parsed.OutputPath);
                            Console.WriteLine($"Generated exports written to '{parsed.OutputPath}'.");
                        // }
                    }
                }
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

        static ParsedArguments Parse(string[] args)
        {
            var parsed = new ParsedArguments()
            {
                OutputPath = string.Empty
            };

            Console.WriteLine("Passed arguments: " + string.Join('|', args));

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

                    parsed.AssemblyPath = arg;
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
    }
}
