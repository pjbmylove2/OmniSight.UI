# OmniSight.UI

OmniSight 계열 프로젝트가 공용으로 사용하는 WinForms 커스텀 UI 컨트롤 라이브러리 (출력: `OmniSight.UI.dll`).

## 구조
- `src/OmniSight.UI` : 라이브러리 (net472 전용)
- `samples/OmniSight.UI.Demo` : 컨트롤 확인용 데모 앱

## 컨트롤
- `SquareButton` : VS2022 다크·라이트 테마 색을 쓰는 각진 버튼(기본·기본 동작·위험, 토글, `DialogResult`). 사용법·색 표·크기 조절·결과 화면은 [docs/SquareButton.md](docs/SquareButton.md)
- `ImageViewer` : 비전용 이미지 뷰어(외부 의존 없음, 원시 버퍼 입력, 스레드 안전, 확대·오버레이·검사 모드). 네임스페이스 `OmniSight.UI.Controls.Vision`. 사용법·구조·성능은 [docs/ImageViewer/Usage.ko.md](docs/ImageViewer/Usage.ko.md)
- `DeviceStatusStrip` : 장치 연결 상태를 램프 + 이름 칩으로 보여 주는 헤더용 표시줄. 6단계 상태, 다크·라이트 테마, 상세 팝업. 사용법·크기 조절·결과 화면은 [docs/DeviceStatusStrip.md](docs/DeviceStatusStrip.md)
- `CardPanel` : 둥근 테두리와 왼쪽·가운데·오른쪽 헤더를 제공하는 컨테이너. 카드에 컨트롤을 배치하고 테마를 적용하는 방법은 [docs/CardPanel.md](docs/CardPanel.md)
- `ColorLabel` : 상태별 배경·글자색과 사용자 지정 배경의 자동 대비를 제공하는 둥근 라벨. 속성, 예제, 표 셀에서 그리는 방법은 [docs/ColorLabel.md](docs/ColorLabel.md)

## 테마 색
Visual Studio 2022 스타일의 다크·라이트 테마를 지원한다. `SquareButton`, `DeviceStatusStrip`, `CardPanel`, `ColorLabel`은 `OmniSight.UI.Controls.UiTheme`(`Dark`, `Light`)를 공유한다.

## 빌드
```
dotnet build OmniSight.UI.sln -c Release
```
DLL 위치: `src/OmniSight.UI/bin/x64/Release/net472/OmniSight.UI.dll`

## 데모 실행

`samples/OmniSight.UI.Demo/bin/x64/Release/net472/OmniSight.UI.Demo.exe`를 실행한다.
메인 화면에서 `CardPanel`과 `ColorLabel`을 다크·라이트 테마로 나란히 볼 수 있다. **카드 패널·컬러 라벨 데모**를 누르면 검사 모니터와 컬러 라벨 갤러리를 열 수 있다.

## 프로젝트에서 참조
버전별 폴더에 DLL을 배포하고 HintPath로 참조한다.
```xml
<ItemGroup>
  <Reference Include="OmniSight.UI">
    <HintPath>경로\OmniSight.UI\0.8.2\net472\OmniSight.UI.dll</HintPath>
  </Reference>
</ItemGroup>
```

## 버전 규칙
- SemVer, 버전은 `OmniSight.UI.csproj`의 `<Version>` 하나로 관리
- 릴리스 시 Git 태그 `vX.Y.Z` 생성, 호환성 깨지면 메이저 증가
- 컨트롤 수정은 이 저장소에서만 한다 (소비 프로젝트에 UI 소스 복사 금지)
