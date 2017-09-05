# What?

* 実行中のゲーム画面 (など) をシンプルにキャプチャします。

# Why?

* 何かとスクショ撮る要件あるよね、ってコトで。

# Install

```shell
$ npm install github:umm-projects/simple_screenshot.git
```

# Usage

```csharp
SimpleScreenshot
    .Install(Camera.main)
    .Capture()
    .Subscribe(texture => Debug.Log(texture));

SimpleScreenshot
    .Install(Camera.main)
    .CaptureToFile("/path/to/file")
    .Subscribe(_ => Debug.Log("Finish to output!"));
```

* Install メソッドに撮影対象のカメラを渡します
    * List で渡すコトも可能です
* Capture メソッドが、キャプチャ完了時に Texture を流すストリームを返します
    * あとはそれを REST API に送るなりなんなりご自由に
* CaptureToFile メソッドはキャプチャ結果をファイルに出力します
    * これの戻り値のストリームは、出力完了時に Unit.Default を流します
    * 内部でランダムなファイル名生成して、それを流すコトも考えたけどメンドイのでひとまずパス

# License

Copyright (c) 2017 Tetsuya Mori

Released under the MIT license, see [LICENSE.txt](LICENSE.txt)

