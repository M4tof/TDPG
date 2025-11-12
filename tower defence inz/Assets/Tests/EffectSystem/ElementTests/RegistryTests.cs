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
    [TestFixture]
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
            bool added = registry.PutPreMadeElement(new List<int> { 0 }, e1);
            Assert.IsTrue(added);

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
            Assert.IsTrue(registry.PutPreMadeElement(new List<int> { 0 }, p1));
            Assert.IsTrue(registry.PutPreMadeElement(new List<int> { 0 }, p2));

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
            
            var a = new Element("A", 1, new Seed(1, 0, "A"),new List<Effect>{slowDown});
            var b = new Element("B", 2, new Seed(2, 0, "B"), new List<Effect>{hit});
            var c = new Element("C", 3, new Seed(3, 0, "C"),new List<Effect>{slowDown, hit});
            var d = new Element("D", 4, new Seed(4, 0, "D"));
            var e = new Element("E", 5, new Seed(5, 0, "E"));
            var f = new Element("F", 6, new Seed(6, 0, "F"),new List<Effect>{slowDown, hit});

            // Root → A, B, C
            registry.PutPreMadeElement(new List<int> { root.Id }, a);
            registry.PutPreMadeElement(new List<int> { root.Id }, b);
            registry.PutPreMadeElement(new List<int> { root.Id }, c);

            // A → C (reinforce)
            registry.PutPreMadeElement(new List<int> { a.Id }, c);

            // B → C, D
            registry.PutPreMadeElement(new List<int> { b.Id }, c);
            registry.PutPreMadeElement(new List<int> { b.Id }, d);

            // D → E
            registry.PutPreMadeElement(new List<int> { d.Id }, e);

            // C, E → F (multi-parent)
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
        }

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
            bool added = registry.PutPreMadeElement(new List<int> { 0 }, e1);
            Assert.IsTrue(added);

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
        
        [Test]
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
            var a = new Element("A", 0, new Seed(1, 0, "A"), new List<Effect>{slowDown});
            var b = new Element("B", 0, new Seed(2, 0, "B"), new List<Effect>{hit});
            var c = new Element("C", 0, new Seed(3, 0, "C"), new List<Effect>{slowDown, hit});
            var d = new Element("D", 0, new Seed(4, 0, "D"));
            var e = new Element("E", 0, new Seed(5, 0, "E"));
            var f = new Element("F", 0, new Seed(6, 0, "F"), new List<Effect>{slowDown, hit});

            // --- Build Graph ---
            registry.PutPreMadeElement(new List<int> { root.Id }, a);
            registry.PutPreMadeElement(new List<int> { root.Id }, b);
            registry.PutPreMadeElement(new List<int> { root.Id }, c);

            registry.PutPreMadeElement(new List<int> { a.Id, b.Id }, c);  // A → C // B → C
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
        }
        
        
        
        
        
        
        
    }
}
