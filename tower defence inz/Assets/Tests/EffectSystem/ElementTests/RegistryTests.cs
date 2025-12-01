using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using TDPG.EffectSystem.ElementRegistry;
using TDPG.EffectSystem.ElementLogic;
using TDPG.Generators.Seed;
using UnityEngine;

namespace Tests.EffectSystem.ElementTests
{
    [TestFixture, Category("EffectSystemTest")]
    public class RegistryTests
    {
        private Registry registry;

        [SetUp]
        public void SetUp()
        {
            registry = new Registry();
        }

        [TearDown]
        public void TearDown()
        {
            // clear after each test to avoid state bleed
            registry.ClearRegistry();
        }

        [Test]
        public void SetMutateSeedRule_ReturnsSetValue()
        {
            var ret = registry.SetMutateSeedRule(MutateTypes.Random);
            Assert.AreEqual(MutateTypes.Random, ret);
        }

        [Test]
        public void PutPreMadeElement_AddsElementAndCanRetrieveByNameAndId()
        {
            // create an element and add it under root (root id = 0)
            var e1 = new Element("E1", 1, new Seed(1, -1, "E1"));
            int added = registry.PutPreMadeElement(new List<int> { 0 }, e1);
            Assert.IsTrue(Convert.ToBoolean(added));

            // CountElements should be >= 2 (root + e1)
            Assert.GreaterOrEqual(registry.CountElements(), 2);

            // retrieval by name and id
            var byName = registry.GetElement("E1");
            Assert.IsNotNull(byName);
            Assert.AreEqual(1, byName.Id);

            var byId = registry.GetElement(1);
            Assert.IsNotNull(byId);
            Assert.AreEqual("E1", byId.Name);
        }

        [Test]
        public void GenerateChildElementFromParents_CreatesChildAndLinksToParents()
        {
            // Create two parents under root
            var p1 = new Element("P1", 1, new Seed(1, 0, "P1"));
            var p2 = new Element("P2", 2, new Seed(2, 0, "P2"));
            Assert.That(registry.PutPreMadeElement(new List<int> { 0 }, p1) > 0);
            Assert.IsTrue(registry.PutPreMadeElement(new List<int> { 0 }, p2) > 0);

            // Generate child using the two parents' IDs
            var child = registry.GenerateChildElementFromParents_Recombine(new List<int> { 1, 2 });
            Assert.IsNotNull(child);

            // Child should be retrievable and should appear as child of both parents
            var parentsList = new List<Element> { registry.GetElement(1), registry.GetElement(2) };
            var commonChildren = registry.GetElementsFromParents(parentsList).ToList();

            Assert.IsTrue(commonChildren.Any(c => c.Id == child.Id),
                "Generated child should be returned by GetElementsFromParents");

            // GetElement(parents) single-child version should return a matching child (or first of many)
            var single = registry.GetElement(parentsList);
            Assert.IsNotNull(single);
            Assert.IsTrue(parentsList.Any() && (single.Id == child.Id || commonChildren.Any(c => c.Id == single.Id)));
        }

        [Test]
        public void GetDescendants_DepthLimitsWork()
        {
            // create a chain: root -> A -> B
            var a = new Element("A", 1, new Seed(1, 0, "A"));
            registry.PutPreMadeElement(new List<int> { 0 }, a);

            var b = new Element("B", 2, new Seed(2, 0, "B"));
            registry.PutPreMadeElement(new List<int> { 1 }, b);

            // depth 1 from root should include A but not B
            var root = registry.GetElement("Root") ?? registry.GetElement(0); // fallback
            var d1 = registry.GetDescendants(root).ToList();
            Assert.IsTrue(d1.Any(e => e.Name == "A"));
            Assert.IsFalse(d1.Any(e => e.Name == "B"));

            // depth 2 should include both A and B
            var d2 = registry.GetDescendants(root, 2).ToList();
            Assert.IsTrue(d2.Any(e => e.Name == "A"));
            Assert.IsTrue(d2.Any(e => e.Name == "B"));
        }

        [Test]
        public void PrintFunctions_DoNotThrow()
        {
            // Add a small sample tree then call print functions to ensure they don't throw
            var a = new Element("PA", 1, new Seed(1, 0, "PA"));
            registry.PutPreMadeElement(new List<int> { 0 }, a);
            var b = new Element("PB", 2, new Seed(2, 0, "PB"));
            registry.PutPreMadeElement(new List<int> { 0 }, b);

            // call prints (no asserts — just ensure no exception)
            registry.PrintElementDetails("PA");
            registry.PrintRegistryMap();
        }

