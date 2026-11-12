# Changelog

## 0.5.0
- 호환성 변경: 타깃을 `net472` 단일로 변경(`net9.0-windows` 제거), Debug/Release x64 전용. 출력 경로는 `bin/x64/<구성>/net472`
- 호환성 변경: 샘플 컨트롤 `ThemedButton` 삭제(`SquareButton`을 사용)
- 데모를 `net472`로 변경, `System.Design` 참조 추가

## 0.4.1
- 수정: `SquareButton`을 디자이너에 올리면 `Font`(Malgun Gothic 9), `BackColor`, `Cursor`, `AccessibleRole`이 소비 프로젝트의 Designer.cs에 항상 박히던 문제. 생성자의 지정과 `BackColor` 재정의를 없애 손대지 않은 버튼은 위치·크기·텍스트만 기록되고, 글꼴은 부모를 따라간다
- 접근성: `AccessibleRole`을 건드리지 않고 `CreateAccessibilityInstance`로 푸시 버튼 역할을 유지
- 데모 `--check`에 디자이너 기본값 검사 추가

## 0.4.0
- `DeviceStatusStrip`의 색을 VS2022 다크·라이트 테마 값으로 교체(Theme.Dark/Light.pkgdef에서 읽은 값)
- 호환성 변경: `DeviceStatusTheme`을 없애고 `UiTheme`(`SquareButton`과 공용)을 쓴다. `DeviceStatusPalette`의 `AccentBack`은 `Hover`·`Selected`로 나뉘고 `PopupList`·`PopupBorder` 등이 추가됨
- 상세 팝업의 재연결 버튼을 `SquareButton`으로 교체(내부용 `PopupButton` 삭제)
- 칩에 마우스를 올리면 VS의 ButtonHover 색으로 바뀜

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
