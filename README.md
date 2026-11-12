# OmniSight.UI

OmniSight 계열 프로젝트가 공용으로 사용하는 WinForms 커스텀 UI 컨트롤 라이브러리 (출력: `OmniSight.UI.dll`).

## 구조
- `src/OmniSight.UI` : 라이브러리 (net472 전용)
- `samples/OmniSight.UI.Demo` : 컨트롤 확인용 데모 앱

## 컨트롤
- `ThemedButton` : Theme 색을 쓰는 플랫 버튼(파이프라인 검증용 샘플)
- `SquareButton` : VS2022 다크·라이트 테마 색을 쓰는 각진 버튼(기본·기본 동작·위험, 토글, `DialogResult`). 사용법·색 표·크기 조절·결과 화면은 [docs/SquareButton.md](docs/SquareButton.md)
- `DeviceStatusStrip` : 장치 연결 상태를 램프 + 이름 칩으로 보여 주는 헤더용 표시줄. 6단계 상태, 다크·라이트 테마, 상세 팝업. 사용법·크기 조절·결과 화면은 [docs/DeviceStatusStrip.md](docs/DeviceStatusStrip.md)

## 테마 색
모든 사용자 정의 컨트롤의 다크·라이트 색은 Visual Studio 2022 테마 색을 따른다. `SquareButton`과 `DeviceStatusStrip` 모두 VS2022 값을 쓴다. 두 컨트롤은 같은 `UiTheme`(Dark/Light)를 공유한다.

## 빌드
```
dotnet build OmniSight.UI.sln -c Release
```
DLL 위치: `src/OmniSight.UI/bin/Release/<TFM>/OmniSight.UI.dll`

## 프로젝트에서 참조
버전별 폴더에 DLL을 배포하고 HintPath로 참조한다.
```xml
<ItemGroup>
  <Reference Include="OmniSight.UI">
    <HintPath>경로\OmniSight.UI\0.1.0\net472\OmniSight.UI.dll</HintPath>
  </Reference>
</ItemGroup>
```

## 버전 규칙
- SemVer, 버전은 `OmniSight.UI.csproj`의 `<Version>` 하나로 관리
- 릴리스 시 Git 태그 `vX.Y.Z` 생성, 호환성 깨지면 메이저 증가
- 컨트롤 수정은 이 저장소에서만 한다 (소비 프로젝트에 UI 소스 복사 금지)