        [Test]
        public void ClearRegistry_ResetsCount()
        {
            var e = new Element("C1", 1, new Seed(1, 0, "C1"));
            registry.PutPreMadeElement(new List<int> { 0 }, e);
            Assert.GreaterOrEqual(registry.CountElements(), 2);

            registry.ClearRegistry();
            // After ClearRegistry we expect the registry to be initialized; CountElements should be >= 1
            Assert.GreaterOrEqual(registry.CountElements(), 1);
        }

        [Test]
        public void GetElement_ByNonexistentNameReturnsNull()
        {
            var notFound = registry.GetElement("NoSuchElement");
            Assert.IsNull(notFound);
        }
        /*
        [Test]
        public void ComplexGraph_MultiParentChildAndDescendantsBehaveCorrectly()
        {
            // Graph structure we’re building:
            //      Root
            //     / |  \
            //    A  |   B
            //     \ |  / \
            //       C     D
            //       |    /
            //       |   E
            //       |  /  
            //       F       

            // --- Setup ---
            var root = registry.RootElement;

            Effect slowDown = new SlowDown(2, 2);
            Effect hit = new  HealthDown(1 );
            
            var a = new Element("A", 1 ,new List<Effect>{slowDown});
            var b = new Element("B", 2, new List<Effect>{hit});
            var c = new Element("C", 3, new List<Effect>{slowDown, hit});
            var d = new Element("D", 4, new Seed(4, 0, "D"));
            var e = new Element("E", 5, new Seed(5, 0, "E"));
            var f = new Element("F", 6, new List<Effect>{slowDown, hit});

            // Root → A, B, C  (C has parents {Root, A, B})
            registry.PutPreMadeElement(new List<int> { root.Id }, a);
            registry.PutPreMadeElement(new List<int> { root.Id }, b);
            registry.PutPreMadeElement(new List<int> { root.Id, a.Id, b.Id }, c);
            
            // B → D
            registry.PutPreMadeElement(new List<int> { b.Id }, d);

            // D → E
            registry.PutPreMadeElement(new List<int> { d.Id }, e);

            // C, E → F  (unique parent set → allowed)
            registry.PutPreMadeElement(new List<int> { c.Id, e.Id }, f);

            // --- Test 1: Root descendants by depth ---
            var depth1 = registry.GetDescendants(root, 1).Select(e => e.Name).ToList();
            var depth2 = registry.GetDescendants(root, 2).Select(e => e.Name).ToList();
            var depth3 = registry.GetDescendants(root, 3).Select(e => e.Name).ToList();

            Assert.That(depth1, Is.EquivalentTo(new[] { "A", "B", "C" }));
            Assert.That(depth2, Does.Contain("D"));
            Assert.That(depth3, Does.Contain("E"));
            Assert.That(depth3, Does.Contain("F"));

            // --- Test 2: Common child lookups ---
            var commonAB = registry.GetElementsFromParents(new List<Element> { a, b }).ToList();
            Assert.That(commonAB.Count, Is.EqualTo(1));
            Assert.That(commonAB[0].Name, Is.EqualTo("C"));

            var commonCE = registry.GetElementsFromParents(new List<Element> { c, e }).ToList();
            Assert.That(commonCE.Count, Is.EqualTo(1));
            Assert.That(commonCE[0].Name, Is.EqualTo("F"));

            // --- Test 3: Parent-child relationships ---
            var cParents = registry.GetElementsFromParents(new List<Element> { a }).Concat(
                           registry.GetElementsFromParents(new List<Element> { b })).Select(x => x.Name).ToList();
            Assert.That(cParents, Does.Contain("C"));

            var dChildren = registry.GetDescendants(d, 1).Select(e => e.Name);
            Assert.That(dChildren, Does.Contain("E"));

            var eChildren = registry.GetDescendants(e, 1).Select(e => e.Name);
            Assert.That(eChildren, Does.Contain("F"));

            // --- Test 4: Print calls (should not throw) ---
            registry.PrintRegistryMap();
            registry.PrintElementDetails("F");

            // --- Test 5: Total count ---
            Assert.AreEqual(7, registry.CountElements(), "Registry should contain Root, A, B, C, D, E, F");
        }*/

        [Test]
        public void SimpleGraph_SerializeDeserialize()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter>
                {
                    new EffectConverter(),
                    new ElementConverter(),
                    new SeedConverter(),
                    new RegistryConverter()
                }
            };

