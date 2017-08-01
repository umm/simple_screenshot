# What?

* 実行中のゲーム画面 (など) をシンプルにキャプチャします。

# Why?

* 何かとスクショ撮る要件あるよね、ってコトで。

# Install

```shell
$ npm install @umm/simple_screenshot
```

# Usage

```csharp
SimpleScreenshot.Instance.Capture().Subscribe(texture => Debug.Log(texture));
```

* Capture メソッドが、キャプチャ完了時に Texture を流すストリームを返します
* あとはそれをファイルに吐くなり REST API に送るなりご自由に

# License

Copyright (c) 2017 Tetsuya Mori

Released under the MIT license, see [LICENSE.txt](LICENSE.txt)

