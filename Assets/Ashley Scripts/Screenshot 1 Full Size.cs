using UnityEngine;
using System.Collections;
using System.IO;
public class ScreenshotTaker : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) // Press 'P' to take a screenshot
        {
            string folderPath = @"C:\Users\G16\Unity Projects\Unity Production Project Screenshots";  // Custom path
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);  // Create the folder if it doesn't exist
            }
            string path = Path.Combine(folderPath, "screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
            ScreenCapture.CaptureScreenshot(path);
        }
    }
}