// #define DEBUG_LOG

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Linq;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Components.Video;

[RequireComponent(typeof(VRCUnityVideoPlayer))]
[RequireComponent(typeof(Camera))]
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class WeatherVideo : UdonSharpBehaviour
{
    public readonly string version = "v1.0.1";
    [Header("渲染空白材質")]
    public Texture2D texture;
    [Header("顯示 UI")]
    public Canvas canvas;
    [Header("天氣圖示")]
    public Sprite[] weatherTextures;
    [Header("預設顯示城市，可用名稱請參見說明文件")]
    public string defaultCity;
    private Dropdown dropDown;
    private VRCUnityVideoPlayer videoPlayer;
    private Camera cam;
    private Text[] textDate = new Text[7];
    private Text[] textTempture = new Text[7];
    private Text[] textRain = new Text[7];
    private Text[] textWeather = new Text[7];
    private Text textVersion;
    private Image[] imageWeather = new Image[7];
    private GameObject objLoading;
    private VRCUrl url = new VRCUrl("https://raw.githubusercontent.com/rogeraabbccdd/Udon-TaiwanWeather/gh-pages/out.mp4");
    private bool rendered = true;
    private int sizePixel = 4;
    private int sizeTexture = 1024;
    private string[] namesCity = new string[] {"基隆市", "台北市", "新北市", "桃園市", "新竹市", "新竹縣", "苗栗縣", "台中市", "彰化縣", "南投縣", "雲林縣", "嘉義市", "嘉義縣", "台南市", "高雄市", "屏東縣", "宜蘭縣", "花蓮縣", "台東縣", "澎湖縣", "金門縣", "連江縣"};
    private string[] namesDay = new string[] { "日", "一", "二", "三", "四", "五", "六" };
    private string textDecoded = "";
    private bool loading = true;
    private int readingY = -1;
    private string textBinary = "";
    private int count = 0;
    private string temp = "";
    private char[] c;
    private int byteIndex = 0;
    private void Start()
    {
        transform.position = new Vector3(0, float.MaxValue, 0);

        videoPlayer = (VRCUnityVideoPlayer)GetComponent(typeof(VRCUnityVideoPlayer));
        videoPlayer.Loop = false;

        cam = (Camera)GetComponent(typeof(Camera));

        canvas.transform.Find("Footer/Version").GetComponent<Text>().text = version;

        textDate[0] = canvas.transform.Find("Weathers/1/Date").GetComponent<Text>();
        textDate[1] = canvas.transform.Find("Weathers/2/Date").GetComponent<Text>();
        textDate[2] = canvas.transform.Find("Weathers/3/Date").GetComponent<Text>();
        textDate[3] = canvas.transform.Find("Weathers/4/Date").GetComponent<Text>();
        textDate[4] = canvas.transform.Find("Weathers/5/Date").GetComponent<Text>();
        textDate[5] = canvas.transform.Find("Weathers/6/Date").GetComponent<Text>();
        textDate[6] = canvas.transform.Find("Weathers/7/Date").GetComponent<Text>();

        textTempture[0] = canvas.transform.Find("Weathers/1/Tempture").GetComponent<Text>();
        textTempture[1] = canvas.transform.Find("Weathers/2/Tempture").GetComponent<Text>();
        textTempture[2] = canvas.transform.Find("Weathers/3/Tempture").GetComponent<Text>();
        textTempture[3] = canvas.transform.Find("Weathers/4/Tempture").GetComponent<Text>();
        textTempture[4] = canvas.transform.Find("Weathers/5/Tempture").GetComponent<Text>();
        textTempture[5] = canvas.transform.Find("Weathers/6/Tempture").GetComponent<Text>();
        textTempture[6] = canvas.transform.Find("Weathers/7/Tempture").GetComponent<Text>();

        textRain[0] = canvas.transform.Find("Weathers/1/Rain/Rain").GetComponent<Text>();
        textRain[1] = canvas.transform.Find("Weathers/2/Rain/Rain").GetComponent<Text>();
        textRain[2] = canvas.transform.Find("Weathers/3/Rain/Rain").GetComponent<Text>();
        textRain[3] = canvas.transform.Find("Weathers/4/Rain/Rain").GetComponent<Text>();
        textRain[4] = canvas.transform.Find("Weathers/5/Rain/Rain").GetComponent<Text>();
        textRain[5] = canvas.transform.Find("Weathers/6/Rain/Rain").GetComponent<Text>();
        textRain[6] = canvas.transform.Find("Weathers/7/Rain/Rain").GetComponent<Text>();

        textWeather[0] = canvas.transform.Find("Weathers/1/WeatherText").GetComponent<Text>();
        textWeather[1] = canvas.transform.Find("Weathers/2/WeatherText").GetComponent<Text>();
        textWeather[2] = canvas.transform.Find("Weathers/3/WeatherText").GetComponent<Text>();
        textWeather[3] = canvas.transform.Find("Weathers/4/WeatherText").GetComponent<Text>();
        textWeather[4] = canvas.transform.Find("Weathers/5/WeatherText").GetComponent<Text>();
        textWeather[5] = canvas.transform.Find("Weathers/6/WeatherText").GetComponent<Text>();
        textWeather[6] = canvas.transform.Find("Weathers/7/WeatherText").GetComponent<Text>();

        imageWeather[0] = canvas.transform.Find("Weathers/1/Weather").GetComponent<Image>();
        imageWeather[1] = canvas.transform.Find("Weathers/2/Weather").GetComponent<Image>();
        imageWeather[2] = canvas.transform.Find("Weathers/3/Weather").GetComponent<Image>();
        imageWeather[3] = canvas.transform.Find("Weathers/4/Weather").GetComponent<Image>();
        imageWeather[4] = canvas.transform.Find("Weathers/5/Weather").GetComponent<Image>();
        imageWeather[5] = canvas.transform.Find("Weathers/6/Weather").GetComponent<Image>();
        imageWeather[6] = canvas.transform.Find("Weathers/7/Weather").GetComponent<Image>();

        objLoading = canvas.transform.Find("Loading").gameObject;

        dropDown = canvas.transform.Find("Top/Dropdown").GetComponent<Dropdown>();

        c = new char[(texture.height / sizePixel) * (texture.width / sizePixel)];

        SendCustomEventDelayedSeconds("LoadVideo", 5);
    }
    public void LoadVideo ()
    {
        loading = true;
        objLoading.SetActive(true);
        objLoading.transform.Find("Text").GetComponent<Text>().text = "天氣資料載入中";
        videoPlayer.LoadURL(url);

#if DEBUG_LOG
        Debug.Log("Load Weather Video");
#endif

    }
    public override void OnVideoReady ()
    {

#if DEBUG_LOG
        Debug.Log("On Weather VideoReady");
#endif
        // Capture video frame to render texture
        SendCustomEvent(nameof(EnableCam));
        SendCustomEventDelayedFrames(nameof(DisableCam), 2);
    }
    public override void OnVideoError (VideoError videoError)
    {
        videoPlayer.Stop();
        objLoading.transform.Find("Text").GetComponent<Text>().text = "天氣載入失敗，五秒後重試";
        loading = false;

#if DEBUG_LOG
        Debug.Log("On Weather VideoError: " + videoError);
#endif
        SendCustomEventDelayedSeconds("Reload", 5);
    }
    public void EnableCam()
    {
        cam.enabled = true;
        rendered = false;
    }
    public void DisableCam()
    {
        cam.enabled = false;
    }
    private void OnPostRender()
    {
        if (rendered) return;
        rendered = true;

#if DEBUG_LOG
        Debug.Log("On Weather PostRender");
#endif

        // Read pixel from camera render texture
        texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0, false);
        texture.Apply();

        // Reset decode variables
        c = new char[(texture.height / sizePixel) * (texture.width / sizePixel)];
        byteIndex = 0;
        temp = "";
        count = 0;
        textBinary = "";
        textDecoded = "";
        readingY = texture.height - (sizePixel / 2);

        // Decode pixel to string
        SendCustomEventDelayedSeconds("DecodeVideo", 1);
    }
    public void DecodeVideo ()
    {
        bool end = false;
        for (int x = (sizePixel / 2); x <= texture.width; x += sizePixel)
        {
            Color color = texture.GetPixel(x, readingY);
            if (color.b > 0.5f && color.g < 0.5f && color.r < 0.5f)
            {
                end = true;
                break;
            }
            string data = color.r > 0.5 ? "1" : "0";
            textBinary += data;
            temp += data;
            count++;
            if (count % 8 == 0)
            {
                c[byteIndex] = Convert.ToChar(Convert.ToByte(temp, 2));
                byteIndex++;
                temp = "";
            }
        }
        readingY -= sizePixel;

        // Delay 3 frames to reduce lag
        if (readingY >= 0 && !end)
        {
            SendCustomEventDelayedFrames("DecodeVideo", 3);
        }
        else
        {
            SendCustomEventDelayedFrames("OnVideoDecoded", 3);
        }
    }
    public void OnVideoDecoded ()
    {
        textDecoded = (new string(c)).Trim(char.Parse("\0"));

#if DEBUG_LOG
        Debug.Log("Decoded Weather Binary: " + textBinary);
        Debug.Log("Decoded Weather Text: " + textDecoded);
#endif

        if (defaultCity.Length == 0)
        {
            defaultCity = "台北市";
            dropDown.value = 1;
        }
        else
        {
            for (int i = 0; i < namesCity.Length; i++)
            {
                if (namesCity[i] == defaultCity)
                {
                    dropDown.value = i;
                    break;
                }
            }
        }

        // Disable loading UI
        objLoading.SetActive(false);

        loading = false;
    }
    private void RenderCanvas ()
    {
        if (textDecoded.Length == 0) return;

        string[] textDecodedCites = textDecoded.Split(char.Parse("-"));

#if DEBUG_LOG
        Debug.Log("textDecodedCites: " + textDecodedCites[dropDown.value]);
#endif
        string[] days = textDecodedCites[dropDown.value].Split(char.Parse("|"));
        for (int j = 0; j < 7; j++)
        {
#if DEBUG_LOG
            Debug.Log("days: " + days[j]);
#endif
            string[] data = days[j].Split(char.Parse(","));
            textDate[j].text = $"{data[0]}  星期{namesDay[int.Parse(data[1])]}";
            textTempture[j].text = $"{data[5]}° ~ {data[4]}°";
            textRain[j].text = $"{data[6]}%";
            imageWeather[j].sprite = weatherTextures[int.Parse(data[2]) - 1];
            
            // https://stackoverflow.com/questions/35026517/change-escaped-unicode-to-string-in-c-sharp#answer-35026806
            string[] chars = data[3].Split(new[]{@"\u"}, StringSplitOptions.RemoveEmptyEntries);
            for (int k = 0; k < chars.Length; k++)
            {
                chars[k] = ((char) Convert.ToInt32(chars[k], 16)).ToString();
            }
            textWeather[j].text = string.Join("", chars);
        }
    }
    public void OnCitySelected ()
    {
        if (textDecoded.Length == 0) return;
        RenderCanvas();
    }
    public void Reload ()
    {
        if (loading)    return;
        LoadVideo();
    }
}