            var registry = new Registry();
            var e1 = new Element("E1", 1, new Seed(1, -1, "E1"));
            int added = registry.PutPreMadeElement(new List<int> { 0 }, e1);
            Assert.That(added > 0 );

            registry.PrintRegistryMap();

            string json = JsonConvert.SerializeObject(registry, settings);
            Assert.NotNull(json);

            var deserialized = JsonConvert.DeserializeObject<Registry>(json, settings);
            Assert.IsNotNull(deserialized);
            deserialized.PrintRegistryMap();

            Debug.Log("Serialized registry\n" + json);

            // --- Basic sanity checks ---
            Assert.AreEqual(registry.CountElements(), deserialized.CountElements());

            // --- Map original elements by ID for comparison ---
            var originalElementsById = registry.GetAllElements().ToDictionary(e => e.Id);
            var deserializedElementsById = deserialized.GetAllElements().ToDictionary(e => e.Id);

            CollectionAssert.AreEquivalent(originalElementsById.Keys, deserializedElementsById.Keys);

            // --- Check edges/children match by IDs ---
            foreach (var origElement in originalElementsById.Values)
            {
                var origChildrenIds = registry.GetElementsFromParents(new List<Element> { origElement })
                                              .Select(e => e.Id)
                                              .OrderBy(id => id)
                                              .ToList();

                var deserElement = deserializedElementsById[origElement.Id];

                // Use IDs to fetch children in deserialized registry
                var deserChildrenIds = deserialized.GetElementsFromParents(
                                            new List<Element> { deserializedElementsById[origElement.Id] })
                                            .Select(e => e.Id)
                                            .OrderBy(id => id)
                                            .ToList();

                CollectionAssert.AreEqual(origChildrenIds, deserChildrenIds, 
                    $"Children mismatch for element ID {origElement.Id} ({origElement.Name})");
            }

