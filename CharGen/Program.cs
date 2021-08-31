using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CharGen
{
    // This was supposed to be a simple script, but figured its much easier to just do a dotnetcore cli for it :P
    // pls do not code shame me, I know this is ugly af, this is what I create when I want something done in a single evening.
    // Fun exercise: do a ctrl-f with "replace"
    class Program
    {
        static Program()
        {
            _optionalStrings = new[] {"EquipmentPools"};
        }

        private static readonly string[] _optionalStrings;

        private static string _manifestFileTemplate;
        private static string _characterFileTemplate;
        private static string _outputFolder;


        private static List<MeatyCharacter> _toCreate;

        static string ReplaceFields(object obj, string input)
        {
            string output = input;
            var type = obj.GetType();
            var list = type.GetFields();
            //TODO: Extend if needed
            Dictionary<Type, ReplaceType> typeMap = new Dictionary<Type, ReplaceType>();
            typeMap.Add(typeof(string), ReplaceType.Characters);
            typeMap.Add(typeof(Version), ReplaceType.Characters);
            typeMap.Add(typeof(int), ReplaceType.Number);
            typeMap.Add(typeof(float), ReplaceType.Number);
            typeMap.Add(typeof(List<string>), ReplaceType.List);

            Regex regex;

            //TODO! Check if there is more than one ReplaceAdditions attribute, if yes, throw exception.
            foreach (var field in list)
            {
                if (field.GetCustomAttribute(typeof(ReplaceValue)) is ReplaceValue att)
                {
                    if (typeMap[field.FieldType] == ReplaceType.Characters)
                    {
                        //Do a normal replace 1:1 replace
                        output = Helpers.ReplaceInString(output, $"_{field.Name}_", field.GetValue(obj)?.ToString());
                    }
                    else if (typeMap[field.FieldType] == ReplaceType.Number)
                    {
                        regex = new Regex($"\"_(\\d+\\.?\\d*)_{field.Name}_\"");
                        var multiplyMatches = regex.Matches(output);
                        foreach (Match match in multiplyMatches)
                        {
                            //Unboxing, just in case casting wont work
                            var unbox = Convert.ToSingle(field.GetValue(obj));
                            output = Helpers.ReplaceAndMultiply(output, match.Groups["0"].Value,
                                float.Parse(match.Groups["1"].Value), unbox , att.AllowFloat);
                        }

                        regex = new Regex($"\"_{field.Name}_\"");
                        var replaceMatches = regex.Matches(output);
                        foreach (Match match in replaceMatches)
                        {
                            //Unboxing, just in case casting wont work
                            var unbox = Convert.ToSingle(field.GetValue(obj));
                            output = Helpers.ReplaceInString(output, match.Groups["0"].Value,
                                field.GetValue(obj)?.ToString());
                        }

                        //TODO Here add support for partial replaces for number, so it doesnt have to always surrounded with ""
                    }
                    else if (typeMap[field.FieldType] == ReplaceType.List)
                    {
                        //TODO Instead of string.Join, which uses ToString(), probably should use serialization, to make complex classes also supported. This is faster tho :)
                        if (field.GetValue(obj) is IEnumerable IEnum)
                        {
                            if (att.AsNumbered)
                            {
                                int i = 0;
                                foreach (var elem in IEnum)
                                {
                                    output = Helpers.ReplaceInString(output, $"_{field.Name}{i}_", elem.ToString());
                                    i++;
                                }
                            }
                            else
                            {
                                string toReplaceWith = string.Empty;
                                foreach (var elem in IEnum)
                                {
                                    toReplaceWith += $"\"{elem.ToString()}\"";
                                    toReplaceWith += ',';
                                }

                                toReplaceWith = $"[{toReplaceWith.Substring(0, toReplaceWith.Length - 1)}]";
                                output = Helpers.ReplaceInString(output, $"\"_{field.Name}_\"", toReplaceWith);
                            }
                        }
                    }
                }
                else if (field.GetCustomAttribute(typeof(ReplaceAdditions)) is ReplaceAdditions additions)
                {
                    //TODO! 
                }
            }

            return output;
        }

        static void Main(string[] args)
        {
            if (args.Length != 4)
                return;

            _toCreate = JsonConvert.DeserializeObject<List<MeatyCharacter>>(File.ReadAllText(args[0]));
            _characterFileTemplate = File.ReadAllText(args[1]);
            _manifestFileTemplate = File.ReadAllText(args[2]);
            _outputFolder = args[3];

            foreach (var character in _toCreate)
            {
                string readyChar = GenerateCharacter(character);

                var charName = character.Name.Replace(" ", string.Empty) + character.Version.ToString();
                var folderName = Path.Combine(_outputFolder, charName);

                if (Directory.Exists(folderName))
                    Directory.Delete(folderName, true);
                    //throw new Exception($"Directory {folderName} already exists!");

                var innerFolderpath = Path.Combine(folderName, character.Name);
                if (Directory.Exists(innerFolderpath))
                    Directory.Delete(innerFolderpath, true);
                    //throw new Exception($"Directory {innerFolderpath} already exists!");
                    
                Directory.CreateDirectory(innerFolderpath);

                string readyManifest = GenerateManifest(character, innerFolderpath);

                File.WriteAllText(Path.Combine(innerFolderpath, "character.json"), readyChar);
                File.WriteAllText(Path.Combine(folderName, "manifest.json"), readyManifest);
                
                ZipFile.CreateFromDirectory(folderName, Path.Combine(_outputFolder,"delis",charName+".deli"));
            }

            var delisFolder = Path.Combine(_outputFolder, "delis");
            if (Directory.Exists(delisFolder))
                Directory.Delete(delisFolder, true);
            Directory.CreateDirectory(delisFolder);

            var outputZip = Path.Combine(_outputFolder, "Chars.zip");
            if(File.Exists(outputZip))
                File.Delete(outputZip);
            ZipFile.CreateFromDirectory(delisFolder, outputZip);
        }

        private static string GenerateCharacter(MeatyCharacter character)
        {
            Console.WriteLine($"Processing {character.Name}.");
            string output = _characterFileTemplate;

            if (string.IsNullOrWhiteSpace(character.NameId))
            {
                character.NameId = character.Name.Replace(" ", string.Empty).ToLowerInvariant();
            }

            if (character.AmmoIdTiers.Count > 5)
            {
                throw new Exception($"AmmoIdTiers is bigger than 5!");
            }

            while (character.AmmoIdTiers.Count < 5)
            {
                Console.WriteLine(
                    $"AmmoIdTiers is only {character.AmmoIdTiers.Count}, replicating last one to fill up to 5.");
                character.AmmoIdTiers.Add(character.AmmoIdTiers.Last());
            }

            output = ReplaceFields(character, output);

            return output;
        }

        private static string GenerateManifest(MeatyCharacter character, string innerFolderpath)
        {
            string output = _manifestFileTemplate;
            output = ReplaceFields(character, output);

            if (!string.IsNullOrWhiteSpace(character.AdditionalContentPath))
            {
                List<string> sosigList = new List<string>();
                List<string> additionalContentList = Directory.GetFiles(character.AdditionalContentPath).ToList();
                // Copy all additional files needed
                foreach (var file in additionalContentList)
                {
                    if (Path.GetFileName(file).StartsWith("sosig"))
                    {
                        string sosig = File.ReadAllText(file);

                        sosig = ReplaceFields(character, sosig);

                        string newFileName = $"{Path.GetFileNameWithoutExtension(file)}-{character.NameId}.json";
                        File.WriteAllText(Path.Combine(innerFolderpath, newFileName), sosig);
                        sosigList.Add($"\"{character.Name}/{newFileName}\": \"h3vr.tnhtweaker.deli:sosig\"");
                    }

                    if (Path.GetFileName(file).EndsWith(".png"))
                    {
                        File.Copy(file, Path.Combine(innerFolderpath, Path.GetFileName(file)));
                    }
                }

                //TODO why tho?
                output = output.Replace(@":""_RemoveMe_""", string.Empty);

                string manifestSosigList = string.Join(",\n", sosigList.ToArray());
                output = Helpers.ReplaceInString(output, "\"_CustomSosigs_\"",
                    manifestSosigList);
            }

            return output;
        }
    }

    public static class Helpers
    {
        public static string ReplaceAndMultiply(string input, string fieldName, float baseNumber, float mult,
            bool floatAllowed = false)
        {
            string output = input;

            if (floatAllowed)
            {
                float toSet = baseNumber * mult;
                output = output.Replace(fieldName, toSet.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                int toSet = (int) (baseNumber * mult);
                output = output.Replace(fieldName, toSet.ToString());
            }

            return output;
        }

        // Remember to add any needed chars before to the value! For example, strings will have to be:
        // value = $"\"string\"";
        public static string ReplaceInString(string input, string fieldName, string value)
        {
            string output = input;

            output = output.Replace(fieldName, value);
            return output;
        }

        public static int ExtractNumber(string str) => int.Parse(str.Substring(0, str.IndexOf('_')));
    }
}