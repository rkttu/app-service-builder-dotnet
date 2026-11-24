# 오늘의 대한민국

대한민국 17개 광역자치단체의 오늘 날씨와 주요 8개 통화의 원화 환율을 제공하는
반응형 ASP.NET 웹 애플리케이션입니다. 브라우저 화면과 JSON API를 같은 서버에서
제공하며, 날씨와 환율 정보는 각각 Open-Meteo와 Frankfurter에서 가져옵니다.

## 주요 기능

- 전국 17개 시·도의 대표 도시별 현재 기온, 최고/최저 기온, 강수 확률, 습도, 풍속
- USD, EUR, JPY, CNY, GBP, AUD, CAD, CHF의 원화 환율
- 지역 검색, 원화-외화 환율 계산기, 새로고침 및 부분 장애 안내
- 모바일부터 데스크톱까지 반응하는 접근 가능한 단일 페이지
- .NET 서버 캐시: 날씨 10분, 환율 30분
- 외부 API 키나 NuGet 패키지 없이 실행

## 엔드포인트

| 메서드 | 경로 | 응답 |
| --- | --- | --- |
| `GET` | `/` | 반응형 날씨·환율 대시보드 |
| `GET` | `/api/dashboard` | 날씨와 환율 통합 JSON |
| `GET` | `/api/weather` | 전국 날씨 JSON |
| `GET` | `/api/exchange-rates` | 주요 통화 환율 JSON |
| `GET` | `/health` | `OK`와 줄바꿈 |

통합 API는 두 데이터 소스가 모두 성공하면 `200`, 한쪽만 성공하면 `206`, 둘 다
사용할 수 없으면 `503`을 반환합니다. 실패한 응답에도 `isAvailable`, `sources`,
`message` 필드가 포함되어 원인을 확인할 수 있습니다.

## 실행 방법

.NET 10 SDK가 필요합니다. 저장소 루트에서 다음을 실행합니다.

```sh
dotnet publish -c Release -o bin/sample-publish --configfile NuGet.Config
dotnet bin/sample-publish/BuilderDotnetSample.dll
```

서버는 `http://localhost:8080`에서 시작됩니다.

```sh
curl http://localhost:8080/
curl http://localhost:8080/health
curl http://localhost:8080/api/dashboard
curl http://localhost:8080/api/weather
curl http://localhost:8080/api/exchange-rates
```

`Program.cs`가 명시적으로 포트 8080에 바인딩합니다. 다른 포트를 잠시 사용할
때는 환경의 Kestrel 엔드포인트 설정을 사용할 수 있습니다. 저장소의 스모크
테스트는 이를 사용합니다.

```sh
bash scripts/smoke-test.sh
SMOKE_TEST_PORT=8097 bash scripts/smoke-test.sh
```

## 데이터 출처와 표시 기준

- 날씨: [Open-Meteo](https://open-meteo.com/) 예보 API, `Asia/Seoul`
- 환율: [Frankfurter v1](https://frankfurter.dev/v1/), 외화 1단위당 원화
- 엔화는 화면에서 `100 JPY` 기준으로 표시하며 API의 `krwPerUnit`은 1엔 기준입니다.
- 환율은 최신 영업일 데이터이므로 주말·공휴일에는 실시간 거래 시세가 아닐 수 있습니다.

외부 서비스 연결 실패는 별도 가짜 데이터로 대체하지 않습니다. 서버는 오류를
기록하고 API와 화면에 사용 불가 상태를 명시합니다. 환율은 금융 거래 기준 시세가
아니므로 실제 거래에는 금융기관 고시 환율을 확인해야 합니다.

## 배포

`builder.yaml`은 .NET 10 웹 컴포넌트, 포트 8080, `/health` 헬스 체크를 설정합니다.
App Service Builder로 배포할 때 저장소 루트를 소스 루트로 선택하고 명시적으로
배포합니다.

## 라이선스

[MIT](LICENSE.md)
