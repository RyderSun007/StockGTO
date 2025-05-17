import json
import requests
from bs4 import BeautifulSoup
import time
import random

headers = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36",
    "Referer": "https://udn.com",
    "Accept-Language": "zh-TW,zh;q=0.9"
}

url = "https://udn.com/news/breaknews/1"
res = requests.get(url, headers=headers, timeout=10)
soup = BeautifulSoup(res.text, "html.parser")

articles = soup.select("div.story-list__text a")
news_list = []
seen_links = set()

for article in articles:
    title = article.text.strip()
    link = article.get("href")
    if not link.startswith("http"):
        link = "https://udn.com" + link

    if link in seen_links:
        continue
    seen_links.add(link)

    try:
        # 💤 模擬人類停頓時間
        time.sleep(random.uniform(2, 4))

        article_res = requests.get(link, headers=headers, timeout=10)
        article_soup = BeautifulSoup(article_res.text, "html.parser")

        # 內文
        content_div = article_soup.select_one("section.article-content__editor")
        content = content_div.get_text(strip=True) if content_div else "❌ 無內文"

        # 圖片
        image = ""
        img_tag = article_soup.select_one("figure.article-content__cover img")
        if img_tag:
            srcset = img_tag.get("srcset") or ""
            src = img_tag.get("src") or ""
            if "http" in srcset:
                image = srcset.split(",")[0].split(" ")[0].strip()
            elif "http" in src:
                image = src
        if "logo.svg" in image or "scorecard" in image or ".gif" in image:
            image = ""

        # 時間
        time_tag = article_soup.select_one("div.story_bady_info span")
        time_str = time_tag.get_text(strip=True) if time_tag else ""

    except Exception as e:
        content = f"❌ 內文錯誤：{e}"
        image = ""
        time_str = ""

    news_list.append({
        "title": title,
        "link": link,
        "content": content,
        "image": image,
        "time": time_str
    })

# 寫入 JSON
with open("../data/news_data.json", "w", encoding="utf-8") as f:
    json.dump(news_list, f, ensure_ascii=False, indent=2)

print(f"✅ 新聞已寫入，共 {len(news_list)} 則！")
