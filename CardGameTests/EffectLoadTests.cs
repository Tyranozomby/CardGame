﻿using System;
using System.IO;
using CardGameEngine.GameSystems.Effects;
using CardGameEngine.GameSystems.Targeting;
using NUnit.Framework;

namespace CardGameTests
{
    [TestFixture]
    public class Tests
    {
        private EffectsDatabase _effectsDatabase;

        private readonly string _randomDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Split('.')[0]);

        [SetUp]
        public void SetUp()
        {
            Directory.CreateDirectory(_randomDir);
            _effectsDatabase = new EffectsDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_randomDir, true);
        }


        private string PutScript(string script,EffectType type)
        {
            var typePath = Path.Combine(_randomDir, type.ToString());
            Directory.CreateDirectory(typePath);
            
            var filePath = Path.Combine(typePath, Path.GetRandomFileName());
            using (var f = new StreamWriter(File.Create(filePath)))
            {
                f.Write(script);
            }

            return filePath;
        }


        [TestCase(ExampleCardScript, TestName = nameof(ExampleCardScript))]
        [TestCase(ExampleCardScriptWithOnLevelUp, TestName = nameof(ExampleCardScriptWithOnLevelUp))]
        // [TestCaseSource("ExampleArtefactScript")]
        // [TestCaseSource("ExampleKeywordScript")]
        [Test]
        public void Test_Load_Effect_Good_Example(string script)
        {
            var path = PutScript(script,EffectType.Card);
            Console.WriteLine(script);
            Assert.DoesNotThrow(() => _effectsDatabase.LoadAllEffects(_randomDir)
                , "Les effets sont bien formés et ne doivent pas être rejetés");
        }

        [Test]
        [TestCase(ExampleCardScriptWithError, TestName = nameof(ExampleCardScriptWithError))]
        [TestCase("", TestName = "BlankScript")]
        [TestCase("dqdqs", TestName = "GarbageScript")]
        public void Test_Load_Effect_Bad_Example(string script)
        {
            var path = PutScript(script,EffectType.Card);
            Console.WriteLine(script);
            Assert.Throws<InvalidEffectException>(() => _effectsDatabase.LoadAllEffects(_randomDir)
                , "Les effets sont invalides et doivent être rejetés");
        }

        [Test]
        public void Test_Effect_Has_Data()
        {
            var path = Path.GetFileNameWithoutExtension(PutScript(ExampleCardScript,EffectType.Card));
            _effectsDatabase.LoadAllEffects(_randomDir);
            var eft = _effectsDatabase[path];
            Assert.NotNull(eft);

            Assert.AreEqual(EffectType.Card, eft.EffectType);
            Assert.AreEqual(path, eft.EffectId);
            // Assert.IsNotEmpty(eft.AllTargets);
            Assert.That(eft, Has.Property(nameof(Effect.AllTargets)).Not.Empty);

            Assert.AreEqual(2, eft.AllTargets.Count);

            var tgt = eft.AllTargets[0];
            Assert.NotNull(tgt);
            Assert.AreEqual("Une cible carte", tgt.Name);
            Assert.AreEqual(TargetTypes.Card, tgt.TargetType);
            Assert.IsFalse(tgt.IsAutomatic);

            var tgp = eft.AllTargets[1];
            Assert.NotNull(tgp);
            Assert.AreEqual("Un joueur", tgp.Name);
            Assert.AreEqual(TargetTypes.Player, tgp.TargetType);
            Assert.IsTrue(tgp.IsAutomatic);
        }

        #region testData

        private const string ExampleCardScript = @"max_level = 2
image_id = 500


name = ""Nom""
        pa_cost = 2

        targets = {
            -- Nom, Type, Automatique ou non,Fonction de filtre des cibles potentielles
            CreateTarget(""Une cible carte"", TargetTypes.Card, false, cardFilter),
            CreateTarget(""Un joueur"", TargetTypes.Player, true),
        }

        function card_filter(a_card)
            -- permet uniquement le ciblage de carte ayant comme nom 'Exemple'
        return a_card.Name == ""Exemple""
        end 

        -- fonction qui renvoie un booléen si la carte peut être jouée ou non
            function precondition()
            -- la carte peut être jouée sans aucun critère spécifiques
        return true
        end 

            function description()
            return ""une description de la carte qui peut changer""
        end

            function do_effect()
            -- le code de l'effet de la carte
        end";

        private const string ExampleCardScriptWithOnLevelUp = ExampleCardScript + @"
        function on_level_up()

        end
        ";

        private const string ExampleCardScriptWithError = @"max_level = ""oui""
image_id = ""a""


name = 5
        pa_cost = ""oui""

        targets = {

            CreateTarget(""Une cible carte"", TargetTypes.Card, false, card_filter),

            CreateTarget(""Un joueur"", TargetTypes.Player, true),
        }

        function card_filter(a_card)
            -- permet uniquement le ciblage de carte ayant comme nom 'Exemple'
        return a_card.Name == ""Exemple""
        end 

        -- fonction qui renvoie un booléen si la carte peut être jouée ou non
            function precondition()
            -- la carte peut être jouée sans aucun critère spécifiques
        return true
        end 

            function description()
            return ""une description de la carte qui peut changer""
        end

            function do_effect()
            -- le code de l'effet de la carte
        end";

        #endregion
    }
}