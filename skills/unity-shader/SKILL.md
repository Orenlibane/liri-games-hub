---
name: unity-shader
description: >
  Creates Unity shaders for visual effects: outline shaders, dissolve/burn effects,
  cel shading, glowing items, fog of war, water, holographic effects, screen-space
  distortion, and more. Supports all three render pipelines (Built-in, URP, HDRP)
  and outputs either ShaderGraph node descriptions (for visual editor) or complete
  HLSL shader code. Use this skill when the user wants to create a visual shader
  effect, add a glow or outline to a sprite or 3D object, make something look
  magical or stylized, create a special material effect, or asks "how do I make
  X look like Y" where Y involves a shader. Also trigger for: "sprite outline",
  "dissolve effect", "hit flash", "cel shading", "water shader", "fog of war",
  "2D lighting effect", "pixel art shader".
---

# Unity Shader Generation Skill

You generate Unity shaders for visual effects. Before writing anything, determine the render pipeline and output format — these decisions gate everything else.

## Step 1: Gather context

Always ask (or infer from PRD) before generating:

1. **Render pipeline**: Built-in, URP, or HDRP?
   - Built-in: use `.shader` files with CG/HLSL
   - URP: use `.shader` with HLSL and URP tags, or ShaderGraph
   - HDRP: use ShaderGraph (hand-written HDRP shaders are painful)

2. **2D or 3D?**
   - 2D sprite shaders need `Sprites/Default` replacement approach
   - 3D shaders use standard mesh rendering

3. **ShaderGraph or hand-coded?**
   - ShaderGraph: user is comfortable with node editor, wants to tweak visually
   - Hand-coded: faster runtime, easier to version control, necessary for some effects

4. **Target effect** — be specific. "Outline" is different from "inner glow" is different from "drop shadow."

---

## Output format by pipeline

### Built-in Render Pipeline (.shader file)

```hlsl
Shader "MyGame/SpriteOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineThickness ("Outline Thickness", Range(0,0.1)) = 0.02
        [Toggle] _EnableOutline ("Enable Outline", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            sampler2D _MainTex;
            float4    _MainTex_TexelSize;
            float4    _OutlineColor;
            float     _OutlineThickness;
            float     _EnableOutline;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = v.uv;
                o.color  = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                if (_EnableOutline > 0.5)
                {
                    float t = _OutlineThickness;
                    float4 neighbors =
                        tex2D(_MainTex, i.uv + float2( t,  0)).a +
                        tex2D(_MainTex, i.uv + float2(-t,  0)).a +
                        tex2D(_MainTex, i.uv + float2( 0,  t)).a +
                        tex2D(_MainTex, i.uv + float2( 0, -t)).a;

                    if (col.a < 0.1 && neighbors > 0.1)
                        col = _OutlineColor;
                }

                return col;
            }
            ENDCG
        }
    }
}
```

### URP (.shader with URP tags)

```hlsl
Shader "MyGame/URP/SpriteOutline"
{
    Properties { ... }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "Transparent"
            "RenderType"     = "Transparent"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ... structs and logic same as above but using HLSL types
            ENDHLSL
        }
    }
}
```

### ShaderGraph (node description)

When the user wants ShaderGraph, produce a **step-by-step node construction guide** since you can't emit a `.shadergraph` JSON directly:

```
## Dissolve Effect — ShaderGraph Setup (URP Unlit)

### Shader type
Create a new Shader Graph: right-click Assets → Create → Shader Graph → URP → Unlit Shader Graph

### Properties (add in Blackboard)
| Name            | Type    | Default |
|-----------------|---------|---------|
| _MainTex        | Texture2D | white |
| _NoiseTex       | Texture2D | white |
| _DissolveAmount | Float   | 0       |
| _EdgeColor      | Color   | orange  |
| _EdgeWidth      | Float   | 0.05    |

### Node graph
1. Sample Texture 2D (_MainTex, UV0) → [mainColor]
2. Sample Texture 2D (_NoiseTex, UV0) → Split → R channel → [noiseR]
3. Subtract(_DissolveAmount, noiseR) → [dissolveMask]
4. Step(0, dissolveMask) → [alphaClip]  ← this is the dissolve cutoff
5. Add(_DissolveAmount, _EdgeWidth) → [edgeThreshold]
6. Step(noiseR, edgeThreshold) → Subtract(_, alphaClip) → [isEdge]
7. Lerp(mainColor, _EdgeColor, isEdge) → [finalColor]
8. Connect finalColor → Base Color
9. Connect alphaClip → Alpha
10. Set Alpha Clip Threshold to 0.5 in Graph Settings
```

---

## Common effects reference

### Hit flash (sprite turns white briefly)
Replace sprite color with white for 0.1s on damage. Use a material property block to avoid creating per-object material instances:
```csharp
private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
private MaterialPropertyBlock _mpb;
// In Awake: _mpb = new MaterialPropertyBlock();
// On hit: _renderer.GetPropertyBlock(_mpb); _mpb.SetColor(FlashColorId, Color.white); _renderer.SetPropertyBlock(_mpb);
// After delay: reset to clear color
```

### Sprite outline (2D)
Two approaches:
- **Shader-based** (above): single draw call, supports any color, animatable
- **Duplicate sprite approach**: render same sprite slightly offset in 4-8 directions with solid color — simpler to set up, more draw calls

### Fog of war (2D top-down)
Use a RenderTexture as a mask: black where unexplored, white where visible. Sample this texture in a full-screen shader to darken unexplored areas. Update the RT via a camera that renders explored zone indicators.

### Cel shading / toon
In URP: use a Shader Graph with a Posterize node on the diffuse light value to create hard light bands. Add a rim light term for the characteristic toon edge lighting.

---

## Output format

After generating shader code, tell the developer:

1. **File path** — where to save the `.shader` file or ShaderGraph asset
2. **Material setup** — how to create the material and assign it
3. **Script integration** — if a C# script is needed to drive the shader (e.g., hit flash controller)
4. **Properties to tune** — which Inspector sliders to adjust for the right look
5. **Performance notes** — any caveats (texture reads, overdraw, etc.)

Example:
```
📁 Save to: Assets/Shaders/SpriteOutline.shader
🎨 Create material: Right-click → Create → Material, assign this shader
🎛️ Tune: OutlineThickness (0.02–0.05 for sprites at 100 PPU), OutlineColor
⚡ Performance: 4 extra texture samples per fragment. Fine for < 50 outlined sprites.
   For many sprites, consider a post-process outline instead.
```
