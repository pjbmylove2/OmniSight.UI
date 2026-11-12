# Changelog

## 0.3.0
- `SquareButton` 추가: VS2022 다크·라이트 테마 색을 쓰는 각진 버튼. 변형(기본·기본 동작·위험), 5가지 상태, 토글, `IButtonControl`(`DialogResult`, `AcceptButton`)
- `SquareButtonPalette`, `UiTheme`, `SquareButtonVariant`, `SquareButtonState` 추가
- 문서 `docs/SquareButton.md`, 데모 `--check`/`--capture`

## 0.2.0
- `DeviceStatusStrip`, `DeviceStatusChip` 추가: 장치 연결 상태(미사용·연결 중·정상·경고·오류·끊김)를 램프 + 이름 칩으로 표시, 클릭 시 `DeviceStatusPopup` 상세 팝업
- `DeviceStatusRegistry`: 스레드 안전 상태 보고 창구, 응답 지연 자동 강등(경고·끊김)과 흔들림 방지 승격, 장치당 50건 변경 이력
- `DeviceStatusPalette`/`DeviceStatusTheme`: 다크·라이트 테마
- 디자이너에서 샘플 칩 6개를 보여 줘 모양과 크기를 바로 확인
- 문서 `docs/DeviceStatusStrip.md`, 데모 `--check`/`--capture`

## 0.1.0
- 라이브러리 골격, `Theme`, 샘플 `ThemedButton`
