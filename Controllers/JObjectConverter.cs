﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoblegardenLauncherSharp.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NoblegardenLauncherSharp.Controllers
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

        private static NoblePatchModel ConvertTokenToPatch(JToken token) {
            NoblePatchModel patch = new NoblePatchModel();
            var reader = new JTokenReader(token);
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
                                patch.Hash = reader.ReadAsString();
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

        public static List<NoblePatchModel> ConvertToPatch(JObject Target) {
            var tokens = ConvertToTokenList(Target);
            var patches = new NoblePatchModel[tokens.Count];

            Parallel.For(0, tokens.Count, (i) => {
                patches[i] = ConvertTokenToPatch(tokens[i]);
                patches[i].Index = i;
            });

            return new List<NoblePatchModel>(patches);
        }
    }
}