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
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class WeatherVideo : UdonSharpBehaviour
{
    public readonly string version = "v1.1.0";
    [Header("顯示 UI")]
    public Canvas canvas;
    [Header("天氣圖示")]
    public Sprite[] weatherTextures;
    [Header("預設顯示城市，可用名稱請參見說明文件")]
    public string defaultCity;
    private Dropdown dropDown;
    private Text[] textDate = new Text[7];
    private Text[] textTempture = new Text[7];
    private Text[] textRain = new Text[7];
    private Text[] textWeather = new Text[7];
    private Text textVersion;
    private Image[] imageWeather = new Image[7];
    private GameObject objLoading;
    private VRCUrl url = new VRCUrl("https://rogeraabbccdd.github.io/Udon-TaiwanWeather/weathers_str.txt");
    private string[] namesCity = new string[] {"基隆市", "台北市", "新北市", "桃園市", "新竹市", "新竹縣", "苗栗縣", "台中市", "彰化縣", "南投縣", "雲林縣", "嘉義市", "嘉義縣", "台南市", "高雄市", "屏東縣", "宜蘭縣", "花蓮縣", "台東縣", "澎湖縣", "金門縣", "連江縣"};
    private string[] namesDay = new string[] { "日", "一", "二", "三", "四", "五", "六" };
    private string textDecoded = "";
    private bool loading = true;
    private IUdonEventReceiver _udonEventReceiver;
    private void Start()
    {
        transform.position = new Vector3(0, float.MaxValue, 0);

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

        _udonEventReceiver = (IUdonEventReceiver)this;
        VRCStringDownloader.LoadUrl(url, _udonEventReceiver);
    }
    public void LoadVideo ()
    {
        loading = true;
        objLoading.SetActive(true);
        objLoading.transform.Find("Text").GetComponent<Text>().text = "天氣資料載入中";
        VRCStringDownloader.LoadUrl(url, _udonEventReceiver);

#if DEBUG_LOG
        Debug.Log("Load Weather");
#endif

    }
    public override void OnStringLoadSuccess (IVRCStringDownload result)
    {
        textDecoded = result.Result;

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
        RenderCanvas();
    }
    public override void OnStringLoadError (IVRCStringDownload result)
    {
        objLoading.transform.Find("Text").GetComponent<Text>().text = "天氣載入失敗，五秒後重試";
        loading = false;
        SendCustomEventDelayedSeconds("Reload", 5);
#if DEBUG_LOG
        Debug.Log($"OnStringLoadError {result.Error}");
#endif
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
