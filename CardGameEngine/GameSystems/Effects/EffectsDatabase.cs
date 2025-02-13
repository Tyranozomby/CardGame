﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CardGameEngine.Cards;
using CardGameEngine.GameSystems.Targeting;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Serialization;
using MoonSharp.VsCodeDebugger;

namespace CardGameEngine.GameSystems.Effects
{
    /// <summary>
    /// Classe lisant, vérifiant et stockant tous les effets valides
    /// </summary>
    internal class EffectsDatabase
    {
        private readonly Action<string, string, string> _luaDebugPrint;

        /// <summary>
        /// Le dictionnaire stockant les effets valides avec leur nom comme clé
        /// </summary>
        private readonly Dictionary<string, EffectSupplier>
            _effectDictionary = new Dictionary<string, EffectSupplier>();

        private Dictionary<string, string> _scripts = new Dictionary<string, string>();

        internal delegate Effect EffectSupplier();

        private static MoonSharpVsCodeDebugServer vsdebug = new MoonSharpVsCodeDebugServer();

        /// <summary>
        /// Accède au dictionnaire des effets de l'objet
        /// </summary>
        /// <param name="s">Nom de l'effet</param>
        internal EffectSupplier this[string s] => _effectDictionary[s];

        internal EffectSupplier Blank { get; }


        /// <summary>
        /// Méthode principale de la classe, elle permet de charger tous les effets
        /// Redirige vers l'autre méthode LoadAllEffects en fonction du type des effets du sous-dossier
        /// </summary>
        /// <param name="path">Nom complet du dossier</param>
        /// <param name="luaDebugPrint"></param>
        /// <param name="debuggerToUse"></param>
        /// <seealso cref="LoadAllEffects(string, EffectType)"/>
        internal EffectsDatabase(string path, Action<string, string, string>? luaDebugPrint = null)
        {
            _luaDebugPrint = luaDebugPrint ?? ((from, script, s) => { });
            UserData.RegistrationPolicy = InteropRegistrationPolicy.Automatic;
            Table dump = UserData.GetDescriptionOfRegisteredTypes(true);
            File.WriteAllText("./dump.txt", dump.Serialize());

            foreach (var directory in Directory.EnumerateDirectories(path))
            {
                var validFile = Enum.TryParse(Path.GetFileName(directory), out EffectType type);
                if (validFile)
                    LoadAllEffects(directory, type);
            }

            if (_effectDictionary.ContainsKey("_blank"))
                Blank = _effectDictionary["_blank"];
            else
                _luaDebugPrint("C#", "EffectsDatabase.cs", "Blank effect not found");
        }

        /// <summary>
        /// Méthode chargeant tous les effets contenus dans un dossier
        /// </summary>
        /// <param name="path">Nom complet du dossier</param>
        /// <param name="effectType">Type de l'effet (correspond aussi au nom du dossier)</param>
        /// <seealso cref="LoadEffect(string, EffectType)"/>
        private void LoadAllEffects(string path, EffectType effectType)
        {
            foreach (var file in Directory.EnumerateFiles(path)
                         .Where(f => Path.GetExtension(f) == ".lua"))
            {
                LoadEffect(file, effectType);
            }
        }

        /// <summary>
        /// Méthode lisant et vérifiant la validité d'un script puis le stocke
        /// </summary>
        /// <param name="path">Nom complet du fichier</param>
        /// <param name="effectType">Type de l'effet</param>
        /// <exception cref="InvalidEffectException">Si l'effet est invalide</exception>
        /// <seealso cref="EffectChecker"/>
        private void LoadEffect(string path, EffectType effectType)
        {
            // ID de l'effet (le nom du fichier sans extension)
            string effectId = Path.GetFileNameWithoutExtension(path);
            var fileContent = File.ReadAllText(path);

            // Teste la validité de l'effet
            if (!EffectChecker.CheckEffect(path, effectType, out var error))
            {
                throw error switch
                {
                    InterpreterException exc => new InvalidEffectException(effectId, effectType, exc, fileContent),
                    _ => new InvalidEffectException(effectId, effectType, fileContent, error.ToString())
                };
            }

            _scripts[effectId] = fileContent;
            _effectDictionary[effectId] = () =>
            {
                // Charge le script de l'effet
                var script = GetDefaultScript();
                script.Options.DebugPrint = s => _luaDebugPrint("Lua", effectId, s);


                script.DoString(fileContent, codeFriendlyName: effectId);

                // Récupère les cibles de l'effet
                var targets = script.Globals.Get("targets").CheckType(nameof(LoadAllEffects), DataType.Table)
                    .Table.Values
                    .Select(t => t.UserData.Object)
                    .Cast<Target>()
                    .ToList();

                // Enregistre l'effet dans le dictionnaire
                return new Effect(effectType, effectId, targets, script);
            };
        }

        /// <summary>
        /// Méthode simple permettant de récupérer le script avec l'intégration du c# nécessaire
        /// </summary>
        /// <returns>L'objet script créé</returns>
        internal static Script GetDefaultScript()
        {
            var script = new Script
            {
                // Élements c# à intégrer dans le fichier lua
                Globals =
                {
                    ["CreateTarget"] = (Func<string, TargetTypes, bool, Closure?, Target>)CreateTarget,
                    ["TargetTypes"] = UserData.CreateStatic<TargetTypes>(),
                    ["ChainMode"] = UserData.CreateStatic<ChainMode>()
                }
            };
            return script;
        }

        /// <summary>
        /// Méthode de création d'un objet Target
        /// </summary>
        /// <returns>Un objet Target</returns>
        /// <seealso cref="Target(string, TargetTypes, bool, Closure?)"/>
        private static Target CreateTarget(string targetName, TargetTypes targetType, bool isAutomatic,
            Closure? cardFilter = null)
        {
            return new Target(targetName, targetType, isAutomatic, cardFilter);
        }

        public string? GetScript(string effectName)
        {
            return _scripts.ContainsKey(effectName) ? _scripts[effectName] : null;
        }
    }
}