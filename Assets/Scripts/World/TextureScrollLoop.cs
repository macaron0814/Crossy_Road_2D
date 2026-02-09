using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TextureScrollLoop : MonoBehaviour
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    [Header("Scroll")]
    [Tooltip("UV scroll speed (units per second)")]
    public Vector2 scrollSpeed = new Vector2(0.1f, 0f);
    [Tooltip("Use unscaled time for scrolling")]
    public bool useUnscaledTime = false;
    [Tooltip("If true, scroll based on player movement instead of time")]
    public bool usePlayerMovement = false;

    [Header("Tiling")]
    [Tooltip("Texture tiling (repeat count)")]
    public Vector2 textureRepeat = Vector2.one;
    [Tooltip("Wrap UV to 0-1 to avoid large values")]
    public bool wrap01 = true;

    [Header("Material")]
    [Tooltip("Material to scroll. If empty, use Renderer material.")]
    public Material targetMaterial;
    [Tooltip("Create material instance so only this object is affected")]
    public bool instantiateMaterial = true;

    [Header("Player Movement")]
    [Tooltip("Player transform to track")]
    public Transform player;
    [Tooltip("Use local position for movement")]
    public bool useLocalPlayerPosition = false;
    [Tooltip("Player movement axis to sample")]
    public Axis playerMovementAxis = Axis.Y;
    [Tooltip("UV axis to move")]
    public Axis uvAxis = Axis.X;
    [Tooltip("UV offset per 1 unit of player movement")]
    public float playerMovementToUv = 0.01f;
    [Tooltip("Invert player movement direction")]
    public bool invertPlayerMovement = false;
    [Tooltip("Only scroll when movement on the axis is positive")]
    public bool onlyPositiveMovement = true;

    private string texturePropertyName = "_BaseMap";
    private Renderer cachedRenderer;
    private RawImage rawImage;
    private Material materialInstance;
    private Vector2 currentOffset;
    private bool hasTextureProperty;
    private Vector3 lastPlayerPosition;
    private bool hasPlayerPosition;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        cachedRenderer = GetComponent<Renderer>();

        if (rawImage == null && cachedRenderer == null)
        {
            Debug.LogWarning("TextureScrollLoop: No Renderer or RawImage found.", this);
            enabled = false;
            return;
        }

        if (rawImage != null)
        {
            InitializeRawImage();
            return;
        }

        InitializeMaterial();
    }

    private void InitializeRawImage()
    {
        var uv = rawImage.uvRect;
        uv.width = textureRepeat.x;
        uv.height = textureRepeat.y;
        uv.x = 0f;
        uv.y = 0f;
        rawImage.uvRect = uv;

        if (rawImage.texture != null)
        {
            rawImage.texture.wrapMode = TextureWrapMode.Repeat;
        }
    }

    private void InitializeMaterial()
    {
        if (targetMaterial == null)
        {
            targetMaterial = cachedRenderer.sharedMaterial;
        }

        if (targetMaterial == null)
        {
            Debug.LogWarning("TextureScrollLoop: No material found.", this);
            enabled = false;
            return;
        }

        if (instantiateMaterial)
        {
            materialInstance = new Material(targetMaterial);
            cachedRenderer.material = materialInstance;
        }
        else
        {
            materialInstance = targetMaterial;
            cachedRenderer.sharedMaterial = materialInstance;
        }

        if (materialInstance.HasProperty("_BaseMap"))
        {
            texturePropertyName = "_BaseMap";
            hasTextureProperty = true;
        }
        else if (materialInstance.HasProperty("_MainTex"))
        {
            texturePropertyName = "_MainTex";
            hasTextureProperty = true;
        }
        else
        {
            Debug.LogWarning("TextureScrollLoop: Texture property not found.", this);
            hasTextureProperty = false;
        }

        if (hasTextureProperty)
        {
            materialInstance.SetTextureScale(texturePropertyName, textureRepeat);
            Texture mainTexture = materialInstance.GetTexture(texturePropertyName);
            if (mainTexture != null)
            {
                mainTexture.wrapMode = TextureWrapMode.Repeat;
            }
        }
    }

    private void Update()
    {
        if (usePlayerMovement)
        {
            if (player == null)
            {
                return;
            }

            Vector3 currentPosition = useLocalPlayerPosition ? player.localPosition : player.position;
            if (!hasPlayerPosition)
            {
                lastPlayerPosition = currentPosition;
                hasPlayerPosition = true;
                return;
            }

            Vector3 delta = currentPosition - lastPlayerPosition;
            lastPlayerPosition = currentPosition;

            float movement = GetAxisValue(delta, playerMovementAxis);
            if (invertPlayerMovement)
            {
                movement = -movement;
            }

            if (onlyPositiveMovement && movement <= 0f)
            {
                return;
            }

            if (uvAxis == Axis.X)
            {
                currentOffset.x += movement * playerMovementToUv;
            }
            else if (uvAxis == Axis.Y)
            {
                currentOffset.y += movement * playerMovementToUv;
            }
        }
        else
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            currentOffset += scrollSpeed * dt;
        }

        if (wrap01)
        {
            currentOffset.x = Mathf.Repeat(currentOffset.x, 1f);
            currentOffset.y = Mathf.Repeat(currentOffset.y, 1f);
        }

        if (rawImage != null)
        {
            var uv = rawImage.uvRect;
            uv.x = currentOffset.x;
            uv.y = currentOffset.y;
            uv.width = textureRepeat.x;
            uv.height = textureRepeat.y;
            rawImage.uvRect = uv;
            return;
        }

        if (materialInstance != null && hasTextureProperty)
        {
            materialInstance.SetTextureOffset(texturePropertyName, currentOffset);
        }
    }

    private static float GetAxisValue(Vector3 value, Axis axis)
    {
        switch (axis)
        {
            case Axis.X:
                return value.x;
            case Axis.Y:
                return value.y;
            case Axis.Z:
                return value.z;
            default:
                return 0f;
        }
    }

    private void OnDestroy()
    {
        if (materialInstance != null && instantiateMaterial)
        {
            Destroy(materialInstance);
        }
    }
}

