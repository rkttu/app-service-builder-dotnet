internal static class DashboardPage
{
    internal static string Render() => Page;

    private const string Page = """
<!doctype html>
<html lang="ko">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <meta name="theme-color" content="#122238">
  <meta name="description" content="오늘의 대한민국 17개 시·도 날씨와 주요 통화 원화 환율을 한눈에 확인하세요.">
  <link rel="icon" href="data:,">
  <title>오늘의 대한민국 | 날씨와 환율</title>
  <style>
    :root {
      color-scheme:light;
      --navy:#122238;
      --navy-2:#1b314b;
      --ink:#1c2938;
      --muted:#6f7985;
      --paper:#f7f6f1;
      --white:#fff;
      --line:#dfe3df;
      --lime:#dff06b;
      --sky:#a8ddf0;
      --coral:#f18b6d;
      --radius:22px;
    }
    * { box-sizing:border-box; }
    html { scroll-behavior:smooth; }
    body { margin:0; color:var(--ink); background:var(--paper); font:15px/1.6 "Pretendard Variable","Apple SD Gothic Neo","Malgun Gothic","Segoe UI",sans-serif; }
    button,input,select { font:inherit; }
    button { cursor:pointer; }
    a { color:inherit; }
    [hidden] { display:none!important; }
    a:focus-visible,button:focus-visible,input:focus-visible,select:focus-visible { outline:3px solid #5ab3d1; outline-offset:3px; }
    .skip { position:fixed; z-index:30; top:-80px; left:16px; padding:10px 14px; background:var(--lime); border-radius:10px; color:var(--navy); font-weight:800; }
    .skip:focus { top:12px; }
    .site-header { height:76px; display:flex; align-items:center; justify-content:space-between; gap:24px; padding:0 max(24px,calc((100vw - 1180px)/2)); color:#fff; background:var(--navy); border-bottom:1px solid #ffffff17; }
    .brand { display:flex; align-items:center; gap:11px; font-weight:850; letter-spacing:-.03em; text-decoration:none; }
    .brand-mark { position:relative; display:grid; place-items:center; width:35px; height:35px; border-radius:11px; color:var(--navy); background:var(--lime); font-size:20px; }
    .header-actions { display:flex; align-items:center; gap:28px; }
    .header-actions nav { display:flex; gap:24px; font-size:13px; font-weight:650; }
    .header-actions nav a { text-decoration:none; color:#dce4eb; }
    .header-actions nav a:hover { color:var(--lime); }
    .refresh { display:inline-flex; align-items:center; gap:8px; min-height:39px; padding:8px 14px; border:1px solid #ffffff32; border-radius:12px; color:#fff; background:#ffffff0b; font-size:12px; font-weight:750; }
    .refresh:hover { background:#ffffff17; }
    .refresh:disabled { opacity:.55; cursor:wait; }
    .hero { color:#fff; background:radial-gradient(circle at 82% 25%,#315f75 0,transparent 29%),radial-gradient(circle at 15% 110%,#24405b 0,transparent 42%),var(--navy); }
    .hero-inner { display:grid; grid-template-columns:minmax(0,1.25fr) minmax(290px,.75fr); gap:56px; align-items:end; max-width:1180px; margin:auto; padding:68px 24px 78px; }
    .eyebrow { display:flex; align-items:center; gap:10px; margin:0 0 17px; color:var(--lime); font-size:12px; font-weight:800; letter-spacing:.09em; }
    .eyebrow::before { content:""; width:27px; height:2px; background:currentColor; }
    h1 { max-width:780px; margin:0; font-size:clamp(40px,5.3vw,68px); font-weight:850; line-height:1.13; letter-spacing:-.055em; text-wrap:balance; }
    .hero-copy { max-width:650px; margin:24px 0 0; color:#c0cbd4; font-size:16px; }
    .hero-copy strong { color:#fff; }
    .hero-card { position:relative; min-height:238px; padding:26px; overflow:hidden; border:1px solid #ffffff23; border-radius:var(--radius); background:#ffffff0e; box-shadow:0 20px 55px #08111d33; backdrop-filter:blur(8px); }
    .hero-card::after { content:""; position:absolute; width:145px; height:145px; top:-54px; right:-43px; border-radius:50%; background:var(--lime); opacity:.92; }
    .hero-card-label { margin:0 0 26px; color:#becbd5; font-size:12px; font-weight:700; }
    .featured-weather { position:relative; z-index:1; display:flex; align-items:flex-start; justify-content:space-between; gap:18px; }
    .featured-temp { display:block; font-size:64px; font-weight:850; line-height:1; letter-spacing:-.06em; }
    .featured-place { display:block; margin-top:10px; font-size:13px; color:#d3dde5; }
    .featured-icon { font-size:45px; filter:drop-shadow(0 6px 12px #0003); }
    .hero-meta { display:flex; flex-wrap:wrap; gap:13px 18px; margin-top:29px; color:#d9e2e9; font-size:12px; }
    main { max-width:1180px; margin:auto; padding:0 24px 62px; }
    .status-banner { display:flex; align-items:center; gap:12px; margin:20px 0 0; padding:13px 16px; border:1px solid #ddc86a; border-radius:13px; background:#fff9d9; color:#695a1b; font-size:13px; }
    .status-banner::before { content:"i"; display:grid; place-items:center; flex:none; width:21px; height:21px; border-radius:50%; background:#796b2f; color:#fff; font-weight:800; }
    .section { padding-top:62px; scroll-margin-top:10px; }
    .section-header { display:flex; justify-content:space-between; align-items:end; gap:30px; margin-bottom:25px; }
    .section-kicker { margin:0 0 7px; color:#648098; font-size:11px; font-weight:850; letter-spacing:.12em; }
    h2 { margin:0; font-size:31px; line-height:1.25; font-weight:850; letter-spacing:-.045em; }
    .section-note { max-width:520px; margin:8px 0 0; color:var(--muted); font-size:13px; }
    .updated-at { margin:0; color:var(--muted); font-size:11px; white-space:nowrap; }
    .weather-toolbar { display:flex; justify-content:space-between; align-items:center; gap:16px; margin-bottom:20px; }
    .search { position:relative; width:min(330px,100%); }
    .search span { position:absolute; left:15px; top:50%; transform:translateY(-50%); color:#8b959f; }
    .search input { width:100%; height:45px; padding:0 16px 0 42px; border:1px solid var(--line); border-radius:13px; background:#fff; color:var(--ink); box-shadow:0 4px 12px #12223808; }
    .region-count { color:var(--muted); font-size:12px; }
    .weather-grid { display:grid; grid-template-columns:repeat(4,minmax(0,1fr)); gap:14px; }
    .weather-card { min-width:0; padding:20px; border:1px solid var(--line); border-radius:18px; background:#fff; box-shadow:0 9px 25px #15283a08; transition:transform .18s ease,box-shadow .18s ease; }
    .weather-card:hover { transform:translateY(-3px); box-shadow:0 14px 30px #15283a12; }
    .card-top { display:flex; justify-content:space-between; align-items:flex-start; gap:10px; }
    .region { margin:0; font-size:16px; font-weight:850; letter-spacing:-.03em; }
    .city { margin:2px 0 0; color:var(--muted); font-size:11px; }
    .weather-symbol { font-size:29px; line-height:1; }
    .temperature { margin:22px 0 0; font-size:37px; font-weight:850; line-height:1; letter-spacing:-.055em; }
    .condition { min-height:24px; margin:7px 0 18px; color:#526171; font-size:12px; }
    .weather-facts { display:grid; grid-template-columns:1fr 1fr; gap:9px 13px; padding-top:15px; border-top:1px solid #edf0ec; color:var(--muted); font-size:11px; }
    .weather-facts strong { display:block; margin-top:1px; color:#2d3a47; font-size:12px; font-weight:750; }
    .currency-layout { display:grid; grid-template-columns:minmax(0,1.5fr) minmax(280px,.65fr); gap:24px; }
    .currency-panel,.converter { border:1px solid var(--line); border-radius:20px; background:#fff; box-shadow:0 9px 25px #15283a08; }
    .currency-panel { overflow:auto; }
    .rate-table { width:100%; border-collapse:collapse; text-align:left; }
    .rate-table th { padding:14px 20px; background:#edf0eb; color:#71808c; font-size:10px; letter-spacing:.08em; }
    .rate-table td { padding:16px 20px; border-top:1px solid #edf0ec; font-size:13px; }
    .rate-table tbody tr:hover { background:#fafbf8; }
    .currency-name { display:flex; align-items:center; gap:12px; font-weight:800; }
    .currency-badge { display:grid; place-items:center; width:34px; height:34px; flex:none; border-radius:10px; background:#e9edf7; color:#405b80; font-size:10px; font-weight:900; }
    .currency-sub { display:block; color:var(--muted); font-size:11px; font-weight:400; }
    .rate-value { font-variant-numeric:tabular-nums; font-weight:800; white-space:nowrap; }
    .rate-value span { color:var(--muted); font-weight:500; font-size:10px; }
    .converter { align-self:start; padding:24px; color:#fff; background:radial-gradient(circle at 100% 0,#35536f 0,transparent 50%),var(--navy); border-color:var(--navy); }
    .converter h3 { margin:0; font-size:18px; letter-spacing:-.03em; }
    .converter>p { margin:7px 0 23px; color:#bdcad5; font-size:12px; }
    .field { display:grid; gap:7px; margin-top:14px; }
    .field label { color:#cbd6df; font-size:11px; font-weight:700; }
    .field input,.field select { width:100%; height:46px; padding:0 13px; border:1px solid #ffffff2e; border-radius:12px; color:#fff; background:#ffffff0d; }
    .field option { color:var(--ink); background:#fff; }
    .conversion-result { margin:20px 0 0; padding-top:19px; border-top:1px solid #ffffff26; }
    .conversion-result span { display:block; color:#b7c5cf; font-size:11px; }
    .conversion-result strong { display:block; margin-top:4px; color:var(--lime); font-size:25px; letter-spacing:-.03em; }
    .source-note { margin:17px 0 0; color:#75818d; font-size:10px; }
    .source-note a { text-underline-offset:3px; }
    .skeleton { min-height:232px; border-radius:18px; background:linear-gradient(100deg,#e8ebe6 20%,#f2f4ef 42%,#e8ebe6 64%); background-size:220% 100%; animation:shimmer 1.4s infinite linear; }
    .empty-state { grid-column:1/-1; padding:48px 20px; text-align:center; border:1px dashed #bdc7c1; border-radius:18px; color:var(--muted); }
    .api-card { display:grid; grid-template-columns:minmax(0,1fr) auto; gap:18px; align-items:center; margin-top:55px; padding:26px 28px; color:#fff; background:var(--navy-2); border-radius:20px; }
    .api-card h2 { font-size:20px; }
    .api-card p { margin:7px 0 0; color:#bdcad5; font-size:12px; }
    .api-card a { padding:10px 16px; border:1px solid #ffffff30; border-radius:11px; color:#fff; text-decoration:none; font:700 12px/1.5 Consolas,monospace; }
    footer { display:flex; justify-content:space-between; gap:20px; max-width:1180px; margin:auto; padding:24px; border-top:1px solid var(--line); color:#7b858e; font-size:10px; }
    @keyframes shimmer { to { background-position-x:-220%; } }
    @media(prefers-reduced-motion:reduce) { html { scroll-behavior:auto; } *,*::before,*::after { animation:none!important; transition:none!important; } }
    @media(max-width:980px) {
      .hero-inner { grid-template-columns:1fr; gap:34px; padding-top:52px; }
      .hero-card { max-width:580px; }
      .weather-grid { grid-template-columns:repeat(3,minmax(0,1fr)); }
      .currency-layout { grid-template-columns:1fr; }
      .converter { width:100%; max-width:560px; }
    }
    @media(max-width:720px) {
      .site-header { height:68px; padding:0 18px; }
      .header-actions nav { display:none; }
      .refresh .refresh-label { display:none; }
      .refresh { width:41px; height:41px; justify-content:center; padding:0; }
      .hero-inner { padding:44px 20px 52px; }
      h1 { font-size:42px; }
      .hero-copy { font-size:14px; }
      main { padding:0 18px 44px; }
      .section { padding-top:45px; }
      .section-header { align-items:flex-start; flex-direction:column; gap:8px; }
      h2 { font-size:27px; }
      .updated-at { white-space:normal; }
      .weather-grid { grid-template-columns:repeat(2,minmax(0,1fr)); }
      .weather-toolbar { align-items:flex-start; flex-direction:column; gap:8px; }
      .search { width:100%; }
      .rate-table th,.rate-table td { padding:14px 13px; }
      .api-card { grid-template-columns:1fr; padding:23px; }
      footer { flex-direction:column; padding:20px; }
    }
    @media(max-width:440px) {
      .brand { font-size:14px; }
      .brand-mark { width:32px; height:32px; }
      h1 { font-size:37px; }
      .hero-card { padding:22px; }
      .featured-temp { font-size:54px; }
      .weather-grid { grid-template-columns:1fr; }
      .weather-card { display:grid; grid-template-columns:1fr auto; column-gap:18px; }
      .card-top { grid-column:1; }
      .temperature { grid-column:2; grid-row:1; margin:0; font-size:35px; }
      .condition { grid-column:2; grid-row:2; margin:4px 0 10px; text-align:right; }
      .weather-facts { grid-column:1/-1; }
      .rate-table th:nth-child(2),.rate-table td:nth-child(2) { display:none; }
    }
  </style>
</head>
<body>
  <a class="skip" href="#main">본문으로 건너뛰기</a>
  <header class="site-header">
    <a class="brand" href="/" aria-label="오늘의 대한민국 홈"><span class="brand-mark" aria-hidden="true">☀</span><span>오늘의 대한민국</span></a>
    <div class="header-actions">
      <nav aria-label="주요 메뉴"><a href="#weather">전국 날씨</a><a href="#exchange">주요 환율</a><a href="/api/dashboard">API</a></nav>
      <button class="refresh" id="refresh" type="button" aria-label="최신 정보 불러오기"><span aria-hidden="true">↻</span><span class="refresh-label">새로고침</span></button>
    </div>
  </header>

  <section class="hero" aria-labelledby="page-title">
    <div class="hero-inner">
      <div>
        <p class="eyebrow" id="today-label">TODAY'S KOREA</p>
        <h1 id="page-title">오늘의 날씨와 환율,<br>한번에 확인하세요.</h1>
        <p class="hero-copy">전국 17개 시·도의 현재 날씨와 주요 통화를 <strong>원화 기준</strong>으로 정리했습니다.</p>
      </div>
      <div class="hero-card" aria-live="polite">
        <p class="hero-card-label">서울 현재 브리핑</p>
        <div class="featured-weather">
          <div><span class="featured-temp" id="featured-temp">--°</span><span class="featured-place" id="featured-place">날씨 정보를 불러오는 중</span></div>
          <span class="featured-icon" id="featured-icon" aria-hidden="true">◌</span>
        </div>
        <div class="hero-meta"><span id="featured-range">최고 --° / 최저 --°</span><span id="featured-rain">강수확률 --%</span><span id="featured-humidity">습도 --%</span></div>
      </div>
    </div>
  </section>

  <main id="main">
    <div class="status-banner" id="status-banner" hidden role="status"></div>
    <section class="section" id="weather" aria-labelledby="weather-title">
      <div class="section-header">
        <div><p class="section-kicker">NATIONWIDE WEATHER</p><h2 id="weather-title">전국 지역 날씨</h2><p class="section-note">광역 행정구역 대표 도시의 현재 기온과 오늘의 예보입니다.</p></div>
        <p class="updated-at" id="weather-updated">데이터 수집 중…</p>
      </div>
      <div class="weather-toolbar">
        <label class="search"><span aria-hidden="true">⌕</span><input id="region-search" type="search" placeholder="지역이나 도시 검색" aria-label="지역 또는 도시 검색"></label>
        <span class="region-count" id="region-count">17개 지역</span>
      </div>
      <div class="weather-grid" id="weather-grid" aria-live="polite" aria-busy="true"><div class="skeleton"></div><div class="skeleton"></div><div class="skeleton"></div><div class="skeleton"></div></div>
      <p class="source-note">날씨 출처: <a href="https://open-meteo.com/">Open-Meteo</a> · 기준 시간대: Asia/Seoul</p>
    </section>

    <section class="section" id="exchange" aria-labelledby="exchange-title">
      <div class="section-header">
        <div><p class="section-kicker">EXCHANGE RATES</p><h2 id="exchange-title">주요 통화 환율</h2><p class="section-note">외화 1단위(일본 엔은 100엔)를 대한민국 원으로 환산한 참고 시세입니다.</p></div>
        <p class="updated-at" id="exchange-updated">데이터 수집 중…</p>
      </div>
      <div class="currency-layout">
        <div class="currency-panel">
          <table class="rate-table">
            <thead><tr><th>통화</th><th>표시 기준</th><th>원화 환율</th></tr></thead>
            <tbody id="rate-list"><tr><td colspan="3">환율 정보를 불러오는 중…</td></tr></tbody>
          </table>
        </div>
        <aside class="converter" aria-labelledby="converter-title">
          <h3 id="converter-title">환율 계산기</h3><p>원화 금액을 선택한 통화로 계산합니다.</p>
          <div class="field"><label for="krw-amount">원화 금액</label><input id="krw-amount" type="number" min="0" step="1000" value="10000" inputmode="decimal"></div>
          <div class="field"><label for="currency-select">바꿀 통화</label><select id="currency-select" aria-label="환산할 통화"><option value="USD">USD · 미국 달러</option></select></div>
          <p class="conversion-result"><span>환산 결과</span><strong id="conversion-value">-- USD</strong></p>
        </aside>
      </div>
      <p class="source-note">환율 출처: <a href="https://frankfurter.dev/">Frankfurter</a> · 최신 영업일 기준이며 실거래 시 고시 환율과 다를 수 있습니다.</p>
    </section>

    <section class="api-card" aria-labelledby="api-title">
      <div><h2 id="api-title">개발자를 위한 통합 API</h2><p>날씨와 환율 데이터를 JSON으로 제공합니다. API는 별도 키 없이 서버를 통해 사용할 수 있습니다.</p></div>
      <a href="/api/dashboard">GET /api/dashboard ↗</a>
    </section>
  </main>

  <footer><span>대한민국 표준시(KST) 기준으로 표시됩니다.</span><span>Open data dashboard · ASP.NET 10</span></footer>

  <script>
    const state = { weather: [], rates: [] };
    const weatherGrid = document.querySelector("#weather-grid");
    const rateList = document.querySelector("#rate-list");
    const refreshButton = document.querySelector("#refresh");
    const statusBanner = document.querySelector("#status-banner");
    const formatter = new Intl.NumberFormat("ko-KR", { maximumFractionDigits: 2 });
    const krwFormatter = new Intl.NumberFormat("ko-KR", { maximumFractionDigits: 0 });

    function escapeHtml(value) {
      return String(value).replace(/[&<>"']/g, character => ({"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#39;"})[character]);
    }

    function renderWeather(items) {
      const query = document.querySelector("#region-search").value.trim().toLocaleLowerCase("ko-KR");
      const filtered = items.filter(item => `${item.region} ${item.city}`.toLocaleLowerCase("ko-KR").includes(query));
      document.querySelector("#region-count").textContent = query ? `${filtered.length}개 지역 검색됨` : `${items.length}개 지역`;
      weatherGrid.innerHTML = filtered.length ? filtered.map(item => `
        <article class="weather-card">
          <div class="card-top"><div><h3 class="region">${escapeHtml(item.region)}</h3><p class="city">${escapeHtml(item.city)}</p></div><span class="weather-symbol" aria-hidden="true">${escapeHtml(item.icon)}</span></div>
          <p class="temperature">${Math.round(item.temperature)}°</p><p class="condition">${escapeHtml(item.condition)}</p>
          <div class="weather-facts">
            <span>최고 / 최저<strong>${Math.round(item.high)}° / ${Math.round(item.low)}°</strong></span>
            <span>강수 확률<strong>${Math.round(item.precipitationProbability)}%</strong></span>
            <span>습도<strong>${Math.round(item.humidity)}%</strong></span>
            <span>풍속<strong>${formatter.format(item.windSpeed)} km/h</strong></span>
          </div>
        </article>`).join("") : '<p class="empty-state">검색 결과가 없습니다.</p>';
    }

    function renderHero(items) {
      const seoul = items.find(item => item.code === "SEOUL") ?? items[0];
      if (!seoul) {
        document.querySelector("#featured-place").textContent = "날씨 정보를 사용할 수 없음";
        return;
      }
      document.querySelector("#featured-temp").textContent = `${Math.round(seoul.temperature)}°`;
      document.querySelector("#featured-place").textContent = `${seoul.city} · ${seoul.condition}`;
      document.querySelector("#featured-icon").textContent = seoul.icon;
      document.querySelector("#featured-range").textContent = `최고 ${Math.round(seoul.high)}° / 최저 ${Math.round(seoul.low)}°`;
      document.querySelector("#featured-rain").textContent = `강수확률 ${Math.round(seoul.precipitationProbability)}%`;
      document.querySelector("#featured-humidity").textContent = `습도 ${Math.round(seoul.humidity)}%`;
    }

    function renderRates(rates) {
      rateList.innerHTML = rates.length ? rates.map(item => {
        const unit = item.code === "JPY" ? 100 : 1;
        return `<tr>
          <td><span class="currency-name"><span class="currency-badge">${escapeHtml(item.code)}</span><span>${escapeHtml(item.name)}<span class="currency-sub">${escapeHtml(item.country)}</span></span></span></td>
          <td>${unit.toLocaleString("ko-KR")} ${escapeHtml(item.code)}</td>
          <td class="rate-value">${krwFormatter.format(item.krwPerUnit * unit)} <span>KRW</span></td>
        </tr>`;
      }).join("") : '<tr><td colspan="3">현재 환율 정보를 사용할 수 없습니다.</td></tr>';
      const select = document.querySelector("#currency-select");
      select.innerHTML = rates.map(item => `<option value="${escapeHtml(item.code)}">${escapeHtml(item.code)} · ${escapeHtml(item.name)}</option>`).join("");
      convertCurrency();
    }

    function convertCurrency() {
      const amount = Math.max(0, Number(document.querySelector("#krw-amount").value) || 0);
      const code = document.querySelector("#currency-select").value;
      const rate = state.rates.find(item => item.code === code);
      document.querySelector("#conversion-value").textContent = rate ? `${formatter.format(amount / rate.krwPerUnit)} ${rate.code}` : "--";
    }

    function formatDateTime(value) {
      if (!value) return "업데이트 정보 없음";
      return new Intl.DateTimeFormat("ko-KR", { month:"long", day:"numeric", hour:"numeric", minute:"2-digit", hour12:false }).format(new Date(value));
    }

    async function loadDashboard() {
      refreshButton.disabled = true;
      statusBanner.hidden = true;
      weatherGrid.setAttribute("aria-busy", "true");
      try {
        const response = await fetch("/api/dashboard", { headers: { Accept:"application/json" }, cache:"no-store" });
        const data = await response.json();
        state.weather = data.weather?.locations ?? [];
        state.rates = data.exchangeRates?.rates ?? [];
        renderWeather(state.weather);
        renderHero(state.weather);
        renderRates(state.rates);
        if (data.localDate) document.querySelector("#today-label").textContent = `${data.localDate} · TODAY'S KOREA`;
        document.querySelector("#weather-updated").textContent = data.weather?.isAvailable ? `${formatDateTime(data.weather.updatedAt)} 업데이트` : "날씨 서비스 연결 실패";
        document.querySelector("#exchange-updated").textContent = data.exchangeRates?.isAvailable ? `${data.exchangeRates.date} 기준` : "환율 서비스 연결 실패";
        const errors = (data.sources ?? []).filter(source => !source.isAvailable);
        if (errors.length) {
          statusBanner.textContent = errors.map(source => source.message).join(" ");
          statusBanner.hidden = false;
        }
        if (!response.ok && !errors.length) throw new Error("데이터를 불러오지 못했습니다.");
      } catch (error) {
        statusBanner.textContent = "최신 데이터를 불러오지 못했습니다. 잠시 후 다시 시도해 주세요.";
        statusBanner.hidden = false;
        if (!state.weather.length) weatherGrid.innerHTML = '<p class="empty-state">날씨 정보를 사용할 수 없습니다.</p>';
        if (!state.rates.length) rateList.innerHTML = '<tr><td colspan="3">환율 정보를 사용할 수 없습니다.</td></tr>';
      } finally {
        refreshButton.disabled = false;
        weatherGrid.setAttribute("aria-busy", "false");
      }
    }

    document.querySelector("#region-search").addEventListener("input", () => renderWeather(state.weather));
    document.querySelector("#krw-amount").addEventListener("input", convertCurrency);
    document.querySelector("#currency-select").addEventListener("change", convertCurrency);
    refreshButton.addEventListener("click", loadDashboard);
    loadDashboard();
  </script>
</body>
</html>
""";
}
