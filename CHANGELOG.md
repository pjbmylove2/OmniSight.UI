# Changelog

## 0.8.2
- 장치 상세 팝업의 이력 목록이 상속한 GDI 글꼴 핸들을 폐기 전에 해제
- 장치 상태 표시줄별로 상세 팝업을 재사용하고 표시줄 폐기 시 해제해 반복 표시 중 관리 메모리 누적 방지
- 장치 변경 시 팝업의 제목·이력·테마와 호스트 종료 시 폐기를 확인하는 회귀 검사 추가

## 0.8.1
- 데모 시작 화면에 다크·라이트 `CardPanel`과 OK·NG·수동 모드·대기 `ColorLabel` 비교 예제를 추가
- 카드와 라벨이 시작 화면에서 스크롤 없이 보이는지 검사하는 회귀 테스트 추가

## 0.8.0
- `CardPanel` 추가. 둥근 테두리, 3칸 헤더, 자식 컨트롤 배치와 공용 `UiTheme` 지원
- `ColorLabel` 추가. 상태 배지, 사용자 지정 배경의 자동 대비, `DataGridView`용 `DrawBadge` 지원
- 기존 데모에 검사 화면과 배지 갤러리 추가. `--check`와 `--capture`에 카드·라벨 검증과 캡처 추가
- 설치와 사용 예제는 `docs/CardPanel.md`, `docs/ColorLabel.md`에 정리

## 0.7.0
- `SquareButton.Image`가 버튼 높이의 절반 칸에 비율을 유지하며 맞춰지도록 변경(전에는 원본 크기 그대로 그림). 호환성 변경: 큰 이미지는 줄어들고 작은 이미지는 늘어난다
- `SquareButton.ImageSize` 추가(0이면 자동, 아니면 칸 크기 px)
- 데모 `--check`에 이미지 크기·위치·세로 가운데, 이미지 없을 때 글자 가운데 검사 추가

## 0.6.0
- `ImageViewer` 추가(별도 저장소 VisionImageViewer v0.1.0의 코어를 이식, 네임스페이스 `OmniSight.UI.Controls.Vision`): 외부 의존 없는 GDI 이미지 뷰어. 원시 버퍼 입력, 스레드 안전, 최신 프레임 우선, 확대·스크롤·검사 모드, 오버레이, 픽셀 값 이벤트, 파일 열기·저장
- 문서 `docs/ImageViewer/`(사용법·구조·성능·타 라이브러리 연동). OpenCvSharp 어댑터는 이식하지 않음
- 데모: "이미지 뷰어 데모" 창, `--check`에 뷰어 검증 58항목 추가, `--viewer bench|soak|stress`
- `OmniSight.UI`에 `AllowUnsafeBlocks` 켬, 데모에 `InternalsVisibleTo`

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
