using System.Collections.Generic;
using System.IO;
using GameObjectExtension;
using UniRx;
using UnityEngine;

/// <summary>
/// スクリーンショットを簡単に撮影する
/// </summary>
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
    private RenderTexture outputRenderTexture;

    /// <summary>
    /// 描画先の RenderTexture
    /// </summary>
    public RenderTexture OutputRenderTexture {
        get {
            if (this.outputRenderTexture == default(RenderTexture)) {
                this.outputRenderTexture = new RenderTexture(Screen.width, Screen.height, RENDER_TEXTURE_DEPTH, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            }
            return this.outputRenderTexture;
        }
        private set {
            this.outputRenderTexture = value;
        }
    }

    /// <summary>
    /// 描画元の RenderTexture の実体
    /// </summary>
    private RenderTexture inputRenderTexture;

    /// <summary>
    /// 描画元の RenderTexture
    /// </summary>
    public RenderTexture InputRenderTexture {
        get {
            if (this.inputRenderTexture == default(RenderTexture)) {
                this.inputRenderTexture = new RenderTexture(Screen.width, Screen.height, RENDER_TEXTURE_DEPTH, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            }
            return this.inputRenderTexture;
        }
        private set {
            this.inputRenderTexture = value;
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
            if (this.targetCameraList == default(List<Camera>)) {
                this.targetCameraList = new List<Camera>();
            }
            return this.targetCameraList;
        }
        set {
            this.targetCameraList = value;
        }
    }

    /// <summary>
    /// 元の RenderTexture を Camera 毎に保持するためのマップ
    /// </summary>
    private Dictionary<Camera, RenderTexture> originalRenderTextureMap;

    /// <summary>
    /// キャプチャ結果を流すストリームの実体
    /// </summary>
    private Subject<Texture2D> subjectCapture;

    /// <summary>
    /// Unity lifecycle: Awake
    /// </summary>
    /// <remarks>自身の GameObject に必要に応じて Camera を追加しつつ RenderTexture を設定</remarks>
    public override void Awake() {
        base.Awake();
        Camera screenshotCamera = this.gameObject.GetOrAddComponent<Camera>();
        screenshotCamera.targetTexture = this.OutputRenderTexture;
        screenshotCamera.clearFlags = CameraClearFlags.Color;
        screenshotCamera.backgroundColor = Color.white;
    }

    /// <summary>
    /// 対象のカメラに描画されている映像をキャプチャする
    /// </summary>
    /// <returns>キャプチャ結果を流すストリーム</returns>
    public IObservable<Texture2D> Capture() {
        if (this.TargetCameraList.Count == 0) {
            return Observable.Throw<Texture2D>(new System.InvalidOperationException("Target camera list is empty"));
        }
        if (this.subjectCapture != default(Subject<Texture2D>)) {
            return Observable.Throw<Texture2D>(new System.InvalidOperationException("Capture stream has already generated."));
        }
        this.originalRenderTextureMap = new Dictionary<Camera, RenderTexture>();
        this.subjectCapture = new Subject<Texture2D>();
        foreach (Camera targetCamera in this.TargetCameraList) {
            this.originalRenderTextureMap[targetCamera] = targetCamera.targetTexture;
            targetCamera.targetTexture = this.InputRenderTexture;
        }
        return this.subjectCapture.AsObservable();
    }

    /// <summary>
    /// 対象のカメラに描画されている映像をキャプチャしてファイルに出力する
    /// </summary>
    /// <param name="filePath">キャプチャ結果を出力するファイルパス</param>
    /// <returns>ファイルへの保存が完了したことを通知するストリーム</returns>
    public IObservable<Unit> CaptureToFile(string filePath) {
        return Observable.Create<Unit>(
            (observer) => {
                this.Capture()
                    .Subscribe(
                        (capturedTexture) => {
                            File.WriteAllBytes(filePath, capturedTexture.EncodeToPNG());
                            observer.OnNext(Unit.Default);
                            observer.OnCompleted();
                        }
                    );
                return null;
            }
        );
    }

    /// <summary>
    /// シーン上にインストールする
    /// </summary>
    /// <remarks>カメラを1つのみ設定するためのオーバーロード</remarks>
    /// <param name="targetCamera">対象のカメラ</param>
    /// <returns>インストールしたインスタンス</returns>
    public static SimpleScreenshot Install(Camera targetCamera) {
        return Install(
            new List<Camera>() {
                targetCamera,
            }
        );
    }

    /// <summary>
    /// シーン上にインストールする
    /// </summary>
    /// <param name="targetCameraList">対象のカメラリスト</param>
    /// <returns>インストールしたインスタンス</returns>
    public static SimpleScreenshot Install(List<Camera> targetCameraList = null) {
        Instance.gameObject.name = GAME_OBJECT_NAME;
        if (targetCameraList != default(List<Camera>)) {
            Instance.TargetCameraList = targetCameraList;
        }
        return Instance;
    }

    /// <summary>
    /// Unity lifecycle: OnRenderImage
    /// </summary>
    /// <remarks>RenderTexture への描画が完了した時点でコールバックされる</remarks>
    /// <param name="src">描画済の RenderTexture</param>
    /// <param name="dest">描画先の RenderTexture</param>
    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        // ストリームが無い場合はキャプチャ指示が出ていないと見なして落とす
        if (this.subjectCapture == default(Subject<Texture2D>)) {
            return;
        }
        Graphics.Blit(src, dest);
        RenderTexture.active = dest;
        Texture2D resultTexture = new Texture2D(dest.width, dest.height, TextureFormat.ARGB32, false, false);
        resultTexture.ReadPixels(new Rect(0, 0, dest.width, dest.height), 0, 0);
        resultTexture.Apply();
        this.subjectCapture.OnNext(resultTexture);
        // 一回流したら終了させる
        this.subjectCapture.OnCompleted();
        // インスタンスを破棄
        this.subjectCapture = default(Subject<Texture2D>);
        Destroy(resultTexture);
        // RenderTexture を解除
        foreach (Camera targetCamera in this.TargetCameraList) {
            if (!this.originalRenderTextureMap.ContainsKey(targetCamera)) {
                continue;
            }
            targetCamera.targetTexture = this.originalRenderTextureMap[targetCamera];
        }
    }

}