            // --- Optional: check DNA and effects match ---
            foreach (var origElement in originalElementsById.Values)
            {
                var deserElement = deserializedElementsById[origElement.Id];

                Assert.AreEqual(origElement.GetDna().Value, deserElement.GetDna().Value);
                Assert.AreEqual(origElement.GetEffects().Count, deserElement.GetEffects().Count);

                for (int i = 0; i < origElement.GetEffects().Count; i++)
                {
                    var origEffect = origElement.GetEffects()[i];
                    var deserEffect = deserElement.GetEffects()[i];

                    Assert.AreEqual(origEffect.Name, deserEffect.Name);
                    CollectionAssert.AreEqual(origEffect.GetValues(), deserEffect.GetValues());
                }
            }
        }
        
        /*[Test]
        public void ComplexGraph_SerializeDeserialize()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter>
                {
                    new EffectConverter(),
                    new ElementConverter(),
                    new SeedConverter(),
                    new RegistryConverter()
                }
            };
            
            var registry = new Registry();
            var root = registry.RootElement;

            // --- Setup Effects ---
            Effect slowDown = new SlowDown(2, 2);
            Effect hit = new HealthDown(1);

            // --- Setup Elements ---
            var b = new Element("B", 0,  new List<Effect>{hit});
            var a = new Element("A", 0,  new List<Effect>{slowDown});
            var c = new Element("C", 0,  new List<Effect>{slowDown, hit});
            var d = new Element("D", 0, new Seed(4, 0, "D"));
            var e = new Element("E", 0, new Seed(5, 0, "E"));
            var f = new Element("F", 0, new List<Effect>{slowDown, hit});

            // --- Build Graph ---
            registry.PutPreMadeElement(new List<int> { root.Id }, a);
            registry.PutPreMadeElement(new List<int> { root.Id }, b);
            registry.PutPreMadeElement(new List<int> { root.Id , a.Id, b.Id }, c);
            registry.PutPreMadeElement(new List<int> { b.Id }, d);        // B → D
            registry.PutPreMadeElement(new List<int> { d.Id }, e);        // D → E
            registry.PutPreMadeElement(new List<int> { c.Id, e.Id }, f);  // C,E → F

            registry.PrintRegistryMap();
            
            // --- Serialize / Deserialize ---
            string json = JsonConvert.SerializeObject(registry, settings);
            Assert.NotNull(json);

            var deserialized = JsonConvert.DeserializeObject<Registry>(json, settings);
            Assert.IsNotNull(deserialized);
            deserialized.PrintRegistryMap();

            // --- Map elements by ID for easy lookup ---
            var origElementsById = registry.GetAllElements().ToDictionary(e => e.Id);
            var deserElementsById = deserialized.GetAllElements().ToDictionary(e => e.Id);

            CollectionAssert.AreEquivalent(origElementsById.Keys, deserElementsById.Keys);

            // --- Verify all edges / children ---
            foreach (var origElement in origElementsById.Values)
            {
                var origChildrenIds = registry.GetElementsFromParents(new List<Element> { origElement })
                    .Select(e => e.Id)
                    .OrderBy(id => id)
                    .ToList();

                var deserChildrenIds = deserialized.GetElementsFromParents(new List<Element> { deserElementsById[origElement.Id] })
                    .Select(e => e.Id)
                    .OrderBy(id => id)
                    .ToList();

                CollectionAssert.AreEqual(origChildrenIds, deserChildrenIds, 
                    $"Children mismatch for element ID {origElement.Id} ({origElement.Name})");
            }
            
            // --- verify effects ---
            foreach (var origElement in origElementsById.Values)
            {
                var deserElement = deserElementsById[origElement.Id];

                Assert.AreEqual(origElement.GetEffects().Count, deserElement.GetEffects().Count);

                for (int i = 0; i < origElement.GetEffects().Count; i++)
                {
                    var origEffect = origElement.GetEffects()[i];
                    var deserEffect = deserElement.GetEffects()[i];

                    Assert.AreEqual(origEffect.Name, deserEffect.Name);
                    CollectionAssert.AreEqual(origEffect.GetValues(), deserEffect.GetValues());
                }
            }
        }*/
        
        [Test]
        public void CannotAdd_SecondElementWithSameNonRootParents()
        {
            var a = new Element("A", 1, new Seed(1, 0, "A"));
            var b = new Element("B", 2, new Seed(2, 0, "B"));
            var x = new Element("X", 3, new Seed(3, 0, "X"));
            var y = new Element("Y", 4, new Seed(4, 0, "Y"));

            registry.PutPreMadeElement(new List<int> { 0 }, a);
            registry.PutPreMadeElement(new List<int> { 0 }, b);

            // X has parents {A, B}
            Assert.IsTrue(registry.PutPreMadeElement(new List<int> { 1, 2 }, x) > 0);

            // Y tries the same → should fail
            Assert.IsFalse(registry.PutPreMadeElement(new List<int> { 1, 2 }, y) > 0);
        }
        
        [Test]
        public void ReassignExistingElementToNewParentSet_Fails()
        {
            var a = new Element("A", 1, new Seed(1, 0, "A"));
            var b = new Element("B", 2, new Seed(2, 0, "B"));

            registry.PutPreMadeElement(new List<int> { 0 }, a);

            // A exists under {Root}
            Assert.IsFalse(registry.PutPreMadeElement(new List<int> { 2 }, a) > 0 );
        }

        [Test]
        public void Allow_MultipleChildrenOfRootOnly()
        {
            for (int i = 1; i <= 10; i++)
            {
                var e = new Element("R" + i, i, new Seed((ulong)i, 0, "R" + i));
                Assert.That(registry.PutPreMadeElement(new List<int> { 0 }, e) > 0);
            }

            Assert.AreEqual(11, registry.CountElements()); // root + 10 elems
        }

        [Test]
        public void GenerateChild_FailsIfParentSetAlreadyUsed()
        {
            var a = new Element("A", 1, new Seed(1, 0, "A"));
            var b = new Element("B", 2, new Seed(2, 0, "B"));

            registry.PutPreMadeElement(new List<int> { 0 }, a);
            registry.PutPreMadeElement(new List<int> { 0 }, b);

            // First child with parent-set {A, B}
            var first = registry.GenerateChildElementFromParents_Addition(new List<int> { 1, 2 });
            Assert.IsNotNull(first);

            // Second attempt → must fail, return null
            var second = registry.GenerateChildElementFromParents_Recombine(new List<int> { 1, 2 });
            Assert.IsNull(second);
        }

        [Test]
        public void CircularParentAttempt_FailsGracefully()
        {
            var a = new Element("A", 1, new Seed(1, 0, "A"));
            var b = new Element("B", 2, new Seed(2, 0, "B"));

            registry.PutPreMadeElement(new List<int> { 0 }, a);
            registry.PutPreMadeElement(new List<int> { 1 }, b);

            // Attempt to add A as a child of B → would create cycle
            Assert.That(registry.PutPreMadeElement(new List<int> { 2 }, a) == 0);  //is false '==0'
        }

        [Test]
        public void Descendants_IgnoreRejectedParentInsert()
        {
            var a = new Element("A", 1, new Seed(1, 0, "A"));
            var b = new Element("B", 2, new Seed(2, 0, "B"));
            var c = new Element("C", 3, new Seed(3, 0, "C"));

            registry.PutPreMadeElement(new List<int> { 0 }, a);
            registry.PutPreMadeElement(new List<int> { 0 }, b);
            registry.PutPreMadeElement(new List<int> { 1 }, c); // C under A

            // Try to reassign C under B (should fail)
            Assert.That(registry.PutPreMadeElement(new List<int> { 2 }, c) == 0);

            // Descendants of B should NOT include C
            var bChildren = registry.GetDescendants(registry.GetElement(2), 1);
            Assert.IsEmpty(bChildren);
        }

        [Test]
        public void RegistryRejects_InvalidParentId()
        {
            var x = new Element("X", 1, new Seed(1, 0, "X"));

            // parent -99 does not exist
            Assert.That(registry.PutPreMadeElement(new List<int> { -99 }, x) == 0);
        }

        /*[Test]
        public void Registry_Genetic_Test()
        {
            registry.SetMutateSeedRule(MutateTypes.None);

            // --- Setup Effects ---
            Effect slowDown = new SlowDown(2, 3);
            Effect hit = new HealthDown(1);
            Effect heal = new Heal(5);

            // --- Setup Elements ---
            var a = new Element("WatterBubble", 1,  new List<Effect>{heal}); // 4
            var b = new Element("Ice", 2,  new List<Effect>{slowDown, hit}); // 3
            var c = new Element("Fireball",3,  new List<Effect>{hit}); // 2
            
            registry.PutPreMadeElement(new List<int> { 0 }, a);
            registry.PutPreMadeElement(new List<int> { 0 }, b);
            registry.PutPreMadeElement(new List<int> { 0 }, c);
            
            // --- Addition Child ---
            Element newElement = registry.GenerateChildElementFromParents_Addition(new List<int> { 1, 2, 3 });
            List<Effect> effects = newElement.GetEffects();

            // Assert that newElement has all the expected effects
            Assert.IsTrue(effects.Any(e => e is SlowDown), "Child should have SlowDown effect.");
            Assert.IsTrue(effects.Any(e => e is HealthDown), "Child should have HealthDown effect.");
            Assert.IsTrue(effects.Any(e => e is Heal), "Child should have Heal effect.");
            
            // --- Recombine Child (XOR-like) ---
            Element newElementXor = registry.GenerateChildElementFromParents_Recombine(new List<int> { 2, 3 });
            List<Effect> xorEffects = newElementXor.GetEffects();
            
            // Assert recombine behavior
            Assert.IsTrue(xorEffects.Any(e => e is SlowDown), "XOR child should have SlowDown.");
            Assert.IsFalse(xorEffects.Any(e => e is HealthDown), "XOR child should NOT have HealthDown.");
            Assert.IsFalse(xorEffects.Any(e => e is Heal), "XOR child should NOT have Heal.");
            
            // Assert recombined effect parameters are within parent bounds
            foreach (var effect in xorEffects)
            {
                Debug.Log(effect.Name+effect.Description);
                var parentValuesList = new List<float[]>();

                foreach (var parentId in new List<int> { 2, 3 })
                {
                    parentValuesList.AddRange(
                        registry.GetElement(parentId).GetEffects()
                            .Where(e => e.GetType() == effect.GetType())
                            .Select(e => e.GetValues())
                    );
                }

                // For each parameter in this effect, check it doesn't exceed max among parents
                for (int i = 0; i < effect.GetValues().Length; i++)
                {
                    float maxParentValue = parentValuesList.Count > 0
                        ? parentValuesList.Max(vals => vals[i])
                        : float.MinValue;

                    Assert.LessOrEqual(effect.GetValues()[i], maxParentValue,
                        $"Effect {effect.GetType().Name} param {i} should not exceed parent's maximum.");
                }
            }

            
            // --- Mutation Test ---
            registry.SetMutateSeedRule(MutateTypes.Random);
            Element lastElement = registry.GenerateChildElementFromParents_Addition(new List<int> { 1, 2 });

            Seed dna = lastElement.GetDna();
            Seed aPlusB = a.GetDna() + b.GetDna();

            // Assert that mutation produced a different DNA
            Assert.AreNotEqual(aPlusB, dna, "Mutation should produce different DNA than simple addition.");
            
        }*/

        
    }
}
