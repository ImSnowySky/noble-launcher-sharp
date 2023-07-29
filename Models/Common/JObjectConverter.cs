using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NobleLauncher.Models
{
    class JObjectConverter
    {
        public static List<JToken> ConvertToTokenList(JObject Target) {
            var convertedList = new List<JToken>();

            int tokensCount = Target.Count;

            if (Target.Count == 0)
                return convertedList;

            int appendedTokensCount = 1;
            convertedList.Add(Target.First);

            while (appendedTokensCount < tokensCount) {
                convertedList.Add(convertedList[appendedTokensCount - 1].Next);
                appendedTokensCount++;
            }

            return convertedList;
        }

        private static PatchModel ConvertTokenToPatch(JToken Token) {
            PatchModel patch = new PatchModel();
            var reader = new JTokenReader(Token);
            bool isObjectStarted = false;

            while (reader.Read()) {
                var tokenType = reader.TokenType;

                if (tokenType == JsonToken.StartObject) {
                    isObjectStarted = true;
                }

                if (tokenType == JsonToken.PropertyName) {
                    string property = (string)reader.Value;

                    if (isObjectStarted) {
                        switch (property) {
                            case "path":
                                patch.RemotePath = reader.ReadAsString();
                                break;
                            case "crc32-hash":
                                patch.RemoteHash = reader.ReadAsString();
                                break;
                            case "description":
                                patch.Description = reader.ReadAsString();
                                break;
                            default:
                                break;
                        }
                    }
                    else {
                        patch.LocalPath = property;
                    }
                }
            }
            patch.CalcNameFromPath();
            return patch;
        }

        public static List<NecessaryPatchModel> ConvertToNecessaryPatchesList(JObject Target) {
            var tokens = ConvertToTokenList(Target);
            var patches = new NecessaryPatchModel[tokens.Count];

            Parallel.For(0, tokens.Count, (i) => {
                var patch = ConvertTokenToPatch(tokens[i]);
                patch.Index = i;
                var patchAsNecessaryPatch = patch.ToNecessaryPatch();
                patches[i] = patchAsNecessaryPatch;
            });

            return new List<NecessaryPatchModel>(patches);
        }

        public static List<CustomPatchModel> ConvertToCustomPatchesList(JObject Target) {
            var tokens = ConvertToTokenList(Target);
            var patches = new CustomPatchModel[tokens.Count];

            Parallel.For(0, tokens.Count, (i) => {
                var patch = ConvertTokenToPatch(tokens[i]);
                patch.Index = i;
                var customPatch = patch.ToCustomPatchModel();
                patches[i] = customPatch;
            });

            return new List<CustomPatchModel>(patches);
        }
    }
}
