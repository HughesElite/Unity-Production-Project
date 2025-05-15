using UnityEngine;
using System.IO;

public class ScreenshotTaker2 : MonoBehaviour
{
    public int screenshotWidth = 1920;  // Custom width
    public int screenshotHeight = 1080; // Custom height

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) // Press 'P' to take a screenshot
        {
            string folderPath = @"C:\Users\G16\Unity Projects\Unity Production Project Screenshots";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);  // Create the folder if it doesn't exist
            }

            // Create a RenderTexture with the specified resolution
            RenderTexture renderTexture = new RenderTexture(screenshotWidth, screenshotHeight, 24);
            RenderTexture.active = renderTexture;

            // Capture the screen into the RenderTexture
            Camera.main.targetTexture = renderTexture;
            Camera.main.Render();

            // Create a new Texture2D to read the RenderTexture
            Texture2D screenShot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
            screenShot.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
            screenShot.Apply();

            // Save the screenshot to the file
            byte[] bytes = screenShot.EncodeToPNG();
            string path = Path.Combine(folderPath, "screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
            File.WriteAllBytes(path, bytes);

            // Clean up
            Camera.main.targetTexture = null;
            RenderTexture.active = null;
            Destroy(renderTexture);
            Destroy(screenShot);

            Debug.Log("Screenshot saved to: " + path);
        }
    }
}
