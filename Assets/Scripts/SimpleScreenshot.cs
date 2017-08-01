using System.Collections.Generic;
using UnityEngine;

public class SimpleScreenshot : SingletonMonoBehaviour<SimpleScreenshot> {

    /// <summary>
    /// RenderTexture の深度バッファの精度
    /// </summary>
    private const int RENDER_TEXTURE_DEPTH = 24;

    /// <summary>
    /// 一時的に構築される GameObject の名称
    /// </summary>
    private const string GAME_OBJECT_NAME = "SimpleScreenshot";

    /// <summary>
    /// 描画先の RenderTexture の実体
    /// </summary>
    private RenderTexture renderTexture;

    /// <summary>
    /// 描画先の RenderTexture
    /// </summary>
    public RenderTexture RenderTexture {
        get {
            if (this.renderTexture == default(RenderTexture)) {
                this.renderTexture = new RenderTexture(Screen.width, Screen.height, RENDER_TEXTURE_DEPTH, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            }
            return this.renderTexture;
        }
        set {
            this.renderTexture = value;
        }
    }

    /// <summary>
    /// 撮影対象のカメラリストの実体
    /// </summary>
    private List<Camera> targetCameraList;

    /// <summary>
    /// 撮影対象のカメラリスト
    /// </summary>
    public List<Camera> TargetCameraList {
        get {
            return this.targetCameraList;
        }
        set {
            this.targetCameraList = value;
        }
    }

    public Texture2D Capture(Camera targetCamera = null) {
        if (targetCamera == default(Camera)) {
            targetCamera = Camera.main;
        }

        return null;
    }

}