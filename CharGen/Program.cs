using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

        // Values to replace
        //
        // Calculate values:
        // _MinTargetsEndless1_
        // _MaxTargetsEndless1_
        // _AmmoShopPremium_
        // _AmmoShopCheap_
        // 10_EncryptionsAmount_
        //
        // Only replace values

        private static readonly string[] _optionalStrings;

        private static string _manifestFileTemplate;
        private static string _characterFileTemplate;
        private static string _folderName;
        private static string _innerFolderpath;

        private static List<MeatyCharacter> _toCreate;

        static void Main(string[] args)
        {
            if (args.Length != 3)
                return;

            _toCreate = JsonConvert.DeserializeObject<List<MeatyCharacter>>(File.ReadAllText(args[0]));
            _characterFileTemplate = File.ReadAllText(args[1]);
            _manifestFileTemplate = File.ReadAllText(args[2]);
            var test = File.ReadAllText(args[2]);

            foreach (var character in _toCreate)
            {
                string readyChar = GenerateCharacter(character);

                _folderName = character.Name.Replace(" ", string.Empty) + character.Version.ToString();

                if (Directory.Exists(_folderName))
                    throw new Exception($"Directory {_folderName} already exists!");

                _innerFolderpath = Path.Combine(_folderName, character.Name);
                if (Directory.Exists(_innerFolderpath))
                    throw new Exception($"Directory {_innerFolderpath} already exists!");
                Directory.CreateDirectory(_innerFolderpath);

                string readyManifest = GenerateManifest(character);

                File.WriteAllText(Path.Combine(_innerFolderpath, "character.json"), readyChar);
                File.WriteAllText(Path.Combine(_folderName, "manifest.json"), readyManifest);
            }
        }

        private static string GenerateCharacter(MeatyCharacter character)
        {
            Console.WriteLine($"Processing {character.Name}.");
            string output = _characterFileTemplate;
            // Replacing additional options
            foreach (var opt in character.AdditionalOptions)
            {
                if (string.IsNullOrWhiteSpace(opt.Value))
                {
                    continue;
                }

                if (!_optionalStrings.Contains(opt.Key))
                {
                    continue;
                }

                output = Helpers.ReplaceInString(output, opt.Key, opt.Value, false);
            }

            output = Helpers.ReplaceInString(output, "NameId",
                $"{character.Name.ToLowerInvariant().Replace(" ", string.Empty)}");
            output = Helpers.ReplaceInString(output, "Name", $"\"{character.Name}\"");
            output = Helpers.ReplaceInString(output, "Description", $"\"{character.Description}\"");
            output = Helpers.ReplaceInString(output, "SignatureWeaponId", $"\"{character.SignatureWeaponId}\"");
            output = Helpers.ReplaceInString(output, "SignatureMeleeId", $"\"{character.SignatureMeleeId}\"");

            output = Helpers.ReplaceInString(output, "StartingMagsAmount", $"{character.StartingMagsAmount}");

            
            output = Helpers.ReplaceInString(output, "MagazineIds", $"[{string.Join(',', character.MagazineIds.Select(x => $"\"{x}\""))}]");

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

            for (int i = 0; i < 5; i++)
            {
                output = Helpers.ReplaceInString(output, $"AmmoTier{i}", $"\"{character.AmmoIdTiers[i]}\"");
            }

            output = Helpers.ReplaceAndMultiply(output, "AmmoShopCheap", character.AmmoShopMultiplier);
            output = Helpers.ReplaceAndMultiply(output, "AmmoShopPremium", character.AmmoShopMultiplier);

            output = Helpers.ReplaceAndMultiply(output, "EncryptionsModifier", character.EncryptionsModifier);

            return output;
        }

        private static string GenerateManifest(MeatyCharacter character)
        {
            string output = _manifestFileTemplate;
            output = Helpers.ReplaceInString(output, "Name", $"\"{character.Name}\"");
            output = Helpers.ReplaceInString(output, "NameId",
                $"{character.Name.ToLowerInvariant().Replace(" ", string.Empty)}", false);
            output = Helpers.ReplaceInString(output, "Version", $"\"{character.Version}\"");

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

                        //TODO: Apply sosig multipliers
                        sosig = Helpers.ReplaceAndMultiply(sosig, "\"SosigHpModifier\"", character.SosigHpModifier, ignoreIfNotFound:true, floatAllowed:true);
                        sosig = Helpers.ReplaceAndMultiply(sosig, "\"SosigSpeedModifier\"", character.SosigSpeedModifier, ignoreIfNotFound:true, floatAllowed:true);

                        File.WriteAllText(Path.Combine(_innerFolderpath, Path.GetFileName(file)), sosig);

                        //TODO: Add sosig file to manifest list
                        sosigList.Add($"\"{character.Name}/{Path.GetFileName(file)}\": \"h3vr.tnhtweaker.deli:sosig\"");
                    }

                    if (Path.GetFileName(file).EndsWith(".png"))
                    {
                        File.Copy(file, Path.Combine(_innerFolderpath, Path.GetFileName(file)));
                    }
                }
                
                //TODO why tho?
                output = output.Replace(@":""_RemoveMe_""", string.Empty);
                
                string manifestSosigList = string.Join(",\n", sosigList.ToArray());
                output = Helpers.ReplaceInString(output, "CustomSosigs",
                    manifestSosigList);
            }

            return output;
        }
    }

    public static class Helpers
    {
        public static string ReplaceAndMultiply(string input, string fieldName, float mult,
            bool floatAllowed = false, bool ignoreIfNotFound=false)
        {
            int lastIndex = input.IndexOf($"{fieldName}_\"", StringComparison.Ordinal);

            if (lastIndex == -1)
            {
                if (!ignoreIfNotFound)
                {
                    throw new KeyNotFoundException("Value not found!");
                }
                else
                {
                    return input;
                }
            }

            lastIndex = lastIndex - 1;
            int currentIndex = lastIndex;
            if (currentIndex == 0)
            {
                throw new KeyNotFoundException("It starts actually at index 0, no place for a number!");
            }

            char currentChar;
            currentIndex = currentIndex;
            //TODO! EW YUCKY
            while (true)
            {
                currentChar = input[currentIndex - 1];

                if (int.TryParse(currentChar.ToString(), out int result) || currentChar == '.' || currentChar == ',')
                {
                    currentIndex = currentIndex - 1;
                }
                else
                {
                    break;
                }
            }

            string number = input.Substring(currentIndex, lastIndex - currentIndex).Replace('.',',');
            if (!float.TryParse(number, out float resultF))
            {
                throw new Exception($"Couldnt parse {number}!");
            }

            if (floatAllowed)
            {
                int toSet = (int) (resultF * mult);
                input = ReplaceInString(input, fieldName, toSet.ToString(), false);
                return input;
            }
            else
            {
                float toSet = resultF * mult;
                input = ReplaceInString(input, fieldName, toSet.ToString(CultureInfo.InvariantCulture), false);
                return input;
            }
        }

        // Remember to add any needed chars before to the value! For example, strings will have to be:
        // value = $"\"string\"";
        public static string ReplaceInString(string input, string fieldName, string value, bool includeEars = true)
        {
            string toReplace = $"_{fieldName}_";
            if (includeEars)
                toReplace = $"\"{toReplace}\"";

            input = input.Replace(toReplace, value);
            return input;
        }

        public static int ExtractNumber(string str) => int.Parse(str.Substring(0, str.IndexOf('_')));
    }
}