using System.Collections.Generic;
using System.Linq; // Needed for sorting
using UnityEngine;
using ColorThief;
using Color = UnityEngine.Color;

namespace TDPG.VideoGeneration
{
    [ExecuteAlways]
    public class ColorSwapController_1Dominant : MonoBehaviour
    {
        [Header("Settings")] 
        public Color targetColor = Color.red;
        [Range(0, 10)] public float tolerance = 0.01f;
        
        [Header("Selection Logic")]
        [Tooltip("0 = Best Match, 1 = Second Best (useful if outline is picked), etc.")]
        [Min(0)] public int colorRankIndex = 0;

        [Tooltip("If true, picks the most colorful (saturated) color from the palette instead of the most frequent one.")]
        public bool prioritizeSaturated = true;
        [Tooltip("If true, ignores colors that are very close to black or white.")]
        public bool ignoreBlackAndWhite = true;

        [Header("Debug Info")]
        [SerializeField] private Color calculatedOriginalColor = Color.white;

        private Renderer _renderer;
        private MaterialPropertyBlock _propBlock;

        private static readonly int OriginalColorID = Shader.PropertyToID("_OriginalColor");
        private static readonly int TargetColorID = Shader.PropertyToID("_TargetColor");
        private static readonly int ToleranceID = Shader.PropertyToID("_Tolerance");

        // Helper struct to sort colors
        private struct ColorCandidate
        {
            public Color color;
            public float score;
            public int originalIndex;
        }

        void OnEnable()
        {
            RecalculateDominantColor();
            UpdateShaderProperties();
        }

        void OnValidate()
        {
            UpdateShaderProperties();
        }

        [ContextMenu("Force Recalculate Color")]
        public void RecalculateDominantColor()
        {
            if (_renderer == null) _renderer = GetComponent<Renderer>();
            if (_renderer == null) return;

            Texture2D textureToAnalyze = null;
            bool createdTempTexture = false;

            // 1. Get Texture (Cropped if Sprite)
            if (_renderer is SpriteRenderer spriteRen && spriteRen.sprite != null)
            {
                textureToAnalyze = GetCroppedTexture(spriteRen.sprite);
                createdTempTexture = true;
            }
            else if (_renderer.sharedMaterial != null)
            {
                textureToAnalyze = _renderer.sharedMaterial.mainTexture as Texture2D;
            }

            if (textureToAnalyze == null)
            {
                Debug.LogWarning($"[ColorSwapDebug] Could not find texture on '{name}'.");
                return;
            }

            Debug.Log($"[ColorSwapDebug] Analyzing: {textureToAnalyze.name}");

            try
            {
                var thief = new ColorThief.ColorThief();
                var palette = thief.GetPalette(textureToAnalyze, 10); // Get top 10
                
                List<ColorCandidate> candidates = new List<ColorCandidate>();

                // 2. Score and Filter
                for (int i = 0; i < palette.Count; i++)
                {
                    Color c = palette[i].UnityColor;
                    Color.RGBToHSV(c, out float h, out float s, out float v);

                    bool isTransparent = c.a < 0.9f;
                    bool isTooDark = v < 0.1f;    
                    bool isTooBright = s < 0.05f && v > 0.9f; 
                    
                    if (isTransparent) continue;
                    if (ignoreBlackAndWhite && (isTooDark || isTooBright)) continue;

                    // Score calculation
                    float score = prioritizeSaturated ? s : (100f - i);

                    candidates.Add(new ColorCandidate { color = c, score = score, originalIndex = i });
                }

                // 3. Sort by Score (Descending)
                candidates.Sort((a, b) => b.score.CompareTo(a.score));

                if (candidates.Count > 0)
                {
                    // Clamp index to prevent out of range errors if palette is small
                    int safeIndex = Mathf.Clamp(colorRankIndex, 0, candidates.Count - 1);
                    
                    calculatedOriginalColor = candidates[safeIndex].color;
                    
                    // Log the ranking so you know which Index to pick
                    Debug.Log($"[ColorSwapDebug] --- Ranking Results ---");
                    for (int i = 0; i < candidates.Count; i++)
                    {
                        string prefix = (i == safeIndex) ? ">>> SELECTED" : $"    Rank {i}";
                        Debug.Log($"[ColorSwapDebug] {prefix}: {candidates[i].color} (Score: {candidates[i].score:F2})");
                    }
                }
                else
                {
                    Debug.LogWarning("[ColorSwapDebug] No valid colors found after filtering.");
                }
            }
            catch (UnityException e)
            {
                Debug.LogWarning($"[ColorSwapDebug] Error: {e.Message}");
            }
            finally
            {
                if (createdTempTexture && textureToAnalyze != null)
                {
                    if (Application.isPlaying) Destroy(textureToAnalyze);
                    else DestroyImmediate(textureToAnalyze);
                }
            }
            
            UpdateShaderProperties();
        }

        private Texture2D GetCroppedTexture(Sprite sprite)
        {
            if (sprite == null || sprite.texture == null) return null;
            Rect r = sprite.textureRect;
            try 
            {
                Texture2D newTex = new Texture2D((int)r.width, (int)r.height);
                Color[] pixels = sprite.texture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
                newTex.SetPixels(pixels);
                newTex.Apply();
                newTex.name = sprite.name + "_Cropped";
                return newTex;
            }
            catch (UnityException) { return null; }
        }

        public void UpdateShaderProperties()
        {
            if (_renderer == null) _renderer = GetComponent<Renderer>();
            if (_renderer == null) return;
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(OriginalColorID, calculatedOriginalColor);
            _propBlock.SetColor(TargetColorID, targetColor);
            _propBlock.SetFloat(ToleranceID, tolerance);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}