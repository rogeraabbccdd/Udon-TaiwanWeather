const axios = require('axios')
const PImage = require('pureimage')
const fs = require('fs')
const cheerio = require('cheerio')

const sizeImage = 1024
const sizePixel = 4
const apis = [
  // 基隆市
  'https://www.accuweather.com/zh/tw/keelung-city/312605/daily-weather-forecast/312605',
  // 台北市
  'https://www.accuweather.com/zh/tw/taipei-city/315078/daily-weather-forecast/315078',
  // 新北市
  'https://www.accuweather.com/zh/tw/new-taipei-city/2515397/daily-weather-forecast/2515397',
  // 桃園市
  'https://www.accuweather.com/zh/tw/taoyuan-city/3369297/daily-weather-forecast/3369297',
  // 新竹市
  'https://www.accuweather.com/zh/tw/hsinchu-city/313567/daily-weather-forecast/313567',
  // 新竹縣
  'https://www.accuweather.com/zh/tw/hsinchu-county/3369298/daily-weather-forecast/3369298',
  // 苗栗縣
  'https://www.accuweather.com/zh/tw/miaoli-county/3369299/daily-weather-forecast/3369299',
  // 台中市
  'https://www.accuweather.com/zh/tw/taichung-city/315040/daily-weather-forecast/315040',
  // 彰化縣
  'https://www.accuweather.com/zh/tw/changhua-county/3369300/daily-weather-forecast/3369300',
  // 南投縣
  'https://www.accuweather.com/zh/tw/nantou-county/3369301/daily-weather-forecast/3369301',
  // 雲林縣
  'https://www.accuweather.com/zh/tw/yunlin-county/3369302/daily-weather-forecast/3369302',
  // 嘉義市
  'https://www.accuweather.com/zh/tw/chiayi-city/312591/daily-weather-forecast/312591',
  // 嘉義縣
  'https://www.accuweather.com/zh/tw/chiayi-county/3369303/daily-weather-forecast/3369303',
  // 台南市
  'https://www.accuweather.com/zh/tw/tainan-city/314999/daily-weather-forecast/314999',
  // 高雄市
  'https://www.accuweather.com/zh/tw/kaohsiung-city/313812/daily-weather-forecast/313812',
  // 屏東縣
  'https://www.accuweather.com/zh/tw/pingtung-county/3369304/daily-weather-forecast/3369304',
  // 宜蘭縣
  'https://www.accuweather.com/zh/tw/yilan-county/3369296/daily-weather-forecast/3369296',
  // 花蓮縣
  'https://www.accuweather.com/zh/tw/hualien-county/3369306/daily-weather-forecast/3369306',
  // 台東縣
  'https://www.accuweather.com/zh/tw/taitung-county/3369305/daily-weather-forecast/3369305',
  // 澎湖縣
  'https://www.accuweather.com/zh/tw/penghu-county/3369307/daily-weather-forecast/3369307',
  // 金門縣
  'https://www.accuweather.com/zh/tw/kinmen-county/2332525/daily-weather-forecast/2332525',
  // 連江縣
  'https://www.accuweather.com/zh/tw/lienchiang-county/3369309/daily-weather-forecast/3369309'
]
const days = ['日', '一', '二', '三', '四', '五', '六']

const main = async () => {
  try {
    // Fetch weather data
    const requests = apis.map(async (api) => {
      const { data: html } = await axios.get(api, {
        headers: {
          cookie: 'awx_user=tp:C',
          'Accept-Encoding': 'gzip, deflate, br',
          'accept-language': 'zh-TW,zh;q=0.9,ja-JP;q=0.8,ja;q=0.7,en-US;q=0.6,en;q=0.5,ko;q=0.4',
          accept: 'accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9',
          'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36'
        }
      })
      const $ = cheerio.load(html)
      const data = []
      $('.daily-wrapper').each(function (i) {
        if (i > 6) return false
        data.push(
          // date
          $(this).find('.module-header.sub.date').text().trim() + ',' +
          // day
          days.indexOf($(this).find('.module-header.dow.date').text().trim().replace('周', '')) + ',' +
          // weather
          $(this).find('svg.icon').attr('data-src').trim().replace('/images/weathericons/', '').replace('.svg', '') + ',' +
          // text
          $(this).find('.phrase').text().trim().split('').map(char => '\\u' + ('0000' + char.charCodeAt(0).toString(16).toUpperCase()).slice(-4)).join('') + ',' +
          // temperature high
          $(this).find('.high').text().trim().replace('°', '') + ',' +
          // temperature low
          $(this).find('.low').text().trim().replace('°', '').replace('/', '') + ',' +
          // rain
          $(this).find('.precip').text().trim().replace('%', '')
        )
        return true
      })
      return data.join('|')
    })
    const results = await Promise.all(requests)
    const combined = results.join('-')
    // Convert data ascii binary string
    const binString = combined.split('').map(char => char.charCodeAt(0).toString(2).padStart(8, '0')).join('')

    // Create image from binary string
    const img = PImage.make(sizeImage, sizeImage)
    const ctx = img.getContext('2d')
    ctx.fillStyle = 'blue'
    ctx.fillRect(0, 0, sizeImage, sizeImage)
    const ratio = sizeImage / sizePixel
    for (let i = 0; i < binString.length; i++) {
      ctx.fillStyle = binString[i] === '0' ? 'black' : 'white'
      const x = i % ratio
      const y = Math.floor(i / ratio)
      ctx.fillRect(x * sizePixel, y * sizePixel, sizePixel, sizePixel)
    }
    if (!fs.existsSync('./output')) fs.mkdirSync('./output')
    // fs.writeFileSync('./output/weathers_bin.txt', binString)
    // fs.writeFileSync('./output/weathers_str.txt', combined)
    await PImage.encodePNGToStream(img, fs.createWriteStream('./output/out.png'))
  } catch (error) {
    throw new Error(error.message)
  }
}

main()
