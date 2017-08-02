using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SimpleScreenshot.Tests {

    public class SimpleScreenshotTest {

        private const string TEST_SCENE_NAME = "Tests/Scenes/CaptureTest";

        [UnityTest]
        public IEnumerator CaptureTest() {
            SceneManager.LoadScene(TEST_SCENE_NAME);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            // Texture2D 出力を検証
            SimpleScreenshot.Install(Camera.main).Capture().Subscribe(Assert.IsInstanceOf<Texture2D>);
            yield return new WaitForEndOfFrame();
            // ファイル出力を検証
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), string.Format("{0:yyyyMMddHHmmss.fff}.png", DateTime.Now));
            SimpleScreenshot.Install(Camera.main).CaptureToFile(filePath).Subscribe(
                (_) => {
                    Assert.IsTrue(File.Exists(filePath));
                    File.Delete(filePath);
                }
            );
        }

    }

}
