using UnityEngine;

namespace TDPG.Generators.Seed
{
    public class GlobalSeedGameObject : MonoBehaviour
    {
        [SerializeField] private ulong initVal;

        private GlobalSeed _globalSeed;
        private int _seedsCreated = 0;

        private void Awake()
        {
            _globalSeed = new GlobalSeed(initVal);    
        }

        public Seed GetNextSeed()
        {
            _seedsCreated++;
            return _globalSeed.NextSubSeed("DebugKey_WillResultInDeterminism");
        }

        public Seed RetrieveSeed(int id)
        {
            if (id < 0 || id > _seedsCreated)
            {
                return null;
            }
            return _globalSeed.GetSubSeed(id);
        }
    }
}