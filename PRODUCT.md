# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users and purpose

대한민국 사용자가 17개 광역 지역의 오늘 날씨와 주요 통화의 원화 환율을 빠르게
확인하는 공개 웹 대시보드입니다. 개발자는 같은 정보를 JSON API로 사용할 수
있습니다.

## Capabilities and constraints

앱은 API 키 없는 Open-Meteo와 Frankfurter 데이터를 사용합니다. 외부 서비스가
사용할 수 없을 때 임의의 값을 만들지 않고 `isAvailable`과 사용자용 오류 메시지를
반환합니다. 통합 API는 정상 `200`, 부분 성공 `206`, 전체 실패 `503` 계약을 사용합니다.

날씨는 10분, 환율은 30분 동안 서버에서 캐시합니다. 환율은 최신 영업일 기준의
참고값이며 실거래 시세가 아닙니다. 앱은 .NET 10, 포트 8080, `/health` 헬스
체크 계약을 유지합니다.

## Brand commitments

짙은 네이비, 따뜻한 종이색, 라임 강조색을 기반으로 정보 중심의 깨끗한
대시보드를 제공합니다. 데스크톱은 넓은 그리드, 모바일은 한 열 카드와 가로 스크롤
없는 표 구조를 사용합니다. 모든 조작은 키보드로 가능하고 로딩·오류·부분 성공
상태를 색상만으로 전달하지 않습니다.

## Evidence on hand

`Program.cs`는 API 경로와 HTTP 상태 계약을 정의하고, `DashboardService.cs`,
`WeatherProvider.cs`, `ExchangeRateProvider.cs`는 캐시와 외부 데이터 변환을
담당합니다. `DashboardPage.cs`는 반응형 화면과 클라이언트 상호작용을 정의합니다.
