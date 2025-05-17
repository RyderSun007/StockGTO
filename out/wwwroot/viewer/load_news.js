fetch('/data/news_data.json')
  .then(response => response.json())
  .then(data => {
    const newsList = document.getElementById("news-list");
    newsList.innerHTML = "";

    data.forEach(item => {
      const div = document.createElement("div");
      div.className = "news-card";
      div.innerHTML = `
        <h3>${item.title}</h3>
        <p><strong>🕒 時間：</strong>${item.time || "無時間資料"}</p>
        ${item.image ? `<img src="${item.image}" alt="新聞圖片">` : ''}
        <p>${item.content}</p>
        <p><a href="${item.link}" target="_blank">🔗 前往原文</a></p>
      `;
      newsList.appendChild(div);
    });
  })
  .catch(err => {
    document.getElementById("news-list").innerHTML = "🚫 載入資料失敗：" + err;
  });
